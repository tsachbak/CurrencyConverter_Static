using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverter_Static
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection m_sqlConnection = new SqlConnection();
        private SqlCommand m_sqlCommand = new SqlCommand();
        private SqlDataAdapter m_sqlDataAdapter = new SqlDataAdapter();

        private int m_CurrencyId = 0;
        private double m_FromAmount = 0;
        private double m_ToAmount = 0;
        public MainWindow() 
        {
            InitializeComponent();
            BindCurrency();
            GetData();
        }

        public void ConnectToDB()
        {
            try
            {
                string conString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                m_sqlConnection = new SqlConnection(conString);
                m_sqlConnection.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void BindCurrency()
        {
            ConnectToDB();

            DataTable dt = new DataTable();
            m_sqlCommand = new SqlCommand("select Id, CurrencyName from Currency_Master", m_sqlConnection);
            m_sqlCommand.CommandType = CommandType.Text;

            m_sqlDataAdapter = new SqlDataAdapter(m_sqlCommand);
            m_sqlDataAdapter.Fill(dt);

            DataRow newRow = dt.NewRow();
            newRow["Id"] = 0;
            newRow["CurrencyName"] = "--SELECT--";

            dt.Rows.InsertAt(newRow, 0);

            if (dt != null && dt.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dt.DefaultView;
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }

            m_sqlConnection.Close();

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double val;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();

                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Enter Currency to Convert from", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();

                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please Enter Currency to Convert to", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();

                return;
            }

            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                val = double.Parse(txtCurrency.Text);
                lblCurrency.Content = val.ToString("N3") + " " + cmbToCurrency.Text;
            }
            else
            {
                val = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * 
                      (double.Parse(txtCurrency.Text)) /
                      (double.Parse(cmbToCurrency.SelectedValue.ToString())));

                lblCurrency.Content = val.ToString("N3") + " " + cmbToCurrency.Text;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtAmount.Text))
                {
                    MessageBox.Show("Please enter amount", "Information", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                
                if (string.IsNullOrEmpty(txtCurrencyName.Text))
                {
                    MessageBox.Show("Please enter currency name", "Information",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }

                if(m_CurrencyId > 0)
                {
                    if (MessageBox.Show("Are you sure you want to update?", "information", MessageBoxButton.YesNo,
                                         MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        ConnectToDB();
                        DataTable dt = new DataTable();
                        m_sqlCommand = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, " +
                                                      "CurrencyName = @CurrencyName WHERE Id = @Id", m_sqlConnection);
                        m_sqlCommand.CommandType = CommandType.Text;
                        m_sqlCommand.Parameters.AddWithValue("@Id", m_CurrencyId);
                        m_sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        m_sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                        m_sqlCommand.ExecuteNonQuery();

                        m_sqlConnection.Close();

                        MessageBox.Show("Data updated successfully", "Information",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    if (MessageBox.Show("Are you sure you want to save?", "information", MessageBoxButton.YesNo,
                                         MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        ConnectToDB();
                        DataTable dt = new DataTable();
                        m_sqlCommand = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) " +
                                                      "VALUES(@Amount, @CurrencyName)", m_sqlConnection);
                        m_sqlCommand.CommandType = CommandType.Text;
                        m_sqlCommand.Parameters.AddWithValue("@Id", m_CurrencyId);
                        m_sqlCommand.Parameters.AddWithValue("@Amount", txtAmount.Text);
                        m_sqlCommand.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                        m_sqlCommand.ExecuteNonQuery();

                        m_sqlConnection.Close();

                        MessageBox.Show("Data updated successfully", "Information",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                ClearMaster();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                m_CurrencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetData()
        {
            ConnectToDB();

            DataTable dt = new DataTable();

            m_sqlCommand = new SqlCommand("SELECT * FROM Currency_Master", m_sqlConnection);
            m_sqlCommand.CommandType = CommandType.Text;

            m_sqlDataAdapter = new SqlDataAdapter(m_sqlCommand);
            m_sqlDataAdapter.Fill(dt);

            if (dt != null && dt.Rows.Count > 0)
                dgvCurrency.ItemsSource = dt.DefaultView;
            else
                dgvCurrency.ItemsSource = null;

            m_sqlConnection.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            DataRowView rowSelected = dg.CurrentItem as DataRowView;

            if (rowSelected != null)
            {
                if (dgvCurrency.Items.Count > 0)
                {
                    if (dg.SelectedCells.Count > 0)
                    {
                        m_CurrencyId = Int32.Parse(rowSelected["Id"].ToString());

                        //user press edit
                        if (dg.SelectedCells[0].Column.DisplayIndex == 0)
                        {
                            txtAmount.Text = rowSelected["Amount"].ToString();
                            txtCurrencyName.Text = rowSelected["CurrencyName"].ToString();
                            btnSave.Content = "Update";
                        }

                        //user press delete
                        if (dg.SelectedCells[0].Column.DisplayIndex == 1)
                        {
                            if (MessageBox.Show("Are you sure you want to delete?", "Information",
                                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                ConnectToDB();

                                DataTable dt = new DataTable();
                                m_sqlCommand = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", m_sqlConnection);
                                m_sqlCommand.CommandType = CommandType.Text;
                                m_sqlCommand.Parameters.AddWithValue("@Id", m_CurrencyId);
                                m_sqlCommand.ExecuteNonQuery();

                                m_sqlConnection.Close();

                                MessageBox.Show("Data deleted successfullt", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                ClearMaster();
                            }
                        }
                    }
                }
            }
        }
    }
}

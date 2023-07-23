using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Data.SqlClient;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Configuration;
using DB_Analyzer.DB_Tools;
namespace DB_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            registerProviders();
            fillInputProvidersComboBox();
        }
        readonly string sqlserver_name = "sql server";
        readonly string mysqlserver_name = "mysql";
        readonly string none_name = "none";
        private void registerProviders()
        {
            DbProviderFactories.RegisterFactory(sqlserver_name, SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory(mysqlserver_name, MySqlClientFactory.Instance);
        }
        private void fillInputProvidersComboBox()
        {
            var providersNames = DbProviderFactories.GetProviderInvariantNames();
            InputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected=true });
            foreach (var pn in providersNames)
            {
                InputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
        }
        private async Task FillInputDbComboBox(List<string>? databases)
        {
            if (DatabasesComboBox == null) return;
            DatabasesComboBox.Items.Clear();
            DatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name });
            DatabasesComboBox.SelectedItem = DatabasesComboBox.Items[0];
            if (databases == null) return;
            string selectedDB = inputDBTool.SelectedDatabase;
            foreach (var dbName in databases)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = dbName };
                
                if (!string.IsNullOrEmpty(selectedDB) && (item.Content as string) == selectedDB)
                    item.IsSelected = true;
                else
                    item.IsSelected = false;
                DatabasesComboBox.Items.Add(item);
            }
        }
        DBTool inputDBTool;
        private async void InputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedProvider = (InputProvidersComboBox.SelectedItem as ComboBoxItem).Content as string;
            if (selectedProvider == none_name)
            {
                await FillInputDbComboBox(null);
                return;
            }
            try
            {
                if (selectedProvider == sqlserver_name)
                    inputDBTool = new SQLServerTool(ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
                else if (selectedProvider == mysqlserver_name)
                    inputDBTool = new MySQLTool(ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);

                List<string>? dbs = await inputDBTool.GetDatabasesAsync();
                await FillInputDbComboBox(dbs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void DatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatabasesComboBox == null || DatabasesComboBox.SelectedItem == null) return;
            string selectedDB = (DatabasesComboBox.SelectedItem as ComboBoxItem).Content as string;
            if (selectedDB == none_name) return;
            inputDBTool.SelectedDatabase = selectedDB;
        }
    }
}

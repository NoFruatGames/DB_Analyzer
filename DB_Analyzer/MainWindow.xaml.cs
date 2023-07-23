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
            fillProvidersComboBox(InputProvidersComboBox);
        }
        private void registerProviders()
        {
            DbProviderFactories.RegisterFactory("sql server", SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("mysql", MySqlClientFactory.Instance);
        }
        private void fillProvidersComboBox(ComboBox comboBox)
        {
            var providersNames = DbProviderFactories.GetProviderInvariantNames();
            foreach (var pn in providersNames)
            {
                comboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
        }
        private async Task FillDbComboBox(List<string>? databases)
        {
            if (DatabasesComboBox == null) return;
            DatabasesComboBox.Items.Clear();
            DatabasesComboBox.Items.Add(new ComboBoxItem() { Content = "none"});
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
            if (selectedProvider == "none")
            {
                await FillDbComboBox(null);
                return;
            }
            try
            {
                if (selectedProvider == "sql server")
                    inputDBTool = new SQLServerTool(ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
                else if (selectedProvider == "mysql")
                    inputDBTool = new MySQLTool(ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);

                List<string>? dbs = await inputDBTool.GetDatabasesAsync();
                await FillDbComboBox(dbs);
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
            if (selectedDB == "none") return;
            inputDBTool.SelectedDatabase = selectedDB;
        }
    }
}

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
            fillOutputProvidersComboBox();
        }
        readonly string sqlserver_name = "sql server";
        readonly string mysqlserver_name = "mysql";
        readonly string none_name = "none";
        readonly string textfile_name = "text file";
        private void registerProviders()
        {
            var connstr = ConfigurationManager.ConnectionStrings[sqlserver_name];
            if (connstr != null && !string.IsNullOrEmpty(connstr.ConnectionString))
                DbProviderFactories.RegisterFactory(sqlserver_name, SqlClientFactory.Instance);
            connstr = ConfigurationManager.ConnectionStrings[mysqlserver_name];
            if (connstr != null && !string.IsNullOrEmpty(connstr.ConnectionString))
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
        private void fillOutputProvidersComboBox()
        {
            var providersNames = DbProviderFactories.GetProviderInvariantNames();
            OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            foreach (var pn in providersNames)
            {
                OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
            OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = textfile_name});
        }
        private async Task FillDbComboBox(List<string>? databases, ComboBox comboBox, DBTool tool)
        {
            if (OutputDatabasesComboBox == null) return;
            comboBox.Items.Clear();
            comboBox.Items.Add(new ComboBoxItem() { Content = none_name });
            comboBox.SelectedItem = comboBox.Items[0];
            if (databases == null) return;
            string selectedDB = tool.SelectedDatabase;
            foreach (var dbName in databases)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = dbName };

                if (!string.IsNullOrEmpty(selectedDB) && (item.Content as string) == selectedDB)
                    item.IsSelected = true;
                else
                    item.IsSelected = false;
                comboBox.Items.Add(item);
            }
        }
        DBTool inputDBTool;
        DBTool outputDBTool;
        private async void InputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedProvider = (InputProvidersComboBox.SelectedItem as ComboBoxItem).Content as string;
            InputDatabasesComboBox.Visibility = Visibility.Visible;
            if (selectedProvider == none_name)
            {
                InputDatabasesComboBox.Visibility = Visibility.Hidden;
                return;
            }
            try
            {
                if (selectedProvider == sqlserver_name)
                    inputDBTool = new SQLServerTool(ConfigurationManager.ConnectionStrings[sqlserver_name].ConnectionString);
                else if (selectedProvider == mysqlserver_name)
                    inputDBTool = new MySQLTool(ConfigurationManager.ConnectionStrings[mysqlserver_name].ConnectionString);

                List<string>? dbs = await inputDBTool.GetDatabasesAsync();
                await FillDbComboBox(dbs, InputDatabasesComboBox, inputDBTool);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void inputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InputDatabasesComboBox == null || InputDatabasesComboBox.SelectedItem == null) return;
            string selectedDB = (InputDatabasesComboBox.SelectedItem as ComboBoxItem).Content as string;
            if (selectedDB == none_name) return;
            inputDBTool.SelectedDatabase = selectedDB;
        }

        private async void OutputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedProvider = (OutputProvidersComboBox.SelectedItem as ComboBoxItem).Content as string;
            OutputDatabasesComboBox.Visibility = Visibility.Visible;
            if(selectedProvider == none_name)
            {
                OutputDatabasesComboBox.Visibility = Visibility.Hidden;
                return;
            }
            else if(selectedProvider == textfile_name)
            {
                OutputDatabasesComboBox.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                try
                {
                    if (selectedProvider == sqlserver_name)
                        outputDBTool = new SQLServerTool(ConfigurationManager.ConnectionStrings[sqlserver_name].ConnectionString);
                    else if (selectedProvider == mysqlserver_name)
                        outputDBTool = new MySQLTool(ConfigurationManager.ConnectionStrings[mysqlserver_name].ConnectionString);

                    List<string>? dbs = await outputDBTool.GetDatabasesAsync();
                    await FillDbComboBox(dbs, OutputDatabasesComboBox, outputDBTool);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void OutputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputDatabasesComboBox == null || OutputDatabasesComboBox.SelectedItem == null) return;
            string selectedDB = (OutputDatabasesComboBox.SelectedItem as ComboBoxItem).Content as string;
            if (selectedDB == none_name) return;
            outputDBTool.SelectedDatabase = selectedDB;
        }
    }
}

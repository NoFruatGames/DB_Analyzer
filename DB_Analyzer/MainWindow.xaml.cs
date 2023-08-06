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
        readonly string new_database_name = "new database";
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
        Analyzer analyzer = new Analyzer();
        List<string> outputDbs;
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
                InputDatabasesComboBox.Items.Clear();
                InputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
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
            OutputDatabasesComboBox.Visibility = Visibility.Hidden;
            OutputTextBox.Visibility = Visibility.Hidden;
            if(selectedProvider == none_name)
            {
                OutputDatabasesComboBox.Visibility = Visibility.Hidden;
                OutputTextBox.Visibility = Visibility.Hidden;
                return;
            }
            else if(selectedProvider == textfile_name)
            {
                OutputDatabasesComboBox.Visibility = Visibility.Hidden;
                OutputTextBox.Visibility = Visibility.Visible;
                outputLabel.Content = "file path";
                return;
            }
            else
            {
                OutputDatabasesComboBox.Visibility = Visibility.Visible;
                try
                {
                    if (selectedProvider == sqlserver_name)
                        outputDBTool = new SQLServerTool(ConfigurationManager.ConnectionStrings[sqlserver_name].ConnectionString);
                    else if (selectedProvider == mysqlserver_name)
                        outputDBTool = new MySQLTool(ConfigurationManager.ConnectionStrings[mysqlserver_name].ConnectionString);

                    outputDbs = await outputDBTool.GetDatabasesAsync();
                    OutputDatabasesComboBox.Items.Clear();
                    OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name});
                    OutputDatabasesComboBox.SelectedItem = OutputDatabasesComboBox.Items[0];
                    string initselectedDB = outputDBTool.SelectedDatabase;
                    foreach (var el in outputDbs)
                    {
                        outputDBTool.SelectedDatabase = el;
                        List<string> tables = await outputDBTool.GetTablesAsync();
                        
                        if (tables.Count == 0 || (tables.Count == 3 && tables.All(table => new[] { "dbs", "common_info", "tables_info" }.Contains(table))))
                        {
                            ComboBoxItem item = new ComboBoxItem() { Content = el };
                            if (el == initselectedDB)
                                item.IsSelected = true;
                            else
                                item.IsSelected = false;
                            OutputDatabasesComboBox.Items.Add(item);
                        }
                    }
                    OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = new_database_name });
                }
                catch (Exception ex)
                {
                    OutputDatabasesComboBox.Items.Clear();
                    OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private void OutputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OutputTextBox.Visibility = Visibility.Hidden;
            OutputTextBox.Text = "";
            if (OutputDatabasesComboBox == null || OutputDatabasesComboBox.SelectedItem == null) return;
            string selectedDB = (OutputDatabasesComboBox.SelectedItem as ComboBoxItem).Content as string;
            if (selectedDB == none_name) return;
            outputDBTool.SelectedDatabase = selectedDB;
            if(selectedDB == new_database_name)
            {
                outputLabel.Content = "db name";
                OutputTextBox.Visibility = Visibility.Visible;
            }    
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputDatabasesComboBox.SelectedItem == null || OutputDatabasesComboBox.SelectedItem == null)
                return;
            string inpCBText = (InputDatabasesComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            string outCBText = (OutputDatabasesComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (inpCBText == none_name || outCBText == none_name || (outCBText == new_database_name && string.IsNullOrEmpty(OutputTextBox.Text)))
                return;
            foreach(var db in outputDbs)
            {
                if (db == OutputTextBox.Text)
                    return;
            }
            analyzer.InputTool = inputDBTool;
            analyzer.OutputTool = outputDBTool;
            try
            {
                analyzer.Analyze();

            }catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            MessageBox.Show("sucess");
        }
    }
}

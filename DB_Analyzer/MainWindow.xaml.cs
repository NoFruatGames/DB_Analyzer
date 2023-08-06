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

        private async void InputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void inputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void OutputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void OutputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

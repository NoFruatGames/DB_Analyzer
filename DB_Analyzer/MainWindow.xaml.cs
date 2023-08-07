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
using DB_Analyzer_dll;
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
            fillInputProvidersComboBox();
            fillOutputProvidersComboBox();
        }
        readonly string none_name = "none";
        readonly string textfile_name = "text file";
        readonly string new_database_name = "new database";
        private void fillInputProvidersComboBox()
        {
            var providersNames = DBAnalyzer.Providers.GetProvidersList();
            InputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected=true });
            foreach (var pn in providersNames)
            {
                InputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
        }
        private void fillOutputProvidersComboBox()
        {
            var providersNames = DBAnalyzer.Providers.GetProvidersList();
            OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            foreach (var pn in providersNames)
            {
                OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
            OutputProvidersComboBox.Items.Add(new ComboBoxItem() { Content = textfile_name});
        }
        DBAnalyzer analyzer = new DBAnalyzer();
        private async void InputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InputDatabasesComboBox.Visibility = Visibility.Hidden;
            if (InputProvidersComboBox == null) return;
            string provider = (InputProvidersComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (provider == DBAnalyzer.Providers.SqlServerName)
                analyzer.SetInputDatabase(DB_Analyzer_dll.InputType.sql_server, ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
            else if (provider == DBAnalyzer.Providers.MySqlName)
                analyzer.SetInputDatabase(DB_Analyzer_dll.InputType.mysql, ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);
            else
                return;
            InputDatabasesComboBox.Visibility = Visibility.Visible;
            List<string> dbs = null;
            try
            {
                dbs = await analyzer.GetInputDatabasesAsync();
            }catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            InputDatabasesComboBox.Items.Clear();
            InputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            foreach(var db in dbs)
            {
                InputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = db });
            }
        }

        private void inputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void OutputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputProvidersComboBox == null) return;
            string provider = (OutputProvidersComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            OutputDatabasesComboBox.Visibility = Visibility.Hidden;
            if (provider == DBAnalyzer.Providers.SqlServerName)
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.sql_server, ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
            else if (provider == DBAnalyzer.Providers.MySqlName)
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.mysql, ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);
            else if (provider == textfile_name)
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.text_file, string.Empty);
            else
                return;
            OutputDatabasesComboBox.Visibility = Visibility.Visible;
            List<string> dbs = null;
            try
            {
                dbs = await analyzer.GetOutputDatabasesAsync();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            OutputDatabasesComboBox.Items.Clear();
            OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            foreach (var db in dbs)
            {
                OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = db });
            }

        }

        private void OutputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}

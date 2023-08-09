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
                analyzer.SetInputServer(DB_Analyzer_dll.InputType.sql_server, ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
            else if (provider == DBAnalyzer.Providers.MySqlName)
                analyzer.SetInputServer(DB_Analyzer_dll.InputType.mysql, ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);
            else
                return;
            InputDatabasesComboBox.Visibility = Visibility.Visible;
            List<string> dbs = null;
            InputDatabasesComboBox.Items.Clear();
            InputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            try
            {
                dbs = await analyzer.GetInputDatabasesAsync();
            }catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            if (dbs == null) return;
            foreach(var db in dbs)
            {
                InputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = db });
            }
        }

        private async void inputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void OutputProvidersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputProvidersComboBox == null) return;
            string provider = (OutputProvidersComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            OutputDatabasesComboBox.Visibility = Visibility.Hidden;
            OutputTextBox.Visibility = Visibility.Hidden;
            if (provider == DBAnalyzer.Providers.SqlServerName)
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.sql_server, ConfigurationManager.ConnectionStrings["sql server"].ConnectionString);
            else if (provider == DBAnalyzer.Providers.MySqlName)
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.mysql, ConfigurationManager.ConnectionStrings["mysql"].ConnectionString);
            else if (provider == textfile_name)
            {
                analyzer.SetOutputType(DB_Analyzer_dll.OutputType.text_file, string.Empty);
                OutputTextBox.Visibility = Visibility.Visible;
                return;
            }
            else
                return;
            OutputDatabasesComboBox.Visibility = Visibility.Visible;
            List<string> dbs = null;
            OutputDatabasesComboBox.Items.Clear();
            OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = none_name, IsSelected = true });
            try
            {
                dbs = await analyzer.GetOutputDatabasesAsync();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            if (dbs == null) return;
            foreach (var db in dbs)
            {
                OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = db });
            }
            OutputDatabasesComboBox.Items.Add(new ComboBoxItem() { Content = new_database_name });

        }

        private async void OutputDatabasesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OutputTextBox.Visibility = Visibility.Hidden;
            if (OutputDatabasesComboBox.SelectedItem == null || 
                (OutputDatabasesComboBox.SelectedItem as ComboBoxItem).Content.ToString() != new_database_name) return;
            OutputTextBox.Visibility = Visibility.Visible;
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            string outProv = (OutputProvidersComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if ((InputProvidersComboBox.SelectedItem as ComboBoxItem).Content.ToString() == none_name ||
                 outProv == none_name)
                return;
            string inpDB = (InputDatabasesComboBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (outProv == textfile_name && !string.IsNullOrEmpty(OutputTextBox.Text))
            {
                await analyzer.Analyze(inpDB, OutputTextBox.Text);
            }
            else
            {
                
                string outDB = (OutputDatabasesComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                if (inpDB == none_name || outDB == none_name) return;
                if (outDB == new_database_name)
                {
                    if (string.IsNullOrEmpty(OutputTextBox.Text)) return;
                    await analyzer.Analyze(inpDB, OutputTextBox.Text, true);
                }
                else
                {
                    await analyzer.Analyze(inpDB, outDB);
                }
            }

            //

            //if (inpDB == none_name || outDB == none_name) return;
            //string outTextBoxVal = OutputTextBox.Text;
            //if (outDB == new_database_name && string.IsNullOrEmpty(outTextBoxVal)) return;
            //if (outProv == textfile_name && string.IsNullOrEmpty(outTextBoxVal)) return;
            //MessageBox.Show("Sucess");
        }
    }
}

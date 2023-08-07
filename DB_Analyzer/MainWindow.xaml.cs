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

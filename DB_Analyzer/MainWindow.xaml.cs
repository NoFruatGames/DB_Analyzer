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
            fillProvidersComboBox(inputProviders);
        }
        private void registerProviders()
        {
            DbProviderFactories.RegisterFactory("sql server", SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("mysql", MySqlClientFactory.Instance);
        }
        private void fillProvidersComboBox(ComboBox comboBox)
        {
            var providersNames = DbProviderFactories.GetProviderInvariantNames();
            comboBox.Items.Add(new ComboBoxItem() { Content = "none", IsSelected=true });
            foreach (var pn in providersNames)
            {
                comboBox.Items.Add(new ComboBoxItem() { Content = pn });
            }
        }
    }
}

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
using TestWeatherControlApp.DataCall;

namespace TestWeatherControlApp.Pages
{
    /// <summary>
    /// Interaction logic for Page4.xaml
    /// </summary>
    public partial class Page4 : Page
    {
        public Page4()
        {
            InitializeComponent();
            //cWC.GetWeatherDataMethod = APIHelper.GetWeatherDataMethod;
        }
        public void addCity()
        {
            cWC.addData(tb1.Text);
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            cWC.addData(tb1.Text);
        }

        private void tb1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                cWC.addData(tb1.Text);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            cWC.clearData();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                // e.Cancel = true;
                label.Content = "no cleaning up, oh no we don't";
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ouch");
            // ((Button)sender).Visibility = Visibility.Collapsed;
            var but = ((Button)sender);
            but.Margin = new Thickness(273 - but.Margin.Left, but.Margin.Top, 273 - but.Margin.Right, but.Margin.Bottom);
        }
    }
}

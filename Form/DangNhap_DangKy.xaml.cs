using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ql_nhanSW.Form
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SwitchRegister_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            SignUpPanel.Visibility = Visibility.Visible;
        }

        private void SwitchLogin_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Visible;
            SignUpPanel.Visibility = Visibility.Collapsed;
        }
    }
}

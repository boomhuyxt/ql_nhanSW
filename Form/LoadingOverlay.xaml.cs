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
    public partial class LoadingOverlay : Window
    {
        public LoadingOverlay()
        {
            InitializeComponent();
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Mở lại cửa sổ đăng nhập
            Window1 loginWindow = new Window1();
            loginWindow.Show();

            // Đóng cửa sổ thông báo này
            this.Close();
        }
    }
}

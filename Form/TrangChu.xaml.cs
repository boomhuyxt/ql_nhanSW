using ql_nhanSW.Form.TrangChu;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ql_nhanSW
{
    /// <summary>
    /// Interaction logic for TrangChu.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new UC_DashBoard();
            SetActiveButton(BtnDashBoard);
        }

        private void SetActiveButton(Button activeBtn)
        {
            // Reset tất cả về mặc định
            BtnDashBoard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0010"));
            BtnDashBoard.BorderBrush = new SolidColorBrush(Colors.Transparent);
            BtnNhanSu.Background = new SolidColorBrush(Colors.Transparent);
            BtnPheDuyet.Background = new SolidColorBrush(Colors.Transparent);
            BtnCauHinhLuong.Background = new SolidColorBrush(Colors.Transparent);
            BtnBaoCaoTK.Background = new SolidColorBrush(Colors.Transparent);

            // Set button được click thành active
            activeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E0040"));
            activeBtn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED"));
        }

        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnDashBoard);
            MainContent.Content = new UC_DashBoard();
        }

        private void BtnNhanSu_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnNhanSu);
            MainContent.Content = new UC_NhanSu();
        }

        private void BtnPheDuyet_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnPheDuyet);
            //MainContent.Content = new UC_PheDuyet();
        }

        private void BtnCauHinhLuong_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnCauHinhLuong);
            MainContent.Content = new UC_CauHinhLuong();
        }

        private void BtnBaoCaoTK_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnBaoCaoTK);
            //MainContent.Content = new UC_BaoCaoTK();
        }
    }
}

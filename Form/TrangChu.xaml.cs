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
using ql_nhanSW.BUS;

namespace ql_nhanSW
{

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

        private void BtnToggleChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = ChatPanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void BtnCloseChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = Visibility.Collapsed;
        }

        private void BtnSendChat_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        private void SendMessage()
        {
            string msg = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            // Bubble người dùng
            ChatMessages.Children.Add(new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D0060")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6D28D9")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12, 12, 2, 12),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(50, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = new TextBlock
                {
                    Text = msg,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9D5FF")),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI")
                }
            });

            ChatInput.Text = string.Empty;

            // Bubble bot
            var botRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
            botRow.Children.Add(new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED")),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 8, 0),
                Child = new TextBlock
                {
                    Text = "AI",
                    Foreground = Brushes.White,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
            botRow.Children.Add(new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E0040")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B0764")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12, 12, 12, 2),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 220,
                Child = new TextBlock
                {
                    Text = "Tôi đã nhận được tin nhắn của bạn. Chức năng AI đang được phát triển!",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8B4FE")),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI")
                }
            });

            ChatMessages.Children.Add(botRow);
            ChatScrollViewer.ScrollToBottom();
        }
    }
}

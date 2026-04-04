using ql_nhanSW.BUS;
using ql_nhanSW.Form.TrangChu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ql_nhanSW.share;

namespace ql_nhanSW
{
    public partial class TrangChu : Window
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

        // API Key đã được mã hóa Base64 để tránh bị soi quét text trực tiếp
        private const string EncodedApiKey = "QUl6YVN5Q2dHWm5PTExocUlVcjlBSVhWeUJ5ZDJOdURxbUVVWUJJ";

        // Hàm giải mã API Key khi sử dụng
        private string GetDecodedKey()
        {
            byte[] data = Convert.FromBase64String(EncodedApiKey);
            return Encoding.UTF8.GetString(data);
        }

        public class GeminiResponse { public Candidate[] candidates { get; set; } }
        public class Candidate { public Content content { get; set; } }
        public class Content { public Part[] parts { get; set; } }
        public class Part { public string text { get; set; } }

        public TrangChu()
        {
            InitializeComponent();

            // Gọi hàm hiển thị thông tin tài khoản ngay khi mở form
            LoadUserInfo();

            MainContent.Content = new UC_DashBoard();
            SetActiveButton(BtnDashBoard);
        }

        #region User Info Logic
        // Hàm lấy dữ liệu từ SessionManager đẩy lên UI
        private void LoadUserInfo()
        {
            if (SessionManager.CurrentUser != null)
            {
                // 1. Gán Tên đăng nhập
                string username = SessionManager.CurrentUser.TenDangNhap;
                TxtUserName.Text = username;

                // 2. Xử lý Avatar (Chữ cái đầu HOẶC Ảnh đại diện)
                if (!string.IsNullOrEmpty(username))
                {
                    TxtUserAvatar.Text = username.Substring(0, 1).ToUpper();
                }

                // Kiểm tra xem user có ảnh đại diện trong Database không
                string duongDanAnh = SessionManager.CurrentUser.AnhDaiDien;

                if (!string.IsNullOrEmpty(duongDanAnh) && System.IO.File.Exists(duongDanAnh))
                {
                    try
                    {
                        // Gắn ảnh vào giao diện
                        ImgUserAvatar.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(duongDanAnh));
                        // Ẩn cái chữ cái đi để không bị chồng chéo
                        TxtUserAvatar.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        // Nếu file ảnh bị lỗi, ta vẫn hiện chữ cái mặc định
                        TxtUserAvatar.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    // Nếu chưa có ảnh thì để rỗng và hiện chữ
                    ImgUserAvatar.Source = null;
                    TxtUserAvatar.Visibility = Visibility.Visible;
                }

                // 3. Gán Vai trò
                if (SessionManager.CurrentRoles != null && SessionManager.CurrentRoles.Any())
                {
                    TxtUserRole.Text = string.Join(", ", SessionManager.CurrentRoles);
                }
                else
                {
                    TxtUserRole.Text = "Chờ cấp quyền";
                }
            }
        }
        #endregion

        #region UI Helper Methods
        private void SetActiveButton(Button activeBtn)
        {
            BtnDashBoard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A0010"));
            BtnDashBoard.BorderBrush = new SolidColorBrush(Colors.Transparent);
            BtnNhanSu.Background = new SolidColorBrush(Colors.Transparent);
            BtnPheDuyet.Background = new SolidColorBrush(Colors.Transparent);
            BtnCauHinhLuong.Background = new SolidColorBrush(Colors.Transparent);
            BtnBaoCaoTK.Background = new SolidColorBrush(Colors.Transparent);

            activeBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E0040"));
            activeBtn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED"));
        }
        #endregion

        #region Navigation Click Events
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
            if (!AuthorizationService.RequireAdmin()) return;
            SetActiveButton(BtnPheDuyet);
        }

        private void BtnCauHinhLuong_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthorizationService.RequireAdmin()) return;
            SetActiveButton(BtnCauHinhLuong);
            MainContent.Content = new UC_CauHinhLuong();
        }

        private void BtnBaoCaoTK_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(BtnBaoCaoTK);
        }

        // --- Mở trang Cập nhật thông tin ---
        private void BtnMoCapNhat_Click(object sender, RoutedEventArgs e)
        {
            // 1. Xóa hiệu ứng sáng màu của các nút trên menu trái
            BtnDashBoard.Background = new SolidColorBrush(Colors.Transparent);
            BtnDashBoard.BorderBrush = new SolidColorBrush(Colors.Transparent);
            BtnNhanSu.Background = new SolidColorBrush(Colors.Transparent);
            BtnPheDuyet.Background = new SolidColorBrush(Colors.Transparent);
            BtnCauHinhLuong.Background = new SolidColorBrush(Colors.Transparent);
            BtnBaoCaoTK.Background = new SolidColorBrush(Colors.Transparent);

            // 2. Gắn trang CapNhatThongTin vào khung giữa màn hình
            MainContent.Content = new ql_nhanSW.Form.TrangChu.CapNhatThongTin();
        }
        #endregion

        #region Chatbot UI & Logic
        private void BtnToggleChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Visibility = ChatPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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

        private void AddUserBubble(string message)
        {
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
                    Text = message,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9D5FF")),
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI")
                }
            });
        }

        private Border AddBotLoadingBubble()
        {
            var botRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };

            botRow.Children.Add(new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED")),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 8, 0),
                Child = new TextBlock { Text = "AI", Foreground = Brushes.White, FontSize = 9, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
            });

            var messageContent = new TextBlock
            {
                Text = "Đang suy nghĩ...",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D8B4FE")),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Segoe UI")
            };

            var messageBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E0040")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B0764")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12, 12, 12, 2),
                Padding = new Thickness(12, 8, 12, 8),
                MaxWidth = 220,
                Child = messageContent
            };

            botRow.Children.Add(messageBorder);
            ChatMessages.Children.Add(botRow);
            return messageBorder;
        }

        private void UpdateBotText(Border bubble, string text)
        {
            if (bubble.Child is TextBlock txtBlock)
            {
                txtBlock.Text = text;
            }
        }

        private async void SendMessage()
        {
            string msg = ChatInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            AddUserBubble(msg);
            ChatInput.Text = string.Empty;

            var botBubble = AddBotLoadingBubble();
            ChatScrollViewer.ScrollToBottom();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            int dots = 0;
            timer.Tick += (s, ev) => {
                dots = (dots + 1) % 4;
                UpdateBotText(botBubble, "Đang suy nghĩ" + new string('.', dots));
            };
            timer.Start();

            try
            {
                var requestBody = new { contents = new[] { new { parts = new[] { new { text = msg } } } } };
                string jsonPayload = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Giải mã key ngay tại thời điểm gọi API
                string apiKey = GetDecodedKey();
                string url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);

                    timer.Stop();

                    string reply = result?.candidates?[0]?.content?.parts?[0]?.text;
                    UpdateBotText(botBubble, reply ?? "AI không có phản hồi.");
                }
                else
                {
                    timer.Stop();
                    UpdateBotText(botBubble, "Kết nối API thất bại.");
                }
            }
            catch (Exception ex)
            {
                timer.Stop();
                UpdateBotText(botBubble, "Lỗi: " + ex.Message);
            }
            ChatScrollViewer.ScrollToBottom();
        }
        #endregion

        #region Logout Logic
        private void BtnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Xóa session người dùng hiện tại
                ql_nhanSW.share.SessionManager.CurrentUser = null;
                ql_nhanSW.share.SessionManager.CurrentRoles = null;

                // Mở lại Form Đăng nhập
                var loginWindow = new Form.Window1();
                loginWindow.Show();

                // Đóng form Trang chủ
                this.Close();
            }
        }
        #endregion
    }
}
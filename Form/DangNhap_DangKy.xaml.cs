using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ql_nhanSW.Models; // Đảm bảo đúng namespace của TaiKhoan và NhanVien

namespace ql_nhanSW.Form
{
    public partial class Window1 : Window
    {
        // Giả sử bạn đã có AppDbContext để kết nối Database QLNhanSu
         private readonly AppDbContext _db = new AppDbContext();

        public Window1()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #region Chuyển đổi giao diện
        private void SwitchRegister_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            SignUpPanel.Visibility = Visibility.Visible;
        }

        private void SwitchLogin_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Visible;
            SignUpPanel.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Xử lý Đăng Nhập
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hiệu ứng chuyển cảnh
            await PlayFadeAnimation(SignInPanel, false);
            LoadingOverlay.Visibility = Visibility.Visible;
            await PlayFadeAnimation(LoadingOverlay, true);

            // Giả lập kiểm tra Database (Sử dụng model TaiKhoan)
            var user = _db.TaiKhoans.FirstOrDefault(u => u.TenDangNhap == username && u.MatKhauHash == password);
            if (user != null) {
                if (user.TrangThai == 0) { // Chờ duyệt
                     // Giữ nguyên LoadingOverlay để hiện thông báo chờ duyệt
                } else {
                   
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
            } else {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!");
                LogoutBtn_Click(null, null); // Quay lại màn hình login
            }
            
        }
        #endregion

        #region Xử lý Đăng Ký
        private void Register_Click_1(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ giao diện
            string email = TxtRegEmail.Text.Trim();
            string username = TxtRegUsername.Text.Trim();
            string password = TxtRegPassword.Password;
            string confirmPass = TxtRegConfirmPassword.Password;

            // 2. Kiểm tra dữ liệu đầu vào cơ bản
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!");
                return;
            }

            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            try
            {
                // 3. Khởi tạo đối tượng Tài Khoản
                var newAccount = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhauHash = password, // Lưu ý: thực tế nên hash mật khẩu
                    Email = email,
                    SoDienThoai = "",
                    AnhDaiDien = "default.png",
                    TrangThai = 0, // 0: Chờ duyệt (Khớp với logic hiển thị LoadingOverlay)
                    NgayTao = DateTime.Now
                };

                // Thêm vào DbSet TaiKhoans và lưu để lấy MaTaiKhoan tự động tăng
                _db.TaiKhoans.Add(newAccount);
                _db.SaveChanges();

                // 4. Khởi tạo đối tượng Nhân Viên và liên kết với Tài Khoản
                var newEmployee = new NhanVien
                {
                    MaTaiKhoan = newAccount.MaTaiKhoan, // Lấy ID vừa sinh ra từ newAccount
                    HoTen = username, // Gán Họ tên bằng Tên đăng nhập theo yêu cầu của bạn
                    GioiTinh = "Chưa xác định",
                    DiaChi = "Chưa cập nhật",
                    TrangThai = 1, // Trạng thái hoạt động của nhân viên
                    NgayVaoLam = DateTime.Now
                };

                // Thêm vào DbSet NhanViens và lưu lần cuối
                _db.NhanViens.Add(newEmployee);
                _db.SaveChanges();

                MessageBox.Show("Đăng ký thành công! Tài khoản của bạn đang chờ quản trị viên phê duyệt.");

                SwitchLogin_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống khi lưu dữ liệu: " + ex.Message);
            }
        }
        #endregion

        #region Hiệu ứng & Thoát
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            SignInPanel.Visibility = Visibility.Visible;
            SignInPanel.Opacity = 1;
        }

        private async Task PlayFadeAnimation(UIElement element, bool fadeIn)
        {
            DoubleAnimation fade = new DoubleAnimation
            {
                From = fadeIn ? 0 : 1,
                To = fadeIn ? 1 : 0,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            element.BeginAnimation(OpacityProperty, fade);
            await Task.Delay(400);
            if (!fadeIn) element.Visibility = Visibility.Collapsed;
        }
        #endregion

       
    }
}
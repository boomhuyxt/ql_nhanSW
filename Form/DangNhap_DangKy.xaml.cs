using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.EntityFrameworkCore;
using ql_nhanSW.Models; // Để nhận diện TaiKhoan, VaiTro, NhanVien

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

            // 1. Kiểm tra tài khoản trong Database TRƯỚC khi chạy hiệu ứng
            var user = _db.TaiKhoans
                .FirstOrDefault(u => u.TenDangNhap == username && u.MatKhauHash == password);

            if (user != null)
            {
               // 2. Lấy danh sách vai trò 
                var roles = _db.Set<TaiKhoanVaiTro>()
                    .Where(rv => rv.MaTaiKhoan == user.MaTaiKhoan)
                    .Select(rv => rv.VaiTro.MaCode)
                    .ToList();

               // 3. Kiểm tra điều kiện vào thẳng 
                if (user.TrangThai != 0 && roles.Count > 0)
                {
                    // LƯU SESSION VÀ VÀO THẲNG [cite: 141]
                    ql_nhanSW.share.SessionManager.CurrentUser = user;
                    ql_nhanSW.share.SessionManager.CurrentRoles = roles;

                    MainWindow main = new MainWindow(); // [cite: 98]
                    main.Show();
                    this.Close();
                    return; // Kết thúc hàm, không chạy hiệu ứng Overlay bên dưới
                }

                // 4. Nếu chưa duyệt hoặc chưa có vai trò mới hiện LoadingOverlay 
                await PlayFadeAnimation(SignInPanel, false);
                LoadingOverlay.Visibility = Visibility.Visible;
                await PlayFadeAnimation(LoadingOverlay, true);
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                // Reset form nếu cần
                TxtPassword.Clear();
            }
        }
        #endregion

        #region Xử lý Đăng Ký
        private void Register_Click_1(object sender, RoutedEventArgs e)
        {
         
            string email = TxtRegEmail.Text.Trim();
            string username = TxtRegUsername.Text.Trim();
            string password = TxtRegPassword.Password;
            string confirmPass = TxtRegConfirmPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!");
                return;
            }
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            try
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                if (_db.TaiKhoans.Any(t => t.TenDangNhap == username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!");
                    return;
                }

              
                var newAccount = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhauHash = password, // Lưu ý: Nên dùng thư viện BCrypt để Hash mật khẩu thực tế
                    Email = email,
                  TrangThai = 0, // Mặc định chờ duyệt để hiện LoadingOverlay khi login [cite: 88, 91]
                    NgayTao = DateTime.Now
                };

                _db.TaiKhoans.Add(newAccount);
                _db.SaveChanges(); // Lưu để lấy MaTaiKhoan tự động tăng

                // Tạo hồ sơ nhân viên đi kèm
                var newEmployee = new NhanVien
                {
                    MaTaiKhoan = newAccount.MaTaiKhoan,
                    HoTen = username,
                    NgayVaoLam = DateTime.Now,
                    TrangThai = 1
                };
                _db.NhanViens.Add(newEmployee);
                _db.SaveChanges();

                MessageBox.Show("Đăng ký thành công! Vui lòng chờ quản trị viên phê duyệt.");
                SwitchLogin_Click(null, null); // Quay lại màn hình đăng nhập [cite: 87]
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
            }
        }
        #endregion

        #region Hiệu ứng & Thoát
        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Ẩn lớp phủ Loading
            LoadingOverlay.Visibility = Visibility.Collapsed;

            // Hiển thị lại bảng Đăng nhập
            SignInPanel.Visibility = Visibility.Visible;
            SignInPanel.Opacity = 1;

            // Xóa trắng thông tin cũ (tùy chọn)
            TxtPassword.Clear();
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
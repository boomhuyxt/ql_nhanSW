using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ql_nhanSW.Models;
using ql_nhanSW.share;
using Microsoft.EntityFrameworkCore;

namespace ql_nhanSW.Form
{
    public partial class Window1 : Window
    {
        private readonly AppDbContext _db = new AppDbContext();

        public Window1()
        {
            InitializeComponent();
        }

        // Cho phép kéo giữ cửa sổ
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #region Chuyển đổi giao diện Đăng nhập/Đăng ký
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
        #endregion

        #region Xử lý Đăng Nhập
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = TxtUsername.Text.Trim();
                string password = TxtPassword.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Lấy tài khoản và nạp luôn danh sách VaiTro để tránh lỗi Lazy Loading
                var user = _db.TaiKhoans
                    .Include(t => t.TaiKhoanVaiTro)
                        .ThenInclude(rv => rv.VaiTro)
                    .FirstOrDefault(u => u.TenDangNhap == username && u.MatKhauHash == password);

                if (user != null)
                {
                    // 2. Lấy danh sách mã vai trò trực tiếp từ đối tượng user đã nạp
                    var roles = user.TaiKhoanVaiTros
                        .Select(rv => rv.VaiTro.MaCode)
                        .ToList();

                    // 3. Kiểm tra sự tồn tại của vai trò
                    bool hasRoleAssignment = roles.Any();

                    // Lưu vào Session
                    SessionManager.CurrentUser = user;
                    SessionManager.CurrentRoles = roles;

                    // 4. KIỂM TRA ĐIỀU KIỆN (Sửa logic TrangThai để linh hoạt hơn)
                    // Nếu bạn muốn tài khoản admin (TrangThai = 1) vào thẳng, hãy dùng >= 1 hoặc != 0
                    if (user.TrangThai.GetValueOrDefault() != 0 && hasRoleAssignment)
                    {
                        // VÀO TRANG CHỦ
                        var main = new ql_nhanSW.TrangChu();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        // VÀO MÀN HÌNH CHỜ (Nếu TrangThai = 0 hoặc chưa có Role)
                        var loadingWindow = new ql_nhanSW.Form.LoadingOverlay();
                        loadingWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    TxtPassword.Clear();
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Lỗi hệ thống: " + innerMessage, "Thông báo lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Kiểm tra tên đăng nhập đã tồn tại chưa
                if (_db.TaiKhoans.Any(t => t.TenDangNhap == username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2. Tạo tài khoản mới
                var newAccount = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhauHash = password,
                    Email = email,
                    TrangThai = 0, // Chờ duyệt
                    NgayTao = DateTime.Now,

                    // SỬA LỖI TẠI ĐÂY: Gán giá trị mặc định cho các cột NOT NULL
                    AnhDaiDien = "default_avatar.png",
                    SoDienThoai = ""
                };

                _db.TaiKhoans.Add(newAccount);
                _db.SaveChanges(); // Lưu để SQL sinh ra MaTaiKhoan

                // 3. Tạo hồ sơ nhân viên đi kèm
                var newEmployee = new NhanVien
                {
                    MaTaiKhoan = newAccount.MaTaiKhoan,
                    HoTen = username,
                    NgayVaoLam = DateTime.Now,
                    TrangThai = 1,

                    // SỬA LỖI TẠI ĐÂY: Gán giá trị mặc định cho các cột NOT NULL
                    DiaChi = "Chưa cập nhật",
                    GioiTinh = "Chưa xác định"
                };

                _db.NhanViens.Add(newEmployee);
                _db.SaveChanges(); // Lưu hồ sơ nhân viên


                MessageBox.Show("Đăng ký thành công! Vui lòng chờ quản trị viên phê duyệt.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Quay lại màn hình đăng nhập
                SwitchLogin_Click(null, null);
            }
            catch (Exception ex)
            {
                // Lấy thông báo lỗi chi tiết từ SQL Server
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Lỗi hệ thống: " + innerMessage, "Lỗi kết nối cơ sở dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
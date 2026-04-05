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
                // Lấy Email từ ô nhập liệu mới (TxtEmail)
                string email = TxtEmail.Text.Trim();
                string password = TxtPassword.Password;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Tìm tài khoản bằng Email thay vì TenDangNhap
                var user = _db.TaiKhoans
                    .Include(t => t.TaiKhoanVaiTro)
                        .ThenInclude(rv => rv.VaiTro)
                    .FirstOrDefault(u => u.Email == email && u.MatKhauHash == password);

                if (user != null)
                {
                    // ... (Giữ nguyên các logic phân quyền và chuyển trang phía sau)
                    var roles = user.TaiKhoanVaiTro.Select(rv => rv.VaiTro.MaCode).ToList();
                    SessionManager.CurrentUser = user;
                    SessionManager.CurrentRoles = roles;

                    if (user.TrangThai.GetValueOrDefault() != 0 && roles.Any())
                    {
                        var main = new ql_nhanSW.TrangChu();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        var loadingWindow = new ql_nhanSW.Form.LoadingOverlay();
                        loadingWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Email hoặc mật khẩu không chính xác!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ql_nhanSW.Models;
using ql_nhanSW.share;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ql_nhanSW.Form
{
    public partial class Window1 : Window
    {
        private readonly AppDbContext _db = new AppDbContext();

        public Window1()
        {
            InitializeComponent();
        }

        // Hàm hỗ trợ băm chuỗi thành SHA-256
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
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
        }
        #endregion

        #region Xử lý Đăng Nhập
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = TxtEmail.Text.Trim();
                string password = TxtPassword.Password;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                    return;
                }

                // 1. Tìm tài khoản theo Email trước để lấy chuỗi mật khẩu trong DB ra kiểm tra
                var user = _db.TaiKhoans
                    .Include(t => t.TaiKhoanVaiTro)
                        .ThenInclude(rv => rv.VaiTro)
                    .FirstOrDefault(u => u.Email == email);

                if (user != null)
                {
                    bool isPasswordCorrect = false;
                    string hashedInput = ComputeSha256Hash(password);

                    // 2. KIỂM TRA ĐA LUỒNG:
                    // TH1: So sánh với mã đã hash SHA-256
                    // TH2: So sánh trực tiếp với mật khẩu chưa hash (plaintext)
                    if (user.MatKhauHash == hashedInput || user.MatKhauHash == password)
                    {
                        isPasswordCorrect = true;
                    }

                    if (isPasswordCorrect)
                    {
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
                        MessageBox.Show("Mật khẩu không chính xác!");
                        TxtPassword.Clear();
                    }
                }
                else
                {
                    MessageBox.Show("Email không tồn tại!");
                    TxtPassword.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
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
                MessageBox.Show("Vui lòng điền đủ thông tin!");
                return;
            }
            if (password != confirmPass)
            {
                MessageBox.Show("Mật khẩu không khớp!");
                return;
            }

            try
            {
                if (_db.TaiKhoans.Any(t => t.TenDangNhap == username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!");
                    return;
                }

                // Khi đăng ký mới, chúng ta LUÔN LUÔN băm mật khẩu để đảm bảo bảo mật
                string hashedPw = ComputeSha256Hash(password);

                var newAccount = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhauHash = hashedPw,
                    Email = email,
                    TrangThai = 0,
                    NgayTao = DateTime.Now,
                    AnhDaiDien = "default_avatar.png",
                    SoDienThoai = ""
                };

                _db.TaiKhoans.Add(newAccount);
                _db.SaveChanges();

                var newEmployee = new NhanVien
                {
                    MaTaiKhoan = newAccount.MaTaiKhoan,
                    HoTen = username,
                    NgayVaoLam = DateTime.Now,
                    TrangThai = 1,
                    DiaChi = "Chưa cập nhật",
                    GioiTinh = "Chưa xác định"
                };

                _db.NhanViens.Add(newEmployee);
                _db.SaveChanges();

                MessageBox.Show("Đăng ký thành công!");
                SwitchLogin_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        #endregion
    }
}
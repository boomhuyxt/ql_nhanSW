using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ql_nhanSW.Models;
using ql_nhanSW.share;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class CapNhatThongTin : UserControl
    {
        // Khởi tạo kết nối Database
        private readonly AppDbContext _db = new AppDbContext();

        // Biến lưu tạm đường dẫn ảnh khi người dùng chọn
        private string _duongDanAnhMoi = "";

        public CapNhatThongTin()
        {
            InitializeComponent();
            LoadData(); // Gọi hàm load dữ liệu ngay khi form vừa mở lên
        }

        // =================================================================
        // 1. HÀM TẢI DỮ LIỆU CŨ LÊN FORM
        // =================================================================
        private void LoadData()
        {
            if (SessionManager.CurrentUser != null)
            {
                int maTK = SessionManager.CurrentUser.MaTaiKhoan;

                // Lấy thông tin từ Database dựa trên tài khoản đang đăng nhập
                var tk = _db.TaiKhoans.FirstOrDefault(t => t.MaTaiKhoan == maTK);
                var nv = _db.NhanViens.FirstOrDefault(n => n.MaTaiKhoan == maTK);

                if (tk != null)
                {
                    // Thông tin tài khoản
                    txtTenDangNhap.Text = tk.TenDangNhap;
                    txtTenDangNhap.IsReadOnly = true;

                    txtEmail.Text = tk.Email;
                    txtEmail.IsReadOnly = true;

                    txtSoDienThoai.Text = tk.SoDienThoai;

                    // Load ảnh đại diện nếu có
                    if (!string.IsNullOrEmpty(tk.AnhDaiDien) && File.Exists(tk.AnhDaiDien))
                    {
                        try
                        {
                            imgAvatar.Source = new BitmapImage(new Uri(tk.AnhDaiDien));
                        }
                        catch { /* Bỏ qua nếu ảnh lỗi */ }
                    }
                }

                if (nv != null)
                {
                    // Thông tin nhân viên
                    if (nv.GioiTinh == "Nữ") rdoNu.IsChecked = true;
                    else rdoNam.IsChecked = true;

                    dpNgaySinh.SelectedDate = nv.NgaySinh;
                    txtDiaChi.Text = nv.DiaChi;
                }
            }
        }

        // =================================================================
        // 2. HÀM CHỌN ẢNH ĐẠI BIỂN MỚI
        // =================================================================
        private void btnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _duongDanAnhMoi = openFileDialog.FileName;

                // Hiển thị tạm ảnh vừa chọn lên giao diện
                imgAvatar.Source = new BitmapImage(new Uri(_duongDanAnhMoi));
            }
        }

        // =================================================================
        // 3. HÀM LƯU CẬP NHẬT & ĐÁNH DẤU ĐỂ ẨN THÔNG BÁO TRANG CHỦ
        // =================================================================
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser == null)
            {
                MessageBox.Show("Vui lòng đăng nhập lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int maTK = SessionManager.CurrentUser.MaTaiKhoan;
                var tk = _db.TaiKhoans.FirstOrDefault(t => t.MaTaiKhoan == maTK);
                var nv = _db.NhanViens.FirstOrDefault(n => n.MaTaiKhoan == maTK);

                if (tk != null)
                {
                    // --- 3.1. XỬ LÝ LƯU ẢNH ---
                    if (!string.IsNullOrEmpty(_duongDanAnhMoi))
                    {
                        string thuMucLuu = @"C:\AvataNhanSu";
                        if (!Directory.Exists(thuMucLuu)) Directory.CreateDirectory(thuMucLuu);

                        string tenFileGoc = Path.GetFileName(_duongDanAnhMoi);
                        string tenFileMoi = $"TK{maTK}_{tenFileGoc}";
                        string duongDanLuu = Path.Combine(thuMucLuu, tenFileMoi);

                        File.Copy(_duongDanAnhMoi, duongDanLuu, true);
                        tk.AnhDaiDien = duongDanLuu;
                    }

                    // --- 3.2. CẬP NHẬT THÔNG TIN ---
                    tk.SoDienThoai = txtSoDienThoai.Text.Trim();
                    tk.NgayCapNhat = DateTime.Now;

                    if (nv != null)
                    {
                        nv.GioiTinh = (rdoNam.IsChecked == true) ? "Nam" : "Nữ";
                        nv.NgaySinh = dpNgaySinh.SelectedDate;
                        nv.DiaChi = txtDiaChi.Text.Trim();
                    }

                    // --- LƯU XUỐNG DATABASE ---
                    _db.SaveChanges();

                    // --- ĐÁNH DẤU ĐÃ CẬP NHẬT (Để ẩn thông báo ở Bảng Điều Khiển) ---
                    GhiNhanDaCapNhat();

                    // Cập nhật lại session
                    SessionManager.CurrentUser = tk;

                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm tạo file đánh dấu trong bộ nhớ ứng dụng
        private void GhiNhanDaCapNhat()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
                using (var stream = new IsolatedStorageFileStream("DaCapNhatThongTin.txt", FileMode.Create, store))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("true");
                }
            }
            catch { /* Tránh gây treo ứng dụng nếu lỗi file hệ thống */ }
        }

        // =================================================================
        // 4. HÀM HỦY BỎ
        // =================================================================
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            _duongDanAnhMoi = "";
            LoadData();
        }
    }
}
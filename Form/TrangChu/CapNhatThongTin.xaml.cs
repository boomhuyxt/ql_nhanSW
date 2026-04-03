using System;
using System.IO;
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
                    txtTenDangNhap.IsReadOnly = true; // Khóa không cho sửa Tên đăng nhập

                    txtEmail.Text = tk.Email;
                    txtEmail.IsReadOnly = true; // Thường email cũng không cho tự sửa (tùy nghiệp vụ)

                    txtSoDienThoai.Text = tk.SoDienThoai;

                    // Load ảnh đại diện nếu có
                    if (!string.IsNullOrEmpty(tk.AnhDaiDien) && File.Exists(tk.AnhDaiDien))
                    {
                        imgAvatar.Source = new BitmapImage(new Uri(tk.AnhDaiDien));
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
        // 2. HÀM CHỌN ẢNH ĐẠI DIỆN MỚI
        // =================================================================
        private void btnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _duongDanAnhMoi = openFileDialog.FileName;

                // Hiển thị tạm ảnh vừa chọn lên giao diện (chưa lưu vào DB)
                imgAvatar.Source = new BitmapImage(new Uri(_duongDanAnhMoi));
            }
        }

        // =================================================================
        // 3. HÀM LƯU CẬP NHẬT
        // =================================================================
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra an toàn xem có mất session không
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

                if (tk != null && nv != null)
                {
                    // --- XỬ LÝ LƯU ẢNH VÀO C:\AvataNhanSu ---
                    if (!string.IsNullOrEmpty(_duongDanAnhMoi))
                    {
                        string thuMucLuu = @"C:\AvataNhanSu";

                        // Nếu thư mục chưa tồn tại trên ổ C thì tự động tạo mới
                        if (!Directory.Exists(thuMucLuu))
                        {
                            Directory.CreateDirectory(thuMucLuu);
                        }

                        // Lấy tên file gốc (VD: anh1.png)
                        string tenFileGoc = Path.GetFileName(_duongDanAnhMoi);

                        // Ghép thêm Mã tài khoản vào tên file để tránh 2 người up ảnh trùng tên nhau
                        string tenFileMoi = $"TK{maTK}_{tenFileGoc}";
                        string duongDanLuu = Path.Combine(thuMucLuu, tenFileMoi);

                        // Copy file ảnh từ máy vào thư mục C:\AvataNhanSu (cho phép ghi đè)
                        File.Copy(_duongDanAnhMoi, duongDanLuu, true);

                        // Cập nhật đường dẫn mới vào DB
                        tk.AnhDaiDien = duongDanLuu;
                    }

                    // --- CẬP NHẬT THÔNG TIN BẢNG TaiKhoan ---
                    tk.SoDienThoai = txtSoDienThoai.Text.Trim();
                    tk.NgayCapNhat = DateTime.Now;

                    // --- CẬP NHẬT THÔNG TIN BẢNG NhanVien ---
                    nv.GioiTinh = (rdoNam.IsChecked == true) ? "Nam" : "Nữ";
                    nv.NgaySinh = dpNgaySinh.SelectedDate;
                    nv.DiaChi = txtDiaChi.Text.Trim();

                    // --- LƯU XUỐNG DATABASE ---
                    _db.SaveChanges();

                    // Cập nhật lại biến toàn cục để trên góc màn hình thay đổi theo (nếu có)
                    SessionManager.CurrentUser = tk;

                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =================================================================
        // 4. HÀM HỦY BỎ
        // =================================================================
        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            // Reset lại giao diện bằng cách Load lại dữ liệu từ DB
            _duongDanAnhMoi = "";
            LoadData();
        }
    }
}
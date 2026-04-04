using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_nhanSW.Models;
// using ql_nhanSW.BUS; // Không cần dùng file BUS cũ nữa vì tính thẳng ở giao diện

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_CauHinhLuong : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        private NhanVien _nhanVienHienTai;
        private Luong _luongTamThoi;

        public UC_CauHinhLuong()
        {
            InitializeComponent();
            LoadComboNhanVien();

            // Set mặc định là tháng/năm hiện tại
            txtThang.Text = DateTime.Now.Month.ToString();
            txtNam.Text = DateTime.Now.Year.ToString();
        }

        // ==========================================
        // 1. TẢI DANH SÁCH NHÂN VIÊN VÀO COMBOBOX
        // ==========================================
        private void LoadComboNhanVien()
        {
            var ds = _db.NhanViens.Select(nv => new
            {
                MaNhanVien = nv.MaNhanVien,
                HoTen = nv.HoTen ?? "Không tên"
            }).ToList();

            cmbNhanVien.ItemsSource = ds;
            cmbNhanVien.DisplayMemberPath = "HoTen";
            cmbNhanVien.SelectedValuePath = "MaNhanVien";
        }

        // ==========================================
        // 2. KHI CHỌN NHÂN VIÊN
        // ==========================================
        private void cmbNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbNhanVien.SelectedValue is int maNV)
            {
                _nhanVienHienTai = _db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == maNV);

                if (_nhanVienHienTai != null)
                {
                    txtHoTen.Text = _nhanVienHienTai.HoTen ?? "Không tên";
                    txtMaNV.Text = $"MNV: NV{_nhanVienHienTai.MaNhanVien:D3}";
                }
            }
        }

        // ==========================================
        // 3. TÍNH LƯƠNG TRỰC TIẾP TỪ GIAO DIỆN
        // ==========================================
        private void btnTinhLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_nhanVienHienTai == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần tính lương!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Dùng Replace(",", "") để chống lỗi crash nếu người dùng nhập số có dấu phẩy (VD: 25,000,000)
                int thang = int.Parse(txtThang.Text.Trim());
                int nam = int.Parse(txtNam.Text.Trim());
                decimal luongCoBan = decimal.Parse(txtLuongCoBan.Text.Replace(",", "").Trim());
                decimal thuong = string.IsNullOrEmpty(txtThuong.Text) ? 0 : decimal.Parse(txtThuong.Text.Replace(",", "").Trim());
                decimal khauTru = string.IsNullOrEmpty(txtKhauTru.Text) ? 0 : decimal.Parse(txtKhauTru.Text.Replace(",", "").Trim());
                int soNgayCong = string.IsNullOrEmpty(txtSoNgayCong.Text) ? 26 : int.Parse(txtSoNgayCong.Text.Trim());

                // Tính toán: Lương thực tế = (Lương cơ bản / 26) * số ngày công
                decimal luongTheoNgay = (luongCoBan / 26) * soNgayCong;
                decimal tongThucNhan = luongTheoNgay + thuong - khauTru;

                // Lưu tạm vào biến _luongTamThoi để chuẩn bị đưa vào Database
                _luongTamThoi = new Luong
                {
                    MaNhanVien = _nhanVienHienTai.MaNhanVien,
                    Thang = thang,
                    Nam = nam,
                    LuongCoBan = luongCoBan,
                    Thuong = thuong,
                    KhauTru = khauTru,
                    TongLuong = Math.Round(tongThucNhan, 0)
                };

                // Đẩy kết quả hiển thị xuống các ô bên dưới
                tbLuongCoBan.Text = _luongTamThoi.LuongCoBan.ToString("N0");
                tbThuong.Text = _luongTamThoi.Thuong.ToString("N0");
                tbKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0");
                tbTongLuong.Text = _luongTamThoi.TongLuong.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nhập liệu, vui lòng chỉ nhập số hợp lệ: " + ex.Message, "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // 4. LƯU BẢN LƯƠNG VÀO DATABASE
        // ==========================================
        private void btnXacNhanXuatLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_luongTamThoi == null)
            {
                MessageBox.Show("Vui lòng ấn nút 'Tính lương' trước khi Xác nhận xuất lương!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Kiểm tra xem nhân viên này tháng/năm đó đã tính lương chưa
                var luongCu = _db.Luongs.FirstOrDefault(l => l.MaNhanVien == _luongTamThoi.MaNhanVien
                                                          && l.Thang == _luongTamThoi.Thang
                                                          && l.Nam == _luongTamThoi.Nam);

                if (luongCu != null)
                {
                    // NẾU ĐÃ CÓ LƯƠNG -> CẬP NHẬT GHI ĐÈ LẠI
                    luongCu.LuongCoBan = _luongTamThoi.LuongCoBan;
                    luongCu.Thuong = _luongTamThoi.Thuong;
                    luongCu.KhauTru = _luongTamThoi.KhauTru;
                    luongCu.TongLuong = _luongTamThoi.TongLuong;

                    MessageBox.Show($"✅ Đã cập nhật lại bảng lương tháng {_luongTamThoi.Thang}/{_luongTamThoi.Nam} cho {_nhanVienHienTai?.HoTen} thành công!",
                                    "Cập nhật thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // NẾU CHƯA CÓ LƯƠNG -> THÊM DÒNG MỚI
                    _db.Luongs.Add(_luongTamThoi);

                    MessageBox.Show($"✅ Xuất lương mới thành công cho {_nhanVienHienTai?.HoTen}!\nTổng thực nhận: {_luongTamThoi.TongLuong:N0} VNĐ",
                                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // LƯU XUỐNG CƠ SỞ DỮ LIỆU
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu Database: " + ex.Message, "Lỗi Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
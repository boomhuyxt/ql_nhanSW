using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_nhanSW.Models;
using ql_nhanSW.BUS; // Cần thiết để gọi TinhLuongService

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_CauHinhLuong : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly TinhLuongService _tinhLuongBUS = new TinhLuongService(); // Khởi tạo Service

        private NhanVien _nhanVienHienTai;
        private Luong _luongTamThoi;

        public UC_CauHinhLuong()
        {
            InitializeComponent();
            LoadComboNhanVien();

            txtThang.Text = DateTime.Now.Month.ToString();
            txtNam.Text = DateTime.Now.Year.ToString();
        }

        // 1. Tải danh sách nhân viên với đầy đủ thông tin: Họ tên, Giới tính, Ngày sinh
        private void LoadComboNhanVien()
        {
            var ds = _db.NhanViens.Select(nv => new
            {
                MaNhanVien = nv.MaNhanVien,
                HoTen = nv.HoTen ?? "Không tên",
                GioiTinh = nv.GioiTinh ?? "N/A",
                NgaySinh = nv.NgaySinh // Dùng để hiển thị trong Template của XAML
            }).ToList();

            cmbNhanVien.ItemsSource = ds;
            cmbNhanVien.SelectedValuePath = "MaNhanVien";
        }

        // 2. Khi chọn nhân viên từ danh sách
        private void cmbNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbNhanVien.SelectedValue is int maNV)
            {
                _nhanVienHienTai = _db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == maNV);

                if (_nhanVienHienTai != null)
                {
                    txtHoTen.Text = _nhanVienHienTai.HoTen;
                    txtMaNV.Text = $"MNV: NV{_nhanVienHienTai.MaNhanVien:D3} • {_nhanVienHienTai.GioiTinh}";
                }
            }
        }

        // 3. Áp dụng thuật toán tính lương từ TinhLuong.cs
        private void btnTinhLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_nhanVienHienTai == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Lấy dữ liệu từ giao diện
                int thang = int.Parse(txtThang.Text.Trim());
                int nam = int.Parse(txtNam.Text.Trim());
                decimal luongCoBan = decimal.Parse(txtLuongCoBan.Text.Replace(",", "").Trim());
                decimal thuong = string.IsNullOrEmpty(txtThuong.Text) ? 0 : decimal.Parse(txtThuong.Text.Replace(",", "").Trim());
                int soNgayCong = string.IsNullOrEmpty(txtSoNgayCong.Text) ? 26 : int.Parse(txtSoNgayCong.Text.Trim());

                // GỌI THUẬT TOÁN TỪ BUS
                // Thuật toán này tự động trừ 10.5% bảo hiểm và 10% thuế
                _luongTamThoi = _tinhLuongBUS.TinhVaTaoLuong(
                    _nhanVienHienTai.MaNhanVien,
                    thang,
                    nam,
                    luongCoBan,
                    thuong,
                    soNgayCong
                );

                // Cập nhật hiển thị kết quả lên giao diện
                tbLuongCoBan.Text = _luongTamThoi.LuongCoBan.ToString("N0");
                tbThuong.Text = _luongTamThoi.Thuong.ToString("N0");
                tbKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0"); // Hiển thị số tiền thuế + bảo hiểm đã trừ
                tbTongLuong.Text = _luongTamThoi.TongLuong.ToString("N0");

                // Hiển thị thông tin khấu trừ vào ô nhập (để người dùng biết đã trừ bao nhiêu)
                txtKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dữ liệu nhập không hợp lệ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 4. Xác nhận và lưu vào Database
        private void btnXacNhanXuatLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_luongTamThoi == null) return;

            try
            {
                var luongTonTai = _db.Luongs.FirstOrDefault(l => l.MaNhanVien == _luongTamThoi.MaNhanVien
                                                              && l.Thang == _luongTamThoi.Thang
                                                              && l.Nam == _luongTamThoi.Nam);

                if (luongTonTai != null)
                {
                    luongTonTai.LuongCoBan = _luongTamThoi.LuongCoBan;
                    luongTonTai.Thuong = _luongTamThoi.Thuong;
                    luongTonTai.KhauTru = _luongTamThoi.KhauTru;
                    luongTonTai.TongLuong = _luongTamThoi.TongLuong;
                    _db.Entry(luongTonTai).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                }
                else
                {
                    // Tạo mới và ép kiểu tường minh ID
                    var luongMoi = new Luong
                    {
                        MaNhanVien = _nhanVienHienTai.MaNhanVien, // Lấy trực tiếp từ nhân viên đang chọn
                        Thang = _luongTamThoi.Thang,
                        Nam = _luongTamThoi.Nam,
                        LuongCoBan = _luongTamThoi.LuongCoBan,
                        Thuong = _luongTamThoi.Thuong,
                        KhauTru = _luongTamThoi.KhauTru,
                        TongLuong = _luongTamThoi.TongLuong
                    };
                    _db.Luongs.Add(luongMoi);
                }

                _db.SaveChanges();
                MessageBox.Show("✅ Đã lưu lương thành công!", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }


    }
}

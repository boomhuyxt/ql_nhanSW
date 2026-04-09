using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_nhanSW.Models;
using ql_nhanSW.BUS;
using Microsoft.EntityFrameworkCore;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_CauHinhLuong : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private readonly TinhLuongService _tinhLuongBUS = new TinhLuongService();

        private NhanVien _nhanVienHienTai;
        private Luong _luongTamThoi;

        public UC_CauHinhLuong()
        {
            InitializeComponent();
            LoadComboNhanVien();

            txtThang.Text = DateTime.Now.Month.ToString();
            txtNam.Text = DateTime.Now.Year.ToString();

            // Cập nhật thống kê ngay khi load form
            CapNhatThongKeQuyLuong();
        }

        // 1. Tải danh sách nhân viên
        private void LoadComboNhanVien()
        {
            var ds = _db.NhanViens.Select(nv => new
            {
                MaNhanVien = nv.MaNhanVien,
                HoTen = nv.HoTen ?? "Không tên",
                GioiTinh = nv.GioiTinh ?? "N/A",
                NgaySinh = nv.NgaySinh
            }).ToList();

            cmbNhanVien.ItemsSource = ds;
            cmbNhanVien.SelectedValuePath = "MaNhanVien";
        }

        // 2. Hàm cập nhật Thống kê Quỹ lương (Tổng tiền đã trả)
        private void CapNhatThongKeQuyLuong()
        {
            try
            {
                if (int.TryParse(txtThang.Text, out int thang) && int.TryParse(txtNam.Text, out int nam))
                {
                    // Tính tổng cột TongLuong trong database cho tháng/năm này
                    decimal tongTien = _db.Luongs
                        .Where(l => l.Thang == thang && l.Nam == nam)
                        .Sum(l => (decimal?)l.TongLuong) ?? 0;

                    // Hiển thị lên giao diện
                    txtTongQuyLuong.Text = tongTien.ToString("N0") + " VND";

                    // Hiệu ứng thanh Progress Bar (Ví dụ: Giả định quỹ tối đa là 1 tỷ để tính độ dài)
                    // Bạn có thể chỉnh sửa con số 1,000,000,000 tùy quy mô công ty
                    double maxBudget = 1000000000;
                    double phanTram = (double)tongTien / maxBudget;
                    if (phanTram > 1) phanTram = 1;

                    rectProgress.Width = phanTram * 250; // 250 là chiều rộng tối đa của thanh bar
                }
            }
            catch { /* Bỏ qua lỗi khi đang nhập dở dang */ }
        }

        // 3. Khi chọn nhân viên
        private void cmbNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbNhanVien.SelectedValue is int maNV)
            {
                _nhanVienHienTai = _db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == maNV);
                if (_nhanVienHienTai != null)
                {
                    txtHoTen.Text = _nhanVienHienTai.HoTen;
                    txtMaNV.Text = $"MNV: NV{_nhanVienHienTai.MaNhanVien:D3} • {_nhanVienHienTai.GioiTinh}";

                    // Mỗi khi đổi nhân viên, cập nhật lại thống kê tháng đó
                    CapNhatThongKeQuyLuong();
                }
            }
        }

        // 4. Tính lương
        private void btnTinhLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_nhanVienHienTai == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int thang = int.Parse(txtThang.Text.Trim());
                int nam = int.Parse(txtNam.Text.Trim());
                decimal luongCoBan = decimal.Parse(txtLuongCoBan.Text.Replace(",", "").Trim());
                decimal thuong = string.IsNullOrEmpty(txtThuong.Text) ? 0 : decimal.Parse(txtThuong.Text.Replace(",", "").Trim());
                int soNgayCong = string.IsNullOrEmpty(txtSoNgayCong.Text) ? 26 : int.Parse(txtSoNgayCong.Text.Trim());

                _luongTamThoi = _tinhLuongBUS.TinhVaTaoLuong(
                    _nhanVienHienTai.MaNhanVien,
                    thang,
                    nam,
                    luongCoBan,
                    thuong,
                    soNgayCong
                );

                tbLuongCoBan.Text = _luongTamThoi.LuongCoBan.ToString("N0");
                tbThuong.Text = _luongTamThoi.Thuong.ToString("N0");
                tbKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0");
                tbTongLuong.Text = _luongTamThoi.TongLuong.ToString("N0");
                txtKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dữ liệu nhập không hợp lệ: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 5. Xác nhận và Lưu
        private void btnXacNhanXuatLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_luongTamThoi == null)
            {
                MessageBox.Show("Hãy nhấn 'Tính Lương' trước khi lưu!", "Thông báo");
                return;
            }

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
                }
                else
                {
                    _db.Luongs.Add(_luongTamThoi);
                }

                _db.SaveChanges();

                // Cập nhật lại Quỹ lương ngay sau khi lưu thành công
                CapNhatThongKeQuyLuong();

                MessageBox.Show("✅ Đã lưu lương thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
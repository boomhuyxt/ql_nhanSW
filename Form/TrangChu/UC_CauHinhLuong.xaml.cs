using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_nhanSW.Models;
using ql_nhanSW.BUS;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_CauHinhLuong : UserControl
    {
        private readonly TinhLuongService _service = new TinhLuongService();
        private readonly AppDbContext _db = new AppDbContext();

        private NhanVien _nhanVienHienTai;
        private Luong _luongTamThoi;

        public UC_CauHinhLuong()
        {
            InitializeComponent();
            LoadComboNhanVien();
            txtThang.Text = DateTime.Now.Month.ToString();
            txtNam.Text = DateTime.Now.Year.ToString();
        }

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

        private void cmbNhanVien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbNhanVien.SelectedValue is int maNV)
            {
                _nhanVienHienTai = _db.NhanViens.FirstOrDefault(nv => nv.MaNhanVien == maNV);
                if (_nhanVienHienTai != null)
                {
                    txtHoTen.Text = _nhanVienHienTai.HoTen ?? "Không tên";
                    //txtMaNV.Text = $"MNV: {_nhanVienHienTai.MaNhanVien} • {_nhanVienHienTai.ChucVu ?? ""}";
                }
            }
        }

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
                decimal luongCoBan = decimal.Parse(txtLuongCoBan.Text.Trim());
                decimal thuong = string.IsNullOrEmpty(txtThuong.Text) ? 0 : decimal.Parse(txtThuong.Text.Trim());
                int soNgayCong = string.IsNullOrEmpty(txtSoNgayCong.Text) ? 26 : int.Parse(txtSoNgayCong.Text.Trim());

                _luongTamThoi = _service.TinhVaTaoLuong(_nhanVienHienTai.MaNhanVien, thang, nam, luongCoBan, thuong, soNgayCong);

                tbLuongCoBan.Text = _luongTamThoi.LuongCoBan.ToString("N0");
                tbThuong.Text = _luongTamThoi.Thuong.ToString("N0");
                tbKhauTru.Text = _luongTamThoi.KhauTru.ToString("N0");
                tbTongLuong.Text = _luongTamThoi.TongLuong.ToString("N0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnXacNhanXuatLuong_Click(object sender, RoutedEventArgs e)
        {
            if (_luongTamThoi == null)
            {
                MessageBox.Show("Vui lòng tính lương trước khi xác nhận!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_db.Luongs.Any(l => l.MaNhanVien == _luongTamThoi.MaNhanVien
                                     && l.Thang == _luongTamThoi.Thang
                                     && l.Nam == _luongTamThoi.Nam))
                {
                    MessageBox.Show("Lương tháng này đã tồn tại!", "Cảnh báo");
                    return;
                }

                _db.Luongs.Add(_luongTamThoi);
                _db.SaveChanges();

                MessageBox.Show($"✅ Xuất lương thành công cho {_nhanVienHienTai?.HoTen}!\nTổng thực nhận: {_luongTamThoi.TongLuong:N0} VNĐ",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu lương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
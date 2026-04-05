using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ql_nhanSW.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_DashBoard : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();

        public UC_DashBoard()
        {
            InitializeComponent();
            RefreshData();
        }

        // Hàm làm mới toàn bộ dữ liệu trên tất cả các Tab
        private void RefreshData()
        {
            try
            {
                // 1. Thống kê KPI tài khoản
                int tongTaiKhoan = _db.TaiKhoans.Count();
                txtTongNhanSu.Text = tongTaiKhoan.ToString("N0");

                var listChoDuyet = _db.TaiKhoans.Where(t => t.TrangThai == 0).ToList();
                txtChoDuyetCount.Text = listChoDuyet.Count.ToString();

                // 2. Nạp dữ liệu Tab "Phê duyệt mới"
                ItemsPheDuyet.ItemsSource = listChoDuyet;

                // 3. Nạp dữ liệu Tab "Quản lý Phòng ban"
                lstPhongBan.ItemsSource = _db.PhongBans.ToList();

                // 4. Nạp dữ liệu Tab "Điều chỉnh tài khoản"
                ItemsTaiKhoanHienTai.ItemsSource = _db.TaiKhoans.ToList();

                // 5. Nạp dữ liệu Tab "Phê duyệt Nghỉ phép"
                // Sử dụng Include để lấy thông tin nhân viên đi kèm đơn nghỉ phép
                ItemsNghiPhep.ItemsSource = _db.NghiPhep
                    .Include(np => np.NhanVien)
                    .Where(np => np.TrangThai == "Chờ duyệt")
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message);
            }
        }

        #region Logic Phê duyệt tài khoản mới
        private void CmbPhongBan_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox combo)
                combo.ItemsSource = _db.PhongBans.ToList();
        }

        private void CmbVaiTro_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox combo)
                combo.ItemsSource = _db.VaiTros.ToList();
        }

        private void BtnDuyet_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var taiKhoan = btn.Tag as TaiKhoan;
            var grid = (btn.Parent as StackPanel).Parent as Grid;

            var cmbPB = grid.Children.OfType<ComboBox>().FirstOrDefault(c => c.Name == "CmbPhongBan");
            var cmbVT = grid.Children.OfType<ComboBox>().FirstOrDefault(c => c.Name == "CmbVaiTro");

            if (cmbPB?.SelectedValue == null || cmbVT?.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Phòng ban và Vai trò!");
                return;
            }

            try
            {
                var user = _db.TaiKhoans.Find(taiKhoan.MaTaiKhoan);
                if (user != null) user.TrangThai = 1; // Chuyển sang hoạt động

                var nv = _db.NhanViens.FirstOrDefault(n => n.MaTaiKhoan == taiKhoan.MaTaiKhoan);
                if (nv != null) nv.MaPhongBan = (int)cmbPB.SelectedValue;

                _db.TaiKhoanVaiTros.Add(new TaiKhoanVaiTro { MaTaiKhoan = taiKhoan.MaTaiKhoan, MaVaiTro = (int)cmbVT.SelectedValue });

                _db.SaveChanges();
                MessageBox.Show("Phê duyệt tài khoản thành công!");
                RefreshData();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnTuChoi_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Yêu cầu đã được bỏ qua.");
        }
        #endregion

        #region Logic Quản lý Phòng Ban
        private void BtnThemPB_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenPB.Text)) return;
            try
            {
                _db.PhongBans.Add(new PhongBan { TenPhongBan = txtTenPB.Text });
                _db.SaveChanges();
                txtTenPB.Clear();
                RefreshData();
                MessageBox.Show("Đã thêm phòng ban mới!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnXoaPB_Click(object sender, RoutedEventArgs e)
        {
            if (lstPhongBan.SelectedItem is PhongBan pb)
            {
                if (_db.NhanViens.Any(nv => nv.MaPhongBan == pb.MaPhongBan))
                {
                    MessageBox.Show("Không thể xóa phòng ban đang có nhân viên!");
                    return;
                }
                _db.PhongBans.Remove(pb);
                _db.SaveChanges();
                RefreshData();
                MessageBox.Show("Đã xóa phòng ban!");
            }
        }
        #endregion

        #region Logic Điều chỉnh & Chặn tài khoản
        private void CmbPB_Update_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox combo)
                combo.ItemsSource = _db.PhongBans.ToList();
        }

        private void CmbVT_Update_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox combo)
                combo.ItemsSource = _db.VaiTros.ToList();
        }

        private void BtnSaveAccountUpdate_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tk = btn.Tag as TaiKhoan;
            var grid = btn.Parent as Grid;

            var cmbPB = grid.Children.OfType<ComboBox>().FirstOrDefault(c => c.Name == "CmbUpdatePB");
            var cmbVT = grid.Children.OfType<ComboBox>().FirstOrDefault(c => c.Name == "CmbUpdateVT");

            if (cmbPB?.SelectedValue == null || cmbVT?.SelectedValue == null) return;

            try
            {
                var nv = _db.NhanViens.FirstOrDefault(n => n.MaTaiKhoan == tk.MaTaiKhoan);
                if (nv != null) nv.MaPhongBan = (int)cmbPB.SelectedValue;

                var oldRoles = _db.TaiKhoanVaiTros.Where(r => r.MaTaiKhoan == tk.MaTaiKhoan);
                _db.TaiKhoanVaiTros.RemoveRange(oldRoles);

                _db.TaiKhoanVaiTros.Add(new TaiKhoanVaiTro { MaTaiKhoan = tk.MaTaiKhoan, MaVaiTro = (int)cmbVT.SelectedValue });

                _db.SaveChanges();
                MessageBox.Show("Đã cập nhật thông tin tài khoản!");
                RefreshData();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var tk = btn.Tag as TaiKhoan;

            if (tk == null) return;

            try
            {
                var userInDb = _db.TaiKhoans.Find(tk.MaTaiKhoan);
                if (userInDb != null)
                {
                    userInDb.TrangThai = (userInDb.TrangThai == 1) ? 0 : 1;
                    _db.SaveChanges();

                    string msg = (userInDb.TrangThai == 0) ? "Đã chặn tài khoản!" : "Đã mở khóa tài khoản!";
                    MessageBox.Show(msg, "Thông báo");
                    RefreshData();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        #endregion

        #region Logic Phê duyệt Nghỉ phép

        // Mở ảnh/file minh chứng từ đường dẫn máy tính
        private void BtnXemMinhChung_Click(object sender, RoutedEventArgs e)
        {
            var path = (sender as Button).Tag?.ToString();
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                MessageBox.Show("Không tìm thấy file minh chứng tại: " + path);
                return;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Lỗi mở file: " + ex.Message); }
        }

        // Chấp nhận đơn nghỉ phép
        private void BtnDuyetNghiPhep_Click(object sender, RoutedEventArgs e)
        {
            var don = (sender as Button).Tag as NghiPhep;
            if (don == null) return;
            try
            {
                var np = _db.NghiPhep.Find(don.Id);
                if (np != null)
                {
                    np.TrangThai = "Đã chấp nhận";
                    _db.SaveChanges();
                    MessageBox.Show("Đã chấp nhận đơn nghỉ phép.");
                    RefreshData();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        // Từ chối đơn nghỉ phép
        private void BtnTuChoiNghiPhep_Click(object sender, RoutedEventArgs e)
        {
            var don = (sender as Button).Tag as NghiPhep;
            if (don == null) return;
            try
            {
                var np = _db.NghiPhep.Find(don.Id);
                if (np != null)
                {
                    np.TrangThai = "Từ chối";
                    _db.SaveChanges();
                    MessageBox.Show("Đã từ chối đơn nghỉ phép.");
                    RefreshData();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        #endregion
    }
}
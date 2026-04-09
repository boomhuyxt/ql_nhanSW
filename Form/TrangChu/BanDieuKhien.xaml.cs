using ql_nhanSW.Models;
using ql_nhanSW.share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.IO.IsolatedStorage;

namespace ql_nhanSW.Form.TrangChu
{
    public class ChartData
    {
        public string Label { get; set; }
        public double ColumnHeight { get; set; }
        public string ValueDisplay { get; set; }
        public string FullInfo { get; set; }
    }

    public partial class BanDieuKhien : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private string duongDanMinhChung = "";

        public BanDieuKhien()
        {
            InitializeComponent();
            LoadPersonalDashboard();

            // Mỗi khi form được load, kiểm tra xem người dùng đã cập nhật thông tin ở form kia chưa
            ShowHuongDanAlert();
        }

        // ==================== LOGIC HIỂN THỊ THÔNG BÁO ====================
        private void ShowHuongDanAlert()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                // Nếu file này tồn tại (do form CapNhatThongTin tạo ra), ẩn thông báo ngay
                if (store.FileExists("DaCapNhatThongTin.txt"))
                {
                    alertHuongDan.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            // Nếu chưa có file đánh dấu, hiện thông báo hướng dẫn
            alertHuongDan.Visibility = Visibility.Visible;
        }

        private void LoadPersonalDashboard()
        {
            try
            {
                var currentUser = SessionManager.CurrentUser;
                if (currentUser == null) return;

                var nhanVien = _db.NhanViens.FirstOrDefault(nv => nv.MaTaiKhoan == currentUser.MaTaiKhoan);
                if (nhanVien != null)
                {
                    // 1. Hiển thị phòng ban
                    var phongBan = _db.PhongBans.Find(nhanVien.MaPhongBan);
                    txtPhongBan.Text = phongBan?.TenPhongBan ?? "Chưa phân phòng";

                    // 2. Lấy danh sách lương
                    var luongList = _db.Luongs
                        .Where(l => l.MaNhanVien == nhanVien.MaNhanVien)
                        .OrderByDescending(l => l.Nam).ThenByDescending(l => l.Thang)
                        .ToList();

                    var luongMoiNhat = luongList.FirstOrDefault();
                    txtLuong.Text = luongMoiNhat != null ? luongMoiNhat.TongLuong.ToString("N0") : "0";

                    // 3. Vẽ biểu đồ 6 tháng
                    var dataChart = luongList.Take(6).OrderBy(l => l.Nam).ThenBy(l => l.Thang).ToList();
                    DrawChart(dataChart);

                    // 4. Trạng thái nghỉ phép gần đây
                    var listNghiPhep = _db.NghiPhep
                        .Where(np => np.NhanVienId == nhanVien.MaNhanVien)
                        .OrderByDescending(np => np.NgayBatDau)
                        .Take(5)
                        .ToList();

                    ItemsTrangThaiNghiPhep.ItemsSource = listNghiPhep;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin bảng điều khiển: " + ex.Message);
            }
        }

        private void DrawChart(List<Luong> list)
        {
            if (list == null || list.Count == 0) return;

            decimal maxVal = list.Max(l => l.TongLuong);
            if (maxVal == 0) maxVal = 1;

            ChartLuong.ItemsSource = list.Select(l => new ChartData
            {
                Label = $"T{l.Thang}",
                ColumnHeight = (double)(l.TongLuong / maxVal) * 100,
                ValueDisplay = (l.TongLuong / 1000000).ToString("N1") + "tr",
                FullInfo = $"Tháng {l.Thang}/{l.Nam}: {l.TongLuong:N0} VND"
            }).ToList();
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Tài liệu (*.docx;*.pdf;*.png;*.jpg)|*.docx;*.pdf;*.png;*.jpg";

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    string thuMucLuu = @"C:\MinhChungNghiPhepNhanSu";
                    if (!Directory.Exists(thuMucLuu)) Directory.CreateDirectory(thuMucLuu);

                    string fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(openFile.FileName);
                    duongDanMinhChung = Path.Combine(thuMucLuu, fileName);

                    File.Copy(openFile.FileName, duongDanMinhChung);
                    txtFilePath.Text = Path.GetFileName(openFile.FileName);
                    MessageBox.Show("Đã tải lên minh chứng thành công!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lưu file minh chứng: " + ex.Message);
                }
            }
        }

        private void BtnGuiDon_Click(object sender, RoutedEventArgs e)
        {
            if (dpStart.SelectedDate == null || dpEnd.SelectedDate == null || cbLoaiNghi.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin ngày nghỉ và loại nghỉ!", "Thông báo");
                return;
            }

            try
            {
                var currentUser = SessionManager.CurrentUser;
                if (currentUser == null) return;

                var nhanVien = _db.NhanViens.FirstOrDefault(nv => nv.MaTaiKhoan == currentUser.MaTaiKhoan);
                if (nhanVien == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin nhân viên liên kết!", "Lỗi");
                    return;
                }

                NghiPhep donMoi = new NghiPhep
                {
                    NhanVienId = nhanVien.MaNhanVien,
                    NgayBatDau = dpStart.SelectedDate.Value,
                    NgayKetThuc = dpEnd.SelectedDate.Value,
                    LoaiNghi = (cbLoaiNghi.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    LyDo = duongDanMinhChung,
                    TrangThai = "Chờ duyệt"
                };

                _db.NghiPhep.Add(donMoi);
                _db.SaveChanges();

                MessageBox.Show("Gửi đơn thành công! Đang chờ quản trị viên phê duyệt.", "Thành công");

                // Cập nhật lại danh sách hiển thị ở Dashboard
                LoadPersonalDashboard();
                txtFilePath.Clear();
                duongDanMinhChung = "";
                dpStart.SelectedDate = null;
                dpEnd.SelectedDate = null;
                cbLoaiNghi.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống khi gửi đơn: " + ex.Message, "Lỗi");
            }
        }
    }
}
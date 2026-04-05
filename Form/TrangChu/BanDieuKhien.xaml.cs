using ql_nhanSW.Models;
using ql_nhanSW.share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace ql_nhanSW.Form.TrangChu
{
    // Class định nghĩa dữ liệu cho cột biểu đồ
    public class ChartData
    {
        public string Label { get; set; }        // Ví dụ: T10
        public double ColumnHeight { get; set; } // Chiều cao (px)
        public string ValueDisplay { get; set; } // Ví dụ: 17.5tr
        public string FullInfo { get; set; }     // Thông tin chi tiết khi hover
    }

    public partial class BanDieuKhien : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private string duongDanMinhChung = "";

        public BanDieuKhien()
        {
            InitializeComponent();
            LoadPersonalDashboard();
        }

        // Hàm tự động tải thông tin cá nhân, biểu đồ và trạng thái nghỉ phép
        private void LoadPersonalDashboard()
        {
            try
            {
                // Lấy thông tin tài khoản hiện tại từ Session
                var currentUser = SessionManager.CurrentUser;
                if (currentUser == null) return;

                // 1. Tìm nhân viên liên kết với tài khoản này
                var nhanVien = _db.NhanViens.FirstOrDefault(nv => nv.MaTaiKhoan == currentUser.MaTaiKhoan);
                if (nhanVien != null)
                {
                    // Hiển thị tên Phòng ban
                    var phongBan = _db.PhongBans.Find(nhanVien.MaPhongBan);
                    txtPhongBan.Text = phongBan?.TenPhongBan ?? "Chưa phân phòng";

                    // 2. Lấy danh sách lương của nhân viên để hiển thị và vẽ biểu đồ
                    var luongList = _db.Luongs
                        .Where(l => l.MaNhanVien == nhanVien.MaNhanVien)
                        .OrderByDescending(l => l.Nam).ThenByDescending(l => l.Thang)
                        .ToList();

                    // Hiển thị số lương tháng gần nhất
                    var luongMoiNhat = luongList.FirstOrDefault();
                    txtLuong.Text = luongMoiNhat != null ? luongMoiNhat.TongLuong.ToString("N0") : "0";

                    // 3. VẼ BIỂU ĐỒ (Lấy 6 tháng gần nhất)
                    var dataChart = luongList.Take(6).OrderBy(l => l.Nam).ThenBy(l => l.Thang).ToList();
                    DrawChart(dataChart);

                    // 4. HIỂN THỊ TRẠNG THÁI NGHỈ PHÉP (MỚI THÊM)
                    // Lấy danh sách các đơn nghỉ phép của chính nhân viên này
                    var listNghiPhep = _db.NghiPhep
                        .Where(np => np.NhanVienId == nhanVien.MaNhanVien)
                        .OrderByDescending(np => np.NgayBatDau)
                        .Take(5) // Hiển thị 5 đơn gần nhất
                        .ToList();

                    ItemsTrangThaiNghiPhep.ItemsSource = listNghiPhep;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin bảng điều khiển: " + ex.Message);
            }
        }

        // Hàm xử lý dữ liệu và nạp vào ItemsControl biểu đồ
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

        // Xử lý sự kiện Tải file minh chứng
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

        // Xử lý sự kiện Gửi đơn nghỉ phép
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

                // Tạo đối tượng nghỉ phép mới
                NghiPhep donMoi = new NghiPhep
                {
                    NhanVienId = nhanVien.MaNhanVien,
                    NgayBatDau = dpStart.SelectedDate.Value,
                    NgayKetThuc = dpEnd.SelectedDate.Value,
                    LoaiNghi = (cbLoaiNghi.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    LyDo = duongDanMinhChung,
                    TrangThai = "Chờ duyệt"
                };

                // Lưu vào database (Sử dụng NghiPheps theo cấu trúc chuẩn)
                _db.NghiPhep.Add(donMoi);
                _db.SaveChanges();

                MessageBox.Show("Gửi đơn thành công! Đang chờ quản trị viên phê duyệt.", "Thành công");

                // Làm mới lại giao diện để hiển thị đơn vừa gửi trong danh sách trạng thái
                LoadPersonalDashboard();

                // Reset form nhập liệu
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
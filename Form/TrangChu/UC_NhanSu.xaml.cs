using Emgu.CV;
using ql_nhanSW.BUS;
using ql_nhanSW.Models;
using ql_nhanSW.share;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_NhanSu : UserControl
    {
        private FaceID? _faceBus;
        private DispatcherTimer? _timer;
        private List<EmployeeFaceData> _employeeList = new();
        private NhanVien? _currentEmployee;

        public UC_NhanSu()
        {
            InitializeComponent();
            LoadEmployeeData();
            InitializeCamera();
            LoadCurrentUserInfo();
            LoadLichSuToday();
        }

        // =================================================================
        // 1. Load thông tin người đang đăng nhập
        // =================================================================
        private void LoadCurrentUserInfo()
        {
            if (SessionManager.CurrentUser == null) return;

            var tk = SessionManager.CurrentUser;

            using var db = new AppDbContext();
            var nv = db.NhanViens.FirstOrDefault(n => n.MaTaiKhoan == tk.MaTaiKhoan);

            _currentEmployee = nv;

            txtHoTen.Text = nv?.HoTen ?? tk.TenDangNhap ?? "Người dùng";
            txtPhongBan.Text = "Nhân viên";

            if (!string.IsNullOrEmpty(tk.AnhDaiDien) && File.Exists(tk.AnhDaiDien))
            {
                try
                {
                    imgUserAvatar.Source = new BitmapImage(new Uri(tk.AnhDaiDien));
                }
                catch { }
            }
        }

        // =================================================================
        // 2. Load danh sách nhân viên để so sánh FaceID
        // =================================================================
        private void LoadEmployeeData()
        {
            using var db = new AppDbContext();
            var rawData = db.NhanViens
                .Join(db.TaiKhoans,
                    nv => nv.MaTaiKhoan,
                    tk => tk.MaTaiKhoan,
                    (nv, tk) => new EmployeeFaceData
                    {
                        NhanVien = nv,
                        AvatarPath = tk.AnhDaiDien
                    })
                .Where(x => !string.IsNullOrEmpty(x.AvatarPath))
                .ToList();

            _employeeList = rawData
                .Where(x => File.Exists(x.AvatarPath))
                .ToList();
        }

        private class EmployeeFaceData
        {
            public NhanVien NhanVien { get; set; } = null!;
            public string AvatarPath { get; set; } = string.Empty;
        }

        // =================================================================
        // 3. Camera
        // =================================================================
        private void InitializeCamera()
        {
            try
            {
                _faceBus = new FaceID();
                _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo Camera: " + ex.Message);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_faceBus == null) return;
            using var frame = _faceBus.ScanFace();
            if (frame != null && !frame.IsEmpty)
                imgCamera.Source = MatToBitmapSource(frame);
        }

        // =================================================================
        // 4. Mat → BitmapSource
        // =================================================================
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource? MatToBitmapSource(Mat mat)
        {
            if (mat.IsEmpty) return null;
            using var bitmap = mat.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(hBitmap); }
        }

        // =================================================================
        // 5. BUTTON CHẤM CÔNG
        // =================================================================
        private void BtnChamCong_Click(object sender, RoutedEventArgs e)
        {
            if (_faceBus == null || _employeeList.Count == 0)
            {
                MessageBox.Show("Hệ thống chưa có dữ liệu nhân viên!", "Cảnh báo");
                return;
            }

            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"temp_face_{DateTime.Now.Ticks}.jpg");
                using var frame = _faceBus.GetCameraFrame();
                _faceBus.SaveFaceImage(frame, tempPath);

                float[] inputVector = _faceBus.GetFaceVector(tempPath);
                if (inputVector == null)
                {
                    MessageBox.Show("Không phát hiện khuôn mặt! Vui lòng thử lại.", "Cảnh báo");
                    return;
                }

                EmployeeFaceData? matched = null;
                foreach (var emp in _employeeList)
                {
                    float[] dbVector = _faceBus.GetFaceVector(emp.AvatarPath);
                    if (dbVector != null && _faceBus.IsSamePerson(inputVector, dbVector))
                    {
                        matched = emp;
                        break;
                    }
                }

                if (matched == null)
                {
                    MessageBox.Show("Không nhận diện được nhân viên!", "Thông báo");
                    return;
                }

                _currentEmployee = matched.NhanVien;

                using var db = new AppDbContext();
                var today = DateTime.Today;
                var record = db.ChamCongs.FirstOrDefault(c =>
                    c.MaNhanVien == matched.NhanVien.MaNhanVien && c.NgayLamViec == today);

                if (record == null)
                {
                    db.ChamCongs.Add(new ChamCong
                    {
                        MaNhanVien = matched.NhanVien.MaNhanVien,
                        NgayLamViec = today,
                        GioVao = DateTime.Now.TimeOfDay
                    });
                    db.SaveChanges();
                    MessageBox.Show($"✅ Vào ca thành công!\nChào {matched.NhanVien.HoTen}");
                    UpdateButtonState("Tan ca", "#10B981");
                }
                else if (record.GioRa == null)
                {
                    record.GioRa = DateTime.Now.TimeOfDay;
                    db.SaveChanges();
                    MessageBox.Show($"✅ Tan ca thành công!\nTạm biệt {matched.NhanVien.HoTen}");
                    UpdateButtonState("Đã hoàn thành", "#64748B");
                }
                else
                {
                    MessageBox.Show($"{matched.NhanVien.HoTen} đã chấm công hoàn tất hôm nay!");
                }

                UpdateUIAfterRecognition(matched.NhanVien);
                LoadLichSuToday();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void UpdateUIAfterRecognition(NhanVien nv)
        {
            txtHoTen.Text = nv.HoTen ?? "Nhân viên";
            txtPhongBan.Text = "Nhân viên";
        }

        // =================================================================
        // 6. Load lịch sử chấm công
        // =================================================================
        private void LoadLichSuToday()
        {
            spLichSu.Children.Clear();

            using var db = new AppDbContext();
            var today = DateTime.Today;

            var history = db.ChamCongs
                .Where(c => c.NgayLamViec == today)
                .OrderByDescending(c => c.GioVao)
                .Take(5)
                .ToList();

            foreach (var item in history)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Avatar nhỏ
                var border = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(14),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F3FF"))
                };
                var tb = new TextBlock
                {
                    Text = "NV",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED")),
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                border.Child = tb;
                Grid.SetColumn(border, 0);
                grid.Children.Add(border);

                // Thông tin
                var stack = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
                stack.Children.Add(new TextBlock
                {
                    Text = $"Nhân viên {item.MaNhanVien}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1B4B")),
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold
                });
                stack.Children.Add(new TextBlock
                {
                    Text = item.GioRa.HasValue ? "Tan ca" : "Vào ca",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A")),
                    FontSize = 10
                });
                Grid.SetColumn(stack, 1);
                grid.Children.Add(stack);

                // Giờ
                var time = new TextBlock
                {
                    Text = item.GioVao?.ToString(@"hh\:mm") ?? "",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B")),
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(time, 2);
                grid.Children.Add(time);

                spLichSu.Children.Add(grid);
            }

            if (history.Count == 0)
            {
                spLichSu.Children.Add(new TextBlock
                {
                    Text = "Chưa có chấm công nào hôm nay",
                    Foreground = Brushes.Gray,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }
        }

        private void UpdateButtonState(string text, string hexColor)
        {
            if (btnChamCong.Template.FindName("txtButtonText", btnChamCong) is TextBlock txt)
                txt.Text = text;

            btnChamCong.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            _faceBus = null;
        }
    }
}
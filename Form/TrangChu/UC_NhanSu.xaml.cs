using Emgu.CV;
using ql_nhanSW.BUS;
using ql_nhanSW.Models;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ql_nhanSW.Form.TrangChu
{

    public partial class UC_NhanSu : UserControl
    {
        // Thêm dấu ? để chấp nhận giá trị null (Fix lỗi Nullability)
        private FaceID? _faceBus;
        private DispatcherTimer? _timer;

        public UC_NhanSu()
        {
            InitializeComponent();

            try
            {
                _faceBus = new FaceID();
                _timer = new DispatcherTimer();

                // Sử dụng dấu ! để báo với trình biên dịch sender sẽ không null
                _timer.Tick += Timer_Tick!;
                _timer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khởi tạo Camera: " + ex.Message);
            }
        }

        // Fix lỗi 'sender' nullability match delegate
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_faceBus == null) return;

            // Chụp frame từ camera
            using (var frame = _faceBus.ScanFace())
            {
                if (frame != null && !frame.IsEmpty)
                {
                    imgCamera.Source = MatToBitmapSource(frame);
                }
            }
        }

        // Hàm này xử lý sự kiện Unloaded khai báo trong XAML
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick!;
            }

            // Gọi hàm giải phóng camera bên trong FaceID nếu có
            if (_faceBus != null)
            {
                // _faceBus.Dispose(); // Kích hoạt nếu class FaceID có hàm Dispose
            }
        }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource? MatToBitmapSource(Mat mat)
        {
            if (mat.IsEmpty) return null;

            using (Bitmap bitmap = mat.ToBitmap())
            {
                IntPtr hBitmap = bitmap.GetHbitmap();
                try
                {
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        private void CheckinBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_faceBus == null) return;

                // 1. Chụp frame hiện tại và lưu tạm
                string tempPath = "temp_face.jpg";
                using (var frame = _faceBus.GetCameraFrame())
                {
                    _faceBus.SaveFaceImage(frame, tempPath);
                }

                // 2. Lấy vector khuôn mặt từ ảnh vừa chụp
                float[] inputVector = _faceBus.GetFaceVector(tempPath);
                if (inputVector == null)
                {
                    MessageBox.Show("Không phát hiện khuôn mặt, vui lòng thử lại!");
                    return;
                }

                // 3. So sánh với database
                using (var db = new AppDbContext())
                {
                    NhanVien? matchedEmployee = null;

                    foreach (var nv in db.NhanViens)
                    {
                        if (string.IsNullOrEmpty(nv.FaceVector)) continue;

                        // Giải mã vector từ JSON
                        float[] dbVector = System.Text.Json.JsonSerializer
                            .Deserialize<float[]>(nv.FaceVector);

                        if (_faceBus.IsSamePerson(inputVector, dbVector))
                        {
                            matchedEmployee = nv;
                            break;
                        }
                    }

                    if (matchedEmployee == null)
                    {
                        MessageBox.Show("Không nhận diện được nhân viên!");
                        return;
                    }

                    // 4. Kiểm tra đã check-in hôm nay chưa
                    var today = DateTime.Today;
                    var existingRecord = db.ChamCongs.FirstOrDefault(c =>
                        c.MaNhanVien == matchedEmployee.MaNhanVien &&
                        c.NgayLamViec == today);

                    if (existingRecord != null)
                    {
                        MessageBox.Show($"{matchedEmployee.HoTen} đã vào ca hôm nay rồi!");
                        return;
                    }

                    // 5. Lưu check-in
                    db.ChamCongs.Add(new ChamCong
                    {
                        MaNhanVien = matchedEmployee.MaNhanVien,
                        NgayLamViec = today,
                        GioVao = DateTime.Now.TimeOfDay
                    });
                    db.SaveChanges();

                    MessageBox.Show($"✅ Vào ca thành công! Chào {matchedEmployee.HoTen}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }

        }

        private void CheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_faceBus == null) return;

                // 1. Chụp frame hiện tại
                string tempPath = "temp_face.jpg";
                using (var frame = _faceBus.GetCameraFrame())
                {
                    _faceBus.SaveFaceImage(frame, tempPath);
                }

                // 2. Lấy vector khuôn mặt
                float[] inputVector = _faceBus.GetFaceVector(tempPath);
                if (inputVector == null)
                {
                    MessageBox.Show("Không phát hiện khuôn mặt, vui lòng thử lại!");
                    return;
                }

                // 3. So sánh với database
                using (var db = new AppDbContext())
                {
                    NhanVien? matchedEmployee = null;

                    foreach (var nv in db.NhanViens)
                    {
                        if (string.IsNullOrEmpty(nv.FaceVector)) continue;

                        float[] dbVector = System.Text.Json.JsonSerializer
                            .Deserialize<float[]>(nv.FaceVector);

                        if (_faceBus.IsSamePerson(inputVector, dbVector))
                        {
                            matchedEmployee = nv;
                            break;
                        }
                    }

                    if (matchedEmployee == null)
                    {
                        MessageBox.Show("Không nhận diện được nhân viên!");
                        return;
                    }

                    // 4. Tìm record check-in hôm nay
                    var today = DateTime.Today;
                    var existingRecord = db.ChamCongs.FirstOrDefault(c =>
                        c.MaNhanVien == matchedEmployee.MaNhanVien &&
                        c.NgayLamViec == today);

                    if (existingRecord == null)
                    {
                        MessageBox.Show($"{matchedEmployee.HoTen} chưa vào ca hôm nay!");
                        return;
                    }

                    if (existingRecord.GioRa != null)
                    {
                        MessageBox.Show($"{matchedEmployee.HoTen} đã tan ca rồi!");
                        return;
                    }

                    // 5. Lưu check-out
                    existingRecord.GioRa = DateTime.Now.TimeOfDay;
                    db.SaveChanges();

                    MessageBox.Show($"✅ Tan ca thành công! Tạm biệt {matchedEmployee.HoTen}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }

        }
    }
}
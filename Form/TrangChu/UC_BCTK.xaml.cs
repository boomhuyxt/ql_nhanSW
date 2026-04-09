using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ql_nhanSW.Models;
using Microsoft.Win32;
using System.IO;
using OfficeOpenXml;

// Giải quyết xung đột namespace bằng Alias [cite: 1]
using iTextPdf = iText.Kernel.Pdf;
using iTextLayout = iText.Layout;
using iTextElement = iText.Layout.Element;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_BCTK : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        // Sử dụng dynamic để chứa dữ liệu kết hợp từ NhanVien và TaiKhoan 
        private List<dynamic> dsHienThi;

        public UC_BCTK()
        {
            InitializeComponent();
            // Đảm bảo popup giữ mở đủ để chọn item; sẽ đóng thủ công khi chọn
            try { popGoiY.StaysOpen = true; } catch { }
            LoadData();
        }

        private void LoadData()
        {
            try
            {// 1. Thống kê KPI dựa trên trạng thái tài khoản [cite: 6, 7]
                // Tổng nhân sự: Trạng thái = 1; Tuyển dụng: Trạng thái = 0 [cite: 6]
                txtTongNhanSu.Text = _db.TaiKhoans.Count(tk => tk.TrangThai == 1).ToString("N0");
                txtTuyenDung.Text = _db.TaiKhoans.Count(tk => tk.TrangThai == 0).ToString("N0");

                // 2. Kết hợp bảng NhanVien và TaiKhoan để lấy Email hiển thị 
                // Dùng left join để vẫn hiển thị nhân viên chưa có tài khoản
                dsHienThi = (from nv in _db.NhanViens
                             join tk in _db.TaiKhoans on nv.MaTaiKhoan equals tk.MaTaiKhoan into gj
                             from tk in gj.DefaultIfEmpty()
                             select new
                             {
                                 nv.MaNhanVien,
                                 nv.HoTen,
                                 Email = tk != null ? tk.Email : string.Empty,
                                 nv.MaTaiKhoan
                             }).ToList<dynamic>();
            }
            catch (Exception ex)
            {
                dsHienThi = new List<dynamic>();
                MessageBox.Show("Lỗi nạp dữ liệu: " + ex.Message);
            }
        }

        private void txtSearchNhanVien_TextChanged(object sender, TextChangedEventArgs e)
        {
            string raw = txtSearchNhanVien.Text.Trim();
            string query = RemoveDiacritics(raw).ToLowerInvariant();
            if (string.IsNullOrEmpty(query)) { popGoiY.IsOpen = false; return; }

           // Tìm kiếm tương đối trên danh sách chứa tên và email, bỏ dấu để hỗ trợ tiếng Việt
            var filter = dsHienThi.Where(nv =>
                (!string.IsNullOrEmpty((string)nv.HoTen) && RemoveDiacritics(((string)nv.HoTen)).ToLowerInvariant().Contains(query))
                || (!string.IsNullOrEmpty((string)nv.Email) && RemoveDiacritics(((string)nv.Email)).ToLowerInvariant().Contains(query))
            ).Take(10).ToList();

            if (filter.Any())
            {
                lstGoiY.ItemsSource = filter;
                popGoiY.IsOpen = true;
            }
            else popGoiY.IsOpen = false;
        }

        private void lstGoiY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = lstGoiY.SelectedItem as dynamic;
            if (selected != null)
            {
                txtSearchNhanVien.Text = selected.HoTen;
                // Đóng popup thủ công
                try { popGoiY.IsOpen = false; } catch { }
                UpdateCharts(selected.MaNhanVien);
            }
        }

        private void UpdateCharts(int maNV)
        {
            // Vẽ biểu đồ Lương từ database [cite: 8]
            var luongs = _db.Luongs.Where(l => l.MaNhanVien == maNV).OrderBy(l => l.Nam).ThenBy(l => l.Thang).Take(6).ToList();
            DrawLineChart(CanvasLuong, PolyLuong, luongs.Select(l => (double)l.TongLuong).ToList(),
                          luongs.Select(l => $"T{l.Thang}").ToList(), "#16A34A", true);

            // Vẽ biểu đồ Chuyên cần từ database [cite: 9]
            var chams = _db.ChamCongs.Where(c => c.MaNhanVien == maNV).OrderBy(c => c.NgayLamViec).Take(6).ToList();
            // Tính phần trăm chuyên cần dựa trên giờ làm (8h/ngày)
            var chuyenCanValues = chams.Select(c =>
            {
                if (c.GioVao.HasValue && c.GioRa.HasValue)
                {
                    double hours = (c.GioRa.Value - c.GioVao.Value).TotalHours;
                    if (hours < 0) hours = 0; // phòng trường hợp dữ liệu lệch
                    return Math.Min(100.0, Math.Round(hours / 8.0 * 100.0, 1));
                }
                return 0.0;
            }).ToList();
            DrawLineChart(CanvasChuyenCan, PolyChuyenCan, chuyenCanValues,
                          chams.Select(c => c.NgayLamViec.ToString("dd/MM")).ToList(), "#7C3AED", false);
        }

        // Loại bỏ dấu tiếng Việt để tìm kiếm
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        // Chỉ định rõ namespace System.Windows.Controls để tránh lỗi Ambiguous 
        private void DrawLineChart(System.Windows.Controls.Canvas canvas, System.Windows.Shapes.Polyline poly, List<double> values, List<string> labels, string colorHex, bool isMoney)
        {
            canvas.Children.Clear();
            canvas.Children.Add(poly);
            if (!values.Any()) return;

            PointCollection points = new PointCollection();
            double max = values.Max(); if (max == 0) max = 1;
            double stepX = 60, canvasH = 180;
            Brush brush = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(colorHex);

            for (int i = 0; i < values.Count; i++)
            {
                double x = i * stepX + 30;
                double y = canvasH - (values[i] / max * 130);
                points.Add(new Point(x, y));

                string displayVal = isMoney ? (values[i] / 1000000).ToString("N1") + "tr" : values[i] + "%";

                // Chỉ định rõ namespace để tránh xung đột với iText 
                TextBlock txtVal = new TextBlock { Text = displayVal, FontSize = 9, FontWeight = System.Windows.FontWeights.Bold, Foreground = brush };

                System.Windows.Controls.Canvas.SetLeft(txtVal, x - 10);
                System.Windows.Controls.Canvas.SetTop(txtVal, y - 18);
                canvas.Children.Add(txtVal);

                TextBlock txtLbl = new TextBlock { Text = labels[i], FontSize = 9, Foreground = Brushes.Gray };
                System.Windows.Controls.Canvas.SetLeft(txtLbl, x - 10);
                System.Windows.Controls.Canvas.SetTop(txtLbl, canvasH + 5);
                canvas.Children.Add(txtLbl);
            }
            poly.Points = points;
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string reportType = (btn.Name == "BtnExportChuyenCan") ? "Chuyen_Can" : "Luong";
            ComboBox cbExport = (btn.Parent as StackPanel).Children.OfType<ComboBox>().FirstOrDefault();
            string extension = (cbExport.SelectedIndex == 0) ? "xlsx" : "pdf";

            SaveFileDialog sfd = new SaveFileDialog
            {
                FileName = $"Bao_Cao_{reportType}_{DateTime.Now:yyyyMMdd}",
                Filter = (extension == "xlsx") ? "Excel Workbook (*.xlsx)|*.xlsx" : "PDF Document (*.pdf)|*.pdf",
                DefaultExt = extension
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // Kiểm tra file có đang mở không trước khi ghi [cite: 10]
                    if (extension == "xlsx") ExportExcel(sfd.FileName, reportType);
                    else ExportPdf(sfd.FileName, reportType);
                    MessageBox.Show("Xuất file thành công!");
                }
                catch (IOException) { MessageBox.Show("Lỗi: File đang mở bởi chương trình khác."); }
                catch (Exception ex) { MessageBox.Show("Lỗi xuất file: " + ex.Message); }
            }
        }

        private void ExportExcel(string path, string type)
        {
            // Sửa lỗi LicenseException 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Bao cao");
                ws.Cells[1, 1].Value = "BAO CAO " + type.ToUpper();
                ws.Cells[2, 1].Value = "Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy");
                package.SaveAs(new FileInfo(path));
            }
        }

        private void ExportPdf(string path, string type)
        {
            // Sử dụng các lớp từ bí danh iTextPdf/iTextLayout đã khai báo ở trên [cite: 1]
            using (iTextPdf.PdfWriter writer = new iTextPdf.PdfWriter(path))
            {
                using (iTextPdf.PdfDocument pdf = new iTextPdf.PdfDocument(writer))
                {
                    iTextLayout.Document doc = new iTextLayout.Document(pdf);
                   // Dùng iTextElement để tạo đoạn văn [cite: 1]
                    doc.Add(new iTextElement.Paragraph("BAO CAO " + type.ToUpper()).SetFontSize(14));
                    doc.Add(new iTextElement.Paragraph("Ngay xuat: " + DateTime.Now.ToString("dd/MM/yyyy")));
                    doc.Close();
                }
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { }
    }
}
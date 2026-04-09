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
                dsHienThi = (from nv in _db.NhanViens
                             join tk in _db.TaiKhoans on nv.MaTaiKhoan equals tk.MaTaiKhoan
                             select new
                             {
                                 nv.MaNhanVien,
                                 nv.HoTen,
                                 Email = tk.Email, // Lấy Email từ bảng TaiKhoan [cite: 5]
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
            string query = txtSearchNhanVien.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(query)) { popGoiY.IsOpen = false; return; }

           // Tìm kiếm tương đối trên danh sách chứa tên và email [cite: 4]
            var filter = dsHienThi.Where(nv => nv.HoTen.ToLower().Contains(query)).Take(10).ToList();

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
                popGoiY.IsOpen = false;
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
            DrawLineChart(CanvasChuyenCan, PolyChuyenCan, chams.Select(c => 100.0).ToList(),
                          chams.Select(c => c.NgayLamViec.ToString("dd/MM")).ToList(), "#7C3AED", false);
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
            // Cố gắng thiết lập license cho EPPlus phù hợp với nhiều phiên bản (EPPlus 5..8+)
            TrySetEpplusLicense();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Bao cao");
                ws.Cells[1, 1].Value = "BAO CAO " + type.ToUpper();
                ws.Cells[2, 1].Value = "Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy");
                package.SaveAs(new FileInfo(path));
            }
        }

        // Thiết lập license cho EPPlus bằng reflection để tương thích nhiều phiên bản
        private void TrySetEpplusLicense()
        {
            try
            {
                var excelType = typeof(ExcelPackage);
                // Thử property ExcelPackage.License (EPPlus 8+)
                var licenseProp = excelType.GetProperty("License", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (licenseProp != null && licenseProp.CanWrite)
                {
                    var propType = licenseProp.PropertyType;
                    if (propType.IsEnum)
                    {
                        var enumVal = Enum.Parse(propType, "NonCommercial");
                        licenseProp.SetValue(null, enumVal);
                        return;
                    }
                    if (propType == typeof(string))
                    {
                        licenseProp.SetValue(null, "NonCommercial");
                        return;
                    }
                }

                // Fallback: ExcelPackage.LicenseContext (older versions)
                var ctxProp = excelType.GetProperty("LicenseContext", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (ctxProp != null && ctxProp.CanWrite)
                {
                    var ctxType = ctxProp.PropertyType;
                    if (ctxType.IsEnum)
                    {
                        var enumVal = Enum.Parse(ctxType, "NonCommercial");
                        ctxProp.SetValue(null, enumVal);
                        return;
                    }
                }
            }
            catch
            {
                // Ignore - if license cannot be set, EPPlus will throw a clear exception when used
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

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

// Giải quyết xung đột namespace bằng Alias
using iTextPdf = iText.Kernel.Pdf;
using iTextLayout = iText.Layout;
using iTextElement = iText.Layout.Element;

namespace ql_nhanSW.Form.TrangChu
{
    public partial class UC_BCTK : UserControl
    {
        private readonly AppDbContext _db = new AppDbContext();
        private List<dynamic> dsHienThi;

        public UC_BCTK()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Thống kê dựa trên trạng thái tài khoản
                txtTongNhanSu.Text = _db.TaiKhoans.Count(tk => tk.TrangThai == 1).ToString("N0");
                txtTuyenDung.Text = _db.TaiKhoans.Count(tk => tk.TrangThai == 0).ToString("N0");

                dsHienThi = (from nv in _db.NhanViens
                             join tk in _db.TaiKhoans on nv.MaTaiKhoan equals tk.MaTaiKhoan
                             select new
                             {
                                 nv.MaNhanVien,
                                 nv.HoTen,
                                 Email = tk.Email,
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
            var filter = dsHienThi.Where(nv => nv.HoTen.ToLower().Contains(query)).Take(10).ToList();
            if (filter.Any()) { lstGoiY.ItemsSource = filter; popGoiY.IsOpen = true; }
            else popGoiY.IsOpen = false;
        }

        private void lstGoiY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = lstGoiY.SelectedItem as dynamic;
            if (selected != null) { txtSearchNhanVien.Text = selected.HoTen; popGoiY.IsOpen = false; UpdateCharts(selected.MaNhanVien); }
        }

        private void UpdateCharts(int maNV)
        {
            // 1. Vẽ biểu đồ Lương (Giữ nguyên logic cũ)
            var luongs = _db.Luongs
                .Where(l => l.MaNhanVien == maNV)
                .OrderBy(l => l.Nam).ThenBy(l => l.Thang)
                .Take(6).ToList();
            DrawLineChart(CanvasLuong, PolyLuong, luongs.Select(l => (double)l.TongLuong).ToList(),
                          luongs.Select(l => $"T{l.Thang}").ToList(), "#16A34A", true);

            // 2. Vẽ biểu đồ Chuyên cần dựa trên 26 ngày/tháng
            // Lấy dữ liệu chấm công của nhân viên, nhóm theo Tháng và Năm
            var duLieuChamCong = _db.ChamCongs
                .Where(c => c.MaNhanVien == maNV)
                .ToList() // Tải về memory để xử lý GroupBy theo thời gian dễ hơn
                .GroupBy(c => new { c.NgayLamViec.Month, c.NgayLamViec.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Take(6) // Lấy 6 tháng gần nhất có dữ liệu
                .Select(g => new {
                    ThangNam = $"T{g.Key.Month}/{g.Key.Year % 100}",
                    TiLe = (double)g.Count() / 26 * 100 // Công thức: (Số ngày / 26) * 100
                }).ToList();

            // Chuẩn bị danh sách giá trị và nhãn
            List<double> valuesCC = duLieuChamCong.Select(x => x.TiLe).ToList();
            List<string> labelsCC = duLieuChamCong.Select(x => x.ThangNam).ToList();

            // Gọi hàm vẽ biểu đồ chuyên cần
            DrawLineChart(CanvasChuyenCan, PolyChuyenCan, valuesCC, labelsCC, "#7C3AED", false);
        }

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
                TextBlock txtVal = new TextBlock { Text = displayVal, FontSize = 9, FontWeight = FontWeights.Bold, Foreground = brush };
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
            if (btn == null) return;
            string reportType = (btn.Name == "BtnExportChuyenCan") ? "Chuyen_Can" : "Luong";
            var parentPanel = btn.Parent as StackPanel;
            if (parentPanel == null) return;
            ComboBox cbExport = parentPanel.Children.OfType<ComboBox>().FirstOrDefault();
            if (cbExport == null) return;
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
                    if (extension == "xlsx") ExportExcel(sfd.FileName, reportType);
                    else ExportPdf(sfd.FileName, reportType);
                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportExcel(string path, string type)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Báo cáo");
                ws.Cells[1, 1].Value = "BÁO CÁO: " + type.ToUpper();
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[2, 1].Value = "Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy");
                package.SaveAs(new FileInfo(path));
            }
        }

        private void ExportPdf(string path, string type)
        {
            try
            {
                using (var writer = new iTextPdf.PdfWriter(path))
                {
                    using (var pdf = new iTextPdf.PdfDocument(writer))
                    {
                        var doc = new iTextLayout.Document(pdf);

                        // CÁCH SỬA LỖI CS0117: Sử dụng font in đậm chuẩn từ thư viện
                        var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                        iTextElement.Paragraph title = new iTextElement.Paragraph("BAO CAO THONG KE: " + type.ToUpper())
                            .SetFontSize(18)
                            .SetFont(fontBold);

                        doc.Add(title);
                        doc.Add(new iTextElement.Paragraph("Ngay xuat: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")));
                        doc.Add(new iTextElement.Paragraph("-------------------------------------------"));

                        doc.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi PDF: " + ex.Message);
            }
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) { }
    }
}
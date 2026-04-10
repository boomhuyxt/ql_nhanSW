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
            var luongs = _db.Luongs
                .Where(l => l.MaNhanVien == maNV)
                .OrderBy(l => l.Nam).ThenBy(l => l.Thang)
                .Take(6).ToList();
            DrawLineChart(CanvasLuong, PolyLuong, luongs.Select(l => (double)l.TongLuong).ToList(),
                          luongs.Select(l => $"T{l.Thang}").ToList(), "#16A34A", true);

            var duLieuChamCong = _db.ChamCongs
                .Where(c => c.MaNhanVien == maNV)
                .ToList()
                .GroupBy(c => new { c.NgayLamViec.Month, c.NgayLamViec.Year })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Take(6)
                .Select(g => new {
                    ThangNam = $"T{g.Key.Month}/{g.Key.Year % 100}",
                    TiLe = (double)g.Count() / 26 * 100
                }).ToList();

            List<double> valuesCC = duLieuChamCong.Select(x => x.TiLe).ToList();
            List<string> labelsCC = duLieuChamCong.Select(x => x.ThangNam).ToList();

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
                string displayVal = isMoney ? (values[i] / 1000000).ToString("N1") + "tr" : values[i].ToString("N1") + "%";
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
                int row = 4;

                if (type == "Luong")
                {
                    ws.Cells[1, 1].Value = "BÁO CÁO CHI TRẢ LƯƠNG NHÂN VIÊN";
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[2, 1].Value = "Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                    string[] headers = { "Họ Tên", "Email", "Giới tính", "Lương CB", "Thưởng", "Tổng Lương" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        ws.Cells[row, i + 1].Value = headers[i];
                        ws.Cells[row, i + 1].Style.Font.Bold = true;
                        ws.Cells[row, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    var data = (from l in _db.Luongs
                                join nv in _db.NhanViens on l.MaNhanVien equals nv.MaNhanVien
                                join tk in _db.TaiKhoans on nv.MaTaiKhoan equals tk.MaTaiKhoan
                                select new { nv.HoTen, tk.Email, nv.GioiTinh, l.LuongCoBan, l.Thuong, l.TongLuong }).ToList();

                    foreach (var item in data)
                    {
                        row++;
                        ws.Cells[row, 1].Value = item.HoTen;
                        ws.Cells[row, 2].Value = item.Email;
                        ws.Cells[row, 3].Value = item.GioiTinh;
                        ws.Cells[row, 4].Value = item.LuongCoBan;
                        ws.Cells[row, 5].Value = item.Thuong;
                        ws.Cells[row, 6].Value = item.TongLuong;
                        ws.Cells[row, 4, row, 6].Style.Numberformat.Format = "#,##0";
                    }
                }
                else
                {
                    ws.Cells[1, 1].Value = "BÁO CÁO CHI TIẾT CHUYÊN CẦN";
                    ws.Cells[1, 1].Style.Font.Bold = true;

                    string[] headers = { "Mã NV", "Ngày Làm", "Giờ Vào", "Giờ Ra", "Tỷ lệ tháng (%)" };
                    for (int i = 0; i < headers.Length; i++) ws.Cells[row, i + 1].Value = headers[i];

                    var data = _db.ChamCongs.OrderByDescending(c => c.NgayLamViec).ToList();
                    foreach (var item in data)
                    {
                        row++;
                        ws.Cells[row, 1].Value = item.MaNhanVien;
                        ws.Cells[row, 2].Value = item.NgayLamViec.ToString("dd/MM/yyyy");
                        ws.Cells[row, 3].Value = item.GioVao?.ToString(@"hh\:mm") ?? "--:--";
                        ws.Cells[row, 4].Value = item.GioRa?.ToString(@"hh\:mm") ?? "--:--";

                        var countInMonth = _db.ChamCongs.Count(c => c.MaNhanVien == item.MaNhanVien && c.NgayLamViec.Month == item.NgayLamViec.Month);
                        ws.Cells[row, 5].Value = Math.Round(((double)countInMonth / 26) * 100, 1) + "%";
                    }
                }

                ws.Cells.AutoFitColumns();
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
                        var fontBold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

                        doc.Add(new iTextElement.Paragraph("BAO CAO " + type.ToUpper())
                            .SetFontSize(18).SetFont(fontBold));
                        doc.Add(new iTextElement.Paragraph("Ngay xuat: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")));

                        if (type == "Luong")
                        {
                            iTextLayout.Element.Table table = new iTextLayout.Element.Table(5).UseAllAvailableWidth();
                            table.AddHeaderCell("Ho Ten");
                            table.AddHeaderCell("GT");
                            table.AddHeaderCell("Luong CB");
                            table.AddHeaderCell("Thuong");
                            table.AddHeaderCell("Tong");

                            var data = (from l in _db.Luongs
                                        join nv in _db.NhanViens on l.MaNhanVien equals nv.MaNhanVien
                                        select new { nv.HoTen, nv.GioiTinh, l.LuongCoBan, l.Thuong, l.TongLuong }).ToList();

                            foreach (var item in data)
                            {
                                table.AddCell(item.HoTen);
                                table.AddCell(item.GioiTinh);
                                table.AddCell(item.LuongCoBan.ToString("N0"));
                                table.AddCell(item.Thuong.ToString("N0"));
                                table.AddCell(item.TongLuong.ToString("N0"));
                            }
                            doc.Add(table);
                        }
                        else
                        {
                            iTextLayout.Element.Table table = new iTextLayout.Element.Table(4).UseAllAvailableWidth();
                            table.AddHeaderCell("Ma NV");
                            table.AddHeaderCell("Ngay");
                            table.AddHeaderCell("Vao");
                            table.AddHeaderCell("Ra");

                            var data = _db.ChamCongs.OrderByDescending(c => c.NgayLamViec).Take(50).ToList();
                            foreach (var item in data)
                            {
                                table.AddCell(item.MaNhanVien.ToString());
                                table.AddCell(item.NgayLamViec.ToString("dd/MM"));
                                table.AddCell(item.GioVao?.ToString(@"hh\:mm") ?? "-");
                                table.AddCell(item.GioRa?.ToString(@"hh\:mm") ?? "-");
                            }
                            doc.Add(table);
                        }

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
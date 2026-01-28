using System;
using System.Collections.Generic;
using System.Text;

namespace ql_nhanSW.BUS
{
    internal class TinhLuong
    {
    }
    public class TinhLuongService
    {
        // Các hằng số bảo hiểm và thuế (ví dụ 10.5% bảo hiểm, 10% thuế)
        private const double PHI_BAO_HIEM = 0.105;
        private const double PHI_THUE = 0.1;

        public double TinhLuong(double luongCoBan, double kpi, int soNgayCong, int ngayCongChuan = 26)
        {
            // 1. Tính lương theo ngày công thực tế
            double luongTheoNgayCong = (luongCoBan / ngayCongChuan) * soNgayCong;

            // 2. Tổng thu nhập trước thuế
            double tongThuNhap = luongTheoNgayCong + kpi;

            // 3. Tính các khoản trừ
            double tienBaoHiem = tongThuNhap * PHI_BAO_HIEM;
            double thuNhapTinhThue = tongThuNhap - tienBaoHiem;
            double tienThue = thuNhapTinhThue * PHI_THUE;

            // 4. Lương thực nhận cuối cùng
            double luongThucNhan = tongThuNhap - (tienBaoHiem + tienThue);

            return Math.Round(luongThucNhan, 0); // Làm tròn số tiền
        }
    }
}

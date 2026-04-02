using System;
using ql_nhanSW.Models;

namespace ql_nhanSW.BUS
{
    public class TinhLuongService
    {
        private const decimal PHI_BAO_HIEM = 0.105m;  // 10.5%
        private const decimal PHI_THUE = 0.1m;        // 10%

        /// <summary>
        /// Tính lương và trả về object Luong để lưu DB
        /// </summary>
        public Luong TinhVaTaoLuong(int maNhanVien, int thang, int nam,
            decimal luongCoBan, decimal kpi = 0, int soNgayCong = 26, int ngayCongChuan = 26)
        {
            // 1. Lương theo ngày công thực tế
            decimal luongTheoNgayCong = (luongCoBan / ngayCongChuan) * soNgayCong;

            // 2. Tổng thu nhập trước trừ
            decimal tongThuNhap = luongTheoNgayCong + kpi;

            // 3. Các khoản trừ
            decimal tienBaoHiem = tongThuNhap * PHI_BAO_HIEM;
            decimal thuNhapTinhThue = tongThuNhap - tienBaoHiem;
            decimal tienThue = thuNhapTinhThue * PHI_THUE;

            // 4. Lương thực nhận
            decimal tongLuong = tongThuNhap - (tienBaoHiem + tienThue);

            return new Luong
            {
                MaNhanVien = maNhanVien,
                Thang = thang,
                Nam = nam,
                LuongCoBan = luongCoBan,
                Thuong = kpi,                    // mình dùng KPI làm Thuong cho đơn giản
                KhauTru = tienBaoHiem + tienThue, // tự động tính
                TongLuong = Math.Round(tongLuong, 0)
            };
        }
    }
}
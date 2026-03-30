using ql_nhanSW.data;
using ql_nhanSW.Models;
using ql_nhanSW.share;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Text;

namespace ql_nhanSW.BUS
{
    public class NghiPhepService
    {
        private readonly NghiPhepRepository _repo = new();

        // Nhân viên gửi đơn nghỉ phép
        public (bool success, string message) GuiDon(
            DateTime tuNgay, DateTime denNgay,
            string loaiNghi, string lyDo)
        {
            if (SessionManager.CurrentUser == null)
                return (false, "Chưa đăng nhập!");

            if (tuNgay > denNgay)
                return (false, "Ngày bắt đầu phải trước ngày kết thúc!");

            // Lấy MaNhanVien từ session (giả sử lưu trong CurrentUser)
            // Cần join TaiKhoan -> NhanVien để lấy MaNhanVien
            var db = new AppDbContext();
            var nhanVien = db.NhanViens.FirstOrDefault(
                nv => nv.MaTaiKhoan == SessionManager.CurrentUser.MaTaiKhoan);

            if (nhanVien == null)
                return (false, "Không tìm thấy hồ sơ nhân viên!");

            var don = new NghiPhep
            {
                NhanVienId = nhanVien.MaNhanVien,
                NgayBatDau = tuNgay,
                NgayKetThuc = denNgay,
                LoaiNghi = loaiNghi,
                LyDo = lyDo,
                TrangThai = "Chờ duyệt"  // Mặc định khi gửi đơn
            };

            return _repo.GuiDon(don)
                ? (true, "Gửi đơn nghỉ phép thành công! Vui lòng chờ Admin duyệt.")
                : (false, "Gửi đơn thất bại!");
        }

        // Admin duyệt đơn
        public (bool success, string message) DuyetDon(int maNghiPhep)
        {
            if (!SessionManager.IsAdmin)
                return (false, "Chỉ Admin mới có quyền duyệt đơn!");

            return _repo.UpdateTrangThai(maNghiPhep, "Đồng ý")
                ? (true, "Đã duyệt đơn nghỉ phép!")
                : (false, "Không tìm thấy đơn nghỉ phép!");
        }

        // Admin từ chối đơn
        public (bool success, string message) TuChoiDon(int maNghiPhep)
        {
            if (!SessionManager.IsAdmin)
                return (false, "Chỉ Admin mới có quyền từ chối đơn!");

            return _repo.UpdateTrangThai(maNghiPhep, "Từ chối")
                ? (true, "Đã từ chối đơn nghỉ phép!")
                : (false, "Không tìm thấy đơn nghỉ phép!");
        }

        // Admin lấy danh sách chờ duyệt
        public List<NghiPhep> GetChoDuyet()
        {
            if (!SessionManager.IsAdmin) return new List<NghiPhep>();
            return _repo.GetChoDuyet();
        }

        // Nhân viên xem đơn của mình
        public List<NghiPhep> GetMyDon(int maNhanVien)
        {
            return _repo.GetByNhanVien(maNhanVien);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using global::ql_nhanSW.Models;
using ql_nhanSW.Models;
using System.Linq;

namespace ql_nhanSW.data
{
    public class NghiPhepRepository
    {
        private readonly AppDbContext _db;
        public NghiPhepRepository() { _db = new AppDbContext(); }

        // Lấy tất cả đơn (dành cho Admin)
        public List<NghiPhep> GetAll() => _db.NghiPhep.ToList();

        // Lấy đơn của 1 nhân viên
        public List<NghiPhep> GetByNhanVien(int maNhanVien) =>
            _db.NghiPhep.Where(x => x.NhanVienId == maNhanVien).ToList();

        // Lấy các đơn đang chờ duyệt (dành cho Admin)
        public List<NghiPhep> GetChoDuyet() =>
            _db.NghiPhep.Where(x => x.TrangThai == "Chờ duyệt").ToList();

        // Gửi đơn nghỉ phép
        public bool GuiDon(NghiPhep don)
        {
            _db.NghiPhep.Add(don);
            return _db.SaveChanges() > 0;
        }

        // Cập nhật trạng thái (duyệt / từ chối)
        public bool UpdateTrangThai(int maNghiPhep, string trangThai)
        {
            var don = _db.NghiPhep.FirstOrDefault(x => x.Id == maNghiPhep);
            if (don == null) return false;
            don.TrangThai = trangThai;
            return _db.SaveChanges() > 0;
        }
    }
}

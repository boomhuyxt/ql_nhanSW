using System;
using System.Collections.Generic;
using System.Text;
using global::ql_nhanSW.Models;
using System.Collections.Generic;
using System.Linq;

namespace ql_nhanSW.data
{
    public class HopDongRepository
    {
        private readonly AppDbContext _db;
        public HopDongRepository() { _db = new AppDbContext(); }

        public List<HopDongLaoDong> GetByNhanVien(int maNhanVien) =>
            _db.HopDongLaoDong.Where(x => x.NhanVienId == maNhanVien).ToList();

        public bool Add(HopDongLaoDong hd)
        {
            _db.HopDongLaoDong.Add(hd);
            return _db.SaveChanges() > 0;
        }

        public bool Update(HopDongLaoDong hd)
        {
            var existing = _db.HopDongLaoDong.FirstOrDefault(x => x.Id == hd.Id);
            if (existing == null) return false;
            existing.LoaiHopDong = hd.LoaiHopDong;
            existing.NgayBatDau = hd.NgayBatDau;
            existing.NgayKetThuc = hd.NgayKetThuc;
            existing.LuongCoBan = hd.LuongCoBan;
            existing.TrangThai = hd.TrangThai;
            return _db.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var hd = _db.HopDongLaoDong.FirstOrDefault(x => x.Id == id);
            if (hd == null) return false;
            _db.HopDongLaoDong.Remove(hd);
            return _db.SaveChanges() > 0;
        }
    }
}
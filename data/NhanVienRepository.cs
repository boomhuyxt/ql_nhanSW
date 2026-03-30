using System;
using System.Collections.Generic;
using System.Text;
using global::ql_nhanSW.Models;
using ql_nhanSW.Models;
using System.Linq;

namespace ql_nhanSW.data
{
    public class NhanVienRepository
    {
        private readonly AppDbContext _db;
        public NhanVienRepository() { _db = new AppDbContext(); }

        public List<NhanVien> GetAll() => _db.NhanViens.ToList();

        public NhanVien? GetById(int id) =>
            _db.NhanViens.FirstOrDefault(x => x.MaNhanVien == id);

        public List<NhanVien> Search(string keyword) =>
            _db.NhanViens
               .Where(x => x.HoTen.Contains(keyword))
               .ToList();

        public bool Add(NhanVien nv)
        {
            _db.NhanViens.Add(nv);
            return _db.SaveChanges() > 0;
        }

        public bool Update(NhanVien nv)
        {
            var existing = _db.NhanViens.FirstOrDefault(x => x.MaNhanVien == nv.MaNhanVien);
            if (existing == null) return false;

            existing.HoTen = nv.HoTen;
            existing.GioiTinh = nv.GioiTinh;
            existing.NgaySinh = nv.NgaySinh;
            existing.DiaChi = nv.DiaChi;
            existing.NgayVaoLam = nv.NgayVaoLam;
            existing.TrangThai = nv.TrangThai;
            existing.MaPhongBan = nv.MaPhongBan;
            existing.MaHopDong = nv.MaHopDong;

            return _db.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var nv = _db.NhanViens.FirstOrDefault(x => x.MaNhanVien == id);
            if (nv == null) return false;
            _db.NhanViens.Remove(nv);
            return _db.SaveChanges() > 0;
        }
    }
}
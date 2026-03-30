using System;
using System.Collections.Generic;
using System.Text;
using global::ql_nhanSW.Models;
using System.Linq;

namespace ql_nhanSW.data
{
    public class PhongBanRepository
    {
        private readonly AppDbContext _db;
        public PhongBanRepository() { _db = new AppDbContext(); }

        public List<PhongBan> GetAll() => _db.PhongBans.ToList();

        public bool Add(PhongBan pb)
        {
            _db.PhongBans.Add(pb);
            return _db.SaveChanges() > 0;
        }

        public bool Update(PhongBan pb)
        {
            var existing = _db.PhongBans.FirstOrDefault(x => x.MaPhongBan == pb.MaPhongBan);
            if (existing == null) return false;
            existing.TenPhongBan = pb.TenPhongBan;
            return _db.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var pb = _db.PhongBans.FirstOrDefault(x => x.MaPhongBan == id);
            if (pb == null) return false;
            _db.PhongBans.Remove(pb);
            return _db.SaveChanges() > 0;
        }
    }
}

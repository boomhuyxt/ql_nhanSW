using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ql_nhanSW.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<LichSuCongTac> LichSuCongTac { get; set; }
        public DbSet<HopDongLaoDong> HopDongLaoDong { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=QLNhanSu;Trusted_Connection=True;MultipleActiveResultSets=true"
            );
        }
    }
}
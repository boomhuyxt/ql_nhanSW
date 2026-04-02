using Microsoft.EntityFrameworkCore;
using System;

namespace ql_nhanSW.Models
{
    public class AppDbContext : DbContext
    {
        // Constructor không tham số (dành cho Repository/Service hiện tại của bạn)
        public AppDbContext() { }

        // Constructor có options (dành cho EF Core Tools + DI sau này)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongBan> PhongBans { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ChamCong> ChamCongs { get; set; }
        public DbSet<LichSuCongTac> LichSuCongTac { get; set; }
        public DbSet<HopDongLaoDong> HopDongLaoDong { get; set; }
        public DbSet<NghiPhep> NghiPhep { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<TaiKhoanVaiTro> TaiKhoanVaiTros { get; set; }
        public DbSet<Luong> Luongs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=QLNhanSu;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaiKhoanVaiTro>()
                .HasKey(tv => new { tv.MaTaiKhoan, tv.MaVaiTro });
        }
    }
}
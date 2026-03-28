using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        public int? MaTaiKhoan { get; set; }

        public string HoTen { get; set; }

        public string GioiTinh { get; set; }

        public DateTime? NgaySinh { get; set; }

        public string DiaChi { get; set; }

        public DateTime? NgayVaoLam { get; set; }

        public int? TrangThai { get; set; }

        public int? MaLuong { get; set; }

        public int? MaHopDong { get; set; }

        public int? MaPhongBan { get; set; }

        public string? FaceVector { get; set; }
    }
}
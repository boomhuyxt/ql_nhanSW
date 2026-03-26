using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        public string TenDangNhap { get; set; }

        public string MatKhauHash { get; set; }

        public string Email { get; set; }

        public string SoDienThoai { get; set; }

        public string AnhDaiDien { get; set; }

        public int? TrangThai { get; set; }

        public DateTime? NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }
    }
}
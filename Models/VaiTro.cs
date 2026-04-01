using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("VaiTro")]
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }

        [MaxLength(100)]
        public string? MaCode { get; set; }

        [MaxLength(255)]
        public string? TenVaiTro { get; set; }

        // Quan hệ N-N với TaiKhoan thông qua bảng TaiKhoanVaiTro
        public ICollection<TaiKhoanVaiTro>? TaiKhoanVaiTros { get; set; }
    }
}
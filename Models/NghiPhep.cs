using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    public class NghiPhep
    {
        [Key]
        public int Id { get; set; }

        public int NhanVienId { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public string? LoaiNghi { get; set; }

        public string? LyDo { get; set; }

        public string? TrangThai { get; set; }

        [ForeignKey("NhanVienId")]
        public NhanVien? NhanVien { get; set; }
    }
}
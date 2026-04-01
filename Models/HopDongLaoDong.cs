using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    public class HopDongLaoDong
    {
        [Key]
        public int Id { get; set; }

        public int NhanVienId { get; set; }

        [Required]
        public string LoaiHopDong { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public decimal LuongCoBan { get; set; }

        public string TrangThai { get; set; }

        // Liên kết Nhân viên
        [ForeignKey("NhanVienId")]
        public NhanVien NhanVien { get; set; }
    }
}
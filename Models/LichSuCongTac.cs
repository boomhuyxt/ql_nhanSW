using System;
using System.Collections.Generic;
using System.Text;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    public class LichSuCongTac
    {
        [Key]
        public int Id { get; set; }

        public int NhanVienId { get; set; }

        [Required]
        public string PhongBanCu { get; set; }

        [Required]
        public string PhongBanMoi { get; set; }

        public DateTime NgayChuyen { get; set; }

        public string LyDo { get; set; }

        // Liên kết tới bảng Nhân viên
        [ForeignKey("NhanVienId")]
        public NhanVien NhanVien { get; set; }
    }
}
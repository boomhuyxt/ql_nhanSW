using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("ChamCong")]
    public class ChamCong
    {
        [Key]
        public int MaChamCong { get; set; }

        public int MaNhanVien { get; set; }

        public DateTime NgayLamViec { get; set; }

        public TimeSpan? GioVao { get; set; }

        public TimeSpan? GioRa { get; set; }
    }
}
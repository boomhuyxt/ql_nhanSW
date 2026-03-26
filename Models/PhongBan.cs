using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("PhongBan")]
    public class PhongBan
    {
        [Key]
        public int MaPhongBan { get; set; }

        public string TenPhongBan { get; set; }
    }
}
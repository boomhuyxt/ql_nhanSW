using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    [Table("TaiKhoanVaiTro")]
    public class TaiKhoanVaiTro
    {
        public int MaTaiKhoan { get; set; }
        public int MaVaiTro { get; set; }

        // Navigation properties
        public TaiKhoan? TaiKhoan { get; set; }
        public VaiTro? VaiTro { get; set; }
    }
}
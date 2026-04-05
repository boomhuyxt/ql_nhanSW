using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ql_nhanSW.Models
{
    public class Luong
    {
        [Key]
        public int MaLuong { get; set; }

        // Sửa tại đây: Chỉ định rõ tên cột trong SQL là MaNhanVien
        [Column("MaNhanVien")]
        public int MaNhanVien { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; }

        public int Thang { get; set; }
        public int Nam { get; set; }
        public decimal LuongCoBan { get; set; }
        public decimal Thuong { get; set; }
        public decimal KhauTru { get; set; }
        public decimal TongLuong { get; set; }
    }
}
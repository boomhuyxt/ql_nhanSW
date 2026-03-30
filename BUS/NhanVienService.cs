using ql_nhanSW.data;
using ql_nhanSW.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ql_nhanSW.BUS
{
    public class NhanVienService
    {
        private readonly NhanVienRepository _repo = new();

        public List<NhanVien> GetAll() => _repo.GetAll();

        public NhanVien? GetById(int id) => _repo.GetById(id);

        public List<NhanVien> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return _repo.GetAll();
            return _repo.Search(keyword.Trim());
        }

        public (bool success, string message) Add(NhanVien nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen))
                return (false, "Họ tên không được để trống!");

            return _repo.Add(nv)
                ? (true, "Thêm nhân viên thành công!")
                : (false, "Thêm nhân viên thất bại!");
        }

        public (bool success, string message) Update(NhanVien nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen))
                return (false, "Họ tên không được để trống!");

            return _repo.Update(nv)
                ? (true, "Cập nhật thành công!")
                : (false, "Không tìm thấy nhân viên!");
        }

        public (bool success, string message) Delete(int id)
        {
            return _repo.Delete(id)
                ? (true, "Xóa thành công!")
                : (false, "Không tìm thấy nhân viên!");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using ql_nhanSW.share;
using System.Windows;

namespace ql_nhanSW.BUS
{
    public static class AuthorizationService
    {
        // Kiểm tra quyền Admin, nếu không có thì chặn + thông báo
        public static bool RequireAdmin()
        {
            if (SessionManager.IsAdmin) return true;

            MessageBox.Show(
                "Bạn không có quyền truy cập chức năng này!\nChỉ Admin mới được phép.",
                "Không có quyền",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return false;
        }

        // Kiểm tra quyền bất kỳ role nào
        public static bool HasRole(string roleCode)
        {
            return SessionManager.CurrentRoles.Contains(roleCode);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using ql_nhanSW.Models;


namespace ql_nhanSW.share
{
    public static class SessionManager
    {
        public static TaiKhoan? CurrentUser { get; set; }
        public static List<string> CurrentRoles { get; set; } = new();

        public static bool IsAdmin => CurrentRoles.Contains("ADMIN");

        public static void Clear()
        {
            CurrentUser = null;
            CurrentRoles.Clear();
        }
    }
}
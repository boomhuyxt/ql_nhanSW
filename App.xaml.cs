using System.Configuration;
using System.Data;
using System.Windows;

namespace ql_nhanSW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Thiết lập license EPPlus cho môi trường phát triển (NonCommercial)
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            }
            catch { }

        }
    }

}

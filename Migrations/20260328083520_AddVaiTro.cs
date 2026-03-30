using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ql_nhanSW.Migrations
{
    /// <inheritdoc />
    public partial class AddVaiTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenVaiTro = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoanVaiTro",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    TaiKhoanMaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    VaiTroMaVaiTro = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanVaiTro", x => new { x.MaTaiKhoan, x.MaVaiTro });
                    table.ForeignKey(
                        name: "FK_TaiKhoanVaiTro_TaiKhoan_TaiKhoanMaTaiKhoan",
                        column: x => x.TaiKhoanMaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                    table.ForeignKey(
                        name: "FK_TaiKhoanVaiTro_VaiTro_VaiTroMaVaiTro",
                        column: x => x.VaiTroMaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanVaiTro_TaiKhoanMaTaiKhoan",
                table: "TaiKhoanVaiTro",
                column: "TaiKhoanMaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanVaiTro_VaiTroMaVaiTro",
                table: "TaiKhoanVaiTro",
                column: "VaiTroMaVaiTro");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaiKhoanVaiTro");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ql_nhanSW.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLuongRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Luongs",
                columns: table => new
                {
                    MaLuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhanVien = table.Column<int>(type: "int", nullable: false),
                    NhanVienMaNhanVien = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    LuongCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Thuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KhauTru = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongLuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Luongs", x => x.MaLuong);
                    table.ForeignKey(
                        name: "FK_Luongs_NhanVien_NhanVienMaNhanVien",
                        column: x => x.NhanVienMaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Luongs_NhanVienMaNhanVien",
                table: "Luongs",
                column: "NhanVienMaNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Luongs");
        }
    }
}

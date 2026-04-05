using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ql_nhanSW.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalaryStructureV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Luongs_NhanVien_NhanVienMaNhanVien",
                table: "Luongs");

            migrationBuilder.DropIndex(
                name: "IX_Luongs_NhanVienMaNhanVien",
                table: "Luongs");

            migrationBuilder.DropColumn(
                name: "NhanVienMaNhanVien",
                table: "Luongs");

            migrationBuilder.CreateIndex(
                name: "IX_Luongs_MaNhanVien",
                table: "Luongs",
                column: "MaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_Luongs_NhanVien_MaNhanVien",
                table: "Luongs",
                column: "MaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Luongs_NhanVien_MaNhanVien",
                table: "Luongs");

            migrationBuilder.DropIndex(
                name: "IX_Luongs_MaNhanVien",
                table: "Luongs");

            migrationBuilder.AddColumn<int>(
                name: "NhanVienMaNhanVien",
                table: "Luongs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Luongs_NhanVienMaNhanVien",
                table: "Luongs",
                column: "NhanVienMaNhanVien");

            migrationBuilder.AddForeignKey(
                name: "FK_Luongs_NhanVien_NhanVienMaNhanVien",
                table: "Luongs",
                column: "NhanVienMaNhanVien",
                principalTable: "NhanVien",
                principalColumn: "MaNhanVien",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

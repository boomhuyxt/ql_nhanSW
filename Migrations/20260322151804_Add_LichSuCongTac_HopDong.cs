using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ql_nhanSW.Migrations
{
    /// <inheritdoc />
    public partial class Add_LichSuCongTac_HopDong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HopDongLaoDong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    LoaiHopDong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LuongCoBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDongLaoDong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HopDongLaoDong_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuCongTac",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhanVienId = table.Column<int>(type: "int", nullable: false),
                    PhongBanCu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhongBanMoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuCongTac", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuCongTac_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HopDongLaoDong_NhanVienId",
                table: "HopDongLaoDong",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuCongTac_NhanVienId",
                table: "LichSuCongTac",
                column: "NhanVienId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HopDongLaoDong");

            migrationBuilder.DropTable(
                name: "LichSuCongTac");
        }
    }
}

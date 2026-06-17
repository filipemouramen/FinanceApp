using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HashOtpCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "FinanceApp",
                table: "CodigosVerificacao",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1289));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1296));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1299));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1301));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1302));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1304));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1306));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1307));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 9,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1311));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 10,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1313));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 11,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1315));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 12,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1316));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 13,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1318));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 14,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1320));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 15,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1330));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 16,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1333));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 17,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1334));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 18,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1336));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 19,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 17, 12, 52, 49, 402, DateTimeKind.Utc).AddTicks(1338));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                schema: "FinanceApp",
                table: "CodigosVerificacao",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7067));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7195));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7200));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7204));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7208));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7212));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7215));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7218));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 9,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 10,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7226));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 11,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7229));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 12,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7232));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 13,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7236));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 14,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7240));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 15,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7243));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 16,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7252));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 17,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7257));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 18,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7260));

            migrationBuilder.UpdateData(
                schema: "FinanceApp",
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 19,
                column: "CriadoEm",
                value: new DateTime(2026, 6, 14, 2, 25, 10, 229, DateTimeKind.Utc).AddTicks(7263));
        }
    }
}

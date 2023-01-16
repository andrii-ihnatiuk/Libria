using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libria.Migrations
{
    public partial class smalluserfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "5120a7d2-1e62-4f56-a0b2-71253d296714");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1ff6c84d-e102-4feb-9007-9f585e3f6d2b", "AQAAAAEAACcQAAAAEOpIjW2hV4ftCXv2/KU4X6HnQHm15rF2OEgO7ZKnkcH8WMX7/eB5glx/wnGPABe0Zg==", "5df6421c-dfc2-4cbf-9530-b40ed746d31b" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "aa7baac2-38be-409f-a728-1e81d6d45d01");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "834b92c1-ab1d-4b9b-87a8-97d4418f7179", "AQAAAAEAACcQAAAAEA0BJzjRDnJdbAmUtul94z/FLL+YThQyzMrMFkyv1GFXDsCqFs3N7k/EFPHyCvLcIw==", "81572b5d-8f7c-408e-8d54-0cd456f17d7a" });
        }
    }
}

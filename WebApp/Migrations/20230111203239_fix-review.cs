using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libria.Migrations
{
    public partial class fixreview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "a16a19c5-94ac-41cd-b544-9e723accb46b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ed4108e7-8b2f-4e62-ad9f-e4c74dbf522e", "AQAAAAEAACcQAAAAECRtoesbcMCquR0seTsNAtFHkUWQowJ5ZsqC9VCpdl3i1+4VAE9DLDzQarVsmb3+AQ==", "92150d51-5fa9-4371-bc82-4b128e28c85d" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2c5e174e-3b0e-446f-86af-483d56fd7210",
                column: "ConcurrencyStamp",
                value: "89107da1-efe0-46c1-916e-767c97cb4d59");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "8e445865-a24d-4543-a6c6-9443d048cdb9",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6c0c8b67-e32f-4956-a198-727200a76195", "AQAAAAEAACcQAAAAENgBXFKe5CyzPaRKf3kWK/DAeJ+fmCIrk5GACN+Txug0uyOyWvi9TZXWxouSwpTOgg==", "e985457e-c9b6-4ad7-960f-915ef6e69ec8" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Libria.Migrations
{
    public partial class orderupd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "AspNetUsers",
                newName: "Address");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Orders",
                type: "text",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AspNetUsers",
                newName: "Country");

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
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RagChat.Migrations
{
    /// <inheritdoc />
    public partial class AddDatasourceTypeToUserIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DatasourceType",
                table: "UserIndexes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatasourceType",
                table: "UserIndexes");
        }
    }
}

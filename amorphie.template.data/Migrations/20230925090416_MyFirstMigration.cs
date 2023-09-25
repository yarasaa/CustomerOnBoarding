using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace amorphie.template.data.Migrations
{
    /// <inheritdoc />
    public partial class MyFirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepositMobApproval",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: false),
                    CitizenshipNumber = table.Column<long>(type: "bigint", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    MobApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsMobApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByBehalfOf = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedByBehalfOf = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositMobApproval", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositMobApproval");
        }
    }
}

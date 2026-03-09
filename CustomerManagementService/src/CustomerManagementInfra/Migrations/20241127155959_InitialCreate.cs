using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CustomerManagementInfra.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        private const string TimestampWithTimeZone = "timestamp with time zone";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTickets",
                columns: table => new
                {
                    Number = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    WaitTime = table.Column<DateTime>(type: TimestampWithTimeZone, nullable: false),
                    CallTime = table.Column<DateTime>(type: TimestampWithTimeZone, nullable: false),
                    CompleteTime = table.Column<DateTime>(type: TimestampWithTimeZone, nullable: false),
                    CancelTime = table.Column<DateTime>(type: TimestampWithTimeZone, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTickets", x => x.Number);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTickets");
        }
    }
}

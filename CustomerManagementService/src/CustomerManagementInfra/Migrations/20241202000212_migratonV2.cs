using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerManagementInfra.Migrations
{
    /// <inheritdoc />
    public partial class migratonV2 : Migration
    {
        private const string UserTicketsTable = "UserTickets";
        private const string TimestampWithTimeZone = "timestamp with time zone";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CompleteTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CancelTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CallTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CompleteTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CancelTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CallTime",
                table: UserTicketsTable,
                type: TimestampWithTimeZone,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: TimestampWithTimeZone,
                oldNullable: true);
        }
    }
}

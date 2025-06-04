using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GSAuth.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "SEQ_USERS");

            migrationBuilder.CreateTable(
                name: "GS_USERS",
                columns: table => new
                {
                    ID = table.Column<long>(type: "NUMBER(19)", nullable: false, defaultValueSql: "SEQ_USERS.NEXTVAL"),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PHONE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PASSWORD_HASH = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    ROLE = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    IS_ACTIVE = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false),
                    LAST_LOGIN = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ORGANIZATION_ID = table.Column<long>(type: "NUMBER(19)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GS_USERS", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GS_USERS");

            migrationBuilder.DropSequence(
                name: "SEQ_USERS");
        }
    }
}

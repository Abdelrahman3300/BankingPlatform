using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE SCHEMA IF NOT EXISTS identity;
                CREATE SCHEMA IF NOT EXISTS banking;
                CREATE SCHEMA IF NOT EXISTS payments;
                CREATE SCHEMA IF NOT EXISTS ledger;
                CREATE SCHEMA IF NOT EXISTS audit;
                CREATE SCHEMA IF NOT EXISTS infrastructure;
                """);
        }

        /// <inheritdoc />
       protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP SCHEMA IF EXISTS infrastructure RESTRICT;
                DROP SCHEMA IF EXISTS audit RESTRICT;
                DROP SCHEMA IF EXISTS ledger RESTRICT;
                DROP SCHEMA IF EXISTS payments RESTRICT;
                DROP SCHEMA IF EXISTS banking RESTRICT;
                DROP SCHEMA IF EXISTS identity RESTRICT;
                """);
        }
    }
}

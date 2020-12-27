using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.DataProtection.Data.Migrations.SqlServer
{
	public partial class SqlServerDataProtectionMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "DataProtectionKeys");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "DataProtectionKeys",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					FriendlyName = table.Column<string>(nullable: true),
					Xml = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
				});
		}

		#endregion
	}
}
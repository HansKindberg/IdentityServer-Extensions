using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.DataProtection.Data.Migrations.Sqlite
{
	public partial class SqliteDataProtectionMigration : Migration
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
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					FriendlyName = table.Column<string>(type: "TEXT COLLATE NOCASE", nullable: true),
					Xml = table.Column<string>(type: "TEXT COLLATE NOCASE", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
				});
		}

		#endregion
	}
}
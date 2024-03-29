using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Data.Migrations.Operational.Sqlite
{
	public partial class SqliteOperationalMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "DeviceCodes");

			migrationBuilder.DropTable(
				name: "Keys");

			migrationBuilder.DropTable(
				name: "PersistedGrants");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "DeviceCodes",
				columns: table => new
				{
					UserCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					DeviceCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					SubjectId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true, collation: "NOCASE"),
					SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
					ClientId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true, collation: "NOCASE"),
					CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
					Expiration = table.Column<DateTime>(type: "TEXT", nullable: false),
					Data = table.Column<string>(type: "TEXT", maxLength: 50000, nullable: false, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DeviceCodes", x => x.UserCode);
				});

			migrationBuilder.CreateTable(
				name: "Keys",
				columns: table => new
				{
					Id = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					Version = table.Column<int>(type: "INTEGER", nullable: false),
					Created = table.Column<DateTime>(type: "TEXT", nullable: false),
					Use = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					Algorithm = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
					IsX509Certificate = table.Column<bool>(type: "INTEGER", nullable: false),
					DataProtected = table.Column<bool>(type: "INTEGER", nullable: false),
					Data = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Keys", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "PersistedGrants",
				columns: table => new
				{
					Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, collation: "NOCASE"),
					SubjectId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true, collation: "NOCASE"),
					SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, collation: "NOCASE"),
					ClientId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true, collation: "NOCASE"),
					CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
					Expiration = table.Column<DateTime>(type: "TEXT", nullable: true),
					ConsumedTime = table.Column<DateTime>(type: "TEXT", nullable: true),
					Data = table.Column<string>(type: "TEXT", maxLength: 50000, nullable: false, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PersistedGrants", x => x.Key);
				});

			migrationBuilder.CreateIndex(
				name: "IX_DeviceCodes_DeviceCode",
				table: "DeviceCodes",
				column: "DeviceCode",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_DeviceCodes_Expiration",
				table: "DeviceCodes",
				column: "Expiration");

			migrationBuilder.CreateIndex(
				name: "IX_Keys_Use",
				table: "Keys",
				column: "Use");

			migrationBuilder.CreateIndex(
				name: "IX_PersistedGrants_ConsumedTime",
				table: "PersistedGrants",
				column: "ConsumedTime");

			migrationBuilder.CreateIndex(
				name: "IX_PersistedGrants_Expiration",
				table: "PersistedGrants",
				column: "Expiration");

			migrationBuilder.CreateIndex(
				name: "IX_PersistedGrants_SubjectId_ClientId_Type",
				table: "PersistedGrants",
				columns: new[] { "SubjectId", "ClientId", "Type" });

			migrationBuilder.CreateIndex(
				name: "IX_PersistedGrants_SubjectId_SessionId_Type",
				table: "PersistedGrants",
				columns: new[] { "SubjectId", "SessionId", "Type" });
		}

		#endregion
	}
}
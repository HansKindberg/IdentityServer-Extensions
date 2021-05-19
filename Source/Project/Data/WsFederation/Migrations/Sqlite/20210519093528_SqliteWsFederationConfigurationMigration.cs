using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Data.WsFederation.Migrations.Sqlite
{
	public partial class SqliteWsFederationConfigurationMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "RelyingPartyClaimMappings");

			migrationBuilder.DropTable(
				name: "RelyingParties");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "RelyingParties",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Realm = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					TokenType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					SignatureAlgorithm = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					DigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					SamlNameIdentifierFormat = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RelyingParties", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "RelyingPartyClaimMappings",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					OriginalClaimType = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false, collation: "NOCASE"),
					NewClaimType = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false, collation: "NOCASE"),
					RelyingPartyId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RelyingPartyClaimMappings", x => x.Id);
					table.ForeignKey(
						name: "FK_RelyingPartyClaimMappings_RelyingParties_RelyingPartyId",
						column: x => x.RelyingPartyId,
						principalTable: "RelyingParties",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_RelyingParties_Realm",
				table: "RelyingParties",
				column: "Realm",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_RelyingPartyClaimMappings_RelyingPartyId",
				table: "RelyingPartyClaimMappings",
				column: "RelyingPartyId");
		}

		#endregion
	}
}
using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Data.WsFederation.Migrations.SqlServer
{
	public partial class SqlServerWsFederationConfigurationMigration : Migration
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
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Realm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					TokenType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					SignatureAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: true),
					DigestAlgorithm = table.Column<string>(type: "nvarchar(max)", nullable: true),
					SamlNameIdentifierFormat = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RelyingParties", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "RelyingPartyClaimMappings",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					OriginalClaimType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					NewClaimType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					RelyingPartyId = table.Column<int>(type: "int", nullable: false)
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
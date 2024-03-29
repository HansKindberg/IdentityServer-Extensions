using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Data.Saml.Migrations.Sqlite
{
	public partial class SqliteSamlConfigurationMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ServiceProviderAssertionConsumerServices");

			migrationBuilder.DropTable(
				name: "ServiceProviderClaimMappings");

			migrationBuilder.DropTable(
				name: "ServiceProviderSignCertificates");

			migrationBuilder.DropTable(
				name: "ServiceProviderSingleLogoutServices");

			migrationBuilder.DropTable(
				name: "ServiceProviders");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "ServiceProviders",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					EntityId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
					EncryptionCertificate = table.Column<byte[]>(type: "BLOB", nullable: true),
					SignAssertions = table.Column<bool>(type: "INTEGER", nullable: false),
					EncryptAssertions = table.Column<bool>(type: "INTEGER", nullable: false),
					RequireSamlMessageDestination = table.Column<bool>(type: "INTEGER", nullable: false),
					AllowIdpInitiatedSso = table.Column<bool>(type: "INTEGER", nullable: false),
					RequireAuthenticationRequestsSigned = table.Column<bool>(type: "INTEGER", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviders", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ServiceProviderAssertionConsumerServices",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Binding = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, collation: "NOCASE"),
					Location = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, collation: "NOCASE"),
					Index = table.Column<int>(type: "INTEGER", nullable: false),
					IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
					ServiceProviderId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviderAssertionConsumerServices", x => x.Id);
					table.ForeignKey(
						name: "FK_ServiceProviderAssertionConsumerServices_ServiceProviders_ServiceProviderId",
						column: x => x.ServiceProviderId,
						principalTable: "ServiceProviders",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ServiceProviderClaimMappings",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					OriginalClaimType = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false, collation: "NOCASE"),
					NewClaimType = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false, collation: "NOCASE"),
					ServiceProviderId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviderClaimMappings", x => x.Id);
					table.ForeignKey(
						name: "FK_ServiceProviderClaimMappings_ServiceProviders_ServiceProviderId",
						column: x => x.ServiceProviderId,
						principalTable: "ServiceProviders",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ServiceProviderSignCertificates",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Certificate = table.Column<byte[]>(type: "BLOB", nullable: false),
					ServiceProviderId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviderSignCertificates", x => x.Id);
					table.ForeignKey(
						name: "FK_ServiceProviderSignCertificates_ServiceProviders_ServiceProviderId",
						column: x => x.ServiceProviderId,
						principalTable: "ServiceProviders",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ServiceProviderSingleLogoutServices",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					Binding = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, collation: "NOCASE"),
					Location = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false, collation: "NOCASE"),
					Index = table.Column<int>(type: "INTEGER", nullable: false),
					IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
					ServiceProviderId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviderSingleLogoutServices", x => x.Id);
					table.ForeignKey(
						name: "FK_ServiceProviderSingleLogoutServices_ServiceProviders_ServiceProviderId",
						column: x => x.ServiceProviderId,
						principalTable: "ServiceProviders",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ServiceProviderAssertionConsumerServices_ServiceProviderId",
				table: "ServiceProviderAssertionConsumerServices",
				column: "ServiceProviderId");

			migrationBuilder.CreateIndex(
				name: "IX_ServiceProviderClaimMappings_ServiceProviderId",
				table: "ServiceProviderClaimMappings",
				column: "ServiceProviderId");

			migrationBuilder.CreateIndex(
				name: "IX_ServiceProviders_EntityId",
				table: "ServiceProviders",
				column: "EntityId",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_ServiceProviderSignCertificates_ServiceProviderId",
				table: "ServiceProviderSignCertificates",
				column: "ServiceProviderId");

			migrationBuilder.CreateIndex(
				name: "IX_ServiceProviderSingleLogoutServices_ServiceProviderId",
				table: "ServiceProviderSingleLogoutServices",
				column: "ServiceProviderId");
		}

		#endregion
	}
}
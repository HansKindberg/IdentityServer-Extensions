using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Data.Saml.Migrations.SqlServer
{
	public partial class SqlServerSamlConfigurationMigration : Migration
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
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					EntityId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
					EncryptionCertificate = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
					SignAssertions = table.Column<bool>(type: "bit", nullable: false),
					EncryptAssertions = table.Column<bool>(type: "bit", nullable: false),
					RequireSamlMessageDestination = table.Column<bool>(type: "bit", nullable: false),
					AllowIdpInitiatedSso = table.Column<bool>(type: "bit", nullable: false),
					RequireAuthenticationRequestsSigned = table.Column<bool>(type: "bit", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceProviders", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "ServiceProviderAssertionConsumerServices",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Binding = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
					Location = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
					Index = table.Column<int>(type: "int", nullable: false),
					IsDefault = table.Column<bool>(type: "bit", nullable: false),
					ServiceProviderId = table.Column<int>(type: "int", nullable: false)
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
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					OriginalClaimType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					NewClaimType = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
					ServiceProviderId = table.Column<int>(type: "int", nullable: false)
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
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Certificate = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
					ServiceProviderId = table.Column<int>(type: "int", nullable: false)
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
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Binding = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
					Location = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
					Index = table.Column<int>(type: "int", nullable: false),
					IsDefault = table.Column<bool>(type: "bit", nullable: false),
					ServiceProviderId = table.Column<int>(type: "int", nullable: false)
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
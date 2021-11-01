using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HansKindberg.IdentityServer.Identity.Data.Migrations.Sqlite
{
	public partial class SqliteIdentityMigration : Migration
	{
		#region Methods

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AspNetRoleClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserLogins");

			migrationBuilder.DropTable(
				name: "AspNetUserRoles");

			migrationBuilder.DropTable(
				name: "AspNetUserTokens");

			migrationBuilder.DropTable(
				name: "AspNetRoles");

			migrationBuilder.DropTable(
				name: "AspNetUsers");
		}

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AspNetRoles",
				columns: table => new
				{
					Id = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUsers",
				columns: table => new
				{
					Id = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, collation: "NOCASE"),
					EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
					PasswordHash = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					SecurityStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					PhoneNumber = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
					TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
					LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
					LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
					AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUsers", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetRoleClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					RoleId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					ClaimType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					ClaimValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					ClaimType = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					ClaimValue = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetUserClaims_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserLogins",
				columns: table => new
				{
					LoginProvider = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					ProviderKey = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE"),
					UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
					table.ForeignKey(
						name: "FK_AspNetUserLogins_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserRoles",
				columns: table => new
				{
					UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					RoleId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserTokens",
				columns: table => new
				{
					UserId = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					LoginProvider = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
					Value = table.Column<string>(type: "TEXT", nullable: true, collation: "NOCASE")
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
					table.ForeignKey(
						name: "FK_AspNetUserTokens_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AspNetRoleClaims_RoleId",
				table: "AspNetRoleClaims",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "RoleNameIndex",
				table: "AspNetRoles",
				column: "NormalizedName",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserClaims_UserId",
				table: "AspNetUserClaims",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserLogins_UserId",
				table: "AspNetUserLogins",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserRoles_RoleId",
				table: "AspNetUserRoles",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "EmailIndex",
				table: "AspNetUsers",
				column: "NormalizedEmail");

			migrationBuilder.CreateIndex(
				name: "UserNameIndex",
				table: "AspNetUsers",
				column: "NormalizedUserName",
				unique: true);
		}

		#endregion
	}
}
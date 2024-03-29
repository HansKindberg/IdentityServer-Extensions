﻿// <auto-generated />
using HansKindberg.IdentityServer.Data.WsFederation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HansKindberg.IdentityServer.Data.WsFederation.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerWsFederationConfiguration))]
    partial class SqlServerWsFederationConfigurationModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Rsk.WsFederation.EntityFramework.Entities.RelyingParty", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DigestAlgorithm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Realm")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("SamlNameIdentifierFormat")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SignatureAlgorithm")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TokenType")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Realm")
                        .IsUnique();

                    b.ToTable("RelyingParties");
                });

            modelBuilder.Entity("Rsk.WsFederation.EntityFramework.Entities.WsFederationClaimMap", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("NewClaimType")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<string>("OriginalClaimType")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<int>("RelyingPartyId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RelyingPartyId");

                    b.ToTable("RelyingPartyClaimMappings");
                });

            modelBuilder.Entity("Rsk.WsFederation.EntityFramework.Entities.WsFederationClaimMap", b =>
                {
                    b.HasOne("Rsk.WsFederation.EntityFramework.Entities.RelyingParty", "RelyingParty")
                        .WithMany("ClaimMapping")
                        .HasForeignKey("RelyingPartyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RelyingParty");
                });

            modelBuilder.Entity("Rsk.WsFederation.EntityFramework.Entities.RelyingParty", b =>
                {
                    b.Navigation("ClaimMapping");
                });
#pragma warning restore 612, 618
        }
    }
}

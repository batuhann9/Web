﻿// <auto-generated />
using BerberSalonu.Veritabanı;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BerberSalonu.Migrations
{
    [DbContext(typeof(BerberContext))]
    [Migration("20241204162201_İlkOlusturma")]
    partial class İlkOlusturma
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BerberSalonu.Models.Berber", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Berberler");
                });

            modelBuilder.Entity("BerberSalonu.Models.Yetenek", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Yetenekler");
                });

            modelBuilder.Entity("BerberYetenek", b =>
                {
                    b.Property<int>("BerberlerId")
                        .HasColumnType("integer");

                    b.Property<int>("YeteneklerId")
                        .HasColumnType("integer");

                    b.HasKey("BerberlerId", "YeteneklerId");

                    b.HasIndex("YeteneklerId");

                    b.ToTable("BerberYetenek");
                });

            modelBuilder.Entity("BerberYetenek", b =>
                {
                    b.HasOne("BerberSalonu.Models.Berber", null)
                        .WithMany()
                        .HasForeignKey("BerberlerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BerberSalonu.Models.Yetenek", null)
                        .WithMany()
                        .HasForeignKey("YeteneklerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}

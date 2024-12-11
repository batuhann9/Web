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
    [Migration("20241206164154_berber-kullanici")]
    partial class berberkullanici
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

                    b.Property<int?>("KullaniciId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("KullaniciId");

                    b.ToTable("Berberler");
                });

            modelBuilder.Entity("BerberSalonu.Models.Kullanici", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Ad")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Eposta")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("RolId")
                        .HasColumnType("integer");

                    b.Property<string>("SifreHashi")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Soyad")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("RolId");

                    b.ToTable("Kullanicilar");
                });

            modelBuilder.Entity("BerberSalonu.Models.Rol", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Roller");
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

            modelBuilder.Entity("BerberSalonu.Models.Berber", b =>
                {
                    b.HasOne("BerberSalonu.Models.Kullanici", "Kullanici")
                        .WithMany()
                        .HasForeignKey("KullaniciId");

                    b.Navigation("Kullanici");
                });

            modelBuilder.Entity("BerberSalonu.Models.Kullanici", b =>
                {
                    b.HasOne("BerberSalonu.Models.Rol", "Rol")
                        .WithMany()
                        .HasForeignKey("RolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rol");
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

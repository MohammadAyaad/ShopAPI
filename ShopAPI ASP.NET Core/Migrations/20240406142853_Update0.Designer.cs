﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShopAPI.Data;

#nullable disable

namespace ShopAPI.Migrations
{
    [DbContext(typeof(ShopDBContext))]
    [Migration("20240406142853_Update0")]
    partial class Update0
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("ShopAPI.Model.Packages.Package", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageLink")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<long>("TimesRates")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("ShopAPI.Model.Packages.PackageContent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PackageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProductId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PackageContent");
                });

            modelBuilder.Entity("ShopAPI.Model.Packages.PackageRating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("DownVotes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PackageId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("RatedAt")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<double>("Score")
                        .HasColumnType("REAL");

                    b.Property<long>("UpVotes")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PackageId");

                    b.ToTable("PackageRatings");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageLink")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<long>("TimesRates")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.ProductRating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("DownVotes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProductId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProductVariantId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("RatedAt")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Rating")
                        .HasColumnType("REAL");

                    b.Property<double>("Score")
                        .HasColumnType("REAL");

                    b.Property<long>("UpVotes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VariantId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ProductVariantId");

                    b.HasIndex("ProductId", "VariantId")
                        .IsUnique();

                    b.ToTable("ProductRatings");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.ProductVariant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AvailableQuantity")
                        .HasColumnType("REAL");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.Property<int>("ProductId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VariantProperties")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductVariants");
                });

            modelBuilder.Entity("ShopAPI.Model.Users.UserAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<long>("CreatedAt")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("Role")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("ShopAPI.Model.Packages.PackageRating", b =>
                {
                    b.HasOne("ShopAPI.Model.Packages.Package", "Package")
                        .WithMany("PackageRatings")
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.ProductRating", b =>
                {
                    b.HasOne("ShopAPI.Model.Products.Product", "Product")
                        .WithMany("ProductRatings")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShopAPI.Model.Products.ProductVariant", "ProductVariant")
                        .WithMany()
                        .HasForeignKey("ProductVariantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("ProductVariant");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.ProductVariant", b =>
                {
                    b.HasOne("ShopAPI.Model.Products.Product", "Product")
                        .WithMany("ProductVariants")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ShopAPI.Model.Packages.Package", b =>
                {
                    b.Navigation("PackageRatings");
                });

            modelBuilder.Entity("ShopAPI.Model.Products.Product", b =>
                {
                    b.Navigation("ProductRatings");

                    b.Navigation("ProductVariants");
                });
#pragma warning restore 612, 618
        }
    }
}

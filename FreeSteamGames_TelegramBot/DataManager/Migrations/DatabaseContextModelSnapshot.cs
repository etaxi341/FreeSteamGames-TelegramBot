﻿// <auto-generated />
using DataManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataManager.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("DataManager.Models.Notifications", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("chatID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("steamLink")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("chatID", "steamLink");

                    b.ToTable("notifications");
                });

            modelBuilder.Entity("DataManager.Models.Subscribers", b =>
                {
                    b.Property<long>("chatID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("wantsDlcInfo")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("wantsGameInfo")
                        .HasColumnType("INTEGER");

                    b.HasKey("chatID");

                    b.HasIndex("chatID");

                    b.ToTable("subscribers");
                });
#pragma warning restore 612, 618
        }
    }
}

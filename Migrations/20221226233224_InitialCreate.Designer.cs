﻿// <auto-generated />
using System;
using DevCommuBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DevCommuBot.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20221226233224_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.1");

            modelBuilder.Entity("DevCommuBot.Data.Models.Forums.Forum", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClosedTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("MessageDescription")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Forum");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Forums.ForumEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ForumId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("ForumId");

                    b.ToTable("ForumEntry");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.StarboardEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ArrivedTime")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("StarboardMessageId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("StarboardEntry");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DisplayPartnerAds")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ForumId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Points")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Tier")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ForumId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.UserWarning", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarningId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId", "WarningId");

                    b.HasIndex("WarningId");

                    b.ToTable("UserWarning");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Warnings.Warning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Details")
                        .HasColumnType("TEXT");

                    b.Property<int>("Reason")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Warning");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Forums.ForumEntry", b =>
                {
                    b.HasOne("DevCommuBot.Data.Models.Users.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId");

                    b.HasOne("DevCommuBot.Data.Models.Forums.Forum", null)
                        .WithMany("Entries")
                        .HasForeignKey("ForumId");

                    b.Navigation("Author");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.User", b =>
                {
                    b.HasOne("DevCommuBot.Data.Models.Forums.Forum", null)
                        .WithMany("Moderators")
                        .HasForeignKey("ForumId");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.UserWarning", b =>
                {
                    b.HasOne("DevCommuBot.Data.Models.Users.User", "User")
                        .WithMany("Warnings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DevCommuBot.Data.Models.Warnings.Warning", "Warning")
                        .WithMany()
                        .HasForeignKey("WarningId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Warning");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Forums.Forum", b =>
                {
                    b.Navigation("Entries");

                    b.Navigation("Moderators");
                });

            modelBuilder.Entity("DevCommuBot.Data.Models.Users.User", b =>
                {
                    b.Navigation("Warnings");
                });
#pragma warning restore 612, 618
        }
    }
}

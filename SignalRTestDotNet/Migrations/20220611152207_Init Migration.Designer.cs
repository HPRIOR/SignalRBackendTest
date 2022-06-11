﻿// <auto-generated />
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SignalRTestDotNet.GameContextNs;

#nullable disable

namespace SignalRTestDotNet.Migrations
{
    [DbContext(typeof(GameContext))]
    [Migration("20220611152207_Init Migration")]
    partial class InitMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Game", b =>
                {
                    b.Property<string>("GameId")
                        .HasColumnType("text");

                    b.Property<int>("CurrentTurn")
                        .HasColumnType("integer");

                    b.Property<List<int>>("PlayedTurns")
                        .HasColumnType("integer[]");

                    b.Property<string>("SessionId")
                        .HasColumnType("text");

                    b.HasKey("GameId");

                    b.HasIndex("SessionId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Move", b =>
                {
                    b.Property<string>("MoveId")
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<string>("SessionId")
                        .HasColumnType("text");

                    b.Property<int>("Turn")
                        .HasColumnType("integer");

                    b.HasKey("MoveId");

                    b.HasIndex("SessionId");

                    b.ToTable("Moves");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Player", b =>
                {
                    b.Property<string>("PlayerId")
                        .HasColumnType("text");

                    b.Property<string>("SessionId")
                        .HasColumnType("text");

                    b.HasKey("PlayerId");

                    b.HasIndex("SessionId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Session", b =>
                {
                    b.Property<string>("SessionId")
                        .HasColumnType("text");

                    b.Property<string>("AdminId")
                        .HasColumnType("text");

                    b.HasKey("SessionId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Game", b =>
                {
                    b.HasOne("SignalRTestDotNet.GameContextNs.Session", "Session")
                        .WithMany()
                        .HasForeignKey("SessionId");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Move", b =>
                {
                    b.HasOne("SignalRTestDotNet.GameContextNs.Session", "session")
                        .WithMany()
                        .HasForeignKey("SessionId");

                    b.Navigation("session");
                });

            modelBuilder.Entity("SignalRTestDotNet.GameContextNs.Player", b =>
                {
                    b.HasOne("SignalRTestDotNet.GameContextNs.Session", "Session")
                        .WithMany()
                        .HasForeignKey("SessionId");

                    b.Navigation("Session");
                });
#pragma warning restore 612, 618
        }
    }
}
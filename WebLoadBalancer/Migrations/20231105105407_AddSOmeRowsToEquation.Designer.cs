﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebLoadBalancer.Models;

#nullable disable

namespace WebLoadBalancer.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20231105105407_AddSOmeRowsToEquation")]
    partial class AddSOmeRowsToEquation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("WebLoadBalancer.Models.EquationSol", b =>
                {
                    b.Property<int>("task_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("task_id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<byte[]>("equation")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("equation_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("solution")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<int>("user_id")
                        .HasColumnType("integer");

                    b.HasKey("task_id");

                    b.ToTable("equation", (string)null);
                });

            modelBuilder.Entity("WebLoadBalancer.Models.web_user", b =>
                {
                    b.Property<int>("user_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("user_id"));

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("user_password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("user_id");

                    b.ToTable("web_user", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using System;
using EQToolApis.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EQToolApis.Migrations
{
    [DbContext(typeof(EQToolContext))]
    partial class EQToolContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EQToolApis.DB.Models.EqToolException", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<string>("Exception")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("EqToolExceptions");
                });

            modelBuilder.Entity("EQToolApis.DB.Models.Player", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(24)
                        .HasColumnType("nvarchar(24)");

                    b.Property<byte>("Server")
                        .HasColumnType("tinyint");

                    b.Property<byte>("Level")
                        .HasColumnType("tinyint");

                    b.Property<byte>("PlayerClass")
                        .HasColumnType("tinyint");

                    b.HasKey("Name", "Server");

                    b.ToTable("Players");
                });
#pragma warning restore 612, 618
        }
    }
}

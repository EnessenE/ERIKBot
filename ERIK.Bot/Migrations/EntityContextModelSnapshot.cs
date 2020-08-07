﻿// <auto-generated />
using System;
using ERIK.Bot.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ERIK.Bot.Migrations
{
    [DbContext(typeof(EntityContext))]
    partial class EntityContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ERIK.Bot.Models.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("LfgPrepublishChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<decimal>("LfgPublishChannelId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Prefix")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("ERIK.Bot.Models.Reactions.DiscordUser", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Id");

                    b.ToTable("DiscordUsers");
                });

            modelBuilder.Entity("ERIK.Bot.Models.Reactions.MessageReaction", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("SavedMessageMessageId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<decimal?>("UserId")
                        .HasColumnType("decimal(20,0)");

                    b.HasKey("Guid");

                    b.HasIndex("SavedMessageMessageId");

                    b.HasIndex("UserId");

                    b.ToTable("MessageReaction");
                });

            modelBuilder.Entity("ERIK.Bot.Models.Reactions.SavedMessage", b =>
                {
                    b.Property<decimal>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(20,0)");

                    b.Property<bool>("IsFinished")
                        .HasColumnType("bit");

                    b.Property<bool>("Published")
                        .HasColumnType("bit");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("MessageId");

                    b.ToTable("SavedMessages");
                });

            modelBuilder.Entity("ERIK.Bot.Models.Reactions.MessageReaction", b =>
                {
                    b.HasOne("ERIK.Bot.Models.Reactions.SavedMessage", null)
                        .WithMany("Reactions")
                        .HasForeignKey("SavedMessageMessageId");

                    b.HasOne("ERIK.Bot.Models.Reactions.DiscordUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}

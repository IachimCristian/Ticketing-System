﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TicketingSystem.Infrastructure.Data;

#nullable disable

namespace TicketingSystem.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250525034808_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TicketingSystem.Core.Entities.Administrator", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Administrators", (string)null);
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Customers", (string)null);
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.CustomerNotification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ActionText")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ActionUrl")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("EmailSent")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("EmailSentAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Priority")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("SmsSent")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("SmsSentAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("TicketId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("EventId");

                    b.HasIndex("TicketId");

                    b.ToTable("CustomerNotifications");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Event", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ImageUrl")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid>("OrganizerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("TicketPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("OrganizerId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.NotificationPreferences", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("EmailEventCancellations")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailEventReminders")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailEventUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailPromotions")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailRefundUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("EmailTicketPurchase")
                        .HasColumnType("bit");

                    b.Property<int>("EventReminderHours")
                        .HasColumnType("int");

                    b.Property<bool>("InAppEventCancellations")
                        .HasColumnType("bit");

                    b.Property<bool>("InAppEventReminders")
                        .HasColumnType("bit");

                    b.Property<bool>("InAppEventUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("InAppPromotions")
                        .HasColumnType("bit");

                    b.Property<bool>("InAppRefundUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("InAppTicketPurchase")
                        .HasColumnType("bit");

                    b.Property<bool>("SmsEventCancellations")
                        .HasColumnType("bit");

                    b.Property<bool>("SmsEventReminders")
                        .HasColumnType("bit");

                    b.Property<bool>("SmsEventUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("SmsRefundUpdates")
                        .HasColumnType("bit");

                    b.Property<bool>("SmsTicketPurchase")
                        .HasColumnType("bit");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId")
                        .IsUnique();

                    b.ToTable("NotificationPreferences");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Organizer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ContactPhone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrganizationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Organizers", (string)null);
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Payment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("RelatedPaymentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.SeatMap", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Columns")
                        .HasColumnType("int");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Rows")
                        .HasColumnType("int");

                    b.Property<string>("SeatLayout")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.ToTable("SeatMaps");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.SeatSection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EndColumn")
                        .HasColumnType("int");

                    b.Property<int>("EndRow")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PriceMultiplier")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("SeatMapId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("StartColumn")
                        .HasColumnType("int");

                    b.Property<int>("StartRow")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SeatMapId");

                    b.ToTable("SeatSections");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Ticket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CancelDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("PaymentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("PurchaseDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("QRCode")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<Guid?>("RefundId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RefundProcessDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RefundStatus")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int?>("SeatColumn")
                        .HasColumnType("int");

                    b.Property<Guid?>("SeatId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("SeatRow")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TicketNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("EventId");

                    b.HasIndex("PaymentId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.CustomerNotification", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Customer", "Customer")
                        .WithMany("Notifications")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TicketingSystem.Core.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("TicketingSystem.Core.Entities.Ticket", "Ticket")
                        .WithMany()
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Customer");

                    b.Navigation("Event");

                    b.Navigation("Ticket");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Event", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Organizer", "Organizer")
                        .WithMany("Events")
                        .HasForeignKey("OrganizerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Organizer");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.NotificationPreferences", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Customer", "Customer")
                        .WithOne("NotificationPreferences")
                        .HasForeignKey("TicketingSystem.Core.Entities.NotificationPreferences", "CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Payment", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.SeatMap", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.SeatSection", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.SeatMap", "SeatMap")
                        .WithMany("Sections")
                        .HasForeignKey("SeatMapId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SeatMap");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Ticket", b =>
                {
                    b.HasOne("TicketingSystem.Core.Entities.Customer", "Customer")
                        .WithMany("Tickets")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TicketingSystem.Core.Entities.Event", "Event")
                        .WithMany("Tickets")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TicketingSystem.Core.Entities.Payment", "Payment")
                        .WithMany("Tickets")
                        .HasForeignKey("PaymentId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Customer");

                    b.Navigation("Event");

                    b.Navigation("Payment");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Customer", b =>
                {
                    b.Navigation("NotificationPreferences")
                        .IsRequired();

                    b.Navigation("Notifications");

                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Event", b =>
                {
                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Organizer", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.Payment", b =>
                {
                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("TicketingSystem.Core.Entities.SeatMap", b =>
                {
                    b.Navigation("Sections");
                });
#pragma warning restore 612, 618
        }
    }
}

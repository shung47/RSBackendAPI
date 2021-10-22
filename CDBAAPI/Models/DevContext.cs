﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace CDBAAPI.Models
{
    public partial class DevContext : DbContext
    {
        public DevContext()
        {
        }

        public DevContext(DbContextOptions<DevContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblDbControl> TblDbControls { get; set; }
        public virtual DbSet<TblTask> TblTasks { get; set; }
        public virtual DbSet<TblTicket> TblTickets { get; set; }
        public virtual DbSet<TblTicketLog> TblTicketLogs { get; set; }
        public virtual DbSet<TblUser> TblUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=(local);Database=Dev;Trusted_Connection=True;User ID=ASIA\\\\\\\\057533;Password=");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<TblDbControl>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tblDB_Control");

                entity.Property(e => e.Database)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaMaster)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SA_Master");
            });

            modelBuilder.Entity<TblTask>(entity =>
            {
                entity.ToTable("tblTasks");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Department)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Region)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Summary)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTicket>(entity =>
            {
                entity.ToTable("tblTickets");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Assignee)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.BusinessReviewer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompletedDateTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.Dbmaster)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("DBMaster");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Developer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsRpa).HasColumnName("IsRPA");

                entity.Property(e => e.LastModificationDateTime).HasColumnType("datetime");

                entity.Property(e => e.PrimaryCodeReviewer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SecondaryCodeReviewer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SecondaryDeveloper)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTicketLog>(entity =>
            {
                entity.ToTable("tblTicket_Logs");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovalType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModificationDatetime).HasColumnType("datetime");

                entity.Property(e => e.TicketId).HasColumnName("TicketID");

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<TblUser>(entity =>
            {
                entity.ToTable("tblUsers");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

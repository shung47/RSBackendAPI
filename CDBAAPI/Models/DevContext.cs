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

        public virtual DbSet<TblTicket> TblTickets { get; set; }
        public virtual DbSet<TblTicketComment> TblTicketComments { get; set; }
        public virtual DbSet<TblTicketDbcontrol> TblTicketDbcontrols { get; set; }
        public virtual DbSet<TblTicketLog> TblTicketLogs { get; set; }
        public virtual DbSet<TblTicketLoginInfo> TblTicketLoginInfos { get; set; }
        public virtual DbSet<TblTicketModifiedTable> TblTicketModifiedTables { get; set; }
        public virtual DbSet<TblTicketReviewList> TblTicketReviewLists { get; set; }
        public virtual DbSet<TblTicketTask> TblTicketTasks { get; set; }
        public virtual DbSet<TblTicketUser> TblTicketUsers { get; set; }

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

                entity.Property(e => e.CreatorId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

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

                entity.Property(e => e.NotificationList)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryCodeReviewer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SaleaderRequired).HasColumnName("SALeaderRequired");

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

            modelBuilder.Entity<TblTicketComment>(entity =>
            {
                entity.ToTable("tblTicket_Comments");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CommentContent)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDateTime).HasColumnType("datetime");

                entity.Property(e => e.Creator)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.CreatorId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDateTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<TblTicketDbcontrol>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tblTicket_DBControl");

                entity.Property(e => e.Database)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Samaster)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("SAMaster");
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

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModificationDatetime).HasColumnType("datetime");

                entity.Property(e => e.TicketId).HasColumnName("TicketID");
            });

            modelBuilder.Entity<TblTicketLoginInfo>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tblTicket_LoginInfo");

                entity.Property(e => e.CanCreateTask)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.GeneralAlert)
                    .HasMaxLength(1)
                    .IsFixedLength(true);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .HasColumnName("ID");

                entity.Property(e => e.Inactive)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.LoginName)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PersonalEmail)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("Personal Email");

                entity.Property(e => e.Samaster)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("SAMaster");

                entity.Property(e => e.Team)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTicketModifiedTable>(entity =>
            {
                entity.ToTable("tblTicket_ModifiedTables");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateDateTime).HasColumnType("datetime");

                entity.Property(e => e.Creator)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.CreatorId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.DatabaseName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDateTime).HasColumnType("datetime");

                entity.Property(e => e.Summary)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTicketReviewList>(entity =>
            {
                entity.ToTable("tblTicket_ReviewList");

                entity.Property(e => e.Answers)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.ReviewType)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Reviewer)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.ReviewerId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TblTicketTask>(entity =>
            {
                entity.ToTable("tblTicket_Tasks");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");

                entity.Property(e => e.Creator)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.CreatorId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Functions)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastModificationDateTime).HasColumnType("datetime");

                entity.Property(e => e.Priority)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Region)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
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

            modelBuilder.Entity<TblTicketUser>(entity =>
            {
                entity.ToTable("tblTicket_Users");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Team)
                    .HasMaxLength(25)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

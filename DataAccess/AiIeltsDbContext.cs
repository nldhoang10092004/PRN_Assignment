using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class AiIeltsDbContext : DbContext
{
    public AiIeltsDbContext()
    {
    }

    public AiIeltsDbContext(DbContextOptions<AiIeltsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Apikey> Apikeys { get; set; }

    public virtual DbSet<SpeakingAnswer> SpeakingAnswers { get; set; }

    public virtual DbSet<SpeakingQuestion> SpeakingQuestions { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    public virtual DbSet<WritingAnswer> WritingAnswers { get; set; }

    public virtual DbSet<WritingQuestion> WritingQuestions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DBDefault"));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Account__1788CC4C92099754");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E46DBD207A").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D1053411C8859F").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HashPass)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Apikey>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__APIKey__1788CC4C8DFCC2D3");

            entity.ToTable("APIKey");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.ChatGptkey)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ChatGPTKey");
            entity.Property(e => e.DeepgramKey)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.Apikey)
                .HasForeignKey<Apikey>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_APIKey_Account");
        });

        modelBuilder.Entity<SpeakingAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Speaking__3214EC0740075B13");

            entity.ToTable("SpeakingAnswer");

            entity.Property(e => e.AudioUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("AudioURL");
            entity.Property(e => e.Grade).HasColumnType("decimal(4, 1)");

            entity.HasOne(d => d.Question).WithMany(p => p.SpeakingAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SAnswer_Question");
        });

        modelBuilder.Entity<SpeakingQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Speaking__0DC06FACECF87D4F");

            entity.ToTable("SpeakingQuestion");
        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserDeta__1788CC4CA118B9BB");

            entity.ToTable("UserDetail");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("AvatarURL");
            entity.Property(e => e.FullName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.UserDetail)
                .HasForeignKey<UserDetail>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserDetail_Account");
        });

        modelBuilder.Entity<WritingAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WritingA__3214EC078F04F662");

            entity.ToTable("WritingAnswer");

            entity.Property(e => e.Grade).HasColumnType("decimal(4, 1)");

            entity.HasOne(d => d.Question).WithMany(p => p.WritingAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WAnswer_Question");
        });

        modelBuilder.Entity<WritingQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WritingQ__3214EC074E0B8649");

            entity.ToTable("WritingQuestion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

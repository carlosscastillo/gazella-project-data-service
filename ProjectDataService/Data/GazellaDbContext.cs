using Microsoft.EntityFrameworkCore;
using ProjectDataService.Entities;

namespace ProjectDataService.Data;

public class GazellaDbContext(DbContextOptions<GazellaDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects { get; init; }
    public DbSet<Enrollment> Enrollments { get; init; }
    public DbSet<Category> Categories { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                  .HasMaxLength(EntitySizeConstraints.IdLength)
                  .HasColumnName("id");

            entity.Property(p => p.Title)
                  .HasMaxLength(EntitySizeConstraints.ProjectTitleMaxLength)
                  .HasColumnName("title")
                  .IsRequired();

            entity.Property(p => p.Description)
                  .HasMaxLength(EntitySizeConstraints.ProjectDescriptionMaxLength)
                  .HasColumnName("description")
                  .IsRequired();

            entity.Property(p => p.CoverUri)
                  .HasMaxLength(EntitySizeConstraints.ProjectCoverUriMaxLength)
                  .HasColumnName("cover_uri");

            entity.Property(p => p.Location)
                  .HasMaxLength(EntitySizeConstraints.ProjectLocationMaxLength)
                  .HasColumnName("location")
                  .IsRequired();

            entity.Property(p => p.Category)
                  .HasMaxLength(EntitySizeConstraints.ProjectCategoryMaxLength)
                  .HasColumnName("category")
                  .IsRequired();

            entity.Property(p => p.OrganizerId)
                  .HasMaxLength(EntitySizeConstraints.OrganizerIdMaxLength)
                  .HasColumnName("organizer_id")
                  .IsRequired();

            entity.Property(p => p.OrganizerName)
                  .HasMaxLength(EntitySizeConstraints.OrganizerNameMaxLength)
                  .HasColumnName("organizer_name")
                  .IsRequired();

            entity.Property(p => p.OrganizerPfpUri)
                  .HasMaxLength(EntitySizeConstraints.OrganizerPfpUriMaxLength)
                  .HasColumnName("organizer_pfp_uri");

            entity.Property(p => p.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(p => p.EndDate).HasColumnName("end_date").IsRequired();
            entity.Property(p => p.MaxVolunteers).HasColumnName("max_volunteers").IsRequired();
            entity.Property(p => p.EnrolledCount).HasColumnName("enrolled_count").IsRequired();

            entity.Property(p => p.Status)
                  .HasConversion<string>()
                  .HasColumnName("status")
                  .IsConcurrencyToken();

            entity.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");

            entity.HasMany(p => p.Enrollments)
                  .WithOne(e => e.Project)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasMaxLength(EntitySizeConstraints.IdLength)
                  .HasColumnName("id");

            entity.Property(e => e.ProjectId)
                  .HasMaxLength(EntitySizeConstraints.IdLength)
                  .HasColumnName("project_id")
                  .IsRequired();

            entity.Property(e => e.VolunteerId)
                  .HasMaxLength(EntitySizeConstraints.VolunteerIdMaxLength)
                  .HasColumnName("volunteer_id")
                  .IsRequired();

            entity.Property(e => e.VolunteerFullName)
                  .HasMaxLength(EntitySizeConstraints.VolunteerFullNameMaxLength)
                  .HasColumnName("volunteer_full_name")
                  .IsRequired();

            entity.Property(e => e.VolunteerEmail)
                  .HasMaxLength(EntitySizeConstraints.VolunteerEmailMaxLength)
                  .HasColumnName("volunteer_email")
                  .IsRequired();

            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasColumnName("status")
                  .IsRequired();

            entity.Property(e => e.EnrolledAt)
                  .HasColumnName("enrolled_at")
                  .IsRequired();

            entity.HasIndex(e => new { e.ProjectId, e.VolunteerId })
                  .IsUnique()
                  .HasDatabaseName("ix_enrollments_project_volunteer");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                  .HasMaxLength(EntitySizeConstraints.IdLength)
                  .HasColumnName("id");

            entity.Property(c => c.Name)
                  .HasMaxLength(EntitySizeConstraints.CategoryNameMaxLength)
                  .HasColumnName("name")
                  .IsRequired();
        });
    }
}
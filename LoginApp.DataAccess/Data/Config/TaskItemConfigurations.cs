using LoginApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoginApp.DataAccess.Data.Config
{
    public class TaskItemConfigurations : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            // Table name (optional, defaults to "Users")
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);
            builder.Property(p => p.Title).HasMaxLength(300)
                .IsRequired();

            builder.HasKey(t => t.Id);
            builder.Property(p => p.Description).HasMaxLength(3000)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
               .IsRequired();

            // Relationships
            builder.HasOne(t => t.User)
                   .WithMany(u => u.Tasks)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete                //when a User is deleted
                   (DeleteBehavior.Cascade);// delete all related tasks automatically

            builder.HasOne(t => t.Status)
                   .WithMany(s => s.Tasks)
                   .HasForeignKey(t => t.TaskStatusId)
                   .OnDelete(DeleteBehavior.Restrict);//you cannot delete a status if tasks are using it



            builder.HasQueryFilter(t => t.IsDeleted == false); //where IsDeleted = false


        }

    }


}

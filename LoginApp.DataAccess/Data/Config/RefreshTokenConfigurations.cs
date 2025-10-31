using LoginApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoginApp.DataAccess.Data.Config
{
    public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
 
            builder.ToTable("RefreshTokens");

            builder.HasKey(T => T.Id);


            builder.HasIndex(rt => new { rt.ExpiresDate, rt.IsCanceled }); // set index for deleting inActive tokens fro preformance


            builder.HasOne(rt => rt.User)
                .WithMany(u=>u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade); // delete all related tasks automatically

        }


    }
}

using LoginApp.DataAccess.Data.Interceptors;
using LoginApp.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskStatus = LoginApp.DataAccess.Entities.TaskStatus;

namespace LoginApp.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        private readonly SoftDeleteInterceptor _SoftDeleteInterseptor;

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskStatus> TasksStatus { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options , SoftDeleteInterceptor? SoftDeleteInterseptor = null)
            : base(options)
        {
            _SoftDeleteInterseptor = SoftDeleteInterseptor!;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.AddInterceptors(_SoftDeleteInterseptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}

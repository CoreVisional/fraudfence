using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FraudFence.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly IConfiguration _configuration;

        public DesignTimeDbContextFactory()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(@Directory.GetCurrentDirectory() + "/../FraudFence.Web/appsettings.json").Build();
        }

        public DesignTimeDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new ApplicationDbContext(builder.Options);
        }

        public ApplicationDbContext CreateDbContext(string[] args) => CreateDbContext();
    }
}

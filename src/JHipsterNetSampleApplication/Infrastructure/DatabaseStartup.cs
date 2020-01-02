using System;
using JHipsterNetSampleApplication.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JHipsterNetSampleApplication.Infrastructure {
    public static class DatabaseConfiguration {
        public static IServiceCollection AddDatabaseModule(this IServiceCollection @this, IConfiguration configuration)
        {
            var entityFrameworkConfiguration = configuration.GetSection("EntityFramework");
            var connection = new SqliteConnection(new SqliteConnectionStringBuilder {
                DataSource = entityFrameworkConfiguration["DataSource"]
            }.ToString());

            @this.AddDbContext<ApplicationDatabaseContext>(options => { options.UseSqlite(connection); });

            return @this;
        }

        public static IApplicationBuilder UseApplicationDatabase(this IApplicationBuilder @this,
            IServiceProvider serviceProvider, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment()) {
                @this.UseDatabaseErrorPage();
            }

            if (environment.IsDevelopment() || environment.IsProduction()) {
                var context = serviceProvider.GetRequiredService<ApplicationDatabaseContext>();
                context.Database.OpenConnection();
                context.Database.EnsureCreated();
            }

            return @this;
        }
    }
}

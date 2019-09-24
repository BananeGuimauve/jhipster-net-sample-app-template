using System;
using JHipsterNetSampleApplication.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JHipsterNetSampleApplication.Infrastructure {
    public static class DatabaseConfiguration {
        public static IServiceCollection AddDatabaseModule(this IServiceCollection @this, IConfiguration configuration,
            IHostingEnvironment environment)
        {
            var entityFrameworkConfiguration = configuration.GetSection("EntityFramework");
            if (environment.IsDevelopment()) {
                var connection = new SqliteConnection(new SqliteConnectionStringBuilder {
                        DataSource = entityFrameworkConfiguration["DataSource"]
                    }.ToString()
                );

                @this.AddDbContext<ApplicationDatabaseContext>(context => { context.UseSqlite(connection); });
            } else if (environment.IsProduction()) {
                var host = entityFrameworkConfiguration["Host"];
                var database = entityFrameworkConfiguration["Database"];
                var username = entityFrameworkConfiguration["Username"];
                var password = entityFrameworkConfiguration["Password"];
                @this.AddDbContext<ApplicationDatabaseContext>(context => {
                    context.UseNpgsql($"Host={host};Database={database};Username={username};Password={password}");
                });
            }

            return @this;
        }

        public static IApplicationBuilder UseApplicationDatabase(this IApplicationBuilder @this,
            IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDatabaseContext>();
            context.Database.OpenConnection();
            var dbCreated = context.Database.EnsureCreated();
            // When creating the database, insert users and roles
            if (dbCreated) {
                @this.UseApplicationIdentity(serviceProvider);
            }

            return @this;
        }
    }
}

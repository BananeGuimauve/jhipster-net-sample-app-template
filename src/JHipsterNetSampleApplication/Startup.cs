using System;
using JHipsterNet.Config;
using JHipsterNetSampleApplication.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

[assembly: ApiController]

namespace JHipsterNetSampleApplication
{
    public sealed class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddNhipsterModule(Configuration)
                .AddDatabaseModule(Configuration)
                .AddSecurityModule()
                .AddProblemDetailsModule()
                .AddAutoMapperModule()
                .AddWebModule()
                .AddSwaggerModule();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider,
            IOptions<JHipsterSettings> jhipsterSettingsOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            var jhipsterSettings = jhipsterSettingsOptions.Value;
            app
                .UseApplicationSwagger()
                .UseRouting()
                .UseApplicationSecurity(jhipsterSettings, env)
                .UseApplicationProblemDetails()
                .UseApplicationWeb()
                .UseApplicationDatabase(serviceProvider, env)
                .UseApplicationIdentity(serviceProvider);
        }
    }
}

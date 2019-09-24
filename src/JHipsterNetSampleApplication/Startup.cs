using System;
using JHipsterNet.Config;
using JHipsterNetSampleApplication.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

[assembly: ApiController]

namespace JHipsterNetSampleApplication {
    public sealed class Startup {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddNhipsterModule(Configuration)
                .AddDatabaseModule(Configuration, Environment)
                .AddSecurityModule()
                .AddProblemDetailsModule()
                .AddAutoMapperModule()
                .AddWebModule()
                .AddSwaggerModule()
                .AddMvc().AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider,
            IOptions<JHipsterSettings> jhipsterSettingsOptions)
        {
            var jhipsterSettings = jhipsterSettingsOptions.Value;
            app
                .UseApplicationSecurity(jhipsterSettings)
                .UseApplicationProblemDetails()
                .UseApplicationWeb(Environment)
                .UseApplicationSwagger()
                .UseApplicationDatabase(serviceProvider);
        }
    }
}

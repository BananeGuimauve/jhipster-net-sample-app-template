using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using JHipsterNetSampleApplication.Infrastructure;
using JHipsterNetSampleApplication.Security;
using JHipsterNetSampleApplication.Test.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JHipsterNetSampleApplication.Test.Setup {
    public class NhipsterWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class {
        private IServiceProvider _serviceProvider;
        private ClaimsPrincipal User { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => {
                services
                    .AddMvc(TestMvcStartup.ConfigureMvcAuthorization());
                services.Replace(new ServiceDescriptor(typeof(IHttpContextFactory), typeof(MockHttpContextFactory),
                    ServiceLifetime.Transient));
                services.AddTransient(sp => new MockClaimsPrincipalProvider(User));
            });
        }

        public TService GetRequiredService<TService>()
        {
            if (_serviceProvider == null) _serviceProvider = Services.CreateScope().ServiceProvider;

            return _serviceProvider.GetRequiredService<TService>();
        }

        public NhipsterWebApplicationFactory<TEntryPoint> WithMockUser(string name = "user",
            IEnumerable<string> roles = null, string authenticationType = "MockAuthenticationType")
        {
            User = BuildClaimsPrincipal(name, roles, authenticationType);
            return this;
        }

        private static ClaimsPrincipal BuildClaimsPrincipal(string name, IEnumerable<string> roles,
            string authenticationType)
        {
            if (roles == null || !roles.Any()) roles = new HashSet<string> {RolesConstants.USER};

            var claims = new List<Claim> {new Claim(SecurityStartup.UserNameClaimType, name)};
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            return new ClaimsPrincipal(new ClaimsIdentity(claims.ToArray(), authenticationType));
        }
    }
}

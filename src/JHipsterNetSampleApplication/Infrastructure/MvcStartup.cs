using JHipsterNet.Pagination.Binders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JHipsterNetSampleApplication.Infrastructure
{
    public static class WebConfiguration
    {
        public static IServiceCollection AddWebModule(this IServiceCollection @this)
        {
            @this.AddHttpContextAccessor();

            //https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.0
            @this.AddHealthChecks();

            //TODO use AddMvcCore + config
            @this.AddMvc(options => { options.ModelBinderProviders.Insert(0, new PageableBinderProvider()); /*options.ModelBinderProviders.Insert(0, new PageableBinderProvider());*/ })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            return @this;
        }

        public static IApplicationBuilder UseApplicationWeb(this IApplicationBuilder @this)
        {
            @this.UseDefaultFiles();
            @this.UseStaticFiles();
            @this.UseCookiePolicy();

            @this.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            return @this;
        }
    }
}

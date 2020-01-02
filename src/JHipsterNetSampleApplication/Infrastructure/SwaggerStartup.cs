using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace JHipsterNetSampleApplication.Infrastructure {
    public static class SwaggerConfiguration {
        public static IServiceCollection AddSwaggerModule(this IServiceCollection @this)
        {
            @this.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "NhipsterSampleApplication API", Version = "v0.0.1"});
            });

            return @this;
        }

        public static IApplicationBuilder UseApplicationSwagger(this IApplicationBuilder @this)
        {
            @this.UseSwagger();
            @this.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "NhipsterSampleApplication API");
            });
            return @this;
        }
    }
}

using System;
using System.Diagnostics;
using System.Security.Authentication;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace JHipsterNetSampleApplication.Web.Rest.Problems {
    public class ProblemDetailsConfiguration : IConfigureOptions<ProblemDetailsOptions> {
        public ProblemDetailsConfiguration(IWebHostEnvironment env, IHttpContextAccessor httpContextAcc)
        {
            Environment = env;
            HttpContextAccessor = httpContextAcc;
        }

        private IWebHostEnvironment Environment { get; }
        private IHttpContextAccessor HttpContextAccessor { get; }

        public void Configure(ProblemDetailsOptions options)
        {
            options.IncludeExceptionDetails = ctx => Environment.IsDevelopment();

            options.OnBeforeWriteDetails = (ctx, details) => {
                // keep consistent with asp.net core 2.2 conventions that adds a tracing value
                var traceId = Activity.Current?.Id ?? HttpContextAccessor.HttpContext.TraceIdentifier;
                details.Extensions["traceId"] = traceId;
            };

            options.Map<AuthenticationException>(exception =>
                new ExceptionProblemDetails(exception, StatusCodes.Status401Unauthorized));
            options.Map<NotImplementedException>(exception =>
                new ExceptionProblemDetails(exception, StatusCodes.Status501NotImplemented));

            //TODO add Headers to HTTP responses
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;

namespace JHipsterNetSampleApplication.Test.Setup {
    public class MockHttpContextFactory : IHttpContextFactory {
        private readonly DefaultHttpContextFactory _delegate;
        private readonly MockClaimsPrincipalProvider _mockClaimsPrincipalProvider;

        public MockHttpContextFactory(IServiceProvider serviceProvider, MockClaimsPrincipalProvider mockClaimsPrincipalProvider)
        {
            _delegate = new DefaultHttpContextFactory(serviceProvider);
            _mockClaimsPrincipalProvider = mockClaimsPrincipalProvider;
        }

        /*public MockHttpContextFactory(IOptions<FormOptions> formOptions, IHttpContextAccessor httpContextAccessor,
            MockClaimsPrincipalProvider mockClaimsPrincipalProvider)
        {
            _delegate = new DefaultHttpContextFactory(formOptions, httpContextAccessor);
            _mockClaimsPrincipalProvider = mockClaimsPrincipalProvider;
        }*/

        public HttpContext Create(IFeatureCollection featureCollection)
        {
            var httpContext = _delegate.Create(featureCollection);
            httpContext.User = _mockClaimsPrincipalProvider.User;
            return httpContext;
        }

        public void Dispose(HttpContext httpContext)
        {
            _delegate.Dispose(httpContext);
        }
    }
}

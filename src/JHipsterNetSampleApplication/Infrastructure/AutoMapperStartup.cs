using AutoMapper;
using JHipsterNetSampleApplication.Service.Mapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace JHipsterNetSampleApplication.Infrastructure {
    public static class AutoMapperStartup {
        public static IServiceCollection AddAutoMapperModule(this IServiceCollection @this)
        {
            @this.AddAutoMapper(new Type[] { typeof(UserProfile) });
            return @this;
        }
    }
}

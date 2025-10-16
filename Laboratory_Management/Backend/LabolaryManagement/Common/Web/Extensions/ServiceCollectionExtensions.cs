using Common.Web.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IMvcBuilder AddStandardApi(this IServiceCollection services)
        {
            return services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilter>();
                options.Filters.Add<ResponseWrapperFilter>();
            });
        }
    }
}

namespace OnlineBankingSystem.Extensions
{
    public static class OpenApiExtenstions
    {
        public static IServiceCollection AddOpenApiServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }
    }
}

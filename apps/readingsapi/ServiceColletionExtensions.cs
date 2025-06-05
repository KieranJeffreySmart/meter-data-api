namespace readingsapi;

public static class ServiceColletionExtensions
{    
    public static void AddLoggingDecorator<TAbstraction, TInner>(this IServiceCollection services, Func<TInner, ILogger<TAbstraction>, TAbstraction> decoratorFactory)
        where TInner : class
        where TAbstraction : class
    {
        services.AddScoped(provider =>
        {
            var inner = provider.GetRequiredService<TInner>();
            var logger = provider.GetRequiredService<ILogger<TAbstraction>>();
            return decoratorFactory(inner, logger);
        });
    }
}

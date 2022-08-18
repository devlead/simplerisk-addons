namespace API;

public static class Bootstrapper
{
    public static void Register(IServiceCollection services, IConfiguration config)
    {
        LoggingBootstrapper.RegisterLogging();
        ServicesBootstrapper.RegisterServices(services, config);
        AuthenticationBootstrapper.RegisterAuthentication(services, config);
    }
}
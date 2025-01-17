﻿using Microsoft.Extensions.Configuration;
using Splat;
using GUIClient.Configuration;
using GUIClient.Models;
using GUIClient.Services;

namespace GUIClient;

public static  class ConfigurationBootstrapper
{
    
    public static void RegisterConfiguration(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var configuration = BuildConfiguration();

        RegisterConfiguration(services, configuration);
        RegisterLoggingConfiguration(services, configuration);
        RegisterLanguagesConfiguration(services, configuration);
        RegisterServerConfiguration(services, configuration);
        RegisterMutableConfiguration(services, configuration);

    }
    
    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

    private static void RegisterMutableConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        services.RegisterLazySingleton<IMutableConfigurationService>(() => new MutableConfigurationService(GetService<IEnvironmentService>()));
    }
    

    private static void RegisterConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        services.RegisterConstant(configuration);
    }
    
    private static void RegisterServerConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        var config = new ServerConfiguration();
        configuration.GetSection("Server").Bind(config);
        services.RegisterConstant(config);
    }
    
    private static void RegisterLoggingConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        var config = new LoggingConfiguration();
        configuration.GetSection("Logging").Bind(config);
        services.RegisterConstant(config);
    }
    
    private static void RegisterLanguagesConfiguration(IMutableDependencyResolver services,
        IConfiguration configuration)
    {
        var config = new LanguagesConfiguration();
        configuration.GetSection("Languages").Bind(config);
        services.RegisterConstant(config);
    }
    
    private static T GetService<T>() => Locator.Current.GetService<T>();

}
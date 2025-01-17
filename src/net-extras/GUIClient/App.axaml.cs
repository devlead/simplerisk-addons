using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GUIClient.Services;
using GUIClient.ViewModels;
using GUIClient.Views;
using Splat;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Model.Statistics;

namespace GUIClient
{
    public partial class App : Application
    {

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            
            LiveCharts.Configure(config => 
                    config 
                        // registers SkiaSharp as the library backend
                        // REQUIRED unless you build your own
                        .AddSkiaSharp() 
                        
                        // adds the default supported types
                        // OPTIONAL but highly recommend
                        .AddDefaultMappers() 

                        // select a theme, default is Light
                        // OPTIONAL
                        .AddDarkTheme()
                        //.AddLightTheme() 

                        // finally register your own mappers
                        // you can learn more about mappers at:
                        // ToDo add website link...
                        .HasMap<RisksOnDay>((risks, point) =>
                        {
                            point.PrimaryValue = risks.RisksCreated;
                            //point.SecondaryValue = point.Context.Index;
                            point.SecondaryValue = risks.Day.Day;
                            
                        })
            );
            
            
        }

        public override void OnFrameworkInitializationCompleted()
        {
            
            var mutableConfigurationService = GetService<IMutableConfigurationService>();
            mutableConfigurationService.Initialize();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

           
            base.OnFrameworkInitializationCompleted();
        }
        private static T GetService<T>() => Locator.Current.GetService<T>();
    }
}
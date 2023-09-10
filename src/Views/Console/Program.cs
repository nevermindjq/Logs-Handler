using Autofac;
using Console.Models;
using Console.Views;
using Core.Models.Logs;
using Core.View;
using Core.View.Models.Abstractions;
using Microsoft.Extensions.Configuration;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Splat;

namespace Console;

public static class Program
{
    public static async Task Main()
    {
        System.Console.Title = "https://github.com/f0ll1253";
        
        await App.Initialize(builder =>
        {
            builder.RegisterServices();
            builder.RegisterViews();
        });
        await App.Run(Locator.Current.GetService<StartView>()!);
    }

    private static void RegisterServices(this ContainerBuilder builder)
    {
        builder.RegisterType<LogsInfo>()
            .SingleInstance();
        builder.RegisterType<RedlineFactory>()
            .SingleInstance();

        builder.Register(ctx =>
            {
                var cfg = ctx.Resolve<IConfiguration>();
                
                return new Web3(cfg["Web3:Ethereum"]);
            })
            .Named<Web3>("Ethereum")
            .SingleInstance();
    }
    
    private static void RegisterViews(this ContainerBuilder builder)
    {
        builder.RegisterType<Root>()
            .As<IRoot>()
            .SingleInstance();

        builder.RegisterType<StartView>();
        builder.RegisterType<MainView>();
    }
}
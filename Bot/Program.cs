using System.Reflection;

using Bot.Data;
using Bot.Models.Users;
using Bot.Services.Hosted;

using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

namespace Bot;

internal static class Program {
    public static void Main(string[] args) {
        Directory.CreateDirectory(Directory_Session);
        Directory.CreateDirectory(Directory_Extracted);
        Directory.CreateDirectory(Directory_Downloaded);
        
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSerilog(config => config
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console()
        );

        builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

        // Instances
        builder.Services.AddActivatedSingleton(_ => 
            new Client(
                int.Parse(builder.Configuration["Bot:ApiId"]!), 
                builder.Configuration["Bot:ApiHash"]!, 
                Path.Combine(Directory_Session, ".session")
            )
        );

        builder.Services.AddDbContext<UsersDbContext>(options => options
            .UseSqlite(builder.Configuration["ConnectionStrings:Users"])
        );

        builder.Services.AddDbContext<DataDbContext>(options => options
            .UseSqlite(builder.Configuration["ConnectionStrings:Data"])
        );

        // Services
        builder.Services.AddHostedService<Bootstrapper>();
        
        // Injectio
        builder.Services.AddBot();
        builder.Services.AddBotParsers();

        var app = builder.Build();

        app.ConfigureLogging();
        app.InitializeContext<UsersDbContext>();
        app.InitializeContext<DataDbContext>();

        app.Run();
    }
    
    public static void ConfigureLogging(this IHost app) {
        Helpers.Log = (lvl, str) => Log.Write((LogEventLevel)lvl, str);
    }

    public static void InitializeContext<TContext>(this IHost app) where TContext : DbContext {
        var context = app.Services.GetRequiredService<TContext>();
        
        context.Database.EnsureCreated();

        switch (context) {
            case UsersDbContext:
                app._InitializeUsers(context);
                break;
        }
    }

    private static void _InitializeUsers(this IHost app, DbContext context) {
        if (context.Set<ApplicationUser>().Any()) {
            return;
        }
        
        var config = app.Services.GetRequiredService<IConfiguration>();
            
        foreach (var id in config.GetRequiredSection("Bot:Admins").Get<List<long>>()) {
            context.Add(new ApplicationUser()
            {
                Id = id,
                Roles = ["Admin"]
            });
        }

        context.SaveChanges();
    }
}
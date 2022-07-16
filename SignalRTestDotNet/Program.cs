using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // CreateDbIfNotExists(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<GameContext>();
                    context.Database.EnsureCreated();
                    context.Sessions.Add(new Session{AdminId="something", SessionId="sessionId"});
                    context.Players.Add(new Player{PlayerId="playerId", SessionId="sessionId", Url="someurl", Country="france"});
                    context.SaveChanges();

                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred creating the DB: {e}");
                }
            }
        }
    }
}

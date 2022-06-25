using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRTestDotNet.Hubs;
using SignalRTestDotNet.Middleware;
using Microsoft.EntityFrameworkCore;


namespace SignalRTestDotNet
{
    public class Startup
    {
        private const string ClientUri = "http://localhost:63343";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(host => true)
                    .AllowCredentials();
            }));
            services.AddSignalR();
            services.AddDbContext<GameContextNs.GameContext>(options =>
                    options.UseNpgsql("Host=localhost:5432;Username=myusername;Password=mypassword;Database=myusername"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseMiddleware<AdminStartMiddleware>();

            app.UseCors();


            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/hub");
                endpoints.MapControllers();
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });

            });
        }
    }
}

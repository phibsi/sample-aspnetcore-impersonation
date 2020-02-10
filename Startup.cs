using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sample.AspNetCore.Impersonation.Services;
using Sample.AspNetCore.Impersonation.Settings;

namespace Sample.AspNetCore.Impersonation
{
    public class Startup
    {
        // This method gets called in Program.cs. Use this method to add settings to the container.
        public static void ConfigureSettings(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            services.Configure<AppSettings>(options => configuration.Bind(options));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ImpersonationService>();

            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}

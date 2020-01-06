using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.AspNetCore.Impersonation.Services;
using Sample.AspNetCore.Impersonation.Settings;

namespace Sample.AspNetCore.Impersonation
{
    public class Startup
    {
        // This method gets called in Program.cs. Use this method to add settings to the container.
        public static void ConfigureSettings(WebHostBuilderContext context, IServiceCollection services)
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
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}

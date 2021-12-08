using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Blessed_Party.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Blessed_Party
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages(options => {
                options.Conventions.AuthorizePage("/Admin/SalesDashboard");
                options.Conventions.AuthorizePage("/Admin/Master_Users");
                options.Conventions.AuthorizePage("/Admin/Master_Products");
                options.Conventions.AuthorizePage("/Admin/Master_Orders");
                options.Conventions.AuthorizePage("/Admin/Order_Detail");
                options.Conventions.AuthorizePage("/Admin/Master_Carts");
                options.Conventions.AuthorizePage("/Admin/Master_RatingProducts");
                options.Conventions.AuthorizePage("/Admin/Master_Shipments");
                options.Conventions.AuthorizePage("/Admin/Master_Reports");
                options.Conventions.AuthorizePage("/Cart");
                options.Conventions.AuthorizePage("/HistoryOrder");
                options.Conventions.AuthorizePage("/OrderAndPayment");
                options.Conventions.AuthorizePage("/ProfileSettings");
                options.Conventions.AuthorizePage("/ChangePassword");
                options.Conventions.AuthorizePage("/Logout");
            });
            services.AddControllers().AddNewtonsoftJson();
            services.AddDistributedMemoryCache();
            services.AddHttpContextAccessor();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie("Cookies", options =>
            {
                options.EventsType = typeof(CookieAuthenticationEvents);
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
            });

            services.AddScoped<CookieAuthenticationEvents>();

            services.AddDbContext<BPartyContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("BPartyContext")));

            services.AddScoped<BPartyContext>();

            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error404");
            }

            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            };

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCookiePolicy(cookiePolicyOptions);
            //app.UseSession();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}

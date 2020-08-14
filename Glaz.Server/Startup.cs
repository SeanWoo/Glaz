using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Glaz.Server.Data;
using Glaz.Server.Data.AppSettings;
using Glaz.Server.Entities;
using Glaz.Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Glaz.Server
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<GlazAccount, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.Cookie.Name = "GlazServer";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/Identity/Account/Login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services.AddLogging();
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddOptions();
            services.Configure<EmailSenderOptions>(Configuration.GetSection("EmailSender"));
            services.Configure<VuforiaCredentials>(Configuration.GetSection("VuforiaCredentials"));

            services.AddScoped<IVuforiaService, VuforiaService>();
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            UserManager<GlazAccount> userManager, RoleManager<IdentityRole> roleManager)
        {
            var dbInitializer = new DatabaseInitializer(roleManager, userManager);
            dbInitializer.SeedRoles();
            dbInitializer.SeedUserAccounts();

            CreateServerDirectoriesIfNotExist(env);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
        private void CreateServerDirectoriesIfNotExist(IWebHostEnvironment env)
        {
            var rootDirectory = env.WebRootPath;
            const string attachmentsDirectory = "Attachments";
            const string videosDirectory = "Videos";
            var targetsDirectory = Path.Combine(attachmentsDirectory, "Targets");
            var responseFilesDirectory = Path.Combine(attachmentsDirectory, "ResponseFiles");
            var bundlesDirectory = Path.Combine(attachmentsDirectory, "Bundles");
            CreateDirectories(
                Path.Combine(rootDirectory, targetsDirectory),
                Path.Combine(rootDirectory, responseFilesDirectory),
                Path.Combine(rootDirectory, bundlesDirectory),
                Path.Combine(rootDirectory, videosDirectory));
        }
        private void CreateDirectories(params string[] paths)
        {
            foreach (var path in paths)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}

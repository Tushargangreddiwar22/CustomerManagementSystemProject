using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVCDHProject.Models;

namespace MVCDHProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            
            builder.Services.AddDbContext<MVCCoreDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ConStr")
            ));

            builder.Services.AddScoped<ICustomerDAL, CustomerSqlDAL>();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<MVCCoreDbContext>().AddDefaultTokenProviders();

            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                //Middleware's to handle client errors:
                //app.UseStatusCodePages();
                //app.UseStatusCodePagesWithRedirects("/ClientError/{0}");
                app.UseStatusCodePagesWithReExecute("/ClientError/{0}");

                //Middleware to handle server error:
                app.UseExceptionHandler("/Home/Error");
                app.UseExceptionHandler("/ServerError");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            //p.MapStaticAssets();
            app.UseStaticFiles();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

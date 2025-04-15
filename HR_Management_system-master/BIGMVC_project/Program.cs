using BIGMVC_project.Models;
using BIGMVC_project.Services;
using Microsoft.EntityFrameworkCore;

namespace BIGMVC_project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
			builder.Services.AddDbContext<MyDbContext>(options =>
	        options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectionString")));

			builder.Services.AddDistributedMemoryCache();

			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(20);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});
			builder.Services.AddScoped<EmailService>();
			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
			app.UseSession();

			app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Manager}/{action=contact}/{id?}");
            app.Run();
        }
    }
}

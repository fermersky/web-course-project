using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoApp.Business.Azure;
using TodoApp.Business.Repositories.Implementations;
using TodoApp.Business.Smtp.Abstract;
using TodoApp.Business.Smtp.Implementations;
using TodoApp.Business.Smtp.Models;
using TodoApp.Business.TodosSignalR;
using TodoApp.Entities;
using TodoApp.Entities.Models;

namespace TodoApp
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
            services.AddControllersWithViews();

            services.AddDbContext<TodoDbContext>(provider =>
            {
                provider.UseSqlServer(Configuration.GetConnectionString("LocalDbConnection"));
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = true)
                .AddEntityFrameworkStores<TodoDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<TodoRepository>();
            services.AddSignalR();

            services.AddAuthentication().AddGoogle(options => 
            {
                options.ClientId = Configuration["GoogleClientId"];
                options.ClientSecret = Configuration["GoogleClientSecret"];
            });

            services.AddTransient(_ =>
                new AzureBlobTodosClient(Configuration.GetConnectionString("AzureBlobConnection"), "todos"));

            var emailConfig = Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);

            services.AddTransient<IEmailSender, EmailSender>();
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
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<TodoHub>("/todohub");
            });
        }
    }
}

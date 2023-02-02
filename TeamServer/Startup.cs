using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TeamServer.Services;
using TeamServer.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SQLite;
using TeamServer.Services.Extra;
using TeamServer.Models.Database;

namespace TeamServer
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
            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamServer", Version = "v1" });
            });
            services.AddSingleton<ImanagerService, managerService>();
            services.AddSingleton<IEngineerService, EngineerService>();

            string sqliteConnectionString = DatabaseService.ConnectionString;


            services.AddTransient<IUserStore<UserInfo>, UserStore>();
            services.AddTransient<IRoleStore<RoleInfo>, RoleStore>();
            services.AddTransient<UserManager<UserInfo>, MyUserManager>();
            

            services.AddIdentity<UserInfo, RoleInfo>().AddDefaultTokenProviders();



            //add role-based authorization services
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"]
                };
            });

            Authentication.SignInManager = services.BuildServiceProvider().GetService<SignInManager<UserInfo>>();
            Authentication.UserManager = services.BuildServiceProvider().GetService<UserManager<UserInfo>>();
            Authentication.Configuration = Configuration;


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1"));
                Console.WriteLine("TeamServer is running in development mode.");
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<HardHatHub>("/HardHatHub");
            });
            LoggingService.Init();
            Console.WriteLine("Generating unique encryption keys for pathing and metadata id");
            Encryption.GenerateUniversialKeys();
            Console.WriteLine("Initiating SQLite server");
            DatabaseService.Init();
            DatabaseService.ConnectDb();
            DatabaseService.CreateTables();
            UsersRolesDatabaseService.CreateDefaultRoles();
            DatabaseService.FillTeamserverFromDatabase();
        }
    }
}

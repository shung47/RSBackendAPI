using CDBAAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;

namespace CDBAAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddControllers().AddNewtonsoftJson();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder.AllowAnyOrigin().AllowAnyHeader()
                                                  .AllowAnyMethod());
            });
            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));
            
            if(CurrentEnvironment.IsDevelopment())
            {
                services.AddDbContext<DevContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DevDatabase")));
            }else
            {
                services.AddDbContext<DevContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ProductionDatabase")));
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidIssuer = Configuration["Jwt:Issuer"],
                       ValidateAudience = true,
                       ValidAudience = Configuration["Jwt:Audience"],
                       ValidateLifetime = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:KEY"]))
                   };
               });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            //Order is matter
            app.UseRouting();
            app.UseCors();
            //app.UseCors(x => x
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .SetIsOriginAllowed(origin => true) // allow any origin
            //    .AllowCredentials());
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            //app.UseStatusCodePages(async context => {
            //    var request = context.HttpContext.Request;
            //    var response = context.HttpContext.Response;

            //    if (response.StatusCode == 304)
            //    {
            //        response.Redirect("/login");
            //       };
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SignalRApiForSql.DAL;
using SignalRApiForSql.Hubs;
using SignalRApiForSql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRApiForSql
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
            services.AddScoped<VisitorService>();
            services.AddSignalR();

            //AddPolicy metodu ile cors policy olu�turuldu. Bu policy a�a��da Configure i�erisinde �a�r�lmas� gerekmektedir.
            services.AddCors(options => options.AddPolicy("CorsPolicy", //Consume �zerinden server � t�ketmemize olanak sa�layan metot.
                builder =>
                {
                    builder.AllowAnyHeader()     //D��ar�dan herhangi bir ba�l���n gelmesine izin ver.
                    .AllowAnyMethod()      //D��ar�dan herhangi bir metodun gelmesine izin ver.
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials();
                }));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalRApiForSql", Version = "v1" });
            });

            services.AddDbContext<Context>(options =>
            {
                options.UseSqlServer(Configuration["DefaultConnection"]);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalRApiForSql v1"));
            }

            app.UseRouting();

            app.UseCors("CorsPolicy"); // Yukar�da olu�turdu�umuz corspolicy burada �a�r�ld�.

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<VisitorHub>("/VisitorHub");   //Nereyi t�ketece�imiz burada belirtilir.Syntax tam olarak bu �ekilde.
            });
        }
    }
}

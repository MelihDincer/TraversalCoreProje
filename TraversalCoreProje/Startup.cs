using BusinessLayer.Container;
using DataAccessLayer.Concrete;
using EntityLayer.Concrete;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TraversalCoreProje.CQRS.Handlers.DestinationHandlers;
using TraversalCoreProje.Models;

namespace TraversalCoreProje
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

            services.AddScoped<GetAllDestinationQueryHandler>();
            services.AddScoped<GetDestinationByIDQueryHandler>();
            services.AddScoped<CreateDestinationCommandHandler>();
            services.AddScoped<RemoveDestinationCommandHandler>();
            services.AddScoped<UpdateDestinationCommandHandler>();

            services.AddMediatR(typeof(Startup)); //MediatR projeye dahil edildi.
            
            services.AddDbContext<Context>(); //Proje seviyesinde Authentication uygulad�k.
            services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<Context>().AddErrorDescriber<CustomIdentityValidator>().AddEntityFrameworkStores<Context>(); //Identity yap�lanmas�n� eklemi� olduk. En sona ekledi�imiz AddErrorDescriber ise custom olarak olu�turdu�umuz identityvalidatorunu dahil etmek i�in.

            services.AddHttpClient(); //HttpClient projeye dahil edildi.

            services.ContainerDependencies(); //Business katman�nda Container klas�r� i�inde tan�mlad���m�z Extensions class i�erisindeki metoda direkt eri�im sa�lad�k.

            services.AddAutoMapper(typeof(Startup)); //AutoMapper dahil edildi.

            services.CustomerValidator(); //Extension i�erisindeki customervalidator u burada �a��rd�k. Yukar�daki containerdependencies gibi.

            services.AddControllersWithViews().AddFluentValidation(); //FluentValidation dahil edildi.

            services.AddMvc(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });
            
            services.AddLocalization(opt =>
            {
                opt.ResourcesPath = "Resources"; //Dil deste�inin eklenmesi k�sm�nda, dil resource dosyalar�n� "Resources" adl� klas�rde aramas� gerekti�ini belirttik.
            });

            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/SignIn/"; //Oturum d��t���nde/Cookie temizlendi�inde bu sayfaya y�nlendir.
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/ErrorPage/Error404", "?code={0}"); //Sayfa bulunamad���nda bu k�sma y�nlendir. Parametre alabilir.
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();

            app.UseAuthorization();
            //Burada, uygulama i�erisinde desteklenecek dillerin etiketini/suffix/�n ek leri burada belirttik.
            var suppertedCultures = new[] { "en", "fr", "es", "gr", "tr", "de" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(suppertedCultures[4]).AddSupportedCultures(suppertedCultures).AddSupportedUICultures(suppertedCultures); //Uygulamada ilgili sayfa aya�a kalkt���nda default olarak hangi dille aya�a kalkaca�� belirtildi. (tr) --- Ayr�ca son iki metod ile birlikte, backend ve UI k�sm�na ekleme i�lemi yap�ld�.
            app.UseRequestLocalization(localizationOptions);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Default}/{action=Index}/{id?}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                  name: "areas",
                  pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });

        }
    }
}

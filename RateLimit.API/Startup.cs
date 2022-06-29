using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
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

namespace RateLimit.API
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
            //Bu servis sayesinde appsettings.json içerisindeki key value çiftleri bir class üzerinden okuyabiliriz.
            services.AddOptions();

            //RateLimit kütüphanesi requestlerin sayýsýný cachede tuttuðundan default olarak gelmeyen memorycache servisini ekliyoruz.
            services.AddMemoryCache();

            //Ip adresleri ile ilgili izinleri belirtmemiz lazým.
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

            //Eðer uygulamamýzýn birden fazla instanceý ayaða kalkma durumu varsa distrubuted cache kullanýlmalý.
            //services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            //RateLimit kütüphanesi bir middleware olduðundan ve middleware olarak uygulamamýza bir katman
            //eklendiðindne dolayý, request ilk olarak ratelimit'in belirlemiþ olduðu bu katmana girecek.
            //Bu katman üzerinden requestin içerisindeki request yapanýn IP adresini, header bilgisini
            //okuyabilmek için HttpContextAccessor'u ekliyoruz ki gelen requestin içindeki datalarý okuyabilelim.
            //Default olarak gelmez!
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            //Ana servis
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RateLimit.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RateLimit.API v1"));
            }

            //Middleware
            app.UseIpRateLimiting();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

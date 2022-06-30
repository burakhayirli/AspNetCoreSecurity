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
            //Bu servis sayesinde appsettings.json i�erisindeki key value �iftleri bir class �zerinden okuyabiliriz.
            services.AddOptions();

            //RateLimit k�t�phanesi requestlerin say�s�n� cachede tuttu�undan default olarak gelmeyen memorycache servisini ekliyoruz.
            services.AddMemoryCache();

            //Ip adresleri ile ilgili izinleri belirtmemiz laz�m.
            //services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

            //Client bazl� kurallar vermek i�in
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();

            //E�er uygulamam�z�n birden fazla instance� aya�a kalkma durumu varsa distrubuted cache kullan�lmal�.
            //services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            //RateLimit k�t�phanesi bir middleware oldu�undan ve middleware olarak uygulamam�za bir katman
            //eklendi�indne dolay�, request ilk olarak ratelimit'in belirlemi� oldu�u bu katmana girecek.
            //Bu katman �zerinden requestin i�erisindeki request yapan�n IP adresini, header bilgisini
            //okuyabilmek i�in HttpContextAccessor'u ekliyoruz ki gelen requestin i�indeki datalar� okuyabilelim.
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

            //Ip Rate Limit Middleware
            //app.UseIpRateLimiting();

            //ClientId Rate Limit
            app.UseClientRateLimiting();

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

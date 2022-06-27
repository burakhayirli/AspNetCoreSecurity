using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cors.API
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
            #region Middleware Level CORS

            //Accept all request from everywhere
            //services.AddCors(opts => {
            //    opts.AddDefaultPolicy(builder =>
            //    {
            //        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            //    });
            //});


            //services.AddCors(opts =>
            //{
            //    //Accept specific domains with policy
            //    opts.AddPolicy("AllowSite",builder =>
            //    {
            //        builder.WithOrigins("https://localhost:44309", "https://www.burakhayirli.com")
            //        .AllowAnyHeader()
            //        .AllowAnyMethod();
            //    });

            //    //allow specific domain and header
            //    opts.AddPolicy("AllowSite2", builder =>
            //    {
            //        builder.WithOrigins("https://www.burakhayirli2.com").WithHeaders(HeaderNames.ContentType, "x-custom-header");
            //    });
            //});

            //services.AddCors(opts =>
            //{
            //    //Accept subdomains with policy
            //    opts.AddPolicy("AllowSubDomains", builder =>
            //    {
            //        //SetIsOriginAllowedToAllowWildcardSubdomains
            //        //subdomain ne olursa olsun bu siteden tüm istekleri kabul et.
            //        builder.WithOrigins("https://*.burakhayirli.com")
            //        .SetIsOriginAllowedToAllowWildcardSubdomains()
            //        .AllowAnyHeader()
            //        .AllowAnyMethod();
            //    });
            //});

            #endregion

            #region Controller and Action Level CORS
            //Controller veya Action seviyesindeki CORS uygularken middleware'e policy vermiyoruz. Sadece useCors() yeterli.
            services.AddCors(opts =>
            {
                //Accept subdomains with policy
                opts.AddPolicy("AllowSubDomains", builder =>
                {
                    //SetIsOriginAllowedToAllowWildcardSubdomains
                    //subdomain ne olursa olsun bu siteden tüm istekleri kabul et.
                    builder.WithOrigins("https://*.burakhayirli.com")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });

                opts.AddPolicy("AllowSiteActionLevel", builder =>
                {
                    builder.WithOrigins("https://localhost:44309").WithMethods("GET", "POST").AllowAnyHeader();
                });
            });

            //Gelen istekte kimlikle ilgili bilgileri kabul edip etmeyeceðimizi belirliyoruz.
            //Credential'a izin verirsek cookie ile birlikte kimlik bilgilerimiz de gelecek.
            //Ýstek yapýldýðý zaman browser bu bilgileri de göndersin istiyorsak kullanýrýz.
            //Credential'a izin verdiðimiz zaman, uygulama tarafýnda bir istek yapýldýðý zaman örneðin jquery ile, 
            //credential bilgilerinin gitmesi gerektiðini de ajax isteði ile ilgili browser'a söylememiz lazým.
            //.AllowCredentials()
            #endregion

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cors.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cors.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //Accept all request from everywhere
            //app.UseCors();

            //Accept specific domains with policy
            //app.UseCors("AllowSite");

            //allow specific domain and header
            //app.UseCors("AllowSite2");

            //Accept subdomains with policy
            //app.UseCors("AllowSubDomains");

            //Controller Level CORS. 
            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

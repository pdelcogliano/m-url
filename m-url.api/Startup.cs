using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using M_url.Data;
using M_url.Data.Repositories;

namespace M_url.Api
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
            services.AddControllers(setupAction => 
            {
                setupAction.ReturnHttpNotAcceptable = true;     // sends 406 for unsupported output formats, i.e. zip or other formats
            }).AddXmlDataContractSerializerFormatters();        // adds support XML output format if specified in the accept header by the client consumer

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "M_url.Api", Version = "v1" });
            });
            // use DI to create the singleton class responsible for storing data
            //services.AddSingleton<IDataAccess, MurlDataAccess>();
            string connectionString = Configuration["ConnectionStrings:MurlsDBConnectionString"];
            services.AddDbContext<MurlContext>(options =>
              options.UseSqlServer(connectionString, x => x.MigrationsAssembly("M-url.Data.Migrations")));
            services.AddScoped<IMurlRepository, MurlRepository>();

            services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "M_url.Api v1"));
            }
            app.UseHsts();
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

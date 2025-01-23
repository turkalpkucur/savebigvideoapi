using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using weasewebrtcsavebigfileapi.Models;

namespace weasewebrtcsavebigfileapi
{
    public class Startup
    {
        public IConfiguration _config { get; }
        public Startup(IConfiguration configuration, IConfiguration config)
        {
            Configuration = configuration;
            _config = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder => builder
                .WithOrigins("https://www.wease.io")
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader());
            });
            services.AddControllersWithViews();
            services.AddMvc();
            IConfigurationSection bigVideoSaveFileLocation = _config.GetSection("BigVideoSaveLocation");
            services.Configure<BigVideoSaveLocation>(bigVideoSaveFileLocation);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
            app.UseCors("MyPolicy");
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

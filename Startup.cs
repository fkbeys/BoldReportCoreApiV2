using Bold.Licensing;
using BoldReports.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;


namespace ReportsCoreSamples
{
    public class Startup
    {
 
        public Startup(IConfiguration configuration, IWebHostEnvironment _hostingEnvironment)
 
        {

            //string License = File.ReadAllText(System.IO.Path.Combine(_hostingEnvironment.ContentRootPath, "BoldLicense.txt"), Encoding.UTF8);

            //BoldLicenseProvider.RegisterLicense(License, bool.Parse(configuration.GetSection("appSettings").GetSection("IsOfflineLicense").Value));

            Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(configuration)
       .CreateLogger();

            ReportConfig.DefaultSettings = new ReportSettings()
            {
                MapSetting = this.GetMapSettings(_hostingEnvironment)
            }.RegisterExtensions(this.GetDataExtension(configuration.GetSection("appSettings").GetSection("ExtAssemblies").Value));

            Configuration = configuration;
            env = _hostingEnvironment;
        }
        private List<string> GetDataExtension(string ExtAssemblies)
        {
            var extensions = !string.IsNullOrEmpty(ExtAssemblies) ? ExtAssemblies : string.Empty;
            try
            {
                var ExtNames = new List<string>(extensions.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                List<string> ExtCollections = new List<string>();
                ExtNames.ForEach(Extension => ExtCollections.Add(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, Extension + ".dll")));
                return ExtCollections;
            }
            catch (Exception ex)
            {
                //LogExtension.LogError("Failed to Load Data Extension", ex, MethodBase.GetCurrentMethod());
            }
            return null;
        }
         
        public IWebHostEnvironment env { get; }
 
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
 
            services.AddControllersWithViews(); 
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
            services.AddCors(o => o.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            }));
        }


        private MapSetting GetMapSettings(IWebHostEnvironment _hostingEnvironment)
        {
            try
            {
                string basePath = _hostingEnvironment.WebRootPath;
                return new MapSetting()
                {
                    ShapePath = basePath + "\\ShapeData\\",
                    MapShapes = JsonConvert.DeserializeObject<List<MapShape>>(System.IO.File.ReadAllText(basePath + "\\ShapeData\\mapshapes.txt"))
                };
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            return null;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseFileServer();

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    ServeUnknownFileTypes = true,
            //    DefaultContentType = "plain/text",
            //    FileProvider = new PhysicalFileProvider(
            //Path.Combine(Directory.GetCurrentDirectory(), "Controllers")),
            //    RequestPath = "/Controllers"
            //});
             
            app.UseRouting();
            app.UseCors("AllowAllOrigins");
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                          name: "ReportViewer",
                          pattern: "ReportViewer/{controller}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Main}/{action=Index}/{id?}");
            });

        }
    }
}

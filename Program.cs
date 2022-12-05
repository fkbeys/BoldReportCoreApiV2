using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Bold.Licensing;
using Serilog;

#if !NETCOREAPP2_1
using Microsoft.Extensions.Hosting;
#endif

namespace ReportsCoreSamples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BoldLicenseProvider.RegisterLicense("YPoyaU5+r322Px4op+v6TEhC8iUaRubI+g/SZanqSBk=");
            CreateHostBuilder(args).Build().Run();
        }
         
        public static IHostBuilder CreateHostBuilder(string[] args) =>

        Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                   
                    webBuilder.UseStartup<Startup>();
                });
 
    }
}

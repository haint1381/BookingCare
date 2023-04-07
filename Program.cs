using BookingCare.Data.DBBase;
using BookingCare.DataAccess.Repositoy.System.Impl;
using BookingCare.DataAccess.Repositoy.System;
using BookingCare.Service.System.Impl;
using BookingCare.Service.System;
using BookingCare.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookigCare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((context, services) => {
                    services.AddTransient<ILoggerFactory, LoggerFactory>();
                    services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddScoped<IDatabaseHelper, MicrosoftSqlDatabaseHelper>();
                    services.AddScoped<IAccountRepository, AccountRepository>();
                    services.AddScoped<IBaseAppService, BaseAppService>();
                    services.AddScoped<IAccountService, AccountService>();
                    services.AddScoped<IDatabaseHelper, MicrosoftSqlDatabaseHelper>();
                });
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Testing
{
  public class CustomWebApplicationFactory<TProgram>
      : WebApplicationFactory<TProgram> where TProgram : class
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseWebRoot("./");

var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Testing.json")
        .AddEnvironmentVariables()
        .Build();

      builder.UseConfiguration(config)
          .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")
      ;
    }
  }
}
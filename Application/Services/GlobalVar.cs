using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Domain.Models;
using System.Web;

namespace Application.Services
{
  public class GlobalVar
  {
    private readonly IWebHostEnvironment _webHostEnvironment;
    public static SystemConfig SystemConfig { get; set; }

    public GlobalVar(IWebHostEnvironment webHostEnvironment)
    {
      this._webHostEnvironment = webHostEnvironment;
    }
    
    public async Task LoadConfig()
    {
      string webRootPath = _webHostEnvironment.WebRootPath;
      string contentRootPath = _webHostEnvironment.ContentRootPath;
      var path = contentRootPath + "\\systemConfig.json";

      var config = await File.ReadAllTextAsync(path);
      var json = JsonSerializer.Deserialize<SystemConfig>(config, new JsonSerializerOptions()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      });
      SystemConfig = json;
    }

    // public async Task<SystemConfig> GetConfig()
    // {
    //   if (SystemConfig == null) await LoadConfig();
    //   return SystemConfig;
    // }
  }
}
using Application.Helpers;
using Application.SignalR;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(opts =>
{
  opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
  opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;

  opts.JsonSerializerOptions.DictionaryKeyPolicy = null;
  opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

  opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
await SeedHelper.Seed(app.Services);
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(p => p
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials()
  .WithOrigins("https://localhost:3000", "http://localhost:3000", 
      "https://uniinc-cnb.com", 
      "http://localhost:5000", "https://localhost:5001")
);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionResolveMiddleware>();

app.UseEndpoints(endpoints =>
  {
    endpoints.MapControllers();
    endpoints.MapHub<UserHub>("/userHub");
  });

app.MapFallbackToController("Index", "Fallback");

app.Run();
//await SeedHelper.Seed(app.Services);
public partial class Program { }
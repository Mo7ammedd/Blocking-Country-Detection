using BD.Core.Interfaces.Repositories;
using BD.Core.Interfaces.Services;
using BD.Repository;
using BD.Services;
using BD.Services.Config;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<GeoLocationApiConfig>(builder.Configuration.GetSection("GeoLocationApi"));

builder.Services.AddSingleton<ICountryRepository, CountryRepository>();
builder.Services.AddSingleton<ILogsRepository, LogsRepository>();

builder.Services.AddHttpClient<IIpGeolocationService, IpGeolocationService>();

builder.Services.AddHostedService<TemporalBlocksCleanupService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Blocking Detection API", Version = "v1" });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blocking Detection API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

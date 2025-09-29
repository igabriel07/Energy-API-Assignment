using Microsoft.EntityFrameworkCore;
using StellarBlueAssignment.Data;
using StellarBlueAssignment.Models;
using StellarBlueAssignment.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<EnergyOptions>(
    builder.Configuration.GetSection(StellarBlueAssignment.Models.EnergyOptions.StellarBlueApiSettings));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddHttpClient("StellarBlueApi", client =>{
    client.BaseAddress = new Uri("https://assignment.stellarblue.eu/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<EnergyOptions>(
    builder.Configuration.GetSection(EnergyOptions.StellarBlueApiSettings));

builder.Services.AddScoped<EnergyDataService>();

builder.Services.AddHostedService<DataUpdateBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stellar Blue Energy API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath)){
        c.IncludeXmlComments(xmlPath);
    }
});


var app = builder.Build();

using (var scope = app.Services.CreateScope()){
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); 
}

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI(c =>{
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stellar Blue Energy API V1");
        c.RoutePrefix = "docs"; 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers(); 

app.Run();



using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Banco de dados: o endereço vem dos Segredos do Usuário (conexão "PocGestante")
builder.Services.AddDbContext<MonitoramentoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PocGestante")));

builder.Services.AddScoped<IEnfermeiraService, EnfermeiraService>();
builder.Services.AddScoped<IGestanteService, GestanteService>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
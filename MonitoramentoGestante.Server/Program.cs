// Eu sou o cara qe liga a aplicação: registro os controllers, conecto o banco 
// (GeoSaudeIntegrada_DEV) e digo qual service cada Controller recebe. também entrego o site em VUE

using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Services;

var builder = WebApplication.CreateBuilder(args);


// Adiciona o serviço ao container

builder.Services.AddControllers();

// builder OpenaAPI
builder.Services.AddOpenApi();

// Banco de dados: o endereço vem dos Segredos do Usuário (conexão "GeoSaudeDev").
// O nome aqui tem que ser IGUAL ao do secrets.json

builder.Services.AddDbContext<MonitoramentoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GeoSaudeDev")));

builder.Services.AddScoped<IEnfermeiraService, EnfermeiraService>();
builder.Services.AddScoped<IGestanteService, GestanteService>();
builder.Services.AddScoped<IBuscaAtivaService, BuscaAtivaService>();

var app = builder.Build();
app.UseDefaultFiles();
app.MapStaticAssets();

// Configurações das requisiçoes HTTP na pipeline 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();
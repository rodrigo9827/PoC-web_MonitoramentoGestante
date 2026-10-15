// Eu sou o cara qe liga a aplicação: registro os controllers, conecto o banco 
// (GeoSaudeIntegrada_DEV) e digo qual service cada Controller recebe. também entrego o site em VUE

using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Data;
using MonitoramentoGestante.Server.Services;
using MonitoramentoGestante.Server.Security;

var builder = WebApplication.CreateBuilder(args);
// Security
builder.AddHardening();

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
builder.Services.AddScoped<IObservacaoService, ObservacaoService>();
builder.Services.AddScoped<IFinalizarAtendimentoService, FinalizarAtendimentoService>();
builder.Services.AddScoped<ITeleoperadorService, TeleoperadorService>();
builder.Services.AddSingleton<IAutomacaoESaude, AutomacaoESaude>();

var app = builder.Build();
app.UseHardening();
app.UseDefaultFiles();
app.MapStaticAssets();

// Configurações das requisiçoes HTTP na pipeline 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();
// eu sou o cara que blinda o servidor do MonitoramentoGestante: tiro o "Server: Kestrel" das respostas,
// coloco os cabeçalhos de segurança (CSP, nosniff, anti-clickjacking), forço HTTPS com HSTS,
// travo o CORS só nas origens permitidas, limito requisições por IP contra força bruta
// e escondo stack trace em produção.
//
// COMO USAR no Program.cs:
//   using MonitoramentoGestante.Server.Security;
//   builder.AddHardening();          // antes do builder.Build()
//   var app = builder.Build();
//   app.UseHardening();              // LOGO depois do Build(), antes de UseDefaultFiles/MapStaticAssets

using System.Threading.RateLimiting;

namespace MonitoramentoGestante.Server.Security;

public static class SecurityHardening
{
    public const string CorsPolicy = "SomenteFrontend";

    public static WebApplicationBuilder AddHardening(this WebApplicationBuilder builder)
    {
        // 1) Não anunciar qual servidor é (atacante ganha menos informação)
        builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

        // 2) CORS: só as origens listadas em appsettings -> "Security:AllowedOrigins"
        //    Lista vazia = nenhuma origem externa (o padrão mais seguro quando o Vue é servido pelo próprio servidor)
        var origens = builder.Configuration
            .GetSection("Security:AllowedOrigins").Get<string[]>() ?? [];

        builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins(origens)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")));

        // 3) Rate limit: 100 requisições por minuto por IP
        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        // 4) HSTS: navegador só fala com o site via HTTPS por 1 ano
        builder.Services.AddHsts(o =>
        {
            o.MaxAge = TimeSpan.FromDays(365);
            o.IncludeSubDomains = true;
        });

        // 5) Erros viram respostas padronizadas, sem detalhes internos
        builder.Services.AddProblemDetails();

        return builder;
    }

    public static WebApplication UseHardening(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(); // sem stack trace pro usuário
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.Use(async (ctx, next) =>
        {
            var h = ctx.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "no-referrer";
            h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            h["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; " +
                "base-uri 'self'; form-action 'self'";
            await next();
        });

        app.UseCors(CorsPolicy);
        app.UseRateLimiter();

        return app;
    }
}
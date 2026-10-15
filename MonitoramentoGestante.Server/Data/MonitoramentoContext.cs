using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Data
{
    // Aqui é a "ponte" com o banco de dados
    // Sou o cara que diz quais tabelas, views e consultas o programa conhece/vai usar
    public class MonitoramentoContext : DbContext
    {
        public MonitoramentoContext(DbContextOptions<MonitoramentoContext> options) : base(options)
        { }

        public DbSet<GestanteRetorno> GestantesRetorno => Set<GestanteRetorno>();
        public DbSet<GestanteAtual> GestantesAtuais => Set<GestanteAtual>();
        public DbSet<MonitoraGestanteAdm> HistoricoAdm => Set<MonitoraGestanteAdm>();

        // listas da tela
        public DbSet<Hospital> Hospitais => Set<Hospital>();
        public DbSet<OutroLocalParto> OutrosLocaisParto => Set<OutroLocalParto>();
        public DbSet<Teleoperador> Teleoperadores => Set<Teleoperador>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GestanteRetorno>(e =>
            {
                e.HasNoKey();
                // view
                e.ToView("vw_RetornoGestantes", "dbo");
                // Propriedades em c# -> nome da coluna na view
                e.Property(g => g.Enfermeira).HasColumnName("enfermeira");
                e.Property(g => g.Cns).HasColumnName("cns");
                e.Property(g => g.Gestante).HasColumnName("gestante");
                e.Property(g => g.DataContato).HasColumnName("data_contato");
                e.Property(g => g.IgSemanas).HasColumnName("ig_semanas");
                e.Property(g => g.IgDiasResto).HasColumnName("ig_dias_resto");
                e.Property(g => g.DataRetorno).HasColumnName("data_retorno");
                e.Property(g => g.AltoRisco).HasColumnName("alto_risco");
                e.Property(g => g.Cpf).HasColumnName("cpf");
            });

            // lista de hospitais: consulta do supervisor na Core
            // (hospitais tipo 006 da cidade de São Paulo - 73 linhas)
            modelBuilder.Entity<Hospital>(e =>
            {
                e.ToSqlQuery(@"
                    select  est_core.EstabelecimentoId,
                            est_core.EstabelecimentoCNES,
                            est_core.EstabelecimentoCMES,
                            est_core.EstabelecimentoNome
                    from    Core.Estabelecimento        est_core
                    join    Core.Municipio              mun_core  on mun_core.MunicipioId = est_core.MunicipioId
                    join    Core.TipoEstabelecimento    test_core on test_core.TipoEstabelecimentoId = est_core.TipoEstabelecimentoId
                    where   left(mun_core.MunicipioCodigoMunicipal, 6) = '355030'   -- SAO PAULO
                      and   test_core.CodigoDatasusTipoEstabelecimento = '006'      -- HOSPITAL");

                e.HasKey(h => h.Id);
                // propriedade em C# -> coluna da consulta
                e.Property(h => h.Id).HasColumnName("EstabelecimentoId");
                e.Property(h => h.Cnes).HasColumnName("EstabelecimentoCNES");
                e.Property(h => h.Cmes).HasColumnName("EstabelecimentoCMES");
                e.Property(h => h.Nome).HasColumnName("EstabelecimentoNome");
            });

            // outros locais de parto: parâmetros da Core (mesmos que o Pedro usa no RN)
            modelBuilder.Entity<OutroLocalParto>(e =>
            {
                e.ToSqlQuery(@"
                    select  ParametroId,
                            ParametroValue
                    from    Core.Parametro
                    where   ParametroKey = 'OutrosLocaisDePartoRN'");

                e.HasKey(o => o.Id);
                e.Property(o => o.Id).HasColumnName("ParametroId");
                e.Property(o => o.Nome).HasColumnName("ParametroValue");
            });

            // tabela app.Teleoperadores
            modelBuilder.Entity<Teleoperador>(e =>
            {
                e.ToTable("Teleoperadores", "app");
                e.HasKey(t => t.Id);
            });
        }
    }
}
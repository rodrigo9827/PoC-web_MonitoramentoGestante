using Microsoft.EntityFrameworkCore;
using MonitoramentoGestante.Server.Models;

namespace MonitoramentoGestante.Server.Data
{
    // Aqui é a "ponte" com o banco dedados
    // Sou o cara que diz quais tabelas e views o programa conhece/vai usar
    public class MonitoramentoContext : DbContext
    {
        public MonitoramentoContext(DbContextOptions<MonitoramentoContext> options) : base(options)
        {}
        public DbSet<GestanteRetorno> GestantesRetorno => Set<GestanteRetorno>();
        public DbSet<GestanteAtual> GestantesAtuais => Set<GestanteAtual>();
        public DbSet<MonitoraGestanteAdm> HistoricoAdm => Set<MonitoraGestanteAdm>();
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
        }

    }
}



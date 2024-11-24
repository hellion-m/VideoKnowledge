using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoKnowledge.Models;

namespace VideoKnowledge.Infrastructure
{
    /// <summary>
    /// La classe <c>DataContext</c> estende <see cref="IdentityDbContext{AppUser}"/> e rappresenta il contesto del database dell'applicazione.
    /// Gestisce l'accesso e le operazioni di CRUD su tutte le entità di dominio, inclusi gli utenti (tramite <see cref="AppUser"/>) e le entità correlate come VideoKnowledgeContent, VideoKnowledgeEvent, e altre.
    /// </summary>
    public class DataContext : IdentityDbContext<AppUser>
    {
        /// <summary>
        /// Inizializza una nuova istanza di <see cref="DataContext"/> con le opzioni specificate.
        /// </summary>
        /// <param name="options">Le opzioni di configurazione per il contesto del database.</param>
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        /// <summary>
        /// Ottiene o imposta il set di entità <see cref="VideoKnowledgeContent"/>.
        /// </summary>
        public DbSet<VideoKnowledgeContent> VideoKnowledgeContents { get; set; }

        /// <summary>
        /// Ottiene o imposta il set di entità <see cref="VideoKnowledgeEvent"/>.
        /// </summary>
        public DbSet<VideoKnowledgeEvent> VideoKnowledgeEvents { get; set; }

        /// <summary>
        /// Ottiene o imposta il set di entità <see cref="UserTestResults"/>.
        /// </summary>
        public DbSet<UserTestResults> UserTestResults { get; set; }

        /// <summary>
        /// Ottiene o imposta il set di entità <see cref="Category"/>.
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Ottiene o imposta il set di entità <see cref="Question"/>.
        /// </summary>
        public DbSet<Question> Questions { get; set; }

        /// <summary>
        /// Configura le relazioni tra le entità del modello e il database, inclusi i comportamenti di eliminazione e le chiavi esterne.
        /// </summary>
        /// <param name="modelBuilder">Il costruttore del modello da configurare.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relazione uno-a-molti tra VideoKnowledgeContent e VideoKnowledgeEvent
            modelBuilder.Entity<VideoKnowledgeContent>()
                .HasMany(vkc => vkc.EvntList)
                .WithOne(vke => vke.VidKnowLinked)
                .HasForeignKey(vke => vke.VidKnowLinkedId)
                .OnDelete(DeleteBehavior.Cascade);  // Se un VideoKnowledgeContent viene eliminato, elimina anche i VideoKnowledgeEvent associati

            // Relazione uno-a-uno per EvntOnPause
            modelBuilder.Entity<VideoKnowledgeContent>()
                .HasOne(vkc => vkc.EvntOnPause)
                .WithOne(vke => vke.VidKnowOnPause)
                .HasForeignKey<VideoKnowledgeEvent>(vke => vke.VidKnowOnPauseId)
                .OnDelete(DeleteBehavior.Restrict);  // Non eliminare EvntOnPause se VideoKnowledgeContent viene eliminato

            // Relazione tra VideoKnowledgeContent e AppUser tramite OwnerUserId (chiave esterna a AspNetUsers)
            modelBuilder.Entity<VideoKnowledgeContent>()
                .HasOne(vkc => vkc.OwnerUser)    // Proprietà di navigazione per l'utente proprietario
                .WithMany()                       // Un utente può avere molti VideoKnowledgeContent
                .HasForeignKey(vkc => vkc.OwnerUserId)  // Chiave esterna
                .OnDelete(DeleteBehavior.Restrict);     // Non eliminare i VideoKnowledgeContent quando un utente viene eliminato

            // Relazione uno-a-molti tra VideoKnowledgeEvent e Question
            modelBuilder.Entity<VideoKnowledgeEvent>()
                .HasMany(vke => vke.QuestionList)
                .WithOne(vkq => vkq.Evnt)
                .HasForeignKey(vkq => vkq.EventId)
                .OnDelete(DeleteBehavior.Cascade);  // Se un VideoKnowledgeEvent viene eliminato, elimina anche le domande associate
        }
    }
}



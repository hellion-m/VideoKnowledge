using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using VideoKnowledge.Infrastructure;

namespace VideoKnowledge
{
    /// <summary>
    /// Factory per la creazione di una nuova istanza di <see cref="DataContext"/>.
    /// Utilizzato durante la progettazione per la gestione di operazioni come le migrazioni del database.
    /// </summary>
    public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        /// <summary>
        /// Crea una nuova istanza di <see cref="DataContext"/> con le opzioni di connessione configurate.
        /// Questo metodo viene utilizzato durante la progettazione, ad esempio, per le migrazioni del database.
        /// </summary>
        /// <param name="args">Argomenti della riga di comando passati dalla CLI.</param>
        /// <returns>Una nuova istanza di <see cref="DataContext"/> configurata con il connection string.</returns>
        public DataContext CreateDbContext(string[] args)
        {
            // Crea un oggetto DbContextOptionsBuilder per configurare le opzioni del DataContext
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

            // Crea una configurazione da un file JSON
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Imposta la directory corrente come base per la configurazione
                .AddJsonFile("appsettings.json")               // Carica la configurazione dal file "appsettings.json"
                .Build();

            // Ottiene la stringa di connessione al database dalla configurazione
            var connectionString = configuration.GetConnectionString("DbConnection");

            // Configura il DbContext per usare SQL Server con la stringa di connessione
            optionsBuilder.UseSqlServer(connectionString);

            // Restituisce una nuova istanza di DataContext con le opzioni configurate
            return new DataContext(optionsBuilder.Options);
        }
    }
}

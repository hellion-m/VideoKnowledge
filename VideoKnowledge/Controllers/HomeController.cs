using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;
using VideoKnowledge.Models.ViewModels;
using Microsoft.EntityFrameworkCore; // namespace per usare Include

namespace VideoKnowledge.Controllers
{
    /// <summary>
    /// Gestisce le operazioni per la visualizzazione della home page, la ricerca di contenuti e la gestione degli errori.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly DataContext _context;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="HomeController"/> con i servizi necessari.
        /// </summary>
        /// <param name="context">Istanza del contesto dati per accedere al database.</param>
        /// <param name="logger">Istanza del logger per registrare eventi e messaggi di errore.</param>
        public HomeController(DataContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Mostra la pagina principale e, opzionalmente, i risultati di una ricerca.
        /// </summary>
        /// <param name="query">
        /// Stringa di ricerca inserita dall'utente. La ricerca viene effettuata nel nome, nella descrizione
        /// o nella categoria dei contenuti di tipo <see cref="VideoKnowledgeContent"/>.
        /// </param>
        /// <returns>
        /// Una vista contenente i risultati della ricerca se <paramref name="query"/> è valorizzata,
        /// altrimenti una lista vuota.
        /// </returns>
        [HttpGet]
        public IActionResult Index(string query)
        {
            IEnumerable<VideoKnowledgeContent> results;

            if (string.IsNullOrWhiteSpace(query))
            {
                results = new List<VideoKnowledgeContent>(); // Nessun risultato se la query è vuota
            }
            else
            {
                results = _context.VideoKnowledgeContents
                    .Include(v => v.Category) // Include la categoria correlata
                    .Where(v => v.Name.Contains(query) || v.Description.Contains(query) || v.Category.Name.Contains(query))
                    .ToList();
            }

            return View(results);
        }

        /// <summary>
        /// Mostra la pagina relativa alla privacy.
        /// </summary>
        /// <returns>Vista della pagina sulla privacy.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Gestisce la visualizzazione della pagina di errore.
        /// Include i dettagli del problema riscontrato durante l'elaborazione della richiesta.
        /// </summary>
        /// <returns>
        /// Una vista con un modello <see cref="ErrorViewModel"/> contenente l'ID della richiesta corrente
        /// o un identificatore vuoto se non disponibile.
        /// </returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

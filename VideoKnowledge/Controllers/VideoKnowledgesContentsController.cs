using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;

namespace VideoKnowledge.Controllers
{
    /// <summary>
    /// Gestisce le operazioni per visualizzare e cercare contenuti di tipo <see cref="VideoKnowledgeContent"/>.
    /// Include funzionalità per la paginazione e la ricerca.
    /// </summary>
    public class VideoKnowledgesContentsController : Controller
    {
        private readonly DataContext _context;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="VideoKnowledgesContentsController"/> con il contesto dati specificato.
        /// </summary>
        /// <param name="context">Istanza del contesto dati per accedere al database.</param>
        public VideoKnowledgesContentsController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mostra una lista di contenuti con supporto alla paginazione e alla visualizzazione per categoria.
        /// </summary>
        /// <param name="categorySlug">
        /// Lo slug della categoria per filtrare i contenuti.
        /// Se vuoto, vengono mostrati tutti i contenuti.
        /// </param>
        /// <param name="p">Il numero della pagina da visualizzare (predefinito: 1).</param>
        /// <returns>
        /// Una vista con una lista paginata di contenuti. Se la categoria non esiste, reindirizza alla vista principale.
        /// </returns>
        public async Task<IActionResult> Index(string categorySlug = "", int p = 1)
        {
                int pageSize = 8;
                ViewBag.PageNumber = p;
                ViewBag.PageRange = pageSize;
                ViewBag.CategorySlug = categorySlug;

                if (categorySlug == "")
                {
                        //ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Products.Count() / pageSize);
                        ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.VideoKnowledgeContents.Count() / pageSize);

                        //return View(await _context.Products.OrderByDescending(p => p.Id).Skip((p - 1) * pageSize).Take(pageSize).ToListAsync());
                        return View(await _context.VideoKnowledgeContents.OrderByDescending(p => p.Id).Skip((p - 1) * pageSize).Take(pageSize).ToListAsync());
                }

                Category category = await _context.Categories.Where(c => c.Slug == categorySlug).FirstOrDefaultAsync();
                if (category == null) return RedirectToAction("Index");

                //var productsByCategory = _context.Products.Where(p => p.CategoryId == category.Id);
                var productsByCategory = _context.VideoKnowledgeContents.Where(p => p.CategoryId == category.Id);
                ViewBag.TotalPages = (int)Math.Ceiling((decimal)productsByCategory.Count() / pageSize);

                return View(await productsByCategory.OrderByDescending(p => p.Id).Skip((p - 1) * pageSize).Take(pageSize).ToListAsync());
        }

        /// <summary>
        /// Cerca contenuti basandosi su un termine di ricerca specificato.
        /// </summary>
        /// <param name="query">Il termine di ricerca da utilizzare per trovare i contenuti.</param>
        /// <returns>
        /// Una vista con una lista di risultati che corrispondono al termine di ricerca nei campi *Name*, *Description* o *Category*.
        /// </returns>
        [HttpGet]
        public IActionResult ContentsSearch(string query)
        {
            var results = _context.VideoKnowledgeContents
                .Where(v => v.Name.Contains(query) || v.Description.Contains(query) || v.Category.Name.Contains(query))
                .ToList();

            return View(results);
        }

    }
}

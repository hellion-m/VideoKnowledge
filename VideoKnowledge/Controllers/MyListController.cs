using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;
using VideoKnowledge.Models.ViewModels;
using System.Threading.Tasks;

namespace VideoKnowledge.Controllers
{
    /// <summary>
    /// Gestisce la lista personale degli utenti per i contenuti di tipo <see cref="VideoKnowledgeContent"/>.
    /// Include operazioni per visualizzare, aggiungere, rimuovere e gestire i preferiti.
    /// </summary>
    public class MyListController : Controller
    {
        private readonly DataContext _context;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="MyListController"/> con il contesto dati specificato.
        /// </summary>
        /// <param name="context">Istanza del contesto dati per accedere al database.</param>
        public MyListController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mostra la lista personale degli utenti caricata dalla sessione corrente.
        /// </summary>
        /// <returns>
        /// Una vista contenente il modello <see cref="MyListViewModel"/> con gli elementi presenti nella lista.
        /// </returns>
        public IActionResult Index()
        {
            List<MyListItem> vdkList = HttpContext.Session.GetJson<List<MyListItem>>("MyList") ?? new List<MyListItem>();

            MyListViewModel vdkListVM = new()
            {
                MyListItems = vdkList,
            };

            return View(vdkListVM);
        }

        /// <summary>
        /// Aggiunge un elemento alla lista personale dell'utente.
        /// </summary>
        /// <param name="id">L'ID del contenuto da aggiungere.</param>
        /// <returns>
        /// Reindirizza alla vista principale dei contenuti di tipo <see cref="VideoKnowledgeContent"/>.
        /// Imposta un messaggio di successo se l'elemento è stato aggiunto o già presente.
        /// </returns>
        public async Task<IActionResult> Add(long id)
        {
            VideoKnowledgeContent product = await _context.VideoKnowledgeContents.FindAsync(id);

            List<MyListItem> vdklist = HttpContext.Session.GetJson<List<MyListItem>>("MyList") ?? new List<MyListItem>();

            MyListItem vdklistItem = vdklist.FirstOrDefault(c => c.VdkItemId == id);

            if (vdklistItem == null)
            {
                vdklist.Add(new MyListItem(product));
                TempData["Success"] = "The product has been added!";
            }
            else
            {
                TempData["Success"] = "The product is already in your list.";
            }

            HttpContext.Session.SetJson("MyList", vdklist);

            return RedirectToAction("Index", "VideoKnowledgesContents");
        }

        /// <summary>
        /// Rimuove un elemento dalla lista personale dell'utente.
        /// </summary>
        /// <param name="id">L'ID dell'elemento da rimuovere.</param>
        /// <returns>
        /// Reindirizza alla pagina della lista personale e rimuove l'elemento selezionato.
        /// Se la lista è vuota, elimina la sessione associata.
        /// </returns>
        public async Task<IActionResult> Remove(long id)
        {
            List<MyListItem> vdklist = HttpContext.Session.GetJson<List<MyListItem>>("MyList");

            vdklist.RemoveAll(p => p.VdkItemId == id);

            if (vdklist.Count == 0)
            {
                HttpContext.Session.Remove("MyList");
            }
            else
            {
                HttpContext.Session.SetJson("MyList", vdklist);
            }

            TempData["Success"] = "The product has been removed!";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Svuota completamente la lista personale dell'utente.
        /// </summary>
        /// <returns>
        /// Un oggetto JSON che indica il successo dell'operazione.
        /// </returns>
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove("MyList");
            return Json(new { success = true });

        }

        /// <summary>
        /// Aggiunge gli elementi presenti nella lista personale dell'utente ai preferiti.
        /// </summary>
        /// <param name="myListItems">Una lista di oggetti <see cref="MyListItem"/> da aggiungere ai preferiti.</param>
        /// <returns>
        /// Un oggetto JSON che indica il successo o il fallimento dell'operazione.
        /// Include un messaggio appropriato in base all'esito.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddToFavorites([FromBody] List<MyListItem> myListItems)
        {
            if (myListItems == null || !myListItems.Any())
            {
                return Json(new { success = false, message = "No items to add." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Json(new { success = false, message = "You need to be logged in to add items to favorites." });
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found in the database." });
            }

            var newFavorites = myListItems.Select(item => item.VdkItemId.ToString()).ToList();
            var existingFavorites = string.IsNullOrEmpty(user.FavoritesVdks)
                ? new List<string>()
                : user.FavoritesVdks.Split(',').ToList();

            existingFavorites.AddRange(newFavorites.Except(existingFavorites));

            user.FavoritesVdks = string.Join(",", existingFavorites);

            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Favorites updated successfully." });
        }
    }
}

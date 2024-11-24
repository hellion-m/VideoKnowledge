using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VideoKnowledge.Infrastructure.Components
{
    /// <summary>
    /// La classe <c>CategoriesViewComponent</c> è un componente di visualizzazione che estende <see cref="ViewComponent"/>.
    /// Questo componente recupera un elenco di categorie dal contesto del database e lo passa alla vista per la visualizzazione.
    /// </summary>
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly DataContext _context;

        /// <summary>
        /// Costruttore del componente di vista. Inietta il contesto del database per accedere alle entità.
        /// </summary>
        /// <param name="context">Il contesto del database utilizzato per accedere alle categorie.</param>
        public CategoriesViewComponent(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Metodo asincrono invocato per restituire il risultato del componente di vista.
        /// Recupera un elenco di categorie dal contesto del database e restituisce una vista con tale elenco.
        /// </summary>
        /// <returns>Un risultato di tipo <see cref="IViewComponentResult"/> che rappresenta la vista generata con l'elenco delle categorie.</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Recupera tutte le categorie dal contesto del database e restituisce la vista con i dati
            return View(await _context.Categories.ToListAsync());
        }
    }
}

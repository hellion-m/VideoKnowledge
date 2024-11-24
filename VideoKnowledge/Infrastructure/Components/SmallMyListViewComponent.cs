using Microsoft.AspNetCore.Mvc;
using VideoKnowledge.Models;
using VideoKnowledge.Models.ViewModels;

namespace VideoKnowledge.Infrastructure.Components
{
    /// <summary>
    /// La classe <c>SmallMyListViewComponent</c> è un componente di visualizzazione che estende <see cref="ViewComponent"/>.
    /// Questo componente viene utilizzato per recuperare una lista di elementi personalizzati dalla sessione utente e visualizzarla in una vista.
    /// </summary>
    public class SmallMyListViewComponent : ViewComponent
    {
        /// <summary>
        /// Il metodo <c>Invoke</c> è invocato per restituire il risultato del componente di vista.
        /// Recupera una lista di elementi dalla sessione e crea un modello di vista contenente informazioni sulla lista.
        /// Se la lista è vuota o nulla, restituisce una vista con un modello nullo.
        /// </summary>
        /// <returns>Un risultato di tipo <see cref="IViewComponentResult"/> che rappresenta la vista generata per il componente.</returns>
        public IViewComponentResult Invoke()
        {
            // Recupera la lista degli elementi MyList dalla sessione
            List<MyListItem> vdkList = HttpContext.Session.GetJson<List<MyListItem>>("MyList");

            // Crea un modello di vista per la lista degli elementi
            SmallMyListViewModel smallVdkListVM;

            if (vdkList == null || vdkList.Count == 0)
            {
                // Se la lista è vuota o nulla, crea un modello nullo
                smallVdkListVM = null;
            }
            else
            {
                // Se la lista contiene elementi, crea il modello di vista con il numero totale di articoli
                smallVdkListVM = new()
                {
                    NumberOfItems = vdkList.Sum(x => x.Quantity),  // Calcola la somma delle quantità di tutti gli articoli nella lista
                    MyListItems = vdkList                          // Imposta la lista degli articoli
                };
            }

            // Restituisce la vista con il modello creato
            return View(smallVdkListVM);
        }
    }
}


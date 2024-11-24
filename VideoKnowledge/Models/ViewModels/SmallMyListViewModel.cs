namespace VideoKnowledge.Models.ViewModels
{
    /// <summary>
    /// ViewModel che rappresenta una versione ridotta della lista di VideoKnowledge preferiti dall'utente.
    /// </summary>
    public class SmallMyListViewModel
    {
        /// <summary>
        /// Ottiene o imposta il numero totale di oggetti VideoKnowledge aggiunti alla lista preferiti personale.
        /// </summary>
        /// <remarks>
        /// Questo campo rappresenta la quantità totale di elementi presenti nella lista personale dell'utente.
        /// </remarks>
        public int NumberOfItems { get; set; }

        /// <summary>
        /// Ottiene o imposta la lista degli oggetti VideoKnowledge aggiunti alla lista preferiti dell'utente.
        /// </summary>
        /// <remarks>
        /// Contiene tutti gli oggetti VideoKnowledge che l'utente ha aggiunto alla propria lista preferiti.
        /// </remarks>
        public List<MyListItem> MyListItems { get; set; }
    }
}


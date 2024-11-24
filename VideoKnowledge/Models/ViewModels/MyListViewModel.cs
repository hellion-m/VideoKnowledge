namespace VideoKnowledge.Models.ViewModels
{
    /// <summary>
    /// ViewModel che rappresenta la lista personale completa di VideoKnowledge.
    /// </summary>
    public class MyListViewModel
    {
        /// <summary>
        /// Ottiene o imposta la lista degli oggetti VideoKnowledge aggiunti alla lista personale.
        /// </summary>
        /// <remarks>
        /// Contiene tutti gli oggetti VideoKnowledge che l'utente ha aggiunto alla propria lista personale.
        /// </remarks>
        public List<MyListItem> MyListItems { get; set; }
    }
}


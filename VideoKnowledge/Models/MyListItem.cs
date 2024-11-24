namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta un elemento della lista dell'utente, contenente informazioni su un contenuto di VideoKnowledge.
    /// Questa classe è usata per gestire gli articoli che l'utente ha aggiunto alla propria lista personalizzata.
    /// </summary>
    public class MyListItem
    {
        /// <summary>
        /// L'ID univoco dell'elemento VideoKnowledge.
        /// </summary>
        public long VdkItemId { get; set; }

        /// <summary>
        /// Il nome del contenuto VideoKnowledge.
        /// </summary>
        public string VdkItemName { get; set; }

        /// <summary>
        /// La quantità dell'elemento nella lista dell'utente.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// La descrizione del contenuto VideoKnowledge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// L'immagine associata al contenuto VideoKnowledge.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Costruttore vuoto per la classe <see cref="MyListItem"/>.
        /// </summary>
        public MyListItem()
        {
        }

        /// <summary>
        /// Costruttore che crea un <see cref="MyListItem"/> a partire da un oggetto <see cref="VideoKnowledgeContent"/>.
        /// Inizializza le proprietà con i dati del contenuto VideoKnowledge.
        /// </summary>
        /// <param name="vdk">Il contenuto VideoKnowledge da cui creare l'elemento della lista.</param>
        public MyListItem(VideoKnowledgeContent vdk)
        {
            VdkItemId = vdk.Id;
            VdkItemName = vdk.Name;
            Description = vdk.Description;
            Quantity = 1; // Default quantity set to 1
            Image = vdk.Image;
        }
    }
}


namespace VideoKnowledge.Models.ViewModels
{
    /// <summary>
    /// Modello di vista utilizzato per la gestione degli errori.
    /// Contiene informazioni relative all'errore, come l'ID della richiesta, che può essere visualizzato in caso di errore.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Ottiene o imposta l'ID della richiesta associata all'errore.
        /// Questo ID viene utilizzato per tracciare l'errore.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Indica se l'ID della richiesta è presente e non vuoto.
        /// Restituisce true se l'ID della richiesta è stato assegnato, false altrimenti.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

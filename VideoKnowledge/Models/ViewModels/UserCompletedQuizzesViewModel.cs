namespace VideoKnowledge.Models.ViewModels
{
    /// <summary>
    /// Modello di vista che rappresenta i quiz completati da un utente.
    /// Contiene i risultati dei test completati dall'utente e i contenuti VideoKnowledge correlati.
    /// </summary>
    public class UserCompletedQuizzesViewModel
    {
        /// <summary>
        /// Ottiene o imposta la lista dei risultati dei test completati dall'utente.
        /// Ogni risultato include informazioni sui quiz completati, come il punteggio e lo stato di completamento.
        /// </summary>
        public IEnumerable<UserTestResults> UserTestResults { get; set; }

        /// <summary>
        /// Ottiene o imposta la lista dei contenuti VideoKnowledge associati ai quiz completati.
        /// Ogni contenuto VideoKnowledge potrebbe essere correlato ai test dell'utente.
        /// </summary>
        public IEnumerable<VideoKnowledgeContent> VideoKnowledgeContents { get; set; }
    }
}


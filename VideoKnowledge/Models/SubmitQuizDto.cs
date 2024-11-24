namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta un DTO (Data Transfer Object) utilizzato per inviare i dati relativi a un quiz da sottoporre.
    /// Contiene le informazioni dell'evento associato al quiz, come la descrizione, il tipo di evento, e i dettagli del quiz.
    /// </summary>
    public class SubmitQuizDto
    {
        /// <summary>
        /// Identificatore univoco per il quiz.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome del quiz.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrizione del quiz.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica se l'evento di pausa video dell'utente è stato attivato durante il quiz.
        /// </summary>
        public bool UserVideoPauseEvent { get; set; }

        /// <summary>
        /// Il tempo in secondi in cui l'evento dovrebbe fermare il video.
        /// </summary>
        public int EvntTimeStopinSec { get; set; }

        /// <summary>
        /// La durata in secondi dell'evento del timer.
        /// </summary>
        public int EvntTimerDuration { get; set; }

        /// <summary>
        /// Tipo dell'evento (es. "Quiz", "Video", "Web Content", ecc.).
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Link Web associato all'evento (se disponibile).
        /// </summary>
        public string EvntWebLink { get; set; }

        /// <summary>
        /// Video associato all'evento.
        /// </summary>
        public string EvntVideo { get; set; }

        /// <summary>
        /// Link al video web associato all'evento.
        /// </summary>
        public string EvntWebVideoLink { get; set; }

        /// <summary>
        /// ID del contenuto VideoKnowledge collegato a questo evento (se presente).
        /// </summary>
        public long? VidKnowLinkedId { get; set; }

        /// <summary>
        /// ID del contenuto VideoKnowledge associato alla pausa video (se presente).
        /// </summary>
        public long? VidKnowOnPauseId { get; set; }

        /// <summary>
        /// Nome dell'immagine associata all'evento.
        /// </summary>
        public string EvntImage { get; set; }

        /// <summary>
        /// ID del quiz associato all'evento.
        /// </summary>
        public long QuizId { get; set; }

        /// <summary>
        /// Nome del quiz associato all'evento.
        /// </summary>
        public string EvntQuizName { get; set; }

        /// <summary>
        /// Elenco delle domande del quiz.
        /// </summary>
        public List<Question> QuestionList { get; set; }

    }
}

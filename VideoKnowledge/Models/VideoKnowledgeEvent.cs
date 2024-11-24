using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VideoKnowledge.Infrastructure.Validation;


namespace VideoKnowledge.Models
{
    /// <summary>
    /// Class <c>VideoKnowledgeContent</c> modella il contenuto dell'evento collegato al main video
    /// </summary>
    public class VideoKnowledgeEvent
    {
        /// <summary>
        /// Identificatore univoco dell'evento.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome dell'evento.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descrizione dell'evento.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Indica se l'evento è una pausa video impostata dall'utente.
        /// </summary>
        public bool UserVideoPauseEvent { get; set; }

        /// <summary>
        /// Tempo in secondi in cui l'evento deve essere attivato.
        /// Deve essere compreso tra 1 e la durata massima del video.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter valid integer Number in range (1 to video duration")]  //Max Value = 2147483646
        public int EvntTimeStopinSec { get; set; }

        /// <summary>
        /// Durata in secondi dell'evento.
        /// </summary>
        public int EvntTimerDuration { get; set; }

        /// <summary>
        /// Tipo di evento, rappresentato da un codice.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Link web associato all'evento.
        /// </summary>
        public string EvntWebLink { get; set; }

        /// <summary>
        /// Percorso del video locale associato all'evento.
        /// </summary>
        public string EvntVideo { get; set; }

        /// <summary>
        /// Link web di un video esterno associato all'evento.
        /// </summary>
        public string EvntWebVideoLink { get; set; }

        /// <summary>
        /// Identificatore del contenuto video principale associato all'evento.
        /// </summary>
        public long? VidKnowLinkedId { get; set; }

        /// <summary>
        /// Oggetto di navigazione al contenuto video principale associato.
        /// </summary>
        public VideoKnowledgeContent VidKnowLinked { get; set; }

        /// <summary>
        /// Identificatore del contenuto video utilizzato come pausa.
        /// </summary>
        public long? VidKnowOnPauseId { get; set; }

        /// <summary>
        /// Oggetto di navigazione al contenuto video utilizzato come pausa.
        /// </summary>
        public VideoKnowledgeContent VidKnowOnPause { get; set; }

        /// <summary>
        /// Percorso dell'immagine associata all'evento.
        /// </summary>
        public string EvntImage { get; set; }

        /// <summary>
        /// File immagine caricato per l'evento (non mappato nel database).
        /// </summary>
        [NotMapped]
        [FileExtension]
        public IFormFile EvntImageUpload { get; set; }

        /// <summary>
        /// File video caricato per l'evento (non mappato nel database).
        /// </summary>
        [NotMapped]
        public IFormFile EvntVideoUpload { get; set; }

        /// <summary>
        /// Nome del quiz associato all'evento.
        /// </summary>
        public string EvntQuizName { get; set; }

        /// <summary>
        /// Lista di domande associate all'evento, se l'evento è un quiz.
        /// </summary>
        public virtual ICollection<Question> QuestionList { get; set; }

        /// <summary>
        /// Restituisce una descrizione leggibile del tipo di evento basata sul codice `EventType`.
        /// </summary>
        public string EventTypeDescription
        {
            get
            {
                return EventType switch
                {
                    "1" => "Image",
                    "2" => "Quiz",
                    "3" => "Web Content",
                    "4" => "Video",
                    "5" => "Web Video",
                    _ => "Unknown"
                };
            }
        }

        /// <summary>
        /// Restituisce una descrizione del tempo di attivazione dell'evento.
        /// Se il tempo è impostato al valore massimo (int.MaxValue), indica "On User Time Stop".
        /// </summary>
        public string EventTimeStopDescription
        {
            get
            {
                return EvntTimeStopinSec == int.MaxValue ? "On User Time Stop" : EvntTimeStopinSec.ToString();
            }
        }
    }

}

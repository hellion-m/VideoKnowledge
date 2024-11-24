using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VideoKnowledge.Infrastructure.Validation;


namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta i risultati dei test dell'utente relativi agli eventi di un video.
    /// Include informazioni sull'utente, sull'evento video, e sui risultati del quiz.
    /// </summary>
    public class UserTestResults
    {
        /// <summary>
        /// Identificatore univoco del risultato del test.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificatore dell'evento video a cui il test è associato.
        /// </summary>
        public long VidEventId { get; set; }

        /// <summary>
        /// Identificatore del contenuto video associato. Non mappato nel database.
        /// </summary>
        [NotMapped]
        public long? VidKnowId { get; set; }

        /// <summary>
        /// Identificatore univoco dell'utente che ha completato il test.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Nome dell'utente o del risultato associato.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dettaglio dei risultati delle domande del test (rapporto tra domande corrette e sbagliate).
        /// </summary>
        public string QuestionsResult { get; set; }

        /// <summary>
        /// Esito del quiz: indica se il test è stato superato o meno (ad esempio, "Pass" o "Not Passed").
        /// </summary>
        public string QuizResult { get; set; }

    }
}

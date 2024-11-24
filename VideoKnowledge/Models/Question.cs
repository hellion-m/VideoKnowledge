using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta una domanda di un quiz associata a un evento VideoKnowledge.
    /// La classe contiene la domanda, le risposte multiple e la risposta corretta.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Identificatore univoco per la domanda.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// L'evento VideoKnowledge associato a questa domanda.
        /// </summary>
        public VideoKnowledgeEvent Evnt { get; set; }

        /// <summary>
        /// ID dell'evento a cui la domanda è associata.
        /// </summary>
        [Required]
        public long EventId { get; set; }

        /// <summary>
        /// Il testo della domanda.
        /// </summary>
        [Required]
        public string QuestionText { get; set; }

        /// <summary>
        /// La prima risposta possibile per la domanda.
        /// </summary>
        [Required]
        public string AnswerA { get; set; }

        /// <summary>
        /// La seconda risposta possibile per la domanda.
        /// </summary>
        [Required]
        public string AnswerB { get; set; }

        /// <summary>
        /// La terza risposta possibile per la domanda.
        /// </summary>
        [Required]
        public string AnswerC { get; set; }

        /// <summary>
        /// La quarta risposta possibile per la domanda.
        /// </summary>
        [Required]
        public string AnswerD { get; set; }

        /// <summary>
        /// La risposta corretta tra le opzioni (A, B, C o D).
        /// </summary>
        [Required]
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// La risposta selezionata dall'utente, non viene salvata nel database.
        /// È una proprietà temporanea per la visualizzazione o la validazione.
        /// </summary>
        [NotMapped]
        [Display(Name = "Selected Answer")]
        public string SelectedAnswer { get; set; }

    }

}

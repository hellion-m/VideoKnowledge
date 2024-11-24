using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VideoKnowledge.Infrastructure.Validation;

using Microsoft.AspNetCore.Identity;

namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta un contenuto video nel sistema VideoKnowledge.
    /// Include informazioni sulla categoria, media associati, eventi collegati, e proprietà del proprietario.
    /// </summary>
    public class VideoKnowledgeContent
    {
        /// <summary>
        /// Identificatore univoco del contenuto video.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome del contenuto video. Obbligatorio.
        /// </summary>
        [Required(ErrorMessage = "Please enter a value")]
        public string Name { get; set; }

        /// <summary>
        /// Slug utilizzato per identificare univocamente il contenuto nei percorsi URL.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Descrizione del contenuto video. Deve avere una lunghezza minima di 4 caratteri.
        /// </summary>
        [Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        public string Description { get; set; }

        /// <summary>
        /// Collezione di eventi associati a questo contenuto video.
        /// </summary>
        public virtual ICollection<VideoKnowledgeEvent> EvntList { get; set; }

        /// <summary>
        /// Identificatore della categoria a cui appartiene il contenuto video. Obbligatorio.
        /// </summary>
        [Required, Range(1, int.MaxValue, ErrorMessage = "You must choose a category")]
        public long CategoryId { get; set; }

        /// <summary>
        /// Oggetto di navigazione alla categoria a cui appartiene il contenuto video.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Percorso dell'immagine associata al contenuto video.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Link video locale associato al contenuto.
        /// </summary>
        public string VideoLink { get; set; }

        /// <summary>
        /// Sorgente del video (ad esempio, un file locale o un URL remoto).
        /// </summary>
        public string VideoSource { get; set; }

        /// <summary>
        /// Link a un video web associato al contenuto.
        /// </summary>
        public string WebVideoLink { get; set; }

        /// <summary>
        /// Durata del video, espressa in secondi.
        /// </summary>
        public long VideoDuration { get; set; }

        /// <summary>
        /// Identificatore dell'utente proprietario del contenuto video (chiave esterna alla tabella AspNetUsers).
        /// </summary>
        public string OwnerUserId { get; set; }

        /// <summary>
        /// Proprietà di navigazione al proprietario del contenuto video (oggetto di tipo AppUser o IdentityUser).
        /// </summary>
        public virtual AppUser OwnerUser { get; set; }

        /// <summary>
        /// Evento attivato quando il video è in pausa.
        /// </summary>
        public virtual VideoKnowledgeEvent EvntOnPause { get; set; }

        /// <summary>
        /// File immagine caricato per il contenuto video (non mappato nel database).
        /// </summary>
        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }

        /// <summary>
        /// File video caricato per il contenuto video (non mappato nel database).
        /// </summary>
        [NotMapped]
        public IFormFile VideoUpload { get; set; }

    }
}

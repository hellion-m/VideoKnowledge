using Microsoft.AspNetCore.Identity;

namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta un utente nell'applicazione, estendendo la classe <see cref="IdentityUser"/>.
    /// Contiene informazioni aggiuntive specifiche per gli utenti, come l'occupazione e la quota del disco,
    /// e la lista dei contenuti VideoKnowledge preferiti.
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// Ottiene o imposta l'occupazione del disco dell'utente in MB.
        /// Rappresenta lo spazio utilizzato dai file dell'utente.
        /// </summary>
        public float DiskOccupation { get; set; }

        /// <summary>
        /// Ottiene o imposta la quota di disco dell'utente in MB.
        /// Limita lo spazio massimo che un utente può utilizzare per i file.
        /// </summary>
        public float DiskQuota { get; set; }

        /// <summary>
        /// Ottiene o imposta la lista dei contenuti VideoKnowledge preferiti dall'utente.
        /// È una stringa che rappresenta gli ID dei contenuti preferiti separati da virgole.
        /// </summary>
        public string FavoritesVdks { get; set; }
    }
}


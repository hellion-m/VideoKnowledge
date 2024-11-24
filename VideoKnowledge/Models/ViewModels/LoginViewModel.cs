using System.ComponentModel.DataAnnotations;

namespace VideoKnowledge.Models.ViewModels
{
    /// <summary>
    /// Modello di vista utilizzato per il login dell'utente.
    /// Contiene le proprietà necessarie per raccogliere il nome utente, la password e l'eventuale URL di ritorno dopo il login.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Ottiene o imposta il nome utente dell'utente che sta effettuando il login.
        /// Deve avere una lunghezza minima di 2 caratteri.
        /// </summary>
        [Required, MinLength(2, ErrorMessage = "Minimum length is 2")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// Ottiene o imposta la password dell'utente.
        /// Deve avere una lunghezza minima di 4 caratteri.
        /// </summary>
        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        public string Password { get; set; }

        /// <summary>
        /// Ottiene o imposta l'URL di ritorno, a cui l'utente verrà reindirizzato dopo aver effettuato il login.
        /// Se non specificato, l'utente verrà reindirizzato alla home page.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
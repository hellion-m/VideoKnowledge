using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VideoKnowledge.Models
{
    /// <summary>
    /// Rappresenta un utente nel sistema, contenente informazioni relative all'account, come username, email e password.
    /// Inoltre, gestisce le impostazioni di quota disco e i contenuti preferiti dell'utente.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificatore univoco dell'utente.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome utente dell'utente. Deve essere lungo almeno 2 caratteri.
        /// </summary>
        [Required, MinLength(2, ErrorMessage = "Minimum length is 2")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// L'indirizzo email dell'utente. Deve essere un'email valida.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// La password dell'utente. Deve essere lunga almeno 4 caratteri.
        /// </summary>
        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        public string Password { get; set; }

        /// <summary>
        /// La quota di disco assegnata all'utente, espressa in megabyte (MB). Valore predefinito di 2000 MB.
        /// </summary>
        [DefaultValue(2000)]
        public float DiskQuota { get; set; }

        /// <summary>
        /// Una stringa che rappresenta l'elenco dei contenuti preferiti dall'utente, contenente gli ID dei contenuti VideoKnowledge.
        /// </summary>
        public string FavoritesVdks { get; set; }

    }
}
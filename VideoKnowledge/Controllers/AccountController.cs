using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VideoKnowledge.Models;
using VideoKnowledge.Models.ViewModels;

namespace VideoKnowledge.Controllers
{
    /// <summary>
    /// Gestisce le operazioni relative alla gestione degli account utente, tra cui registrazione, autenticazione e logout.
    /// </summary>
    public class AccountController : Controller
        {
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
                _userManager = userManager;
                _signInManager = signInManager;
        }

        /// <summary>
        /// Mostra la vista per creare un nuovo account utente.
        /// </summary>
        /// <returns>Vista per la registrazione di un nuovo utente.</returns>
        public IActionResult Create() => View();

        /// <summary>
        /// Gestisce la registrazione di un nuovo utente.
        /// Crea un nuovo account con i dati forniti e lo salva nel database.
        /// </summary>
        /// <param name="user">Oggetto contenente i dati di registrazione dell'utente.</param>
        /// <returns>
        /// In caso di successo, reindirizza alla pagina dei contenuti di amministrazione.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                    AppUser newUser = new AppUser { UserName = user.UserName, Email = user.Email };
                    IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);

                    if (result.Succeeded)
                    {
                            /*redirect alla pagina dei contenuti videoknowledges*/
                            return Redirect("/admin/VideoKnowledgesContents");
                    }

                    foreach (IdentityError error in result.Errors)
                    {
                            ModelState.AddModelError("", error.Description);
                    }

            }
            return View(user);
        }
        /// <summary>
        /// Mostra la vista per accedere con un account esistente.
        /// </summary>
        /// <param name="returnUrl">URL a cui reindirizzare l'utente dopo l'accesso.</param>
        /// <returns>Vista per l'accesso utente.</returns>
        public IActionResult Login(string returnUrl) => View(new LoginViewModel { ReturnUrl = returnUrl });

        /// <summary>
        /// Gestisce il processo di accesso utente.
        /// Verifica le credenziali e autentica l'utente se valide.
        /// </summary>
        /// <param name="loginVM">Oggetto contenente nome utente, password e URL di ritorno.</param>
        /// <returns>
        /// In caso di successo, reindirizza all'URL specificato o alla home page.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
                if (ModelState.IsValid)
                {
                        Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, false, false);

                        if (result.Succeeded)
                        {
                                return Redirect(loginVM.ReturnUrl ?? "/");
                        }

                        ModelState.AddModelError("", "Invalid username or password");
                }

                return View(loginVM);
        }

        /// <summary>
        /// Gestisce il logout dell'utente corrente.
        /// Disconnette l'utente e lo reindirizza all'URL specificato o alla home page.
        /// </summary>
        /// <param name="returnUrl">URL a cui reindirizzare dopo il logout. Default: "/".</param>
        /// <returns>Reindirizzamento all'URL specificato.</returns>
        public async Task<RedirectResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
    }
}

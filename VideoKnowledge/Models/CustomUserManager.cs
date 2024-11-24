using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VideoKnowledge.Models;

/// <summary>
/// Estende la classe <see cref="UserManager{AppUser}"/> per personalizzare la gestione degli utenti in base a logiche specifiche,
/// come l'assegnazione automatica del ruolo "Admin" a un utente con nome utente "admin".
/// </summary>
public class CustomUserManager : UserManager<AppUser>
{
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Costruttore per inizializzare un'istanza di <see cref="CustomUserManager"/>.
    /// </summary>
    /// <param name="store">Archiviazione dell'utente.</param>
    /// <param name="optionsAccessor">Accessor per le opzioni di configurazione di Identity.</param>
    /// <param name="passwordHasher">Hashing della password dell'utente.</param>
    /// <param name="userValidators">Validatori per gli utenti.</param>
    /// <param name="passwordValidators">Validatori per la password dell'utente.</param>
    /// <param name="keyNormalizer">Normalizzazione delle chiavi.</param>
    /// <param name="errors">Descrittore degli errori di Identity.</param>
    /// <param name="services">Servizi aggiuntivi.</param>
    /// <param name="logger">Logger per la registrazione delle attività.</param>
    /// <param name="roleManager">Gestore dei ruoli per l'assegnazione dei ruoli agli utenti.</param>
    public CustomUserManager(IUserStore<AppUser> store,
                             IOptions<IdentityOptions> optionsAccessor,
                             IPasswordHasher<AppUser> passwordHasher,
                             IEnumerable<IUserValidator<AppUser>> userValidators,
                             IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
                             ILookupNormalizer keyNormalizer,
                             IdentityErrorDescriber errors,
                             IServiceProvider services,
                             ILogger<UserManager<AppUser>> logger,
                             RoleManager<IdentityRole> roleManager)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Crea un nuovo utente, e se l'utente ha il nome utente "admin", gli assegna automaticamente il ruolo "Admin".
    /// </summary>
    /// <param name="user">L'utente da creare.</param>
    /// <param name="password">La password dell'utente.</param>
    /// <returns>Un risultato che indica se la creazione è riuscita o meno.</returns>
    public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
    {
        var result = await base.CreateAsync(user, password);

        // Se la creazione è riuscita e l'utente è l'admin, assegna il ruolo "Admin"
        if (result.Succeeded && user.UserName == "admin")
        {
            // Verifica se il ruolo "Admin" esiste
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                // Crea il ruolo "Admin" se non esiste
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Aggiungi l'utente al ruolo "Admin"
            await this.AddToRoleAsync(user, "Admin");
        }

        return result;
    }
}


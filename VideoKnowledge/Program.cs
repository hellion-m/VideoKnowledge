/*
    Punto di ingresso principale per l'applicazione ASP.NET Core.
    Configura i servizi e la pipeline di elaborazione delle richieste.
*/

using Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;

var builder = WebApplication.CreateBuilder(args);

/*
    Configurazione del contesto del database:
    - Aggiunge il servizio di DbContext per la gestione dei dati tramite Entity Framework.
    - Utilizza una connessione SQL Server definita nella configurazione dell'app ("ConnectionStrings:DbConnection").
*/
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DbConnection"]);
});

/*
    Configura il servizio DataContext come Scoped:
    - Garantisce che una nuova istanza venga creata per ogni richiesta HTTP.
*/
builder.Services.AddScoped<DataContext>();

/*
    Aggiunge il supporto per HttpClient:
    - Per effettuare richieste HTTP verso servizi esterni.
*/
builder.Services.AddHttpClient();

/*
    Configura la cache distribuita in memoria:
    - Consente di memorizzare temporaneamente dati per migliorare le prestazioni.
*/
builder.Services.AddDistributedMemoryCache();

/*
    Configura le opzioni per le sessioni:
    - Timeout di inattività: 30 minuti.
    - I cookie sono marcati come essenziali, necessari per il funzionamento delle sessioni.
*/
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

/*
    Configura Identity per la gestione degli utenti e dei ruoli:
    - Utilizza AppUser come modello di utente.
    - Utilizza IdentityRole per la gestione dei ruoli.
    - Usa il contesto dati configurato (DataContext) per salvare e gestire i dati di Identity.
    - Aggiunge i provider di token predefiniti per la gestione della sicurezza.
*/
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

/*
    Sostituzione di UserManager con un CustomUserManager:
    - Consente di estendere o personalizzare il comportamento predefinito di UserManager.
*/
builder.Services.AddScoped<UserManager<AppUser>, CustomUserManager>();

/*
    Configurazione delle opzioni di Identity:
    - Requisiti minimi per la password (lunghezza minima 4, niente caratteri speciali obbligatori).
    - Richiesta di un'email univoca per ciascun utente.
*/
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
});

/*
    Aggiunge il supporto per i controller e le viste:
    - Consente l'utilizzo di pattern MVC per la gestione delle richieste.
*/
builder.Services.AddControllersWithViews();

var app = builder.Build();

/*
    Abilita l'uso delle sessioni nella pipeline delle richieste.
*/
app.UseSession();

/*
    Configura la gestione degli errori:
    - Utilizza una pagina personalizzata "/Home/Error" in ambienti di produzione.
*/
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

/*
    Abilita il supporto per file statici:
    - Consente di servire file come immagini, CSS, JavaScript dal percorso "wwwroot".
*/
app.UseStaticFiles();

/*
    Configura il routing per l'app:
    - Imposta i middleware per autenticazione e autorizzazione.
*/
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

/*
    Configurazione delle rotte per il controller:
    - Area-specifica: Supporta rotte per aree definite nell'app.
    - Rotta per i prodotti: "/products/{categorySlug?}".
    - Rotta predefinita: "{controller=Home}/{action=Index}/{id?}".
*/
app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=VideoKnowledges}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "products",
    pattern: "/products/{categorySlug?}",
    defaults: new { controller = "Products", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});

/*
    Creazione del ruolo "Admin" all'avvio dell'applicazione:
    - Utilizza il RoleManager per verificare l'esistenza del ruolo "Admin".
    - Se non esiste, lo crea automaticamente.
*/
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roleExists = await roleManager.RoleExistsAsync("Admin");

    if (!roleExists)
    {
        var role = new IdentityRole("Admin");
        await roleManager.CreateAsync(role);
    }
}

/*
    Inizializza il database con dati predefiniti:
    - Utilizza la classe SeedData per popolare il database con valori iniziali.
*/
var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedDatabase(context);

/*
    Avvia l'applicazione ASP.NET Core.
*/
app.Run();

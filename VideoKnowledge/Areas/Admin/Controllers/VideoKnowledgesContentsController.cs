using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;
using MediaInfoDotNet;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using System.Security.Policy;
using Google;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace VideoKnowledge.Areas.Admin.Controllers
{
    /// <summary>
    /// Inizializza una nuova istanza del controller per la gestione dei contenuti VideoKnowledge.
    /// Carica le chiavi API da un file JSON esterno e inizializza i servizi necessari.
    /// </summary>
    /// <param name="context">Il contesto del database per accedere ai dati di VideoKnowledge.</param>
    /// <param name="webHostEnvironment">L'ambiente di hosting per accedere ai file di sistema.</param>
    /// <param name="clientFactory">La fabbrica di client HTTP per effettuare richieste API esterne.</param>
    /// <exception cref="FileNotFoundException">Lanciata se il file di configurazione delle chiavi API non viene trovato.</exception>
    /// <exception cref="InvalidOperationException">Lanciata se una o più chiavi API sono mancanti nel file di configurazione.</exception>
    [Area("Admin")]
    [Authorize]
    public class VideoKnowledgesContentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpClientFactory _clientFactory;
        private readonly PlayerApiKeysConfig _apiKeys;

        public VideoKnowledgesContentsController(DataContext context, IWebHostEnvironment webHostEnvironment, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _clientFactory = clientFactory;

            // Carica le chiavi API da appsettings.json
            _apiKeys = new PlayerApiKeysConfig
            {
                YouTubeApiKey = configuration["ApiKeys:YouTube"],
                VimeoAccessToken = configuration["ApiKeys:Vimeo"]
            };

        }

        /// <summary>
        /// Show the Index page with VideoKnowledge contents.
        /// ID string generated is "M:VideoKnowledge.Areas.Admin.Controllers.VideoKnowledgesContentsController.Index(System.Int32@)".
        /// </summary>
        /// <param name="p">The page number most be displayed (default =1).</param>
        /// <returns>The List of all VideoKnowledges stored.</returns>
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 4;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.VideoKnowledgeContents.Count() / pageSize);

            // Lista per contenere i contenuti filtrati
            IQueryable<VideoKnowledgeContent> videoKnowledgeContents;

            // Controllo se l'utente è un amministratore bisogna quindi aggiornare la tabella 
            if (User.IsInRole("Admin"))
            {
                // Se è un admin, seleziona tutti i contenuti
                videoKnowledgeContents = _context.VideoKnowledgeContents;
            }
            else
            {
                // Se non è un admin, seleziona solo i contenuti creati dall'utente loggato
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                videoKnowledgeContents = _context.VideoKnowledgeContents.Where(v => v.OwnerUserId == userId);
            }

            // Ordina gli eventi collegati e aggiungili al contenuto
            var OrdEvntList = _context.VideoKnowledgeEvents.ToList().OrderByDescending(e => e.EvntTimeStopinSec);
            foreach (var vdk in videoKnowledgeContents)
            {
                foreach (var e in OrdEvntList)
                {
                    if (e.VidKnowLinkedId == vdk.Id && e.UserVideoPauseEvent) {

                        vdk.EvntOnPause = e;
                    }
                    else if (e.VidKnowLinkedId == vdk.Id)
                    {
                        vdk.EvntList.Add(e);
                    }
                }
            }
            

            // Paginazione dei risultati
            var paginatedContents = await videoKnowledgeContents
                .OrderByDescending(v => v.Id)
                .Include(v => v.Category)
                .Skip((p - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(paginatedContents);
        }


        /// <summary>
        /// Visualizza la vista per la creazione di un nuovo contenuto VideoKnowledge.
        /// </summary>
        /// <remarks>
        /// Popola il `ViewBag.Categories` con un elenco di categorie disponibili, 
        /// che viene utilizzato per generare un menu a tendina nella vista di creazione.
        /// </remarks>
        /// <returns>
        /// Una vista per la creazione di un nuovo contenuto.
        /// </returns>
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");

            return View();
        }


        /// <summary>
        /// Visualizza il lettore multimediale per un contenuto VideoKnowledge specifico.
        /// </summary>
        /// <param name="id">ID del contenuto VideoKnowledge da riprodurre.</param>
        /// <param name="time">
        /// Tempo di avvio opzionale in secondi, che indica da dove iniziare la riproduzione del video.
        /// </param>
        /// <returns>
        /// Una vista con il contenuto VideoKnowledge specificato o una risposta 404 se non trovato.
        /// </returns>
        /// <remarks>
        /// Include i dati associati, come eventi e domande, utilizzando le relazioni di entità definite.
        /// </remarks>
        public async Task<IActionResult> Player(long id, int? time)
        {
            VideoKnowledgeContent vidknow = await _context.VideoKnowledgeContents
                .Include(v => v.EvntList)
                .ThenInclude(e => e.QuestionList) // Includi la lista delle domande
                .Include(v => v.EvntOnPause) // Includi EvntOnPause (relazione uno-a-uno)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vidknow == null)
            {
                return NotFound();
            }

            ViewBag.StartTime = time ?? 0; // Pass the start time to the view
            return View(vidknow);
        }



        /// Crea un nuovo contenuto di tipo VideoKnowledge e lo salva nel database.
        /// Consente la configurazione delle chiavi API tramite un file esterno.
        /// </summary>
        /// <param name="vidknow">Il modello che rappresenta il nuovo contenuto VideoKnowledge da creare.</param>
        /// <returns>
        /// Una vista che reindirizza all'Index se l'operazione è completata con successo;
        /// in caso contrario, restituisce la vista di creazione con i messaggi di errore.
        /// </returns>
        /// <remarks>
        /// Supporta il caricamento di file multimediali locali, il calcolo della durata video e la gestione delle chiavi API 
        /// attraverso un file JSON configurabile.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Create(VideoKnowledgeContent vidknow)
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", vidknow.CategoryId);

            if (ModelState.IsValid)
            {
                vidknow.Slug = vidknow.Name.ToLower().Replace(" ", "-");

                var slug = await _context.VideoKnowledgeContents.FirstOrDefaultAsync(p => p.Slug == vidknow.Slug);

                if (slug != null)
                {
                    ModelState.AddModelError("", "The Video Knowledge already exists.");
                    return View(vidknow);
                }

                vidknow.OwnerUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (vidknow.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                    string imageName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknow.ImageUpload.FileName;

                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await vidknow.ImageUpload.CopyToAsync(fs);
                    fs.Close();

                    vidknow.Image = imageName;
                }

                if (vidknow.VideoUpload != null)
                {
                    string vidUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                    string videoName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknow.VideoUpload.FileName;

                    string filePath = Path.Combine(vidUploadsDir, videoName);

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        vidknow.VideoUpload.CopyTo(stream);
                        ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", videoName);
                    }
                    vidknow.VideoLink = videoName;

                    using (var mediaInfo = new MediaFile(filePath))
                    {

                        // valore durata video in millisecondi
                        long durationMilliseconds = mediaInfo.Video[0].Duration;

                        // Conversione milliseconds to TimeSpan
                        TimeSpan duration = TimeSpan.FromMilliseconds(durationMilliseconds);

                        vidknow.VideoDuration = (long)duration.TotalSeconds;
                    }
                }
                if (vidknow.WebVideoLink != null) {
                    if (vidknow.WebVideoLink.Contains("youtube.com"))
                    {
                        var videoId = vidknow.WebVideoLink.Split("embed/")[1].Split("&")[0];
                        var apiKey = _apiKeys.YouTubeApiKey;
                        var requestUrl = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&part=contentDetails&key={apiKey}";
                        vidknow.VideoDuration = await GetDurationFromYouTube(requestUrl);
                    }
                    else if (vidknow.WebVideoLink.Contains("vimeo.com"))
                    {
                        var videoId = vidknow.WebVideoLink.Split("/video/")[1];
                        var requestUrl = $"https://api.vimeo.com/videos/{videoId}";
                        var apiKey = _apiKeys.VimeoAccessToken;
                        using (var httpClient = new HttpClient())
                        {
                            // Sostituisci 'YOUR_VIMEO_ACCESS_TOKEN' con il tuo Bearer Token di Vimeo
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                            var response = await httpClient.GetAsync(requestUrl);

                            if (response.IsSuccessStatusCode)
                            {
                                var content = await response.Content.ReadAsStringAsync();
                                dynamic videoInfo = JsonConvert.DeserializeObject(content);
                                vidknow.VideoDuration = (int)videoInfo.duration;  // Ottieni la durata del video
                            }
                            else
                            {
                                return BadRequest("Failed to get Vimeo video duration.");
                            }
                        }

                    }
                    else if (vidknow.WebVideoLink.Contains("dailymotion.com"))
                    {
                        var videoId = vidknow.WebVideoLink.Split("/video/")[1];
                        var requestUrl = $"https://api.dailymotion.com/video/{videoId}?fields=duration";
                        vidknow.VideoDuration = await GetDurationFromDailymotion(requestUrl);
                    }
                    else
                    {
                        return BadRequest("Unsupported video platform.");
                    }
                }
            }
            
            _context.Add(vidknow);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The Video Knowledge has been created!";

            return Redirect("/admin/VideoKnowledgesContents/Edit/" + vidknow.Id);
        }




        /// <summary>
        /// Recupera la durata di un video di YouTube tramite la YouTube Data API.
        /// </summary>
        /// <param name="videoId">L'ID del video di YouTube per cui ottenere la durata.</param>
        /// <returns>La durata del video in secondi.</returns>
        /// <remarks>
        /// </remarks>
        private async Task<int> GetDurationFromYouTube(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var contentDetails = JObject.Parse(responseJson)["items"][0]["contentDetails"];
                var duration = XmlConvert.ToTimeSpan(contentDetails["duration"].Value<string>());
                return (int)duration.TotalSeconds;
            }

            return 0;
        }


        /// <summary>
        /// Retrieves the duration of a Dailymotion video using its API.
        /// </summary>
        /// <param name="url">The API endpoint with video ID.</param>
        /// <returns>The duration of the video in seconds, or 0 if the request fails.</returns>
        /// <remarks>
        /// - Sends a GET request to the Dailymotion API to retrieve video metadata.
        /// - Parses the `duration` field from the API's JSON response.
        /// </remarks>
        private async Task<int> GetDurationFromDailymotion(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var duration = JObject.Parse(responseJson)["duration"].Value<int>();
                return duration;
            }

            return 0;
        }
        /// <summary>
        /// Recupera un contenuto di tipo VideoKnowledge dal database per modificarne i dettagli.
        /// </summary>
        /// <param name="id">L'identificatore univoco del contenuto VideoKnowledge da modificare.</param>
        /// <returns>
        /// Una vista contenente i dettagli del contenuto VideoKnowledge da modificare. 
        /// Restituisce un errore 404 se il contenuto non viene trovato.
        /// </returns>
        /// <remarks>
        /// Il metodo popola anche la lista di eventi associati al contenuto e 
        /// prepara i dati delle categorie per la selezione nella vista.
        /// </remarks>
        public async Task<IActionResult> Edit(long id)
        {
            //cerca il contenuto da modificare attravero l'id
            VideoKnowledgeContent vidknow = await _context.VideoKnowledgeContents.FindAsync(id);
        

            foreach (VideoKnowledgeEvent e in _context.VideoKnowledgeEvents)
            {
                if (e.VidKnowLinkedId == id)
                {
                    vidknow.EvntList.Add(e);
                }
            }
            
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", vidknow.CategoryId);

            return View(vidknow);
        }


        /// <summary>
        /// Aggiorna un contenuto di tipo VideoKnowledge con i dati forniti dall'utente.
        /// </summary>
        /// <param name="id">L'identificatore univoco del contenuto VideoKnowledge da aggiornare.</param>
        /// <param name="vidknow">Il modello contenente i nuovi dati per l'aggiornamento.</param>
        /// <returns>
        /// Un redirect alla vista Index se l'operazione è completata con successo; 
        /// in caso contrario, la vista attuale con i messaggi di errore appropriati.
        /// </returns>
        /// <remarks>
        /// Questo metodo gestisce il caricamento e la sostituzione di file multimediali 
        /// (immagini e video), convalidando i dati caricati e aggiornando il database.
        /// Inoltre, verifica la durata dei video caricati e rimuove eventuali eventi associati
        /// che eccedono la durata del video.
        /// Utilizza la chiave API di YouTube e Vime caricata dal file di configurazione appsettings. Se la richiesta fallisce, restituisce 0.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Edit (int? id, VideoKnowledgeContent vidknow)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", vidknow.CategoryId);
            var vdkToUpdate = await _context.VideoKnowledgeContents
                     .Include(v => v.EvntList)  // Carica anche gli eventi correlati
                     .FirstOrDefaultAsync(s => s.Id == id);
            // Assicurati che EvntList non sia null
            vdkToUpdate.EvntList ??= new List<VideoKnowledgeEvent>();
            if (await TryUpdateModelAsync<VideoKnowledgeContent>(
                vdkToUpdate,
                "",
                s => s.Name, s => s.Description, s => s.Image, s => s.VideoLink, s => s.Category))
            {
                try
                {
                    // Assegna manualmente il CategoryId per riflettere il cambiamento di categoria
                    vdkToUpdate.CategoryId = vidknow.CategoryId;

                    if (vidknow.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                        string imageName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknow.ImageUpload.FileName;

                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await vidknow.ImageUpload.CopyToAsync(fs);
                        fs.Close();

                        vdkToUpdate.Image = imageName;
                    }

                    if (vidknow.VideoUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                        string videoName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknow.VideoUpload.FileName;

                        string filePath = Path.Combine(uploadsDir, videoName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await vidknow.VideoUpload.CopyToAsync(fs);
                        fs.Close();

                        vdkToUpdate.VideoLink = videoName;
                    }

                    if (vidknow.WebVideoLink != null)
                    {
                        vdkToUpdate.WebVideoLink = vidknow.WebVideoLink;
                        if (vidknow.WebVideoLink.Contains("youtube.com"))
                        {
                            var videoId = vidknow.WebVideoLink.Split("embed/")[1].Split("&")[0];
                            var apiKey = _apiKeys.YouTubeApiKey;

                            var requestUrl = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&part=contentDetails&key={apiKey}";
                            vdkToUpdate.VideoDuration = await GetDurationFromYouTube(requestUrl);
                        }
                        else if (vidknow.WebVideoLink.Contains("vimeo.com"))
                        {
                            var videoId = vidknow.WebVideoLink.Split("/")[4];
                            var requestUrl = $"https://api.vimeo.com/videos/{videoId}";
                            var apiKey = _apiKeys.VimeoAccessToken;
                            using (var httpClient = new HttpClient())
                            {
                                // Sostituisci 'YOUR_VIMEO_ACCESS_TOKEN' con il tuo Bearer Token di Vimeo
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                                //First key//httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "139740e5ba48500e9bd13a7f23733c66");

                                var response = await httpClient.GetAsync(requestUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    var content = await response.Content.ReadAsStringAsync();
                                    dynamic videoInfo = JsonConvert.DeserializeObject(content);
                                    vdkToUpdate.VideoDuration = (int)videoInfo.duration;  // Ottieni la durata del video
                                }
                                else
                                {
                                    return BadRequest("Failed to get Vimeo video duration.");
                                }
                            }
                    }
                        else if (vidknow.WebVideoLink.Contains("dailymotion.com"))
                        {
                            var videoId = vidknow.WebVideoLink.Split("/video/")[1];
                            var requestUrl = $"https://api.dailymotion.com/video/{videoId}?fields=duration";
                            vdkToUpdate.VideoDuration = await GetDurationFromDailymotion(requestUrl);
                        }
                        else
                        {
                            return BadRequest("Unsupported video platform.");
                        }
                        // Rimozione degli eventi con EventTimeStopInSec che eccedono la durata del video
                        var videoDuration = vdkToUpdate.VideoDuration;
                        if (vdkToUpdate.EvntList != null)
                        {
                            var eventsToRemove = vdkToUpdate.EvntList
                                .Where(e => e.EvntTimeStopinSec > videoDuration && e.EvntTimeStopinSec != int.MaxValue)
                                .ToList();

                            foreach (var evt in eventsToRemove)
                            {
                                await DeleteEvent(evt.Id);
                            }
                        }
                    }

                        await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(vdkToUpdate);
        }


        /// <summary>
        /// Metodo helper, rimuove un evento specifico associato a un contenuto VideoKnowledge dal database.
        /// </summary>
        /// <param name="id">L'identificatore univoco dell'evento da eliminare.</param>
        /// <returns>Un'operazione asincrona completata al termine della rimozione dell'evento.</returns>
        /// <remarks>
        /// Durante l'eliminazione, il metodo rimuove anche tutti i dati correlati, 
        /// come domande, immagini, video, e risultati dei test associati all'evento.
        /// </remarks>
        private async Task DeleteEvent(long id)
        {
            VideoKnowledgeEvent vidknowEvent = await _context.VideoKnowledgeEvents.FindAsync(id);
            if (vidknowEvent == null) return;

            // Elimina le domande correlate all'evento
            var relatedQuestions = _context.Questions
                .Where(q => q.EventId == id)
                .ToList();
            _context.Questions.RemoveRange(relatedQuestions);

            // Elimina l'immagine se esiste
            if (vidknowEvent.EvntImage != null)
            {
                string uploadsImgDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                string oldImagePath = Path.Combine(uploadsImgDir, vidknowEvent.EvntImage);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Elimina il video se esiste
            if (vidknowEvent.EvntVideo != null)
            {
                string uploadsVideoDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                string oldVideoPath = Path.Combine(uploadsVideoDir, vidknowEvent.EvntVideo);
                if (System.IO.File.Exists(oldVideoPath))
                {
                    System.IO.File.Delete(oldVideoPath);
                }
            }

            // Cancella i risultati dei test relativi all'evento
            var userTestResults = _context.UserTestResults
                .Where(utr => utr.VidEventId == id)
                .ToList();
            _context.UserTestResults.RemoveRange(userTestResults);

            // Cancella la lista delle domande se presente
            if (vidknowEvent.QuestionList != null)
            {
                vidknowEvent.QuestionList.Clear();
            }

            // Rimuovi l'evento dal contesto
            _context.VideoKnowledgeEvents.Remove(vidknowEvent);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un contenuto esistente di tipo VideoKnowledge, insieme ai file multimediali associati.
        /// </summary>
        /// <param name="id">L'identificatore univoco del contenuto VideoKnowledge da eliminare.</param>
        /// <returns>
        /// Un redirect alla vista Index dopo l'eliminazione, oppure un errore se il contenuto non viene trovato.
        /// </returns>
        /// <remarks>
        /// Durante l'eliminazione, il metodo rimuove tutti gli eventi, domande, immagini e video
        /// associati al contenuto. Anche i risultati dei test relativi agli eventi vengono cancellati.
        /// </remarks>
        public async Task<IActionResult> Delete(long id)
        {
            VideoKnowledgeContent vidknow = await _context.VideoKnowledgeContents.FindAsync(id);

            if (vidknow.Image!=null && !string.Equals(vidknow.Image, "noimage.png"))
            {
                string uploadsImgDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                string oldImagePath = Path.Combine(uploadsImgDir, vidknow.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            if (vidknow.VideoLink != null && !string.Equals(vidknow.VideoLink, "novideo.mp4"))
            {
                string uploadsVideoDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                string oldVideoPath = Path.Combine(uploadsVideoDir, vidknow.VideoLink);
                if (System.IO.File.Exists(oldVideoPath))
                {
                    System.IO.File.Delete(oldVideoPath);
                }
            }

            //controllo lista eventi associati
            foreach (VideoKnowledgeEvent e in _context.VideoKnowledgeEvents)
            {
                if (e.VidKnowLinkedId == id)
                {
                    if ((e.EvntVideo != null))
                    {
                        string uploadsImgDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                        string oldImagePath = Path.Combine(uploadsImgDir, e.EvntImage);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    if ((e.EvntVideo != null))
                    {
                        string uploadsVideoDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                        string oldVideoPath = Path.Combine(uploadsVideoDir, e.EvntVideo);
                        if (System.IO.File.Exists(oldVideoPath))
                        {
                            System.IO.File.Delete(oldVideoPath);
                        }
                    }
                    if (e.QuestionList != null)
                    {
                        foreach (var QuestionElement in e.QuestionList.ToList()) // Converti in una lista per evitare modifiche mentre si itera
                        {
                            e.QuestionList.Remove(QuestionElement);
                        }
                        // Cancella i risultati dei test relativi all'evento
                        var userTestResults = _context.UserTestResults
                            .Where(utr => utr.VidEventId == id)
                            .ToList();
                        _context.UserTestResults.RemoveRange(userTestResults);

                    }
                    _context.VideoKnowledgeEvents.Remove(e);
                }
            }
            _context.VideoKnowledgeContents.Remove(vidknow);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The Video Knowledge has been deleted!";

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Rimuove un contenuto VideoKnowledge dalla lista dei preferiti dell'utente corrente.
        /// </summary>
        /// <param name="contentId">L'identificatore del contenuto da rimuovere dai preferiti.</param>
        /// <returns>
        /// Un oggetto JSON che indica il successo dell'operazione o un redirect alla lista dei preferiti 
        /// se l'operazione è completata con successo.
        /// </returns>
        /// <remarks>
        /// Se l'utente non è autenticato o il contenuto non è presente nella lista dei preferiti, 
        /// viene restituito un messaggio di errore.
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(long contentId)
        {
            // Recupera l'ID dell'utente corrente
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Recupera l'utente dal database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found in database." });
            }

            // Recupera la stringa FavoritesVdks
            var favoriteIds = user.FavoritesVdks;

            if (string.IsNullOrEmpty(favoriteIds))
            {
                return Json(new { success = false, message = "No favorites to remove." });
            }

            // Converti la stringa in una lista di long
            var favoriteIdsList = favoriteIds.Split(',').Select(id => long.Parse(id)).ToList();

            // Rimuovi l'ID specificato dalla lista
            if (favoriteIdsList.Contains(contentId))
            {
                favoriteIdsList.Remove(contentId);
                user.FavoritesVdks = string.Join(",", favoriteIdsList);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FavoritesList");
        }


        /// <summary>
        /// Recupera e visualizza la lista dei contenuti VideoKnowledge preferiti dall'utente corrente.
        /// </summary>
        /// <returns>
        /// Una vista contenente i contenuti preferiti dell'utente. 
        /// Se l'utente non ha preferiti, restituisce una lista vuota.
        /// </returns>
        /// <remarks>
        /// Il metodo utilizza l'identificatore dell'utente corrente per caricare dal database i contenuti 
        /// preferiti e supporta la visualizzazione paginata.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> FavoritesList()
        {
            // Recupera l'ID dell'utente corrente
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int pageSize = 3;
            ViewBag.PageNumber = 1;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.VideoKnowledgeContents.Count() / pageSize);

            if (userId == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Recupera l'utente dal database
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found in database." });
            }

            // Recupera la stringa FavoritesVdks
            var favoriteIds = user.FavoritesVdks;

            if (string.IsNullOrEmpty(favoriteIds))
            {
                return View(new List<VideoKnowledgeContent>());
            }

            // Converti la stringa in una lista di long
            var favoriteIdsList = favoriteIds.Split(',').Select(id => long.Parse(id)).ToList();

            // Recupera gli oggetti VideoKnowledgeContent corrispondenti agli ID
            var favoriteList = await _context.VideoKnowledgeContents
                .Where(vk => favoriteIdsList.Contains(vk.Id))
                .ToListAsync();

            // Restituisci la lista alla vista
            return View(favoriteList);
        }
    }
}
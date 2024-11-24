using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;
using System;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;
using static System.Net.WebRequestMethods;

namespace VideoKnowledge.Areas.Admin.Controllers
{
    /// <summary>
    /// Gestisce le operazioni CRUD (Create, Read, Update, Delete) relative agli eventi associati ai video.
    /// Questi eventi possono includere quiz, collegamenti, pause, o media aggiuntivi.
    /// Accessibile solo agli utenti con autorizzazioni amministrative.
    /// </summary>
    [Area("Admin")]
    [Authorize]
    public class VideoKnowledgesEventsController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VideoKnowledgesEventsController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        /// <summary>
        /// Visualizza la lista degli eventi associati ai contenuti video.
        /// Permette di filtrare gli eventi per un contenuto video specifico.
        /// </summary>
        /// <param name="SelectedVideoKnowledgeContent">
        /// ID del contenuto video selezionato per filtrare gli eventi.
        /// Se nullo, mostra tutti gli eventi.
        /// </param>
        /// <returns>Vista con la lista degli eventi filtrati e ordinati.</returns>
        public ActionResult Index(int? SelectedVideoKnowledgeContent)
        {
            var vdkcontents = _context.VideoKnowledgeContents.OrderBy(q => q.Name).ToList();
            ViewBag.SelectedVideoKnowledgeContent = new SelectList(vdkcontents, "videoKnowledgeContentId", "Name", SelectedVideoKnowledgeContent);
            int vdkcontentId = SelectedVideoKnowledgeContent.GetValueOrDefault();

            IQueryable<VideoKnowledgeEvent> VideoKnowledgeEvents = _context.VideoKnowledgeEvents
                .Where(c => !SelectedVideoKnowledgeContent.HasValue || c.VidKnowLinkedId == vdkcontentId)
                .OrderBy(d => d.Id)
                .Include(d => d.VidKnowLinked);
            var sql = VideoKnowledgeEvents.ToString();
            return View(VideoKnowledgeEvents.ToList().OrderByDescending(p=>p.EvntTimeStopinSec));
        }

        /// <summary>
        /// Restituisce il nome del quiz associato a un evento specifico.
        /// </summary>
        /// <param name="eventId">ID dell'evento per il quale si desidera ottenere il nome del quiz.</param>
        /// <returns>
        /// Una stringa JSON contenente il nome del quiz associato.
        /// Se non è presente un quiz, restituisce una stringa vuota.
        /// </returns>
        [HttpGet("GetQuizName")]
        public IActionResult GetQuizName(int eventId)
        {
            var eventQuizName = _context.VideoKnowledgeEvents
                                        .Where(e => e.Id == eventId)
                                        .Select(e => e.EvntQuizName)
                                        .FirstOrDefault();
            return Json(eventQuizName ?? string.Empty);
        }

        /// <summary>
        /// Fornisce la vista per creare un nuovo evento associato a un video.
        /// </summary>
        /// <returns>Vista per creare un nuovo evento.</returns>
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "EvntTimeStopinSec");

            return View();
        }

        /// <summary>
        /// Crea un nuovo evento associato a un contenuto video.
        /// Verifica l'unicità dell'evento in base al tempo di stop e carica eventuali file di immagine o video associati.
        /// </summary>
        /// <param name="vidknowEvent">Dati del nuovo evento da creare.</param>
        /// <param name="Id">ID del contenuto video associato.</param>
        /// <returns>
        /// In caso di successo, reindirizza alla pagina di modifica del contenuto video.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Create(VideoKnowledgeEvent vidknowEvent, long Id)
        {

            if (ModelState.IsValid)
            {
                // Controlla se esiste già un evento con lo stesso VidKnowLinkedId e EvntTimeStopinSec, due eventi non si possono sovrapporre
                var existingEvent = await _context.VideoKnowledgeEvents
                    .FirstOrDefaultAsync(e => e.VidKnowLinkedId == Id && e.EvntTimeStopinSec == vidknowEvent.EvntTimeStopinSec);

                if (existingEvent != null)
                {
                    ModelState.AddModelError("", "An event with the same VideoKnowledgeLinkedId and Event Time Stop already exists.");
                    return View(vidknowEvent);
                }

                var vdkToUpdate = await _context.VideoKnowledgeContents.FirstOrDefaultAsync(s => s.Id == Id);
                if (await TryUpdateModelAsync<VideoKnowledgeContent>(
                    vdkToUpdate,
                    "",
                    s => s.Name, s => s.Description, s => s.Image, s => s.VideoLink, s => s.VideoDuration))
                {
                    if (vidknowEvent.EvntTimeStopinSec > vdkToUpdate.VideoDuration && !vidknowEvent.UserVideoPauseEvent)
                    {
                        ModelState.AddModelError("", "The Video Knowledge Event time cannot exceed " +
                                                    "the duration of the  main video: " +
                                                    vdkToUpdate.VideoDuration + " seconds");
                        return View(vidknowEvent);
                    }

                    try
                    {
                        vidknowEvent.Id = 0;

                        vdkToUpdate.EvntList ??= new List<VideoKnowledgeEvent>();

                        if (vidknowEvent.EvntImageUpload != null)
                        {
                            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                            string imageName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknowEvent.EvntImageUpload.FileName;

                            string filePath = Path.Combine(uploadsDir, imageName);

                            FileStream fs = new FileStream(filePath, FileMode.Create);
                            await vidknowEvent.EvntImageUpload.CopyToAsync(fs);
                            fs.Close();

                            vidknowEvent.EvntImage = imageName;
                        }

                        if (vidknowEvent.EvntVideoUpload != null)
                        {
                            string vidUploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                            string videoName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknowEvent.EvntVideoUpload.FileName;

                            string filePath = Path.Combine(vidUploadsDir, videoName);

                            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                            {
                                vidknowEvent.EvntVideoUpload.CopyTo(stream);
                                ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", videoName);
                            }
                            vidknowEvent.EvntVideo = videoName;
                        }

                        if (vidknowEvent.UserVideoPauseEvent)
                        {
                            if (vdkToUpdate.EvntOnPause!=null) {
                                ModelState.AddModelError("", "A pause event already exists, please edit the existing pause event or delete it to recreate a new one");
                                return View();
                            }
                            else {
                                vdkToUpdate.EvntOnPause = vidknowEvent;
                                vidknowEvent.VidKnowLinkedId = vdkToUpdate.Id;
                                vidknowEvent.VidKnowOnPauseId = vdkToUpdate.Id;
                            }
                        }
                        else
                        {
                            vdkToUpdate.EvntList.Add(vidknowEvent);
                            vdkToUpdate.EvntList = vdkToUpdate.EvntList.OrderBy((c => c.EvntTimeStopinSec)).ToList();
                        }

                    }
                    catch (DbUpdateException /* ex */)
                    {
                        //Log the error
                        ModelState.AddModelError("", "Unable to save changes!");
                    }
                }
            }
            else
            {
                return View(vidknowEvent);
            }

            vidknowEvent.VidKnowLinkedId = Id;
            _context.Add(vidknowEvent);
            //_context.VideoKnowledgeContents. Add(vdkToUpdate);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The Video Knowledge Event has been created!";

            return Redirect("/admin/VideoKnowledgesContents/Edit/"+Id);
        }

        /// <summary>
        /// Recupera i dati di un evento specifico per modificarlo.
        /// Include le domande associate all'evento, se presenti.
        /// </summary>
        /// <param name="id">ID dell'evento da modificare.</param>
        /// <returns>Vista con i dati dell'evento da modificare.</returns>
        public async Task<IActionResult> Edit(long id)
        {
            //get the VideoKnowledge Event that most be edited by id (in videvnt)
            VideoKnowledgeEvent videvnt = await _context.VideoKnowledgeEvents.FindAsync(id);

            foreach (Question e in _context.Questions)
            {
                if (e.EventId == id)
                {
                    videvnt.QuestionList.Add(e);
                }
            }

            return View(videvnt);
        }

        /// <summary>
        /// Aggiorna i dati di un evento esistente. Gestisce la modifica del tipo di evento, 
        /// l'upload di file, e i vincoli di unicità.
        /// </summary>
        /// <param name="id">ID dell'evento da modificare.</param>
        /// <param name="vidknowEvent">Dati aggiornati dell'evento.</param>
        /// <returns>
        /// In caso di successo, reindirizza alla pagina di modifica del contenuto video associato.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> Edit(int? id, VideoKnowledgeEvent vidknowEvent)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vdkEventToUpdate = await _context.VideoKnowledgeEvents.FirstOrDefaultAsync(s => s.Id == id);

            if (vdkEventToUpdate == null)
            {
                return NotFound();
            }

            // Verifica se esiste già un altro evento con lo stesso VidKnowLinkedId e EvntTimeStopinSec
            var existingEvent = await _context.VideoKnowledgeEvents
                .FirstOrDefaultAsync(e => e.VidKnowLinkedId == vdkEventToUpdate.VidKnowLinkedId &&
                                          e.EvntTimeStopinSec == vidknowEvent.EvntTimeStopinSec &&
                                          e.Id != vdkEventToUpdate.Id);

            if (existingEvent != null)
            {
                ModelState.AddModelError("", "An event with the same VideoKnowledgeLinkedId and Event Time Stop already exists.");
                return View(vidknowEvent);
            }

            if (await TryUpdateModelAsync(vdkEventToUpdate, "", s => s.EvntWebLink, s => s.Description, s => s.EvntTimeStopinSec, s => s.EvntTimerDuration, s => s.EvntImage, s => s.EvntVideo, s => s.EvntWebVideoLink, s => s.EventType, s=>s.UserVideoPauseEvent))
            {
                vdkEventToUpdate.EventType = vidknowEvent.EventType;

                switch (vidknowEvent.EventType)
                {
                    case "1":
                        // Image event
                        vdkEventToUpdate.EvntVideo =
                        vdkEventToUpdate.EvntWebVideoLink =
                        vdkEventToUpdate.EvntQuizName =
                        vdkEventToUpdate.EvntWebLink = null;
                        break;
                    case "2":
                        // Questionary event
                        vdkEventToUpdate.EvntWebVideoLink =
                        vdkEventToUpdate.EvntWebLink =
                        vdkEventToUpdate.EvntImage =
                        vdkEventToUpdate.EvntVideo = null;
                        break;
                    case "3":
                        // Web content link
                        vdkEventToUpdate.EvntWebVideoLink =
                        vdkEventToUpdate.EvntQuizName =
                        vdkEventToUpdate.EvntImage =
                        vdkEventToUpdate.EvntVideo = null;
                        break;
                    case "4":
                        // Local video
                        vdkEventToUpdate.EvntWebVideoLink =
                        vdkEventToUpdate.EvntQuizName =
                        vdkEventToUpdate.EvntImage =
                        vdkEventToUpdate.EvntWebLink = null;
                        break;
                    case "5":
                        // Web video
                        vdkEventToUpdate.EvntVideo =
                        vdkEventToUpdate.EvntQuizName =
                        vdkEventToUpdate.EvntImage =
                        vdkEventToUpdate.EvntWebLink = null;
                        break;
                    default:
                        break;
                }
                //Verifica che l'evento non sia un quiz in quel caso se prima lo era cancella le domande
                if (vidknowEvent.EventType != "2" && vidknowEvent.QuestionList != null)
                {
                    foreach (var QuestionElement in vidknowEvent.QuestionList)
                    {
                        vidknowEvent.QuestionList.Remove(QuestionElement);
                    }
                }

                try
                {
                    // Gestione dell'upload di immagini e video
                    if (vidknowEvent.EvntImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/imagesOfKnowledges");
                        string imageName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknowEvent.EvntImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await vidknowEvent.EvntImageUpload.CopyToAsync(fs);
                        }
                        vdkEventToUpdate.EvntImage = imageName;
                    }

                    if (vidknowEvent.EvntVideoUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/videosOfKnowledges");
                        string videoName = this.User.Identity.Name + "_" + Guid.NewGuid().ToString() + "_" + vidknowEvent.EvntVideoUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, videoName);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            await vidknowEvent.EvntVideoUpload.CopyToAsync(fs);
                        }
                        vdkEventToUpdate.EvntVideo = videoName;
                    }

                    if (vidknowEvent.EvntWebVideoLink != null)
                    {
                        vdkEventToUpdate.EvntWebVideoLink = vidknowEvent.EvntWebVideoLink;
                    }

                    // Gestione del Video Pause Event
                    if (vidknowEvent.UserVideoPauseEvent != vdkEventToUpdate.UserVideoPauseEvent)
                    {
                        var linkedVdk = await _context.VideoKnowledgeContents.FirstOrDefaultAsync(s => s.Id == vdkEventToUpdate.VidKnowLinkedId);
                        if (linkedVdk != null)
                        {
                            if (linkedVdk.EvntOnPause != null)
                            {
                                ModelState.AddModelError("", "A pause event already exists, please edit the existing pause event or delete it to recreate a new one");
                                return View(vdkEventToUpdate);
                            }

                            if (vidknowEvent.UserVideoPauseEvent)
                            {
                                linkedVdk.EvntList.Remove(vdkEventToUpdate);
                                linkedVdk.EvntOnPause = vidknowEvent;
                                linkedVdk.EvntOnPause.VidKnowLinkedId = linkedVdk.Id;
                                linkedVdk.EvntOnPause.VidKnowOnPauseId = linkedVdk.Id;
                                //vidknowEvent.VidKnowLinkedId = vdkToUpdate.Id;
                                //vidknowEvent.VidKnowOnPauseId = vdkToUpdate.Id;
                            }
                            else
                            {
                                linkedVdk.EvntList.Add(vdkEventToUpdate);
                                linkedVdk.EvntOnPause = null;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Redirect("/admin/VideoKnowledgesContents/Edit/" + vdkEventToUpdate.VidKnowLinkedId);
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(vdkEventToUpdate);
        }



        /// <summary>
        /// Elimina un evento specifico e tutte le entità correlate, inclusi quiz, immagini e video associati.
        /// </summary>
        /// <param name="id">ID dell'evento da eliminare.</param>
        /// <returns>
        /// Reindirizza alla pagina di modifica del contenuto video associato.
        /// </returns>
        public async Task<IActionResult> Delete(long id)
        {
            VideoKnowledgeEvent vidknowEvent = await _context.VideoKnowledgeEvents.FindAsync(id);
            VideoKnowledgeContent vidknow = await _context.VideoKnowledgeContents.FindAsync(vidknowEvent.VidKnowLinkedId);

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
                foreach (var QuestionElement in vidknowEvent.QuestionList)
                {
                    vidknowEvent.QuestionList.Remove(QuestionElement);
                }
            }

            long? LinkedVideoKnowledge = vidknowEvent.VidKnowLinkedId;
            _context.VideoKnowledgeEvents.Remove(vidknowEvent);

            // Se l'evento è legato alla pausa dell'utente, rimuovilo
            if (vidknowEvent.UserVideoPauseEvent)
            {
                vidknow.EvntOnPause = null;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "L'evento è stato eliminato!";

            return Redirect("/admin/VideoKnowledgesContents/Edit/" + LinkedVideoKnowledge);
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VideoKnowledge.Models;
using VideoKnowledge.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using VideoKnowledge.Models.ViewModels;
using System.Collections.Generic;

namespace VideoKnowledge.Areas.Admin.Controllers
{
    /// <summary>
    /// Gestisce le operazioni CRUD (Create, Read, Update, Delete) relative alle domande dei quiz associati a eventi di conoscenza video.
    /// Comprende anche la visualizzazione dei risultati dei quiz completati dagli utenti.
    /// Accessibile agli utenti autenticati e autorizzati.
    /// </summary>
    [Area("Admin")]
    [Authorize]
    public class VideoKnowledgesQuestionsController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VideoKnowledgesQuestionsController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Visualizza i risultati dei quiz completati dall'utente corrente.
        /// Recupera i quiz completati, i relativi eventi e contenuti video, e li associa in un ViewModel.
        /// </summary>
        /// <returns>Vista contenente i risultati dei quiz completati.</returns>
        public async Task<IActionResult> UserQuizResults()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Ottieni tutti i quiz completati dall'utente
            var completedQuizzes = await _context.UserTestResults
                .Where(utr => utr.UserId == userId)
                .ToListAsync();

            // Estrai tutti gli ID di VideoKnowledgeEvents dai quiz completati
            var videoEventIds = completedQuizzes.Select(q => q.VidEventId).ToList();

            // Ottieni tutti i VideoKnowledgeEvents corrispondenti
            var videoEvents = await _context.VideoKnowledgeEvents
                .Where(ve => videoEventIds.Contains(ve.Id))
                .ToListAsync();

            // Estrai tutti gli ID di VideoKnowledgeContents associati ai VideoKnowledgeEvents
            var videoContentIds = videoEvents.Select(ve => ve.VidKnowLinkedId).ToList();

            // Ottieni tutti i VideoKnowledgeContents corrispondenti
            var videoContents = await _context.VideoKnowledgeContents
                .Where(vc => videoContentIds.Contains(vc.Id))
                .Include(vkc => vkc.Category) // Include la categoria per visualizzarla nella vista
                .ToListAsync();

            // Creazione di un dizionario per mappare VideoEventId a VidKnowId e EvntQuizName
            var eventToContentMap = videoEvents.ToDictionary(ve => ve.Id, ve => new { ve.VidKnowLinkedId, ve.EvntQuizName });

            // Assegnazione di VidKnowId e Name a ogni UserTestResults
            foreach (var quiz in completedQuizzes)
            {
                if (eventToContentMap.ContainsKey(quiz.VidEventId))
                {
                    quiz.VidKnowId = eventToContentMap[quiz.VidEventId].VidKnowLinkedId;
                    quiz.Name = eventToContentMap[quiz.VidEventId].EvntQuizName;
                }
            }

            // Crea il ViewModel con i dati ottenuti
            var viewModel = new UserCompletedQuizzesViewModel
            {
                VideoKnowledgeContents = videoContents,
                UserTestResults = completedQuizzes
            };

            return View(viewModel);
        }

        /// <summary>
        /// Mostra l'elenco delle domande di un evento quiz specifico.
        /// Permette il filtraggio per un evento selezionato.
        /// </summary>
        /// <param name="SelectedEvent">ID dell'evento selezionato per filtrare le domande.</param>
        /// <returns>Vista con l'elenco delle domande associate all'evento specifico.</returns>
        public ActionResult Index(int? SelectedEvent)
        {
            var eventContainer = _context.VideoKnowledgeEvents.OrderBy(q => q.EvntQuizName).ToList();
            ViewBag.SelectedVideoKnowledgeContent = new SelectList(eventContainer, "videoKnowledgeContentId", "Name", SelectedEvent);
            int vdkEventId = SelectedEvent.GetValueOrDefault();

            IQueryable<Question> VideoKnowledgeEvents = _context.Questions
                .Where(c => !SelectedEvent.HasValue || c.EventId == vdkEventId)
                .OrderBy(d => d.Id)
                .Include(d => d.Evnt);
            var sql = VideoKnowledgeEvents.ToString();
            return View(VideoKnowledgeEvents.ToList());
        }

        /// <summary>
        /// Mostra l'interfaccia per giocare un quiz associato a un evento video.
        /// Recupera le domande del quiz e il contenuto video correlato.
        /// </summary>
        /// <param name="id">ID dell'evento del quiz.</param>
        /// <returns>Vista con i dettagli del quiz e le sue domande.</returns>
        public async Task<IActionResult> QuizPlayer(long id)
        {
            // Carica l'evento del quiz con le domande associate
            var vidknowEvnt = await _context.VideoKnowledgeEvents
                .Include(e => e.QuestionList)
                .Include(e => e.VidKnowLinked) // Include il contenuto del video associato
                .FirstOrDefaultAsync(e => e.Id == id);

            if (vidknowEvnt == null)
            {
                return NotFound();
            }

            // Inizializza SelectedAnswer a null per tutte le domande
            foreach (var question in vidknowEvnt.QuestionList)
            {
                question.SelectedAnswer = null; // Imposta SelectedAnswer a null per ogni domanda
            }

            return View(vidknowEvnt);
        }

        /// <summary>
        /// Elabora le risposte inviate da un utente per un quiz specifico.
        /// Calcola il punteggio, determina il risultato (superato o fallito), e salva i dati nel database.
        /// </summary>
        /// <param name="model">Dati del quiz con le risposte dell'utente.</param>
        /// <param name="evntId">ID dell'evento del quiz.</param>
        /// <returns>
        /// Un oggetto JSON contenente il risultato del quiz e il numero di risposte corrette e totali.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SubmitQuizAsync(VideoKnowledgeEvent model, long evntId)
        {
            if (model.QuestionList == null || !model.QuestionList.Any())
            {
                return Json(new { success = false, message = "Non ci sono domande nel quiz." });
            }

            // Calcolo delle risposte corrette
            int correctAnswers = 0;
            int totalQuestions = model.QuestionList.Count;

            foreach (var question in model.QuestionList)
            {
                var storedQuestion = _context.Questions.FirstOrDefault(q => q.Id == question.Id);
                if (storedQuestion != null && question.SelectedAnswer == storedQuestion.CorrectAnswer)
                {
                    correctAnswers++;
                }
            }

            // Recupero dell'ID utente corrente
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Verifica se esiste già un risultato del quiz per lo stesso evento e utente
            var existingResult = _context.UserTestResults
                .FirstOrDefault(utr => utr.VidEventId == evntId && utr.UserId == userId);

            if (existingResult != null)
            {
                // Se esiste, elimina il risultato precedente
                _context.UserTestResults.Remove(existingResult);
            }

            // Crea un nuovo risultato del quiz
            UserTestResults usrQuizResult = new UserTestResults
            {
                VidEventId = evntId,
                UserId = userId,
                Name = model.Name,
                QuestionsResult = $"Correct Answer: {correctAnswers} on {totalQuestions}, total questions",
                QuizResult = (totalQuestions / 2) < correctAnswers ? "Passed" : "Failed"
            };

            // Aggiungi il nuovo risultato del quiz al contesto
            _context.UserTestResults.Add(usrQuizResult);

            // Salva le modifiche nel database
            await _context.SaveChangesAsync();

            // Restituisce i risultati come JSON
            return Json(new { success = true, totalQuestions, correctAnswers });
        }

        /// <summary>
        /// Fornisce la vista per creare una nuova domanda associata a un evento quiz.
        /// </summary>
        /// <returns>Vista per creare una nuova domanda.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Aggiunge una nuova domanda a un evento quiz esistente.
        /// Aggiorna la lista di domande associata all'evento e salva i dati nel database.
        /// </summary>
        /// <param name="vidEvntQuizQuestion">Dati della domanda da aggiungere.</param>
        /// <param name="Id">ID dell'evento quiz associato.</param>
        /// <returns>
        /// In caso di successo, reindirizza alla pagina di modifica dell'evento quiz.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question vidEvntQuizQuestion, long Id)
        {
            if (ModelState.IsValid)
            {
                var eventQuestionaryToUpdate = await _context.VideoKnowledgeEvents.FirstOrDefaultAsync(s => s.Id == Id);

                if (await TryUpdateModelAsync<VideoKnowledgeEvent>(
                    eventQuestionaryToUpdate,
                    "",
                    s => s.EvntQuizName, s => s.QuestionList))
                {
                    try
                    {
                        vidEvntQuizQuestion.Id = 0;
                        eventQuestionaryToUpdate.QuestionList ??= new List<Question>();
                        eventQuestionaryToUpdate.QuestionList.Add(vidEvntQuizQuestion);
                    }
                    catch (DbUpdateException /* ex */)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
            }
            else
            {
                return View(vidEvntQuizQuestion);
            }

            vidEvntQuizQuestion.EventId = Id;
            _context.Add(vidEvntQuizQuestion);
            await _context.SaveChangesAsync();

            TempData["Success"] = "The Video Knowledge Event has been created!";

            return Redirect("/admin/VideoKnowledgesEvents/Edit/" + Id);
        }

        /// <summary>
        /// Recupera i dati di una domanda specifica per modificarla.
        /// </summary>
        /// <param name="id">ID della domanda da modificare.</param>
        /// <returns>Vista con i dati della domanda da modificare.</returns>
        public async Task<IActionResult> Edit(long id)
        {
            Question vidEvntQuizQuestion = await _context.Questions.FindAsync(id);
            return View(vidEvntQuizQuestion);
        }

        /// <summary>
        /// Aggiorna i dati di una domanda esistente associata a un evento quiz.
        /// Verifica e salva le modifiche nel database.
        /// </summary>
        /// <param name="vidEvntQuizQuestion">Dati aggiornati della domanda.</param>
        /// <param name="Id">ID della domanda da modificare.</param>
        /// <returns>
        /// In caso di successo, reindirizza alla pagina di modifica dell'evento quiz.
        /// In caso di errore, restituisce la vista con i messaggi di errore.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Question vidEvntQuizQuestion, long Id)
        {
            if (ModelState.IsValid)
            {
                var eventQuestionaryToUpdate = await _context.Questions.FirstOrDefaultAsync(s => s.Id == Id);

                if (await TryUpdateModelAsync<Question>(
                    eventQuestionaryToUpdate,
                    "",
                    s => s.QuestionText, s => s.AnswerA, s => s.AnswerB, s => s.AnswerC, s => s.AnswerD))
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Redirect("/admin/VideoKnowledgesEvent/Edit/" + Id);
                    }
                    catch (DbUpdateException /* ex */)
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
            }
            else
            {
                return View(vidEvntQuizQuestion);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "The Video Knowledge Event has been created!";
            return Redirect("/admin/VideoKnowledgesEvents/Edit/" + Id);
        }

        /// <summary>
        /// Mostra i quiz completati dall'utente corrente o da tutti gli utenti, a seconda del ruolo.
        /// Recupera i risultati e i contenuti video associati.
        /// </summary>
        /// <returns>Vista contenente i dati dei quiz completati.</returns>
        public async Task<IActionResult> CompletedQuizzes()
        {
            // Recupera l'ID dell'utente corrente
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            // Controlla se l'utente è un amministratore
            bool isAdmin = User.IsInRole("Admin");
            //var completedQuizzes;
            List<UserTestResults> completedQuizzes;

            if (isAdmin)
            {
                completedQuizzes= await _context.UserTestResults.ToListAsync();
            }
            else
            {
                // Recupera tutti i quiz completati dall'utente corrente
                completedQuizzes = await _context.UserTestResults
                    .Where(utr => utr.UserId == userId)
                    .ToListAsync();
            }

            // Recupera anche i VideoKnowledgeContents associati a questi quiz
            var videoContents = await _context.VideoKnowledgeContents
                .Where(vkc => completedQuizzes.Select(utr => utr.VidEventId).Contains(vkc.Id))
                .ToListAsync();

            // Passa i dati alla vista usando un ViewModel
            var viewModel = new UserCompletedQuizzesViewModel
            {
                UserTestResults = completedQuizzes,
                VideoKnowledgeContents = videoContents
            };

            return View(viewModel);
        }
    }
}

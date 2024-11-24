using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VideoKnowledge.Infrastructure;
using VideoKnowledge.Models;
using static System.Net.WebRequestMethods;

public class SeedData
{
    public static void SeedDatabase(DataContext context)
    {
        context.Database.Migrate();
        // Inserisci le categorie se non esistono
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
        {
            new Category { Name = "Teaching", Slug = "teaching" },
            new Category { Name = "HowTo", Slug = "howto" },
            new Category { Name = "Review", Slug = "review" }
        };
            context.Categories.AddRange(categories);
            context.SaveChanges(); // Salva le categorie
        }

        // Recupera gli ID delle categorie appena inserite
        var teaching = context.Categories.FirstOrDefault(c => c.Slug == "teaching");
        var howto = context.Categories.FirstOrDefault(c => c.Slug == "howto");
        var review = context.Categories.FirstOrDefault(c => c.Slug == "review");

        // Verifica se ci sono già dati
        if (!context.VideoKnowledgeContents.Any())
        {
            var videoContents = new List<VideoKnowledgeContent>
            {
                new VideoKnowledgeContent
                {
                    Name = "La storia alimentare",
                    Slug = "La-storia-alimentare",
                    Image ="LaCucinaNellaStoria.png",
                    Description = "Storia della cucina",
                    CategoryId = 1,
                    WebVideoLink = "https://www.youtube.com/embed/SUyYG3ALvpk",
                    VideoSource = "1",
                    VideoDuration = 1446,
                    EvntList = new List<VideoKnowledgeEvent>
                    {
                        new VideoKnowledgeEvent
                        {
                            Description = "Quiz sulla storia del cibo",
                            EvntTimeStopinSec = 10,
                            EvntTimerDuration = 120,
                            EventType = "2", // Quiz
                            EvntQuizName = "Storia del cibo",
                            QuestionList = new List<Question>
                            {
                                new Question { QuestionText = "Come si chiava il frate francescano che scriveva del cibo che trovava nei posti che visitava, nel lontano milleduecento ?", CorrectAnswer = "A", AnswerA= "Frà Salinbene", AnswerB = "Frà Solabene", AnswerC="Frà Solimene", AnswerD="Frà Martino" },
                                new Question { QuestionText = "Una delle prelibatezze della cucina Parmigiana veniva cucinata solo con la crosta. Di che piatto si tratta ?", CorrectAnswer = "B", AnswerA= "tortellini in brodo", AnswerB = "anolini in brodo con crosta di pane in forno", AnswerC="stinco in brodo", AnswerD="agnolotti in brodo" }
                            }
                        },
                        new VideoKnowledgeEvent
                        {
                            Description = "bio Barbero",
                            EvntTimeStopinSec = 15,
                            EvntTimerDuration = 2147483647,
                            EventType = "3", //Web content
                            EvntWebLink="https://it.wikipedia.org/wiki/Alessandro_Barbero"
                        },
                        new VideoKnowledgeEvent
                        {
                            Description = "cibo e storia dell'uomo",
                            EvntTimeStopinSec = 30,
                            EvntTimerDuration = 2147483647,
                            EventType = "5", //Web video
                            EvntWebLink="https://www.youtube.com/embed/LkJT_4qn634"
                        }

                    }
                },
                
                new VideoKnowledgeContent
                {
                    Name = "Vimeo Test",
                    Slug = "Vimeo-Test",
                    Image="VimeoTestCover.png",
                    Description = "Test Vimeo player",
                    CategoryId = 2,
                    WebVideoLink = "https://player.vimeo.com/video/76979871",
                    VideoSource = "1",
                    VideoDuration = 62,
                    EvntList = new List<VideoKnowledgeEvent>
                    {
                        new VideoKnowledgeEvent
                        {
                            Description = "Come accedere a Vimeo.com",
                            EvntTimeStopinSec = 40,
                            EvntTimerDuration = 20,
                            EventType = "5", // Web Video
                            EvntWebLink = "https://www.youtube.com/embed/ljU3i9IVf_U"
                        }
                    }
                }
            };

            // Aggiungi i dati al contesto
            context.VideoKnowledgeContents.AddRange(videoContents);
            context.SaveChanges();
        }
    }
}

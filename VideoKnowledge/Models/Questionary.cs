using System.ComponentModel.DataAnnotations.Schema;
using static VideoKnowledge.Models.Question;
using System.ComponentModel.DataAnnotations;

namespace VideoKnowledge.Models
{
    /// <summary>
    /// Class <c>Questionary</c> modella la domanda a risposta multipla
    /// </summary>
    public class Questionary
    {
        //public long Id { get; set; }
        //public string TestName { get; set; }
        //public List<Question> Answers { get; set; }

        //[Table("Quizzes")]

        [Key]
        public long Id { get; set; }

        [Required]
        public string TestName { get; set; }

        //[Required]
        //[Range(0, 10)]
        //public int NumberOfQuestions { get; set; }


        [Required]
        public virtual ICollection<Question> QuestionList { get; set; }
        //public Question[] QuestionList { get; set; } = new Question[10];

        //human-readable, unique identifier
        public string Slug { get; set; }

        //public virtual ICollection<Question> QuestionList { get; set; }
    }
}

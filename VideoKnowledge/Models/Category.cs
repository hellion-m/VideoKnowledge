namespace VideoKnowledge.Models
{
    /// <summary>
    /// Modella una categoria per i contenuti di VideoKnowledge.
    /// Ogni categoria ha un identificatore unico, un nome e uno slug associato.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Ottiene o imposta l'identificatore unico della categoria.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Ottiene o imposta il nome della categoria.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ottiene o imposta lo slug della categoria, usato per URL friendly.
        /// </summary>
        public string Slug { get; set; }
    }
}

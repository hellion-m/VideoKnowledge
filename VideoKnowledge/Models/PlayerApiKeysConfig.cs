namespace VideoKnowledge.Models
{
    /// <summary>
    /// Classe che memorizza le chiavi API e i token di accesso necessari per interagire con diverse piattaforme video.
    /// Contiene le chiavi per YouTube, Vimeo e Dailymotion.
    /// </summary>
    public class PlayerApiKeysConfig
    {
        /// <summary>
        /// La chiave API per accedere alle funzionalità di YouTube.
        /// </summary>
        public string YouTubeApiKey { get; set; }

        /// <summary>
        /// Il token di accesso per interagire con Vimeo.
        /// </summary>
        public string VimeoAccessToken { get; set; }

        /// <summary>
        /// La chiave API per accedere alle funzionalità di Dailymotion.
        /// </summary>
        public string DailymotionApiKey { get; set; }
    }
}

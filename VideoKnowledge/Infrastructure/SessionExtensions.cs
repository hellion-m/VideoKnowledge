using Newtonsoft.Json;

namespace VideoKnowledge.Infrastructure
{
    /// <summary>
    /// Estensioni per l'oggetto <see cref="ISession"/> per permettere il salvataggio e il recupero di oggetti JSON.
    /// Queste estensioni semplificano la serializzazione e la deserializzazione degli oggetti da e verso la sessione.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Aggiunge un oggetto serializzato come JSON alla sessione.
        /// Il valore dell'oggetto viene salvato come stringa JSON con la chiave specificata.
        /// </summary>
        /// <param name="session">La sessione corrente in cui salvare l'oggetto.</param>
        /// <param name="key">La chiave associata all'oggetto nella sessione.</param>
        /// <param name="value">L'oggetto da serializzare e salvare nella sessione.</param>
        public static void SetJson(this ISession session, string key, object value)
        {
            // Serializza l'oggetto in una stringa JSON e lo salva nella sessione
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Recupera un oggetto dalla sessione, deserializzandolo da JSON.
        /// </summary>
        /// <typeparam name="T">Il tipo dell'oggetto da recuperare.</typeparam>
        /// <param name="session">La sessione corrente da cui recuperare l'oggetto.</param>
        /// <param name="key">La chiave associata all'oggetto nella sessione.</param>
        /// <returns>L'oggetto deserializzato se esiste nella sessione, altrimenti il valore predefinito per il tipo T.</returns>
        public static T GetJson<T>(this ISession session, string key)
        {
            // Ottiene la stringa JSON dalla sessione
            var sessionData = session.GetString(key);

            // Se la sessione contiene dati, deserializza la stringa JSON in un oggetto di tipo T
            return sessionData == null ? default(T) : JsonConvert.DeserializeObject<T>(sessionData);
        }
    }
}


using System.Text.Json;
using System.Text.Json.Serialization;

namespace VideoKnowledge.Helpers
{
    /// <summary>
    /// La classe <c>JsonHelper</c> fornisce metodi di utilità per la serializzazione di oggetti in formato JSON.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Serializza un oggetto in formato JSON preservando i riferimenti agli oggetti.
        /// Questo è utile per gestire cicli di riferimento o oggetti che si riferiscono a se stessi.
        /// </summary>
        /// <typeparam name="T">Il tipo dell'oggetto da serializzare.</typeparam>
        /// <param name="obj">L'oggetto da serializzare.</param>
        /// <returns>Una stringa JSON che rappresenta l'oggetto serializzato.</returns>
        public static string SerializeWithReferences<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,  // Mantiene i riferimenti agli oggetti
                WriteIndented = true  // Formatta il JSON in modo leggibile (con indentazioni)
            };
            return JsonSerializer.Serialize(obj, options);  // Serializza l'oggetto con le opzioni specificate
        }
    }
}


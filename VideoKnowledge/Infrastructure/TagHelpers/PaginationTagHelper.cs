using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace CmsShopping.Infrastructure.TagHelpers
{
    /// <summary>
    /// La classe <c>PaginationTagHelper</c> estende <see cref="TagHelper"/> e fornisce una logica personalizzata per la generazione di una navigazione di paginazione HTML in un'applicazione ASP.NET Core.
    /// Questa classe consente di creare dinamicamente una barra di navigazione paginata, che può essere utilizzata nelle viste Razor per gestire la paginazione dei dati.
    /// </summary>
    public class PaginationTagHelper : TagHelper
    {
        /// <summary>
        /// Numero di pagina attualmente selezionato.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Numero di elementi per pagina.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Numero totale di pagine.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Numero di pagine visibili nella barra di navigazione.
        /// </summary>
        public int PageRange { get; set; }

        /// <summary>
        /// Testo da visualizzare per il primo elemento della paginazione (di default "First").
        /// </summary>
        public string PageFirst { get; set; }

        /// <summary>
        /// Testo da visualizzare per l'ultimo elemento della paginazione (di default "Last").
        /// </summary>
        public string PageLast { get; set; }

        /// <summary>
        /// URL di destinazione per la paginazione. Viene utilizzato per generare i link per ciascuna pagina.
        /// </summary>
        public string PageTarget { get; set; }

        /// <summary>
        /// Override del metodo <see cref="Process"/> di <see cref="TagHelper"/> che genera il contenuto HTML per la barra di navigazione di paginazione.
        /// </summary>
        /// <param name="context">Il contesto dell'elemento TagHelper.</param>
        /// <param name="output">L'output HTML generato da questo TagHelper.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Imposta il nome del tag e i relativi attributi
            output.TagName = "nav";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("aria-label", "Page navigation");
            output.Content.SetHtmlContent(AddPageContent()); // Aggiungi il contenuto di paginazione
        }

        /// <summary>
        /// Genera dinamicamente il contenuto HTML per la barra di navigazione di paginazione.
        /// </summary>
        /// <returns>Una stringa contenente il codice HTML per la barra di navigazione di paginazione.</returns>
        private string AddPageContent()
        {
            // Impostazioni predefinite per la paginazione
            if (PageRange == 0)
            {
                PageRange = 1; // Imposta un intervallo di paginazione minimo
            }

            if (PageCount < PageRange)
            {
                PageRange = PageCount; // Limita l'intervallo di paginazione al numero di pagine disponibili
            }

            // Imposta i valori di "First" e "Last" se non sono specificati
            if (string.IsNullOrEmpty(PageFirst))
            {
                PageFirst = "First";
            }

            if (string.IsNullOrEmpty(PageLast))
            {
                PageLast = "Last";
            }

            var content = new StringBuilder();
            content.Append(" <ul class='pagination'>");

            // Link per la prima pagina, se non siamo già sulla prima pagina
            if (PageNumber != 1)
            {
                content.Append($"<li class='page-item'><a class='page-link' href='{PageTarget}'>{PageFirst}</a></li>");
            }

            // Logica per generare i numeri delle pagine visibili
            if (PageNumber <= PageRange)
            {
                // Pagina iniziale
                for (int currentPage = 1; currentPage < 2 * PageRange + 1; currentPage++)
                {
                    if (currentPage < 1 || currentPage > PageCount)
                    {
                        continue; // Esclude le pagine non valide
                    }
                    var active = currentPage == PageNumber ? "active" : ""; // Evidenzia la pagina attiva
                    content.Append($"<li class='page-item {active}'><a class='page-link' href='{PageTarget}?p={currentPage}'>{currentPage}</a></li>");
                }
            }
            else if (PageNumber > PageRange && PageNumber < PageCount - PageRange)
            {
                // Pagine centrali
                for (int currentPage = PageNumber - PageRange; currentPage < PageNumber + PageRange; currentPage++)
                {
                    if (currentPage < 1 || currentPage > PageCount)
                    {
                        continue; // Esclude le pagine non valide
                    }
                    var active = currentPage == PageNumber ? "active" : ""; // Evidenzia la pagina attiva
                    content.Append($"<li class='page-item {active}'><a class='page-link' href='{PageTarget}?p={currentPage}'>{currentPage}</a></li>");
                }
            }
            else
            {
                // Pagine finali
                for (int currentPage = PageCount - (2 * PageRange); currentPage < PageCount + 1; currentPage++)
                {
                    if (currentPage < 1 || currentPage > PageCount)
                    {
                        continue; // Esclude le pagine non valide
                    }
                    var active = currentPage == PageNumber ? "active" : ""; // Evidenzia la pagina attiva
                    content.Append($"<li class='page-item {active}'><a class='page-link' href='{PageTarget}?p={currentPage}'>{currentPage}</a></li>");
                }
            }

            // Link per l'ultima pagina, se non siamo già sull'ultima pagina
            if (PageNumber != PageCount)
            {
                content.Append($"<li class='page-item'><a class='page-link' href='{PageTarget}?p={PageCount}'>{PageLast}</a></li>");
            }

            content.Append(" </ul>");
            return content.ToString(); // Restituisce l'HTML generato
        }
    }
}

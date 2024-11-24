Il progetto ha l'obiettivo di sviluppare un sistema di gestione dei contenuti video basato su ASP.NET Core MVC 6, denominato "VideoKnowledge". Questo sistema è progettato per integrare funzionalità interattive con video provenienti da piattaforme come YouTube e Vimeo o caricati localmente, permettendo la riproduzione di video con eventi personalizzati, quali immagini, link e questionari. L'obiettivo primario è quello di creare un player video che non solo riproduca contenuti multimediali, ma che interagisca anche con l'utente, arricchendo l'esperienza di visione attraverso l'inclusione di elementi interattivi.


Debug del progetto :

Il progetto è stato sviluppato con Visual Studio 2022.
All'apertura della soluzione eseguire sul terminale : 

dotnet ef migrations add InitialMigration

//creazione del database
dotnet ef database update

Una volta eseguita l'applicazione procedere con la creazione dell'utente admin
admin è l'amministratore del sistema e gestisce ogni contenuto della piattaforma.

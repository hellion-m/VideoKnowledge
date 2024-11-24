 var events = @Html.Raw(events);
 var evntOnPause = @Html.Raw(evntOnPauseJson);
 var quizPlayerUrl = '@Url.Action("QuizPlayer", "VideoKnowledgesQuestions", new { id = "PLACEHOLDER" })';
       
let linkClosed = false;
let handledEvents = new Set();
let currentEventIndex = 0;
let eventVideoPlaying = false;
let stoppedTime = 0;
let timerInterval; // Variabile globale per l'intervallo del timer

let videoOnpauseRunning=0;
        

if (events && events.$values) {
    events.$values.sort((a, b) => a.EvntTimeStopinSec - b.EvntTimeStopinSec);
}

    const Videoplayer = (function makeVideoPlayer() {
    let player = null;      // Player YouTube
    let eventPlayer = null; // Player evento YouTube
    let vimeoPlayer = null; // Player Vimeo

    function onYouTubeIframeAPIReady() {
        player = new YT.Player('youtube-video', {
            events: {
                'onReady': onPlayerReady,
                'onStateChange': onPlayerStateChange
            }
        });
        eventPlayer = new YT.Player('youtube-event-video', {
            events: {
                'onReady': function(event) {
                    eventPlayer = event.target;
                }
            }
        });
    }

    window.onYouTubeIframeAPIReady = onYouTubeIframeAPIReady;

    function onPlayerReady(event) {
        player = event.target;
        checkTimeUpdate();
        addStopEventListener();
    }

    function onPlayerStateChange(event) {
        if (event.data == YT.PlayerState.PLAYING) {
            checkTimeUpdate();
        }
        if (event.data == YT.PlayerState.PAUSED) {
            handleStopEvent();
        }
    }

    function play() {
        if (player && typeof player.playVideo === 'function') {
            player.playVideo();
        } else {
            console.error("playVideo is not a function or player is not initialized");
        }
    }

    function getPlayer() {
        return player;
    }

    function getEventPlayer() {
        return eventPlayer;
    }

    function init() {
        if (typeof YT === 'undefined' || typeof YT.Player === 'undefined') {
            const tag = document.createElement("script");
            tag.src = "https://www.youtube.com/iframe_api";
            const firstScriptTag = document.getElementsByTagName("script")[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        } else {
            onYouTubeIframeAPIReady();
        }
    }

    function addStopEventListener() {
        var webVideoLink = '@Model.WebVideoLink';
        var vimeoPlayer = getVimeoPlayer();
        if (webVideoLink.includes("vimeo.com") && vimeoPlayer) {
            vimeoPlayer.addEventListener('onStateChange', function(event) {
                if (event.data == YT.PlayerState.PAUSED) {
                    handleStopEvent();
                }
            });
        }else{
            player.addEventListener('onStateChange', onPlayerStateChange);
        }
    }

    // Funzione per inizializzare il player Vimeo
    function initVimeoPlayer(vimeoContainerId, vimeoVideoLink) {
        vimeoPlayer = new Vimeo.Player(vimeoContainerId, {
            url: vimeoVideoLink,
            autoplay: true,
            muted: true // Imposta su true se vuoi silenziare il video all'avvio
        });

        // Usa la promise 'ready' per verificare che il player Vimeo sia pronto
        vimeoPlayer.ready().then(function() {
            // Player Vimeo è pronto
            console.log('Player Vimeo inizializzato correttamente.');

            vimeoPlayer.on('play', function() {
                console.log('Il video Vimeo è stato riprodotto.');
                checkVimeoTimeUpdate(); // Inizializza il controllo del tempo per Vimeo
            });

            vimeoPlayer.on('timeupdate', function(data) {
                // Gestisci aggiornamenti di tempo per Vimeo
                checkVimeoTimeUpdate(data);
                console.log(`Tempo attuale video Vimeo: ${data.seconds}`);
            });

            vimeoPlayer.on('pause', function() {
                console.log('Vimeo video is paused');
                handleStopEvent();
            });

            vimeoPlayer.on('ended', function() {
                console.log('Il video Vimeo è terminato.');
            });

            // Avvia la riproduzione
            vimeoPlayer.play().catch(function(error) {
                console.error('Errore durante la riproduzione del video Vimeo:', error);
            });
        }).catch(function(error) {
            console.error('Errore durante l\'inizializzazione del player Vimeo:', error);
        });
    }

    function getVimeoPlayer() {
        return vimeoPlayer;
    }

    return {
        getPlayer,
        getEventPlayer,
        init,
        play,
        initVimeoPlayer,
        getVimeoPlayer
    };
}());

Videoplayer.init();


function playVideo() {
    $('.cover').hide();
    $('.container').show();
    $('#youtube-container').show();
    $('#youtubeEvent-container').hide();

    var videoSource = @Model.VideoSource;
    var webVideoLink = '@Model.WebVideoLink';

    if (videoSource == "1") {
            // Video Vimeo
        if (webVideoLink.includes("vimeo.com")) {
                    
            Videoplayer.initVimeoPlayer('vimeo-video-container', webVideoLink); // Aggiungi l'inizializzazione Vimeo qui
            $('#youtube-container').hide();
            $('#local-video-container').hide();
            $('#video-not-supported-container').hide();
            $('#vimeo-video-container').show();
        } 
        // Video YouTube
        else if (webVideoLink.includes("youtube.com") || webVideoLink.includes("youtu.be")) {
            attemptPlayVideo();
        } else {
            // Caso fallback per video locali
            // attemptPlayLocalVideo();
            showVideoNotSupportedMessage();
        }
    } else if (videoSource == "2") {
        var player = videojs('local-video'); // Inizializza il player Video.js
        var videoLink = "/media/videosOfKnowledges/@Model.VideoLink"; // Percorso del video locale

        // Imposta la sorgente del video e mostra il player
        player.src({ type: 'video/mp4', src: videoLink });
        $('#youtube-container').hide(); // Nascondi il container di YouTube
        $('#local-video-container').show(); // Mostra il container del video locale

        player.play(); // Avvia la riproduzione del video con Video.js
        checkLocalVideoTimeUpdate(player); // Gestisce gli eventi di tempo con Video.js
    }
}

function playEventVideo(eventVideoUrl) {
    var videoSource = @Model.VideoSource;
    if (videoSource == "1") {
        $('#youtube-container').hide();
        $('#vimeo-video-container').hide();
    }else{
        $('#local-video-container').hide();
    }
    $('#youtubeEvent-container').show();

    $('#youtube-event-video').attr('src', eventVideoUrl + "?enablejsapi=1&autoplay=1");
    eventVideoPlaying = true;
}

function attemptPlayVideo() {
        $('#vimeo-video-container').hide();
        // $('#local-video-container').hide();
        $('#video-not-supported-container').hide();
    const player = Videoplayer.getPlayer();
    if (player) {
        player.playVideo();
        var playCheckInterval = setInterval(function() {
            if (player.getPlayerState() === YT.PlayerState.PLAYING) {
                clearInterval(playCheckInterval);
                checkTimeUpdate();
            } else {
                player.playVideo();
            }
        }, 1000);
    } else {
        console.error("Player is not initialized.");
    }
}

function showVideoNotSupportedMessage() {
    // Nascondi tutti i player
    $('#youtube-container').hide();
    $('#youtube-video-container').hide();
    $('#vimeo-video-container').hide();
    $('#local-video-container').hide();
    
    // Mostra il messaggio di video non supportato
    $('#video-not-supported-container').show();
}
function checkVimeoTimeUpdate() {
    if (!Array.isArray(events)) {
        events = [events];
    }

    events.sort(function(a, b) {
        return a.$values[0].EvntTimeStopinSec - b.$values[0].EvntTimeStopinSec;
    });

    var checkInterval = setInterval(function() {
        const player = Videoplayer.getPlayer();
        const vimeoPlayer = Videoplayer.getVimeoPlayer();
        var webVideoLink = '@Model.WebVideoLink';
        var currentTime = 0;
                
        if (vimeoPlayer && webVideoLink.includes("vimeo.com")) {
            if (!videoOnpauseRunning){
                checkEvent(vimeoPlayer.getCurrentTime());
            }
            // videoOnpauseRunning=false;
            } else {
                console.error("Vimeo player non è pronto o non è stato inizializzato correttamente.");
            }

    }, 500);
}

function checkTimeUpdate() {
    if (!Array.isArray(events)) {
        events = [events];
    }

    events.sort(function(a, b) {
        return a.$values[0].EvntTimeStopinSec - b.$values[0].EvntTimeStopinSec;
    });

    var checkInterval = setInterval(function() {
        const player = Videoplayer.getPlayer();
        const vimeoPlayer = Videoplayer.getVimeoPlayer();
        var webVideoLink = '@Model.WebVideoLink';
        var currentTime = 0;

        if (player && (webVideoLink.includes("youtube.com") || webVideoLink.includes("youtu.be"))) {
            // Se è un video YouTube
            currentTime = Math.floor(player.getCurrentTime());
            checkEvent(currentTime);
        // } else if (vimeoPlayer && webVideoLink.includes("vimeo.com")) {
        //     if (typeof vimeoPlayer.getCurrentTime === 'function') {
        //         vimeoPlayer.getCurrentTime().then(function(seconds) {
        //             currentTime = Math.floor(seconds);
        //             checkEvent(currentTime);
        //         }).catch(function(error) {
        //             console.error("Errore durante il recupero del tempo da Vimeo:", error);
        //         });
        //     } else {
        //         console.error("Vimeo player non è pronto o non è stato inizializzato correttamente.");
        //     }
        } else {
            console.error("Nessun player trovato (YouTube o Vimeo).");
        }

    }, 500);
}


function checkEvent(currentTime) {
    if (currentEventIndex < events[0].$values.length) {
        var currentEvent = events[0].$values[currentEventIndex];
        if (currentTime >= Math.floor(currentEvent.EvntTimeStopinSec) && !handledEvents.has(currentEvent.Id)) {
            pauseVideo();
            handleEvent(currentEvent);
            handledEvents.add(currentEvent.Id);
            currentEventIndex++;
        }
    }
}


function checkLocalVideoTimeUpdate(videoElement) {
            
    if (!Array.isArray(events)) {
        events = [events];
    }

    events.sort(function(a, b) {
        return a.$values[0].EvntTimeStopinSec - b.$values[0].EvntTimeStopinSec;
    });

    var checkInterval = setInterval(function() {
        // Utilizza videoElement.currentTime() se videoElement è un oggetto Video.js
        var currentTime = Math.floor(videoElement.currentTime());
                
        // Verifica che gli eventi siano definiti e che non siano terminati
        if (events && events.length > 0 && currentEventIndex < events[0].$values.length) {
            var currentEvent = events[0].$values[currentEventIndex];
            if (currentTime >= Math.floor(currentEvent.EvntTimeStopinSec) && !handledEvents.has(currentEvent.Id)) {
                videoElement.pause(); // Pausa il video con Video.js
                handleEvent(currentEvent); // Gestisce l'evento
                handledEvents.add(currentEvent.Id);
                currentEventIndex++;
                clearInterval(checkInterval);
            }
                    
        } else {
            clearInterval(checkInterval);
        }
    }, 500);
}

function handleEvent(event) {
    switch (event.EventType) {
        case "1":
            pauseVideo();
            showImage(event.EvntImage);
            break;
        case "4":
            playEventVideo("/media/videosOfKnowledges/" + event.EvntVideo);
            break;
        case "5":
            pauseVideo();
            playEventVideo(event.EvntWebVideoLink);
            break;
        case "3":
            pauseVideo();
            showLinkMessage(event.EvntWebLink, event.Description);
            openLinkInNewWindow(event.EvntWebLink);
            break;
        case "2":
            pauseVideo();
            showQuiz(event.Id, event.QuestionList, event.EvntTimerDuration);
            break;
        default:
            pauseVideo();
            console.log('Tipo di evento sconosciuto: ' + event.EventType);
            break;
    }
}

function showImage(imageSrc) {
    $('#youtube-container').hide();
    $('#vimeo-video-container').hide();
    const imageElement = document.getElementById('event-image');
    const imageContainer = document.getElementById('event-image-container');
    var imagePath="/media/imagesOfKnowledges/" + imageSrc
    imageElement.src = imagePath;
    imageContainer.style.display = 'block';
}

function pauseVideo() {
    const player = Videoplayer.getPlayer();
    const vimeoPlayer = Videoplayer.getVimeoPlayer();  // Player di Vimeo
    if (vimeoPlayer && typeof vimeoPlayer.pause === 'function') {
        vimeoPlayer.pause().then(function() {
            console.log('Il video Vimeo è stato messo in pausa.');
        }).catch(function(error) {
            console.error('Errore durante la pausa del video Vimeo:', error);
        });
    }
    else if (player && typeof player.pauseVideo === 'function') {
        player.pauseVideo();
    }
    else{
        console.error('Nessun player disponibile per mettere in pausa il video.');
    }
}

function showLinkMessage(link, description) {
    $('#event-description').text(description);
    $('#link-message').show();
    $('#website-link').attr('href', link);
}

function resumeVideo() {
    // Nascondi l'eventuale video dell'evento
    const evntplayer = Videoplayer.getEventPlayer();
    if (eventVideoPlaying) {
        $('#youtube-event-video').attr('src', ""); // Svuota la sorgente del video dell'evento
        $('#youtubeEvent-container').hide();
        eventVideoPlaying = false;
    }

    // Nascondi messaggi e altre componenti non pertinenti
    $('#link-message').hide();
    $('#event-image-container').hide();
    $('#event-image').hide();

    // Ottieni il tipo di video corrente dal Model (per distinguere locale o YouTube)
    var videoSource = @Model.VideoSource;

    if (videoSource == "1") {
        var webVideoLink = '@Model.WebVideoLink';

        if (webVideoLink.includes("vimeo.com")) {
            $('#vimeo-video-container').show();
            //Gestione Resume Video Vimero
            const vimeoPlayer = Videoplayer.getVimeoPlayer();
            if (vimeoPlayer && typeof vimeoPlayer.play === 'function') {
                vimeoPlayer.play().then(function() {
                    console.log('Il video Vimeo è stato ripreso.');
                    if(videoOnpauseRunning>0){
                    videoOnpauseRunning=0;
                    }else{
                    checkVimeoTimeUpdate();
                    }
                    // checkTimeUpdateVimeo(); // Gestisci il controllo degli eventi per Vimeo
                }).catch(function(error) {
                    console.error('Errore durante la ripresa del video Vimeo:', error);
                });
            } else {
                console.error('Il player Vimeo non è stato correttamente inizializzato.');
            }
        }
        else {
            // Video YouTube
            $('#youtube-container').show();
            const player = Videoplayer.getPlayer();
            if (player && typeof player.playVideo === 'function') {
                player.playVideo(); // Riprendi la riproduzione del video YouTube
                checkTimeUpdate();  // Gestisci il controllo degli eventi
            }
        }
    }else if (videoSource == "2") {
        // Video locale
        $('#youtube-container').hide(); // Nascondi il container di YouTube
        $('#local-video-container').show(); // Mostra il container del video locale

        var localPlayer = videojs('local-video'); // Ottieni il player Video.js

        // Verifica che il player locale sia stato inizializzato correttamente
        if (localPlayer && typeof localPlayer.play === 'function') {
            localPlayer.play(); // Riprendi la riproduzione del video locale
            checkLocalVideoTimeUpdate(localPlayer); // Gestisci il controllo degli eventi per il video locale
        }
    }
}

function openLinkInNewWindow(link) {
    const newWindow = window.open(link, '_blank');
    if (newWindow) {
        const checkWindowClosed = setInterval(function() {
            if (newWindow.closed) {
                clearInterval(checkWindowClosed);
                linkClosed = true;
                resumeVideo();
            }
        }, 1000);
    } else {
        alert('Consenti i popup per vedere il link.');
        linkClosed = true;
    }
    pauseVideo();
}

function handleStopEvent() {
    videoOnpauseRunning=1;
        if (evntOnPause) {
            handleEvent(evntOnPause);
        }
}

function showQuiz(eventId, questions, timerDuration) {
    $('#youtube-container').hide();
    $('#quiz-container').show();

    const quizContainer = $('#quiz-questions');
    quizContainer.empty();

    if (questions) {
        questions.$values.forEach((question, index) => {
            const questionHtml = `
                <div class="border bg-light p-4 mb-4">
                    <div class="form-group row">
                        <div class="col-12">
                            <label>${question.QuestionText}</label>
                        </div>
                    </div>
                    <br />
                    <div class="form-group row">
                        <div class="col-6">
                            <input type="radio" name="QuestionList[${index}].SelectedAnswer" value="A" />
                            <label>${question.AnswerA}</label>
                        </div>
                        <div class="col-6">
                            <input type="radio" name="QuestionList[${index}].SelectedAnswer" value="B" />
                            <label>${question.AnswerB}</label>
                        </div>
                    </div>
                    <div class="form-group row">
                        <div class="col-6">
                            <input type="radio" name="QuestionList[${index}].SelectedAnswer" value="C" />
                            <label>${question.AnswerC}</label>
                        </div>
                        <div class="col-6">
                            <input type="radio" name="QuestionList[${index}].SelectedAnswer" value="D" />
                            <label>${question.AnswerD}</label>
                        </div>
                    </div>
                    <input type="hidden" name="QuestionList[${index}].Id" value="${question.Id}" />
                    <hr />
                </div>
            `;
            quizContainer.append(questionHtml);
        });
    } else {
        quizContainer.append('<p>No questions available for this quiz.</p>');
    }
    if (timerDuration!=0) {
        // Timer duration in seconds
        var display = document.querySelector('#timer');
        startTimer(timerDuration, display);
    }
}
 function submitQuiz() {

    stopTimer();

    const evntId=events[0].$values[currentEventIndex-1].Id
    const formData = $('#quiz-form').serialize();
    const dataToSend = formData + "&evntId=" + evntId;

    $.ajax({
        type: "POST",
        url: '@Url.Action("SubmitQuiz", "VideoKnowledgesQuestions")',
        data: dataToSend,
        success: function(response) {
            // Aggiorna i valori di TotalQuestions e CorrectAnswers con quelli restituiti dal server
            $('#total-questions').text(response.totalQuestions);
            $('#correct-answers').text(response.correctAnswers);

            // Nasconde il container del quiz e mostra i risultati
            $('#quiz-container').hide();
            $('#quiz-results-container').show();

            // Continua la riproduzione del video
            resumeVideo();
        },
        error: function() {
            alert('Si è verificato un errore durante l\'invio del quiz.');
        }
    });
}

function returnToVdk () {
    $('#quiz-results-container').hide();
    var videoSource = @Model.VideoSource;

    if (videoSource == "1") {
        $('#youtube-container').show();
    }
    else {
            $('#local-video-container').show();
    }
}

// Function to start the countdown timer
function startTimer(duration, display) {
    let timer = duration, minutes, seconds;
    timerInterval = setInterval(function () {
        minutes = parseInt(timer / 60, 10);
        seconds = parseInt(timer % 60, 10);

        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.textContent = minutes + ":" + seconds;

        if (--timer < 0) {
            clearInterval(timerInterval); // Ferma il timer quando finisce il countdown
            submitQuiz(); // Invia automaticamente il quiz
        }
    }, 1000);
}

// Function to stop the timer
function stopTimer() {
    if (timerInterval) {
        clearInterval(timerInterval);
        timerInterval = null;
    }
}
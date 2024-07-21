# Istruzioni setup iniziale

Per avviare l’app bisogna scaricare il fe e per attivarlo è necessario avere una versione recente di Node.js. Per verificare se Node.js è installato sul tuo sistema, apri il terminale e digita: ```node -v```

Se Node.js non è installato, puoi scaricarlo da questo [Node.js — Download Node.js® (nodejs.org)](https://nodejs.org/en/download/package-manager).

Una volta verificata l'installazione di Node.js, puoi iniziare a configurare il progetto Angular.



1. Installa le dipendenze del progetto eseguendo il seguente comando nel terminale: ```npm install```
2. Installa Angular CLI globalmente con questo comando: ```npm install -g @angular/cli``` 
3. Avvia il sito eseguendo questo comando: ```ng serve```

Per il database, si utilizza PostgreSQL. Si ricorda che è necessario sostituire la connection string nel file appsettings con le proprie impostazioni come l’host, la porta, il nome del DB, l’username e la password. Infine eseguire i seguenti comandi:



1. ```Add-Migration <nome>```
2. ```Update-Database```

Successivamente, si consiglia di utilizzare Visual Studio per eseguire il backend. Per aprire la soluzione, basta aprire il file con estensione .sln in Visual Studio. Una volta che la soluzione è stata caricata e verificata, premere il tasto verde di avvio.

Verificare che l'URL del dev server del FE corrisponda a quello specificato nel proxy in BuildExtension.cs. In caso contrario, modificarlo.

Per il corretto funzionamento del login con OAuth, è necessario aggiungere i segreti. In Visual Studio, fai clic con il tasto destro del mouse sul progetto, seleziona "Manage User Secrets" e inserisci il seguente codice:

```
{ 
  "Authentication:Google:ClientSecret": "segreto 1", 
  "Authentication:Google:ClientId": "segreto 2" 
}
```

Una volta avviato il backend, sarà sufficiente accedere a ```https://localhost:{{porta}}```. Se tutto funziona correttamente, dovreste visualizzare una pagina di login. (Nota bene: È importante che l'app Angular sia in esecuzione, altrimenti verrà mostrato un errore.)

Se si esegue in modalità dev, si vedrà inizialmente Swagger, è sufficiente cancellare tutto dall'URL, eccetto ```https://localhost:{{porta}}```


# Scelte implementative


## Frontend: Angular

Per la parte frontend del progetto, ho scelto di utilizzare Angular, poiché offre una struttura chiara e modulare per la mia applicazione. Inoltre, permette l’utilizzo semplice dell'API REST grazie alla classe HttpClient.


## Backend: Minimal API

Per il backend, ho scelto di utilizzare Minimal API per creare un'API REST. Per la gestione degli utenti, ho deciso di utilizzare Identity Framework. Tuttavia, ho deciso di disabilitare temporaneamente il controllo della validità dell'email e le funzionalità di sicurezza della password fornite dal framework Identity.

Ecco una breve elenco delle api e del loro uso:


### API Gestione Utenti

Per gestire gli utenti, ho implementato le seguenti API:



* /login: Gestisce l'autenticazione degli utenti.
* /register: Consente agli utenti di registrarsi.
* /logout: Gestisce la disconnessione degli utenti.
* /user: Fornisce informazioni sugli utenti autenticati.
* /role: Gestisce i ruoli degli utenti.


### API Gestione Parcheggi

Per la gestione dei parcheggi, ho implementato le seguenti API:



* /carpark: Gestisce le operazioni generali sui parcheggi.
* /carpark/carspots: Fornisce informazioni sui posti auto disponibili.
* /carpark/queue: Controlla la fila di attesa per i parcheggi.
* /carpark/id/park: Consente di parcheggiare in un posto specifico.
* /carpark/current: Trova il parcheggio attualmente utilizzato, se presente.
* /carpark/update: Utilizza gli eventi del server per aggiornare in tempo reale la posizione nella fila e informare l'utente quando può parcheggiare.


### API Pagamenti

Per gestire i pagamenti, ho implementato le seguenti API:



* /payments: Gestisce le operazioni generali sui pagamenti.
* /payments/settle: Finalizza i pagamenti dovuti.


### Backgroundworker

Ho inoltre sviluppato un backgroundworker che ogni secondo:



1. Libera i Posti Auto: Segna come liberi i parcheggi che non sono più occupati.
2. Addebita i Costi: Addebita automaticamente il dovuto agli utenti che hanno occupato i parcheggi.

Questo worker garantisce che il sistema sia sempre aggiornato e che i pagamenti pagati.


## Database: PostgreSQL

Un'altra scelta implementativa è stata l'uso di PostgreSQL come database, con le seguenti tabelle:



* aspnetrolesclaims: Usata da Identity per gestire i ruoli degli utenti.
* aspnetroles: Contiene i ruoli degli utenti.
* aspnetuserclaims: Memorizza i claims degli utenti.
* aspnetuserlogins: Gestisce le informazioni di login degli utenti.
* aspnetuserroles: Associa gli utenti ai ruoli.
* aspnetusertokens: Memorizza i token degli utenti.
* aspnetusers: Contiene le informazioni sugli utenti.

Inoltre, sono state create le seguenti tabelle per la gestione del sistema di parcheggi:



* books: Gestisce la coda dei parcheggi.
* carparks: Contiene i parcheggi.
* carspots: Contiene i posti auto dei vari parcheggi.
* invoices: Contiene le fatture degli ordini dei parcheggi.

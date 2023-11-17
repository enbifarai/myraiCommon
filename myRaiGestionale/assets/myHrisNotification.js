self.addEventListener("install", function (event) {
    console.log("WORKER: install event in progress.");
    //self.skipWaiting();
});

self.addEventListener("activate", event => {
    console.log('WORKER: activate event in progress.');
});

self.addEventListener('push', function (event) {
    console.log('[Service Worker] Push Received.');
    console.log(`[Service Worker] Push had this data: "${event.data.text()}"`);

    const img = '/assets/img/logoMenu.png';
    const title = 'Hris';
    const options = {
        body: event.data.text(),
        icon: img,
        badge: img
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

//function GetNotification() {
//    fetch('/Home/GetNotification', { method: 'post' })
//        .then(
//            function (response) {
//                if (response.status !== 200) {
//                    console.log('Looks like there was a problem. Status Code: ' + response.status);
//                    return;
//                }

//                // Examine the text in the response
//                response.json().then(function (data) {
//                    if (data && data.length > 0) {
//                        let img = '/assets/img/logoMenu.png';
//                        for (var i = 0; i < data.length; i++) {
//                            //self.postMessage(data[i]);
//                            new Notification("HRIS", {
//                                body: data[i].MESSAGE,
//                                icon: img,
//                                img: img
//                            });
//                            //self.registration.showNotification("HRIS", {
//                            //    body: data[i].MESSAGE,
//                            //    icon: img,
//                            //    img: img
//                            //});
//                        }
//                    }
//                });
//            }
//        )
//        .catch(function (err) {
//            console.log('Fetch Error :-S', err);
//        });
//}

//setInterval(function () {
//    GetNotification();
//}, 60000)


let _URL_GLOBAL_ANAG_MODAL = '/Anagrafica/Modal_DatiDipendente';
const _URL_GLOBAL_ANAG_HEADER = '/Anagrafica/Header_DatiDipendente';

function HrisModalAnagDip(matricola, sezList) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, { m: matricola, sezList: sezList });
}

function HrisModalAnagDipIdPers(idPersona) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, { idPersona: idPersona });
}

function HrisHeaderAnagDip(idwidget, matricola, sezList) {
    RaiUpdateWidget(idwidget, _URL_GLOBAL_ANAG_HEADER, 'html', { m: matricola, sezList: sezList });
}

function HrisHeaderAnagDipIdPers(idwidget, idPersona) {
    RaiUpdateWidget(idwidget, _URL_GLOBAL_ANAG_HEADER, 'html', { idPersona: idPersona });
}

function HrisModalAnagDipParams(params) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, JSON.stringify(params), null, 'POST', null, 'application/json; charset=utf-8');
}

function checkNotificationPromise() {
    try {
        Notification.requestPermission().then();
    } catch (e) {
        return false;
    }

    return true;
}

function HrisNotificationCheck() {
    if (!('serviceWorker' in navigator)) {
        throw new Error('No Service Worker support!')
    }
    if (!('PushManager' in window)) {
        throw new Error('No Push API Support!')
    }
}

function HrisNotificationAskFor() {
    if (!('Notification' in window)) {
        console.log("This browser does not support notifications.");
    } else {
        if (checkNotificationPromise()) {
            Notification.requestPermission()
                .then((permission) => {
                    //handlePermission(permission);
                })
        } else {
            Notification.requestPermission(function (permission) {
                //handlePermission(permission);
            });
        }
    }
}
function HrisNotificationTest() {
    var img = '/assets/img/logoMenu.png';
    var text = 'Questa è una notifica di prova';
    var notification = new Notification('HRIS', { body: text, icon: img });
}

let swRegistration = null;
function HrisStartNotificationWorker() {
    //const worker = new Worker('/assets/myHrisNotification.js');
    //worker.addEventListener("message", function (event) {
    //    swal(event.data.MESSAGE);
    //});

    if ('serviceWorker' in navigator && 'PushManager' in window) {
        console.log('Service Worker and Push is supported');

        navigator.serviceWorker.register('/assets/myHrisNotification.js')
            .then(function (swReg) {
                console.log('Service Worker is registered', swReg);

                swRegistration = swReg;
                swRegistration.pushManager.getSubscription()
                .then(function (subscription) {
                    isSubscribed = !(subscription === null);
                    if (isSubscribed) {
                        console.log('User IS subscribed.');
                    } else {
                        console.log('User is NOT subscribed.');
                    }
                });
            })
            .catch(function (error) {
                console.error('Service Worker Error', error);
            });
    } else {
        console.warn('Push messaging is not supported');
    }
}
function urlB64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
function subscribeUser() {
    $.ajax({
        url: '/Notifiche/GetPushPublicKey',
        type: "GET",
        dataType: "html",
        complete: function () { },
        success: function (data) {
            const applicationServerKey = urlB64ToUint8Array(data);
            swRegistration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: applicationServerKey
            })
            .then(function (subscription) {
                console.log('User is subscribed.');
                console.log(subscription);

                $.ajax({
                    url: '/Notifiche/RegisterSubScription',
                    type: "POST",
                    contentType: 'application/json',
                    data: JSON.stringify(subscription),
                    dataType: "html",
                    complete: function () { },
                    success: function (data) {
                        
                    }
                });

                isSubscribed = true;
            })
            .catch(function (err) {
                console.log('Failed to subscribe the user: ', err);
            });
        }
    });
}
function unsubscribeUser() {
    swRegistration.pushManager.getSubscription()
        .then(function (subscription) {
            if (subscription) {
                $.ajax({
                    url: '/Notifiche/RegisterUnSubScription',
                    type: "GET",
                    data: subscription,
                    dataType: "html",
                    complete: function () { },
                    success: function (data) {
                        return subscription.unsubscribe();
                    }
                });
            }
        })
        .catch(function (error) {
            console.log('Error unsubscribing', error);
        })
        .then(function () {
            console.log('User is unsubscribed.');
            isSubscribed = false;
        });
}
//HrisStartNotificationWorker();

function MatConVisualizzaGestione(idrichiesta) {
    $("#popupview-gestione").modal("show");
    $("#button-prendi").attr("onclick", "PrendiInCarico(" + idrichiesta + ")");
    $("#dettagli-container").html("");
    $("#dettagli-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneGestione',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-container").removeClass("rai-loader");
            $("#dettagli-container").html(data);
        }
    });

    $("#dettagli-richiesta-container").html("");
    $("#dettagli-richiesta-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneDettaglioRichiesta',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-richiesta-container").removeClass("rai-loader");
            $("#dettagli-richiesta-container").html(data);
        }
    });


    $("#assegnazione-container").html("");
    $("#assegnazione-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneAssegnazione',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#assegnazione-container").removeClass("rai-loader");
            $("#assegnazione-container").html(data);
        }
    });


    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneAnnullamento',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#annullamento-container").html(data);
        }
    });
}

let _URL_GLOBAL_ANAG_MODAL = '/Anagrafica/Modal_DatiDipendente';

function HrisModalAnagDip(matricola) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, { m: matricola });
}

function HrisModalAnagDipIdPers(idPersona) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, { idPersona: idPersona });
}

function HrisModalAnagDipParams(params) {
    RaiOpenAsyncModal('modal-global-anag-dip', _URL_GLOBAL_ANAG_MODAL, params);
}

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
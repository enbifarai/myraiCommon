const { type } = require("jquery");

function Dematerializzazione_SetScelteFase1() {
    var tipologiaDocumentale = $('#tipologiaDocumentale').val();
    tipologiaDocumentale = $.trim(tipologiaDocumentale);
    tipologiaDocumentale = tipologiaDocumentale.toUpperCase();

    var tipologiaDocumento = $('#tipodoc').val();
    tipologiaDocumento = $.trim(tipologiaDocumento);
    tipologiaDocumento = tipologiaDocumento.toUpperCase();

    $.ajax({
        url: "/Dematerializzazione/SetScelteFase1",
        type: "POST",
        async: false,
        data: JSON.stringify({
            tipologiaDocumentale: tipologiaDocumentale,
            tipologiaDocumento: tipologiaDocumento
        }),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            // data sarà un oggetto di tipo XR_DEM_TIPIDOC_COMPORTAMENTO
            if (data != null) {
                // definisce se l'approvatore sarà visibile
                $('#ApprovazioneObbligatoria').val(data.ApprovazioneObbligatoria);
                $('#FirmaObbligatoria').val(data.FirmaObbligatoria);
                $('#PosizionaProtocollo').val(data.PosizionaProtocollo);
                $('#ApprovatoreVisibile').val(data.ApprovatoreVisibile);
                $('#FirmaVisibile').val(data.FirmaVisibile);
                $('#FileObbligatorio').val(data.FileObbligatorio);
                $('#IsCustomType').val(data.IsCustomType);
            }

            if (!data.ApprovatoreVisibile) {
                $('#selApprovatore').val("-1");
                $('#div-Approvatore').hide();
            }
            else {
                $('#div-Approvatore').show();
                RaiSelectExtLoadAsyncData('selApprovatore', '/Dematerializzazione/GetElencoApprovatoriJSON');
            }

            if (!data.FirmaVisibile) {
                $('#incaricatoFirma').val("-1");
                $('#div-IncaricatoFirma').hide();
            }
            else {
                $('#div-IncaricatoFirma').show();
            }

            $('#div-tipologiaDocumentale').data('tipo', data.WKF_TIPOLOGIA);

            if (data.FileObbligatorio) {
                $('#div-infoAggiuntive').show();
                $('#div-template-container').show();
            }
            else {
                $('#div-infoAggiuntive').hide();
                $('#div-template-container').hide();
                $('#btns-fase1-next').removeClass('disable');
            }
        },
        error: function (xhr, status) {
            $('#div-infoAggiuntive').hide();
            $('#div-template-container').hide();
            swal({
                title: xhr.statusText,
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function Dematerializzazione_PDFPresente(interval) {
    var idDoc = $('#identificativoAllegato').val();
    if (idDoc == null || idDoc == "" || typeof (idDoc) === "undefined") {
        return false;
    }
    $.ajax({
        url: "/Dematerializzazione/IsReadyPDF",
        type: "GET",
        data: {
            idDoc: idDoc
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data.result) {
                clearInterval(interval);
                $('#isPDF').val('TRUE');
                $('#messaggio-attesa').hide();
                Dematerializzazione_AttivaElementiTab(2);
                // idDoc è l'id dell'allegato
                Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc);
            }
            else {
                if (data.message != "") {
                    swal({
                        title: data.message,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
            }
        },
        error: function (xhr, status) {
            swal({
                title: 'Si è verificato un errore nel reperimento dei dati del documento',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function Dematerializzazione_IsCustomType_And_LoadData() {
    var isCustomType = false;

    $.ajax({
        url: '/Dematerializzazione/IsCustomType',
        type: "GET",
        data: {
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data.esito) {
                if (data.customType === "NONE") {
                    // TO DO
                    isCustomType = false;
                }
                else if (data.customType === "CUSTOMVIEW") {
                    // TO DO
                    isCustomType = true;
                }
                else if (data.customType === "CUSTOMDATA") {
                    // TO DO
                    isCustomType = true;
                    Dematerializzazione_CaricaCustomData();
                }
            }
            else {
                swal({
                    title: data.error,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        }
    });

    return isCustomType;
}

function Dematerializzazione_Btn1Next_Click() {
    debugger;
    var idDocOriginale = $('#identificativoAllegatoOriginale').val();

    if (idDocOriginale !== null && idDocOriginale !== "" && typeof (idDocOriginale) !== "undefined") {
        Dematerializzazione_Btn1Next_Click_InModifica();
        return false;
    }

    var idDoc = $('#identificativoAllegato').val();
    var isCustomType = $('#IsCustomType').val();
    var fileObbligatorio = $('#FileObbligatorio').val();

    isCustomType = (isCustomType.toUpperCase() === "TRUE");
    fileObbligatorio = (fileObbligatorio.toUpperCase() === "TRUE");

    if ((fileObbligatorio && $('#div-tipologiaDocumentale').is(':visible')) ||
        (isCustomType && !fileObbligatorio)) {
        if ($("#modal-inserimentoDocDem").is(':visible')) {
            $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
        }

        if (idDoc > 0) {
            var isPDF = $('#isPDF').val();
            if (isPDF === "TRUE") {

                if (isCustomType) {
                    Dematerializzazione_IsCustomType_And_LoadData();
                    return false;
                }

                Dematerializzazione_AttivaElementiTab(2);

                if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
                    return false;
                }

                Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc);
            }
            else {
                $('#messaggio-attesa').show();

                const interval = setInterval(function () {
                    Dematerializzazione_PDFPresente(interval);
                }, 5000);
            }
        }
        else {
            if (isCustomType) {
                Dematerializzazione_IsCustomType_And_LoadData();
                return false;
            }
        }
    }
}

function Dematerializzazione_Btn1BisPrev_Click() {
    $('#tab1').show();
    $('#tab1-custom-data').hide();
    $('#btns-fase1').show();
    $('#btns-fase1-custom').hide();
    Dematerializzazione_AttivaElementiTab(1);
}

function Dematerializzazione_CaricaCustomData() {
    debugger;
    var matricola = "";

    if ($('#selMatricolaDestinatario').is(':visible')) {
        var scelta = $('#selMatricolaDestinatario').val();
        if (scelta != null && scelta != "") {
            matricola = scelta;
        }
    }

    $.ajax({
        url: '/Dematerializzazione/CaricaCustomData',
        type: "GET",
        data: {
            matricola: matricola
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
            $('#tab1-custom-data').html(data);
            $('#tab1').hide();
            $('#tab1-custom-data').show();
            $('#btns-fase1').hide();
            $('#btns-fase1-custom').show();
        },
        failure: function (jqXHR, textStatus, errorThrown) {
            var tx = jqXHR.status + "; Error: " + jqXHR.responseText;
            swal({
                title: tx,
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });            
        }
    });
}

function Dematerializzazione_Btn1Next_Click_InModifica() {
    debugger;

    var isCustomType = $('#IsCustomType').val();
    var fileObbligatorio = $('#FileObbligatorio').val();

    isCustomType = (isCustomType.toUpperCase() === "TRUE");
    fileObbligatorio = (fileObbligatorio.toUpperCase() === "TRUE");

    if ($('#div-tipologiaDocumentale').is(':visible') ||
        (isCustomType && !fileObbligatorio)) {
        if ($("#modal-inserimentoDocDem").is(':visible')) {
            $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
        }
        debugger;

        if (isCustomType) {
            $('#tab1').hide();
            $('#tab1-custom-data').show();
            $('#btns-fase1').hide();
            $('#btns-fase1-custom').show();
            return false;
        }

        var isPDF = $('#isPDF').val();
        if (isPDF == "TRUE") {
            Dematerializzazione_AttivaElementiTab(2);
            var idDoc = $('#identificativoAllegato').val();
            if (idDoc == null || idDoc == "" || typeof (idDoc) === "undefined") {
                return false;
            }

            var idDocOriginale = $('#identificativoAllegatoOriginale').val();
            var contatore = $('#identificativoAllegato').data('contatore');
            if (contatore === null || contatore === "" || typeof (contatore) === "undefined") {
                contatore = 0;
            }

            contatore = parseInt(contatore, 10);
            contatore = contatore + 1;
            $('#identificativoAllegato').data('contatore', contatore);

            var riposiziona = $('#identificativoAllegato').data('riposizionamento');

            if (idDocOriginale !== null && idDocOriginale !== "" && typeof idDocOriginale !== "undefined") {
                if (idDocOriginale === idDoc && contatore === 1) {
                    // se il documento è quello originale ed il contatore è 1 allora le posizioni saranno quelle di partenza
                    Dematerializzazione_OperazioniPreliminariAttivazioneTab2ModalitaModifica(idDoc, 'START');
                }
                else if (idDocOriginale === idDoc && contatore > 1) {
                    // se il documento è quello originale, ma l'utente è già andato una volta al tab2, allora 
                    // deve caricare le ultime posizioni scelte dall'utente
                    Dematerializzazione_OperazioniPreliminariAttivazioneTab2ModalitaModifica(idDoc, 'LAST');
                }
                else if (idDocOriginale !== idDoc && contatore === 1) {
                    // se il documento è diverso, ma è la prima volta che l'utente va al tab2
                    Dematerializzazione_OperazioniPreliminariAttivazioneTab2ModalitaModifica(idDoc, 'RESET');
                }
                else {
                    // se il documento è diverso e l'utente è già andato più di una volta al tab2
                    Dematerializzazione_OperazioniPreliminariAttivazioneTab2ModalitaModifica(idDoc, 'LAST');
                }
            }
        }
        else {
            $('#messaggio-attesa').show();

            const interval = setInterval(function () {
                Dematerializzazione_PDFPresente(interval);
            }, 5000);
        }

        var guid = $('#selPagina').data('rai-select');
        var mySelect = $('div[class="rai-select-option"][data-search="search-' + guid + '"]');

        $('div[class="rai-select-option"][data-search="search-' + guid + '"]').on('change', function () {
            Dematerializzazione_CambioPagina(null, false, false, true);
        });
    }
}

function Dematerializzazione_SetLabelPosition(posizione) {
    var margineSx = $('#ItemPreview').css('margin-left');
    margineSx = margineSx.replace('px', '');
    margineSx = margineSx.replace('.', ',');
    margineSx = parseInt(margineSx, 10);

    var left = $('#ItemPreview').position().left;
    left = parseInt(left, 10);

    if (posizione === 'NONE') {
        $("#draggable").data('mosso', 'false');
        $("#draggableData").data('mosso', 'false');
        $("#draggableFirma").data('mosso', 'false');
    }
    else if (posizione === 'START') {

        var offTop1 = $("#draggable").data('oldtop');
        var offLeft1 = $("#draggable").data('oldleft');

        offTop1 = offTop1.replace('.', ',');
        offTop1 = parseInt(offTop1, 10);
        
        if (offLeft1 === null || offLeft1 === "" || typeof offLeft1 === "undefined") {
            offLeft1 = margineSx + left;
        }
        else {
            offLeft1 = offLeft1.replace('px', '');
            offLeft1 = offLeft1.replace('.', ',');
            offLeft1 = parseInt(offLeft1, 10);
            offLeft1 += margineSx + left;            
        }

        var offTop2 = $("#draggableData").data('oldtop');
        var offLeft2 = $("#draggableData").data('oldleft');

        offTop2 = offTop2.replace('.', ',');
        offTop2 = parseInt(offTop2, 10);

        if (offLeft2 === null || offLeft2 === "" || typeof offLeft2 === "undefined") {
            offLeft2 = margineSx + left;
        }
        else {
            offLeft2 = offLeft2.replace('px', '');
            off1Left2 = offLeft2.replace('.', ',');
            offLeft2 = parseInt(offLeft2, 10);
            offLeft2 += margineSx + left;
        }

        var offTop3 = $("#draggableFirma").data('oldtop');
        var offLeft3 = $("#draggableFirma").data('oldleft');

        offTop3 = offTop3.replace('.', ',');
        offTop3 = parseInt(offTop3, 10);

        if (offLeft3 === null || offLeft3 === "" || typeof offLeft3 === "undefined") {
            offLeft3 = margineSx + left;
        }
        else {
            offLeft3 = offLeft3.replace('px', '');
            offLeft3 = offLeft3.replace('.', ',');
            offLeft3 = parseInt(offLeft3, 10);
            offLeft3 += margineSx + left;
        }

        $("#draggable").animate(
            { left: offLeft1, top: offTop1 },
            {
                duration: 1
            });

        $("#draggableData").animate(
            { left: offLeft2, top: offTop2 },
            {
                duration: 1
            });

        $("#draggableFirma").animate(
            { left: offLeft3, top: offTop3 },
            {
                duration: 1
            });

        $("#draggable").data('mosso', 'true');
        $("#draggableData").data('mosso', 'true');
        $("#draggableFirma").data('mosso', 'true');
    }
    else if (posizione === 'LAST') {
        $("#draggable").data('mosso', 'true');
        $("#draggableData").data('mosso', 'true');
        $("#draggableFirma").data('mosso', 'true');
    }
    else if (posizione === 'RESET') {

        var offLeft1 = margineSx + left;
        var offLeft2 = margineSx + left;
        var offLeft3 = margineSx + left;

        var offTop1 = 0;
        var offTop2 = 45;
        var offTop3 = 85;

        $("#draggable").animate(
            { left: offLeft1, top: offTop1 },
            {
                duration: 1
            });

        $("#draggableData").animate(
            { left: offLeft2, top: offTop2 },
            {
                duration: 1
            });

        $("#draggableFirma").animate(
            { left: offLeft3, top: offTop3 },
            {
                duration: 1
            });

        $("#draggable").data('mosso', 'false');
        $("#draggableData").data('mosso', 'false');
        $("#draggableFirma").data('mosso', 'false');
    }
}

function Dematerializzazione_OperazioniPreliminariAttivazioneTab2ModalitaModifica(idDoc, posizione) {
    $('#dem-sel-pagina-documento').data('paginacorrente', '1');
    RaiSelectExtLoadAsyncData('selPagina', '/Dematerializzazione/GetNumeroPagineDocumentoJSON', { idAllegato: idDoc, nPagina: 1 });

    $.ajax({
        url: '/Dematerializzazione/GetFileInJpeg',
        type: "GET",
        data: {
            idAllegato: idDoc,
            pagina: "1"
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () {
            Dematerializzazione_SetLabelPosition(posizione);
        },
        success: function (data) {
            $('#ItemPreview').attr('src', data);
        }
    });
}

function Dematerializzazione_Btn2Next_Click() {
    // verifica se sono stati posizionati gli elementi
    var posizionaProtocollo = $('#PosizionaProtocollo').val();
    if (posizionaProtocollo.toUpperCase() === "TRUE") {
        var val1 = $("#draggable").data('mosso');
        var val2 = $("#draggableData").data('mosso');
        var val3 = $("#draggableFirma").data('mosso');

        val1 = val1.toUpperCase();
        val2 = val2.toUpperCase();
        val3 = val3.toUpperCase();

        if (val1 !== "TRUE" || val2 !== "TRUE" || val3 !== "TRUE") {
            swal({
                title: "E' necessario posizionare il protocollo, la data e la firma per poter continuare.",
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
            return false;
        }
    }

    var result = SetPosizioneProtocollo();

    if (result) {
        if ($("modal-inserimentoDocDem").is(':visible')) {
            $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
        }
        Dematerializzazione_AttivaElementiTab(3);
    }
}

function GetDataPerRiepilogo(idDoc) {
    $.ajax({
        url: "/Dematerializzazione/GetDataPerRiepilogo",
        type: "GET",
        async: false,
        data: {
            idDoc: idDoc
        },
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
            $('#div-riepilogo-documento').html(data);
            Dematerializzazione_AttivaElementiTab(4);
        },
        error: function (xhr, status) {
            swal({
                title: 'Si è verificato un errore nel reperimento dei dati del documento',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function Dematerializzazione_Btn2Prev_Click() {
    $('#identificativoAllegato').data('riposizionamento', 'false');

    var guid = $('#selPagina').data('rai-select');
    var mySelect = $('div[class="rai-select-option"][data-search="search-' + guid + '"]');

    $('div[class="rai-select-option"][data-search="search-' + guid + '"]').on('change', function () {
        return false;
    });
    Dematerializzazione_AttivaElementiTab(1);
}

function Dematerializzazione_Btn3Prev_Click() {
    Dematerializzazione_AttivaElementiTab(2);
}

function Dematerializzazione_Btn4Prev_Click() {
    Dematerializzazione_AttivaElementiTab(3);
}

function Dematerializzazione_OnSuccess(response, redirect) {
    var idDoc = response.responseText;
    $('#btns-fase3-next').data('idDoc', response.responseText);
    Dematerializzazione_AttivaElementiTab(4);
    GetDataPerRiepilogo(idDoc);
}

function Dematerializzazione_AbilitaPosizionamentoProtocollo() {
    var posizionaProtocollo = $('#PosizionaProtocollo').val();

    if (posizionaProtocollo.toUpperCase() === "TRUE") {

        var guid = $('#selPagina').data('rai-select');
        var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
        nPagine = parseInt(nPagine, 10);

        var paginarichiesta = $('#dem-sel-pagina-documento').data('paginarichiesta');
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (nPagine < paginarichiesta || isNaN(paginarichiesta)) {
            paginarichiesta = 1;
            $('#dem-sel-pagina-documento').data('paginarichiesta', '1');
        }

        var paginacorrente = $('#dem-sel-pagina-documento').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (nPagine < paginacorrente || isNaN(paginacorrente)) {
            paginacorrente = 1;
            $('#dem-sel-pagina-documento').data('paginacorrente', '1');
        }

        var pagina = $('#selPagina').val();
        pagina = parseInt(pagina, 10);

        if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
            pagina = 1;
            $('#selPagina').val('1');
        }

        var paginafirma = $('#dem-sel-pagina-documento').data('paginafirma');
        paginafirma = parseInt(paginafirma, 10);

        if (nPagine < paginafirma || isNaN(paginafirma)) {
            paginafirma = 1;
            $('#dem-sel-pagina-documento').data('paginafirma', '1');
        }

        if (paginacorrente !== 1) {
            Dematerializzazione_CambioPagina(1, true, $('#draggableData').is(':visible'), false);
        }
        else if (paginacorrente === 1) {
            $('#draggable').show();
        }

        if ($('#icona3-firma').hasClass('evidenzia') && paginafirma !== 1) {
            $('#icona3-firma').removeClass('evidenzia');
        }

        if (!$('#icona1-protocollo').hasClass('evidenzia')) {
            $('#icona1-protocollo').addClass('evidenzia');
        }
    }

    Dematerializzazione_AbilitaPosizionamentoData();
}

function Dematerializzazione_AbilitaPosizionamentoData() {
    var posizionaProtocollo = $('#PosizionaProtocollo').val();

    if (posizionaProtocollo.toUpperCase() === "TRUE") {

        var guid = $('#selPagina').data('rai-select');
        var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
        nPagine = parseInt(nPagine, 10);

        var paginarichiesta = $('#dem-sel-pagina-documento').data('paginarichiesta');
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (nPagine < paginarichiesta || isNaN(paginarichiesta)) {
            paginarichiesta = 1;
            $('#dem-sel-pagina-documento').data('paginarichiesta', '1');
        }

        var paginacorrente = $('#dem-sel-pagina-documento').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (nPagine < paginacorrente || isNaN(paginacorrente)) {
            paginacorrente = 1;
            $('#dem-sel-pagina-documento').data('paginacorrente', '1');
        }

        var pagina = $('#selPagina').val();
        pagina = parseInt(pagina, 10);

        if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
            pagina = 1;
            $('#selPagina').val('1');
        }

        var paginafirma = $('#dem-sel-pagina-documento').data('paginafirma');
        paginafirma = parseInt(paginafirma, 10);

        if (nPagine < paginafirma || isNaN(paginafirma)) {
            paginafirma = 1;
            $('#dem-sel-pagina-documento').data('paginafirma', '1');
        }

        if (paginacorrente !== 1) {
            Dematerializzazione_CambioPagina(1, $('#draggable').is(':visible'), true, false);
        }
        else if (paginacorrente === 1) {
            $('#draggableData').show();
        }

        if (!$('#icona2-data').hasClass('evidenzia')) {
            $('#icona2-data').addClass('evidenzia');
        }

        if ($('#icona3-firma').hasClass('evidenzia') && paginafirma !== 1) {
            $('#icona3-firma').removeClass('evidenzia');
        }
    }
}

function Dematerializzazione_AbilitaPosizionamentoFirma() {
    var posizionaProtocollo = $('#PosizionaProtocollo').val();

    if (posizionaProtocollo.toUpperCase() === "TRUE") {
        var guid = $('#selPagina').data('rai-select');
        var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
        nPagine = parseInt(nPagine, 10);

        var paginarichiesta = $('#dem-sel-pagina-documento').data('paginarichiesta');
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (nPagine < paginarichiesta || isNaN(paginarichiesta)) {
            paginarichiesta = 1;
            $('#dem-sel-pagina-documento').data('paginarichiesta', '1');
        }

        var paginacorrente = $('#dem-sel-pagina-documento').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (nPagine < paginacorrente || isNaN(paginacorrente)) {
            paginacorrente = 1;
            $('#dem-sel-pagina-documento').data('paginacorrente', '1');
        }

        var paginafirma = $('#dem-sel-pagina-documento').data('paginafirma');
        paginafirma = parseInt(paginafirma, 10);

        if (nPagine < paginafirma || isNaN(paginafirma)) {
            paginafirma = 1;
            $('#dem-sel-pagina-documento').data('paginafirma', '1');
        }

        var pagina = $('#selPagina').val();
        pagina = parseInt(pagina, 10);    

        if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
            pagina = 1;
            $('#selPagina').val('1');
        }

        if (!$('#icona3-firma').hasClass('evidenzia')) {
            $('#icona3-firma').addClass('evidenzia');
        }

        if (paginacorrente !== paginafirma) {
            Dematerializzazione_CambioPagina(paginafirma, false, false, true);
        }
        else if (paginacorrente === paginafirma) {
            $('#draggableFirma').show();
            if (nPagine !== 1) {
                $('#dem-sel-pagina-documento').show();
            }
        }
    }
}

function Dematerializzazione_CambioPagina(nuovaPagina, abilitaProt, abilitaData, abilitaFirma) {
    var pagina = "";
    if (nuovaPagina === null || nuovaPagina === "" || typeof nuovaPagina === "undefined") {
        var paginacorrente = $('#dem-sel-pagina-documento').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (isNaN(paginacorrente)) {
            paginacorrente = 1;
        }

        var paginarichiesta = $('#selPagina').val();
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (isNaN(paginarichiesta)) {
            paginarichiesta = 1;
        }

        if (!$('#icona3-firma').hasClass('evidenzia')) {
            paginarichiesta = 1;
        }

        if (paginacorrente === paginarichiesta) {
            return false;
        }
        else {
            pagina = paginarichiesta;
        }
    }
    else {
        pagina = nuovaPagina;
    }

    if (abilitaFirma) {
        $('#dem-sel-pagina-documento').data('paginarichiesta', pagina);
        $('#dem-sel-pagina-documento').data('paginafirma', pagina);
    }

    var idDoc = $('#identificativoAllegato').val();
    if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
        return false;
    }
    $.ajax({
        url: '/Dematerializzazione/GetFileInJpeg',
        type: "GET",
        data: {
            idAllegato: idDoc,
            pagina: pagina
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () {

            $('#dem-sel-pagina-documento').data('paginacorrente', pagina);

            if (abilitaProt) {
                $('#icona1-protocollo').addClass('evidenzia');
                $('#draggable').show();
            }
            else {
                $('#icona1-protocollo').removeClass('evidenzia');
                $('#draggable').hide();
            }

            if (abilitaData) {
                $('#icona2-data').addClass('evidenzia');
                $('#draggableData').show();
            }
            else {
                $('#icona2-data').removeClass('evidenzia');
                $('#draggableData').hide();
            }

            if (abilitaFirma) {
                $('#icona3-firma').addClass('evidenzia');
                $('#draggableFirma').show();
                $('#dem-sel-pagina-documento').show();
            }
            else {
                $('#icona3-firma').removeClass('evidenzia');
                $('#draggableFirma').hide();
                $('#dem-sel-pagina-documento').hide();
            }

            var guid = $('#selPagina').data('rai-select');
            var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
            nPagine = parseInt(nPagine, 10);

            if (nPagine === 1) {
                $('#dem-sel-pagina-documento').hide();
            }
            $('#selPagina').val(pagina);
        },
        success: function (data) {
            $('#ItemPreview').attr('src', data);
        }
    });
}

function Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc) {
    debugger;
    $('#dem-sel-pagina-documento').data('paginacorrente', '1');
    var paginaSelezionata = 1;
    var npagina = $('#dem-sel-pagina-documento').data('paginafirma');
    if (npagina !== null && npagina !== "" && typeof npagina === "undefined") {
        npagina = parseInt(npagina, 10);
        paginaSelezionata = npagina;
    }

    if (idDoc > 0) {
        RaiSelectExtLoadAsyncData('selPagina', '/Dematerializzazione/GetNumeroPagineDocumentoJSON', { idAllegato: idDoc, nPagina: paginaSelezionata });

        $.ajax({
            url: '/Dematerializzazione/GetFileInJpeg',
            type: "GET",
            data: {
                idAllegato: idDoc,
                pagina: "1"
            },
            async: false,
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            complete: function () {
                var margineSx = $('#ItemPreview').css('margin-left');
                margineSx = margineSx.replace('px', '');
                margineSx = margineSx.replace('.', ',');
                margineSx = parseInt(margineSx, 10);

                var left = $('#ItemPreview').position().left;
                left = parseInt(left, 10);

                var off1 = $("#draggable").data('left');
                if (off1 === null || off1 === "" || typeof off1 === "undefined") {
                    off1 = margineSx + left;
                }
                else {
                    off1 = off1.replace('px', '');
                    off1 = off1.replace('.', ',');
                    off1 = parseInt(off1, 10);
                    off1 += margineSx + left;
                    $("#draggable").data('mosso', 'true');
                }

                var off2 = $("#draggableData").data('left');
                if (off2 === null || off2 === "" || typeof off2 === "undefined") {
                    off2 = margineSx + left;
                }
                else {
                    off2 = off2.replace('px', '');
                    off2 = off2.replace('.', ',');
                    off2 = parseInt(off2, 10);
                    off2 += margineSx + left;
                    $("#draggableData").data('mosso', 'true');
                }

                var off3 = $("#draggableFirma").data('left');
                if (off3 === null || off3 === "" || typeof off3 === "undefined") {
                    off3 = margineSx + left;
                }
                else {
                    off3 = off3.replace('px', '');
                    off3 = off3.replace('.', ',');
                    off3 = parseInt(off3, 10);
                    off3 += margineSx + left;
                    $("#draggableFirma").data('mosso', 'true');
                }

                $("#draggable").animate(
                    { left: off1 },
                    {
                        duration: 1
                    });

                $("#draggableData").animate(
                    { left: off2 },
                    {
                        duration: 1
                    });

                $("#draggableFirma").animate(
                    { left: off3 },
                    {
                        duration: 1
                    });
            },
            success: function (data) {
                $('#ItemPreview').attr('src', data);
            }
        });
    }
}

function Dematerializzazione_AttivaElementiTab(nTab) {

    for (var i = 1; i < nTab; i++) {
        $('#tab-dem-' + i).removeClass('active');
        $('#tab-dem-' + i).addClass('disable');
        $('#tab-dem-' + i).addClass('completed');
        $('#btns-fase' + i).hide();
        $('#tab' + i).hide();
        $('#tab2-pannello-abilita-posizionamento').hide();
    }

    $('#tab-dem-' + nTab).removeClass('disable');
    $('#tab-dem-' + nTab).addClass('active');
    $('#btns-fase' + nTab).show();
    $('#tab' + nTab).show();
    if (nTab === 2) {
        var posizionaProtocollo = $('#PosizionaProtocollo').val();
        if (posizionaProtocollo.toUpperCase() === "TRUE") {
            $('#tab2-pannello-abilita-posizionamento').show();
        }
        else {
            $('#draggable').hide();
            $('#draggableData').hide();
            $('#draggableFirma').hide();
        }
    }

    for (var i = nTab + 1; i <= 4; i++) {
        $('#tab-dem-' + i).removeClass('active');
        $('#tab-dem-' + i).addClass('disable');
        $('#tab-dem-' + i).removeClass('completed');
        $('#btns-fase' + i).hide();
        $('#tab' + i).hide();
        if (i == 2) {
            $('#tab2-pannello-abilita-posizionamento').hide();
        }
    }
}

function Dematerializzazione_ReloadModalPrendiInCarico(matricola, idPersona, idDoc) {
    $.ajax({
        url: '/Dematerializzazione/GetDettaglioRichiesta',
        type: "GET",
        data: {
            m: matricola,
            id: idPersona,
            idDoc: idDoc,
            approvatoreEnabled: false,
            presaInCaricoEnabled: true,
            presaInVisioneEnabled: false
        },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#modal-dettaglio-richiesta-internal").html(data);
        }
    });
}

function Dematerializzazione_ConcludiPratica_Click(matricola, idPersona, idDoc) {

    swal({
        title: 'Sei sicuro di voler concludere la pratica corrente?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, concludi',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var nota = $('#Richiesta_Documento_NotaConclusionePratica').val();

        $.ajax({
            url: "/Dematerializzazione/ConcludiPratica",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc,
                nota: nota
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Operazione eseguita correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',                    
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                Dematerializzazione_LoadTabellaDocumentiVSRUO();
                $('#modal-dettaglio-richiesta').modal('hide');
            }
        });
    });
}

function Dematerializzazione_AnnullaPrendiInCarico(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler rilasciare la pratica selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, rilascia la pratica',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/AnnullaPrendiInCarico",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Operazione eseguita correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                Dematerializzazione_ReloadModalPrendiInCarico(matricola, idPersona, idDoc);
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                Dematerializzazione_LoadTabellaDocumentiVSRUO();
            }
        });
    });
}

function Dematerializzazione_ReloadTabelleInPartenza(loader) {
    if (loader === null || loader === "" || typeof loader === "undefined") {
        loader = true;
    }

    RaiUpdateWidget('div_elenco_documenti', '/Dematerializzazione/GetContentInternal', 'html', { approvazioneEnabled: true }, false, null, loader, 'GET');
    RaiUpdateWidget('content-container-operatore', '/Dematerializzazione/GetContentOperatore', 'html', {}, false, null, loader, 'GET');
    //RaiUpdateWidget('content-container-visionatore', '/Dematerializzazione/GetContentVisionatore', 'html', {}, false, null, loader, 'GET');
}

function Dematerializzzazione_AbilitaDisabilita_BtnRifiutaApprovatore() {
    var txt = $('#Richiesta_Documento_NotaApprovatore').val();

    if (txt === null || txt === "" || $.trim(txt) === "") {
        if (!$('#Dem_Btn_RifiutaApprovatore').hasClass('disable')) {
            $('#Dem_Btn_RifiutaApprovatore').addClass('disable');
        }
        
        return false;
    }

    if (txt.length > 3) {
        $('#Dem_Btn_RifiutaApprovatore').removeClass('disable');
    }
    else {
        if (!$('#Dem_Btn_RifiutaApprovatore').hasClass('disable')) {
            $('#Dem_Btn_RifiutaApprovatore').addClass('disable');
        }
    }
}

function Dematerializzzazione_AbilitaDisabilita_BtnConcludiPratica() {
    var txt = $('#Richiesta_Documento_NotaConclusionePratica').val();

    if (txt === null || txt === "" || $.trim(txt) === "") {
        if (!$('#btns-dem-concludi-pratica').hasClass('disable')) {
            $('#btns-dem-concludi-pratica').addClass('disable');
        }

        return false;
    }

    if (txt.length > 3) {
        $('#btns-dem-concludi-pratica').removeClass('disable');
    }
    else {
        if (!$('#btns-dem-concludi-pratica').hasClass('disable')) {
            $('#btns-dem-concludi-pratica').addClass('disable');
        }
    }
}

function Dematerializzazione_RifiutaDocumento(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler rifiutare la richiesta selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, rifiuta',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        swal({
            title: 'Inserisci il motivo del rifiuto',
            type: 'warning',
            html: "<textarea id='dem-text-rifiuto' cols='40' rows='5'></textarea>",
            showCancelButton: true,
            confirmButtonText: 'Ok',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            inputPlaceholder: "Motivo rifiuto",
            customClass: 'rai rai-confirm-cancel'
        }).then(function () {
            var txt = $('#dem-text-rifiuto').val();
            if (txt === false) return false;
            if (txt === "") {
                swal({
                    title: 'Il motivo è un campo obbligatorio',
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                return false;
            }

            $.ajax({
                url: "/Dematerializzazione/RifiutaDocumento",
                type: "POST",
                data: JSON.stringify({
                    matricola: matricola,
                    idPersona: idPersona,
                    idDoc: idDoc,
                    motivo: txt
                }),
                contentType: "application/json; charset=utf-8",
                dataType: 'json',
                success: function (data) {
                    swal({
                        title: 'Operazione eseguita correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                },
                error: function (xhr, status) {
                    swal({
                        title: xhr.statusText,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                },
                complete: function () {
                    Dematerializzazione_ReloadTabelleInPartenza(false);
                }
            });
        });
    });
}

function Dematerializzazione_RifiutaDocumentoInModale(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler rifiutare la richiesta selezionata?',
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sì, rifiuta!',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        var txt = $('#Richiesta_Documento_NotaApprovatore').val();

        if (txt == null || txt == "" || $.trim(txt) == "") {
            swal({
                title: 'Il motivo del rifiuto è obbligatorio',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
            return false;
        }

        $.ajax({
            url: "/Dematerializzazione/RifiutaDocumento",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc,
                motivo: txt
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                swal({
                    title: 'Documento rifiutato correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                Dematerializzazione_ReloadTabelleInPartenza(false);
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                $('#modal-dettaglio-richiesta-internal').html('');
                $('#modal-dettaglio-richiesta').modal('hide');
            }
        });
    });
}

function Dematerializzazione_ApprovaDocumento(matricola, idPersona, idDoc) {

    swal({
        title: 'Sei sicuro di voler approvare la richiesta selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, approva',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var nota = $('#Richiesta_Documento_NotaApprovatore').val();

        $.ajax({
            url: "/Dematerializzazione/ApprovaDocumento",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc,
                nota: nota
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                swal({
                    title: 'Operazione eseguita correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                Dematerializzazione_ReloadTabelleInPartenza(false);
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                $('#modal-dettaglio-richiesta').modal('hide');
            }
        });
    });
}

function Dematerializzazione_ApprovaDocumenti(matricola, idPersona) {
    swal({
        title: 'Sei sicuro di voler approvare le richieste selezionate?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, approva',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        swal({
            title: 'Approvazione richieste in corso',
            type: 'info',
            html: "<label id='dem-label-elaborazione' class='rai-font-md'></label><br/><textarea id='dem-text-esito-approvazione' cols='40' rows='5' disabled></textarea>",
            showCancelButton: false,
            showConfirmButtonButton: false,
            confirmButtonText: 'Continua',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            inputPlaceholder: "Esito",
            customClass: 'rai rai-confirm-cancel'
        });

        var contatoreInizio = 0;
        var contatoreFine = $('input[type="checkbox"][id^="dem-check-"]:not("#dem-check-all"):checked').length;

        $('input[type="checkbox"][id^="dem-check-"]:not("#dem-check-all"):checked').each(function () {
            contatoreInizio++;
            //$('#dem-label-elaborazione').text('Approvazione domanda ' + contatoreInizio + ' di ' + contatoreFine);
            var idDoc = $(this).val();
            $.ajax({
                url: "/Dematerializzazione/ApprovaDocumento",
                type: "POST",
                async: false,
                data: JSON.stringify({
                    matricola: matricola,
                    idPersona: idPersona,
                    idDoc: idDoc
                }),
                contentType: "application/json; charset=utf-8",
                dataType: 'json',
                success: function (data) {
                    var tx = $('#dem-text-esito-approvazione').val();
                    tx = tx + "\n" + data.infoAggiuntive;
                    $('#dem-text-esito-approvazione').val(tx);
                },
                error: function (xhr, status) {
                    var tx = $('#dem-text-esito-approvazione').val();
                    tx = tx + "\n" + xhr.statusText;
                    $('#dem-text-esito-approvazione').val(tx);
                }
            });
        });
        Dematerializzazione_ReloadTabelleInPartenza(false);
    });
}

function Dematerializzazione_RifiutaDocumenti(matricola, idPersona) {
    swal({
        title: 'Sei sicuro di voler rifiutare le richieste selezionate?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, rifiuta',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        swal({
            title: 'Inserisci il motivo del rifiuto',
            type: 'warning',
            html: "<textarea id='dem-text-rifiuto' cols='40' rows='5'></textarea>",
            showCancelButton: true,
            confirmButtonText: 'Ok',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            inputPlaceholder: "Motivo rifiuto",
            customClass: 'rai rai-confirm-cancel'
        }).then(function () {
            var txt = $('#dem-text-rifiuto').val();
            if (txt === false) return false;
            if (txt === "") {
                swal({
                    title: 'Il motivo è un campo obbligatorio',
                    type: 'error',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                return false;
            }

            var eistoArray = new Array();

            swal({
                title: 'Rifiuto richieste in corso',
                type: 'info',
                html: "<label id='dem-label-elaborazione' class='rai-font-md'></label><br/><textarea id='dem-text-esito-rifiuto' cols='40' rows='5' disabled></textarea>",
                showCancelButton: false,
                showConfirmButtonButton: false,
                confirmButtonText: 'Continua',
                cancelButtonText: 'Annulla',
                reverseButtons: true,
                inputPlaceholder: "Motivo rifiuto",
                customClass: 'rai rai-confirm-cancel'
            });

            var contatoreInizio = 0;
            var contatoreFine = $('input[type="checkbox"][id^="dem-check-"]:not("#dem-check-all"):checked').length;

            $('input[type="checkbox"][id^="dem-check-"]:not("#dem-check-all"):checked').each(function () {
                contatoreInizio++;
                $('#dem-label-elaborazione').text('Rifiuto domanda ' + contatoreInizio + ' di ' + contatoreFine);
                var idDoc = $(this).val();
                $.ajax({
                    url: "/Dematerializzazione/RifiutaDocumento",
                    type: "POST",
                    data: JSON.stringify({
                        matricola: matricola,
                        idPersona: idPersona,
                        idDoc: idDoc,
                        motivo: txt
                    }),
                    contentType: "application/json; charset=utf-8",
                    dataType: 'json',
                    async: false,
                    success: function (data) {
                        //var item = new Object();
                        //item.id = idDoc;
                        //item.esito = "Ok";
                        //var tx = $('#dem-text-esito-rifiuto').val();
                        //tx = tx + "\nRichiesta " + idDoc + " esito: OK";
                        //$('#dem-text-esito-rifiuto').val(tx);

                        var tx = $('#dem-text-esito-rifiuto').val();
                        tx = tx + "\n" + data.infoAggiuntive;
                        $('#dem-text-esito-rifiuto').val(tx);

                    },
                    error: function (xhr, status) {
                        var tx = $('#dem-text-esito-rifiuto').val();
                        tx = tx + "\n" + xhr.statusText;
                        $('#dem-text-esito-rifiuto').val(tx);
                    }
                });
            });
            Dematerializzazione_ReloadTabelleInPartenza(false);
        });
    });
}

function OpenModalInserimentoDOC(matricola, idPersona, ricercaLibera) {
    if (typeof (ricercaLibera) === "undefined" || ricercaLibera === null || ricercaLibera === "") {
        ricercaLibera = false;
    }
    RaiOpenAsyncModal('modal-inserimentoDocDem', '/Dematerializzazione/Modal_InserimentoDocDem', { m: matricola, id: idPersona, ricercaLibera: ricercaLibera }, null, 'POST');
}

function OpenModalviewer(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-viewerDoc', '/Dematerializzazione/Modal_Viewer', { m: matricola, id: idPersona, idDoc: idDoc }, null, 'POST');
}

function SetVisioneDocumento(matricola, idPersona, idDoc) {
    $.ajax({
        url: "/Dematerializzazione/SetVisioneDocumento",
        type: "POST",
        data: JSON.stringify({
            matricola: matricola,
            idPersona: idPersona,
            idDoc: idDoc
        }),
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
        },
        error: function (xhr, status) {
        }
    });
}

function OpenModalGetDettaglioDematerializzazione(matricola, idPersona, idDoc, approvazioneEnabled, presaInCaricoEnabled) {
    if (typeof approvazioneEnabled == "undefined" || approvazioneEnabled == null) {
        approvazioneEnabled = false;
    }
    if (typeof presaInCaricoEnabled == "undefined" || presaInCaricoEnabled == null) {
        presaInCaricoEnabled = false;
    }
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetViewDettaglio', { m: matricola, id: idPersona, idDoc: idDoc, approvazioneEnabled: approvazioneEnabled, presaInCaricoEnabled: presaInCaricoEnabled }, null, 'GET');
}

function Dematerializzazione_OpenDettaglio(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc }, null, 'GET');
}

function Dematerializzazione_OpenModal_Doc_Viewer(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetModal_Doc_Viewer', { m: matricola, id: idPersona, idDoc: idDoc }, null, 'GET');
}

function OpenModalDematerializzazioneFileViewer(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-viewerDoc', '/Dematerializzazione/Modal_Viewer_ReadOnly', { m: matricola, id: idPersona, idDoc: idDoc }, null, 'GET');
}

function Dematerializzazione_AbilitaInfoFile() {
    debugger;
    var nomefile = ($("#fileupload-VSDIP").val().split("\\").pop());
    if (nomefile.toLowerCase().indexOf(".pdf") < 0 &&
        nomefile.toLowerCase().indexOf(".doc") < 0 &&
        nomefile.toLowerCase().indexOf(".docx") < 0) {
        swal("Formato file non supportato. Sono ammessi soltanto .doc, .docx e .pdf");
        return;
    }

    if (nomefile.toLowerCase().indexOf(".pdf") >= 0) {
        $('#isPDF').val("TRUE");
    }
    else {
        $('#isPDF').val("FALSE");
    }

    var formData = new FormData();
    formData.append('filePrincipale', true);
    formData.append('file', $('#fileupload-VSDIP')[0].files[0]);
    formData.append("nome", nomefile);
    formData.append("tipo", $('#fileupload-VSDIP').attr("data-tipo"));

    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var data = $.parseJSON(request.responseText);
            if (data.success) {
                Dematerializzazione_TempUpload_OnSuccess(data, true);
                $('#fileupload-VSDIP').val('');
            }
            else {
                Dematerializzazione_TempUpload_OnFailure(data);
            }
        }
    };

    request.open('post', "/Dematerializzazione/UploadTempDocument");
    request.timeout = 45000;
    request.send(formData);
}

function Dematerializzazione_TempUpload_OnSuccess(response, isPrincipal) {
    $.ajax({
        url: '/Dematerializzazione/DrawTRFile',
        type: "GET",
        data: {
            idAllegato: response.responseText,
            isPrincipal: isPrincipal
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () { },
        success: function (data) {
            if (isPrincipal) {
                //
                $('#tabella-file-principale').html(data);
                $('#div-upload-principale').hide();
                $('#btns-fase1-next').removeClass('disable');
                $('#identificativoAllegato').val(response.responseText);
                $('#identificativoAllegato').data('contatore', '0');
            }
            else {
                //
                $('#table-uploading-supporti-body').append(data);
            }
        }
    });
}

function Dematerializzazione_VisualizzaDocumentoTemporaneo(idAllegato) {
    RaiOpenAsyncModal('modal-viewer-temp-allegati', '/Dematerializzazione/GetAllegatoTemporaneo', { idAllegato: idAllegato }, null, 'GET');
}

function Dematerializzazione_CancellaUpl(idAllegato) {
    if (event != undefined)
        event.preventDefault();

    var isPrincipal = $('#riga-allegato-' + idAllegato).data('isprincipal');

    swal({
        title: 'Sei sicuro di voler eliminare il documento selezionato?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, elimina',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/EliminaAllegato",
            type: "POST",
            data: JSON.stringify({
                idAllegato: idAllegato
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Documento eliminato correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                $('#riga-allegato-' + idAllegato).remove();
                $('#div-upload-principale').show();
                if (isPrincipal === 1) {
                    var idDocOriginale = $('#identificativoAllegatoOriginale').val();
                    if (idDocOriginale !== null && idDocOriginale !== "" && typeof idDocOriginale !== "undefined") {
                        $('#identificativoAllegato').val(idDocOriginale);
                        $('#identificativoAllegato').data('riposizionamento', 'true');
                        $('#identificativoAllegato').data('contatore', '0');                        
                    }
                }
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        });
    });

    //Dematerializzazione_CheckTab2();
}

function Dematerializzazione_CancellaUplVirtuale(idAllegato) {
    $('#riga-allegato-' + idAllegato).data('removed', 'true');
    $('#riga-allegato-' + idAllegato).hide();
    var isPrincipal = $('#riga-allegato-' + idAllegato).data('isprincipal');
    if (isPrincipal === 1) {
        var idDocOriginale = $('#identificativoAllegatoOriginale').val();
        if (idDocOriginale !== null && idDocOriginale !== "" && typeof idDocOriginale !== "undefined") {
            $('#identificativoAllegato').val(idDocOriginale);
            $('#identificativoAllegato').data('riposizionamento', 'true');
            $('#identificativoAllegato').data('contatore', '0');
        }
    }
}

function Dematerializzazione_AbilitaInfoFileMultiplo() {
    var totalFiles = document.getElementById("fileupload-VSDIP-allegati-supporto").files.length;
    var formData = new FormData();

    for (var i = 0; i < totalFiles; i++) {
        var file = document.getElementById("fileupload-VSDIP-allegati-supporto").files[i];
        var nomefile = file.name;

        //if (nomefile.toLowerCase().indexOf(".pdf") < 0) {
        //    swal("Sono ammessi soltanto file pdf");
        //    return;
        //}

        if (nomefile.toLowerCase().indexOf(".pdf") < 0 &&
            nomefile.toLowerCase().indexOf(".doc") < 0 &&
            nomefile.toLowerCase().indexOf(".docx") < 0) {
            swal("Formato file non supportato. Sono ammessi soltanto .doc, .docx e .pdf");
            return;
        }

        formData.append('filePrincipale', false);
        formData.append('file', file);
        formData.append("nome", nomefile);
        formData.append("tipo", 'PDF');
    }

    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (request.readyState == 4 && request.status == 200) {
            var data = $.parseJSON(request.responseText);
            if (data.success) {
                Dematerializzazione_TempUpload_OnSuccess(data, false);
                $('#fileupload-VSDIP-allegati-supporto').val('');
            }
            else {
                Dematerializzazione_TempUpload_OnFailure(data);
            }
        }
    };

    request.open('post', "/Dematerializzazione/UploadTempDocument");
    request.timeout = 45000;
    request.send(formData);
}

function OnFailure(response) {
    swal({
        title: response.responseText,
        type: 'error',
        showConfirmButton: true,
        confirmButtonText: 'Ok',
        customClass: 'rai'
    });
}

function ReloadTabellaDocumentiDaApprovare() {
    RaiUpdateWidget('div_elenco_documenti', '/Dematerializzazione/GetContentInternal', 'html', { approvazioneEnabled: true }, false, null);
}

function Dematerializzazione_MarkVisualizzatoDocumento(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler rendere la richiesta come visualizzata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, segna come visualizzato',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        $.ajax({
            url: "/Dematerializzazione/DocumentoMarkAsVisualizzato",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Operazione eseguita correttamente',
                    type: 'success',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                //Dematerializzazione_ReloadTabellaDocumentiDaVisionare();
                Dematerializzazione_ReloadTabelleInPartenza(false);
                $('#modal-dettaglio-richiesta').modal('hide');
            }
        });
    });
}

function Dematerializzazione_ReloadTabellaDocumentiDaVisionare() {
    $.ajax({
        url: '/Dematerializzazione/GetContentVisionatore',
        type: "GET",
        data: {},
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#content-container-visionatore").html(data);
        }
    });
}

function Dematerializzazione_GetElencoRichieste() {
    $.ajax({
        url: '/Dematerializzazione/GetElencoRichieste',
        type: "GET",
        data: {},
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#content-container").html(data);
        }
    });
}

function Dematerializzazione_PrendiInCarico(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler prendere in carico la richiesta selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, prendi in carico',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/PrendiInCarico",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Operazione eseguita correttamente',
                    type: 'success',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
                Dematerializzazione_ReloadModalPrendiInCarico(matricola, idPersona, idDoc);
                //$('#modal-dettaglio-richiesta').modal('hide');
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            },
            complete: function () {
                Dematerializzazione_LoadTabellaDocumentiVSRUO();
            }
        });
    });
}

function Dematerializzazione_PrendiInCaricoAll(matricola, idPersona) {
    swal({
        title: 'Sei sicuro di voler prendere in carico le richieste selezionate?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, prendi in carico',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        swal({
            title: 'Presa in carico richieste in corso',
            type: 'info',
            html: "<label id='dem-label-elaborazione' class='rai-font-md'></label><br/><textarea id='dem-text-esito-presaInCarico' cols='40' rows='5' disabled></textarea>",
            showCancelButton: false,
            showConfirmButtonButton: false,
            confirmButtonText: 'Continua',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            inputPlaceholder: "Esito",
            customClass: 'rai rai-confirm-cancel'
        });

        var contatoreInizio = 0;
        var contatoreFine = $('input[type="checkbox"][id^="dem-check-"]:not("#dem-check-all-2"):checked').length;

        $('input[type="checkbox"][id^="dem-check-"][data-tab="2"]:not("#dem-check-all-2"):checked').each(function () {
            contatoreInizio++;
            $('#dem-label-elaborazione').text('Presa in carico domanda ' + contatoreInizio + ' di ' + contatoreFine);
            var idDoc = $(this).val();
            $.ajax({
                url: "/Dematerializzazione/PrendiInCarico",
                type: "POST",
                async: false,
                data: JSON.stringify({
                    matricola: matricola,
                    idPersona: idPersona,
                    idDoc: idDoc
                }),
                contentType: "application/json; charset=utf-8",
                dataType: 'html',
                success: function (data) {
                    var item = new Object();
                    item.id = idDoc;
                    item.esito = "Ok";
                    var tx = $('#dem-text-esito-presaInCarico').val();
                    tx = tx + "\nRichiesta " + idDoc + " esito: OK";
                    $('#dem-text-esito-presaInCarico').val(tx);
                },
                error: function (xhr, status) {
                    var item = new Object();
                    item.id = idDoc;
                    item.esito = xhr.statusText;
                    var tx = $('#dem-text-esito-presaInCarico').val();
                    tx = tx + "\nRichiesta " + idDoc + " esito: " + xhr.statusText;
                    $('#dem-text-esito-presaInCarico').val(tx);
                }
            });
        });
        Dematerializzazione_LoadTabellaDocumentiVSRUO();
    });
}

function Dematerializzazione_AbilitaNextButton() {
    var pronto1 = false;
    var pronto2 = false;
    var pronto3 = true;

    var tipologiaDocumentale = $('#tipologiaDocumentale').val();

    if (tipologiaDocumentale != null && tipologiaDocumentale != "" && tipologiaDocumentale != "-1") {
        tipologiaDocumentale = $.trim(tipologiaDocumentale);
        tipologiaDocumentale = tipologiaDocumentale.toUpperCase();
        pronto1 = true;
    }

    var tipodoc = $('#tipodoc').val();
    if (tipodoc != null && tipodoc != "" && tipodoc != "-1") {
        tipodoc = $.trim(tipodoc);
        tipodoc = tipodoc.toUpperCase();
        pronto2 = true;
    }

    var ricercaLibera = $('#RicercaLibera').val();

    if (tipologiaDocumentale === "VSDIP" && (ricercaLibera !== null && ricercaLibera !== "" && typeof ricercaLibera !== "undefined")) {
        ricercaLibera = ricercaLibera.trim();
        ricercaLibera = ricercaLibera.toUpperCase();
        //if (ricercaLibera === "TRUE") {
        //    $('#dem-selezione-destinatario').show();
        //}
        //else {
        //    $('#dem-selezione-destinatario').hide();
        //}
    }
    //else {
    //    $('#dem-selezione-destinatario').hide();
    //}

    if ($('#selMatricolaDestinatario').is(':visible')) {
        var scelta = $('#selMatricolaDestinatario').val();
        if (scelta !== null && scelta !== "" && typeof scelta !== "undefined") {
            pronto3 = true;
        }
        else {
            pronto3 = false;
        }
    }

    if (pronto1 && pronto2 && pronto3) {
        Dematerializzazione_SetScelteFase1();
    }
}

function Dematerializzazione_CheckTab2() {
    var pronto1 = false;
    var pronto2 = false;
    var pronto3 = false;
    var incaricato = "";
    var approvatore = "";
    var nomefile = "";
    var approvatoreVisibile = $('#ApprovatoreVisibile').val();
    var firmaVisibile = $('#FirmaVisibile').val();

    if (approvatoreVisibile.toUpperCase() === "FALSE") {
        pronto1 = true;
    }
    else if (approvatoreVisibile.toUpperCase() === "TRUE") {
        approvatore = $('#selApprovatore').val();
        if (approvatore !== null &&
            approvatore !== "undefined" &&
            approvatore !== "" &&
            approvatore !== "-1") {
            pronto1 = true;
        }
    }

    if (firmaVisibile.toUpperCase() === "FALSE") {
        pronto2 = true;
    }
    else if (firmaVisibile.toUpperCase() === "TRUE") {
        incaricato = $('#incaricatoFirma').val();
        if (incaricato !== null &&
            incaricato !== "undefined" &&
            incaricato !== "" &&
            incaricato !== "-1") {
            pronto2 = true;
        }
    }

    if ($("#nome-file-VSDIP").is(':visible')) {
        nomefile = $("#nome-file-VSDIP").text().split("\\").pop();
    }

    if (nomefile.toLowerCase().indexOf(".pdf") > 0) {
        pronto3 = true;
    }

    if (pronto1 && pronto2 && pronto3) {
        $('#btns-fase1-next').removeClass("disable");
    } else {
        if (!$('#btns-fase1-next').hasClass("disable")) {
            $('#btns-fase1-next').addClass("disable");
        }
    }
}

function Dematerializzazione_AbilitaForm() {
    $('div[id^="div-tipologiaDocumentale"]').each(function () {
        $(this).hide();
    });

    $('#div-select-tipologiaDocumentale').show();
    $('#div-tipologiaDocumentale').show();

    Dematerializzazione_AbilitaNextButton();
}

function Dematerializzazione_CheckTab1() {
    var pronto1 = false;
    var pronto2 = false;
    var incaricato = "";
    var approvatore = "";
    var approvatoreVisibile = $('#ApprovatoreVisibile').val();
    var firmaVisibile = $('#FirmaVisibile').val();

    if (approvatoreVisibile.toUpperCase() === "FALSE") {
        pronto1 = true;
    }
    else if (approvatoreVisibile.toUpperCase() === "TRUE") {
        approvatore = $('#selApprovatore').val();
        if (approvatore !== null &&
            approvatore !== "undefined" &&
            approvatore !== "" &&
            approvatore !== "-1") {
            pronto1 = true;
        }
    }

    if (firmaVisibile.toUpperCase() === "FALSE") {
        pronto2 = true;
    }
    else if (firmaVisibile.toUpperCase() === "TRUE") {
        incaricato = $('#incaricatoFirma').val();
        if (incaricato !== null &&
            incaricato !== "undefined" &&
            incaricato !== "" &&
            incaricato !== "-1") {
            pronto2 = true;
        }
    }

    if (pronto1 && pronto2) {
        $('#btns-fase1-next').removeClass("disable");
    } else {
        if (!$('#btns-fase1-next').hasClass("disable")) {
            $('#btns-fase1-next').addClass("disable");
        }
    }
}

function Dematerializzazione_OpenDettaglioVisionatore(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: false, presaInCaricoEnabled: false, presaInVisioneEnabled: true }, null, 'GET');
}

function Dematerializzazione_OpenDettaglioApprovatore(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: true, presaInCaricoEnabled: false, presaInVisioneEnabled: false }, null, 'GET');
}

function Dematerializzazione_OpenDettaglioPrendiInCarico(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: false, presaInCaricoEnabled: true, presaInVisioneEnabled: false }, null, 'GET');
}

function Dematerializzazione_LoadTabellaDocumentiVSRUO() {
    $("#div-dem-documenti").html('');
    $.ajax({
        url: '/Dematerializzazione/GetDocumentiSettoriInterni',
        type: "GET",
        data: {},
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#div-dem-documenti").html(data);
        }
    });
}

function Dematerializzazione_EliminaPratica(matricola, idPersona, idDoc) {
    swal({
        title: 'Sicuro di voler eliminare la pratica selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, elimina',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/EliminaPratica",
            type: "POST",
            async: false,
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                swal({
                    title: 'Pratica eliminata correttamente',
                    type: 'success',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                }).then(function () {
                    $('#modal-dettaglio-richiesta').modal('hide');
                });
            },
            error: function (xhr, status) {
                swal({
                    title: xhr.statusText,
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        });
    });
}

function Dematerializzazione_ChiudiInserimento() {
    $('#modal-inserimentoDocDem-internal').html('');
    $('#modal-inserimentoDocDem').modal('hide');
    $('#modal-modificaRichiestaDematerializzazione-internal').html('');
    $('#modal-modificaRichiestaDematerializzazione').modal('hide');
}

function Dematerializzazione_ReloadTabellaDocumentiOperatore() {
    $.ajax({
        url: '/Dematerializzazione/GetContentOperatore',
        type: "GET",
        data: {},
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#content-container-operatore").html(data);
        }
    });
}

function Dematerializzazione_OpenDettaglioOperatore(matricola, idPersona, idDoc) {
    RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: false, presaInCaricoEnabled: false, presaInVisioneEnabled: false }, null, 'GET');
}

function Dematerializzazione_ModificaPratica(idDoc) {
    $.ajax({
        url: "/Dematerializzazione/SetPresoInModifica",
        type: "GET",
        async: false,
        data: {
            idDoc: idDoc
        },
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data.message != "") {
                swal({
                    title: data.message,
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sì, procedi!',
                    cancelButtonText: 'Annulla',
                    reverseButtons: true,
                    customClass: 'rai rai-confirm-cancel'
                }).then(function () {
                    Dematerializzazione_ApriInModificaIlDocumento(idDoc);
                });
            } else if (data.error) {
                swal({
                    title: data.error,
                    type: 'error',
                    //confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            } else if (data.message == "") {
                Dematerializzazione_ApriInModificaIlDocumento(idDoc);
            }
        },
        error: function (xhr, status) {
            swal({
                title: 'Si è verificato un errore nel reperimento dei dati del documento',
                type: 'error',
                //confirmButtonClass: "btn btn-primary btn-lg",
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function Dematerializzazione_ApriInModificaIlDocumento(idDoc) {
    $('#modal-inserimentoDocDem-internal').data('isDirty', 'false');
    $('#modal-inserimentoDocDem-internal').html('');
    $('#modal-dettaglio-richiesta-internal').html('');
    $('#modal-dettaglio-richiesta').modal('hide');
    RaiOpenAsyncModal('modal-modificaRichiestaDematerializzazione', '/Dematerializzazione/GetDocumentoPerModifica', { idDoc: idDoc }, null, 'GET');
}

function GetDettaglio_Approvatore(id) {
    Dematerializzazione_OpenDettaglioApprovatore(null, 0, id);
}

function GetDettaglio_Operatore(id) {
    Dematerializzazione_OpenDettaglioOperatore(null, 0, id);
}

function GetDettaglio_Visionatore(id) {
    Dematerializzazione_OpenDettaglioVisionatore(null, 0, id);
}

function Dematerializzazione_Btn1BisNext_Click() {
    $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');

    if ($("#modal-inserimentoDocDem").is(':visible')) {
        $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
    }
    $('#tab1-custom-data').hide();
    $('#btns-fase1-custom').hide();

    var idDoc = $('#identificativoAllegato').val();
    if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
        Dematerializzazione_AttivaElementiTab(3);
    }
    else {
        Dematerializzazione_AttivaElementiTab(2);
        Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc);
    }
}

function Dematerializzazione_GetCustomData() {
    // struttura oggetto
    //{
    //  "id": "servizio",
    //  "title": "Servizio",
    //  "label": "Servizio",
    //  "nome": "servizio",
    //  "testo": "Servizio",
    //  "valore": null,
    //  "azioni": null,
    //  "ordinamento": 4,
    //  "tipo": 1,
    //  "required": true,
    //  "TagHRDW": null,
    //  "selectListItems": null
    //}

    var attrs = new Array();

    $('.dem-customdata').each(function () {

        var tx = $(this).val();
        tx = $.trim(tx);

        var id = $(this).attr('id');
        if (id !== null && id !== "" && typeof id !== "undefined") {

            var tipo = $(this).attr('type');

            var gruppo = "";
            var checked = false;

            if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "radio") {
                gruppo = $(this).data('check-group');
                checked = $(this).is(':checked');
            }

            var obj = new Object();

            obj.id = $(this).attr('id');
            obj.valore = $(this).val();
            obj.tipo = $(this).data('tipo');
            obj.gruppo = gruppo;
            obj.checked = checked;

            attrs.push(obj);
        }
    });

    var myJsonString = JSON.stringify(attrs);

    return myJsonString;
}
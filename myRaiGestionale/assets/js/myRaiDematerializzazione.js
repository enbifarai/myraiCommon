function Dematerializzazione_SetScelteFase1() {
    var tipologiaDocumentale = $('#tipologiaDocumentale').val();
    tipologiaDocumentale = $.trim(tipologiaDocumentale);
    tipologiaDocumentale = tipologiaDocumentale.toUpperCase();

    var tipologiaDocumento = $('#tipodoc').val();
    tipologiaDocumento = $.trim(tipologiaDocumento);
    tipologiaDocumento = tipologiaDocumento.toUpperCase();

    var matricola = "";
    if ($('#selMatricolaDestinatario').is(':visible')) {
        var scelta = $('#selMatricolaDestinatario').val();
        if (scelta !== null && scelta !== "") {
            matricola = scelta;
        }
    }

    $.ajax({
        url: "/Dematerializzazione/SetScelteFase1",
        type: "POST",
        async: false,
        data: JSON.stringify({
            tipologiaDocumentale: tipologiaDocumentale,
            tipologiaDocumento: tipologiaDocumento,
            destinatario: matricola
        }),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            // data sarà un oggetto di tipo XR_DEM_TIPIDOC_COMPORTAMENTO
            if (data !== null) {
                // definisce se l'approvatore sarà visibile
                $('#ApprovazioneObbligatoria').val(data.ApprovazioneObbligatoria);
                $('#FirmaObbligatoria').val(data.FirmaObbligatoria);
                $('#PosizionaProtocollo').val(data.PosizionaProtocollo);
                $('#ApprovatoreVisibile').val(data.ApprovatoreVisibile);
                $('#FirmaVisibile').val(data.FirmaVisibile);
                $('#FileObbligatorio').val(data.FileObbligatorio);
                $('#IsCustomType').val(data.IsCustomType);
                $('#FileAggiuntivoObbligatorio').val(data.FileAggiuntivoObbligatorio);
                if (data.MatricolaDestinatarioVisibile.toString().toUpperCase() === "FALSE") {
                    $('#dem-selezione-destinatario').hide();
                    $('#selMatricolaDestinatario').val('');
                    $('#MatricolaDestinatario').val('');
                }
                else {
                    $('#dem-selezione-destinatario').show();
                }

                if (data.NominativoUtenteVistatore !== "" && data.NominativoUtenteVistatore !== null
                    && typeof (data.NominativoUtenteVistatore) !== "undefined") {
                    $('#div-visionatore').show();
                    $('#nominativo-visionatore').html(data.NominativoUtenteVistatore);

                    $.ajax({
                        url: '/Dematerializzazione/RenderViewMultiSelezione',
                        type: "POST",
                        cache: false,
                        dataType: 'html',
                        success: function (data) {
                            if (data !== null) {
                                $('#div-visionatore').html('');
                                $('#div-visionatore').html(data);
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
                        },
                    });
                }
                else {
                    $('#div-visionatore').hide();
                }

                if (!data.ApprovatoreVisibile) {
                    $('#selApprovatore').val("-1");
                    $('#div-Approvatore').hide();
                }
                else {

                    var scelta = "";
                    var matr = "";

                    if ($('#selMatricolaDestinatario').is(':visible')) {
                        scelta = $('#selMatricolaDestinatario').val();
                    }
                    else {
                        scelta = $('#MatricolaDestinatario').val();
                    }

                    if (scelta !== null && scelta !== "" && typeof scelta !== "undefined") {
                        matr = scelta;
                    }

                    $('#div-Approvatore').show();
                    var idPratica = $('#idPratica').val();
                    RaiSelectExtLoadAsyncData('selApprovatore', '/Dematerializzazione/GetElencoApprovatoriJSON', { matr: matr, idPratica: idPratica });
                }

                if (data.FirmaVisibile) {
                    RaiSelectExtLoadAsyncData('incaricatoFirma', '/Dematerializzazione/GetIncaricatiFirmaJSON');
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
                }

                if (data.PannelloAllegati) {
                    $('#div-pannelloallegati').show();
                    //$('li.no-allegati').each(function () {
                    //    $(this).hide();
                    //});
                    //$('li.si-allegati').each(function () {
                    //    $(this).show();
                    //});
                }
                else {
                    $('#div-pannelloallegati').hide();
                    //$('li.no-allegati').each(function () {
                    //    $(this).show();
                    //});
                    //$('li.si-allegati').each(function () {
                    //    $(this).hide();
                    //});
                }

                var matrDest = "";
                var tempAll = '';
                if (data.MatricolaDestinatarioVisibile.toString().toUpperCase() === "FALSE" &&
                    !data.FileObbligatorio) {
                    $('#btns-fase1-next').removeClass('disable');
                }
                else if (data.MatricolaDestinatarioVisibile.toString().toUpperCase() === "TRUE" &&
                    !data.FileObbligatorio) {
                    if ($('#selMatricolaDestinatario').is(':visible')) {
                        matrDest = $('#selMatricolaDestinatario').val();
                    }
                    else {
                        matrDest = $('#MatricolaDestinatario').val();
                    }

                    if (matrDest !== null && matrDest !== "" && typeof matrDest !== "undefined") {
                        $('#btns-fase1-next').removeClass('disable');
                    }
                }
                else if (data.MatricolaDestinatarioVisibile.toString().toUpperCase() === "FALSE" &&
                    data.FileObbligatorio) {

                    $('tr[id^="riga-allegato-"]').each(function () {
                        var id = $(this).data('id');
                        if (tempAll === '') {
                            tempAll = id;
                        }
                        else {
                            tempAll = tempAll + "," + id;
                        }
                    });

                    if (tempAll !== null && tempAll !== "" && typeof tempAll !== "undefined") {
                        $('#btns-fase1-next').removeClass('disable');
                    }

                }
                else if (data.MatricolaDestinatarioVisibile.toString().toUpperCase() === "TRUE" &&
                    data.FileObbligatorio) {
                    if ($('#selMatricolaDestinatario').is(':visible')) {
                        matrDest = $('#selMatricolaDestinatario').val();
                    }
                    else {
                        matrDest = $('#MatricolaDestinatario').val();
                    }

                    $('tr[id^="riga-allegato-"]').each(function () {
                        var id = $(this).data('id');
                        if (tempAll === '') {
                            tempAll = id;
                        }
                        else {
                            tempAll = tempAll + "," + id;
                        }
                    });

                    if (matrDest !== null && matrDest !== "" && typeof matrDest !== "undefined" &&
                        tempAll !== null && tempAll !== "" && typeof tempAll !== "undefined") {
                        $('#btns-fase1-next').removeClass('disable');
                    }
                }
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
    if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
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
                if (data.message !== "") {
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

//function Dematerializzazione_IsCustomType_And_LoadData(stop) {
//    var isCustomType = false;

//    if (typeof stop === "undefined" || stop === "" || stop === null) {
//        stop = false;
//    }

//    $.ajax({
//        url: '/Dematerializzazione/IsCustomType',
//        type: "GET",
//        data: {
//        },
//        async: false,
//        cache: false,
//        contentType: "application/json; charset=utf-8",
//        dataType: 'json',
//        success: function (data) {
//            if (data.esito) {
//                if (data.customType === "NONE") {
//                    // TO DO
//                    isCustomType = false;
//                }
//                else if (data.customType === "CUSTOMVIEW") {
//                    // TO DO
//                    isCustomType = true;
//                }
//                else if (data.customType === "CUSTOMDATA") {
//                    // TO DO
//                    isCustomType = true;
//                    if (!stop) {
//                        Dematerializzazione_CaricaCustomData();
//                    }
//                }
//            }
//            else {
//                swal({
//                    title: data.error,
//                    type: 'error',
//                    showConfirmButton: true,
//                    confirmButtonText: 'Ok',
//                    customClass: 'rai'
//                });
//            }
//        }
//    });

//    return isCustomType;
//}

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
    var idDocOriginale = $('#identificativoAllegatoOriginale').val();


    //----------------------------------------------------------
    //-------- Gestione campi obbligatori primo tab ------------
    //----------------------------------------------------------
    if ($('#tipologiaDocumentale').val() == -1 ||
        $('#tipodoc').val() == -1 ||
        ($('#selMatricolaDestinatario').is(':visible') && $('#selMatricolaDestinatario').val() == -1) ||
        ($('#FileObbligatorio').val().toUpperCase() === "TRUE" && document.getElementById("tabella-file-principale").rows.length == 0)) {
        if (idDocOriginale !== null && idDocOriginale !== "" && typeof (idDocOriginale) !== "undefined") {

        } else {
            swal({
                title: "Compilare tutti i campi obbligatori",
                type: 'warning',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
            return false;
        }

    }
    //----------------------------------------------------------

    if (idDocOriginale !== null && idDocOriginale !== "" && typeof (idDocOriginale) !== "undefined") {
        Dematerializzazione_Btn1Next_Click_InModifica();
        return false;
        
    }
    
    var idDoc = $('#identificativoAllegato').val();
    var isCustomType = $('#IsCustomType').val();
    var fileObbligatorio = $('#FileObbligatorio').val();

    isCustomType = (isCustomType.toUpperCase() === "TRUE");
    fileObbligatorio = (fileObbligatorio.toUpperCase() === "TRUE");

    if ((fileObbligatorio && $('#div-tipologiaDocumentale').is(':visible')) || (isCustomType && !fileObbligatorio)) {
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

function Dematerializzazione_AjaxCaricaCustomData() {
    var matricola = "";
    if ($('#selMatricolaDestinatario').is(':visible')) {
        var scelta = $('#selMatricolaDestinatario').val();
        if (scelta !== null && scelta !== "") {
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
            $('#tab1').hide();
            //var x = document.getElementById("tab1-custom-data").innerHTML;
            //var x = document.getElementById("div-proto");
            //x.style.display = "block";
            //x = document.getElementById("div-proto").innerHTML;

            $('#tab1-custom-data').html(data);
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

function Dematerializzazione_CaricaCustomData() {
    $.when(Dematerializzazione_AjaxCaricaCustomData()).done(function (a1) {
        Dematerializzazione_AbilitaSeCompilati();
    });
}

function Dematerializzazione_Btn1Next_Click_InModifica() {
    var isCustomType = $('#IsCustomType').val();
    var fileObbligatorio = $('#FileObbligatorio').val();

    isCustomType = (isCustomType.toUpperCase() === "TRUE");
    fileObbligatorio = (fileObbligatorio.toUpperCase() === "TRUE");

    if ($('#div-tipologiaDocumentale').is(':visible') ||
        (isCustomType && !fileObbligatorio)) {
        if ($("#modal-inserimentoDocDem").is(':visible')) {
            $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
        }

        if (isCustomType) {
            $('#tab1').hide();
            $('#tab1-custom-data').show();
            $('#btns-fase1').hide();
            $('#btns-fase1-custom').show();
            Dematerializzazione_CheckedAutomatico();
            return false;
        }

        var isPDF = $('#isPDF').val();
        if (isPDF === "TRUE") {
            Dematerializzazione_AttivaElementiTab(2);
            var idDoc = $('#identificativoAllegato').val();
            if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
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

    var offLeft1 = null;
    var offLeft2 = null;
    var offLeft3 = null;

    var offTop1 = null;
    var offTop2 = null;
    var offTop3 = null;

    var left = $('#ItemPreview').position().left;
    left = parseInt(left, 10);

    if (posizione === 'NONE') {
        $("#draggable").data('mosso', 'false');
        $("#draggableData").data('mosso', 'false');
        $("#draggableFirma").data('mosso', 'false');
    }
    else if (posizione === 'START') {

        offTop1 = $("#draggable").data('oldtop');
        offLeft1 = $("#draggable").data('oldleft');

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

        offTop2 = $("#draggableData").data('oldtop');
        offLeft2 = $("#draggableData").data('oldleft');

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

        offTop3 = $("#draggableFirma").data('oldtop');
        offLeft3 = $("#draggableFirma").data('oldleft');

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

        offLeft1 = margineSx + left;
        offLeft2 = margineSx + left;
        offLeft3 = margineSx + left;

        offTop1 = 0;
        offTop2 = 45;
        offTop3 = 85;

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
    RaiSelectExtLoadAsyncData('selPaginaProtocollo', '/Dematerializzazione/GetNumeroPagineDocumentoJSON', { idAllegato: idDoc, nPagina: 1 });

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
        var paginaSelezionata = $('#selPagina').val();
        var i_paginaSelezionata = parseInt(paginaSelezionata, 10);
        if (i_paginaSelezionata === -1000) {
            // se la pagina selezionata è -1000 vuol dire che nella combo
            // è stata selezionata la voce "NESSUNA", quindi il controllo
            // sul posizionamento della firma non va effettuato
            val3 = "TRUE";
        }

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

    var isCustomType = $('#IsCustomType').val();

    if (isCustomType) {
        var nTab = 1;

        $('#tab-dem-' + nTab).removeClass('disable');
        $('#tab-dem-' + nTab).addClass('active');

        $('#tab1-custom-data').show();
        $('#btns-fase1-custom').show();

        $('#tab-dem-2').removeClass('active');
        $('#tab-dem-2').addClass('disable');
        $('#tab-dem-2').addClass('completed');

        for (var j = 2; j <= 4; j++) {
            $('#tab-dem-' + j).removeClass('active');
            $('#tab-dem-' + j).addClass('disable');
            $('#tab-dem-' + j).removeClass('completed');
            $('#btns-fase' + j).hide();
            $('#tab' + j).hide();
            if (j === 2) {
                $('#tab2-pannello-abilita-posizionamento').hide();
            }
        }

    }
    else {
        Dematerializzazione_AttivaElementiTab(1);
    }
}

function Dematerializzazione_Btn3Prev_Click() {

    var isCustomType = $('#IsCustomType').val();
    isCustomType = (isCustomType.toUpperCase() === "TRUE");

    if (isCustomType) {
        var nTab = 1;

        $('#tab-dem-1').removeClass('disable');
        $('#tab-dem-1').addClass('active');
        $('#btns-fase1-custom').show();
        $('#tab1-custom-data').show();


        for (var i = nTab + 1; i <= 4; i++) {
            $('#tab-dem-' + i).removeClass('active');
            $('#tab-dem-' + i).addClass('disable');
            $('#tab-dem-' + i).removeClass('completed');
            $('#btns-fase' + i).hide();
            $('#tab' + i).hide();
            if (i === 2) {
                $('#tab2-pannello-abilita-posizionamento').hide();
            }
        }

    }
    else {
        Dematerializzazione_AttivaElementiTab(2);
    }

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
        $('#dem-sel-pagina-documento').hide();

        var guid = $('#selPaginaProtocollo').data('rai-select');
        var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
        nPagine = parseInt(nPagine, 10);

        var paginarichiesta = $('#dem-paginaprotocollo').data('paginarichiesta');
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (nPagine < paginarichiesta || isNaN(paginarichiesta)) {
            paginarichiesta = 1;
            $('#dem-paginaprotocollo').data('paginarichiesta', '1');
        }

        var paginacorrente = $('#dem-paginaprotocollo').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (nPagine < paginacorrente || isNaN(paginacorrente)) {
            paginacorrente = 1;
            $('#dem-paginaprotocollo').data('paginacorrente', '1');
        }

        //var pagina = $('#selPagina').val();
        //pagina = parseInt(pagina, 10);

        //if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
        //    pagina = 1;
        //    $('#selPagina').val('1');
        //}

        var paginaProtocollo = $('#dem-paginaprotocollo').data('paginaprotocollo');
        paginaProtocollo = parseInt(paginaProtocollo, 10);

        if (nPagine < paginaProtocollo || isNaN(paginaProtocollo)) {
            paginaProtocollo = 1;
            $('#dem-paginaprotocollo').data('paginafirma', '1');
        }

        //if (paginacorrente !== 1) {
        //    Dematerializzazione_CambioPagina(1, true, $('#draggableData').is(':visible'), false);
        //}
        //else if (paginacorrente === 1) {
        //    $('#draggable').show();
        //}

        //if ($('#icona3-firma').hasClass('evidenzia') && paginafirma !== 1) {
        //    $('#icona3-firma').removeClass('evidenzia');
        //}

        //if (!$('#icona1-protocollo').hasClass('evidenzia')) {
        //    $('#icona1-protocollo').addClass('evidenzia');
        //}

        var pagina = $('#selPaginaProtocollo').val();
        pagina = parseInt(pagina, 10);

        if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
            pagina = 1;
            $('#selPaginaProtocollo').val('1');
        }

        if (!$('#icona1-protocollo').hasClass('evidenzia')) {
            $('#icona1-protocollo').addClass('evidenzia');
        }

        if (paginacorrente !== paginaProtocollo) {
            Dematerializzazione_CambioPaginaProtocollo(paginaProtocollo);
        }
        else if (paginacorrente === paginaProtocollo) {
            $('#draggable').show();
            $('#draggableData').show();
            if (nPagine !== 1) {
                $('#dem-paginaprotocollo').show();
            }
        }
    }

    Dematerializzazione_AbilitaPosizionamentoData();

    if ((nPagine - 1) === 1) {
        $('#dem-paginaprotocollo').hide();
    }
}

function Dematerializzazione_AbilitaPosizionamentoData() {
    var posizionaProtocollo = $('#PosizionaProtocollo').val();

    if (posizionaProtocollo.toUpperCase() === "TRUE") {

        var guid = $('#selPaginaProtocollo').data('rai-select');
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

        var pagina = $('#selPaginaProtocollo').val();
        pagina = parseInt(pagina, 10);

        if (pagina === null || pagina === "" || typeof pagina === "undefined" || isNaN(pagina)) {
            pagina = 1;
            $('#selPaginaProtocollo').val('1');
        }

        var paginafirma = $('#dem-sel-pagina-documento').data('paginafirma');
        paginafirma = parseInt(paginafirma, 10);

        if (nPagine < paginafirma || isNaN(paginafirma)) {
            paginafirma = 1;
            $('#dem-sel-pagina-documento').data('paginafirma', '1');
        }

        if (paginacorrente !== 1) {
            Dematerializzazione_CambioPagina(paginacorrente, $('#draggable').is(':visible'), true, false);
        }
        else if (paginacorrente === 1) {
            $('#draggableData').show();
        }

        if (!$('#icona2-data').hasClass('evidenzia')) {
            $('#icona2-data').addClass('evidenzia');
        }

        //if ($('#icona3-firma').hasClass('evidenzia') && paginafirma !== 1) {
        //    $('#icona3-firma').removeClass('evidenzia');
        //}
    }
}

function Dematerializzazione_AbilitaPosizionamentoFirma() {
    var posizionaProtocollo = $('#PosizionaProtocollo').val();

    if (posizionaProtocollo.toUpperCase() === "TRUE") {

        $('#dem-paginaprotocollo').hide();

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

        if ((nPagine - 1) === 1) {
            $('#dem-sel-pagina-documento').hide();
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

    if (pagina === -1000) {
        abilitaProt = true;
        abilitaData = true;
        abilitaFirma = false;
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
            var paginaProtocollo = $('#selPaginaProtocollo').val(); //Pagina Protocollo
            var paginarFirma = $('#selPagina').val();               //Pagina Firma
            paginacorrente = $('#dem-sel-pagina-documento').data('paginacorrente');

            if (paginaProtocollo == pagina) {
                $('#icona1-protocollo').addClass('evidenzia');
                $('#draggable').show();
            }
            else {
                //$('#icona1-protocollo').removeClass('evidenzia');
                $('#draggable').hide();
            }

            if (paginaProtocollo == pagina) {
                $('#icona2-data').addClass('evidenzia');
                $('#draggableData').show();
            }
            else {
                //$('#icona2-data').removeClass('evidenzia');
                $('#draggableData').hide();
            }

            if (paginarFirma == paginacorrente || abilitaFirma) {
                $('#icona3-firma').addClass('evidenzia');
                $('#draggableFirma').show();
                //$('#dem-sel-pagina-documento').show();
            }
            else {
                //$('#icona3-firma').removeClass('evidenzia');
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
    $('#dem-sel-pagina-documento').data('paginacorrente', '1');
    var paginaSelezionata = 1;
    var npagina = $('#dem-sel-pagina-documento').data('paginafirma');
    if (npagina !== null && npagina !== "" && typeof npagina === "undefined") {
        npagina = parseInt(npagina, 10);
        paginaSelezionata = npagina;
    }

    if (idDoc > 0) {
        RaiSelectExtLoadAsyncData('selPagina', '/Dematerializzazione/GetNumeroPagineDocumentoJSON', { idAllegato: idDoc, nPagina: paginaSelezionata });
        RaiSelectExtLoadAsyncData('selPaginaProtocollo', '/Dematerializzazione/GetNumeroPagineDocumentoJSON', { idAllegato: idDoc, nPagina: paginaSelezionata });

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
        $('#btns-fase' + i + '-custom').hide();
        $('#tab' + i).hide();
        $('#tab' + i + '-custom-data').hide();
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

    for (var j = nTab + 1; j <= 4; j++) {
        $('#tab-dem-' + j).removeClass('active');
        $('#tab-dem-' + j).addClass('disable');
        $('#tab-dem-' + j).removeClass('completed');
        $('#btns-fase' + j).hide();
        $('#tab' + j).hide();
        if (j === 2) {
            $('#tab2-pannello-abilita-posizionamento').hide();
        }
    }


    //Dopo il tab del Protocollo e Firma controllo se l'approvatore/Incaricato alla firma è uno soltanto, nel caso lo seleziono direttamente
    if (nTab == 3) {
        var apprId = _RaiSelectIdentifier('selApprovatore');
        var optionList = $('#' + apprId).find('.rai-select-option');
        if (optionList.length == 1) {
            if (optionList[0].dataset.optionValue != -1) {
                RaiSelectOption(optionList[0].dataset.optionValue, apprId)
            }
        }

        var apprId = _RaiSelectIdentifier('incaricatoFirma');
        var optionList = $('#' + apprId).find('.rai-select-option');
        if (optionList.length == 1) {
            if (optionList[0].dataset.optionValue != -1) {
                RaiSelectOption(optionList[0].dataset.optionValue, apprId)
            }
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
                RaiUpdateWidget('div_elenco_documenti', '/Dematerializzazione/GetContentInternal', 'html', { approvazioneEnabled: true }, false, null, false, 'GET');
                RaiUpdateWidget('content-container-operatore', '/Dematerializzazione/GetContentOperatore', 'html', {}, false, null, false, 'GET');
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

        if (txt === null || txt === "" || $.trim(txt) === "") {
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
                if (data.result === "OK") {
                    swal({
                        title: 'Operazione eseguita correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
                else {
                    swal({
                        title: data.infoAggiuntive,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }

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
    if (typeof ricercaLibera === "undefined" || ricercaLibera === null || ricercaLibera === "") {
        ricercaLibera = false;
    }

    $('#modal-inserimentoDocDem-internal').data('isDirty', 'false');
    $('#modal-inserimentoDocDem-internal').html('');
    $('#modal-dettaglio-richiesta-internal').html('');
    $('#modal-modificaRichiestaDematerializzazione-internal').html('');

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
    if (typeof approvazioneEnabled === "undefined" || approvazioneEnabled === null) {
        approvazioneEnabled = false;
    }
    if (typeof presaInCaricoEnabled === "undefined" || presaInCaricoEnabled === null) {
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
    var nomefile = ($("#fileupload-VSDIP").val().split("\\").pop());
    if (nomefile.toLowerCase().indexOf(".pdf") < 0) {
        swal("Formato file non supportato. Sono ammessi soltanto .pdf");
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
        if (request.readyState === 4 && request.status === 200) {
            var data = $.parseJSON(request.responseText);
            if (data.success) {
                Dematerializzazione_TempUpload_OnSuccess(data, true, 1);
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

function Dematerializzazione_TempUpload_OnSuccess(response, isPrincipal, _tipologiaDocumento) {
    $.ajax({
        url: '/Dematerializzazione/DrawTRFile',
        type: "GET",
        data: {
            idAllegato: response.responseText,
            isPrincipal: isPrincipal,
            tipologiaDocumento: _tipologiaDocumento
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

            if (!isPrincipal) {
                Dematerializzazione_AbilitaButtonFase3();
            }
        }
    });
}

function Dematerializzazione_VisualizzaDocumentoTemporaneo(idAllegato) {
    RaiOpenAsyncModal('modal-viewer-temp-allegati', '/Dematerializzazione/GetAllegatoTemporaneo', { idAllegato: idAllegato }, null, 'GET');
}

function Dematerializzazione_UpdateperSiglaperAllegato(IdAllegato) {
    var checked = $('#chkSigla-' + IdAllegato).is(":checked");

    $.ajax({
        url: '/Dematerializzazione/UpdateDaSiglareAllegato',
        type: "GET",
        data: {
            idAllegato: IdAllegato,
            value: checked
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () { },
        success: function (data) {
        }
    });
}

function Dematerializzazione_CancellaUpl(idAllegato) {
    if (event !== undefined)
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
                else {
                    Dematerializzazione_AbilitaButtonFase3();
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
    else {
        Dematerializzazione_AbilitaButtonFase3();
    }
}

function Dematerializzazione_AbilitaInfoFileMultiplo() {
    var totalFiles = document.getElementById("fileupload-VSDIP-allegati-supporto").files.length;
    var formData = new FormData();

    for (var i = 0; i < totalFiles; i++) {
        var file = document.getElementById("fileupload-VSDIP-allegati-supporto").files[i];
        var nomefile = file.name;

        if (nomefile.toLowerCase().indexOf(".pdf") < 0) {
            swal("Formato file non supportato. Sono ammessi soltanto .pdf");
            return;
        }

        formData.append('filePrincipale', false);
        formData.append('file', file);
        formData.append("nome", nomefile);
        formData.append("tipo", 'PDF');
    }

    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            var data = $.parseJSON(request.responseText);
            if (data.success) {
                Dematerializzazione_TempUpload_OnSuccess(data, false, 2);
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

function Dematerializzazione_CaricaTipologieDematerializzazioni() {
    var tipologiaDocumentale = $('#tipologiaDocumentale').val();
    if (!$('#btns-fase1-next').hasClass('disable')) {
        $('#btns-fase1-next').addClass('disable');
    }

    if (tipologiaDocumentale !== null && tipologiaDocumentale !== "" && tipologiaDocumentale !== "-1") {
        tipologiaDocumentale = $.trim(tipologiaDocumentale);
        tipologiaDocumentale = tipologiaDocumentale.toUpperCase();
        RaiSelectExtLoadAsyncData('tipodoc', '/Dematerializzazione/GetTipologieDematerializzazioniJSON', { codice: null, tipologiaDoc: tipologiaDocumentale });
    }
}

function Dematerializzazione_AbilitaNextButton() {
    var pronto1 = false;
    var pronto2 = false;
    var pronto3 = true;

    var tipologiaDocumentale = $('#tipologiaDocumentale').val();

    if (tipologiaDocumentale !== null && tipologiaDocumentale !== "" && tipologiaDocumentale !== "-1" && typeof tipologiaDocumentale !== "undefined" && tipologiaDocumentale !== "undefined") {
        tipologiaDocumentale = $.trim(tipologiaDocumentale);
        tipologiaDocumentale = tipologiaDocumentale.toUpperCase();
        pronto1 = true;
    }

    var tipodoc = $('#tipodoc').val();
    if (tipodoc !== null && tipodoc !== "" && tipodoc !== "-1" && typeof tipodoc !== "undefined" && tipodoc !== "undefined") {
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

    if (pronto1 && pronto2) {
        Dematerializzazione_SetScelteFase1();
    }

    //if (pronto1 && pronto2 && pronto3) {
    //    Dematerializzazione_SetScelteFase1();
    //}
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

    if (approvatoreVisibile !== null && approvatoreVisibile !== "" && typeof approvatoreVisibile !== "undefined" && approvatoreVisibile.toString().toUpperCase() === "FALSE") {
        pronto1 = true;
    }
    else if (approvatoreVisibile !== null && approvatoreVisibile !== "" && typeof approvatoreVisibile !== "undefined" && approvatoreVisibile.toString().toUpperCase() === "TRUE") {
        approvatore = $('#selApprovatore').val();
        if (approvatore !== null &&
            approvatore !== "undefined" &&
            approvatore !== "" &&
            approvatore !== "-1") {
            pronto1 = true;
        }
    }
    else if (approvatoreVisibile === null || approvatoreVisibile === "" || typeof approvatoreVisibile !== "undefined") {
        pronto1 = false;
    }

    if (firmaVisibile.toString().toUpperCase() === "FALSE") {
        pronto2 = true;
    }
    else if (firmaVisibile.toString().toUpperCase() === "TRUE") {
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

function Dematerializzazione_OpenDettaglioPrendiInCarico(matricola, idPersona, idDoc, nascondiBottoniPrendiInCarico) {
    if (nascondiBottoniPrendiInCarico) {
        RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: false, presaInCaricoEnabled: false, presaInVisioneEnabled: false }, null, 'GET');
    }
    else {
        RaiOpenAsyncModal('modal-dettaglio-richiesta', '/Dematerializzazione/GetDettaglioRichiesta', { m: matricola, id: idPersona, idDoc: idDoc, approvatoreEnabled: false, presaInCaricoEnabled: true, presaInVisioneEnabled: false }, null, 'GET');
    }
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
                    Dematerializzazione_ReloadTabellaDocumentiOperatore();
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
    $('#modal-inserimentoDocDem').modal('hide');
    $('#modal-inserimentoDocDem-internal').html('');
    $('#modal-modificaRichiestaDematerializzazione').modal('hide');
    $('#modal-modificaRichiestaDematerializzazione-internal').html('');
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
            if (data.message !== "") {
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
            } else if (data.message === "") {
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

function Dematerializzazione_CustomData_TuttiCompilati() {
    var result = false;
    let count = $('.dem-customdata').filter('[required="required"]:visible').length;
    let countTabCustomDem = $('.tabCustomDem').filter('[required="required"]:visible').length;
    let countTabCustomDemDropDownList = $('.tabCustomDemDropDownList').filter('[required="required"]:visible').length;

    if (count === 0 && countTabCustomDem === 0 && countTabCustomDemDropDownList === 0) {
        return true;
    }
    $('.dem-customdata').filter('[required="required"]:visible').each(function () {
        try {
            var compilato = $(this).context.attributes["compilato"].value;
            if (compilato === "false") {
                result = false;
                return false;
            }
        } catch (e) {
            //var id = $(this).attr('id');
            var _resultInternal = $(this).data('compilato');
            if (_resultInternal === null || _resultInternal === "" || typeof _resultInternal === "undefined" || _resultInternal === "false" || _resultInternal === false) {
                result = false;
                return false;
            }
        }

        result = true;
    });

    $('.tabCustomDemMultiSelect').filter('[required="required"]:visible').each(function () {
        var id = $(this).context.innerText;

        if (id === "Seleziona i valori") {
            result = false;
            return false;
        }

        result = true;
    });

    $('.tabCustomDemDropDownList').filter('[required="required"]:visible').each(function () {
        var compilato = $(this).context.attributes["compilato"].value;

        if (compilato === "false") {
            result = false;
            return false;
        }

        result = true;
    });

    let countAlert = $('.alert-danger').length;
    if (countAlert) {
        result = false;
    }

    return result;
}

function Dematerializzazione_RichiestaGiaPresente() {
    var result = false;

    var matricolaDestinatario = "";
    matricolaDestinatario = $('#selMatricolaDestinatario').val();

    if (matricolaDestinatario !== "" &&
        matricolaDestinatario !== null &&
        typeof matricolaDestinatario !== "undefined") {
        $('#MatricolaDestinatario').val(matricolaDestinatario);
    }

    var val = $('#tipologiaDocumentale').val();
    val = $.trim(val);
    val = val.toUpperCase();

    var val2 = $('#tipodoc').val();
    val2 = $.trim(val2);
    val2 = val2.toUpperCase();

    var tipologiaDocumentale = val;
    var tipodoc = val2;
    var tipoWKF = $('#div-tipologiaDocumentale').data('tipo');
    var MatricolaDestinatario = $('#MatricolaDestinatario').val();
    var attrs = Dematerializzazione_GetCustomData();

    $.ajax({
        url: "/Dematerializzazione/IsRichiestaGiaPresente",
        type: "POST",
        async: false,
        data: JSON.stringify({
            matricolaDestinatario: MatricolaDestinatario,
            tipologiaDocumentale: tipologiaDocumentale,
            tipodoc: tipodoc,
            tipoWKF: tipoWKF,
            attrs: attrs
        }),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data !== null && data !== "" && data) {
                result = data;
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
    return result;
}

function Dematerializzazione_Btn1BisNext_Click() {

    var isCustomType = $('#IsCustomType').val();
    isCustomType = (isCustomType.toUpperCase() === "TRUE");

    var isFileAggiuntivoObbligatorio = $('#FileAggiuntivoObbligatorio').val();
    isFileAggiuntivoObbligatorio = (isFileAggiuntivoObbligatorio.toUpperCase() === "TRUE");

    if (isCustomType) {
        var formCompilato = Dematerializzazione_CustomData_TuttiCompilati();
        if (!formCompilato) {
            swal({
                title: 'Uno o più campi obbligatori non sono stati valorizzati correttamente',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
            return false;
        }
    }
    // verifica che la richiesta inserita non sia già presente nel sistema dematerializzazione
    var giaPresente = Dematerializzazione_RichiestaGiaPresente();

    if (giaPresente) {
        swal({
            title: 'E\' già presente una richiesta con le stesse informazioni.Si desidera continuare con l\'inserimento di questa richiesta?',
            type: 'question',
            showCancelButton: true,
            confirmButtonText: 'Sì',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            customClass: 'rai rai-confirm-cancel'
        }).then(
            function () {
                // deve continuare l'esecuzione del metodo
                $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');

                if ($("#modal-inserimentoDocDem").is(':visible')) {
                    $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
                }
                $('#tab1-custom-data').hide();
                $('#btns-fase1-custom').hide();

                if (isCustomType) {
                    var checkAsterisco = $('#lb_doc_supporto').text();
                    if (isFileAggiuntivoObbligatorio) {
                        if (checkAsterisco.indexOf("*") === -1) {
                            $('#lb_doc_supporto').append(' <font color="#d2322d">*</font>');
                        }
                    }
                    else {
                        // va rimosso l'asterisco
                    }
                }

                var idDoc = $('#identificativoAllegato').val();
                if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
                    Dematerializzazione_AttivaElementiTab(3);
                    if (!$('#btns-fase3-next').hasClass('disable')) {
                        $('#btns-fase3-next').addClass('disable');
                    }
                    Dematerializzazione_AbilitaButtonFase3();
                }
                else {
                    Dematerializzazione_AttivaElementiTab(2);
                    Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc);
                }
            },
            function () {
                // si ferma qui perchè l'utente non vuole continuare
                return false;
            });
    }
    else {
        $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');

        if ($("#modal-inserimentoDocDem").is(':visible')) {
            $('#modal-inserimentoDocDem-internal').data('isDirty', 'true');
        }
        $('#tab1-custom-data').hide();
        $('#btns-fase1-custom').hide();

        if (isCustomType) {
            var checkAsterisco = $('#lb_doc_supporto').text();
            if (isFileAggiuntivoObbligatorio) {
                if (checkAsterisco.indexOf("*") === -1) {
                    $('#lb_doc_supporto').append(' <font color="#d2322d">*</font>');
                }
            }
            else {
                // va rimosso l'asterisco
            }
        }

        var idDoc = $('#identificativoAllegato').val();
        if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
            Dematerializzazione_AttivaElementiTab(3);
            if (!$('#btns-fase3-next').hasClass('disable')) {
                $('#btns-fase3-next').addClass('disable');
            }
            Dematerializzazione_AbilitaButtonFase3();
        }
        else {
            Dematerializzazione_AttivaElementiTab(2);
            Dematerializzazione_OperazioniPreliminariAttivazioneTab2(idDoc);
        }
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
    //  "selectListItems": nullm
    // "Visible": bool
    // "HideInReadOnly": bool
    //}

    var attrs = new Array();

    $('.dem-customdata').each(function () {
        //debugger;
        var tx = $(this).val();
        tx = $.trim(tx);
        var _myID = "";
        var calcId = "";

        var id = $(this).attr('id');
        if (id !== null && id !== "" && typeof id !== "undefined") {
            var tipo = $(this).attr('type');

            var hideInReadOnly = false;
            var gruppo = "";
            var title = $('#lb_' + id).attr('title');
            if (title == undefined) {
                var idExt = String(id).substring(0, id.indexOf("_"));
                title = $('#lb_' + idExt).attr('title');
            }
            var checked = false;
            var label = $('#lb_' + id).attr('title');
            if (label == undefined) {
                var idExt = String(id).substring(0, id.indexOf("_"));
                label = $('#lb_' + idExt).attr('title');
            }
            var dbRefAttribute = "";
            var visibile = $(this).data('visible');
            var divparent = $(this).data('divparent');
            var posizione = $(this).data('posizione');
            var dataTipo = $(this).data('tipo');

            let text = "";
            let valoreTemp = $(this).val();
            //debugger;
            if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "radio") {
                gruppo = $(this).data('check-group');
                checked = $(this).is(':checked');
                _myID = $(this).data('originalid');
                label = $('#lb_' + _myID).attr('title');
                if (checked) {
                    calcId = $(this).attr('id');
                    title = $('#radio_' + calcId).text();
                    visibile = $('#radio_' + calcId).is(':visible');
                }
            }

            if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "checkbox") {
                checked = $(this).is(':checked');
                _myID = $(this).data('originalid');
                label = $('#lb_' + _myID).attr('title');
                if (checked) {
                    calcId = $(this).attr('id');
                    title = $('#lb_' + calcId).text();
                    visibile = $('#' + calcId).is(':visible');
                }
            }

            if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "checkbox" &&
                $(this).hasClass('task-switch')) {
                checked = $(this).is(':checked');
                _myID = $(this).attr('id');
                label = $('#lb_' + _myID).text();
                text = $('#label_switch_' + _myID).text();
                valoreTemp = text;
            }

            var obj = new Object();
            obj.id = $(this).attr('id');

            if (dataTipo !== null && dataTipo !== "" && typeof dataTipo !== "undefined" && dataTipo === "SelectMultiSelezioneLibera") {
                // select multipla con inserimento libero
                valoreTemp = $(this).getTags();
                text = $(this).getTags();
                obj.id = $(this).data('originalid');
            }

            if (dataTipo === "SelectMultiSelezione" &&
                (valoreTemp !== null && valoreTemp !== "" && valoreTemp !== "undefined") &&
                id.toLocaleLowerCase() == "ufficiodestinatario") {
                Dematerializzazione_SetValue("EccezionePerAutomatismo", valoreTemp);
                Dematerializzazione_SetValue("DestinatarioMail", valoreTemp);
            }

            if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "checkbox" &&
                $(this).hasClass('task-switch')) {
                checked = $(this).is(':checked');
                _myID = $(this).attr('id');
                label = $('#lb_' + _myID).text();
                text = $('#label_switch_' + _myID).text();
                valoreTemp = text;
            }

            if (dataTipo !== null && dataTipo !== "" && typeof dataTipo !== "undefined" && dataTipo === "Select") {
                // select
                valoreTemp = $(this).val();
                text = $(this).val();
                checked = true;
                _myID = $(this).attr('id');
                label = $('#lb_' + _myID).text().replace("*", "");
            }

            dbRefAttribute = $(this).data('dbrefattribute');
            hideInReadOnly = $(this).data('hideinreadonly');

            if (hideInReadOnly === null || hideInReadOnly === "" || typeof hideInReadOnly === "undefined") {
                hideInReadOnly = false;
            }

            if (valoreTemp !== null && valoreTemp !== "" && valoreTemp !== "undefined") {
                if (valoreTemp.constructor === Array) {
                    text = valoreTemp.toString();
                }
                else {
                    text = valoreTemp;
                }
            }
            else {
                text = valoreTemp;
            }

            //obj.valore = $(this).val();
            obj.valore = text;
            obj.tipo = $(this).data('tipo');
            obj.gruppo = gruppo;
            obj.checked = checked;
            obj.label = label;
            obj.title = title;
            obj.DBRefAttribute = dbRefAttribute;
            obj.Visible = visibile;
            obj.DivParent = divparent;
            obj.Ordinamento = posizione;
            obj.HideInReadOnly = hideInReadOnly;

            attrs.push(obj);
        }
    });

    var _tempAttrs = new Array();

    $('div.row-inline-class').each(function () {
        var obj = new Object();

        obj.id = $(this).attr('id');
        obj.valore = "";
        obj.tipo = $(this).data('tipo');
        obj.gruppo = "";
        obj.checked = false;
        obj.label = $(this).data('label');
        obj.title = $(this).data('title');
        obj.DBRefAttribute = null;
        obj.Visible = true;
        obj.Ordinamento = $(this).data('posizione');

        obj.InLine = Array();

        $(this).find('.dem-customdata').each(function () {

            var tx = $(this).val();
            tx = $.trim(tx);

            var id = $(this).attr('id');
            if (id !== null && id !== "" && typeof id !== "undefined") {
                var tipo = $(this).attr('type');

                var gruppo = "";
                var title = "";
                var checked = false;
                var label = $('#lb_' + id).attr('title');
                var dbRefAttribute = "";
                var visibile = $(this).data('visible');

                if (tipo !== null && tipo !== "" && typeof tipo !== "undefined" && tipo === "radio") {
                    gruppo = $(this).data('check-group');
                    checked = $(this).is(':checked');
                    var _myID = $(this).data('originalid');
                    label = $('#lb_' + _myID).attr('title');
                    if (checked) {
                        var calcId = $(this).attr('id');
                        title = $('#radio_' + calcId).text();
                        visibile = $('#radio_' + calcId).is(':visible');
                    }
                }

                dbRefAttribute = $(this).data('dbrefattribute');

                var objLine = new Object();

                objLine.id = $(this).attr('id');
                objLine.valore = $(this).val();
                objLine.tipo = $(this).data('tipo');
                objLine.gruppo = gruppo;
                objLine.checked = checked;
                objLine.label = label;
                objLine.title = title;
                objLine.DBRefAttribute = dbRefAttribute;
                objLine.Visible = visibile;
                objLine.Ordinamento = $(this).data('posizione');

                obj.InLine.push(objLine);
                var filter = new Array();
                $.each(attrs, function (key, value) {
                    if (value.id !== objLine.id) {
                        filter.push(value);
                    }
                });
                attrs = filter;
            }
        });

        _tempAttrs.push(obj);

    });
    var ordinamento = 0;
    $.each(_tempAttrs, function (key, value) {
        if (ordinamento === 0) {
            ordinamento = value.Ordinamento;
        }
        else if (ordinamento === value.Ordinamento) {
            ordinamento = value.Ordinamento + 1;
        }
        else if (ordinamento > value.Ordinamento) {
            ordinamento = ordinamento + 1;
        }
        attrs.splice(ordinamento, 0, value);
    });

    var myJsonString = JSON.stringify(attrs);

    return myJsonString;
}

function Dematerializzazione_AttivaDateSuSelezioneEccezione(id) {
    var eccezione = $('#' + id).val();

    var matricolaDestinatario = $('#selMatricolaDestinatario').val();
    if (matricolaDestinatario !== "" &&
        matricolaDestinatario !== null &&
        typeof matricolaDestinatario !== "undefined") {
        matricola = matricolaDestinatario;
    }
    else {
        matricola = $('#Matricola').val();
    }


    if (eccezione === "MT") {
        $.ajax({
            url: "/Dematerializzazione/AttivaDateSuSelezioneEccezione",
            type: "POST",
            async: false,
            data: JSON.stringify({
                matricola: matricola,
                eccezione: eccezione
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {

                $('div[group-id="row_tipoorario"]').hide();

                var txtTemp = $('#lb_codicefiscalebambino').text();
                txtTemp = txtTemp.replace("*", "");
                $('#lb_codicefiscalebambino').text(txtTemp);
                $('#codicefiscalebambino').removeAttr('required');

                if (data !== null && data !== "") {
                    if (data.presuntoParto === null && data.parto === null) {
                        // allora vedi data presunto parto
                        $('div[id="row_data_presunta_parto"]').show();
                        $('div[id="row_data_nascita"]').hide();
                        $('#data_nascita').val('');
                        $('div[id="row_codicefiscalebambino"]').hide();
                    }
                    else if (data.presuntoParto === null && data.parto !== null) {
                        $('#data_nascita').val(data.parto);
                        $('#calendario_data_nascita').addClass('disable');
                        $('div[id="row_data_presunta_parto"]').hide();
                        $('div[id="row_data_nascita"]').show();
                        $('div[id="row_codicefiscalebambino"]').show();
                    }
                    else if (data.presuntoParto !== null && data.parto !== null) {
                        $('#data_presunta_parto').val(data.presuntoParto);
                        $('#calendario_data_presunta_parto').addClass('disable');
                        $('#data_nascita').val(data.parto);
                        $('#calendario_data_nascita').addClass('disable');
                        $('div[id="row_data_presunta_parto"]').show();
                        $('div[id="row_data_nascita"]').show();
                        $('div[id="row_codicefiscalebambino"]').show();
                    }
                    else if (data.presuntoParto !== null && data.parto === null) {
                        $('#data_presunta_parto').val(data.presuntoParto);
                        $('#calendario_data_presunta_parto').addClass('disable');
                        $('#data_nascita').val('');
                        $('#calendario_data_nascita').removeClass('disable');
                        $('div[id="row_data_presunta_parto"]').show();
                        $('div[id="row_data_nascita"]').show();
                        $('div[id="row_codicefiscalebambino"]').show();
                    }
                }
                else {
                    $('div[id="row_data_presunta_parto"]').show();
                    $('div[id="row_data_parto"]').hide();
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
    }
    else if (eccezione === "AF" || eccezione === "BF" || eccezione === "CF") {
        $('div[id="row_data_nascita"]').hide();
        $('div[id="row_data_presunta_parto"]').hide();
        $('div[id="row_codicefiscalebambino"]').show();
        $('div[group-id="row_tipoorario"]').show();

        var txtTemp = $('#lb_codicefiscalebambino').text();
        txtTemp = txtTemp.replace("*", "");
        $('#lb_codicefiscalebambino').text(txtTemp);
        $('#lb_codicefiscalebambino').append(' <font color="#d2322d">*</font>');
        $('#codicefiscalebambino').removeAttr('required');
        $('#codicefiscalebambino').attr('required', 'required');
    }
}

function Dematerializzazione_AbilitaButtonFase3() {
    var pronto1 = false;
    var pronto2 = false;
    var pronto3 = false;

    if ($('#selApprovatore').is(':visible')) {
        var selApprovatore = $('#selApprovatore').val();

        if (selApprovatore !== null && selApprovatore !== "" && selApprovatore !== "-1" && typeof selApprovatore !== "undefined") {
            pronto1 = true;
        }
    }
    else {
        pronto1 = true;
    }

    if ($('#incaricatoFirma').is(':visible')) {
        var incaricatoFirma = $('#incaricatoFirma').val();

        if (incaricatoFirma !== null && incaricatoFirma !== "" && incaricatoFirma !== "-1" && typeof incaricatoFirma !== "undefined") {
            pronto2 = true;
        }
    }
    else {
        pronto2 = true;
    }

    var tx = $('#lb_doc_supporto').text();

    if (tx.indexOf('*') > -1) {
        var tempAll = '';

        $('tr[id^="riga-allegato-"]').each(function () {
            var id = $(this).data('id');
            if (tempAll === '') {
                tempAll = id;
            }
            else {
                tempAll = tempAll + "," + id;
            }
        });

        if (tempAll !== "") {
            pronto3 = true;
        }
    }
    else {
        pronto3 = true;
    }

    if (pronto1 && pronto2 && pronto3) {
        $('#btns-fase3-next').removeClass('disable');
    }
    else {
        if (!$('#btns-fase3-next').hasClass('disable')) {
            $('#btns-fase3-next').addClass('disable');
        }
    }
}

function Dematerializzazione_SetRequired(id, required) {
    var txtTemp2 = $('#lb_' + id).text();
    txtTemp2 = txtTemp2.replace("*", "");
    $('#lb_' + id).text(txtTemp2);

    if (required) {
        $('#lb_' + id).append(' <font color="#d2322d">*</font>');
        $('#' + id).removeAttr('required');
        $('#' + id).attr('required', 'required');
    }
    else {
        $('#' + id).removeAttr('required');
    }
}

function Dematerializzazione_SetRequiredStartWith(id, required) {
    $('label[id^="lb_' + id + '"]').each(function () {
        var txtTemp2 = $(this).text();
        txtTemp2 = txtTemp2.replace("*", "");
        $(this).text(txtTemp2);
    });

    if (required) {
        $('label[id^="lb_' + id + '"]').each(function () {
            $(this).append(' <font color="#d2322d">*</font>');
        });

        $('#' + id + '*').each(function () {
            $(this).removeAttr('required');
            $(this).attr('required', 'required');
        });
    }
    else {
        $('#' + id + '*').each(function () {
            $(this).removeAttr('required');
        });
    }
}

function Dematerializzazione_HideOrShowElement(id, show) {
    var selettore = 'div[id="row_' + id + '"]';

    if ($(selettore).length === 0) {
        selettore = 'div[group-id="row_' + id + '"]';

        if ($(selettore).length === 0) {
            selettore = '#' + id;
        }
    }

    if (show) {
        $(selettore).show();
    }
    else {
        $(selettore).hide();
    }
}

function Dematerializzazione_HideOrShowElementStartWith(id, show, skipGuid) {
    var selettore = 'div[id^="row_' + id + '"]';

    if ($(selettore).length === 0) {
        selettore = 'div[id^="' + id + '"]';

        if ($(selettore).length === 0) {
            selettore = 'div[group-id^="row_' + id + '"]';

            if ($(selettore).length === 0) {
                selettore = '#' + id;
            }
        }
    }

    if (show) {
        $(selettore).each(function () {
            $(this).show();
        });
    }
    else {
        $(selettore).each(function () {
            $(this).hide();
        });
    }
}

function Dematerializzazione_SetDisabled(id, disable) {
    var selettore = "#" + id;

    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {

        /*
            Data = 0,
            Testo = 1,
            Numero = 2,
            Select = 3,
            Nota = 4,
            Check = 5,
            Radio = 6,
            Importo = 7,
            FixedHiddenValue = 8
         */

        tipologia = parseInt(tipologia, 10);

        if (tipologia === 0) {
            selettore = "#calendario_" + id;
        }

        if (tipologia === 6) {
            selettore = 'input[data-originalid="' + id + '"]';
        }
    }

    if (disable) {
        if (!$(selettore).hasClass('disable')) {
            $(selettore).addClass('disable');
        }
    }
    else {
        $(selettore).removeClass('disable');
    }
}

function Dematerializzazione_SetValue(id, value) {
    var selettore = "#" + id;
    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {
        tipologia = parseInt(tipologia, 10);
    }

    if (id.indexOf("radio_") > -1) {
        $(selettore).text(value);
    }
    else {
        $(selettore).val(value);
    }
}

function Dematerializzazione_GetValue(id) {
    var selettore = "#" + id;
    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {
        tipologia = parseInt(tipologia, 10);
        //if (tipologia === 6) {
        //    selettore = 'input[data-originalid="' + id + '"]';
        //}
    }
    return $(selettore).val();
}

function Dematerializzazione_GetRadioCheckedValue(id) {
    var selettore = "#" + id;
    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {
        tipologia = parseInt(tipologia, 10);

        if (tipologia === 6) {
            selettore = 'input[data-originalid="' + id + '"]:checked';
        }
    }
    return $(selettore).val();
}

function Dematerializzazione_SetRadioChecked(id, value) {
    var selettore = "#" + id;
    $(selettore).prop('checked', value);
}

function Dematerializzazione_SetDataValue(id, dataName, value) {
    var selettore = "#" + id;
    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {
        tipologia = parseInt(tipologia, 10);
    }
    $(selettore).data(dataName, value);
}

function Dematerializzazione_GetDataValue(id, dataName) {
    var selettore = "#" + id;
    // prima di tutto deve capire con quale tipo di elemento sta lavorando
    var tipologia = $('#' + id).data('tipo');
    if (tipologia !== null &&
        tipologia !== "" &&
        typeof tipologia !== "undefined") {
        tipologia = parseInt(tipologia, 10);
    }

    var value = $(selettore).data(dataName);

    return value;
}

function Dematerializzazione_AbilitaDivAttributiAggiuntiWKF() {
    if (!$('#dematerializzazione_div_AttributiAggiuntiWKF').is(':visible')) {
        $('#dematerializzazione_div_AttributiAggiuntiWKF').show();
        $('#btn_aggiunta_json').hide();
        $('#btn_annulla_aggiunta_json').show();
    }
    else {
        $('#dematerializzazione_div_AttributiAggiuntiWKF').hide();
        $('#btn_aggiunta_json').show();
        $('#btn_annulla_aggiunta_json').hide();
    }
}

function Dematerializzazione_SalvaAttributiAggiuntiWKF(matricola, idPersona, idDoc, approvazioneEnabled, presaInCaricoEnabled, presaInVisioneEnabled) {
    idPersona = parseInt(idPersona, 10);
    idDoc = parseInt(idDoc, 10);
    swal({
        title: 'Sicuro di voler procedere col salvataggio dei dati?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        // salvataggio dei dati
        var attrs = Dematerializzazione_GetCustomData();

        $.ajax({
            url: "/Dematerializzazione/UpdateDocumento_DatiAggiuntiviWKF",
            type: "POST",
            data: JSON.stringify({
                matricola: matricola,
                idPersona: idPersona,
                idDoc: idDoc,
                customAttrs: attrs
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data.result === "OK") {
                    swal({
                        title: 'Operazione eseguita correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    }).then(function () {
                        // ricarica i dati nella finestra
                        if (approvazioneEnabled.toUpperCase() === "TRUE") {
                            approvazioneEnabled = true;
                        }
                        else {
                            approvazioneEnabled = false;
                        }

                        if (presaInCaricoEnabled.toUpperCase() === "TRUE") {
                            presaInCaricoEnabled = true;
                        }
                        else {
                            presaInCaricoEnabled = false;
                        }

                        if (presaInVisioneEnabled.toUpperCase() === "TRUE") {
                            presaInVisioneEnabled = true;
                        }
                        else {
                            presaInVisioneEnabled = false;
                        }
                        $.ajax({
                            url: "/Dematerializzazione/RefreshGetDettaglioRichiesta",
                            type: "GET",
                            data: {
                                matricola: matricola,
                                idPersona: idPersona,
                                idDoc: idDoc,
                                approvatoreEnabled: approvazioneEnabled,
                                presaInCaricoEnabled: presaInCaricoEnabled,
                                presaInVisioneEnabled: presaInVisioneEnabled
                            },
                            contentType: "application/json; charset=utf-8",
                            dataType: 'html',
                            success: function (data) {
                                $('#dematerializzazione_div_contenitoreAttributi_Aggiuntivi').html(data);
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
                else if (data.result === "KO") {
                    swal({
                        title: data.infoAggiuntive,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
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
            },
            complete: function () {
            }
        });
    });
}

function Dematerializzazione_AbilitaModificaDivAttributiAggiuntiWKF(matricola, idPersona, idDoc, approvazioneEnabled, presaInCaricoEnabled, presaInVisioneEnabled) {
    idPersona = parseInt(idPersona, 10);
    idDoc = parseInt(idDoc, 10);
    if (approvazioneEnabled.toUpperCase() === "TRUE") {
        approvazioneEnabled = true;
    }
    else {
        approvazioneEnabled = false;
    }

    if (presaInCaricoEnabled.toUpperCase() === "TRUE") {
        presaInCaricoEnabled = true;
    }
    else {
        presaInCaricoEnabled = false;
    }

    if (presaInVisioneEnabled.toUpperCase() === "TRUE") {
        presaInVisioneEnabled = true;
    }
    else {
        presaInVisioneEnabled = false;
    }

    $.ajax({
        url: "/Dematerializzazione/ModificaDatiAggiuntiviDettaglioRichiesta",
        type: "GET",
        data: {
            matricola: matricola,
            idPersona: idPersona,
            idDoc: idDoc,
            approvatoreEnabled: approvazioneEnabled,
            presaInCaricoEnabled: presaInCaricoEnabled,
            presaInVisioneEnabled: presaInVisioneEnabled
        },
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
            $('#dematerializzazione_div_contenitoreAttributi_Aggiuntivi').html(data);
            $('#dematerializzazione_div_AttributiAggiuntiWKF').show();
            $('#btn_modifica_json').hide();
            $('#btn_annulla_modifica_json').show();
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
}

function Dematerializzazione_AnnullaModificaDivAttributiAggiuntiWKF(matricola, idPersona, idDoc, approvazioneEnabled, presaInCaricoEnabled, presaInVisioneEnabled) {
    idPersona = parseInt(idPersona, 10);
    idDoc = parseInt(idDoc, 10);
    if (approvazioneEnabled.toUpperCase() === "TRUE") {
        approvazioneEnabled = true;
    }
    else {
        approvazioneEnabled = false;
    }

    if (presaInCaricoEnabled.toUpperCase() === "TRUE") {
        presaInCaricoEnabled = true;
    }
    else {
        presaInCaricoEnabled = false;
    }

    if (presaInVisioneEnabled.toUpperCase() === "TRUE") {
        presaInVisioneEnabled = true;
    }
    else {
        presaInVisioneEnabled = false;
    }
    $.ajax({
        url: "/Dematerializzazione/RefreshGetDettaglioRichiesta",
        type: "GET",
        data: {
            matricola: matricola,
            idPersona: idPersona,
            idDoc: idDoc,
            approvatoreEnabled: approvazioneEnabled,
            presaInCaricoEnabled: presaInCaricoEnabled,
            presaInVisioneEnabled: presaInVisioneEnabled
        },
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
            $('#dematerializzazione_div_contenitoreAttributi_Aggiuntivi').html(data);
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
}

function Dematerializzazione_DuplicaPratica(matricola, idPersona, idDoc) {
    swal({
        title: 'Sei sicuro di voler duplicare la pratica?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, duplica',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/DuplicaPratica",
            type: "GET",
            async: false,
            data: {
                idDoc: idDoc
            },
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data.Esito) {
                    Dematerializzazione_ReloadTabellaDocumentiOperatore();
                    Dematerializzazione_ApriInModificaIlDocumento(data.Id);
                }
                else {
                    swal({
                        title: data.ErrorMessage,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
            },
            error: function (xhr, status) {
                swal({
                    title: 'Si è verificato un errore durante la duplicazione del documento',
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        });
    });
}

function Dematerializzazione_ricerca_pratiche() {
    debugger;
    var data = $('#datadal').val();
    if (data != 0) {
        const myArray = data.split("/");
        var mydate = new Date(myArray[1] + "-" + myArray[0] + "-" + "1");
    } else {
        var mydate = new Date("1900-1-1");
    }

    var nominativo = $('#Filtri_Nominativo').val();
    var matricola = $('#Filtri_MatricolaONominativo').val();
    var oggetto = $('#Filtri_Oggetto').val();
    var id_Tipo_Doc = $('#tipodocRicerca').val();

    if (id_Tipo_Doc === "-1")
        id_Tipo_Doc = null;

    RaiUpdateWidget('div_elenco_documenti', '/Dematerializzazione/GetContentInternal', 'html', { approvazioneEnabled: true, nominativo: nominativo, oggetto: oggetto, matricola: matricola, id_Tipo_Doc: id_Tipo_Doc, datadal: mydate.toJSON() }, false, null, false, 'GET');

    RaiUpdateWidget('content-container-operatore', '/Dematerializzazione/GetContentOperatore', 'html', { nominativo: nominativo, oggetto: oggetto, matricola: matricola, id_Tipo_Doc: id_Tipo_Doc, datadal: mydate.toJSON() }, false, null, false, 'GET');
}

function Dematerializzazione_PortaAlloStatoPrecedentePratica(idDoc) {

    swal({
        title: 'Sei sicuro di voler ripristinare lo stato precedente della pratica selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, continua',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/PortaPraticaStatoPrecedente",
            type: "GET",
            async: false,
            data: {
                idDoc: idDoc
            },
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data.Esito) {
                    swal({
                        title: 'Lo stato della pratica è stato ripristinato correttamente',
                        type: 'info',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                    $('#modal-dettaglio-richiesta').modal('hide');
                    $('#modal-dettaglio-richiesta-internal').html('');
                    Dematerializzazione_ricerca_pratiche();
                }
                else {
                    swal({
                        title: data.ErrorMessage,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
            },
            error: function (xhr, status) {
                swal({
                    title: 'Si è verificato un errore durante il ripristino dello stato precedente della pratica',
                    type: 'error',
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        });
    });
}

function Dematerializzazione_ExecOnChange(id) {
    $('#' + id).change();
}

function Dematerializzazione_ExecClick(id) {
    $('#' + id).click();
}

function Dematerializzazione_EseguiTuttiOnChange() {
    $('.dem-customdata').each(function () {
        if ($(this).is(':visible')) {
            if ($(this).attr('type') === 'radio') {
                var valore2 = Dematerializzazione_GetRadioCheckedValue($(this).attr('data-originalid'));
                if (valore2 !== null && valore2 !== "" && typeof (valore2) !== "undefined")
                    Dematerializzazione_MarkCompilato($(this).attr('id'));
            }
            else {
                var valore = Dematerializzazione_GetValue($(this).attr('id'));

                if (valore !== null && valore !== "" && typeof (valore) !== "undefined")
                    Dematerializzazione_MarkCompilato($(this).attr('id'));
            }
        }
    });
}

function Dematerializzazione_CheckedAutomatico() {
    $('.dem-customdata').each(function () {
        var id = $(this).attr('id');
        id = '#' + id;
        if ($(this).is(':visible')) {
            if ($(this).attr('type') === 'radio') {
                if ($(id).is(':checked')) {
                    $(this).change();
                }
            }
            else {
                if ($(id).is(':checked')) {
                    $(this).change();
                }
            }
        }
    });
}

function Dematerializzazione_ChangeIfChecked(id) {
    var selettore = "#" + id;

    if ($(selettore).is(':checked')) {
        $(selettore).change();
    }


    //// prima di tutto deve capire con quale tipo di elemento sta lavorando
    //var tipologia = $('#' + id).data('tipo');
    //if (tipologia !== null &&
    //    tipologia !== "" &&
    //    typeof tipologia !== "undefined") {
    //    tipologia = parseInt(tipologia, 10);

    //    if (tipologia === 6) {
    //        let posizioneUndescore = id.indexOf('_');
    //        posizioneUndescore = parseInt(posizioneUndescore, 10);

    //        let result = id.substring(0, posizioneUndescore);
    //        selettore = 'input[data-originalid="' + result + '"]:checked';
    //    }
    //}
    //$(selettore).change();
    return true;
}

function Dematerializzazione_EliminaRaw(caller, callback) {
    var target = $(caller).data('rowparent');
    $('#' + target).remove();
    var pos = -1;
    var guid = '';

    if (target.indexOf('_GUID') > -1) {
        // se contiene _GUID allora è un elemento clonato
        pos = target.indexOf('_GUID');
        if (pos > -1) {
            guid = target.substring(pos);
            target = target.substring(0, pos);
        }
    }

    var righe = $('div[id^="' + target + '"]').length;
    if (righe === 1) {

        // prende l'id della riga
        var temp = $('div[id^="' + target + '"]').prop('id');

        // se contiene una guid allora va calcolata la guid
        if (temp.indexOf('_GUID') > -1) {
            pos = temp.indexOf('_GUID');
            if (pos > -1) {
                guid = temp.substring(pos);
            }
            else {
                guid = '';
            }
        } else {
            guid = '';
        }

        var btnRemove = $(caller).data('btn-rimuovi');
        // a questo punto selettore avrà nome bottone rimuovi + eventuale guid
        var selettore = btnRemove + guid;

        $('#' + selettore).hide();
    }

    if (callback !== null && typeof callback !== "undefined") {
        eval(callback(caller));
    }
}

function Dematerializzazione_RiattivaBtnAggiungi(caller) {
    var target = $(caller).data('rowparent');
    var guid = '';
    if (target.indexOf('_GUID') > -1) {
        // se contiene _GUID allora è un elemento clonato
        var pos = target.indexOf('_GUID');
        if (pos > -1) {
            // prende il nome del parent della prima raw
            guid = target.substring(pos);
            target = target.substring(0, pos);
        }
    }

    var righe = $('div[id^="' + target + '"]').length;
    var desTarget = $(caller).data('firstbtn');

    if (righe === 1) {
        if (desTarget !== null && desTarget !== "" && typeof desTarget !== "undefined") {
            $('#' + desTarget).show();
        } else {
            // prende l'id della riga
            var temp = $('div[id^="' + target + '"]').prop('id');
            // se contiene una guid allora va calcolata la guid
            if (temp.indexOf('_GUID') > -1) {
                pos = temp.indexOf('_GUID');
                if (pos > -1) {
                    guid = temp.substring(pos);
                }
                else {
                    guid = '';
                }
            } else {
                guid = '';
            }

            var btnAdd = $(caller).data('btn-aggiungi');
            // a questo punto selettore avrà nome bottone aggiungi + eventuale guid
            var selettore = btnAdd + guid;
            $('#' + selettore).show();
        }
    }
    else {
        if (desTarget !== null && desTarget !== "" && typeof desTarget !== "undefined") {
            $('button[id^="' + desTarget + '"]:last').show();
        } else {
            var btn_aggiungi = $(caller).data('btn-aggiungi');
            $('button[id^="' + btn_aggiungi + '"]:last').show();
        }
    }
}

function Dematerializzazione_checkCodiceFiscale(caller) {
    //debugger;
    document.getElementById(caller.context.id).value = caller.context.value.trim();
    var cf = document.getElementById(caller.context.id).value; //caller.val();

    var validi, i, s, set1, set2, setpari, setdisp;
    if (cf == '') return '';
    cf = cf.toUpperCase();
    if (cf.length != 16) {
        swal({
            title: 'Codice Fiscale non corretto',
            type: 'warning',
            buttons: {
                cancel: true,
                confirm: true,
            }
        });
        caller.removeClass("alert-success");
        caller.addClass("alert-danger");
    }
    validi = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    for (i = 0; i < 16; i++) {
        if (validi.indexOf(cf.charAt(i)) == -1) {
            swal({
                title: 'Codice Fiscale non corretto',
                type: 'warning',
                buttons: {
                    cancel: true,
                    confirm: true,
                }
            });
            caller.removeClass("alert-success");
            caller.addClass("alert-danger");
        }
    }
    set1 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    set2 = "ABCDEFGHIJABCDEFGHIJKLMNOPQRSTUVWXYZ";
    setpari = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    setdisp = "BAKPLCQDREVOSFTGUHMINJWZYX";
    s = 0;
    for (i = 1; i <= 13; i += 2) {
        s += setpari.indexOf(set2.charAt(set1.indexOf(cf.charAt(i))));
    }
    for (i = 0; i <= 14; i += 2) {
        s += setdisp.indexOf(set2.charAt(set1.indexOf(cf.charAt(i))));
    }

    caller.css('display', 'block');
    if (s % 26 != cf.charCodeAt(15) - 'A'.charCodeAt(0)) {
        swal({
            title: 'Codice Fiscale non corretto',
            type: 'warning',
            buttons: {
                cancel: true,
                confirm: true,
            }//,
            //showConfirmButton: true,
            //confirmButtonText: 'Ok',
            //customClass: 'rai'
        });
        caller.removeClass("alert-success");
        caller.addClass("alert-danger");
    }
    else {
        caller.removeClass("alert-danger");
        caller.addClass("alert-success");
    }
    return true;
}

function Dematerializzazione_ClonaRawUsingController(caller) {
    var matricola = "";
    var scelta = $('#selMatricolaDestinatario').val();
    if (scelta !== null && scelta !== "") {
        matricola = scelta;
    }

    var target = $(caller).data('rowparent');
    var tuttiValorizzati = true;
    // cerca in tutti i figli di target la classe .dem-customdata per verificare che i required siano tutti valorizzati
    $('#' + target).find('.dem-customdata').filter('[required="required"]:visible').each(function () {
        var id = $(this).attr('id');
        var _resultInternal = $(this).data('compilato');
        if (_resultInternal === null || _resultInternal === "" || typeof _resultInternal === "undefined" || _resultInternal === "false" || _resultInternal === false) {
            tuttiValorizzati = false;
            return false;
        }
    });

    if (!tuttiValorizzati) {
        return false;
    }

    var rowToClone = $(caller).data('rowparent');
    $.ajax({
        url: "/Dematerializzazione/ClonaElementoCustom",
        type: "GET",
        async: false,
        data: {
            matricola: matricola,
            idToClone: target
        },
        success: function (data) {
            var selettore = "#" + rowToClone;
            var $div = $(selettore);
            var id = $(caller).prop('id');
            $('#' + id).hide();
            var btn_rimuovi_id = $(caller).data('btn-rimuovi');
            var btn_rimuovi = '';
            var guid = '';

            if (id.indexOf('_GUID') > -1) {
                // se contiene _GUID allora è stato premuto il bottone + clonato
                var pos = id.indexOf('_GUID');
                if (pos > -1) {
                    // copia la guid
                    guid = id.substring(pos);
                }
            }

            if (btn_rimuovi_id === null ||
                btn_rimuovi_id === "" ||
                typeof btn_rimuovi_id === "undefined") {
                // prende il bottone rimuovi col lo stesso guid e lo mostra a video
                btn_rimuovi = '#btn_rimuovi' + guid;
            }
            else {
                // prende il bottone rimuovi col lo stesso guid e lo mostra a video
                btn_rimuovi = '#' + btn_rimuovi_id + guid;
            }
            $(btn_rimuovi).show();

            $div.after(data);
        },
        error: function (xhr, status) {
            swal({
                title: 'Si è verificato un errore durante la creazione del nuovo elemento',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function Dematerializzazione_DownloadExport() {
    var link = document.createElement('a');
    link.href = "/Dematerializzazione/ScaricaExport";
    document.body.appendChild(link);
    link.click();
    link.remove();
}

function Dematerializzazione_FiltaDocumentiInArrivo() {
    var data = $('#datamese').val();
    var matricola = $('#Filtri_MatricolaONominativo').val();
    var sede = $('#sede').val();
    var tipologia = $('#tipodoc').val();
    var statoRichiesta = $('#statorichiesta').val();

    if (tipologia === "-1")
        tipologia = null;

    if (statoRichiesta === "-1")
        statoRichiesta = null;

    RaiUpdateWidget('div-dem-documenti', '/Dematerializzazione/FiltaDocumentiInArrivo', 'html', { mese: data, utente: matricola, sede: sede, tipologia: tipologia, statoRichiesta: statoRichiesta }, false, function () {
        $('#li_btab1').removeClass("active");
        $('#btab1').removeClass("active");
        $('#li_btab2').addClass("active");
        $('#btab2').addClass("active");
        $('#btab2').click();
    }, false, 'GET');
}

function Dematerializzazione_AbilitaSeCompilati() {
    var result = Dematerializzazione_IsCompilati();

    if (result) {
        $('#btns-fase1-custom-next').removeClass('disable');
    }
    else {
        // conta quanti elementi sono con la classe .dem-custom e sono required
        var elementiCustomRequiredVisibili = $('.dem-customdata').filter('[required="required"]:visible').length;

        // se non ci sono elementi required allora il bottone "avanti" deve essere abilitato
        if (elementiCustomRequiredVisibili === 0) {
            $('#btns-fase1-custom-next').removeClass('disable');
        }
        else if (!$('#btns-fase1-custom-next').hasClass('disable')) {
            $('#btns-fase1-custom-next').addClass('disable');
        }
    }
}

function Dematerializzazione_IsCompilati() {
    var result = false;
    $('.dem-customdata').filter('[required="required"]:visible').each(function () {
        try {
            var compilato = $(this).context.attributes["compilato"].value;
            if (compilato === "false") {
                result = false;
                return false;
            }
        } catch (e) {
            //var id = $(this).attr('id');
            var _resultInternal = $(this).data('compilato');
            if (_resultInternal === null || _resultInternal === "" || typeof _resultInternal === "undefined" || _resultInternal === "false" || _resultInternal === false) {
                result = false;
                return false;
            }
        }

        result = true;
    });

    return result;
}

function Dematerializzazione_MarkCompilato(id) {
    var tipo = $('#' + id).data('tipo');
    if (tipo === "6" || tipo === 6) {
        var nome = $('#' + id).attr('name');
        $('input[type="radio"][name="' + nome + '"]').each(function () {
            $(this).data('compilato', 'true');
        });
    }
    else {
        var tx = $('#' + id).val();
        tx = $.trim(tx);

        if (tx.length >= 1) {
            $('#' + id).data('compilato', 'true');
        }
        else {
            $('#' + id).data('compilato', 'false');
        }
    }

    Dematerializzazione_AbilitaSeCompilati();
}

function Dematerializzazione_DisabilitaPrimoBottoneAddSePiuDiUno(caller, idBottone) {
    var bottoni = $('button[id^="' + idBottone + '"]:visible').length;
    var nBottoni = parseInt(bottoni, 10);
    if (nBottoni > 1) {
        $('button[id^="' + idBottone + '"]:visible:not(:last)').hide();
    }
}

// @param {any} args
// Il metodo permette di nascondere o visualizzare bottoni
// creati dinamicamente dal JSON
// il parametro args è un array composto da [nome bottone]-opzione-azione separato da ,
// 
// nome bottone è l'id del bottone da cercare nel dom
// opzione può assumere 3 valori: STARTWITH, ENDWITH, EXACT
// le azioni possibili sono:
// HIDEFIRST, HIDELAST,SHOWFIRST,SHOWLAST, HIDEALL,SHOWALL
// le azioni possono essere più di una e separate da pipe
// nel caso di più azioni l'ordine di esecuzione sarà dettato dall'ordine
// delle azioni stesse es: HIDEALL|SHOWLAST
// prima nasconderà tutti gli elementi 
// 
// es uso args = ['btn_aggiungi_dt-STARTWITH-HIDEALL|SHOWLAST','btn_rimuovi_dt-STARTWITH-SHOWALL']
function Dematerializzazione_ResettaBottoniAzione(args) {
    if (args === null || args === "" || typeof args === "undefined" || args.length === 0) {
        return false;
    }
    $.each(args, function (index, val) {
        console.log(index, val);
        try {
            //btn_aggiungi_dt-STARTWITH-HIDEALL|SHOWLAST
            //btn_rimuovi_dt-STARTWITH-SHOWALL
            var splittedCommandS = val.split('-');
            try {
                var splitID = splittedCommandS[0];
                var splitOpt = splittedCommandS[1];
                var splittendActions = splittedCommandS[2];
                var arrayActions = splittendActions.split('|');

                // costruzione selettore
                // STARTWITH, ENDWITH, EXACT
                var selector = null;
                if (splitOpt === "STARTWITH") {
                    selector = $('[id^="' + splitID + '"]');
                } else if (splitOpt === "ENDWITH") {
                    selector = $('[id$="' + splitID + '"]');
                } else if (splitOpt === "EXACT") {
                    selector = $('#' + splitID);
                }

                if (selector === null || selector === "" || typeof selector === "undefined") {
                    console.log("selettore non valido");
                    return false;
                }

                $.each(arrayActions, function (j, v) {
                    console.log(j, v);
                    // HIDEFIRST, HIDELAST,SHOWFIRST,SHOWLAST, HIDEALL,SHOWALL
                    if (v === "HIDEFIRST") {
                        $(selector).first().hide();
                    } else if (v === "HIDELAST") {
                        $(selector).last().hide();
                    } else if (v === "SHOWFIRST") {
                        $(selector).first().show();
                    } else if (v === "SHOWLAST") {
                        $(selector).last().show();
                    } else if (v === "HIDEALL") {
                        $(selector).hide();
                    } else if (v === "SHOWALL") {
                        $(selector).show();
                    }
                });
            } catch (err1) {
                console.log(err1);
            } finally {
                // da valutare
            }
        } catch (err) {
            console.log(err);
        } finally {
            // da valutare
        }
    });
}

function Dematerializzazione_FiltraSedi(selectId, matricola) {
    if (matricola === "null" || typeof matricola === "undefined" || matricola === null) {
        matricola = null;
    }
    selectId = _RaiSelectIdentifier(selectId);

    $('input[data-search$="' + selectId + '"]').attr('data-search-previous', '');

    if (matricola === null) {
        $('input[data-search$="' + selectId + '"]').attr('data-search-func-param', 'Dematerializzazione_SetParametroSelect(null)');
    }
    else {
        $('input[data-search$="' + selectId + '"]').attr('data-search-func-param', 'Dematerializzazione_SetParametroSelect(\'' + matricola + '\')');
    }
}

function Dematerializzazione_OnChangeServizio($this) {
    Dematerializzazione_MarkCompilato('servizio');
    Dematerializzazione_SetDisabled('sezione', false);

    var myValue = Dematerializzazione_GetValue($this);
    Dematerializzazione_FiltraSezioni('sezione', myValue);
}

function Dematerializzazione_SetParametroSelect(parametro) {
    if (parametro === "null" || typeof parametro === "undefined" || parametro === null) {
        parametro = null;
    }

    if (parametro !== null) {
        return {
            parametro: parametro
        };
    }
    else {
        return {
            parametro: null
        };
    }
}

function Dematerializzazione_FiltraSezioni(selectId, parametro) {
    if (parametro === "null" || typeof parametro === "undefined" || parametro === null) {
        parametro = null;
    }
    selectId = _RaiSelectIdentifier(selectId);

    $('input[data-search$="' + selectId + '"]').attr('data-search-previous', '');

    if (parametro === null) {
        $('input[data-search$="' + selectId + '"]').attr('data-search-func-param', 'Dematerializzazione_SetParametroSelect(null)');
    }
    else {
        $('input[data-search$="' + selectId + '"]').attr('data-search-func-param', 'Dematerializzazione_SetParametroSelect(\'' + parametro + '\')');
    }
}

function Dematerializzazione_RicalcoloElementiDaAbilitare(tipoDocumento, tipologiaDocumentale, selezione) {
    var matricola = "";
    var scelta = $('#selMatricolaDestinatario').val();
    if (scelta !== null && scelta !== "" && typeof scelta !== "undefined") {
        matricola = scelta;
    }
    else {
        matricola = $('#MatricolaDestinatario').val();
    }
    $.ajax({
        url: "/Dematerializzazione/RicalcoloElementiDaAbilitare",
        type: "POST",
        async: false,
        data: JSON.stringify({
            tipoDocumento: tipoDocumento,
            tipologiaDocumentale: tipologiaDocumentale,
            selezione: selezione,
            matricolaEsaminata: matricola
        }),
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data !== null && data !== "" && data) {
                if (data.Messaggio !== "" && data.Messaggio !== null) {
                    //swal({
                    //    title: data.Messaggio,
                    //    type: 'info',
                    //    showConfirmButton: true,
                    //    confirmButtonText: 'Ok',
                    //    customClass: 'rai'
                    //});

                    swal({
                        title: '',
                        type: 'info',
                        html: data.Messaggio
                    });
                }

                if (data.RicaricaVista) {
                    Dematerializzazione_AjaxRiCaricaCustomData(matricola, true, data.Elementi);
                }
                else {
                    $.each(data.Elementi, function (key, element) {
                        let id = element.NomeElemento;
                        let visibile = element.Visibile;
                        let campoObbligatorio = element.CampoObbligatorio;
                        let valoreDefault = element.ValoreDefault;
                        let selezionato = element.Selezionato;

                        Dematerializzazione_SetValue(id, valoreDefault);
                        Dematerializzazione_SetRequired(id, campoObbligatorio);
                        Dematerializzazione_HideOrShowElement(id, visibile);

                        if (selezionato) {
                            $('#' + id).attr('checked', 'checked');
                        }
                        else {
                            $('#' + id).removeAttr('checked');
                        }

                        if (id === "sede") {
                            DEM_RaiSelectExtLoadAsyncData(
                                valoreDefault,
                                'sede',
                                '/Dematerializzazione/GetSediSelezionabili', {
                                filter: '',
                                value: valoreDefault,
                                setSelected: true
                            },
                                false,
                                function () {
                                    DEM_RaiSelectOption(valoreDefault, 'sede', false, true);
                                    Dematerializzazione_SetRequired('sede', campoObbligatorio);
                                });
                        }

                        if (id === "servizio") {
                            DEM_RaiSelectExtLoadAsyncData(
                                valoreDefault,
                                'servizio',
                                '/Dematerializzazione/GetServiziSelezionabili', {
                                filter: '',
                                value: valoreDefault,
                                setSelected: true
                            },
                                false,
                                function () {
                                    DEM_RaiSelectOption(valoreDefault, 'servizio', false, true);
                                    Dematerializzazione_SetRequired('servizio', campoObbligatorio);
                                });
                        }

                        if (id === "sezione") {
                            DEM_RaiSelectExtLoadAsyncData(
                                valoreDefault,
                                'sezione',
                                '/Dematerializzazione/GetSezioniSelezionabili', {
                                filter: '',
                                value: valoreDefault,
                                setSelected: true
                            },
                                false,
                                function () {
                                    DEM_RaiSelectOption(valoreDefault, 'sezione', false, true);
                                    Dematerializzazione_SetRequired('sezione', campoObbligatorio);
                                });
                        }
                    });
                }
            }
        },
        error: function (xhr, status) {
            swal({
                title: 'Si è verificato un errore in fase di caricamento delle impostazioni base',
                type: 'error',
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });

    function Dematerializzazione_AjaxRiCaricaCustomData(mt, alternativo, data) {
        $.ajax({
            url: '/Dematerializzazione/CaricaCustomData',
            type: "POST",
            data: JSON.stringify({
                matricola: mt,
                alternativo: alternativo,
                elementiDefault: data
            }),
            async: false,
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'html',
            success: function (data) {
                $('#tab1').hide();
                $('#tab1-custom-data').html(data);
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
}

function Dematerializzazione_AbilitaUploadAllegato() {
    var totalFiles = document.getElementById("fileupload-allegato").files.length;
    var formData = new FormData();

    for (var i = 0; i < totalFiles; i++) {
        var file = document.getElementById("fileupload-allegato").files[i];
        var nomefile = file.name;

        if (nomefile.toLowerCase().indexOf(".pdf") < 0) {
            swal("Formato file non supportato. Sono ammessi soltanto .pdf");
            return;
        }

        formData.append('filePrincipale', false);
        formData.append('tipologiaFile', 2); // questo tipo file indica che è un allegato e non file a supporto
        formData.append('file', file);
        formData.append("nome", nomefile);
        formData.append("tipo", 'PDF');
    }

    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        if (request.readyState === 4 && request.status === 200) {
            var data = $.parseJSON(request.responseText);
            if (data.success) {
                Dematerializzazione_TempUpload_Allegati_OnSuccess(data, 1);
                $('#fileupload-allegato').val('');
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

function Dematerializzazione_TempUpload_Allegati_OnSuccess(response, _tipologiaDocumento) {
    $.ajax({
        url: '/Dematerializzazione/DrawTRFile',
        type: "GET",
        data: {
            idAllegato: response.responseText,
            isPrincipal: false,
            tipologiaDocumento: _tipologiaDocumento,
            tipologiaFile: 2
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () { },
        success: function (data) {
            $('#table-uploading-allegati-body').append(data);
        }
    });
}

function Dematerializzazione_AbilitaDisabilita_btn_dem_RimandaVsOperatore() {
    var txt = $('#Richiesta_Documento_NotaSegreteria').val();

    if (txt === null || txt === "" || $.trim(txt) === "") {
        if (!$('#btn_dem_RimandaVsOperatore').hasClass('disable')) {
            $('#btn_dem_RimandaVsOperatore').addClass('disable');
        }

        return false;
    }

    if (txt.length > 3) {
        $('#btn_dem_RimandaVsOperatore').removeClass('disable');
    }
    else {
        if (!$('#btn_dem_RimandaVsOperatore').hasClass('disable')) {
            $('#btn_dem_RimandaVsOperatore').addClass('disable');
        }
    }
}

function Dematerializzazione_RimandaVsOperatore(iddocumento) {
    var nota = $('#Richiesta_Documento_NotaSegreteria').val();
    if (nota === "" || nota.length === 0) {
        swal({
            title: 'La nota è obbligatoria',
            type: 'error',
            showConfirmButton: true,
            confirmButtonText: 'Ok',
            customClass: 'rai'
        });
        $('#Richiesta_Documento_NotaSegreteria').focus();
        return false;
    }

    swal({
        title: 'Sei sicuro di voler portare la pratica allo stato precedente?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì',
        cancelButtonText: 'No',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Dematerializzazione/RimandaVsOperatore",
            type: "POST",
            data: JSON.stringify({
                idDoc: iddocumento,
                nota: nota
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data.result === "OK") {
                    swal({
                        title: 'Operazione eseguita correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
                else {
                    swal({
                        title: data.infoAggiuntive,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }

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

function Dematerializzazione_ContinuaDaSegreteria(iddocumento) {
    swal({
        title: 'Sei sicuro di voler portare la pratica allo stato successivo?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì',
        cancelButtonText: 'No',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var nota = $('#Richiesta_Documento_NotaSegreteria').val();
        $.ajax({
            url: "/Dematerializzazione/ConfermatoDaSegreteria",
            type: "POST",
            data: JSON.stringify({
                idDoc: iddocumento,
                nota: nota
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data.result === "OK") {
                    swal({
                        title: 'Operazione eseguita correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }
                else {
                    swal({
                        title: data.infoAggiuntive,
                        type: 'error',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    });
                }

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

function Dematerializzazione_AbilitaPosizionamentoSigla(iddocumento) {

    // nasconde la select per il numero pagina della firma
    $('#dem-sel-pagina-documento').hide();
    $('#draggable').hide();
    $('#draggableData').hide();
    $('#draggableFirma').hide();
    $('#draggableSiglaAllegato').show();
    $('#dem-div-sel-allegato').show();
    $('#dem-div-sel-allegato-pagina').show();

    // deve caricare il nome e l'id degli allegati caricati
    $('tr[id^="riga-allegato-"][data-isprincipal="0"][data-tipo="2"]').each(function () {
        let _myId = $(this).data('id');
        let testo = $('#nome-file-VSDIP-' + _myId).text();
        $('#dem-sel-allegato').append($('<option>', {
            value: _myId,
            text: testo
        }));
    });
}

function Dematerializzazione_CaricaAnteprimaAllegatoPerPosizionamentoSigla() {
    let idAllegato = $('#dem-sel-allegato').val();
    idAllegato = parseInt(idAllegato, 10);
    if (idAllegato === -1) {
        return false;
    }

    let pagina = $('#dem-div-sel-allegato-pagina').val();
    pagina = parseInt(pagina, 10);

    $.ajax({
        url: '/Dematerializzazione/GetFileInJpeg',
        type: "GET",
        data: {
            idAllegato: idAllegato,
            pagina: pagina
        },
        async: false,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        complete: function () {
        },
        success: function (data) {
            $('#ItemPreview').attr('src', data);
        }
    });
}



function Dematerializzazione_CambioPaginaProtocollo(nuovaPagina) {
    var pagina = "";
    if (nuovaPagina === null || nuovaPagina === "" || typeof nuovaPagina === "undefined") {
        var paginacorrente = $('#dem-paginaprotocollo').data('paginacorrente');
        paginacorrente = parseInt(paginacorrente, 10);

        if (isNaN(paginacorrente)) {
            paginacorrente = 1;
        }

        var paginarFirma = $('#selPagina').val(); //Pagina Firma

        var paginarichiesta = $('#selPaginaProtocollo').val();
        paginarichiesta = parseInt(paginarichiesta, 10);

        if (isNaN(paginarichiesta)) {
            paginarichiesta = 1;
        }

        if (!$('#icona1-protocollo').hasClass('evidenzia')) {
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

    if (pagina === -1000) {
        abilitaProt = true;
        abilitaData = true;
        abilitaFirma = false;
    }

    //if (abilitaFirma) {
    $('#dem-paginaprotocollo').data('paginarichiesta', pagina);
    $('#dem-paginaprotocollo').data('paginaprotocollo', pagina);
    //}

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

            $('#dem-paginaprotocollo').data('paginacorrente', pagina);

            //if (abilitaProt) {
            $('#icona1-protocollo').addClass('evidenzia');
            $('#draggable').show();
            //}
            //else {
            //    $('#icona1-protocollo').removeClass('evidenzia');
            //    $('#draggable').hide();
            //}

            //if (abilitaData) {
            $('#icona2-data').addClass('evidenzia');
            $('#draggableData').show();
            //}
            //else {
            //    $('#icona2-data').removeClass('evidenzia');
            //    $('#draggableData').hide();
            //}

            if (paginarFirma == pagina) {
                $('#icona3-firma').addClass('evidenzia');
                $('#draggableFirma').show();
                //$('#dem-sel-pagina-documento').show();
            }
            else {
                //$('#icona3-firma').removeClass('evidenzia');
                //$('#dem-sel-pagina-documento').hide();
                $('#draggableFirma').hide();
            }

            var guid = $('#selPaginaProtocollo').data('rai-select');
            var nPagine = $('div[class="rai-select-option"][data-search="search-' + guid + '"]').length;
            nPagine = parseInt(nPagine, 10);

            if (nPagine === 1) {
                $('#dem-paginaprotocollo').hide();
            }
            $('#selPaginaProtocollo').val(pagina);
        },
        success: function (data) {
            $('#ItemPreview').attr('src', data);
        }
    });
}

function OpenBozzaEmail(caller) {
    //SubmitAvviaPratica(false, false, '', false);
    var idDoc = $('#idDocumento').val();
    if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
        idDoc = 0;
    }
    //debugger;
    document.getElementById(caller.context.id).value = caller.context.value.trim();
    debugger;
    //var idTemplate = $('#IdMailTemplate').val();
    var idTemplate = $('#IdMailTemplate').val();
    //alert(idTemplate);
    RaiOpenAsyncModal("modal-viewer-temp-allegati", '/Dematerializzazione/GetTempMail', { idTemplate: idTemplate, idDoc: idDoc }, null, 'GET');
}

function OpenBozzaEmailReload(idTemplate, idDoc) {
    //SubmitAvviaPratica(false, false, '', false);
    //var idTemplate = caller.context.value.trim();
    RaiOpenAsyncModal("modal-viewer-temp-allegati", '/Dematerializzazione/GetTempMail', { idTemplate: idTemplate, idDoc: idDoc }, null, 'GET');
}

function SubmitBozza(invio, sblocca) {
    event.preventDefault();

    //var tipologia = $('#TipologiaBozza').val();

    if (!invio)
        InternalSubmitBozza(invio, sblocca);
}

function InternalSubmitBozza(invio, sblocca) {
    $('#btnBozzaMail').addClass('rai-loader');

    var idDoc = $('#idDocumento').val();
    if (idDoc === null || idDoc === "" || typeof (idDoc) === "undefined") {
        idDoc = 0;
    }

    //var tipologia = $('#TipologiaBozza').val();
    //if (tipologia == 'proposta')
    //InternalSubmitAvviaPratica(false, false, '', false, '', false);
    //else
    //    SubmitModificaBozzaVerbale(idDip, false);

    var formData = new FormData($('#form-bozza')[0]);
    //formData.append('_invioMail', invio)

    //formData.append('_includiProposta', $('#includiAllegato:checked').length > 0);
    //formData.append('_sbloccaPratica', sblocca);

    $.ajax({
        url: "/Dematerializzazione/Save_BozzaMail",
        type: "POST",
        cache: false,
        dataType: 'html',
        contentType: false,
        processData: false,
        data: formData,
        success: function (data) {
            if (data !== null) {
                $('#IdMailTemplate').val(data);
                swal("OK", "Template salvato con successo", 'success');
                OpenBozzaEmailReload(data, idDoc);
            }

            //else {
            //    swal("OK", "Mail inviata con successo", 'success');
            //    RefreshIncentivato(idDip)
            //}

            //var tipologia = $('#TipologiaBozza').val();
            //if (tipologia == "proposta")

            //else
            //    OpenBozzaVerbale(idDip);

        },
        error: function (a, b, c) {
            swal("Oops...", c, 'error')
        },
        complete: function (data) {
            $('#btnBozzaMail').removeClass('rai-loader');
        }
    });
}

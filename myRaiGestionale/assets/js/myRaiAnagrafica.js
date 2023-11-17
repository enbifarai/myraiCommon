function OpenModalDipStati(idPersona, modalName) {
    event.preventDefault();
    RaiOpenAsyncModal(modalName, '/StatiRapporto/Modal_Dipendente', { idPersona: idPersona });
}

function OpenModalCerca(idPersona) {
    RaiOpenAsyncModal('modal-ricerca', '/StatiRapporto/Modal_Ricerca');
}

function OpenModalAnag(matricola) {
    RaiOpenAsyncModal('modal-anagrafica', '/Anagrafica/Modal_Anagrafica', { m: matricola }, null, null, true);
}
function OpenModalRecapiti(matricola) {
    RaiOpenAsyncModal('modal-recapiti', '/Anagrafica/Modal_Recapiti', { m: matricola }, null, null, true);
}
function OpenModalIndirizzo(matricola, tipologia, newRec) {
    RaiOpenAsyncModal('modal-indirizzo', '/Anagrafica/Modal_Indirizzo', { m: matricola, tipologia: tipologia, nuovo: newRec }, null, null, true);
}
function OpenModalStatiRapporto(matricola, idStato) {
    RaiOpenAsyncModal('modal-stati-rapporto', '/Anagrafica/Modal_StatiRapporto', { m: matricola, idStato: idStato });
}
function OpenModalIban(matricola, id, tipologia) {
    RaiOpenAsyncModal('modal-iban', '/Anagrafica/Modal_DatiIban', { m: matricola, id: id, tipo: tipologia }, null, null, true);
}
function OpenModalRichiesta(matricola, tipologia, id) {
    RaiOpenAsyncModal('modal-richiesta', '/Scrivania/Modal_Richiesta', { m: matricola, tipo: tipologia, id: id });
}
function OpenModalTitoloStudio(matricola, cod) {
    RaiOpenAsyncModal('modal-studio', '/Anagrafica/Modal_TitoloStudio', { m: matricola, cod: cod }, null, null, true);
}

function OpenModalStoricoCarriera(matricola) {
    RaiOpenAsyncModal('modal-storico-carriera', '/Anagrafica/Modal_VariazioniContratti', { m: matricola }, null, 'POST');
}

function OpenModalDatiContr(idPersona, tipo) {
    RaiOpenAsyncModal('modal-edit-daticontr', '/Anagrafica/Modal_DatiContr', { idPersona: idPersona, tipo: tipo }, null, null, true);
}

function Save_DatiContr(button, accept) {
    event.preventDefault();
    RaiDisableCheckListener('modal-edit-daticontr');


    var form = $('#form-contract');
    //var validator = $(form).validate();
    var validator = $(form).validate({
        errorPlacement: function (error, element) {
            var name = $(element).attr('id');
            $('[data-valmsg-for="' + name + '"]').html(error);
        }
    });

    if (!$(form).valid()) {
        validator.focusInvalid();
        return false;
    }

    var message = "Vuoi confermare i dati inseriti?";
    var confirmMessage = "Richiesta inserita con successo";
    if (!accept) {
        message = "Vuoi cancellare la richiesta di variazione?";
        confirmMessage = "Richiesta cancellata con successo";
    }

    swal({
        title: 'Sei sicuro?',
        type: 'question',
        html: message,
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var paramGetter = function () {
            var obj = new FormData($('#form-contract')[0]);
            obj.append('approva', accept);
            return obj;
        };

        var onSuccess = function () {
            $('#modal-edit-daticontr').modal("hide");
            $('#modal-richiesta').modal("hide");

            var m = $('#Matricola').val();
            if ($('#carriera').length > 0) {
                RaiUpdateWidget("carriera", "/Anagrafica/Load_DatiContrattuali", "html", { m: m });
            }
            if ($('#notifiche-container').length > 0) {
                RaiUpdateWidget('notifiche-container', '/Anagrafica/GetRichieste', 'html', { m: m }, false, null, true);
            }
        }

        RaiSubmitForm(button, 'form-contract', paramGetter, false, false, confirmMessage, onSuccess, false);
    });
}


function Save_DatiAnagrafici(button, m) {
    RaiDisableCheckListener('modal-anagrafica');
    RaiSubmitForm(button, 'form-dati-anagrafici', function () {
        var obj = new FormData($('#form-dati-anagrafici')[0]);
        return obj;
    }, false, false,
        "Dati salvati con successo",
        function () {
            $('#modal-anagrafica').modal("hide");
            RaiUpdateWidget("anag", "/Anagrafica/Load_DatiAnagrafica", "html", { m: m });
        }, false);
}
function Save_DatiRecapito(button, m) {
    RaiDisableCheckListener('modal-recapiti');
    RaiSubmitForm(button, 'form-dati-recapiti', function () {
        var obj = new FormData($('#form-dati-recapiti')[0]);
        return obj;
    }, false, false,
        "Dati salvati con successo",
        function () {
            $('#modal-recapiti').modal("hide");
            RaiUpdateWidget("recapiti", "/Anagrafica/Load_DatiRecapiti", "html", { m: m });
        }, false);
}

function Save_DatiIndirizzo(button, m) {
    RaiDisableCheckListener('modal-indirizzo');

    event.preventDefault();
    $(button).addClass("disable");

    let idForm = 'form-dati-indirizzo';
    var form = $("#" + idForm).first();
    var validator = $(form).validate();

    if (!$(form).valid()) {
        $(button).removeClass("disable");
        return false;
    }

    var parameters = new FormData($('#form-dati-indirizzo')[0]);

    $(form).parent().addClass("rai-loader");

    $.ajax({
        url: $(form).attr("action"),
        processData: false,
        contentType: false,
        type: "POST",
        data: parameters,
        dataType: 'json',
        success: function (data) {
            if (data.result) {
                let successMsg = 'Dati salvati con successo';
                if (data.errorMsg != '')
                    successMsg = data.errorMsg;
                swal({ title: successMsg, type: !data.errorMsg ? 'success' : 'warning', customClass: 'rai' });
                $('#modal-indirizzo').modal("hide");
                RaiUpdateWidget("resDom", "/Anagrafica/Load_DatiResidenzaDomicilio", "html", { m: m });
            }
            else {
                swal({ title: "Ops...", text: data, type: 'error', customClass: 'rai' });

            }
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        },
        error: function (a, b, c) {
            swal({ title: "Ops...", text: ' ' + b + ' ' + c, type: 'error', customClass: 'rai' });
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        }
    });
}
function Save_DatiStatoRapporto(button, codice) {
    event.preventDefault();
    debugger
    $("#scelta-mod-rec").val("");
    var matricola = $("#matricola-dip").val();

    if ($("#invia-api").prop("checked") && button.textContent.trim()=="Modifica") {
        $.ajax({
            url: "/statiRapporto/CheckPrimaModificaCom",
            type: "POST",
            cache: false,
            dataType: 'json',
            data: { matricola: matricola },
            success: function (data) {
                if (data.esito == true) {
                    swal({
                        title: "Modifica comunicazione",
                        type: 'question',
                        html: "Non sono stati trovati invii di precedenti nuove comunicazioni, vuoi che la presente comunicazione venga inviata come Modifica o Recesso ?" +
                            "<br /> </br> " +
                            "<input type='radio' name ='modrec' id='sc-1' style='margin-right:16px' checked='checked'/><span>Invia come Modifica</span> <br /><br />" +
                            "<input type='radio' name ='modrec' id='sc-2' style='margin-right:16px' /><span>Invia come Recesso</span> <br />"
                        ,
                        showCancelButton: true,
                        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
                        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
                        reverseButtons: true,
                        customClass: 'rai rai-confirm-cancel'
                    }).then(function () {
                        var scelta = "M";
                        if ($("#sc-2").prop("checked"))
                            scelta = "R";

                        $("#scelta-mod-rec").val(scelta);
                        Save_DatiStatoRapportoInternal(button, codice);
                    });
                }
                else {
                    Save_DatiStatoRapportoInternal(button, codice);
                }
            }
        });
    }
    else {
        Save_DatiStatoRapportoInternal(button, codice);
    }
   
}
function Save_DatiStatoRapportoInternal(button, codice) {
    var dataDaStr = $('#form-stati-rapporto-' + codice + ' #DataInizio').val();
    var dataAStr = $('#form-stati-rapporto-' + codice + ' #DataFine').val();

    var modalTitle = $('#form-stati-rapporto-' + codice + ' #stato-swal-title').val()//;'Sei sicuro di voler attivare lo Smartworking per questo utente?';
    var subTitle = $('#form-stati-rapporto-' + codice + ' #stato-swal-subtitle').val();
    var htmlText = "";
    //var idEvento = $('#IdEvento').val();
    //if (idEvento > 0) {
    //    //var oldDate = $('#oldDataFine').val();
    //    //var newDate = $('#DataFine')
    //    modalTitle = 'Sei sicuro di voler modificare lo Smartworking per questo utente?';
    //}

    var dataDa = new Date(dataDaStr.substr(6, 4), dataDaStr.substr(3, 2) - 1, dataDaStr.substr(0, 2));
    var dataA = new Date(dataAStr.substr(6, 4), dataAStr.substr(3, 2) - 1, dataAStr.substr(0, 2));

    var profileHtml = $('#profile-widget').html();

    htmlText = '<br>' + profileHtml + '<br><span class="rai-font-sm-neutral">' + subTitle + '</span><br>' +
        '<span class="rai-font-md">' + dataDa.toLocaleDateString("it-IT", { day: 'numeric', month: 'long', year: 'numeric' }) + ' - ' + dataA.toLocaleDateString("it-IT", { day: 'numeric', month: 'long', year: 'numeric' }) + '</span>';

    swal({
        title: modalTitle,
        type: 'question',
        html: htmlText,
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        RaiSubmitForm(button, 'form-stati-rapporto-' + codice, function () {
            var obj = new FormData($('#form-stati-rapporto-' + codice)[0]);
            return obj;
        }, false, false,
            "Dati salvati con successo",
            function () {
                //$('#modal-stati-rapporto').modal("hide");
                var idPersona = $('#form-stati-rapporto-' + codice + ' #IdPersona').val();
                $('#modal-gest-stato').modal('hide');
                RaiUpdateWidget("block-stati", "/StatiRapporto/Modal_Dipendente", "replaceId", { idPersona: idPersona }, false, function () { FillApiFound($("#matricola-dip").val()); })
                if ($('#form-ricerca-dipendente[action="/StatiRapporto/RicercaDipendente"]').length > 0) {
                    $('#form-ricerca-dipendente[action="/StatiRapporto/RicercaDipendente"]').submit();
                }
                debugger
                //, null, function () { FillApiFound($("#matricola-dip").val()); }
            }, false);
    });
}
function Save_DatiIban(button, m) {
    RaiDisableCheckListener('modal-iban');
    RaiSubmitForm(button, 'form-iban', function () {
        var obj = new FormData($('#form-iban')[0]);
        return obj;
    }, false, false,
        "Dati salvati con successo",
        function () {
            $('#modal-iban').modal("hide");
            RaiUpdateWidget("bank", "/Anagrafica/Load_DatiIban", "html", { m: m });
        }, false);
}
function Save_DatiStudio(button, m) {
    RaiDisableCheckListener('modal-studio');

    let idForm = 'form-studio';
    event.preventDefault();
    $(button).addClass("disable");

    var form = $("#" + idForm).first();
    var validator = $(form).validate({
        rules: {
            italianiban: {
                italianiban: true
            }
        }
    });

    if (!$(form).valid()) {
        $(button).removeClass("disable");
        return false;
    }

    var parameters = new FormData($('#form-studio')[0]);

    $(form).parent().addClass("rai-loader");

    $.ajax({
        url: $(form).attr("action"),
        processData: false,
        contentType: false,
        type: "POST",
        data: parameters,
        success: function (data) {
            switch (data) {
                case "OK":
                    swal({
                        title: 'Dati salvati con successo', type: 'success', customClass: 'rai'
                    });
                    $('#modal-studio').modal("hide");
                    RaiUpdateWidget("studies", "/Anagrafica/Load_DatiStudio", "html", { m: m });
                    break;
                default:
                    swal({ title: "Ops...", text: data, type: 'error', customClass: 'rai' });
                    break;
            }
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        },
        error: function (a, b, c) {
            swal({ title: "Ops...", text: ' ' + b + ' ' + c, type: 'error', customClass: 'rai' });
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        }
    })
}
function Delete_DatiStudio(m, cod) {
    RaiDeleteRecord("Vuoi cancellare il titolo di studio?", "/Anagrafica/Delete_DatiStudio",
        { m: m, cod: cod },
        "Cancellazione effettuata con successo", function () {
            RaiUpdateWidget("studies", "/Anagrafica/Load_DatiStudio", "html", { m: m });
        });
}
function Delete_DatiIban(m, id, tipo) {
    RaiDeleteRecord("", "/Anagrafica/Delete_DatiIban",
        { m: m, id: id, tipo: tipo },
        "Cancellazione effettuata con successo", function () {
            RaiUpdateWidget("bank", "/Anagrafica/Load_DatiIban", "html", { m: m });
        });
}
function Delete_ModIban(m, idEv) {
    swal({
        title: 'Sei sicuro?',
        text: 'La richiesta verrà eliminata.',
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sì, elimina!',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: '/Anagrafica/Delete_ModIban',
            type: "GET",
            dataType: "html",
            data: { idEv: idEv },
            cache: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'Cancellazione effettuata con successo',
                            type: "success",
                            customClass: 'rai'
                        });
                        $('#modal-iban').modal('hide');
                        RaiUpdateWidget("bank", "/Anagrafica/Load_DatiIban", "html", { m: m });
                        break;
                    default:
                        swal({
                            title: "Ops...",
                            text: data,
                            type: 'error',
                            customClass: 'rai'
                        });
                }
            },
            error: function (a, b, c) {
                swal({
                    title: "Ops...",
                    text: "Si è verificato un errore imprevisto\n" + c,
                    type: 'error',
                    customClass: 'rai'
                });
            }
        });
    });
}
function Convalida_ModIban(m, id) {
    swal({
        title: 'Sei sicuro?',
        text: 'La richiesta verrà convalidata senza controlli.',
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sì, convalida!',
        cancelButtonText: 'Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: '/Anagrafica/Convalida_ModificaIban',
            type: "GET",
            dataType: "html",
            data: { m: m, id: id },
            cache: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'Convalida effettuata con successo',
                            type: "success",
                            customClass: 'rai'
                        });
                        $('#modal-iban').modal('hide');
                        RaiUpdateWidget("bank", "/Anagrafica/Load_DatiIban", "html", { m: m });
                        break;
                    default:
                        swal({
                            title: "Ops...",
                            text: data,
                            type: 'error',
                            customClass: 'rai'
                        })
                }

            },
            error: function (a, b, c) {
                swal({
                    title: "Ops...",
                    text: "Si è verificato un errore imprevisto\n" + c,
                    type: 'error',
                    customClass: 'rai'
                })
            }
        });
    });
}

function OpenModalStorico(matricola, tipologia) {
    RaiOpenAsyncModal('modal-storico', '/Anagrafica/Modal_Storico', { m: matricola, tipologia: tipologia });
}

//function OpenModalRichiesteDematerializzazione(matricola, idPersona) {
//    RaiOpenAsyncModal('modal-richiestaDematerializzazione', '/Anagrafica/Modal_RichiestaDematerializzazione', { m: matricola, id: idPersona }, null, 'POST');
//}

function AnagRicercaDip() {
    apriModale('modal-ricerca-dipendente');
    RaiUpdateWidget('modal-ricerca-dipendente-body', '/Ricerca/RicercaDipendente', 'html', { nominativoDipendente: '', matricola: '', action: '' }, false, null, false, 'POST');
}

function AnagIbanUpdateValue() {
    var ibanvalue = $('#IBAN').val();

    if (ibanvalue == '') {
        $('#block-agenzia').hide();
        return;
    }

    $('#Agenzia').parent().addClass('rai-loader');
    $('#IndirizzoAgenzia').parent().addClass('rai-loader');

    $.ajax({
        type: 'GET',
        url: "/Anagrafica/GetAnagBanca",
        datatype: "json",
        data: { iban: ibanvalue },
        success: function (data) {
            $('#block-agenzia').show();

            if (data.result.trim() == "ok") {

                var agenzia = data.agenzia;
                var indirizzo = data.indirizzo;
                if (data.citta != '') {
                    indirizzo += " - " + data.citta;
                }

                var citta = data.citta;

                $('#Agenzia').text(agenzia);
                $('#IndirizzoAgenzia').text(indirizzo);
            }
            else {
                $('#Agenzia').text('');
                $('#IndirizzoAgenzia').text('');
            }
        }
    });

    $('#Agenzia').parent().removeClass('rai-loader');
    $('#IndirizzoAgenzia').parent().removeClass('rai-loader');
}


function AnagSearchFromBar() {
    event.preventDefault();
    $('#searchbar-int-cont .searchbar-item-box .collapse.in').collapse('toggle');
    let formData = new FormData($('#form-searchbar')[0]);
    let sezSelected = $('#sb-jstree').jstree().get_selected(true);
    sezSelected.forEach(function (sez) {
        formData.append('Filtri.Sezioni', sez['li_attr']['data-codice']);
    });
    let faSelected = $('[name="Filtri.Aggiuntivi"]input:checked').toArray();
    faSelected.forEach(function (sez, i) {
        formData.append('Filtri.FiltriAggiuntivi[' + i + '].NomeFiltro', $(sez).attr("data-filter-element"));
        formData.append('Filtri.FiltriAggiuntivi[' + i + '].Descrizione', $(sez).text());
        formData.append('Filtri.FiltriAggiuntivi[' + i + '].Valore', $(sez).val());

    });
    RaiOPNavGoToNext("nav-scrivania", "nav-search-result", "Risultati ricerca", $('#form-searchbar').attr('action'), formData, "POST", true);
}
function AnagSearchBarCheckFilter(elem) {
    let cont = $(elem).data('filter-element');
    debugger
    var valore = $(elem).val().substr(0,2);
    if (cont == "din") {
        $("input[data-filter-element='din']").each(function () {
            if ($(this).attr("id") != $(elem).attr("id") && $(this).val().substr(0,2)==valore)
                $(this).prop("checked", false);
        });
    }
    AnagSearchBarCheckFilterCont(cont);
    AnagSearchBarCheckFilterTot();
}
function AnagSearchBarCheckSelectFilter(cont) {
    AnagSearchBarCheckFilterCont(cont);
    AnagSearchBarCheckFilterTot();
}
function AnagSearchBarCheckFilterCont(cont) {
    let countSelElem = $('[data-filter-element="' + cont + '"]').filter(function () {
        if ($(this).attr('type') === 'text')
            return $(this).val() !== '';
        else if ($(this).attr('type') === 'checkbox')
            return $(this)[0].checked;
        else if ($(this)[0].tagName.toLowerCase() == 'select')
            return $(this).val() !== undefined && $(this).val() !== null;
    }).length;

    $('[data-filter-container="' + cont + '"]').attr('data-filter-selected', countSelElem > 0);
    $('[data-filter-counter="' + cont + '"]').text(countSelElem);
    if (countSelElem > 0) {
        $('[data-filter-counter="' + cont + '"]').show();
    } else {
        $('[data-filter-counter="' + cont + '"]').hide();
    }
}
function AnagSearchBarCheckFilterTot() {
    let counterTotFilter = $('[data-filter-element]').filter(function () {
        if ($(this).attr('type') === 'text')
            return $(this).val() !== '';
        else if ($(this).attr('type') === 'checkbox')
            return $(this)[0].checked;
    }).length;
    $('[data-filter-counter="avanzate"]').text(counterTotFilter);
    if (counterTotFilter > 0) {
        $('[data-filter-counter="avanzate"]').show();
    } else {
        $('[data-filter-counter="avanzate"]').hide();
    }
}
function AnagSearchOrgCheckCont(input) {
    let k = $(input).val();
    $("#sb-jstree").jstree(true).search(k);
}
function AnagSearchClear() {
    event.preventDefault();
    RaiClearForm('form-searchbar', false);
    $("#sb-jstree").jstree(true).uncheck_all();
    $('#Filtri_EscludiCessati')[0].checked = true;

    $('[data-filter-counter]').each(function () {
        let cont = $(this).data('filter-counter');
        AnagSearchBarCheckFilterCont(cont);
    })
    AnagSearchBarCheckFilterTot();

    AnagSearchFromBar();
}

function LoadTrasferte(elem, stato) {
    if ($(elem).closest('#modal-trasferte').length > 0) {
        var parent = $(elem).closest('#modal-trasferte');
    }
    else {
        var parent = $(elem).closest('#trasferte');
    }
    $(parent).find('#table-trasferte').addClass("rai-loader");

    var m = $(parent).find('#trasferte_matricola').val();
    if ($(parent).find('#bodyTrasferte' + stato).attr("data-target") == "trasferteloaded") {
        $(parent).find('#bodyTrasferte' + stato).removeAttr("hidden");
        $(parent).find('#trasferte_statoVisibile').val(stato);
        if (stato == "aperte") {
            $(parent).find('#bodyTrasferteconcluse').attr("hidden", "hidden");
        }
        else {
            $(parent).find('#bodyTrasferteaperte').attr("hidden", "hidden");
        }
        if ($(parent).find('#trasferte_hasnext' + stato).val()) {
            $(parent).find('#toggle-AltreTrasferte').removeClass("hidden");
        }
        else {
            $(parent).find('#toggle-AltreTrasferte').addClass("hidden");
        }
        $(parent).find('#table-trasferte').removeClass("rai-loader");

        return;
    }
    var datada = $(parent).find('#datameseTrasferta').val();
    $.ajax({
        url: "/Anagrafica/LoadTabDatiTrasferte",
        data: {
            matricola: m,
            macroStato: stato,
            data: datada
        },
        type: "POST",
        success: function (data) {
            $(parent).find('#bodyTrasferte' + stato).html(data);
            $(parent).find('#bodyTrasferte' + stato).attr("data-target", "trasferteloaded");
            $(parent).find('#trasferte_statoVisibile').val(stato);
            $(parent).find('#bodyTrasferte' + stato).removeAttr("hidden");
            if (stato == "aperte") {
                $(parent).find('#bodyTrasferteconcluse').attr("hidden", "hidden");
            }
            else {
                $(parent).find('#bodyTrasferteaperte').attr("hidden", "hidden");
            }
            if ($(parent).find('#trasferte_hasnext' + stato).val()) {
                $(parent).find('#toggle-AltreTrasferte').removeClass("hidden");
            }
            else {
                $(parent).find('#toggle-AltreTrasferte').addClass("hidden");
            }
        },
        error: function (err1, err2, err3) {
        },
        complete: function () {
            $(parent).find('#table-trasferte').removeClass("rai-loader");
        }
    });


}

function DettaglioTrasferta(foglioViaggio) {
    RaiOpenAsyncModal('modal-DettaglioAnagrafica', '/Anagrafica/ModalDettaglioTrasferta', { id: foglioViaggio }, null, null, null, null, false);
}
function VisualizzaBiglietti(foglioViaggio) {
    RaiOpenAsyncModal('modal-DettaglioAnagrafica', '/Anagrafica/ModalBigliettiTrasferta', { foglioViaggio: foglioViaggio }, null, null, null, null, false);
}

function toggleTrasferte(elem) {
    if ($(elem).closest('#modal-trasferte').length > 0) {
        var parent = $(elem).closest('#modal-trasferte');
    }
    else {
        var parent = $(elem).closest('#trasferte');
    }
    $(parent).find('#table-trasferte').addClass("rai-loader");
    var m = $(parent).find('#trasferte_matricola').val();
    var stato = $(parent).find('#trasferte_statoVisibile').val();
    // $('#divCaricamentoInCorso' + stato).show();
    var page = $(parent).find('#trasferte_pagina' + stato).val();
    var size = $(parent).find('#trasferte_elementi' + stato).val();

    var datada = $(parent).find('#datameseTrasferta').val();

    $.ajax({
        url: "/Anagrafica/LoadTabDatiTrasferte",
        data: {
            matricola: m,
            page: page,
            size: size,
            macroStato: stato,
            data: datada
        },
        type: "POST",
        success: function (data) {
            $(parent).find('#bodyTrasferte' + stato).html(data);
            $(parent).find('#bodyTrasferte' + stato).attr("data-target", "loaded");
            if (stato == "aperte") {
                $(parent).find('#bodyTrasferteconcluse').attr("hidden", "hidden");
            }
            else {
                $(parent).find('#bodyTrasferteaperte').attr("hidden", "hidden");
            }
            if ($(parent).find('#trasferte_hasnext' + stato).val()) {
                $(parent).find('#toggle-AltreTrasferte').removeClass("hidden");
            }
            else {
                $(parent).find('#toggle-AltreTrasferte').addClass("hidden");
            }
        },
        error: function (err1, err2, err3) {
        },
        complete: function () {
            $(parent).find('#table-trasferte').removeClass("rai-loader");
        }
    });
    

}
function cambioData(elem) {

    if ($(elem).closest('#modal-trasferte').length > 0) {
        var parent = $(elem).closest('#modal-trasferte');
    }
    else {
        var parent = $(elem).closest('#trasferte');
    }

    var stato = $(parent).find('#trasferte_statoVisibile').val();
    $(parent).find('#trasferte_pagina' + stato).val("0");
    $(parent).find('#trasferte_elementi' + stato).val("0");
    $(parent).find('[data-target="trasferteloaded"]').attr("data-target", "");

    toggleTrasferte(elem);
}

function LoadSpeseProduzione(elem, stato) {
    if ($(elem).closest('#modal-speseproduzione').length > 0) {
        var parent = $(elem).closest('#modal-speseproduzione');
    }
    else {
        var parent = $(elem).closest('#speseproduzione');
    }
    $(parent).find('#table-speseproduzione').addClass("rai-loader");

    var m = $(parent).find('#speseproduzione_matricola').val();
    if ($(parent).find('#bodySpeseproduzione' + stato).attr("data-target") == "loaded") {
        $(parent).find('#bodySpeseproduzione' + stato).removeAttr("hidden");
        $(parent).find('#speseproduzione_statoVisibile').val(stato);
        if (stato == "aperte") {
            $(parent).find('#bodySpeseproduzioneconcluse').attr("hidden", "hidden");
        }
        else {
            $(parent).find('#bodySpeseproduzioneaperte').attr("hidden", "hidden");
        }
        if ($(parent).find('#speseproduzione_hasnext' + stato).val()) {
            $(parent).find('#toggle-AltreSpese').removeClass("hidden");
        }
        else {
            $(parent).find('#toggle-AltreSpese').addClass("hidden");
        }
        $(parent).find('#table-speseproduzione').removeClass("rai-loader");
        return;
    }
    var datada = $(parent).find('#datameseSpesa').val();
    $.ajax({
        url: "/Anagrafica/LoadTabDatiSpeseproduzione",
        data: {
            matricola: m,
            page: 0,
            size: 0,
            macroStato: (stato == "aperte" ? true : false),
            data: datada
        },
        type: "POST",
        success: function (data) {
            $(parent).find('#bodySpeseproduzione' + stato).html(data);
            $(parent).find('#bodySpeseproduzione' + stato).attr("data-target", "loaded");
            $(parent).find('#speseproduzione_statoVisibile').val(stato);
            $(parent).find('#bodySpeseproduzione' + stato).removeAttr("hidden");
            if (stato == "aperte") {
                $(parent).find('#bodySpeseproduzioneconcluse').attr("hidden", "hidden");
            }
            else {
                $(parent).find('#bodySpeseproduzioneaperte').attr("hidden", "hidden");
            }
            if ($(parent).find('#speseproduzione_hasnext' + stato).val()) {
                $(parent).find('#toggle-AltreSpese').removeClass("hidden");
            }
            else {
                $(parent).find('#toggle-AltreSpese').addClass("hidden");
            }
        },
        error: function (err1, err2, err3) {
        }
    });
    $(parent).find('#table-speseproduzione').removeClass("rai-loader");
}

function toggleSpese(elem) {
    if ($(elem).closest('#modal-speseproduzione').length > 0) {
        var parent = $(elem).closest('#modal-speseproduzione');
    }
    else {
        var parent = $(elem).closest('#speseproduzione');
    }
    $(parent).find('#table-speseproduzione').addClass("rai-loader");
    var m = $(parent).find('#speseproduzione_matricola').val();
    var stato = $(parent).find('#speseproduzione_statoVisibile').val();
    // $('#divCaricamentoInCorso' + stato).show();
    var page = $(parent).find('#speseproduzione_pagina' + stato).val();
    var size = $(parent).find('#speseproduzione_elementi' + stato).val();

    var datada = $(parent).find('#datameseSpesa').val();

    $.ajax({
        url: "/Anagrafica/LoadTabDatiSpeseProduzione",
        data: {
            matricola: m,
            page: page,
            size: size,
            macroStato: (stato == "aperte" ? true : false),
            data: datada
        },
        type: "POST",
        success: function (data) {
            $(parent).find('#bodySpeseproduzione' + stato).html(data);
            $(parent).find('#bodySpeseproduzione' + stato).attr("data-target", "loaded");
            if (stato == "aperte") {
                $(parent).find('#bodySpeseproduzioneconcluse').attr("hidden", "hidden");
            }
            else {
                $(parent).find('#bodySpeseproduzioneaperte').attr("hidden", "hidden");
            }
            if ($(parent).find('#speseproduzione_hasnext' + stato).val()) {
                $(parent).find('#toggle-AltreSpese').removeClass("hidden");
            }
            else {
                $(parent).find('#toggle-AltreSpese').addClass("hidden");
            }
        },
        error: function (err1, err2, err3) {
        }
    });
    $(parent).find('#table-speseproduzione').removeClass("rai-loader");
}

function cambioDataSpesa(elem) {
    if ($(elem).closest('#modal-speseproduzione').length > 0) {
        var parent = $(elem).closest('#modal-speseproduzione');
    }
    else {
        var parent = $(elem).closest('#speseproduzione');
    }
    var stato = $(parent).find('#speseproduzione_statoVisibile').val();
    $(parent).find('#speseproduzione_pagina' + stato).val("0");
    $(parent).find('#speseproduzione_elementi' + stato).val("0");
    $(parent).find('[data-target="loaded"]').attr("data-target", "");

    toggleSpese(elem);
}

function DettaglioFoglioSpese(foglio) {
    RaiOpenAsyncModal('modal-DettaglioAnagrafica', '/Anagrafica/ModalDettaglioSpeseProduzione', { id: foglio, isAperte: $("#tab1").hasClass("active") });
}
function visualizzaVoce(elem, idfoglio, progressivoVoce) {

    if ($('#nav-abil-funz' + progressivoVoce).attr('data-target') != "loaded") {
        RaiOPNavGoToNext('nav-abil', 'nav-abil-funz' + progressivoVoce, 'voce', '/Anagrafica/VisualizzaVoce', { id_FoglioSpese: idfoglio, progressivo: progressivoVoce }, 'GET');
        // $('#tbody' + progressivoVoce).addClass("open");
        $('#nav-abil-funz' + progressivoVoce).attr("data-target", "loaded");
    }
    else {
        $('#nav-abil-funz' + progressivoVoce).attr("data-target", "");

    }

    return;

}

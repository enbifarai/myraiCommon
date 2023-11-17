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
function OpenModalIndirizzo(matricola, tipologia) {
    RaiOpenAsyncModal('modal-indirizzo', '/Anagrafica/Modal_Indirizzo', { m: matricola, tipologia: tipologia }, null, null, true);
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
function Save_DatiIndirizzo(button, m) {
    RaiDisableCheckListener('modal-indirizzo');
    RaiSubmitForm(button, 'form-dati-indirizzo', function () {
        var obj = new FormData($('#form-dati-indirizzo')[0]);
        return obj;
    }, false, false,
        "Dati salvati con successo",
        function () {
            $('#modal-indirizzo').modal("hide");
            RaiUpdateWidget("resDom", "/Anagrafica/Load_DatiResidenzaDomicilio", "html", { m: m });
        }, false);
}
function Save_DatiStatoRapporto(button, codice) {
    event.preventDefault();

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
                debugger
                var idPersona = $('#form-stati-rapporto-' + codice + ' #IdPersona').val();
                RaiUpdateWidget("block-stati", "/StatiRapporto/Modal_Dipendente", "replaceId", { idPersona: idPersona })

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
    RaiSubmitForm(button, 'form-studio', function () {
        var obj = new FormData($('#form-studio')[0]);
        return obj;
    }, false, false,
        "Dati salvati con successo",
        function () {
            $('#modal-studio').modal("hide");
            RaiUpdateWidget("studies", "/Anagrafica/Load_DatiStudio", "html", { m: m });
        }, false);
}
function Delete_DatiStudio(m, cod) {
    RaiDeleteRecord("Vuoi cancellare il titolo di studio?", "/Anagrafica/Delete_DatiStudio",
        { m: m, cod:cod},
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
    RaiUpdateWidget('modal-ricerca-dipendente-body', '/Ricerca/RicercaDipendente', 'html', { nominativoDipendente: '', matricola: '' }, false, null, false, 'POST');
}
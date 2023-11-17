

// #region Incentivi
function IncGestioneSollecito(idDip, oper) {
    let text = "";
    switch (oper) {
        case "sollecito":
            text = "Vuoi inviare un sollecito al dipendente?";
            break;
        case "decadenza":
            text = "Vuoi far decadere la pratica e inviare una notifica al dipendente?";
            break;
        default:
            break;
    }

    swal({
        title: 'Sei sicuro?',
        text: text,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, confermo!',
        cancelButtonText: 'Annulla',
        customClass: 'rai',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/GestioneSollecito",
            type: "POST",
            data: { idDip: idDip, _oper: oper },
            async: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        CercaIncentivato();
                        UpdateExtra();
                        AggiornaAvanzamento();
                        //Prosegui(idDip);
                        ShowIncentivato(idDip, "");
                        break;
                    default:
                        swal("Errore: " + data);
                        break;
                }
            },
            error: function (result) {
                swal("Errore: " + result);
            }
        });
    });
}

function IncAnnullaStato(idDip, idStato) {
    swal({
        title: 'Sei sicuro?',
        text: "La pratica verrà riportata allo stato precedente",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, confermo!',
        cancelButtonText: 'Annulla',
        customClass: 'rai',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/AnnullaStato",
            type: "POST",
            data: { idDip: idDip, idStato: idStato },
            async: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        CercaIncentivato();
                        AggiornaAvanzamento();
                        Prosegui(idDip);
                        break;
                    default:
                        swal("Errore: " + data);
                        break;
                }
            },
            error: function (result) {
                swal("Errore: " + result);
            }
        });
    });
}

function IncAnnullaPratica(idDip) {
    swal({
        title: 'Sei sicuro?',
        text: "La pratica verrà annullata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, annulla!',
        cancelButtonText: 'Annulla',
        customClass: 'rai',
        reverseButtons: 'true'
    }).then(function () {
        swal({
            //title: '',
            //type: '',
            text: 'Inserisci il motivo dell\'annullamento',
            input: 'textarea',
            showCancelButton: true,
            confirmButtonText: 'Conferma annullamento',
            cancelButtonText: 'Annulla',
            reverseButtons: 'true',
            preConfirm: function (value) {
                return new Promise(function (resolve, reject) {
                    resolve();
                    //if (value == "") {
                    //    reject("Inserisci il testo della nota")
                    //}
                    //else {
                    //    resolve()
                    //}
                })
            },
        }).then(function (result) {
            var nota = result;
            $.ajax({
                url: "/Cessazione/AnnullaPratica",
                type: "POST",
                data: { idDip: idDip, nota: nota },
                async: false,
                success: function (data) {
                    switch (data) {
                        case "OK":
                            CercaIncentivato();
                            AggiornaAvanzamento();
                            Prosegui(idDip);
                            break;
                        default:
                            swal("Errore: " + data);
                            break;
                    }
                },
                error: function (result) {
                    swal("Errore: " + result);
                }
            });
        })
    })
}

function IncAggiungi() {
    RaiOpenAsyncModal('modal-ricerca', '/Cessazione/SelezioneDipendenti');
}

function IncSuccessSubmit(formId, destBox) {
    $("#" + destBox + "Ext").removeClass("rai-loader");
    $("#" + destBox).removeClass("gest-not-authorized");

    //IncSetFilterDescr(formId, destBox);
}
function IncSetFilterDescr(formID, destBox) {
    var hasFilter = $('#' + formID + ' > #HasFilter').val();
    if (hasFilter == 'true') {
        var textFilter = '';
        var jsonString = $('#' + formID).serialize();

        $.ajax({
            url: "/Cessazione/GetStringaFiltri",
            type: "GET",
            dataType: "json",
            data: jsonString,
            cache: false,
            success: function (data) {
                var textFixed = "Stai visualizzando le persone ";

                var elem = $('#' + destBox + ' > #SummaryFiltri')
                $(elem).show();
                $(elem).find('#textSummary').text(textFixed);
                $(elem).find('#textSummaryBold').text(data.Filtro);
            }
        });
    }
    else {
        var elem = $('#' + destBox + ' > #SummaryFiltri')
        $(elem).hide();
        $(elem).find('#textSummary').text('');
        $(elem).find('#textSummaryBold').text('');
    }
}
function IncShowAnteprima(idPersona) {
    event.preventDefault();
    RaiOpenAsyncModal('modal-pratica', '/Cessazione/GetAnteprimaPratica', { idPersona: idPersona });
}
function IncRimuoviPersona(idPersona, idDip) {
    swal({
        title: 'Sei sicuro?',
        text: "La pratica verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        customClass: 'rai',
        reverseButtons: 'true'
    }).then(function () {
        $('#OPER_' + idPersona).show();
        $.ajax({
            url: "/Cessazione/EliminaPratica",
            type: "POST",
            data: { idDip: idDip },
            async: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        $('[data-id-persona="' + idPersona + '"').removeClass('disable');
                        $('[data-prov="' + idPersona + '"').removeClass('text-primary');
                        $('[data-prov="' + idPersona + '"').removeClass('disable');
                        $('[data-prov="' + idPersona + '"').parent().addClass('actions-hover');
                        $('#REMOVE_' + idPersona).hide();
                        //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        break;
                    default:
                        swal("Errore: " + data);

                        break;
                }
                $('#OPER_' + idPersona).hide();
            },
            error: function (result) {
                swal("Errore: " + result);
                $('#OPER_' + idPersona).hide();
            }
        });
    })

}
function IncAggiungiPratica(idPersona) {
    event.preventDefault();
    RaiOpenAsyncModal('modal-add-dip', '/Cessazione/ShowAggiungiDip', { idPersona: idPersona });
}
function IncPreSubmit() {
    event.preventDefault();

    var form = $('#form-Add-Dip');
    var validator = $(form).validate();

    if (!$(form).valid()) {
        validator.focusInvalid();
        return false;
    }

    //$('#form-Add-Dip').submit();
    var obj = $('#form-Add-Dip').serialize();
    $.ajax({
        url: "/Cessazione/AggiungiDip",
        type: "POST",
        cache: false,
        data: obj,
        success: function (data) {
            switch (data) {
                case "OK":
                    swal('Aggiunta pratica', "Pratica creata con successo", "success");
                    $('#modal-add-dip').modal('hide');
                    PulisciFiltri();
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        }
    });
}
function IncAggiungiDip() {

}



function ShowIncentivato(id_dipendente, openFunction, nomeModale) {
    if (nomeModale == undefined || nomeModale == null) {
        nomeModale = 'modal-dettaglioInc';
    }
    RaiOpenAsyncModal(nomeModale, "/Cessazione/GetDettaglioIncentivato", { idDip: id_dipendente, openFunction: openFunction }, null, 'POST');
}
function ShowIncentivatoReview(id_dipendente, openFunction, nomeModale) {
    if (nomeModale == undefined || nomeModale == null) {
        nomeModale = 'modal-pratica-review';
    }
    RaiOpenAsyncModal(nomeModale, "/Cessazione/GetDettaglioIncentivatoReview", { idDip: id_dipendente, openFunction: openFunction }, null, 'POST');
}

function RefreshIncentivato(id_dipendente) {
    var obj = id_dipendente;
    var openFunction = $('#openFunction').val();
    $("#incContent").html(' ');
    $.ajax({
        url: "/Cessazione/GetDettaglioIncentivato",
        type: "POST",
        data: { idDip: obj, openFunction: openFunction },
        async: false,
        success: function (data) {
            $("#incContent").replaceWith($(data).find("#incContent"));
        },
        error: function (result) {
            swal("Errore: " + result);
        }
    });
}

function AzzeraFiltri(formId) {
    formId = formId || 'form-ric-cess';

    RaiClearForm(formId);

    //$('[data-filter]').attr('data-filter', '');
}
function PulisciFiltri() {
    event.preventDefault();
    AzzeraFiltri();
    CercaIncentivato();
}
function IncExtraPulisciFiltri() {
    event.preventDefault();
    AzzeraFiltri('form-ric-extra');
    IncCercaExtra();
}

function CercaIncentivato() {
    if (event)
        event.preventDefault();
    RaiSearchFormCheckHasFilter('form-ric-cess');

    //var matr = $('#txtMatricola').val();
    //var nome = $('#txtNominativo').val();
    //var stato = $('#cmbStato').val();
    //var inCarico = $('#cmbInCarico').val();
    //var dataCess = $('#cmbCessazione').val();
    //var sede = $('#cmbSede').val();
    //var causa = $('#cmbCausa').val();
    //var tipologia = $('#cmbTipologia').val();

    var soloScrittura = true;
    //if (matr != '' || nome != '' || stato != '' || inCarico != '' || dataCess != '' || sede != '' || causa != '' || tipologia != '')
    //    soloScrittura = false;
    if ($('#HasFilter').val() == 'true')
        soloScrittura = false;

    var textFixed = "Stai visualizzando le pratiche";

    $('#ElencoIncentivati').addClass('css-input-disabled');
    $("#ElencoIncentivatiExt").addClass("rai-loader");

    var actionElenco = $('#ElencoIncentivati').attr('data-action');
    var extraFunc = $('#ElencoIncentivati').attr('data-extra');

    var tmpData = "";
    var obj = new FormData($('#form-ric-cess')[0]);
    obj.append('incExtra', extraFunc);

    $.ajax({
        url: "/Cessazione/" + actionElenco,
        type: "POST",
        dataType: "html",
        //data: { matricola: matr, nominativo: nome, stato: stato, inCarico: inCarico, dataCessazione: dataCess, sede: sede, causa: causa, soloScrittura: soloScrittura, tipologia: tipologia },
        contentType: false,
        processData: false,
        data: obj,
        success: function (data) {
            tmpData = data;
            if (soloScrittura) {
                $('#SummaryFiltri').hide();
                $('#ElencoIncentivati').html(data);
                $('#ElencoIncentivati').removeClass('css-input-disabled');
                $("#ElencoIncentivatiExt").removeClass("rai-loader");
            }
            else {
                $.ajax({
                    url: "/Cessazione/GetStringaFiltri",
                    type: "POST",
                    dataType: "json",
                    contentType: false,
                    processData: false,
                    data: obj,
                    cache: false,
                    success: function (data) {
                        $('#ElencoIncentivati').html(tmpData);
                        $('#ElencoIncentivati').removeClass('css-input-disabled');
                        $("#ElencoIncentivatiExt").removeClass("rai-loader");

                        $('#SummaryFiltri').show();
                        $('#textSummary').text(textFixed);
                        $('#textSummaryBold').text(data.Filtro);
                    }
                });
            }
        }
    });
}
function IncCercaExtra(preventDefault) {
    if (preventDefault && event)
        event.preventDefault();

    var textFixed = "Stai visualizzando le pratiche";

    $("#ElencoExtraExt").addClass("rai-loader");

    var actionElenco = $('#ElencoExtra').attr('data-action');
    var extraFunc = $('#ElencoExtra').attr('data-extra');
    var fromModal = $('#ElencoExtra').attr('data-from-modal');

    let visStorico = false;
    if (extraFunc == "__SOLLECITI__" && $('#visStorico:checked').length > 0)
        visStorico = true;

    var tmpData = "";
    var obj = null;
    if (fromModal != 'true') {
        RaiSearchFormCheckHasFilter('form-ric-cess');
        obj = new FormData($('#form-ric-cess')[0]);
    }
    else {
        RaiSearchFormCheckHasFilter('form-ric-extra');
        obj = new FormData($('#form-ric-extra')[0]);
    }
    obj.append('incExtra', extraFunc);
    obj.append('loadHistorySolleciti', visStorico);

    $.ajax({
        url: "/Cessazione/" + actionElenco,
        type: "POST",
        dataType: "html",
        //data: { matricola: matr, nominativo: nome, stato: stato, inCarico: inCarico, dataCessazione: dataCess, sede: sede, causa: causa, soloScrittura: soloScrittura, tipologia: tipologia },
        contentType: false,
        processData: false,
        data: obj,
        success: function (data) {
            tmpData = data;
            $('#ElencoExtra').html(data);
            $("#ElencoExtraExt").removeClass("rai-loader");
        }
    });
}
function IncSaveAnpal() {
    if ($('[data-allegato]').length == 0) {
        swal({
            title: "Attenzione",
            text: 'Non è stato caricato alcun documento',
            type: 'error'
        });
        return false;
    }

    swal({
        title: 'Sei sicuro?',
        html: "Vuoi confermare il caricamento della ricevuta?<br/>NOTA BENE: Una volta confermato, non sarà più possibile modificare i dati.",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var idDip = $('#Pratica_ID_DIPENDENTE').val();
        var dataAnpal = $('#Pratica_DATA_RECESSO_ANPAL').val();

        $.ajax({
            url: "/Cessazione/RicevutaAnpal",
            type: "POST",
            dataType: "html",
            data: { idDip: idDip, dataInvio: dataAnpal },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'OK',
                            text: "Operazione effettuata con successo",
                            type: 'success',
                        });
                        RefreshIncentivato(idDip);
                        UpdateExtra();
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            },
            error: function (a, b, c) {
                swal("Oops...", c, 'error');
            }
        });
    })
}

function IncTessContr(conferma, oper) {
    event.preventDefault();
    if (conferma) {
        let notHasContr = $('#tess-has').prop('checked');
        let notHasMat = $('#mat-has').prop('checked');
        let notHasQual = $('#qual-has').prop('checked');

        if (!notHasContr) {
            let lenAll = $('#wdgt-allegati883 [data-allegato]').length;
            if (lenAll == 0) {
                swal({
                    title: "Attenzione",
                    text: 'Non è stato caricato alcun documento',
                    type: 'error'
                });
                return;
            }
        }

        if (!notHasMat) {
            let lenAll = $('#wdgt-allegati882 [data-allegato]').length;
            if (lenAll == 0) {
                swal({
                    title: "Attenzione",
                    text: 'Non è stato caricato alcun documento',
                    type: 'error'
                });
                return;
            }
        }

        if (!notHasQual) {
            let lenAll = $('#wdgt-allegati881 [data-allegato]').length;
            if (lenAll == 0) {
                swal({
                    title: "Attenzione",
                    text: 'Non è stato caricato alcun documento',
                    type: 'error'
                });
                return;
            }
        }
    } else if (oper == 'mat') {
        //if ($('#notaRichMat').val() == '') {
        //    swal("Attenzione", "Inserisci il motivo della richiesta di approfondimenti", "warning");
        //    $('#notaRichMat').focus();
        //    return false;
        //}
    }

    let title = "Sei sicuro?";
    let message = "";

    if (conferma) {
        message = "Vuoi confermare il caricamento dei dati?<br/>NOTA BENE: Una volta confermato, non sarà più possibile modificare i dati."
    }
    else if (oper == "mat") {
        message = "Vuoi inviare la richiesta di approfondimenti al dipendente?";
    }
    else {
        message = "Nota bene: Il salvataggio dei dati NON renderà disponibile la documentazione."
    }

    swal({
        title: title,
        html: message,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var idDip = $('#Pratica_ID_DIPENDENTE').val();
        var infoTessContr = $('#Pratica_INFO_TESS_CONTR').val();
        //var infoMat = $('#notaRichMat').val();

        var infoMat = $('#Pratica_INFO_MATCON').val();

        var obj = new FormData($('#form-tess-contr')[0]);
        obj.append("_oper", oper);
        obj.append("_conferma", conferma);

        $.ajax({
            url: "/Cessazione/CaricamentoTessereContributive",
            type: "POST",
            dataType: "html",
            contentType: false,
            processData: false,
            //data: { idDip: idDip, infoTessContr: infoTessContr, nota: '', oper: oper, conferma: conferma },
            data: obj,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'OK',
                            text: "Operazione effettuata con successo",
                            type: 'success',
                        });
                        RefreshIncentivato(idDip);
                        UpdateExtra();
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            },
            error: function (a, b, c) {
                swal("Oops...", c, 'error');
            }
        });
    })
}

function IncMatCont(conferma, oper) {
    event.preventDefault();

    if (conferma) {
        let lenAll = $('[data-allegato]').length;
        let annoRif = $('#Pratica_INFO_MATCON').val() || "";

        if (lenAll > 0) {
            var form = $('#form-matcon');
            var validator = $(form).validate();

            if (!$(form).valid()) {
                validator.focusInvalid();
                return false;
            }
        }

        if (annoRif != "" && lenAll == 0) {
            swal({
                title: "Attenzione",
                text: 'Non è stato caricato alcun documento',
                type: 'error'
            });
            return false;
        }
    }

    let title = "Sei sicuro?";
    let message = "";

    if (conferma) {
        message = "Vuoi confermare il caricamento dei dati?<br/>NOTA BENE: Una volta confermato, non sarà più possibile modificare i dati."
    }
    else {
        message = "Nota bene: Il salvataggio dei dati NON renderà disponibile la documentazione."
    }

    swal({
        title: title,
        html: message,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var idDip = $('#Pratica_ID_DIPENDENTE').val();
        var infoMat = $('#Pratica_INFO_MATCON').val();

        $.ajax({
            url: "/Cessazione/CaricamentoMaternita",
            type: "POST",
            dataType: "html",
            data: { idDip: idDip, infoMat: infoMat, conferma: conferma },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'OK',
                            text: "Operazione effettuata con successo",
                            type: 'success',
                        });
                        RefreshIncentivato(idDip);
                        UpdateExtra();
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            },
            error: function (a, b, c) {
                swal("Oops...", c, 'error');
            }
        });
    })
}

function IncConfermaTessMat(idDip) {
    swal({
        title: "Sei sicuro?",
        text: "Vuoi notificare i documenti al dipendente?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/ConfermaTessMat",
            type: "POST",
            dataType: "html",
            data: { idDip: idDip },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal({
                            title: 'OK',
                            text: "Operazione effettuata con successo",
                            type: 'success',
                        });
                        RefreshIncentivato(idDip);
                        UpdateExtra();
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            },
            error: function (a, b, c) {
                swal("Oops...", c, 'error');
            }
        });
    })
}

function FiltroAvanzamento(row, stato, inCarico, tipologia) {
    AzzeraFiltri();
    $('#cmbStato').val(stato);
    $('#cmbInCarico').val(inCarico);
    $('#cmbTipologia').val(tipologia);

    $(row).attr('data-filter', 'selected');

    CercaIncentivato();
}

function AggiornaWidget() {
    AggiornaAvanzamento();
    AggiornaAppuntamenti();
}

function AggiornaAvanzamento() {
    if ($('#Avanzamento').length == 0)
        return;

    RaiUpdateWidget('Avanzamento', '/Cessazione/GetIncentivatiCount', 'html');
}

function AggiornaAppuntamenti() {
    if ($('#Appuntamenti').length == 0)
        return;

    RaiUpdateWidget('Appuntamenti', '/Cessazione/GetCalendarioAppuntamenti', 'html');
}

/**Gestione nota pratica**/
function IncInviaNotaPratica(idDipendente, idNota) {
    event.preventDefault();
    swal({
        title: 'Invio nota',
        text: 'Vuoi inviare la nota al dipendente?',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Invia',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true',
        customClass: 'rai'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/InviaNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idNota: idNota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Invio nota', "Nota inviata con successo", "success");
                        GetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            }
        });
    })
}

function AggiungiNotaPratica(idDipendente, canNotificaDip) {
    let htmlCont = '<p>Inserisci il contenuto della nota</p><textarea tabindex="0" placeholder="Inserisci il contenuto della nota" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important;"></textarea>';
    if (canNotificaDip) {
        htmlCont += '<div class="rai-checkbox push-10-t"><input type="checkbox" id="_tmpDip"><label for="_tmpDip">Da notificare al dipendente</label></div>';
    }

    swal({
        title: 'Aggiungi una nuova nota',
        html: htmlCont,
        //type: 'warning',
        confirmButtonText: 'Salva',
        confirmButtonClass: 'btn btn-primary btn-lg',
        preConfirm: function (value) {
            return new Promise(function (resolve, reject) {
                if ($("#tmp_des").val() == "") {
                    reject("Inserisci il testo della nota");
                }
                else {
                    resolve();
                }
            })
        },
        buttonsStyling: false,
        customClass: 'rai'
    }).then(function (result) {
        let nota = $('#tmp_des').val();
        let inviaDip = canNotificaDip && $('#_tmpDip:checked').length > 0;
        let notTag = '';
        if ($('#openFunction').val() == "TESSCONTR") {
            notTag = "Amministrazione";
        }
        $.ajax({
            url: "/Cessazione/AggiungiNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idDipendente: idDipendente, notaPratica: nota, notificaDip: inviaDip, tag: notTag },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Inserimento nota', "Nota aggiunta con successo", "success");
                        GetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}
function IncModificaNotaPratica(idDipendente, idNota, canNotificaDip) {
    var notaInput = $('span[data-nota="' + idNota + '"]').text().replace("<br/>", "\r\n");
    var notaTag = $('span[data-nota="' + idNota + '"]').attr('data-tag');

    let htmlCont = '<p>Modifica il contenuto della nota</p><textarea tabindex="0" placeholder="Inserisci il contenuto della nota" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important;">' + notaInput + '</textarea>';
    if (canNotificaDip) {
        htmlCont += '<div class="rai-checkbox push-10-t"><input type="checkbox" id="_tmpDip" ' + (notaTag.indexOf("Dipendente") >= 0 ? "checked" : "") + '><label for="_tmpDip">Da notificare al dipendente</label></div>';
    }

    swal({
        title: 'Modifica una nota',
        html: htmlCont,
        //text: "Modifica il contenuto della nota",
        //input: 'textarea',
        //inputValue: notaInput,
        //type: 'warning',
        confirmButtonText: 'Salva',
        confirmButtonClass: 'btn btn-primary btn-lg',
        preConfirm: function (value) {
            return new Promise(function (resolve, reject) {
                if ($("#tmp_des").val() == "") {
                    reject("Inserisci il testo della nota");
                }
                else {
                    resolve();
                }
            })
        },
        buttonsStyling: false,
        customClass: 'rai'
    }).then(function (result) {
        let nota = $('#tmp_des').val();
        let inviaDip = canNotificaDip && $('#_tmpDip:checked').length > 0 || !canNotificaDip && notaTag.indexOf("Dipendente") >= 0;
        $.ajax({
            url: "/Cessazione/ModificaNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idNota: idNota, notaPratica: nota, notificaDip: inviaDip, },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Modifica nota', "Nota modificata con successo", "success");
                        GetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            },
            error: function (a, b, c) {
                swal("Oops...", c, 'error');
            }
        });
    })
}

function CancellazioneNotaPratica(idDipendente, idNota) {
    swal({
        title: 'Sei sicuro?',
        text: "La nota verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/CancellaNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idNotaPratica: idNota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Cancellazione nota', "Nota cancellata con successo", "success");
                        GetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GetNoteDipendenti(idDipendente) {
    let openFunction = $('#openFunction').val();

    $.ajax({
        url: "/Cessazione/GetNotePratica",
        type: "GET",
        dataType: "html",
        data: { idDip: idDipendente, openFunction: openFunction },
        success: function (data) {
            $('#NotePratica').html(data);
            CercaIncentivato();
        }
    });
}

/**Gestione nota stato**/
function AggiungiNotaStato(idOper) {
    swal({
        title: 'Aggiungi una nuova nota',
        text: "Inserisci il contenuto della nota",
        input: 'textarea',
        //type: 'warning',
        confirmButtonText: 'Salva',
        confirmButtonClass: 'btn btn-primary btn-lg',
        preConfirm: function (value) {
            return new Promise(function (resolve, reject) {
                if (value == "") {
                    reject("Inserisci il testo")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false,
        customClass: 'rai'
    }).then(function (result) {
        var nota = result;
        $.ajax({
            url: "/Cessazione/AggiungiNotaStato",
            type: "GET",
            dataType: "html",
            data: { idOper: idOper, notaPratica: nota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Inserimento nota', "Nota aggiunta con successo", "success");
                        GetNoteStato(idOper);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function CancellazioneNotaStato(idOper, idNota) {
    swal({
        title: 'Sei sicuro?',
        text: "La nota verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/CancellaNotaStato",
            type: "GET",
            dataType: "html",
            data: { idNotaStato: idNota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Cancellazione nota', "Nota cancellata con successo", "success");
                        GetNoteStato(idOper);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GetNoteStato(idOper) {
    $.ajax({
        url: "/Cessazione/GetNoteStato",
        type: "GET",
        dataType: "html",
        data: { idOper: idOper },
        success: function (data) {
            $('#NoteStato' + idOper).html(data);
        }
    });
}


/**Gestione stati**/
function Prosegui(idDipendente) {
    //RefreshIncentivato(idDipendente);
    swal("Operazione effettuata con successo", "", "success");
    CercaIncentivato();
    AggiornaAvanzamento();
    $('#modal-dettaglioInc').modal('toggle');
}

function AvviaPratica(idDipendente) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi avviare questa pratica?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var vincoloBCCR = $('#chkVincoloBCCR')[0].checked;
        var causeVertenze = $('#txtGestMan')[0].value;
        var pignoramento = $('#chkPignoramento')[0].checked;
        var estAnticipata = $('#chkEstinzioneAnticipata')[0].checked;
        var cessioneQuinto = $('#chkCessioneQuinto')[0].checked;

        $.ajax({
            url: "/Cessazione/AvviaPratica",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente, vincoloBCCR: vincoloBCCR, causeVertenze: causeVertenze, pignoramento: pignoramento, estAnticipata: estAnticipata, cessioneQuinto: cessioneQuinto },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //per gli incentivi è corretto prendere quelli con stato uno
                        var list = $('[data-id-incentivo][data-stato="1"][data-tipologia="1"]');
                        var index = $(list).index('[data-id-incentivo=' + idDipendente + ']');
                        if (list.length == 1) {
                            swal("Operazione effettuata con successo", "", "success");
                            Prosegui();
                        }
                        else {
                            var test = false;
                            swal({
                                title: '',
                                text: "Operazione effettuata con successo",
                                type: 'success',
                                showCancelButton: true,
                                confirmButtonColor: '#3085d6',
                                cancelButtonColor: '#d33',
                                cancelButtonText: 'Esci',
                                confirmButtonText: 'Prosegui al successivo',
                                reverseButtons: 'true'
                            }).then(function (isConfirm) {
                                var newIndex = index + 1;
                                if (index == list.length - 1)
                                    newIndex = 0;
                                var idIncentivato = $(list[newIndex]).attr('data-id-incentivo');
                                RefreshIncentivato(idIncentivato);
                            }, function (dismiss) {
                                $('#modal-dettaglioInc').modal('toggle');;
                            })
                            CercaIncentivato();
                            AggiornaAvanzamento();
                        }
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function SubmitStatoControllato(idDipendente, idSt, idTip) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi confermare i dati?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var vincoloBCCR = $('#chkVincoloBCCR')[0].checked;
        var causeVertenze = $('#txtGestMan')[0].value;

        $.ajax({
            url: "/Cessazione/SubmitStatoControllato",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente, vincoloBCCR: vincoloBCCR, causeVertenze: causeVertenze },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //Deve prendere quelli con stato precedente al controllato
                        var list = $('[data-id-incentivo][data-stato="' + idSt + '"][data-id-tipologia="' + idTip + '"]');
                        var index = $(list).index('[data-id-incentivo=' + idDipendente + ']');
                        if (list.length == 1) {
                            swal("Operazione effettuata con successo", "", "success");
                            Prosegui();
                        }
                        else {
                            var test = false;
                            swal({
                                title: '',
                                text: "Operazione effettuata con successo",
                                type: 'success',
                                showCancelButton: true,
                                confirmButtonColor: '#3085d6',
                                cancelButtonColor: '#d33',
                                cancelButtonText: 'Esci',
                                confirmButtonText: 'Prosegui al successivo',
                                reverseButtons: 'true'
                            }).then(function (isConfirm) {
                                var newIndex = index + 1;
                                if (index == list.length - 1)
                                    newIndex = 0;
                                var idIncentivato = $(list[newIndex]).attr('data-id-incentivo');
                                RefreshIncentivato(idIncentivato);
                            }, function (dismiss) {
                                $('#modal-dettaglioInc').modal('toggle');;
                            })
                            CercaIncentivato();
                            AggiornaAvanzamento();
                        }
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function PresaInCarico(button, idDipendente, updateDb2, matricola) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi prendere in carico questa pratica?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/PrendiPratica",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //GetNoteDipendenti(idDipendente);
                        //$(button).hide();
                        //$('#lblInCarico').remove();
                        //$('#newwizard').show();
                        RefreshIncentivato(idDipendente);
                        if (updateDb2)
                            CaricaDati(matricola);
                        CercaIncentivato();
                        AggiornaAvanzamento();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function RilasciaPratica(idDipendente) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi rilasciare questa pratica?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/RilasciaPraticaAjax",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //GetNoteDipendenti(idDipendente);
                        //$(button).hide();
                        //$('#lblInCarico').remove();
                        //$('#newwizard').show();
                        RefreshIncentivato(idDipendente);
                        CercaIncentivato();
                        AggiornaAvanzamento();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function SubmitModificaContabili(idDipendente) {
    event.preventDefault();
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi confermare i dati?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        //var obj = $("#form-modificaContabile").serialize();

        var obj = new FormData($('#form-modificaContabile')[0]);

        var vincoloBCCR = false;
        var causeVertenze = '';
        var pignoramento = false;
        var estAnticipata = false;
        var cessioneQuinto = false;
        if ($('#noCheckUffPrest').length > 0) {
            vincoloBCCR = $('#chkVincoloBCCR')[0].checked;
            causeVertenze = $('#txtGestMan')[0].value;
            pignoramento = $('#chkPignoramento')[0].checked;
            estAnticipata = $('#chkEstinzioneAnticipata')[0].checked;
            cessioneQuinto = $('#chkCessioneQuinto')[0].checked;

            obj.append('vincoloBCCR', vincoloBCCR);
            obj.append('causeVertenze', causeVertenze);
            obj.append('pignoramento', pignoramento);
            obj.append('estAnticipata', estAnticipata);
            obj.append('cessioneQuinto', cessioneQuinto);
        }

        $.ajax({
            url: "/Cessazione/ModificaDatiContabili",
            type: "POST",
            cache: false,
            dataType: 'html',
            contentType: false,
            processData: false,
            data: obj,
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function SubmitModificaBozzaVerbale(idDipendente, prosegui) {
    event.preventDefault();

    if (prosegui) {
        var form = $(event.target).parents("form").first();
        var validator = $(form).validate();

        if (!$(form).valid()) {
            validator.focusInvalid();
            return false;
        }

        swal({
            title: 'Sei sicuro?',
            text: "Vuoi confermare i dati?",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sì!',
            cancelButtonText: 'Annulla',
            reverseButtons: 'true'
        }).then(function () {
            InternalSubmitModificaBozzaVerbale(idDipendente, prosegui);
        })
    } else {
        InternalSubmitModificaBozzaVerbale(idDipendente, prosegui);
    }
}
function InternalSubmitModificaBozzaVerbale(idDipendente, prosegui) {
    //var obj = $("#form-modificaBozza").serialize();
    var obj = new FormData($('#form-modificaBozza')[0]);
    obj.append('prosegui', prosegui);
    var async = prosegui;

    $.ajax({
        async: async,
        url: "/Cessazione/ModificaDatiBozza",
        type: "POST",
        cache: false,
        dataType: 'html',
        contentType: false,
        processData: false,
        data: obj,
        success: function (data) {
            switch (data) {
                case "OK":
                    if (prosegui) {
                        RefreshIncentivato(idDipendente);
                        CercaIncentivato();
                        AggiornaAvanzamento();
                        AggiornaAppuntamenti();
                    }
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        }
    });
}

function SubmitModificaAppuntamento(idDipendente, prosegui) {
    debugger
    event.preventDefault();

    if (prosegui) {
        var form = $(event.target).parents("form").first();
        var validator = $(form).validate();

        if (!$(form).valid()) {
            validator.focusInvalid();
            return false;
        }

        swal({
            title: 'Sei sicuro?',
            text: "Vuoi confermare i dati?",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sì!',
            cancelButtonText: 'Annulla',
            reverseButtons: 'true'
        }).then(function () {
            InternalSubmitModificaAppuntamento(idDipendente, prosegui);
        })
    } else {
        InternalSubmitModificaAppuntamento(idDipendente, prosegui);
    }

}
function InternalSubmitModificaAppuntamento(idDipendente, prosegui) {
    //var obj = $("#form-modificaAppuntamento").serialize();
    var obj = new FormData($('#form-modificaAppuntamento')[0]);
    obj.append('prosegui', prosegui);

    var isAsync = true;
    if (!prosegui)
        isAsync = false;

    $.ajax({
        url: "/Cessazione/ModificaDatiAppuntamento",
        async: isAsync,
        type: "POST",
        cache: false,
        dataType: 'html',
        contentType: false,
        processData: false,
        data: obj,
        success: function (data) {
            switch (data) {
                case "OK":
                    //Prosegui(idDipendente);
                    //if ($('#_dataapp').val() != '')
                    //    PromemoriaAppuntamento(idDipendente);
                    RefreshIncentivato(idDipendente);
                    CercaIncentivato();
                    AggiornaAvanzamento();
                    AggiornaAppuntamenti();
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        }
    });
}

function PromemoriaAppuntamento(idDip) {
    swal({
        title: 'Promemoria',
        text: "Vuoi ricevere il promemoria dell'appuntamento?",
        type: 'info',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/CreateICSAppuntamento",
            type: "POST",
            cache: false,
            data: { idDip: idDip },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //Prosegui(idDipendente);
                        swal("Promemoria inviato con successo!", "", "success");
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function AggiungiVerbale(idDip, idOper) {
    if ($("#inputAttachVerbMod").val() == '')
        return;

    var obj = $("#inputAttachVerbMod")[0].files[0];

    if (obj.type != 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
        swal({
            title: "Errore caricamento file",
            text: "Il file deve essere un documento Word",
            type: "error"
        }).then(function () {
            $('#inputAttachVerbMod').click();
        });
        return;
    }

    swal({
        title: 'Il file è stato caricato correttamente',
        text: "Inserisci una breve descrizione",
        html: '<p>Inserisci una breve descrizione</p><textarea tabindex="0" placeholder="Inserisci una breve descrizione" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important;"/>',
        //input: 'text',
        //type: 'warning',
        //showCancelButton: true,
        //confirmButtonColor: '#3085d6',
        //cancelButtonColor: '#d33',
        confirmButtonText: 'Salva',
        //cancelButtonText: 'No, cancel!',
        confirmButtonClass: 'btn btn-primary btn-lg',
        //cancelButtonClass: 'btn btn-danger',
        preConfirm: function () {
            return new Promise(function (resolve, reject) {
                if ($("#tmp_des").val() == "") {
                    reject("Inserisci una breve descrizione")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        var item = result;
        debugger
        var currentdate = new Date();

        var item = 'Verbale modificato';
        var desc = $("#tmp_des").val();

        var formData = new FormData();
        formData.append('idOper', idOper);
        formData.append('_fileUpload', obj);
        formData.append('filename', obj.name);
        formData.append('descr', desc);
        $.ajax({
            url: "/Cessazione/AggiungiVerbaleModificato",
            type: "POST",
            cache: false,
            dataType: 'html',
            contentType: false,
            processData: false,
            data: formData,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal("Verbale caricato", "Il verbale è stato caricato correttamente", 'success')
                        $("#inputAttachVerbMod").val('');
                        RefreshIncentivato(idDip);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function TableCronologia(data, idOper, idDip) {
    var table = '<table class="table table-hover table-responsive" style="font-size:14px;text-align:left;"><thead><tr><th>Data</th><th>Nominativo</th><th>Descrizione</th><th></th><th></th></thead><tbody>';
    for (var i = 0; i < data.length; i++) {
        var rec = data[i];
        table += '<tr><td>' + rec.dataUpload + '</td><td>' + rec.persona + '</td><td>' + rec.descr + '</td><td><a class="btn rai-btn-small" href="/Cessazione/GetDoc?idDoc=' + rec.idVerbale + '">Apri</a></td><td>';
        if (rec.descr.indexOf("Bozza verbale accettata il") < 0)
            table += '<a class="btn rai-btn-small" href="javascript:EliminaDoc(' + idOper + ',' + rec.idVerbale + ',' + idDip + ')">Cancella</a>';
        table += '</td></tr>';
    }
    table += '</tbody></table>';
    return table;
}
function ShowCronologiaVerbali(idDip, idOper) {
    $.ajax({
        url: "/Cessazione/CronologiaVerbali",
        type: "GET",
        dataType: "json",
        data: { idOper: idOper },
        cache: false,
        success: function (data) {
            var table = '<div id="CronologiaVerbali">' + TableCronologia(data, idOper, idDip) + '</div>';

            swal({
                title: 'Elenco modifiche verbali',
                text: "",
                html: table,
                width: 700
            })
        }
    });
}
function EliminaDoc(idOper, idDoc, idDip) {
    swal({
        title: 'Sei sicuro?',
        text: "Se confermi questa versione del verbale verrà eliminata. Vuoi confermare?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/EliminaDoc",
            type: "GET",
            dataType: "json",
            data: { idoper: idOper, idDoc: idDoc },
            cache: false,
            success: function (data) {
                if (data.length == 0) {
                    swal("Verbale eliminato", "Il verbale è stato eliminato correttamente", 'success')
                    RefreshIncentivato(idDip);
                    //swal.close();
                }
                else {
                    swal({
                        title: "Verbale eliminato",
                        text: "Il verbale è stato eliminato correttamente",
                        type: 'success'
                    }).then(function (isConfirm) {
                        ShowCronologiaVerbali(idDip, idOper);
                    }, function (isDismiss) {
                        ShowCronologiaVerbali(idDip, idOper);
                    })
                }
            },
            error: function () {
                swal("Ops...", "Si è verificato un errore durante la cancellazione del verbale", 'error')
            }
        });
    })
}

function VerbaleSelezionato(cognome, nome) {
    var obj = $("#inputAttachFile")[0].files[0];

    if (obj.type != 'application/pdf') {
        swal({
            title: "Errore caricamento file",
            text: "Il file deve essere un pdf",
            type: "error"
        }).then(function () {
            $('#inputAttachFile').click();
        });
        return;
    }

    //swal({
    //    title: 'Il file è stato caricato correttamente',
    //    text: "Assegna un nome al documento caricato",
    //    html: '<p>Assegna un nome al documento caricato</p><input tabindex="0" type="text" name="tmp_name"  id="tmp_name" class="form-control formElements"/><br/><p>Inserisci una breve descrizione (facoltativa)</p><textarea tabindex="0" placeholder="Inserisci una breve descrizione" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important;"/>',
    //    //input: 'text',
    //    //type: 'warning',
    //    //showCancelButton: true,
    //    //confirmButtonColor: '#3085d6',
    //    //cancelButtonColor: '#d33',
    //    confirmButtonText: 'Salva',
    //    //cancelButtonText: 'No, cancel!',
    //    confirmButtonClass: 'btn btn-primary btn-lg',
    //    //cancelButtonClass: 'btn btn-danger',
    //    preConfirm: function () {
    //        return new Promise(function (resolve, reject) {
    //            if ($("#tmp_name").val() == "") {
    //                reject("Inserisci il nome dell'allegato")
    //            }
    //            else if ($("#tmp_name").val().length > 100) {
    //                reject("Il nome può essere lungo al massimo 100 caratteri")
    //            }
    //            else {
    //                resolve()
    //            }
    //        })
    //    },
    //    buttonsStyling: false
    //}).then(function (result) {
    //var item = result;

    var currentdate = new Date();

    var item = "Verbale " + cognome + " " + nome; //$("#tmp_name").val();
    var desc = "Acquisito il " + currentdate.getDate() + "/" + (currentdate.getMonth() + 1 < 10 ? "0" + (currentdate.getMonth() + 1) : (currentdate.getMonth() + 1)) + "/" + currentdate.getFullYear(); //$("#tmp_des").val();
    $('#lblNomeFile').text(item);
    $('#lblDescFile').text(desc);

    $('#iAddFile').hide();
    $('#lblAddFile').hide();

    $('#iAddedFile').show();
    $('#hrefAddedFile').show();
    $('#hrefCanc').show();

    $('#spanAddFile').removeClass('cursor-pointer');

    $('#boxFile').css("border-style", "solid");
    $('#btnSubmitVerbale').removeClass('disable');
    //})

}

function PreviewPDF() {
    var obj = $("#inputAttachFile")[0].files[0];
    var pdffile_url = URL.createObjectURL(obj);
    window.open(pdffile_url, '_blank');
    URL.revokeObjectURL(obj_url);
}

function ConfermaCancellazioneVerbale() {
    $('#inputAttachFile').val('');

    $('#lblNomeFile').text('');
    $('#lblDescFile').text('');

    $('#iAddFile').show();
    $('#lblAddFile').show();
    $('#spanAddFile').addClass('cursor-pointer');

    $('#iAddedFile').hide();
    $('#hrefAddedFile').hide();
    $('#hrefCanc').hide();

    $('#boxFile').css("border-style", "dashed");
}

function SubmitModificaVerbaleFirma(idDip) {
    event.preventDefault();
    swal({
        title: 'Sei sicuro?',
        text: "Se confermi i dati non sarà più possibile modificare i dati dell'appuntamento. Vuoi confermare?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/ModificaDatiVerbaleFirma",
            type: "POST",
            cache: false,
            data: { idDip: idDip },
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDip);
                        AggiornaAppuntamenti();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}
function VerbaleRifiutato(idDip) {
    event.preventDefault();
    swal({
        title: 'Sei sicuro?',
        text: "Se confermi, dovranno essere rieffettuati i conteggi del TFR. Vuoi confermare?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/VerbaleRifiutato",
            type: "POST",
            cache: false,
            data: { idDip: idDip },
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDip);
                        AggiornaAppuntamenti();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function SubmitModificaVerbale(idDip) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi caricare il verbale inserito?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var obj = $("#inputAttachFile")[0].files[0];
        var formData = new FormData();
        formData.append('idDip', idDip);
        formData.append('_fileUpload', obj);
        formData.append('filename', $('#lblNomeFile').text());
        formData.append('descr', $('#lblDescFile').text());
        $.ajax({
            url: "/Cessazione/ModificaDatiVerbale",
            type: "POST",
            cache: false,
            dataType: 'html',
            contentType: false,
            processData: false,
            data: formData,
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDip);
                        //AggiornaAppuntamenti();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function SubmitModificaPagamento(idDipendente) {
    event.preventDefault();
    swal({
        title: 'Sei sicuro?',
        text: "Confermi i dati di pagamento?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        var obj = $("#form-modificaPagamento").serialize();
        $.ajax({
            url: "/Cessazione/ModificaDatiPagamento",
            type: "POST",
            cache: false,
            data: obj,
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

/**Gestione combo rappresentanti sindacali**/
function PopolaRapprSind(sede) {
    var idSind = $('#ID_SIGLASIND').val();

    $.ajax({
        url: "/Cessazione/getAJAXRapprSind",
        type: "GET",
        dataType: "json",
        data: { idSind: idSind, sede: sede },
        cache: false,
        success: function (data) {
            $("#ID_RAPPRSINDACATO").html('<option value="">Rappresentante sindacale</option>');
            for (var i = 0; i < data.result.length; i++) {
                $("#ID_RAPPRSINDACATO").append('<option value="' + data.result[i].Value + '" ' + (data.result.length == 1 ? 'selected' : '') + '>' + data.result[i].Text + '</option>');
            }
            $('#ID_RAPPRSINDACATO').removeClass("disable");
        },
        error: function () { }
    });
}

function CaricaDati(matricola) {
    $.ajax({
        url: "/Cessazione/CaricaDatiDB2",
        type: "GET",
        dataType: "json",
        data: { matricola: matricola },
        cache: false,
        success: function (data) {
            if (data.dtreg == undefined || data.dtreg == '') {
                $('#alertMessage').removeClass("alert-success");
                $('#alertMessage').addClass("alert-danger");
                $('#alertMessage').text("Dati non disponibili");
                $('#alertMessage').show();
                $('#btnSalvaDatiContabili').removeClass('disable');
            }
            else {
                var tmpIncentivo = $('#INCENTIVO_LORDO').val();
                if (parseFloat(tmpIncentivo) != parseFloat(data.incen)) {
                    swal({
                        title: 'Attenzione',
                        html: "L'incentivo lordo risulta differente da quello originale. Vuoi sovrascriverlo?<br/>Incentivo lordo originale: <b>" + tmpIncentivo + "</b><br/>Incentivo lordo FFRA: <b>" + data.incen + '</b>',
                        type: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Sì!',
                        cancelButtonText: 'Annulla',
                        reverseButtons: 'true'
                    }).then(function () {
                        $('#INCENTIVO_LORDO').val(data.incen);
                        $('#_incentivo_lordo').val(parseFloat(data.incen.replace(',', '.')).toLocaleString("it-IT", { style: "currency", currency: "EUR", minimumFractionDigits: 2 }));
                    });
                }

                $('#IMPORTO_NETTO').val(data.netto);
                $('#_importo_netto').val(parseFloat(data.netto.replace(',', '.')).toLocaleString("it-IT", { style: "currency", currency: "EUR", minimumFractionDigits: 2 }));;

                $('#IMPORTO_LORDO').val(data.lordo);
                $('#_importo_lordo').val(parseFloat(data.lordo.replace(',', '.')).toLocaleString("it-IT", { style: "currency", currency: "EUR", minimumFractionDigits: 2 }));;

                $('#lblLastUpdate').text("Aggiornati al " + data.dtreg);
                $('#alertMessage').removeClass("alert-danger");
                $('#alertMessage').addClass("alert-success");
                $('#alertMessage').text("Dati caricati con successo");
                $('#alertMessage').show();
                $('#btnSalvaDatiContabili').removeClass('disable');
                $('#btnVisualizzaProspetto').removeClass('disable');
            }
            $('#btnSalvaDatiContabili').removeClass('disable');
        },
        error: function () { }
    });
}

function CaricaDatiBanca(idDip) {
    var scelta = $('#IND_PROPRIO_IBAN').val();
    if (scelta == 'Y' || scelta == 'V' || scelta == 'C') {
        $.ajax({
            url: "/Cessazione/CaricaDatiIBAN",
            type: "GET",
            dataType: "json",
            data: { idDip: idDip, sceltaConto: scelta },
            cache: false,
            success: function (data) {
                $('#BANCA').addClass('disable');
                $('#INTESTATARIO_CONTO').addClass('disable');
                $('#IBAN').addClass('disable');
                $('#BANCA').val(data.banca);
                $('#IBAN').val(data.iban);
                $('#INTESTATARIO_CONTO').val(data.intestatario);
            },
            error: function () { }
        });
    }
    else if (scelta == "N") {
        $('#BANCA').val('');
        $('#IBAN').val('');
        $('#INTESTATARIO_CONTO').val('');

        $('#BANCA').removeClass('disable');
        $('#IBAN').removeClass('disable');
        $('#INTESTATARIO_CONTO').removeClass('disable');
    }
    else {
        $('#BANCA').addClass('disable');
        $('#IBAN').addClass('disable');
        $('#INTESTATARIO_CONTO').addClass('disable');

        $('#BANCA').val('');
        $('#IBAN').val('');
        $('#INTESTATARIO_CONTO').val('');
    }

}

function LookForKeyDown() {
    if (event.keyCode == 13) {
        CercaIncentivato();
    }
}

function EliminaStato(idDipendente, idOper, message) {
    swal({
        title: 'Sei sicuro?',
        text: message,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {

        $.ajax({
            url: "/Cessazione/EliminaStato",
            type: "POST",
            data: { idOper: idOper },
            cache: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        RefreshIncentivato(idDipendente);
                        CercaIncentivato();
                        AggiornaAvanzamento();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    });
}

function InvalidaStato(idDip, idOper) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi portare la pratica allo stato precedente?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        RilasciaPratica(idDip);
        $.ajax({
            url: "/Cessazione/InvalidaStato",
            type: "POST",
            data: { idOper: idOper },
            cache: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        Prosegui(idDip);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    });
}

function setBorderRadius(accLink, rifNum) {
    if ($(accLink).attr("aria-expanded") == "false") {
        $('#headerCollapse' + rifNum).css("border-radius", "5px 5px 0 0");
    }
    else {
        $('#headerCollapse' + rifNum).css("border-radius", " 5px 5px 5px 5px");
    }
}

function IncApprovazioneRich(idDip, prosegui, state, oper) {
    event.preventDefault();

    var text = "";
    if (prosegui) {
        if (state)
            text = "Vuoi convalidare la richiesta?";
        else {
            if (oper == 'd')
                text = "Vuoi far decadera la pratica?";
            else
                text = "Vuoi rifiutare la richiesta?";
        }
    } else {
        if (oper == 'tesscontr')
            text = "Vuoi inviare la richiesta di approfondimenti?";
        else if (oper == 'update_tesscontr')
            text = "Vuoi aggiornare la richiesta di approfondimenti?";
        else
            text = "Nota bene: Il salvataggio dei dati NON avvierà la pratica.";
    }

    if (prosegui && state) {
        var form = $('#form-accettazioneRichiesta');
        var validator = $(form).validate();

        if (!$(form).valid()) {
            validator.focusInvalid();
            return false;
        }
    } else if (!prosegui && (oper == 'tesscontr' || oper == 'update_tesscontr')) {
        let testNota = '';
        if ($('#notaRichTessContr').length > 0)
            testNota = $('#notaRichTessContr').val();
        else
            testNota = $('#Pratica_NOT_RICH_TESS_CONTR').val();

        if (testNota == '') {
            swal("Attenzione", "Inserisci il motivo della richiesta di approfondimenti", "warning");
            $('#notaRichTessContr').focus();
            return false;
        }
    }

    swal({
        title: 'Sei sicuro?',
        text: text,
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        if (prosegui && !state) {
            var text = 'Inserisci il motivo del rifiuto';
            if (oper == 'd')
                text = "Inserisci il motivo";
            swal({
                //title: '',
                //type: '',
                text: text,
                input: 'textarea',
                showCancelButton: true,
                confirmButtonText: 'Conferma',
                cancelButtonText: 'Annulla',
                reverseButtons: 'true',
                preConfirm: function (value) {
                    return new Promise(function (resolve, reject) {
                        if (value == "") {
                            reject("Inserisci il testo della nota")
                        }
                        else {
                            resolve()
                        }
                    })
                },
            }).then(function (result) {
                var nota = result;
                InternalIncApprovazioneRich(prosegui, state, nota, oper);
            })
        }
        else {
            InternalIncApprovazioneRich(prosegui, state, '', oper);
        }
    });
}
function InternalIncApprovazioneRich(prosegui, state, nota, oper) {
    if (oper == "tesscontr") {
        $('#Pratica_DATA_RICH_TESS_CONTR').val(moment().format("DD/MM/YYYY HH:mm"));
        nota = $('#notaRichTessContr').val();
    } else if (oper == 'update_tesscontr') {
        nota = $('#Pratica_NOT_RICH_TESS_CONTR').val();
    }

    var obj = new FormData($('#form-accettazioneRichiesta')[0]);
    obj.append('prosegui', prosegui);
    obj.append('_stato', state);
    obj.append('_nota', nota);
    obj.append('_oper', oper)

    $.ajax({
        url: "/Cessazione/IncApprovazioneRichiesta",
        type: "POST",
        cache: false,
        dataType: 'html',
        contentType: false,
        processData: false,
        data: obj,
        success: function (data) {
            switch (data) {
                case "OK":
                    var alertMsg = '';
                    if (prosegui) {
                        if (state) {
                            alertMsg = 'Pratica verificata con successo';
                        } else {
                            if (oper == 'd') {
                                alertMsg = "Pratica decaduta";
                            }
                            else {
                                alertMsg = "Pratica rifiutata";
                            }
                        }
                    } else {
                        if (oper == 'tesscontr')
                            alertMsg = "Richiesta inviata con successo";
                        else
                            alertMsg = "Pratica salvata con successo";
                    }
                    swal("OK", alertMsg, 'success');

                    if (prosegui) {
                        var idDip = $('#Pratica_ID_DIPENDENTE').val();
                        Prosegui(idDip);
                        RaiUpdateWidget("wdgt-costi", "/Cessazione/Widget_costi", 'html');
                    } else if (oper == 'tesscontr') {

                        $('#richTessContr').remove();
                        $('#richDoneTessContr').show();
                        $('#Pratica_NOT_RICH_TESS_CONTR').val(nota);
                    }

                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        }
    });
}

function SubmitAvviaPratica(prosegui, stato, oper, showAlert, showInvioMail) {
    event.preventDefault();

    if (showAlert == undefined || showAlert == null)
        showAlert = true;

    if (showInvioMail == undefined || showInvioMail == null)
        showInvioMail = false;

    if (prosegui && stato) {
        var form = $('#form-avvioPratica');
        var validator = $(form).validate();

        if (!$(form).valid()) {
            validator.focusInvalid();
            return false;
        }
    }

    var alertText = "Confermi di voler avviare la pratica?";
    if (!prosegui) {
        alertText = "Nota bene: Il salvataggio dei dati NON avvierà la pratica.";
    } else {
        if (!stato) {
            if (oper == 'd') {
                alertText = "Confermi di voler far decadere la pratica?";
            } else if (oper = 'pr') {
                alertText = 'Il dipendente ha rifiutato la proposta?';
            } else {
                alertText = "Confermi di voler riufiutare la pratica?";
            }
        }
        else {
            if (showInvioMail)
                alertText += '<br/><div class="text-center"><div class="rai-checkbox push-10-t" style="display:inline-flex;"><input type="checkbox" id="_tmpDip" checked="checked"><label for="_tmpDip">Notifica al dipendente</label></div></div>';
        }
    }

    if (showAlert) {
        swal({
            title: 'Sei sicuro?',
            html: alertText,
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Sì!',
            cancelButtonText: 'Annulla',
            reverseButtons: 'true'
        }).then(function () {
            if (prosegui && !stato) {
                var text = 'Inserisci il motivo del rifiuto';
                if (oper == 'd') {
                    text = 'Inserisci il motivo';
                } else if (oper == 'pr') {
                    text = 'Inserisci una nota';
                }
                swal({
                    //title: '',
                    //type: '',
                    text: text,
                    input: 'textarea',
                    showCancelButton: true,
                    confirmButtonText: 'Conferma',
                    cancelButtonText: 'Annulla',
                    reverseButtons: 'true',
                    preConfirm: function (value) {
                        return new Promise(function (resolve, reject) {
                            if (value == "") {
                                reject("Inserisci il testo della nota")
                            }
                            else {
                                resolve()
                            }
                        })
                    },
                }).then(function (result) {
                    var nota = result;
                    InternalSubmitAvviaPratica(prosegui, stato, nota, showAlert, oper, false);
                })
            } else {
                let _inviaMail = false;
                if (prosegui && stato) {
                    _inviaMail = $('#_tmpDip:checked').length > 0;
                }

                InternalSubmitAvviaPratica(prosegui, stato, '', showAlert, oper, _inviaMail);
            }

        });
    }
    else {
        InternalSubmitAvviaPratica(prosegui, stato, '', showAlert, oper, false);
    }

}
function InternalSubmitAvviaPratica(prosegui, stato, nota, showSuccess, oper, _inviaMail) {
    var obj = new FormData($('#form-avvioPratica')[0]);
    obj.append('prosegui', prosegui);
    obj.append('_stato', stato);
    obj.append('_nota', nota);
    obj.append('_oper', oper);
    obj.append('_inviaMail', _inviaMail);

    obj.set("Pratica.INCENTIVO_LORDO", $('#Pratica_INCENTIVO_LORDO').val().replace('.', ''));
    obj.set("Pratica.UNA_TANTUM_LORDA", $('#Pratica_UNA_TANTUM_LORDA').val().replace('.', ''));
    obj.set("Pratica.EX_FISSA", $('#Pratica_EX_FISSA').val().replace('.', ''));

    obj.set("Pratica.TFR_LORDO_INPS_IP", $('#Pratica_TFR_LORDO_INPS_IP').val().replace('.', ''));
    obj.set("Pratica.TFR_LORDO_AZ_IP", $('#Pratica_TFR_LORDO_AZ_IP').val().replace('.', ''));
    obj.set("Pratica.TFR_NETTO", $('#Pratica_TFR_NETTO').val().replace('.', ''));

    $.ajax({
        url: "/Cessazione/ModificaDatiAvviaPratica",
        type: "POST",
        cache: false,
        dataType: 'html',
        contentType: false,
        processData: false,
        data: obj,
        beforeSend: function () {
            $('#form-avvioPratica').addClass('rai-loader');
        },
        success: function (data) {
            switch (data) {
                case "OK":
                    var alertMsg = "";
                    if (prosegui) {
                        var idDipendente = $('#Pratica_ID_DIPENDENTE').val();
                        Prosegui(idDipendente);
                        if (stato) {
                            alertMsg = 'Pratica avviata con successo';
                        } else {
                            if (oper == 'd') {
                                alertMsg = "Pratica decaduta";
                            }
                            else if (oper == 'pr') {
                                alertMsg = "Proposta rifiutata";
                            }
                            else {
                                alertMsg = "Pratica rifiutata con successo";
                            }
                        }
                    } else {
                        alertMsg = "Pratica salvata con successo";
                    }

                    RaiUpdateWidget("wdgt-costi", "/Cessazione/Widget_costi", 'html');

                    if (showSuccess)
                        swal("OK", alertMsg, 'success');
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        },
        complete: function () {
            $('#form-avvioPratica').removeClass('rai-loader');
        }
    });
}

function IncEliminaGenericDoc(idDoc) {
    swal({
        title: 'Sei sicuro?',
        text: "Se confermi questo file verrà eliminato. Vuoi confermare?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/Cessazione/EliminaGenericDoc",
            type: "GET",
            dataType: "html",
            data: { idDoc: idDoc },
            cache: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal("File eliminato", "Il file è stato eliminato correttamente", 'success');
                        debugger
                        $('#allegato' + idDoc).remove();
                        break;
                    default:
                        swal("Ops...", data, 'error');
                        break;
                }
            },
            error: function () {
                swal("Ops...", "Si è verificato un errore durante la cancellazione del file", 'error');
            }
        });
    })
}
function CessFileChanged(i, stato) {
    debugger
    var nomefile = ($("#fileupload" + i + '-' + stato).val().split("\\").pop())

    $('#nome-file' + i + '-' + stato).text(nomefile);

    $("#uploading" + i + '-' + stato).show();
    var formdata = new FormData();
    formdata.append('idDip', $('#Pratica_ID_DIPENDENTE').val())
    formdata.append('_file', $('#fileupload' + i + '-' + stato)[0].files[0]);
    formdata.append("fileName", nomefile);
    formdata.append("tipologia", $('#fileupload' + i + '-' + stato).attr("data-tipo"));
    formdata.append('stato', stato);
    if ($('#tag' + i + '-' + stato).length > 0)
        formdata.append('tags', $('#tag' + i + '-' + stato).val());

    if ($("#titolodoc" + i + '-' + stato).length > 0) {
        formdata.append("titolo", $("#titolodoc" + i + '-' + stato).val());
    }
    if ($("#descrizionedoc" + i + '-' + stato).length > 0) {
        formdata.append("desc", $("#descrizionedoc" + i + '-' + stato).val());
    }
    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        debugger
        if (this.readyState == 4 && this.status == 200) {
            if (this.responseText.indexOf("E") >= 0)
                swal(this.responseText);
            else {
                debugger
                $("#id-allegato-" + i + '-' + stato).val(this.responseText);
                $("#button-allegato-" + i + '-' + stato).addClass("disable");
                $("#nome-file" + i + '-' + stato).text(nomefile);
                RaiUpdateWidget('wdgt-allegati' + stato, '/Cessazione/Widget_allegati', 'html', { idDip: $('#Pratica_ID_DIPENDENTE').val(), stato: stato });
            }
        }
    };
    request.upload.addEventListener('progress', function (e) {

        var filesize = $('#fileupload' + i + '-' + stato)[0].files[0].size;
        $("#total" + i + '-' + stato).text(parseInt(filesize / 1000) + " KB");
        $("#loaded" + i + '-' + stato).text(parseInt(e.loaded / 1000) + " KB");
        var percent = Math.round(e.loaded / filesize * 100);
        $("#progress-bar" + i + '-' + stato).css("width", percent + "%");

        if (e.loaded >= filesize) {
            $("#rimuovi" + i + '-' + stato).show();
        }
        else
            $("#rimuovi" + i + '-' + stato).hide();

    });

    request.open('post', '/cessazione/UploadFile');
    request.timeout = 45000;
    request.send(formdata);
}
function CessDettFileChanged(idDip, i, stato) {
    debugger
    var nomefile = ($("#fileupload" + i + '-' + stato).val().split("\\").pop())

    $("#uploading" + i + '-' + stato).show();
    var formdata = new FormData();
    formdata.append('idDip', idDip)
    formdata.append('_file', $('#fileupload' + i + '-' + stato)[0].files[0]);
    formdata.append("fileName", nomefile);
    formdata.append("tipologia", $('#fileupload' + i + '-' + stato).attr("data-tipo"));
    formdata.append('stato', stato);
    if ($('#tag' + i + '-' + stato).length > 0)
        formdata.append('tags', $('#tag' + i + '-' + stato).val());

    if ($("#titolodoc" + i + '-' + stato).length > 0) {
        formdata.append("titolo", $("#titolodoc" + i + '-' + stato).val());
    }
    if ($("#descrizionedoc" + i + '-' + stato).length > 0) {
        formdata.append("desc", $("#descrizionedoc" + i + '-' + stato).val());
    }
    var request = new XMLHttpRequest();
    request.onreadystatechange = function () {
        debugger
        if (this.readyState == 4 && this.status == 200) {
            if (this.responseText.indexOf("E") >= 0)
                swal(this.responseText);
            else {
                debugger
                $("#id-allegato-" + i + '-' + stato).val(this.responseText);
                $("#button-allegato-" + i + '-' + stato).addClass("disable");
                $("#nome-file" + i + '-' + stato).text(nomefile);
                RaiUpdateWidget('dett-all-' + stato, '/Cessazione/Dettaglio_OperAllegati', 'html', { idDip: idDip, stato: stato });
            }
        }
    };
    request.upload.addEventListener('progress', function (e) {

        var filesize = $('#fileupload' + i + '-' + stato)[0].files[0].size;
        $("#total" + i + '-' + stato).text(parseInt(filesize / 1000) + " KB");
        $("#loaded" + i + '-' + stato).text(parseInt(e.loaded / 1000) + " KB");
        var percent = Math.round(e.loaded / filesize * 100);
        $("#progress-bar" + i + '-' + stato).css("width", percent + "%");

        if (e.loaded >= filesize) {
            $("#rimuovi" + i + '-' + stato).show();
        }
        else
            $("#rimuovi" + i + '-' + stato).hide();

    });

    request.open('post', '/cessazione/UploadFile');
    request.timeout = 45000;
    request.send(formdata);
}
function IncOpenModalCosti() {
    RaiOpenAsyncModal('modal-costi', '/Cessazione/Modal_costi', null, null, 'POST');
}

function IncModalExtra(extra) {
    RaiOpenAsyncModal('modal-incextra', '/Cessazione/Modal_Extra', { extra: extra }, UpdateExtra, 'POST');
}
function UpdateExtra() {
    IncCercaExtra();
}

function IncFileApprove(key, stato) {
    event.preventDefault();
    var text = 'Approvi il documento inserito dal dipendente?';
    swal({
        title: 'Sei sicuro?',
        //type: '',
        text: text,
        //input: 'textarea',
        showCancelButton: true,
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true',
        preConfirm: function (value) {
            return new Promise(function (resolve, reject) {
                if (value == "") {
                    reject("Inserisci il testo della nota")
                }
                else {
                    resolve()
                }
            })
        },
    }).then(function (result) {
        debugger
        var nota = result;
        $.ajax({
            url: "/Cessazione/ApprovaFileDip",
            type: "GET",
            data: { key: key, idStato: stato },
            async: true,
            cache: false,
            success: function (data) {
                RaiUpdateWidget('wdgt-allegati' + stato, '/Cessazione/Widget_allegati', 'html', { idDip: $('#Pratica_ID_DIPENDENTE').val(), stato: stato });
            }
        })
    })
}
function IncFileDecline(key, stato, showTipo) {
    event.preventDefault();
    var text = 'Inserisci il motivo del rifiuto';

    let htmlT = '<div class="form-group text-left">';

    if (showTipo) {
        htmlT = htmlT + '	<div class="push-10">' +
            '		<label class="rai-caption">Tipo rifiuto<font color="#d2322d">*</font></label><br/>' +
            '		<div class="rai-radio">' +
            '			<input type="radio" id="dIncongruo" name="tmpTipoRifiuto" value="I" required/>' +
            '			<label for="dIncongruo">Documentazione incongrua</label>' +
            '		</div>' +
            '		<div class="rai-radio">' +
            '			<input type="radio" id="dParziale" name="tmpTipoRifiuto" value="P" required/>' +
            '			<label for="dParziale">Documentazione parziale</label>' +
            '		</div>' +
            '	</div>';
    }
    htmlT = htmlT + '	<div> ' +
        '		<label class="rai-caption">Motivo rifiuto<font color="#d2322d">*</font></label><br/>' +
        '		<textarea rows="4" id="dMotivo" class="form-control" required="required"></textarea>' +
        '	</div>' +
        '</div>';

    swal({
        title: 'Sei sicuro?',
        //type: '',
        //text: text,
        html: htmlT,
        showCancelButton: true,
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true',
        preConfirm: function () {
            return new Promise(function (resolve, reject) {
                if (showTipo && $('input[type="radio"][name="tmpTipoRifiuto"]:checked').length <= 0) {
                    reject("Scegli il tipo di rifiuto");
                }
                else if ($('#dMotivo').val() == '') {
                    reject("Inserisci il testo della nota");
                }
                else {
                    resolve();
                }
            })
        },
    }).then(function (result) {
        debugger
        var nota = $('#dMotivo').val();
        var tipoRifiuto = showTipo ? $('input[type="radio"][name="tmpTipoRifiuto"]:checked').val() : "I";
        $.ajax({
            url: "/Cessazione/RifiutaFileDip",
            type: "GET",
            data: {
                key: key, nota: nota, tipoRifiuto: tipoRifiuto
            },
            async: true,
            cache: false,
            success: function (data) {
                if (data === "OK") {
                    swal("File rifiutato correttamente", "", 'success');
                    RaiUpdateWidget('wdgt-allegati' + stato, '/Cessazione/Widget_allegati', 'html', { idDip: $('#Pratica_ID_DIPENDENTE').val(), stato: stato });
                }
                else if (data === "UPDATE") {
                    swal("File rifiutato correttamente", "", 'success');
                    ShowIncentivato($('#Pratica_ID_DIPENDENTE').val(), '');
                }
                else
                    swal(data);
            }
        })
    })
}
// #endregion


// #region Politiche Retributive

/**Gestione form di ricerca**/
function GestCheckHasFilter(formID) {
    $('#' + formID + ' > #HasFilter').val('false');
    $('.' + formID + '.form-control-value').each(function () {
        if ($(this).val() != '') {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
}
function gestPreSubmit(destBox) {
    $("#" + destBox + "Ext").addClass("rai-loader");
    $("#" + destBox).addClass("gest-not-authorized");
}
function gestSuccessSubmit(formId, destBox) {
    $("#" + destBox + "Ext").removeClass("rai-loader");
    $("#" + destBox).removeClass("gest-not-authorized");

    gestSetFilterDescr(formId, destBox);
}
function gestSetFilterDescr(formID, destBox) {
    var hasFilter = $('#' + formID + ' > #HasFilter').val();
    if (hasFilter == 'true') {
        var textFilter = '';
        var jsonString = $('#' + formID).serialize();

        $('#' + formID + ' > #MatricoleMultiple').val('false');

        $.ajax({
            url: "/PoliticheRetributive/GetStringaFiltri",
            type: "GET",
            dataType: "json",
            data: jsonString,
            cache: false,
            success: function (data) {
                var textFixed = "Stai visualizzando le persone ";

                var elem = $('#' + destBox + ' > #SummaryFiltri')
                $(elem).show();
                $(elem).find('#textSummary').text(textFixed);
                $(elem).find('#textSummaryBold').text(data.Filtro);
            }
        });
    }
    else {
        var elem = $('#' + destBox + ' > #SummaryFiltri')
        $(elem).hide();
        $(elem).find('#textSummary').text('');
        $(elem).find('#textSummaryBold').text('');
    }
}

function GestPulisciFiltri(formID, submit) {
    event.preventDefault();

    $('.' + formID + '.form-control-value').val('');
    $('.' + formID + '.form-control-value-int').val('0');
    $('.' + formID + '.form-control-bool').val('false');
    $('.' + formID + '.form-control-bool-true').val('true');

    if (submit)
        $('#' + formID).submit();
}
/**Fine Gestione form di ricerca**/

function PopolaDecorrenzaCampagna() {
    var idSind = $('#SceltaCampagna').val();

    if (idSind && idSind > 1) {
        $.ajax({
            url: "/PoliticheRetributive/getAJAXDecorrenzaCampagna",
            type: "GET",
            dataType: "json",
            data: { idCampagna: idSind },
            cache: false,
            success: function (data) {
                $("#SceltaDecorrenza").html('<option value="">Decorrenza</option>');
                for (var i = 0; i < data.result.length; i++) {
                    $("#SceltaDecorrenza").append('<option value="' + data.result[i].Value + '" ' + (data.result.length == 1 ? 'selected' : '') + '>' + data.result[i].Text + '</option>');
                }
                $('#SceltaDecorrenza').show();
            },
            error: function () { }
        });
    }
    else {
        $('#SceltaDecorrenza').hide();
    }

    $('#form-ricerca-RicercaDipendenti').submit();
}

function GestAggiornaDec() {
    $('#form-ricerca-RicercaDipendenti').submit();
}


function CallCreaPraticaSingola(idProvv, idPersona) {

    $('[data-id-persona="' + idPersona + '"').addClass('disable');
    //$('#link-' + idPersona + '-' + idProvv).addClass("text-primary");
    //$('#link-' + idPersona + '-' + idProvv).parent().removeClass("actions-hover");

    idCampagna = $('#SceltaCampagna').val();
    dataDecorrenza = $('#SceltaDecorrenza').val();


    $('#OPER_' + idPersona).show();

    $.ajax({
        url: "/PoliticheRetributive/CreaPratica",
        type: "POST",
        dataType: "json",
        data: { idCampagna: idCampagna, idPersona: idPersona, provv: idProvv, decorrenza: dataDecorrenza },
        async: true,
        success: function (data) {
            switch (data.result) {
                case "OK":
                    //$('[data-prov="' + idPersona + '"').addClass('disable');
                    //$('#REMOVE_' + idPersona).attr('onclick', 'GestRimuoviPratica('+ data.idPratica + ')');
                    //$('#REMOVE_' + idPersona).show();
                    $('[data-id-persona="' + idPersona + '"').removeClass('disable');
                    //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                    $('#form-ricerca-ElencoDipendenti').submit();
                    $('#OPER_' + idPersona).hide();
                    break;
                default:
                    $('[data-id-persona="' + idPersona + '"').removeClass('disable');
                    $('[data-prov="' + idPersona + '"').removeClass('text-primary');
                    $('[data-prov="' + idPersona + '"').removeClass('disable');
                    $('[data-prov="' + idPersona + '"').parent().addClass('actions-hover');
                    $('#OPER_' + idPersona).hide();
                    swal("Errore", data);
                    break;
            }

        },
        error: function (result) {
            $('[data-id-persona="' + idPersona + '"').removeClass('disable');
            $('[data-prov="' + idPersona + '"').removeClass('text-primary');
            $('[data-prov="' + idPersona + '"').removeClass('disable');
            $('[data-prov="' + idPersona + '"').parent().addClass('actions-hover');
            $('#OPER_' + idPersona).hide();
            swal("Errore", result);

        }
    });
}
function GestRimuoviPersona(idPersona, idDip) {
    swal({
        title: 'Sei sicuro?',
        text: "La pratica verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $('#OPER_' + idPersona).show();
        $.ajax({
            url: "/PoliticheRetributive/CancellaPratica",
            type: "POST",
            data: { idDip: idDip },
            async: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        $('[data-id-persona="' + idPersona + '"').removeClass('disable');
                        $('[data-prov="' + idPersona + '"').removeClass('text-primary');
                        $('[data-prov="' + idPersona + '"').removeClass('disable');
                        $('[data-prov="' + idPersona + '"').parent().addClass('actions-hover');
                        $('#REMOVE_' + idPersona).hide();
                        //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        break;
                    default:
                        swal("Errore: " + data);

                        break;
                }
                $('#OPER_' + idPersona).hide();
            },
            error: function (result) {
                swal("Errore: " + result);
                $('#OPER_' + idPersona).hide();
            }
        });
    })

}
function GestRimuoviPratica(idDip) {
    event.preventDefault();
    swal({
        title: 'Sei sicuro?',
        text: "La pratica verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/PoliticheRetributive/CancellaPratica",
            type: "POST",
            data: { idDip: idDip },
            async: false,
            success: function (data) {
                switch (data) {
                    case "OK":
                        //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        break;
                    default:
                        swal("Errore: " + data);
                        break;
                }
            },
            error: function (result) {
                swal("Errore: " + result);
            }
        });
    })
}

function ShowPratica(idDip) {
    event.preventDefault();
    RaiOpenAsyncModal('modal-pratica', '/PoliticheRetributive/GetDettaglioPratica', { idDip: idDip });
}



function GestShowAnteprima(idDip) {
    event.preventDefault();

    dataDecorrenza = $('#SceltaDecorrenza').val() ? $('#SceltaDecorrenza').val() : "";
    RaiOpenAsyncModal('modal-pratica', '/PoliticheRetributive/GetAnteprimaPratica', { idPersona: idDip, decorrenza: dataDecorrenza });
}

function ShowOverview() {
    $('#modal-overview-internal').html(' ');
    $('#modal-overview-internal').load('/PoliticheRetributive/GetOverview');
    $('#modal-overview').modal('show');
}

/**Gestione nota pratica**/
function GestAggiungiNotaPratica(idDipendente) {
    swal({
        title: 'Aggiungi una nuova nota',
        text: "Inserisci il contenuto della nota",
        input: 'textarea',
        //type: 'warning',
        confirmButtonText: 'Salva',
        confirmButtonClass: 'btn btn-primary btn-lg',
        preConfirm: function (value) {
            return new Promise(function (resolve, reject) {
                if (value == "") {
                    reject("Inserisci il testo")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        var nota = result;
        $.ajax({
            url: "/PoliticheRetributive/AggiungiNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idDipendente: idDipendente, notaPratica: nota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Inserimento nota', "Nota aggiunta con successo", "success");
                        GestGetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GestCancellazioneNotaPratica(idDipendente, idNota) {
    swal({
        title: 'Sei sicuro?',
        text: "La nota verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/PoliticheRetributive/CancellaNotaPratica",
            type: "GET",
            dataType: "html",
            data: { idNotaPratica: idNota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Cancellazione nota', "Nota cancellata con successo", "success");
                        GestGetNoteDipendenti(idDipendente);
                        break;
                    default:
                        swal("Oops...", data, 'error');
                }
            }
        });
    })
}

function GestGetNoteDipendenti(idDipendente) {
    $.ajax({
        url: "/PoliticheRetributive/GetNotePratica",
        type: "GET",
        dataType: "html",
        data: { idDip: idDipendente },
        success: function (data) {
            $('#NotePratica').html(data);
            //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
            $('#form-ricerca-ElencoDipendenti').submit();
        }
    });
}
/**Fine gestione nota pratica**/

function GestPresaInCarico(button, idDipendente, matricola) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi prendere in carico questa pratica?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/PoliticheRetributive/PrendiPratica",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente },
            success: function (data) {
                switch (data) {
                    case "OK":
                        ShowPratica(idDipendente);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GestRilasciaPratica(idDipendente) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi rilasciare questa pratica?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/PoliticheRetributive/RilasciaPraticaAjax",
            type: "GET",
            dataType: "html",
            data: { idDip: idDipendente },
            success: function (data) {
                switch (data) {
                    case "OK":
                        //GetNoteDipendenti(idDipendente);
                        //$(button).hide();
                        //$('#lblInCarico').remove();
                        //$('#newwizard').show();
                        ShowPratica(idDipendente);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GestAggiungi() {
    RaiOpenAsyncModal('modal-ricerca', '/PoliticheRetributive/SelezioneDipendenti');
}

function GestAperturaCampagna() {
    RaiOpenAsyncModal('modal-budget', '/PoliticheRetributive/OpenCampagna');
}

function GestModificaCampagna(idCampagna) {
    RaiOpenAsyncModal('modal-budget', '/PoliticheRetributive/ModificaCampagna', { idCampagna: idCampagna });
}

function GestBtnAdd() {
    if ($('[data-id-persona]:checked').length > 0) {
        $('#btnAdd').removeClass('disable');
    }
    else {
        $('#btnAdd').addClass('disable');
    }
}

function GestAddTab(index) {
    $('#wizadd-step1').removeClass('active');
    $('#wizadd-step2').removeClass('active');

    $('#wizadd-step' + index).addClass('active');
}

function GestBudgetTab(index) {
    event.preventDefault();


    if (!CheckForm())
        return false;

    $('#wizbudget-step1').removeClass('active');
    $('#wizbudget-step2').removeClass('active');

    $('.tabheadrich').removeClass('active');

    $('#wizbudget-step' + index).addClass('active');

    for (var i = 1; i <= index; i++) {
        $('#LiTab' + i).addClass('active');
    }

    $('#budget-progress').css("width", (index - 1) * 100 + "%");
    //GestBudgetAree(); //damax
}

function CheckForm() {
    var validator = $('#form-budget').validate();

    if (!$('#form-budget').valid()) {
        //swal('Non è stato possibile completare l\'iscrizione');
        validator.focusInvalid();

        return false;
    }
    return true;
}

function GestUpdateTot() {
    var valTotMod = 0;
    var valPeriodoMod = 0;

    $('div[data-area]').each(function () {
        var valTot = parseFloat($(this).find('[data-budget]').val() ? $(this).find('[data-budget]').val().replace('.', '').replace(',', '.') : 0);
        var valPeriodo = parseFloat($(this).find('[data-budget-periodo]').val() ? $(this).find('[data-budget-periodo]').val().replace('.', '').replace(',', '.') : 0);

        valTotMod = valTotMod + valTot;
        valPeriodoMod = valPeriodoMod + valPeriodo;
    });

    $('[data-area-totale]').html(valTotMod.toLocaleString("it-IT", { style: "currency", currency: "EUR", minimumFractionDigits: 2 }));
    $('[data-area-periodo]').html(valPeriodoMod.toLocaleString("it-IT", { style: "currency", currency: "EUR", minimumFractionDigits: 2 }));
}

function GestUpdateRif(input) {
    if (parseFloat($(input).attr('data-rif')) != parseFloat($(input).val().replace('.', '').replace(',', '.'))) {
        $(input).addClass('value-modified');
    }
    else {
        $(input).removeClass('value-modified');
    }

    let tr = $(input).closest('tr[data-direzione-area]');
    let area = $(tr).attr('data-direzione-area');

    let totOrganico = 0;
    let totBudget = 0.0;
    let totBudgetPeriodo = 0.0;
    $('[data-direzione-area="' + area + '"]').each(function () {
        var organico = parseInt($(this).find('[data-direzione-organico]').val());
        var budget = parseFloat($(this).find('[data-direzione-budget]').val().replace('.', '').replace(',', '.'))
        var budgetPeriodo = parseFloat($(this).find('[data-direzione-budget-periodo]').val().replace('.', '').replace(',', '.'));

        totOrganico += organico;
        totBudget += budget;
        totBudgetPeriodo += budgetPeriodo;
    });

    $('[data-area-eff-orgbudget="' + area + '"]').text(totOrganico);
    $('[data-area-eff-budget="' + area + '"]').text(parseFloat(totBudget).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 })+" €");
    $('[data-area-eff-budget-periodo="' + area + '"]').text(parseFloat(totBudgetPeriodo).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " €");
}

function GestBudgetAree() {
    debugger
    //event.preventDefault();

    if (!CheckForm())
        return false;

    $('div[data-area]').each(function () {
        debugger
        var valTot = parseFloat($(this).find('[data-budget]').val() ? $(this).find('[data-budget]').val().replace('.', '').replace(',', '.') : 0);
        var valPeriodo = parseFloat($(this).find('[data-budget-periodo]').val() ? $(this).find('[data-budget-periodo]').val().replace('.', '').replace(',', '.') : 0);
        var area = $(this).attr('data-area');

        var totOrganico = 0;
        $('[data-direzione-area="' + area + '"]').each(function () {
            totOrganico = totOrganico + parseInt($(this).find('[data-direzione-organico]').val());
        });
        var valOrganico = (valTot / totOrganico);
        var valOrganicoPeriodo = (valPeriodo / totOrganico);

        $('[data-area-eff-org="' + area + '"]').text(totOrganico);
        $('[data-area-eff-orgbudget="' + area + '"]').text(totOrganico);
        $('[data-area-eff-budget="' + area + '"]').text(parseFloat(valTot).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " €");
        $('[data-area-eff-budget-periodo="' + area + '"]').text(parseFloat(valPeriodo).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " €");

        $(this).attr('val-organico', valOrganico);
        $(this).attr('val-organicoperiodo', valOrganicoPeriodo);

        $('[data-direzione-area="' + area + '"]').each(function () {
            var organico = $(this).find('[data-direzione-organico]').val();
            var budget = parseFloat(valOrganico * organico).toFixed(2);
            var budgetPeriodo = parseFloat(valOrganicoPeriodo * organico).toFixed(2);

            $(this).find('[data-direzione-budget]').attr('data-rif', budget);
            $(this).find('[data-direzione-budget]').val(parseFloat(budget).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

            $(this).find('[data-direzione-budget-periodo]').attr('data-rif', budgetPeriodo);
            $(this).find('[data-direzione-budget-periodo]').val(parseFloat(budgetPeriodo).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        });
    });

    debugger
    formatAllInput();

    GestBudgetTab(2);
}

function PRetrUpdateImporti(input) {
    debugger
    let tr = $(input).closest('tr[data-direzione-area]');
    let areaID = $(tr).attr('data-direzione-area');
    let area = $('div[data-area="' + areaID + '"]');

    var valOrganico = parseFloat($(area).attr('val-organico'));
    var valOrganicoPeriodo = parseFloat($(area).attr('val-organicoperiodo'));

    var organico = $(input).val();
    var budget = parseFloat(valOrganico * organico).toFixed(2);
    var budgetPeriodo = parseFloat(valOrganicoPeriodo * organico).toFixed(2);

    $(tr).find('[data-direzione-budget]').val(parseFloat(budget).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
    $(tr).find('[data-direzione-budget-periodo]').val(parseFloat(budgetPeriodo).toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
}

function GestSaveBudget() {
    event.preventDefault();
    $('#gestSaveButton').addClass('rai-loader');

    var list = new Array();
    $('div[data-area]').each(function () {
        var idArea = $(this).attr('data-area');

        var valTot = $(this).find('[data-budget]').val() ? $(this).find('[data-budget]').val().replace('.', '').replace(',', '.') : "0";
        var valPeriodo = $(this).find('[data-budget-periodo]').val() ? $(this).find('[data-budget-periodo]').val().replace('.', '').replace(',', '.') : "0";

        var listDir = new Array();
        $('[data-direzione-area="' + idArea + '"').each(function () {
            listDir.push({
                Id: $(this).attr('data-direzione-id'),
                Organico: $(this).attr('data-direzione-organico'),
                OrganicoM: $(this).attr('data-direzione-organico-m'),
                OrganicoF: $(this).attr('data-direzione-organico-f'),
                OrganicoAD: $(this).find('[data-direzione-organico]').val(),
                BudgetStr: $(this).find('[data-direzione-budget]').val().replace('.', '').replace(',', '.'),
                BudgetPeriodoStr: $(this).find('[data-direzione-budget-periodo]').val().replace('.', '').replace(',', '.')
            });
        });

        list.push({
            Id: idArea,
            ImportoStr: valTot,
            ImportoPeriodoStr: valPeriodo,
            Direzioni: listDir
        })
    });

    var listDec = new Array();
    $('[data-decorrenza]').each(function () {
        if ($(this).val() && $(this).val() != '') {
            listDec.push($(this).val());
        }
    });

    var nomeCampagna = $('#nome-campagna').val();
    var dataStart = $('#data-start').val();
    var dataEnd = $('#data-end').val();
    var riserva = $('#piano-riserva').val();

    var lvAbil = $('#lvAbil').val();

    $.ajax({
        url: "/PoliticheRetributive/SaveCampagna",
        type: "POST",
        cache: false,
        contentType: 'application/json',
        data: JSON.stringify({ nomeCampagna: nomeCampagna, dataInizio: dataStart, dataFine: dataEnd, aree: list, dateDecorrenza: listDec, lvAbil: lvAbil, riserva: riserva }),
        success: function (data) {
            switch (data) {
                case "OK":
                    RaiUpdateWidget('panelApriCampagna', '/PoliticheRetributive/GetCampagne', 'replace', null);
                    swal("Dati salvati correttamente", "", 'success');
                    $('#modal-budget').modal('hide');
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        },
        complete: function () {
            $('#gestSaveButton').removeClass('rai-loader');
        }
    });
}

function GestSaveModBudget(idCampagna) {
    debugger
    event.preventDefault();
    $('#gestSaveButton').addClass('rai-loader');
    var list = new Array();
    $('div[data-area]').each(function () {
        var idArea = $(this).attr('data-area');

        var valTot = $(this).find('[data-budget]').val() ? $(this).find('[data-budget]').val().replace('.', '').replace(',', '.') : "0";
        var valPeriodo = $(this).find('[data-budget-periodo]').val() ? $(this).find('[data-budget-periodo]').val().replace('.', '').replace(',', '.') : "0";

        var listDir = new Array();
        $('[data-direzione-area="' + idArea + '"').each(function () {
            listDir.push({
                Id: $(this).attr('data-direzione-id'),
                Organico: $(this).attr('data-direzione-organico'),
                OrganicoM: $(this).attr('data-direzione-organico-m'),
                OrganicoF: $(this).attr('data-direzione-organico-f'),
                OrganicoAD: $(this).find('[data-direzione-organico]').val(),
                BudgetStr: $(this).find('[data-direzione-budget]').val().replace('.', '').replace(',', '.'),
                BudgetPeriodoStr: $(this).find('[data-direzione-budget-periodo]').val().replace('.', '').replace(',', '.')
            });
        });

        list.push({
            Id: idArea,
            ImportoStr: valTot,
            ImportoPeriodoStr: valPeriodo,
            Direzioni: listDir
        })
    });

    var listDec = new Array();
    $('[data-decorrenza]').each(function () {
        if ($(this).val() && $(this).val() != '') {
            listDec.push($(this).val());
        }
    });

    var nomeCampagna = $('#nome-campagna').val();
    var dataStart = $('#data-start').val();
    var dataEnd = $('#data-end').val();
    var riserva = $('#piano-riserva').val();
    debugger
    $.ajax({
        url: "/PoliticheRetributive/SaveModCampagna",
        type: "POST",
        cache: false,
        contentType: 'application/json',
        data: JSON.stringify({ idCampagna: idCampagna, dataInizio: dataStart, dataFine: dataEnd, aree: list, dateDecorrenza: listDec, riserva: riserva, nome: nomeCampagna }),
        success: function (data) {
            switch (data) {
                case "OK":
                    RaiUpdateWidget('panelApriCampagna', '/PoliticheRetributive/GetCampagne', 'replace', null);
                    swal("Dati salvati correttamente", "", 'success');
                    $('#modal-budget').modal('hide');
                    break;
                default:
                    swal("Oops...", data, 'error')
            }
        },
        complete: function (data) {
            $('#gestSaveButton').removeClass('rai-loader');
        }
    });
}

function GestCancellazionePiano(idCampagna) {
    swal({
        title: 'Sei sicuro?',
        text: "Il piano verrà rimosso.",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $('#tableCampaign').addClass('rai-loader');
        $.ajax({
            url: "/PoliticheRetributive/EliminaCampagna",
            type: "GET",
            dataType: "html",
            data: { idCampagna: idCampagna },
            success: function (data) {
                switch (data) {
                    case "OK":
                        RaiUpdateWidget('panelApriCampagna', '/PoliticheRetributive/GetCampagne', 'replace', null);
                        swal('Rimozione piano', "Piano rimosso con successo", "success");
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            },
            complete: function () {
                $('#table-campaign').removeClass('rai-loader');
            }
        });
    })
}

function GestRefreshDipData(idDip, reloadData, ripristinaHRDW, IsFromComboChange) {
    debugger
    $('#praticaContent').addClass('css-input-disabled');
    $("#praticaContentExt").addClass("rai-loader");

    let catArrivo = $('#inputCatArrivo').val();
    if (ripristinaHRDW)
        catArrivo = "HRDW";

    $.ajax({
        url: "/PoliticheRetributive/RefreshDatiPratica",
        type: "POST",
        data: { idDip: idDip, reloadData: reloadData, catArrivo: catArrivo, IsFromComboChange: IsFromComboChange },
        success: function (data) {
            $("#praticaContent").replaceWith($(data).find("#praticaContent"));
            $('#praticaContent').removeClass('css-input-disabled');
            $("#praticaContentExt").removeClass("rai-loader");
        }
    });
}

function GestUpdatePratica(idDip) {
    var idProvv = $('[data-id-prov]:checked').attr('data-id-prov');
    let gruppo = $('[data-id-prov]:checked').attr('data-prov-gr');
    let ipotesi = $('[data-id-prov]:checked').attr('data-prov-ip');
    var piano = $('#piano-dip').val();
    var dtaDec = "";
    if (piano > 2) {
        dtaDec = $('#data-decorrenza-select').val();
    }
    else {
        dtaDec = $('#data-decorrenza-input').val();
    }
    if (dtaDec.length == 7) dtaDec = '01/' + dtaDec;
    var catRich = $('#inputCatArrivo').val();
    var isGestExt = $('#chkGestExt')[0].checked;
    var idTemplate = $('#ID_TEMPLATE').val();
    var codMansione = $('#COD_MANSIONE').val();
    var statoLettera = $('#stato-lettera').val();
    var livRich = $('#livPrevisto').val();
    CallUpdateProvvPratica(idDip, idProvv, dtaDec, catRich, piano, isGestExt, idTemplate, codMansione, statoLettera, livRich);
}

function CallUpdateProvvPratica(idDip, idProvv, dtaDec, catRich, piano, isGestExt, idTemplate, codMansione, statoLettera, livRich) {
    var list = new Array();

    $('tr[data-row-prov][data-row-prov-custom]').each(function () {
        var rowProv = $(this).attr('data-row-prov');

        list.push({
            ID_PROV: rowProv,
            DIFF_RAL: $(this).find('#diffRal-' + rowProv).val(),
            COSTO_ANNUO: $(this).find('#costoAnnuo-' + rowProv).val(),
            COSTO_PERIODO: $(this).find('#costoPeriodo-' + rowProv).val()
            //COSTO_PERIODO: $(this).find('#costoPeriodo-' + rowProv).data('costoperiodo')
        });
    });
    debugger
    var asy = true;
    if ($("#salva-cate").length > 0) {
        asy = false;
    }
    $.ajax({
        url: "/PoliticheRetributive/UpdatePratica",
        type: "POST",
        contentType: 'application/json',
        aync:asy,
        data: JSON.stringify({ idDip: idDip, idProv: idProvv, dataDec: dtaDec, customProv: list, catRich: catRich, piano: piano, isGestExt: isGestExt, idTemplate: idTemplate, codMansione: codMansione, statoLettera: statoLettera, livRich: livRich }),
        success: function (data) {
            switch (data) {
                case "update":
                    GestRefreshDipData(idDip, false);
                    $('#form-ricerca-ElencoDipendenti').submit();
                    break;
                case "OK":
                    swal('Aggiornamento pratica', "Dati salvati con successo", "success");
                    //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                    $('#form-ricerca-ElencoDipendenti').submit();

                    break;
                default:
                    swal("Oops...", data, 'error');
            }
        }
    });
    debugger
    if ($("#salva-cate").length > 0) {
        SalvaDatiCat(idDip);
    }
}

function GestConsolidaPratica(idDip) {
    $.ajax({
        url: "/PoliticheRetributive/ConsolidaPratica",
        type: "POST",
        data: { idDip: idDip },
        success: function (data) {
            switch (data) {
                case "OK":
                    swal('Aggiornamento pratica', "Dati salvati con successo", "success");
                    //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                    $('#form-ricerca-ElencoDipendenti').submit();
                    GestRefreshDipData(idDip, false);
                    break;
                default:
                    swal("Oops...", data, 'error');
            }
        }
    });
}
function GestConsegnaPratica(idDip) {
    $('#modal-stato-lettera').show();

    $('#modal-stato-lettera-internal').html('<div class="rai-loader" style="height:100vh"></div>');
    $('#modal-stato-lettera').modal('show');
    $.ajax({
        url: '/PoliticheRetributive/ShowConsegnaLettera',
        type: "GET",
        dataType: "html",
        data: { idDip: idDip },
        cache: false,
        success: function (data) {
            $('#modal-stato-lettera-internal').html(data);
        },
        error: function (a, b, c) {
            swal({
                title: "Ops...",
                text: "Si è verificato un errore imprevisto\n" + c,
                type: 'error'
            });
            $('#modal-stato-lettera').modal('hide');
        }
    });
}

function InternalConsegnaPratica(idDip, statoLettera) {
    $.ajax({
        url: "/PoliticheRetributive/ConsegnaLetteraPratica",
        type: "POST",
        data: { idDip: idDip },
        success: function (data) {
            switch (data) {
                case "OK":
                    swal('Aggiornamento pratica', "Dati salvati con successo", "success");
                    $('#modal-stato-lettera').modal('hide');
                    //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                    $('#form-ricerca-ElencoDipendenti').submit();
                    GestRefreshDipData(idDip, false);
                    break;
                default:
                    swal("Oops...", data, 'error');
            }
        }
    });
}

function GestRifiutaPratica(idDip) {
    $.ajax({
        url: "/PoliticheRetributive/RifiutaLetteraPratica",
        type: "POST",
        data: { idDip: idDip },
        success: function (data) {
            switch (data) {
                case "OK":
                    swal('Aggiornamento pratica', "Dati salvati con successo", "success");
                    //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                    $('#form-ricerca-ElencoDipendenti').submit();
                    GestRefreshDipData(idDip, false);
                    break;
                default:
                    swal("Oops...", data, 'error');
            }
        }
    });
}

function GestRimuoviConvalida(idPratica, idOper) {
    swal({
        title: 'Sei sicuro?',
        text: "Sarà possibile modificare nuovamente la pratica",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, rimuovi!',
        cancelButtonText: 'Annulla',
        reverseButtons: 'true'
    }).then(function () {
        $.ajax({
            url: "/PoliticheRetributive/RimuoviConvalida",
            type: "GET",
            dataType: "html",
            data: { idOper: idOper },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Rimozione convalida', "Convalida rimossa con successo", "success");
                        //GestPulisciFiltri('form-ricerca-ElencoDipendenti', true);
                        $('#form-ricerca-ElencoDipendenti').submit();
                        GestRefreshDipData(idPratica, false);
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GestEditProvv(idPratica, enableCustom) {
    $('#praticaContent').addClass('css-input-disabled');
    $("#praticaContentExt").addClass("rai-loader");

    $.ajax({
        url: "/PoliticheRetributive/ToggleCustomProvv",
        type: "POST",
        data: { idDip: idPratica, enableCustom: enableCustom },
        success: function (data) {
            $("#praticaContent").replaceWith($(data).find("#praticaContent"));
            $('#praticaContent').removeClass('css-input-disabled');
            $("#praticaContentExt").removeClass("rai-loader");
        }
    });
}

function GestAddRowDec() {
    var $elem = $('[data-decorrenza]:last').parent().parent();
    var count = $('[data-decorrenza]').length;
    $('<div class="row push-10"><div class="col-sm-4">&nbsp;</div><div class="col-sm-4"><input data-decorrenza class="js-datetimepicker formElements form-control required-min" data-format="DD/MM/YYYY" data-locale="it" id="data-dec-' + (count + 1) + '" placeholder="Selezionare una data" autocomplete="off" /></div><div class="col-sm-4"><a href="#" class="btn btn-action-icon push-10-t" onclick="GestRemoveRowDec(this)"><i class="fa fa-minus"></i></a></div></div>').insertAfter($elem);
    InitDatePicker();
}
function GestRemoveRowDec(anchor) {
    var $elem = $(anchor).parent().parent();
    $elem.remove();
}

function GestAggiornaCostoGratifica(idProv, aliq) {
    var piano = $('#piano-dip').val();
    var dtaDec = "";
    if (piano > 2) {
        dtaDec = $('#data-decorrenza-select').val();
    }
    else {
        dtaDec = $('#data-decorrenza-input').val();
    }
    if (dtaDec.length == 7) dtaDec = '01/' + dtaDec;

    var dateComp = dtaDec.split('/');
    var decorrenzaYear = parseInt(dateComp[2]);
    var currentYear = new Date().getFullYear();

    var diffRal = parseFloat($('#diffRal-' + idProv).val().replace(',', '.'));
    var aliqDec = parseFloat(aliq.replace(',', '.'));

    var costoPeriodo = 0;
    if (decorrenzaYear <= currentYear)
        costoPeriodo = (diffRal + (diffRal * aliqDec / 100));

    $('#costoAnnuo-' + idProv).val(0);
    $('#costoPeriodo-' + idProv).val(costoPeriodo.toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ''));
}
function GestAggiornaCostoAumento(idProv, aliq) {
    debugger
    var piano = $('#piano-dip').val();
    var dtaDec = "";
    if (piano > 2) {
        dtaDec = $('#data-decorrenza-select').val();
    }
    else {
        dtaDec = $('#data-decorrenza-input').val();
    }
    if (dtaDec.length == 7) dtaDec = '01/' + dtaDec;

    var dateComp = dtaDec.split('/');
    var decorrenzaMonth = parseInt(dateComp[1]);
    var decorrenzaYear = parseInt(dateComp[2]);
    var currentYear = new Date().getFullYear();
    var diffRal = parseFloat($('#diffRal-' + idProv).val().replace(',', '.'));
    var aliqDec = parseFloat(aliq.replace(',', '.'));
    var costoAnnuo = diffRal + (diffRal * aliqDec / 100);

    var costoPeriodo = 0;
    if (decorrenzaYear <= currentYear)
        costoPeriodo = costoAnnuo / 14 * ((13 - decorrenzaMonth) + (decorrenzaMonth < 7 ? 2.5 : 1.5));

    debugger
    if (decorrenzaYear > currentYear) {
        costoPeriodo = 0;
        costoAnnuo = costoAnnuo / 14 * ((13 - decorrenzaMonth) + (decorrenzaMonth < 7 ? 2.5 : 1.5));
    }
    $('#costoAnnuo-' + idProv).val(costoAnnuo.toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ''));
    $('#costoPeriodo-' + idProv).val(costoPeriodo.toLocaleString("it-IT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).replace('.', ''));
}
function blurAll() {
   
    $("#table-daticat tr").each(function () {
        var index = $(this).attr("data-row-prov");
        $("#diffRal-" + index).blur();
    });
}
function PRetribUpdateDec() {
    var piano = $('#piano-dip').val();
    var piano_orig = $('#piano-dip').attr('data-orig-piano');
    var dtaDec = "";

    if (piano_orig > 2) {
        dtaDec = $('#data-decorrenza-select').val();
    }
    else {
        dtaDec = $('#data-decorrenza-input').val();
    }
    if (dtaDec != null && dtaDec.length == 7) dtaDec = '01/' + dtaDec;

    if (piano > 2) {
        $.ajax({
            url: "/PoliticheRetributive/getAJAXDecorrenzaCampagna",
            type: "GET",
            dataType: "json",
            data: { idCampagna: piano },
            cache: false,
            success: function (data) {
                $("#data-decorrenza-select").html('<option value="">Data decorrenza</option>');
                for (var i = 0; i < data.result.length; i++) {
                    $("#data-decorrenza-select").append('<option value="' + data.result[i].Value + '" ' + '>' + data.result[i].Text + '</option>');
                }

                $('#data-decorrenza-input').hide();
                $('#data-decorrenza-select').val(dtaDec);
                $('#data-decorrenza-select').show();
            }
        });
    }
    else {
        $('#data-decorrenza-select').hide();
        $('#data-decorrenza-input').val();
        $('#data-decorrenza-input').show();
    }

    $('#piano-dip').attr('data-orig-piano', piano);
}
function GestRichDoppie(matricole) {
    $('#MatricoleMultiple').val('true');
    $('#Matricola').val(matricole);
    $('#btnCerca').click();
}

function GestUploadLettera(idDip, idOper) {
    if ($("#inputAttachVerbMod").val() == '')
        return;

    var obj = $("#inputAttachVerbMod")[0].files[0];

    if (obj.type != 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
        swal({
            title: "Errore caricamento file",
            text: "Il file deve essere un documento Word",
            type: "error"
        }).then(function () {
            $('#inputAttachVerbMod').click();
        });
        return;
    }

    swal({
        title: 'Il file è stato caricato correttamente',
        text: "Inserisci una breve descrizione",
        html: '<p>Inserisci una breve descrizione</p><textarea tabindex="0" placeholder="Inserisci una breve descrizione" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important;"/>',
        //input: 'text',
        //type: 'warning',
        //showCancelButton: true,
        //confirmButtonColor: '#3085d6',
        //cancelButtonColor: '#d33',
        confirmButtonText: 'Salva',
        //cancelButtonText: 'No, cancel!',
        confirmButtonClass: 'btn btn-primary btn-lg',
        //cancelButtonClass: 'btn btn-danger',
        preConfirm: function () {
            return new Promise(function (resolve, reject) {
                if ($("#tmp_des").val() == "") {
                    reject("Inserisci una breve descrizione")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        var item = result;

        var currentdate = new Date();

        var item = 'Lettera modificata';
        var desc = $("#tmp_des").val();

        var formData = new FormData();
        formData.append('idOper', idOper);
        formData.append('_fileUpload', obj);
        formData.append('filename', item);
        formData.append('descr', desc);
        $.ajax({
            url: "/PoliticheRetributive/AggiungiLetteraModificata",
            type: "POST",
            cache: false,
            dataType: 'html',
            contentType: false,
            processData: false,
            data: formData,
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal("Lettera caricata", "La lettera è stata caricata correttamente", 'success')
                        $("#inputAttachVerbMod").val('');
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            }
        });
    })
}

function GestModificaLettera(idPratica) {

    RaiOpenAsyncModal('modal-modifica-lettera', '/PoliticheRetributive/ModificaLettera', { idPratica: idPratica }, null, 'GET');

    //$('#modal-modifica-lettera-internal').html('<div class="rai-loader" style="height:100vh"></div>');
    //$('#modal-modifica-lettera').modal('show');
    //$.ajax({
    //    url: '/PoliticheRetributive/ModificaLettera',
    //    type: "GET",
    //    dataType: "html",
    //    data: { idPratica: idPratica },
    //    cache: false,
    //    success: function (data) {
    //        $('#modal-modifica-lettera-internal').html(data);
    //    },
    //    error: function (a, b, c) {
    //        swal({
    //            title: "Ops...",
    //            text: "Si è verificato un errore imprevisto\n" + c,
    //            type: 'error'
    //        });
    //    }
    //});
}
function GestSaveModificaLettera(button, idForm) {
    RaiSubmitForm(button, idForm,
        function () {
            var obj = new FormData($('#' + idForm)[0]);
            return obj;
        }, false, false,
        "Lettera salvata con successo",
        function () {
            $('#modal-modifica-lettera').modal("hide");
        }, false);
}
// #endregion
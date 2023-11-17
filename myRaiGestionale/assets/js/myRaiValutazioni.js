//#region GestioneCampagne
function ValToggleEndedCampaign(check, containerElement) {
    if ($(check)[0].checked) {
        $('#' + containerElement).find('[data-val-campaign-active=False]').show();
    }
    else {
        $('#' + containerElement).find('[data-val-campaign-active=False]').hide();
    }
}
function ValUpdateCampagne() {
    RaiUpdateWidget('panelApriCampagna', "/Valutazioni/Widget_Campagne", "replace");
}
function ValAperturaCampagna() {
    RaiOpenAsyncModal('modal-campagna', "/Valutazioni/Modal_campagne", { idCampagna: 0 });
}
function ValModificaCampagna(idCampagna) {
    RaiOpenAsyncModal('modal-campagna', "/Valutazioni/Modal_campagne", { idCampagna: idCampagna });
}
function ValSalvaCampagna(button, idForm, idCampagna) {
    event.preventDefault();
    $(button).addClass("disable");

    var form = $("#" + idForm).first();
    var validator = $(form).validate();

    if (!$(form).valid()) {
        $(button).removeClass("disable");
        return false;
    }

    var parameters = new FormData($('#' + idForm)[0]);

    $(form).parent().addClass("rai-loader");

    $.ajax({
        url: $(form).attr("action"),
        processData: false,
        contentType: false,
        type: "POST",
        data: parameters,
        dataType: 'JSON',
        success: function (data) {
            switch (data.Esito) {
                case "OK":
                    swal({ title: "Iniziativa salvata con successo", type: 'success' });
                    ValUpdateCampagne();
                    $('#modal-campagna').modal('hide');
                    if (idCampagna == 0) {
                        ValModificaCampagnaScheda(data.IdCampagna, 0);
                    }
                    break;
                default:
                    swal({ title: "Ops...", text: data.Esito, type: 'error' });
                    break;
            }
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        },
        error: function (a, b, c) {
            swal({ title: "Ops...", text: ' ' + b + ' ' + c, type: 'error' });
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        }
    })
}
function ValCancellaCampagna(idCampagna) {
    RaiDeleteRecord("L'iniziativa verrà cancellata",
        "/Valutazioni/Cancella_campagna",
        { idCampagna: idCampagna },
        "Iniziativa cancellata con successo",
        function () { ValUpdateCampagne(); }
    );
}
//#endregion

//#region GestioneSchedeCampagne
function ValUpdateSchede(idCampagna) {
    RaiUpdateWidget('sheet_campaign_' + idCampagna, "/Valutazioni/Widget_Campagna_Schede", "html", { idCampagna: idCampagna });
    ValUpdateValutazioni();
}
function ValAggiungiScheda(button, idForm, idCampagna) {
    RaiSubmitForm(button, idForm,
        function () {
            var obj = new FormData($('#' + idForm)[0]);

            if ($('#chkFiltroQual').length > 0) {
                if ($('#chkFiltroQual')[0].checked) {
                    $('[data-check-group="qual-filter"]').each(function () {
                        if ($(this)[0].checked) {
                            obj.append('QualificheInt', $(this).val());
                        }
                    });
                }
            }

            if ($('#chkFiltroDir').length > 0) {
                if ($('#chkFiltroDir')[0].checked) {
                    $('[data-check-group="dir-filter"]').each(function () {
                        if ($(this)[0].checked) {
                            obj.append('Servizi', $(this).val());
                        }
                    });
                }
            }

            //if ($('#chkFiltroUorg')[0].checked) {
            //    $('[data-uorg]').each(function () {
            //        if ($(this)[0].checked) {
            //            obj.append('Uorg', $(this).data('uorg'));
            //        }
            //    });
            //}

            return obj;
        }, false, false,
        "Scheda aggiunta con successo",
        function () {
            ValUpdateCampagne();
            //ValUpdateSchede(idCampagna);
            ValUpdateValutazioni();
            $('#modal-campagna-scheda').modal('hide');
        }, true);
}
function ValModificaCampagnaScheda(idCampagna, idCampagnaScheda) {
    RaiOpenAsyncModal('modal-campagna-scheda', "/Valutazioni/Modal_campagna_scheda", { idCampagna: idCampagna, idCampagnaScheda: idCampagnaScheda });

    //RaiUpdateWidget("campagnaEditScheda_container", "/Valutazioni/Modifica_CampagnaScheda", "html", { idCampagna: idCampagna, idCampagnaScheda: idCampagnaScheda });
}
function ValCancellaCampagnaScheda(idCampagna, idCampagnaScheda) {
    RaiDeleteRecord("La scheda verrà cancellata",
        "/Valutazioni/Cancella_CampagnaScheda",
        {
            idCampagnaScheda: idCampagnaScheda
        },
        "La scheda è stata cancellata con successo",
        function () { ValUpdateSchede() }
    );
}
//#endregion

//#region GestioneValutazione
function ValUpdateValutazioni() {
    RaiUpdateWidget('panelElencoValutazioni', "/Valutazioni/Elenco_Valutazioni", "replace", null);
}
function ValUpdateValutazioniSheet(idCampaignSheet) {
    RaiUpdateWidget('panelElencoValutazioni' + idCampaignSheet, "/Valutazioni/Elenco_Valutazioni_Sheet", "replace", { idCampaignSheet: idCampaignSheet });
}
function ValModal_Valutazione(idEvaluation, openAsResp, canModify) {
    debugger
    RaiOpenAsyncModal('modal-valutazione', "/Valutazioni/Modal_Valutazione", { idEvaluation: idEvaluation, openAsResp: openAsResp, canModify: canModify });
}
function ValModal_CambioValutatore(idEvaluation) {
    RaiOpenAsyncModal('modal-cambio-valutatore', "/Valutazioni/Modal_CambioValutatore", { idEvaluation: idEvaluation });
}
function SaveCambioValutatore(idEvaluation, oldEvaluator) {
    //idEvaluation, int idOldEval, int idNewEval
    var newEval = $('#newEval').val();

    $.ajax({
        url: '/Valutazioni/Save_CambioValutatore',
        type: 'POST',
        data: { idEvaluation: idEvaluation, idOldEval: oldEvaluator, idNewEval: newEval },
        cache: false,
        success: function (data) {
            switch (data) {
                case "OK":
                    swal({ title: "Salvataggio effettuato", type: "success", text: "Il valutatore è stato cambiato correttamente" });
                    ValUpdateValutazioni();
                    $('#modal-cambio-valutatore').modal('hide');
                    break;
                default:
                    swal({ title: "Ops...", type: "error", text: data });
            }
        },
        error: function (a, b, c) {
            swal({ title: "Ops...", type: "error", text: a + ' ' + b + ' ' + c });
        }
    });
}
function ValCheckOptional(radio) {
    var qst = $(radio).closest('div[data-val-question]');
    if ($(qst).data('val-optional')) {
        var isChecked = $(radio).attr('is-checked') == 'true';
        $(radio).prop('checked', !isChecked);
        $(radio).attr('is-checked', !isChecked);
    }
}
function ValCalcMediaPond(valType) {
    var sommaValPerPeso = 0;
    var sommaPeso = 0;

    $('#table-Valutazione div[data-val-type=' + valType + ']').each(function () {
        var isOptional = $(this).data('val-optional');
        var checkedOption = $(this).find('input[type="radio"]:checked');

        if (checkedOption.length > 0 || !isOptional) {
            var inputVal = $(this).find('input[type="radio"]:checked').data('val-value');
            var inputWeight = $(this).data('val-weight');
            sommaValPerPeso += inputVal * inputWeight;
            sommaPeso += inputWeight;
        }
    });

    var media = 0;

    if (sommaValPerPeso > 0 && sommaPeso > 0) {
        media = sommaValPerPeso / sommaPeso;
    }

    if (media > 0) {
        $('label[data-val-total-type=' + valType + ']').text(Math.round((media + Number.EPSILON) * 100) / 100);
    }
    else
        $('label[data-val-total-type=' + valType + ']').text('Non ancora valutato');

    var tSommaValPerPeso = 0;
    var tSommaPeso = 0;

    $('label[data-val-total]').each(function () {
        var inputVal = $(this).text();
        var inputWeight = $(this).data('val-total-weight');
        tSommaValPerPeso += inputVal * inputWeight;
        tSommaPeso += inputWeight;
    });

    var tMedia = 0;
    if (tSommaValPerPeso > 0 && tSommaPeso > 0) {
        tMedia = tSommaValPerPeso / tSommaPeso;
    }

    if (tMedia >= 1) {
        $('#totOverall').text(Math.round((tMedia + Number.EPSILON) * 100) / 100);
    }
    else {
        $('#totOverall').text('Non ancora valutato');
    }
}
function ValSalvaValutazione(button, idForm, idCampaignSheet, saveAsDraft)  {
    event.preventDefault();

    var idValutazione = $('#IdValutazione').val();
    var owner = 'Superiore';//$('#Owner').val();

    var anyCustomNotSelected = false;

    if ($('[data-val-typequestion="custom"]').length > 0) {
        $('[data-val-typequestion="custom"]').each(function () {
            var idQst = $(this).attr('data-val-question');
            if ($('[data-val-question-custom-text="' + idQst + '"]').val().length > 0 && $('[data-val-question-custom-value="' + idQst + '"] input[type="radio"]:checked').length == 0) {
                $('#radio' + idQst + '-err').show();
                anyCustomNotSelected = true;
            }
            else {
                $('#radio' + idQst + '-err').hide();
            }
        });
    }

    RaiSubmitForm(button, idForm,
        function () {
            var questionAnswers = new Array();

            $('[data-val-question]').each(function () {
                var questionId = $(this).data("val-question");
                var questionType = $(this).data("val-typequestion");
                var questionDisplay = $(this).data('val-display');

                var questionInt = null;
                var questionStr = null;
                var questionValue = null;
                var questionValue1 = null;

                if (questionDisplay == 'Radio button') {
                    var radioID = "radio" + questionId;
                    questionValue = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                } else if (questionDisplay == 'Edit') {
                    questionValue = $(this).find('textarea').val();
                } else if (questionDisplay == 'Select') {
                    questionValue = $(this).find('select').val();
                } else if (questionDisplay == 'Custom') {
                    var radioID = "radio" + questionId;
                    questionValue = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                    questionValue1 = $(this).find('textarea').val();
                }

                if (questionType == "Stringa") {
                    questionStr = questionValue;
                } else if (questionType == "Intero") {
                    questionInt = questionValue;
                } else if (questionType == "StringIntero") {
                    questionInt = questionValue;
                    questionStr = questionValue1;
                }

                //if (questionType == 'int') {
                //    var radioID = "radio" + questionId;
                //    var questionValue = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                //    questionInt = questionValue;
                //} else if (questionType == 'string') {
                //    questionStr = $(this).val();
                //} else if (questionType == 'edit') {
                //    questionStr = $(this).find('textarea').val();
                //} else if (questionType == 'combo') {
                //    questionInt = $(this).find('select').val();
                //} else if (questionType == 'custom') {
                //    var radioID = "radio" + questionId;
                //    questionInt = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                //    questionStr = $(this).find('textarea').val();
                //}

                questionAnswers.push({
                    ID_QUESTION: questionId,
                    VALUE_INT: questionInt,
                    VALUE_STR: questionStr
                });
            });
            return JSON.stringify({ idEvaluation: idValutazione, questionAnswers: questionAnswers, saveAsDraft: saveAsDraft, owner: owner });
        }, true, 'application/json; charset=UTF-8',
        "Valutazione salvata con successo",
        function () {
            ValUpdateValutazioniSheet(idCampaignSheet);
            $('#modal-valutazione').modal('hide');
        }, true);
}
function ValSalvaValutazionePresaVisione(button, idForm, idValuation, idCampaignSheet) {
    RaiSubmitForm(button, idForm,
        function () {
            return JSON.stringify({ evaluation: idValuation, note: $('#valNotaResp').val() });
        }, true, 'application/json; charset=UTF-8',
        "Nota inserita con successo",
        function () {
            ValUpdateValutazioniSheet(idCampaignSheet);
            $('#modal-valutazione').modal('hide');
        }, true);
}
function ValSalvaValutazioneRUO(button, idForm, idValuation, idCampaignSheet, approved) {
    event.preventDefault();

    if (!approved) {
        var nota = $('#valNotaRuo').val();
        if (nota == '') {
            swal({
                title: "Attenzione",
                text: "Si prega di inserire una nota",
                type: 'error'
            }).then(function () {
                $('#valNotaRuo').focus();
            });
            return;
        }
    }



    RaiSubmitForm(button, idForm,
        function () {
            return JSON.stringify({ evaluation: idValuation, note: $('#valNotaRuo').val(), approved: approved });
        }, true, 'application/json; charset=UTF-8',
        "Analisi inserita con successo",
        function () {
            ValUpdateValutazioniSheet(idCampaignSheet);
            $('#modal-valutazione').modal('hide');
        }, true);
}
function ValModalValutazioneUpdateStatus() {
    let button = $('#bntSaveValutazione');

    let elements = $('#table-Valutazione div[data-val-question][data-val-typequestion][data-val-optional="false"]').length;
    let valuedElements = $('#table-Valutazione div[data-val-question][data-val-typequestion][data-val-optional="false"]').filter(function () {
        let questionDisplay = $(this).data('val-display');
        let isValued = false;

        if (questionDisplay == 'Radio button') {
            isValued = $(this).find('input[type="radio"]:checked').length > 0;
        } else if (questionDisplay == 'Edit') {
            isValued = $(this).find('textarea').val() != '';
        } else if (questionDisplay == 'Select') {
            isValued = $(this).find('select').val() != '';
        } else if (questionDisplay == 'Custom') {
            isValued = $(this).find('input[type="radio"]:checked').length > 0 && $(this).find('textarea').val();
        }

        return isValued;
    }).length;

    if (elements == valuedElements) {
        $(button).removeClass("disable");
    }
    else {
        $(button).addClass('disable');
    }
}
function ValGoToNextEval(valType, idCampaignSheet, idEvaluation) {
    var container = $('#panelElencoValutazioni' + idCampaignSheet);
    var rows = $(container).find('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + ']');
    var rowsCount = $(rows).length;
    var indexCurrent = $(rows).index($('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + '][data-val-eval=' + idEvaluation + ']'));
    if (indexCurrent < rowsCount - 1) {
        var nextElem = $(rows).eq(indexCurrent + 1);
        var newEvaluation = $(nextElem).data('eval');
        ValModal_Valutazione(newEvaluation, valType);
    }
}
function ValGoToPreviousEval(valType, idCampaignSheet, idEvaluation) {
    var container = $('#panelElencoValutazioni' + idCampaignSheet);
    var rows = $(container).find('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + ']');
    var rowsCount = $(rows).length;
    var indexCurrent = $(rows).index($('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + '][data-val-eval=' + idEvaluation + ']'));
    if (indexCurrent > 0) {
        var prevElem = $(rows).eq(indexCurrent - 1);
        var newEvaluation = $(prevElem).data('eval');
        ValModal_Valutazione(newEvaluation, valType);
    }
}

function ValRicercaSubmit(formId) {
    event.preventDefault();
    RaiSearchFormPreSubmit('panelElencoValutazioni');
    RaiSearchFormPreSubmit('panelElencoValutazioniResp');
    RaiSearchFormCheckHasFilter(formId);
    var formData = new FormData($('#' + formId)[0]);
    RaiUpdateWidget('panelElencoValutazioni', "/Valutazioni/Elenco_Valutazioni", "replace", formData, true);
    RaiUpdateWidget('panelElencoValutazioniResp', "/Valutazioni/Elenco_Valutazioni_Resp", "replace", formData, true);
}
function ValRicercaClear(formId) {
    event.preventDefault();
    RaiSearchFormClear(formId, false);
    ValRicercaSubmit(formId);
}

function ValValutazione_rimuovi(idCampSheet, idEval) {
    swal({
        title: 'Elimina valutazione',
        text: "Sei sicuro di voler eliminare la valutazione?",
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-primary btn-lg push-5-r',
        showCancelButton: true,
        cancelButtonText: "Annulla",
        cancelButtonClass: 'btn btn-lg btn-default push-5-l',
        buttonsStyling: false,

    }).then(function (result) {
        $.ajax({
            url: "/Valutazioni/Remove_Valutatore",
            type: "POST",
            //dataType: "json",
            data: { idEval: idEval },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Valutazione eliminata con successo');
                        ValUpdateValutazioniSheet(idCampSheet);
                        break;
                    default:
                        swal('Non è stato possibile completare l\'operazione');
                        break;
                }

            },
            error: function (a, b, c) {
                swal(a + ' ' + b + ' ' + c);
            }
        });
    })
}

function ValValutazione_elimina(idCampSheet, idEval) {
    swal({
        title: 'Pulizia valutazione',
        text: "Sei sicuro di voler azzerare la valutazione?",
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-primary btn-lg push-5-r',
        showCancelButton: true,
        cancelButtonText: "Annulla",
        cancelButtonClass: 'btn btn-lg btn-default push-5-l',
        buttonsStyling: false,

    }).then(function (result) {
        $.ajax({
            url: "/Valutazioni/EliminaValutazione",
            type: "POST",
            //dataType: "json",
            data: { idEval: idEval },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Valutazione azzerata con successo');
                        ValUpdateValutazioniSheet(idCampSheet);
                        break;
                    default:
                        swal('Non è stato possibile completare l\'operazione');
                        break;
                }

            },
            error: function (a, b, c) {
                swal(a + ' ' + b + ' ' + c);
            }
        });
    })
}
//#endregion

//#region GestioneSchede
function ValAnteprimaScheda(id) {
    RaiOpenAsyncModal('modal-valutazione', '/Valutazioni/ShowEvalPreview', { idSheet: id });
}
function ValModificaScheda(id) {
    RaiOpenAsyncModal('modal-scheda-gest', '/Valutazioni/Modal_Scheda', { idScheda: id });
}
function ValCancellaScheda(id) {

}
function ValCreazioneScheda() {
    RaiOpenAsyncModal('modal-scheda-gest', '/Valutazioni/Modal_Scheda', { idScheda: 0 });
}
function ValShowReport(id) {
    RaiOPNavGoToNext("valutazioni", "report_valutazioni", "Report valutazioni", "/Valutazioni/GetReportScheda", { idScheda: id }, "POST")
}
//endregion

function showAlbero2(idRoot) {
    $('#organigramma2').data('zoom', 100);
    $('#organigramma2').css('zoom', '100%');
    $('#organigramma2').html('<div class="rai-loader" style="width:100%;height:700px"></div>');
    $('#tree2').modal('show');

    createTree(idRoot);
}

function createTree(idRoot) {
    var eventInt = setInterval(function () {
        $.ajax({
            url: '/Valutazioni/ShowAlberoByModal2',
            type: 'POST',
            data: { idRoot: idRoot },
            dataType: "json",
            cache: false,
            success: function (data) {
                var tmp = JSON.parse(data);
                //$("#organigramma2").html();
                var chart_config = {
                    chart: {
                        rootOrientation: "NORTH",
                        container: "#organigramma2",
                        levelSeparation: 50,
                        siblingSeparation: 50,
                        subTeeSeparation: 50,
                        nodeAlign: "CENTER",
                        node: {
                            collapsable: true,
                            HTMLclass: 'nodeExample1'
                        },
                        connectors: {
                            type: "step",
                            style: {
                                "stroke-width": 1
                            }
                        },
                    },

                    nodeStructure: tmp
                };
                var c = new Treant(chart_config, null, $);

                clearInterval(eventInt);
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                swal(err.Message);
            }
        });
    }, 1000);
}
//#region GestioneValutazione
function ValUpdateValutazioni() {
    RaiUpdateWidget('panelElencoValutazioni', "/Valutazioni/Elenco_Valutazioni", "replace", null);
}
function ValUpdateValutazioniSheet(idCampaignSheet) {
    var visibleBlock = $('[data-view-container=' + idCampaignSheet + '][data-view-block]:visible').data('view-block');
    RaiUpdateWidget('panelElencoValutazioni' + idCampaignSheet, "/Valutazioni/Elenco_Valutazioni_Sheet", "replace", { idCampaignSheet: idCampaignSheet }, false,
        function () {
            RaiChangeView($('[data-view-container=' + idCampaignSheet + '][data-view-show="' + visibleBlock + '"]'));
        });
}
function ValModal_Valutazione(idEvaluation, openAsResp, asValued, owner) {
    //RaiOpenAsyncModal('modal-valutazione', "/Valutazioni/Modal_Valutazione", { idEvaluation:idEvaluation, openAsResp: openAsResp, asValued: asValued, owner:owner });

    RaiOPNavGoToNext('nav-val', 'nav-val-sch', 'Scheda di valutazione', '/Valutazioni/Modal_Valutazione', { idEvaluation: idEvaluation, openAsResp: openAsResp, asValued: asValued, owner: owner });
}
function ValPage_Valutazione(idEvaluation, openAsResp, asValued, owner) {    
    RaiOPNavGoToNext('nav-scrivania', 'nav-val-sch', 'Scheda di valutazione', '/Valutazioni/Modal_Valutazione', { idEvaluation: idEvaluation, openAsResp: openAsResp, asValued: asValued, owner: owner });
    $('#modal-valutazione-ext').modal('hide');
}
function ValModal_AutoValutazione(idCampaignSheet, idPersona) {
    RaiOpenAsyncModal('modal-valutazione', "/Valutazioni/Modal_AutoValutazione", { idCampaignSheet: idCampaignSheet, idPersona: idPersona });
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

    $('#form-valutazione #table-Valutazione div[data-val-type=' + valType + '][data-val-average=true]').each(function () {
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
function ValCheckCustomDisplayValue(textArea, idQst, check) {
    if (check) {
        if ($(textArea).val().length > 0) {
            $('[data-val-question-custom-value="' + idQst + '"]').removeClass('disable');
            //$('[data-val-question-custom-value="' + idQst + '"] input[type="radio"]' ).attr('required', 'required');
        }
        else {
            $('[data-val-question-custom-value="' + idQst + '"]').addClass('disable')
            $('[data-val-question-custom-value="' + idQst + '"] input[type="radio"]').each(function () {
                $(this)[0].checked = false;
                $('#radio' + idQst + '-err').hide();
            });
        }
    }
}
function ValCheckCustomCheckValue(radio, idQst, check) {
    if (check) {
        if (!$(this)[0].checked) {
            $('#radio' + idQst + '-err').hide();
        }
    }
}
function ValSalvaValutazione(button, idForm, idCampaignSheet, saveAsDraft) {
    //controllo valorizzazione radio su domande custom
    event.preventDefault();

    var idValutazione = $('#IdValutazione').val();
    var owner = $('#Owner').val();

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

    if (anyCustomNotSelected) return;

    RaiSubmitForm(button, idForm,
                    function () {
                        var questionAnswers = new Array();

                        $('[data-val-question]').each(function () {
                            var questionId = $(this).data("val-question");
                            var questionType = $(this).data("val-typequestion");
                            
                            var questionInt = null;
                            var questionStr = null;

                            if (questionType == 'int') {
                                var radioID = "radio" + questionId;
                                var questionValue = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                                questionInt = questionValue;
                            } else if (questionType=='string') {
                                questionStr = $(this).val();
                            } else if (questionType == 'custom') {
                                var radioID = "radio" + questionId;
                                questionInt = $(this).find('input[name="' + radioID + '"]:checked').data("val-value");
                                questionStr = $(this).find('textarea').val();
                            }

                            questionAnswers.push({
                                ID_QUESTION: questionId,
                                VALUE_INT: questionInt,
                                VALUE_STR: questionStr
                            });
                        });
            return JSON.stringify({ idEvaluation: idValutazione, questionAnswers: questionAnswers, saveAsDraft: saveAsDraft, owner:owner  });
                    }, true, 'application/json; charset=UTF-8',
                    "Valutazione salvata con successo",
                    function () {
            if ($('#panelElencoValutazioni').length>0) {
                        ValUpdateValutazioniSheet(idCampaignSheet);
            }
            //$('#modal-valutazione').modal('hide');
            $('[data-op-path="main"]').click();
                    }, true);
}
function ValSalvaValutazionePresaVisione(button, idForm, idValuation, idCampaignSheet, approved) {
    RaiSubmitForm(button, idForm,
            function () {
            return JSON.stringify({ evaluation: idValuation, note: $('#valNotaResp').val(), approved: approved });
            }, true, 'application/json; charset=UTF-8',
            "Nota inserita con successo",
            function () {
                ValUpdateValutazioniSheet(idCampaignSheet);
            //$('#modal-valutazione').modal('hide');
            $('[data-op-path="main"]').click();
        }, true);
}
function ValSalvaPiano(button) {
    event.preventDefault();

    var idCampaignSheet = $('#CampagnaScheda').val();

    RaiSubmitForm(button, 'form-piano',
        function () {
            var obj = new FormData($('#form-piano')[0]);
            return obj;
        }, false, false,
        "Piano inserito con successo",
        function () {
            ValUpdateValutazioniSheet(idCampaignSheet);
            //$('#modal-valutazione').modal('hide');
            $('[data-op-path="main"]').click();
        }, true);
}
function ValSalvaPianoNota(button, idValuation, idCampaignSheet, approved) {
    if (!approved) {
        var nota = $('#valNotaResp').val();
        if (nota == undefined || nota == '') {
            swal({
                title: "Attenzione",
                text: "Inserire una nota in caso di rifiuto",
                type: 'error',
                customClass: 'rai'
            });
        }
    }
    
    RaiSubmitForm(button, 'form-piano-nota',
        function () {
            return JSON.stringify({ evaluation: idValuation, note: $('#valNotaResp').val(), approved: approved });
        }, true, 'application/json; charset=UTF-8',
        approved?"Piano approvato con successo":"Piano rifiutato con successo",
        function () {
            if ($('#panelElencoValutazioni').length > 0) {
                ValUpdateValutazioniSheet(idCampaignSheet);
            }
            //$('#modal-valutazione').modal('hide');
            $('[data-op-path="main"]').click();
            }, true);
}
function ValModalValutazioneUpdateStatus() {
    var button = $('#bntSaveValutazione');

    var elements = $('#table-Valutazione div[data-val-question][data-val-typequestion="int"][data-val-optional="false"]').length;
    var valuedElements = $('#table-Valutazione div[data-val-question][data-val-typequestion="int"][data-val-optional="false"] input[type="radio"]:checked').length;

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
        var newEvaluation = $(nextElem).data('val-eval');
        ValModal_Valutazione(newEvaluation, valType);
    }
}
function ValGoToPreviousEval(valType, idCampaignSheet, idEvaluation) {
    var container = $('#panelElencoValutazioni' + idCampaignSheet);
    var rows = $(container).find('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + ']');
    var indexCurrent = $(rows).index($('[data-val-asResp=' + valType + '][data-val-sheet=' + idCampaignSheet + '][data-val-eval=' + idEvaluation + ']'));
    if (indexCurrent > 0) {
        var prevElem = $(rows).eq(indexCurrent - 1);
        var newEvaluation = $(prevElem).data('val-eval');
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
//#endregion

//#region GestioneDeleghe
function ValCreaDelega() {
    RaiOpenAsyncModal('modal-delega', '/Valutazioni/Modal_delega', { idDelega: 0 });
}
function ValModificaDelega(idDelega) {
    RaiOpenAsyncModal('modal-delega', "/Valutazioni/Modal_delega", { idDelega: idDelega });
}
function ValCancellaDelega(idDelega) {
    RaiDeleteRecord("La delega verrà cancellata",
                "/Valutazioni/Cancella_Delega",
                    {
                        idDelega: idDelega
                    },
                "La delega è stata cancellata con successo",
                function () { ValUpdateDeleghe(); }
            );
}
function ValUpdateDeleghe() {
    RaiUpdateWidget('panelDeleghe', "/Valutazioni/Widget_Deleghe", "replace", null);
}
function ValLoadAvailableDelegate() {
    var evaluatorRole = $('#RuoloDelegato').val();
    RaiUpdateSelectValue("/Valutazioni/GetAJAXPersoneDelegabili", { evaluatorRole: evaluatorRole }, "PersonaDelegata", true, "", "Seleziona la persona");
    ValLoadAvailablePeople();
}
function ValLoadAvailablePeople() {
    var evaluatorRole = $('#RuoloDelegato').val();
    var personChoosen = $('#PersonaDelegata').val();
    RaiUpdateWidget('block-val-delegabili', '/Valutazioni/GetValutazioniDelegabili', 'replace', { evaluatorRole: evaluatorRole, personChoosen: personChoosen })
}
function ValSalvaDelega(button, idForm) {
    RaiSubmitForm(button, idForm,
                function () {
                    var obj = new FormData($('#' + idForm)[0]);
                    $('[data-check-group="persona-delega"]').each(function () {
                        if ($(this)[0].checked) {
                            obj.append('ValutazioniDelegabiliSel', $(this).val());
                        }
                    });
                    return obj;
                }, false, false,
                "Delega salvata con successo",
                function () {
                    ValUpdateDeleghe();
                    var idCampaignSheet = $('#RuoloDelegato').val();
                    ValUpdateValutazioniSheet(idCampaignSheet);
                    $('#modal-delega').modal("hide");
                }, false);
}
//#endregion

//#region GestioneSceltaValutatoreEsterno
function ValModal_ValutazioneEsterno() {
    RaiOpenAsyncModal('modal-valutazione-ext', "/Valutazioni/Modal_ValutatoreEsterno", {  });
}
function ValSalvaRichiestaExtVal(button, idForm) {
    RaiSubmitForm(button, idForm,
                    function () {
                        var obj = new FormData($('#' + idForm)[0]);
                        return obj;
                    }, false, false,
                    "Richiesta salvata con successo",
                    function () {
                        ValModal_ValutazioneEsterno();
                    }, true);
}
//#endregion

//#region GestioneApprovaazioneValutatoreEsterno
function ValSalvaApprovazioneExtVal(button, idForm, idEvaluator, idPerson, idCampaignSheet, esito) {
    var msg = "";
    if (esito) {
        msg = "Richiesta approva con successo";
    }
    else {
        msg = "Richiesta rifiutata con successo";
    }
        
    RaiSubmitForm(button, idForm,
                    function () {
                        var obj = new FormData($('#' + idForm)[0]);
                        obj.append('Approved', esito);
                        return obj;
                    }, false, false,
                    msg,
                    function () {
                        ValUpdateValutazioniSheet(idCampaignSheet);
                        ValModal_Valutazione(idEvaluator, idPerson, false);
                    }, true);
}
//#endregion

//#region PianoSviluppo
function ValCheckPiano(showAlert) {
    debugger
    var lenForza = $('#PuntiForza').val().length;
    var lenMigl = $('#PuntiMiglioramento').val().length;

    if (lenForza > 0 && lenForza < 2) {
        if (showAlert) {
            swal({
                title: "Attenzione",
                text: "Selezionare almeno due punti di forza",
                type: 'error',
                customClass: 'rai'
            });
        }
        return false;
    }
    if (lenMigl > 0 && lenMigl < 2) {
        if (showAlert) {
            swal({
                title: "Attenzione",
                text: "Selezionare almeno due punti di miglioramento",
                type: 'error',
                customClass: 'rai'
            });
        }
        return false;
    }

    return true;
}

function ValAddPunto(button) {
    event.preventDefault();
    var puntoType = $(button).attr('data-punto');
    var otherType = puntoType == "Forza" ? "Miglioramento" : "Forza";
    var isChecked = $(button).attr('data-punto-checked');

    var idGroup = $(button).parent().parent().attr('data-punto-gr')

    if (isChecked == "true") {
        var removeButton = $('#tblPunti' + puntoType + ' tr[data-punto-gr="' + idGroup + '"] button');
        ValRimuoviPunto(removeButton);
        return;
    }
    else {
        $(button).attr('data-punto-checked', 'true');
        $(button).parent().parent().find('div [data-punto="' + otherType + '"]').attr('data-punto-checked', 'false');
    }


    var qstGroup = $('td[data-text-gr="' + idGroup + '"] > span').html();

    var trOther = $('#tblPunti' + otherType).find('tr[data-punto-gr="' + idGroup + '"]');
    if (trOther.length > 0) {
        $(trOther).remove();
        $('#Punti' + otherType + '  option[value="' + qstGroup + '"]').remove();
    }

    var qstGroup = $('td[data-text-gr="' + idGroup + '"] > span').html();
    var qstText = qstGroup;
    $('#tblPunti' + puntoType).append('<tr data-punto-gr="' + idGroup + '" data-punto="' + puntoType + '" data-toggle="tooltip" data-html="true" data-delay="500" title="' + qstText + '">' +
        '<td style="max-width:200px;white-space:nowrap;text-overflow:ellipsis;overflow:hidden;">' +
        '<span class="rai-font-md val-gr" >' + qstGroup + '</span></td > ' +
        '<td class="rai-table-td-action"><button class="btn btn-action-icon feedback-error-color" onclick="ValRimuoviPunto(this)"><i class="fa fa-times"></i></button></td>' +
        '</tr > ');

    $('[data-toggle="tooltip"]').tooltip();

    $('#Punti' + puntoType).append('<option value="' + idGroup+'|'+qstGroup + '">' + qstGroup + '<br/>' + qstText + '</option>');
    $('#Punti' + puntoType + ' option[value="' + idGroup + '|' +qstGroup + '"]')[0].selected = true;


    ValCheckAbilButton();
}
function ValRimuoviPunto(button) {
    var tr = $(button).closest('tr');
    var idQuestion = $(tr).attr('data-punto-gr');
    var puntoType = $(tr).attr('data-punto');
    var otherType = puntoType == "forza" ? "miglioramento" : "forza";

    var qstGroup = $(tr).find('span.val-gr').html();

    $(tr).remove();

    $('#Punti' + puntoType + '  option[value="' + qstGroup + '"]').remove();

    $('div[data-punto-gr="' + idQuestion + '"] button[data-punto="' + puntoType + '"]').attr('data-punto-checked', 'false');

    ValCheckAbilButton();
}
function ValCheckAbilButton() {
    var puntoType = "Forza";
    var otherType = "Miglioramento";

    if ($('tr[data-punto="' + puntoType + '"]').length == 3)
        $('button[data-punto="' + puntoType + '"]').attr('disabled', 'disabled');
    else
        $('button[data-punto="' + puntoType + '"]').removeAttr('disabled');

    if ($('tr[data-punto="' + otherType + '"]').length == 3)
        $('button[data-punto="' + otherType + '"]').attr('disabled', 'disabled');
    else
        $('button[data-punto="' + otherType + '"]').removeAttr('disabled');
}
//#endregion

//Esami
function ExamOpenDipModal(codExam) {
    RaiOpenAsyncModal('modal-gest-half', '/Esami/Modal_Dipendente', { codExam: codExam }, null, "POST");
}

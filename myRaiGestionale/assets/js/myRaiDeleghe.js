function Deleghe_VisualizzaDeleghe(matricola, area, funzioni) {
    $('#modal-Modale_Delega-internal').html('');
    $('#modal-Modale_GestioneDelega-internal').html('');
    RaiOpenAsyncModal('modal-Modale_GestioneDelega', '/Deleghe/ElenchiDeleghe', { m: matricola, area: area, COD_FUNZIONI: funzioni }, null, 'POST');
}

function Deleghe_CreaDelega(matricola, area, funzioni) {
    $('#modal-Modale_Delega-internal').html('');
    RaiOpenAsyncModal('modal-Modale_Delega', '/Deleghe/GetModalCreaDelega', { m: matricola, area: area, COD_FUNZIONI: funzioni }, null, 'POST');
}

function Deleghe_AbilitaButton_Tab_1() {
    var nomeDelega = $('#NomeDelega').val();
    var matDelegato = $('#MatricolaDelegato').val();
    var dataDal = $('#DataInizioDelega').val();
    var dataA = $('#DataFineDelega').val();

    nomeDelega = $.trim(nomeDelega);
    matDelegato = $.trim(matDelegato);

    if (nomeDelega !== null && nomeDelega !== "" &&
        matDelegato !== null && matDelegato !== "" &&
        dataDal !== null && dataDal !== "" &&
        dataA !== null && dataA !== "") {
        $('#btns-fase1-next').removeClass('disable');
    }
}

function Deleghe_btns_fase1_next_Click() {
    $('#deleghe-tab1').removeClass('active');
    $('#deleghe-tab1').addClass('disable');
    $('#deleghe-tab1').addClass('completed');
    $('#btns-fase1').hide();
    $('#sez-1').hide();
    $('#deleghe-tab1').removeClass('disable');
    $('#deleghe-tab2').addClass('active');    
    $('#sez-2').show();
    $('#btns-fase2').show();
}

function Deleghe_btns_fase2_prev_Click() {
    $('#btns-fase2').hide();
    $('#sez-2').hide();
    $('#deleghe-tab2').removeClass('active');
    $('#deleghe-tab1').removeClass('completed');
    $('#deleghe-tab1').addClass('active');
    $('#sez-1').show();
    $('#btns-fase1').show();
}

function Deleghe_btns_fase2_next_Click() {
    var ids = new Array();
    $('input[type="checkbox"][id^="Delega_check_"]:checked').each(function () {
        ids.push($(this).val());
    });

    var nomeDelega = $('#NomeDelega').val();
    var matDelegato = $('#MatricolaDelegato').val();
    var dataDal = $('#DataInizioDelega').val();
    var dataA = $('#DataFineDelega').val();

    nomeDelega = $.trim(nomeDelega);
    matDelegato = $.trim(matDelegato);

    $.ajax({
        url: "/Deleghe/SetScelte",
        type: "POST",
        data: JSON.stringify({
            matricolaDelegato: matDelegato,
            nomeDelega: nomeDelega,
            dataDal: dataDal,
            dataA: dataA,
            abilitazioni: ids
        }),
        async:false,
        contentType: "application/json; charset=utf-8",
        dataType: 'html',
        success: function (data) {
            $('#deleghe-div-riepilogo').html(data);
            $('#deleghe-tab2').removeClass('active');
            $('#deleghe-tab2').addClass('disable');
            $('#deleghe-tab2').addClass('completed');
            $('#btns-fase2').hide();
            $('#sez-2').hide();
            $('#deleghe-tab2').removeClass('disable');
            $('#deleghe-tab3').addClass('active');
            $('#sez-3').show();
            $('#btns-fase3').show();
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
}

function Deleghe_AbilitaButton_Tab_2() {
    var selezionati = $('input[type="checkbox"][id^="Delega_check_"]:checked').length;

    if (selezionati > 0) {
        $('#btns-fase2-next').removeClass('disable');
    }
    else {
        if (!$('#btns-fase2-next').hasClass('disable')) {
            $('#btns-fase2-next').addClass('disable');
        }
    }
}

function Deleghe_VisualizzaRiepilogo(data) {
    $('#deleghe-tab2').removeClass('active');
    $('#deleghe-tab2').addClass('disable');
    $('#deleghe-tab2').addClass('completed');
    $('#btns-fase2').hide();
    $('#sez-2').hide();
    $('#deleghe-tab3').removeClass('disable');
    $('#deleghe-tab3').addClass('active');
    $('#sez-3').show();
    $('#btns-fase3').show();
}

function Deleghe_btns_fase3_finish_Click(matricolaDelegante) {
    swal({
        title: 'Sicuro di voler procedere col salvataggio dei dati?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, continua',
        cancelButtonText: 'No, annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var ids = new Array();
        $('input[type="checkbox"][id^="Delega_check_"]:checked').each(function () {
            ids.push($(this).val());
        });

        var nomeDelega = $('#NomeDelega').val();
        var matDelegato = $('#MatricolaDelegato').val();
        var dataDal = $('#DataInizioDelega').val();
        var dataA = $('#DataFineDelega').val();

        nomeDelega = $.trim(nomeDelega);
        matDelegato = $.trim(matDelegato);

        $.ajax({
            url: "/Deleghe/ConfermaCreazioneDelega",
            type: "POST",
            data: JSON.stringify({
                matricolaDelegato: matDelegato,
                nomeDelega: nomeDelega,
                dataDal: dataDal,
                dataA: dataA,
                abilitazioni: ids
            }),
            async: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data && data.Esito) {
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
                        title: data.Errore,
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
                RaiUpdateWidget('div_deleghe_concesse', '/Deleghe/GetContentDelegheConcesse', 'html', { matricola: matricolaDelegante }, false, null, false, 'GET');
                $('#modal-Modale_Delega').modal('hide');
            }
        });
    });
}

function Deleghe_btns_fase3_prev_Click() {
    $('#btns-fase3').hide();
    $('#sez-3').hide();
    $('#deleghe-tab3').removeClass('active');
    $('#deleghe-tab2').removeClass('completed');
    $('#deleghe-tab2').addClass('active');
    $('#sez-2').show();
    $('#btns-fase2').show();
}

function Deleghe_VisualizzaDelega(idDelega) {
    $('#modal-Modale_Delega-internal').html('');
    RaiOpenAsyncModal('modal-Modale_Delega', '/Deleghe/GetModalDettaglioDelega', { idDelega: idDelega }, null, 'POST');
}

function Deleghe_EliminaDelega(idDelega) {
    swal({
        title: 'Sicuro di voler revocare la delega selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, elimina',
        cancelButtonText: 'No, annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Deleghe/EliminaDelega",
            type: "POST",
            async: false,
            data: JSON.stringify({
                idDelega: idDelega
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {

                if (data && data.Esito) {
                    swal({
                        title: 'Delega eliminata correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    }).then(function () {
                            RaiUpdateWidget('div_deleghe_concesse', '/Deleghe/GetContentDelegheConcesse', 'html', { }, false, null, false, 'GET');
                    });
                }
                else {
                    swal({
                        title: data.Errore,
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

function Deleghe_EsercitaDelega(idDelega) {
    swal({
        title: 'Sicuro di voler attivare la delega selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, attiva',
        cancelButtonText: 'No, annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Deleghe/EsercitaDelega",
            type: "POST",
            async: false,
            data: JSON.stringify({
                idDelega: idDelega
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {

                if (data && data.Esito) {
                    swal({
                        title: 'Delega attivata correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    }).then(function () {
                        RaiUpdateWidget('div_deleghe_ricevute', '/Deleghe/GetContentDelegheRicevute', 'html', { }, false, null, false, 'GET');            
                    });
                }
                else {
                    swal({
                        title: data.Errore,
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

function Deleghe_AnnullaEsercizioDelega(idDelega) {
    swal({
        title: 'Sicuro di voler disattivare la delega selezionata?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, attiva',
        cancelButtonText: 'No, annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/Deleghe/FermaEsercizioDelega",
            type: "POST",
            async: false,
            data: JSON.stringify({
                idDelega: idDelega
            }),
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {

                if (data && data.Esito) {
                    swal({
                        title: 'Delega disattivare correttamente',
                        type: 'success',
                        showConfirmButton: true,
                        confirmButtonText: 'Ok',
                        customClass: 'rai'
                    }).then(function () {
                        RaiUpdateWidget('div_deleghe_ricevute', '/Deleghe/GetContentDelegheRicevute', 'html', {}, false, null, false, 'GET');
                    });
                }
                else {
                    swal({
                        title: data.Errore,
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

function Deleghe_ChiudiModaleNuovaDelega() {
    $('#modal-Modale_Delega').modal('hide');
    $('#modal-Modale_Delega-internal').html('');
}

function Deleghe_ChiudiModaleElencoDeleghe() {
    $('#modal-Modale_GestioneDelega').modal('hide');
    $('#modal-Modale_GestioneDelega-internal').html('');
}

function Deleghe_CreaDelegaNoWizard(matricola, area, funzioni) {
    $('#modal-Modale_Delega-internal').html('');
    RaiOpenAsyncModal('modal-Modale_Delega', '/Deleghe/GetModalCreaDelegaNoWizard', { m: matricola, area: area, COD_FUNZIONI: funzioni }, null, 'POST');
}

function Deleghe_AbilitaBtnSalva() {
    var matDelegato = $('#MatricolaDelegato').val();
    var dataDal = $('#DataInizioDelega').val();
    var dataA = $('#DataFineDelega').val();

    matDelegato = $.trim(matDelegato);

    if (matDelegato !== null && matDelegato !== "" &&
        dataDal !== null && dataDal !== "" &&
        dataA !== null && dataA !== "") {
        $('#deleghe-btn-salva-delega').removeClass('disable');
    }
}

function Deleghe_btn_salva_delega_Click() {
    swal({
        title: 'Sicuro di voler salvare la delega?',
        type: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sì, continua',
        cancelButtonText: 'No, annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {

        var ids = new Array();
        $('input[type="checkbox"][id^="Delega_check_"]:checked').each(function () {
            ids.push($(this).val());
        });

        var nomeDelega = $('#NomeDelega').val();
        var matDelegato = $('#MatricolaDelegato').val();
        var dataDal = $('#DataInizioDelega').val();
        var dataA = $('#DataFineDelega').val();

        nomeDelega = $.trim(nomeDelega);
        matDelegato = $.trim(matDelegato);

        $.ajax({
            url: "/Deleghe/ConfermaCreazioneDelega",
            type: "POST",
            data: JSON.stringify({
                matricolaDelegato: matDelegato,
                nomeDelega: nomeDelega,
                dataDal: dataDal,
                dataA: dataA,
                abilitazioni: ids
            }),
            async: false,
            contentType: "application/json; charset=utf-8",
            dataType: 'json',
            success: function (data) {
                if (data && data.Esito) {
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
                        title: data.Errore,
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
                RaiUpdateWidget('div_deleghe_concesse', '/Deleghe/GetContentDelegheConcesse', 'html', { }, false, null, false, 'GET');
                $('#modal-Modale_Delega').modal('hide');
            }
        });
    });
}

function Deleghe_ChiudiModaleDettaglioDelega() {
    $('#modal-Modale_Delega').modal('hide');
    $('#modal-Modale_Delega-internal').html('');
}


function Deleghe_AggiornaConteggioDeleghe() {
    $.ajax({
        url: "/Deleghe/AggiornaConteggioDeleghe",
        type: "POST",
        data: JSON.stringify({
        }),
        async: false,
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (data) {
            if (data && data.Esito) {
                var valore = data.Obj;
                valore = parseInt(valore, 10);
                if (valore === 1) {
                    $('#deleghe-messaggio-conteggio-deleghe-1').text('Hai ');
                    $('#deleghe-messaggio-conteggio-deleghe-2').text(valore);
                    $('#deleghe-messaggio-conteggio-deleghe-3').text(' delega attiva');
                }
                else if (valore === 0)
                {
                    $('#deleghe-messaggio-conteggio-deleghe-1').text('Non hai deleghe attive');
                    $('#deleghe-messaggio-conteggio-deleghe-2').text('');
                    $('#deleghe-messaggio-conteggio-deleghe-3').text('');
                }
                else {
                    $('#deleghe-messaggio-conteggio-deleghe-1').text('Hai ');
                    $('#deleghe-messaggio-conteggio-deleghe-2').text(valore);
                    $('#deleghe-messaggio-conteggio-deleghe-3').text(' deleghe attive');
                }
            }
            else {
                swal({
                    title: data.Errore,
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
        }
    });
}
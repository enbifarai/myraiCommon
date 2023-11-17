//--> Modale inserimento dati e gestione delle scelte
function OpenModalSceltaDestinazioneTFR(matricola, idPersona, idGestione) {
    // Verifico presenza utente
    $.ajax({
        type: 'POST',
        cache: false,
        url: "/SceltaDestinazioneTfr/VerificaPresenzaDatiUtente",
        data: { matricolaUtente: matricola, idUtente: idPersona, idRecordScelta: idGestione },
        success: function (data) {
            //console.log(data);
            if (data == 'True') {
                // Aggiorno il content del div principale
                RaiOpenAsyncModal('modal-sceltaDestinazioneTfr', '/SceltaDestinazioneTfr/GetModuloSceltaDestinazioneTfr', { matricolaUtente: matricola, idUtente: idPersona, idRecordScelta: idGestione }, null, 'POST');
            } else {
                swal({
                    title: 'Dati utente non presenti',
                    type: 'error',
                    confirmButtonClass: "btn btn-primary btn-lg",
                    showConfirmButton: true,
                    confirmButtonText: 'Ok',
                    customClass: 'rai'
                });
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            swal({
                title: jqXHR.statusText,
                type: 'error',
                confirmButtonClass: "btn btn-primary btn-lg",
                showConfirmButton: true,
                confirmButtonText: 'Ok',
                customClass: 'rai'
            });
        }
    });
}

function sceltaDestinazioneTFR_ChiudiInserimento() {
    $('#modal-sceltaDestinazioneTfr').modal('hide');
}

function sceltaDestinazioneTFR_VisualizzaFile(idAllegato) {
    RaiOpenAsyncModal('modal-viewer-temp-allegati', '/SceltaDestinazioneTfr/GetAllegatoTemporaneo', { idAllegato: idAllegato }, null, 'GET');
}
// <--


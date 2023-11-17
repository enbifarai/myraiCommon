function ShowPopUpEvento() {
    $("#page-loaderR").show();
    $.ajax({
        url: '/eventi/_nuovoEvento',
        type: "GET",
        dataType: "html",
        data: {},
        success: function () {
            $('#modal-popin-nuovo').modal('show');
            $("#page-loaderR").hide();
        },
    });
}

function ShowPopUpProgramma() {
    $("#page-loaderR").show();
    $.ajax({
        url: '/eventi/_nuovoProgramma',
        type: "GET",
        dataType: "html",
        data: {},
        success: function () {
            $('#modal-popin-nuovo').modal('show');
            $("#page-loaderR").hide();
        },
    });
}

function ShowPopUpModEvento(idEvents) {
    $("#page-loaderR").show();
    $.ajax({
        url: '/eventi/_listaEvento?idevento=' + idEvents,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            $('#modal').html(data);
            resetdateTimePicker();
            resetSubmitEvento();
            $('#modal-popin-modifica').modal('show');
            $("#page-loaderR").hide();
            $("#dataNascita").datetimepicker({
                viewMode: 'years',
                format: 'DD/MM/YYYY',
                maxDate: 0
            });

            $('#datanascita').on('dp.hide', function (event) {
                setTimeout(function () {
                    $('#dataNascita').data('DateTimePicker').viewMode('years');
                }, 1);
            });
            $('#datanascita').data("DateTimePicker").maxDate(new Date());

        }

    });
}

function ShowPopUpModProgramma(idPrograms) {
    $("#page-loaderR").show();
    $.ajax({
        url: '/eventi/_listaProgramma?idProgramma=' + idPrograms,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            $('#modal').html(data);
            resetdateTimePicker();
            resetSubmitProgramma();
            $('#modal-popin-modifica').modal('show');
            $("#page-loaderR").hide();
        }

    });
}

function ModificaEvento(form) {
    var serializedForm = form.serialize();
    $.ajax({
        type: 'POST',
        url: "/eventi/_modificaEvento",
        dataType: "json",
        data: serializedForm,
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaEventiJS();
                $('#modal-popin-modifica').modal('hide');
                swal("Modifica effettuata", "L'evento è stato modificato", "success");

            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function ModificaProgramma(form) {
    var serializedForm = form.serialize();
    $.ajax({
        type: 'POST',
        url: "/eventi/_modificaProgramma",
        dataType: "json",
        data: serializedForm,
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaProgrammiJS();
                $('#modal-popin-modifica').modal('hide');
                swal("Modifica effettuata", "Il programma è stato modificato", "success");
                $("#refresh-prog").click();
            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function InserisciEvento(form) {
    var serializedForm = form.serialize();

    $.ajax({
        type: 'POST',
        url: "/eventi/_inserimentoEvento",
        dataType: "json",
        data: serializedForm,
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaEventiJS();
                $('#modal-popin-nuovo').modal('hide');
                swal("Inserimento effettuato", "L'evento è stato creato", "success");

            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function InserisciProgramma(form) {
    var serializedForm = form.serialize();
    $.ajax({
        type: 'POST',
        url: "/eventi/_inserimentoProgramma",
        dataType: "json",
        data: serializedForm,
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaProgrammiJS();
                $('#modal-popin-nuovo').modal('hide');
                swal("Inserimento effettuato", "Il programma è stato creato", "success");
                $("#refresh-prog").click();
            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function CancellaEvento(idEvents) {

    $.ajax({
        type: 'GET',
        url: "/eventi/cancellaEvento",
        dataType: "json",
        data: { idEvento: idEvents },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaEventiJS();
                swal("Cancellazione effettuata", "L'evento è stato cancellato", "success");
            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function CancellaProgramma(idPrograms) {

    $.ajax({
        type: 'GET',
        url: "/eventi/cancellaProgramma",
        dataType: "json",
        data: { idProgramma: idPrograms },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshListaProgrammiJS();
                swal("Cancellazione effettuata", "Il programma è stato cancellato", "success");
            }
            else {
                swal('Errore', data.result, 'error');
            }
        },
        error: function (a, b, c) { }
    });
}

function resetSubmitEvento() {
    $('#form-evento-mod').submit(function (e) {
        e.preventDefault();
        var form = $(this);
        ModificaEvento(form);
    });
}

function resetSubmitProgramma() {
    $('#form-programma-mod').submit(function (e) {
        e.preventDefault();
        var form = $(this);
        ModificaProgramma(form);
    });;
}

function resetdateTimePicker() {

    jQuery('.js-datetimepicker').each(function () {
        var $input = jQuery(this);

        $input.datetimepicker({
            format: $input.data('format') ? $input.data('format') : false,
            useCurrent: $input.data('use-current') ? $input.data('use-current') : false,
            locale: moment.locale('' + ($input.data('locale') ? $input.data('locale') : '') + ''),
            showTodayButton: $input.data('show-today-button') ? $input.data('show-today-button') : false,
            showClear: $input.data('show-clear') ? $input.data('show-clear') : false,
            showClose: $input.data('show-close') ? $input.data('show-close') : false,
            sideBySide: $input.data('side-by-side') ? $input.data('side-by-side') : false,
            inline: $input.data('inline') ? $input.data('inline') : false,
            //formatData: $input.data('formatData') ? $input.data('formatData') : false,
            //minData:  $input.data('minData') ? $input.data('minData') : false,

            icons: {
                time: 'si si-clock',
                date: 'si si-calendar',
                up: 'si si-arrow-up',
                down: 'si si-arrow-down',
                previous: 'icons icon-arrow-left',
                next: 'icons icon-arrow-right',
                today: 'si si-size-actual',
                clear: 'si si-trash',
                close: 'si si-close'
            }
        })
        .on("dp.change", function (e) {
            $input.change();
        });
    });

}
$(document).ready(function () {
    $(".js-example-basic-single").select2()
   
});


function cercaOmaggi(tipoutente) {
    var otipo = $("#tipoOmaggio").val();
    var odatada = $("#datada").val();
    var odataa = $("#dataal").val();
    var matr = $("#Matricola").val();
    var vda = $('#Valoreda').val();
    var va = $('#Valorea').val();
    if (parseFloat($('#Valorea').val()) < parseFloat($('#Valoreda').val())) {
        swal('Errore', 'Range di valori non valido', 'error');
        return false;
    }
    $.ajax({
        url: '/Omaggi/RicercaOmaggiAjax',
        type: "GET",
//        dataType: "html",
        data: { chiamante: tipoutente, tipo: otipo, datada: odatada, dataa: odataa, matricola: matr, valda: vda, vala: va },
        success: function (data) {
            var t = $("#elencoomaggi");
            $(t).html("");
            $(t).html(data);
        }
    });
}

$('#form-abbonamenti').submit(function (e) {
    e.preventDefault();
    var form = $(this);

    
        $.ajax({
            url: '/CampagnaAbbonamenti/Esporta',
            type: "POST",
            data: { idabbonamenti: $("#id_abbonamenti").val(), cittaAbbonamento: $("cittaabb").val() },
            dataType: "json",
            complete: function () { },
            success: function (data) {

            }
        });
    


});
function cercaAbbonamenti() {
    
    var odatada = $("#datada").val();
    var odataa = $("#dataal").val();
    var ocitta = $("#CittaAbbonamento").val();
    var onome = $("#nome").val();
    var ocognome = $("#cognome").val();
    var vettore = $("#VettoreAbbonamento").val();
    
    $.ajax({
        url: '/CampagnaAbbonamenti/RicercaAbbonamentiAjax',
        type: "GET",
        dataType: "html",
        data: {  datada: odatada, dataa: odataa,citta:ocitta,nome:onome,cognome:ocognome, vettore:vettore},
        success: function (data) {
            var t = $("#elencoabbonamenti");
            $(t).html("");
            $(t).html(data);
        }
    });
}
var form = $('#fricercaomaggio');
$(':input[type!="submit"]', form.get(0)).on('change', function (e) {
    //if ($("#tipoOmaggio").val() == "" && 
    //    $("#datada").val() == "" &&
    //    $("#dataal").val() == "" &&
    //    ($("#utentiOmaggio option:selected").val() == "" || $("#utentiOmaggio option:selected").val() == null) &&
    //    ($("#Valoreda").val() == "" || $("#Valoreda").val() == null) &&
    //    ($("#Valorea").val() == "" || $("#Valorea").val() == null)
    //    )
    //     {
    //    form.find(':submit').attr("disabled", true);
    //}
    // else {
    if (e.currentTarget.name == "datada") {
        if ($("#datada").val() != "") {
            $("#dataal").data("DateTimePicker").minDate($("#datada").val());
        }
        else {
            $("#dataal").data("DateTimePicker").minDate("01/01/1900");
        }
    }
    if (e.currentTarget.name == "dataal") {
        if ($("#dataal").val() != "") {
            $("#datada").data("DateTimePicker").maxDate($("#dataal").val());
        }
        else {
            $("#datada").data("DateTimePicker").maxDate(null);
        }
    }

    //    form.find(':submit').removeAttr('disabled');

    //  }
});
var form = $('#fricercaabbonamenti');
$(':input[type!="submit"]', form.get(0)).on('change', function (e) {
    //if ($("#tipoOmaggio").val() == "" && 
    //    $("#datada").val() == "" &&
    //    $("#dataal").val() == "" &&
    //    ($("#utentiOmaggio option:selected").val() == "" || $("#utentiOmaggio option:selected").val() == null) &&
    //    ($("#Valoreda").val() == "" || $("#Valoreda").val() == null) &&
    //    ($("#Valorea").val() == "" || $("#Valorea").val() == null)
    //    )
    //     {
    //    form.find(':submit').attr("disabled", true);
    //}
    // else {
    if (e.currentTarget.name == "datada") {
        if ($("#datada").val() != "") {
            $("#dataal").data("DateTimePicker").minDate($("#datada").val());
        }
        else {
            $("#dataal").data("DateTimePicker").minDate("01/01/1900");
        }
    }
    if (e.currentTarget.name == "dataal") {
        if ($("#dataal").val() != "") {
            $("#datada").data("DateTimePicker").maxDate($("#dataal").val());
        }
        else {
            $("#datada").data("DateTimePicker").maxDate(null);
        }
    }

    //    form.find(':submit').removeAttr('disabled');

    //  }
});

function ConfermaCancellazione_Omaggio(prog) {
    swal({
        title: "Sicuro di voler cancellare l'omaggio?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        window.location.href = "/Omaggi/DeleteOmaggio?prog=" + prog;
    });
}

function ConfermaCancellazione_Abbonamento( prog )
{
    swal({
        title: "Sicuro di voler cancellare la richiesta di abbonamento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        window.location.href = "/Abbonamenti/DeleteAbbonamento?prog=" + prog;
    });
}
function ConfermaGestCancellazione_Abbonamento(prog) {
    swal({
        title: "Sicuro di voler cancellare la richiesta di abbonamento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        window.location.href = "/CampagnaAbbonamenti/DeleteAbbonamento?prog=" + prog;
    });
}

function ConfermaCancellazione_Campagna( prog )
{
    swal({
        title: "Sicuro di voler cancellare la campagna di abbonamento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        window.location.href = "/CampagnaAbbonamenti/DeleteCampagna?prog=" + prog;
    });
}

$(function () {
    $("#datepicker").datepicker({
        changeMonth: true,
        changeYear: true,
        yearRange: '-115:+0M',
        onSelect: function (date) { // bind the builtin onSelect event
            // which gets you the selected date
            var selYear = new Date(date).getFullYear(); // get the full year of selected date 
            var currYear = new Date().getFullYear(); // get the current year

            if ((currYear - selYear) > 10) { // check in the if conditon for 10 years
                $('#display_error').html('You should choose dates from last 10 years only.');
                this.value = '';
            }
        }

    });
});

function ShowGestisciAbbonamento(ope, abb) {
    RaiOpenAsyncModal("modal-abbonamento", "/Abbonamenti/ShowGestisciAbbonamento", { idAbbonamento: abb, cittaAbbonamento: $("#CittaAbbonamento").val() }, function () {
        $("#giorno_inizio").datetimepicker({
            format: 'DD/MM/YYYY'
        });

        $("#giorno_inizio").data('DateTimePicker').minDate($("#giorno_inizio").val());
        var maxDateGiorno = new Date($("#giorno_inizio").data('DateTimePicker').minDate());
        maxDateGiorno.setMonth(maxDateGiorno.getMonth() + 1);
        maxDateGiorno.setDate(maxDateGiorno.getDate() - 1);

        $("#giorno_inizio").data('DateTimePicker').maxDate(maxDateGiorno);

        $('#data_rilascio').on('dp.hide', function (event) {
            setTimeout(function () {
                $('#data_rilascio').data('DateTimePicker').viewMode('years');

            }, 1);
        });
        if ($("#Abbonamenti_0__Nome").val() == "")
            $("#Abbonamenti_0__Nome").attr("readonly", false);

        if ($("#Abbonamenti_0__Cognome").val() == "")
            $("#Abbonamenti_0__Cognome").attr("readonly", false);

        if ($("#data_nascita").val() == "" || $("#data_nascita").val() == "01/01/0001") {
            $("#data_nascita").val("");
            $("#data_nascita").attr("readonly", false);
        }

        if ($("#Abbonamenti_0__ComuneNascita").val() == "")
            $("#Abbonamenti_0__ComuneNascita").attr("readonly", false);

        if ($("#Abbonamenti_0__ProvinciaNascita").val() == "")
            $("#Abbonamenti_0__ProvinciaNascita").attr("readonly", false);

        if ($("#Abbonamenti_0__Genere").val() == "")
            $("#Abbonamenti_0__Genere").attr("readonly", false);

        if ($("#Abbonamenti_0__CodiceFiscale").val() == "")
            $("#Abbonamenti_0__CodiceFiscale").attr("readonly", false);

        if ($("#Abbonamenti_0__Indirizzo").val() == "")
            $("#Abbonamenti_0__Indirizzo").attr("readonly", false);

        if ($("#Abbonamenti_0__Cap").val() == "")
            $("#Abbonamenti_0__Cap").attr("readonly", false);

        if ($("#Abbonamenti_0__Comune").val() == "")
            $("#Abbonamenti_0__Comune").attr("readonly", false);

        if ($("#Abbonamenti_0__Provincia").val() == "")
            $("#Abbonamenti_0__Provincia").attr("readonly", false);

        if ($("#Abbonamenti_0__Nazionalita").val() == "")
            $("#Abbonamenti_0__Nazionalita").attr("readonly", false);

        if ($("#Abbonamenti_0__Email").val() == "")
            $("#Abbonamenti_0__Email").attr("readonly", false);




        //  });
        if (ope == 'v') {
            $("#form-gestioneabbonamento .disabilita").attr("disabled", "disabled");
            $('input[type=checkbox]').attr('disabled', 'true');
            $("#checkbox-email").removeAttr('disabled');
            $("#btnInserisciAbbonamento").hide();
        } else {
            //$('#modal-omaggio').modal("show");


            UIRai.initHelpers('datetimepicker');
            if ($("#Policy").val() == "False") {
                RiepilogoAbbonamenti();
            }

        }
    });
}

function ShowGestisciAbbonamentoExtra() {

    if ($("#matricola").val() == "")
    {
        
        swal({
            title: "",
            text: "Inserire la matricola",
            type: "error"
        }).then(function () {
            $('#modal-abbonamento').hide();
        });

        return;
    }

    if ($("#CittaAbbonamentoExtra").val() == "") {
        
        swal({
            title: "",
            text: "Inserire la città dell'abbonamento",
            type: "error"
        }).then(function () {
            $('#modal-abbonamento').hide();
        });

        return;
    }

    


    $.ajax({
        url: "/Abbonamenti/ShowGestisciAbbonamentoExtra",
        type: "GET",
        data: { matricola: $("#matricola").val(), CittaAbbonamento: $("#CittaAbbonamentoExtra :selected").text(), InviaMail: $("#checkbox-email")[0].checked},
        async: false,
        cache: false,
        success: function (data) {
            $('#modal-abbonamento').html(' ');
            $('#modal-abbonamento').html(data);

            $("#giorno_inizio").datetimepicker({
                format: 'DD/MM/YYYY'
            });
            $("#giorno_inizio").data('DateTimePicker').minDate($("#giorno_inizio").val());
            var maxDateGiorno=new Date($("#giorno_inizio").data('DateTimePicker').minDate());
            maxDateGiorno.setMonth(maxDateGiorno.getMonth() + 1);
            maxDateGiorno.setDate(maxDateGiorno.getDate() - 1);

            $("#giorno_inizio").data('DateTimePicker').maxDate(maxDateGiorno);

            $('#data_rilascio').on('dp.hide', function (event) {
                setTimeout(function () {
                    $('#data_rilascio').data('DateTimePicker').viewMode('years');

                }, 1);
            });

            if ($("#Abbonamenti_0__Nome").val() == "")
                $("#Abbonamenti_0__Nome").attr("readonly", false);

            if ($("#Abbonamenti_0__Cognome").val() == "")
                $("#Abbonamenti_0__Cognome").attr("readonly", false);

            if ($("#Abbonamenti_0__DataNascita").val() == "")
                $("Abbonamenti_0__DataNascita").attr("readonly", false);

            if ($("#Abbonamenti_0__ComuneNascita").val() == "")
                $("#Abbonamenti_0__ComuneNascita").attr("readonly", false);

            if ($("#Abbonamenti_0__ProvinciaNascita").val() == "")
                $("#Abbonamenti_0__ProvinciaNascita").attr("readonly", false);

            if ($("#Abbonamenti_0__Genere").val() == "")
                $("#Abbonamenti_0__Genere").attr("readonly", false);

            if ($("#Abbonamenti_0__CodiceFiscale").val() == "")
                $("#Abbonamenti_0__CodiceFiscale").attr("readonly", false);

            if ($("#Abbonamenti_0__Indirizzo").val() == "")
                $("#Abbonamenti_0__Indirizzo").attr("readonly", false);

            if ($("#Abbonamenti_0__Cap").val() == "")
                $("#Abbonamenti_0__Cap").attr("readonly", false);

            if ($("#Abbonamenti_0__Comune").val() == "")
                $("#Abbonamenti_0__Comune").attr("readonly", false);

            if ($("#Abbonamenti_0__Provincia").val() == "")
                $("#Abbonamenti_0__Provincia").attr("readonly", false);

            if ($("#Abbonamenti_0__Nazionalita").val() == "")
                $("#Abbonamenti_0__Nazionalita").attr("readonly", false);

            if ($("#Abbonamenti_0__Email").val() == "")
                $("#Abbonamenti_0__Email").attr("readonly", false);


          
          UIRai.initHelpers('datetimepicker');
            
        

        },
        error: function (result, a, b) {
            

            swal({
                title: "",
                text: "Matricola non presente",
                type: "error"
            }).then(function () {
                $('#modal-abbonamento').hide();
            });

        }
    })
}

         
function ShowGestisciCampagna(ope, cam) {
    $.ajax({
        url: "/CampagnaAbbonamenti/ShowGestisciCampagna",
        type: "GET",
        data: { idCampagna: cam},
        async: false,
        cache: false,
        success: function (data) {
            $('#modal-campagna').html(' ');
            $('#modal-campagna').html(data);

            //$('#modal-omaggio').modal("show");
            UIRai.initHelpers('datetimepicker');
            if (ope == "v") {
                $("#form-gestionecampagnaabbonamento .disabilita").attr("disabled", "disabled");
                $('input[type=checkbox]').attr('disabled', 'true');
                $("#checkbox-email").removeAttr('disabled');
                $("#btnConfermaAbbonamento").hide();
            }
            if (ope == "t") {
                $('input[type=checkbox]').attr('disabled', 'true');
                $("#data_iniziocampagna").attr('disabled', 'true');
                $("#data_finecampagna").data("DateTimePicker").minDate($("#data_finecampagna").val());
                
                
                
            }            

        }
    })
}

function ShowGestisciOmaggio(ope, omag) {
    $.ajax({
        url: "/Omaggi/ShowGestisciOmaggio",
        type: "GET",
        data: { idOmaggio: omag },
        async: false,
        success: function (data) {
            $('#modal-omaggio').html(' ');
            $('#modal-omaggio').html(data);
            //$('#modal-omaggio').modal("show");
            UIRai.initHelpers('datetimepicker');
            $('#data_ricezione').data("DateTimePicker").maxDate(moment());
            GestisciTipoDenaro();
            GestisciAltroOccasioneDiRicezione();
            RadioAccettoOmaggio($("#Flag_Accetto").is(":checked").toString());
            $('.editOption').attr('style', 'display:none;');

            if (ope != "r") {
                $("#Tipo_id option")[0].disabled = true;
                $("#Motivo_id option")[0].disabled = true;

            }
            else {
                $("#form-gestioneomaggio .disabilita").attr("disabled", "disabled");
                $("#dbtn").hide();
            }


        }
    })
}

function editMotivo()
{
    var selected = $('#Motivo_id option:selected').attr('class');
    var optionText = $('.editable').text();
    $(document).find("span[data-valmsg-for='editOption']").addClass("hidden");
    $(document).find("span[data-valmsg-for='Motivo_id']").addClass("hidden");

    if (selected == "editable") {
        $('.editOption').show();
        $('.editOption').focus();
        CtrInput($('.editOption'));

        $('.editOption').keyup(function () {
            var editText = $('.editOption').val();
            $('.editable').html(editText);
            $('#Motivo_Text').val(editText);
            CtrInput($('.editOption'));
        });

    }
    else {
        $('.editOption').hide();
        $('#Motivo.Text').val(null);
        CtrInput($('#Motivo_id'));
    }

}

function RadioUfficioSpedizioni(che) {

    $("#divufficio").attr("hidden", !che);
    if (che.toLowerCase() == "false") {
        $("#UfficioSpedizioni").val(null);
        $("#divufficio").hide();
        // $("#divufficio").attr("hidden", true);
    }
    else {
        $("#divufficio").show();
        $('#UfficioSpedizioni').focus();
    }
    $("#Flag_UfficioSpedizioni").val(che);

}

function RadioAccettoOmaggio( che )
{
    if (che.toLowerCase() == "false") {
        // $("#divufficio").attr("hidden", true);
        $("#Note_Accetto").prop('required', true);
        $('#Note_Accetto_Error').text('Il Motivo del rifiuto è obbligatorio');
    }
    else {
        $("#Note_Accetto").prop('required', false);
        $('#Note_Accetto_Error').text('');
    }
    
}

function GestisciTipoDenaro()
{
    if ($('#Tipo_id option:selected').text() == "Denaro") {
        $("#ddescrizione").attr("hidden", true);
        $("#dente").attr("hidden", false);
        $("#Descrizione").val(null);
        //CtrInput($("#Ente_Beneficiario"));
    }
    else {
        $("#ddescrizione").attr("hidden", false);
        $("#dente").attr("hidden", true);
        $("#Ente_Beneficiario").val(null);
        //CtrInput($("#Descrizione"));
    }
}

function GestisciAltroOccasioneDiRicezione()
{
    if ($('#Motivo_id option:selected').text() == "Altro") {
        $("#notealtro").attr("hidden", false);
        
    }
    else {
        $("#notealtro").attr("hidden", true);
        $("#Note_Altro").val(null);
    }
}

function selButtonRegistra()
{
    if ($("#checkregistra")[0].checked) {
        $("#btnConfermaAbbonamento")[0].disabled = false;
    } else {
        $("#btnConfermaAbbonamento")[0].disabled = true;
    }
}

function RiepilogoAbbonamenti() {
    
    var citta=$("#CittaAbbonamento").val()

    $.ajax({
        url: "/Abbonamenti/GetTestoPolicy",
        type: "GET",
        async: false,
         data: { CittaAbbonamento: citta },
        cache: false,
        success: function (data) {
            if (data == "") {
                return;
            } else {
                $('#riga-abbonamento').hide();
                $('#testo-riepilogo').html(' ');
                $('#testo-riepilogo').html("<p>" + data + "</p><label class='css-input css-checkbox css-checkbox-rounded css-checkbox-sm css-checkbox-info'><input class='seltutti' type='checkbox' id='checkregistra' onclick='selButtonRegistra()'><span></span>Ho preso visione</label>");
                $('#riga-abbonamento').attr('hidden', true);
                $('#dbtn').attr('hidden', true);
                $('#riepilogo').removeAttr('hidden');
                return;
            }
        }
    });
    
    
}

function ErroriAbbonamenti()
{
    par = false;
    $('.disabilita[required="required"]').each(function () {
        CtrInput($(this));
    })

    return par;
}

function IndietroAbbonamenti() {

    $.ajax({
        url: "/Abbonamenti/SetPolicy",
        type: "POST",
        data: null,
        //contentType: "application/json; charset=utf-8",
        //async: false,
        success: function (data) {
            $('#testo-riepilogo').html(' ');
            $('#riepilogo').attr('hidden', true);
            $('#dbtn').removeAttr('hidden');
            $('#riga-abbonamento').show();
        }
    });
}

function RiepilogoOmaggio() {
    if (ErroriOmaggio() == true) {
        return;
    }
    var riepilogo = "<p>Il/la sottoscritto/a " + $('#Utente_Nome').val() + " " + $('#Utente_Cognome').val() + " dichiara di avere ricevuto in data " + $('#data_ricezione').val() +
        " il seguente omaggio:</p><p>" + "Tipologia: " + $("#Tipo_id option:selected").text();
    if ($("#Tipo_id option:selected").text() != "Denaro") {
        riepilogo += " descrizione: " + $('#Descrizione').val();
    }
    riepilogo += "</p><p>Valore: " + $('#Valore').val() + " in occasione di: " + $("#Motivo_id option:selected").text() +
        "</p><p>da parte di: " + $('#Mittente').val();
    var extra = "";
    if ($("#Tipo_id option:selected").text() == "Denaro") {
        extra += "<p>Ente beneficiario: " + $('#Ente_Beneficiario').val() + "</p>";
    }
    if ($('#Flag_UfficioSpedizioni').val() == "true") {
        extra += "<p>Omaggio pervenuto tramite ufficio spedizioni: " + $('#UfficioSpedizioni').val() + "</p>";
    }
    $('#riga-omaggio').hide();
    $('#testo-riepilogo').html(' ');
    $('#testo-riepilogo').html(riepilogo + " " + extra);
    $('#riga-omaggio').attr('hidden', true);
    $('#dbtn').attr('hidden', true);
    $('#riepilogo').removeAttr('hidden');
    return;
}

function IndietroOmaggio() {
    $('#testo-riepilogo').html(' ');
    $('#riepilogo').attr('hidden', true);
    $('#dbtn').removeAttr('hidden');
    $('#riga-omaggio').show();
}

function GestisciOmaggio() {
    event.preventDefault();
    var omaggio = $("#form-gestioneomaggio").serialize();
    $.ajax({
        url: "/Omaggi/GestisciOmaggio",
        type: "POST",
        data: omaggio,
        //contentType: "application/json; charset=utf-8",
        //async: false,
        success: function (data) {
            switch (data) {
                case "ok":
                    window.location.href = "/Omaggi/Index/";
                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal("Errore:\n" + bo);
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
        }
    });
}

function GestisciAbbonamenti()
{
    
    if (ErroriAbbonamenti() == true) {
        return false;
    }
    event.preventDefault();
    var omaggio = $("#form-gestioneabbonamento").serialize();
    $("#btnInserisciAbbonamento").attr('disabled', 'disabled');
    $.ajax({
        url: "/Abbonamenti/GestisciAbbonamenti",
        type: "POST",
        data: omaggio,
        //contentType: "application/json; charset=utf-8",swal
        //async: false,
        success: function (data) {
            switch (data) {
                case "ok":
                    
                    swal({
                        title: "Inserimento effettuato",
                        text: "La richiesta di abbonamento è stata effettuata",
                        type: "success"
                    }).then(function () {
                        location.reload();
                    });

                    
                    break;
                case "okmodifica":

                    swal({
                        title: "Modifica effettuata",
                        text: "La richiesta di modifica abbonamento è stata effettuata",
                        type: "success"
                    }).then(function () {
                        location.reload();
                    });


                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal("Errore:\n" + bo);
                    $("#btnInserisciAbbonamento").removeAttr('disabled');;
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
        }
    });
}

function GestisciCampagnaAbbonamenti()
{
    if (ErroriCampagna() == true) {
        return false;
    }
    event.preventDefault();
    var campagnaAbb = $("#form-gestionecampagnaabbonamento").serialize();
    var array = "";
    $('input:checked').each(function () {
        array=  array + ";" + ($(this)[0].name);
    });
    
    var a = [1, 2];
    
    $.ajax({
        url: "/CampagnaAbbonamenti/GestisciCampagnaAbbonamenti",
        type: "POST",
        data: { DataInizio: $('[name*=DataInizioCampagna]').val(), DataFine: $('[name*=DataFineCampagna]').val(), IdCampagna: $("#IdCampagna").val(), ArrayIdVettore: array, DataInizioValidita: $("#data_inizioValidita").val() },
        //contentType: "application/json; charset=utf-8",
        //async: false,
        success: function (data) {
            switch (data) {
                case "ok":
                    window.location.href = "/CampagnaAbbonamenti/Index/";
                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal("Errore:\n" + bo);
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
        }
    });
}

function ErroriOmaggio() {
    par = false;
    $('.disabilita[required="required"]').each(function () {
        CtrInput($(this));
    })
    return par;
}

function ErroriCampagna()
{
    par = false;
    $('.disabilita[required="required"]').each(function () {
        CtrInput($(this));
    })
    return par;
}

function CtrInput( event )
{
    var name = event.attr('name');
    if (name == "Descrizione" && $("#Tipo_id option:selected").text() == "Denaro") {
        $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
        return;
    }
    if (name == "UfficioSpedizioni" && $("input[name='Flag_UfficioSpedizioni']:checked").val() == "False") {
        $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
        return;
    }
    if (name == "Ente_Beneficiario" && $("#Tipo_id option:selected").text() != "Denaro") {
        $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
        return;
    }
    if (event.val() == "" || event.val() == null) {
        $(document).find("span[data-valmsg-for='" + name + "']").removeClass();
        $(document).find("span[data-valmsg-for='" + name + "']").addClass("field-validation-error");
        par = true;
    }
    else {
        $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
    }
    if (name == "Tipo_id") GestisciTipoDenaro();
    if (name == "Motivo_id") GestisciAltroOccasioneDiRicezione();
    if (name == "Utente_Indirizzo_Mail" && $("#UtenteEsterno").val() == true) {
        if (!validateEmail($("#UtenteEsterno").val())) {
            $(document).find("span[data-valmsg-for='" + name + "']").removeClass();
            $(document).find("span[data-valmsg-for='" + name + "']").addClass("field-validation-error");
            par = true;
        }
        else {
            $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
        }

    }
    if (name == "Mail_Responsabile") {
        if (!validateEmail($("#Mail_Responsabile").val())) {
            $(document).find("span[data-valmsg-for='" + name + "']").removeClass();
            $(document).find("span[data-valmsg-for='" + name + "']").addClass("field-validation-error");
            par = true;
        }
        else {
            $(document).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
        }
    }
    if (name == "CittaAbbonamento") {
        if ($('#CittaAbbonamento').val() == 2) {
            $('#VettoreAbbonamento').removeClass('disable');
        }
        else {
            $('#VettoreAbbonamento').val("");
            $('#VettoreAbbonamento').addClass('disable');
        }

    }

    return false;
}

function onlyNumeric( evt, campo )
{
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (campo.toLowerCase().includes("valore")) {
        if (evt.key == "," || evt.key == ".") {
            var valore = $("#" + campo).val();
            if (valore.includes(","))
                return false;
            var pos = evt.target.selectionEnd;
            if (pos === 0)
                return false;
            var pos = evt.target.selectionEnd;
            evt.preventDefault();
            var compose;
            compose = valore.substr(0, pos) + ',' + valore.substr(pos, (valore.length + 1));
            $("#" + campo).val(compose);
            return true;
        }
    }
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}

function validateEmail( email )
{
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function ctrValoreRange(evento, campo) {
    if (parseFloat($('#Valorea').val()) < parseFloat($('#Valoreda').val())) {
        swal('Errore', 'Range di valori non valido', 'error');

    }
}

function setMatricola() {
    if ($("#utentiOmaggio option:selected").val() != 0) {
        $('#Matricola').val($("#utentiOmaggio option:selected").val());
    }
    else {
        $('#Matricola').val("");
    }
}

    //function fnExcelReport() {
    //    var tab_text = "<table border='2px'><tr bgcolor='#87AFC6'>";
    //    var textRange; var j = 0;
    //    tab = document.getElementById('tableOmaggi'); // id of table

    //    for (j = 0 ; j < tab.rows.length-1 ; j++) {
        
    //        tab_text = tab_text + tab.rows[j].innerHTML + "</tr>";
    //        //tab_text=tab_text+"</tr>";
    //    }

    //    tab_text = tab_text + "</table>";
    //    // tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, "");//remove if u want links in your table
    //    // tab_text = tab_text.replace(/<img[^>]*>/gi, ""); // remove if u want images in your table
    //    // tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, ""); // reomves input params

    //    var ua = window.navigator.userAgent;
    //    var msie = ua.indexOf("MSIE ");

    //    if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./))      // If Internet Explorer
    //    {
    //        txtArea1.document.open("html/xls", "replace");
    //        txtArea1.document.write(tab_text);
    //        txtArea1.document.close();
    //        txtArea1.focus();
    //        sa = txtArea1.document.execCommand("SaveAs", true, "");
    //    }
    //    else                 //other browser not tested on IE 11
    //        sa = window.open('data:application/vnd.ms-excel,' + encodeURIComponent(tab_text));

    //    return (sa);
//}

$( ".cestini.wizard-next" ).on( 'click', function ()
{
    var myWizard = $( '#w3' ).bootstrapWizard();
    myWizard.bootstrapWizard( 'next' );
} );

function ShowGestisciCestino() {
    $.ajax({
        url: "/Cestini/ShowGestisciCestino",
        type: "POST",
        data: {},
        async: false,
        success: function (data) {
            $('#modal-cestino').html(' ');
            $('#modal-cestino').html(data);
            UIRai.initHelpers('datetimepicker');

            $(".js-data-example-ajax").select2({
                placeholder: "il cognome comincia per",
                minimumInputLength: 3,
                width: '100%',
                ajax: {
                    url: '/Cestini/GetCognomiRisorsa',
                    type: "POST",
                    dataType: 'json',
                    data: function (params) {
                        return {
                            cog: params.term
                        };
                    },
                    processResults: function (data) {
                        return {
                            results: $.map(data, function (obj) {
                                var a = new Array();
                                for (var item in obj) {
                                    var txt = obj[item].text;
                                    var array = txt.split('/');
                                    var nome = array[1];
                                    var cognome = array[0];
                                    var direzione = array[2];
                                    var matricola = obj[item].id;
                                    var fullname = matricola + ' - ' + cognome + ' ' + nome + ' (' + direzione + ')';
                                    a.push({ id: obj[item].id, text: fullname, nome: nome, cognome: cognome });
                                }
                                return a;
                            })
                        };
                    }
                }
            });

            /*
        Wizard #3
        */
            var $w3finish = $('#w3').find('.finish'),
                $w3validator = $("#w3 form").validate({
                    highlight: function (element) {
                        $(element).closest('.form-group').removeClass('has-success').addClass('has-error');
                    },
                    success: function (element) {
                        $(element).closest('.form-group').removeClass('has-error');
                        $(element).remove();
                    },
                    errorPlacement: function (error, element) {
                        element.parent().append(error);
                    }
                });

            $('#w3').bootstrapWizard({
                tabClass: 'wizard-steps',
                nextSelector: '.cestini.wizard-next',
                previousSelector: '.cestini.previous',
                firstSelector: null,
                lastSelector: null,
                onNext: function ( tab, navigation, index, newindex )
                {
                    if ( index != 1 )
                    {
                        if ( index == 2 )
                        {
                            var validator = $( '#form-gestioneordine' ).validate( {
                                errorPlacement: function ( error, element )
                                {
                                    if ( element.hasClass( 'val-custom' ) )
                                    {
                                        error.insertAfter( element.parent().find( '.val-message' ) );
                                    }
                                    else
                                    {
                                        error.insertAfter( element );
                                    }
                                }
                            } );

                            if ( !$( '#form-gestioneordine' ).valid() )
                            {
                                validator.focusInvalid();
                                return false;
                            }

                            var selectedDate = $( "#data_ora_pasto" ).val();
                                                                
                            var dd = selectedDate.substr( 0, 2 );
                            var mm = selectedDate.substr( 3, 2 );
                            var yyyy = selectedDate.substr( 6, 4 );

                            if ( dd < 10 && dd.length == 1 )
                            {
                                dd = '0' + dd
                            }

                            if ( mm < 10 && mm.length == 1 )
                            {
                                mm = '0' + mm
                            }

                            var _selectedDate = dd + '/' + mm + '/' + yyyy;

                            var tipoPasto = $( '#ordine_tipoPasto' ).val();
                            var idOrdine = $( '#ordine_idOrdine' ).val();
                            var isEmergency = false;

                            var currentDate = new Date();

                            var dd = currentDate.getDate();
                            var mm = currentDate.getMonth() + 1; //January is 0!
                            var yyyy = currentDate.getFullYear();

                            if ( dd < 10 )
                            {
                                dd = '0' + dd
                            }

                            if ( mm < 10 )
                            {
                                mm = '0' + mm
                            }

                            var _current = dd + '/' + mm + '/' + yyyy;

                            var isToday = ( _selectedDate == _current );

                            var startDate = new Date( currentDate.getTime() );
                            var endDate = new Date( currentDate.getTime() );

                            var startTime = null;
                            var endTime = null;

                            if ( tipoPasto == "pranzo" && isToday )
                            {
                                // Se il tipo pasto è pranzo e l'orario corrente è compreso tra le 9:31 e le 12:30 allora è un pasto in emergenza
                                startTime = '09:31:00';
                                endTime = '12:30:00';
                            }
                            else if ( tipoPasto == "cena" && isToday )
                            {
                                startTime = '16:31:00';
                                endTime = '19:30:00';
                            }

                            if ( isToday )
                            {
                                startDate.setHours( startTime.split( ":" )[0] );
                                startDate.setMinutes( startTime.split( ":" )[1] );
                                startDate.setSeconds( startTime.split( ":" )[2] );
                                endDate.setHours( endTime.split( ":" )[0] );
                                endDate.setMinutes( endTime.split( ":" )[1] );
                                endDate.setSeconds( endTime.split( ":" )[2] );

                                isEmergency = ( idOrdine <= 0 ) && ( startDate < currentDate && endDate > currentDate );

                                if ( isEmergency )
                                {
                                    swal( "Richiesta in emergenza ", "Non è possibile assicurare la fornitura in quanto l'orario previsto per l'inserimento delle richieste (per il pasto desiderato) è scaduto. Ulteriore riscontro, positivo o negativo, sarà inviato direttamente dal fornitore una volta verificata la possibilità di predisporre comunque il cestino in tempo utile.", "info" );
                                }
                            }
                        }
                        else if ( index == 3 )
                        {
                            // verifica se ci sono richieste di cestini
                            var countRequests = $( 'tr[class="tr-ric editMode"]' ).length;
                            var validator = $( "#form-AddRequest" ).validate( {
                                errorPlacement: function ( error, element )
                                {
                                    if ( element.hasClass( 'val-custom' ) )
                                    {
                                        error.insertAfter( element.parent().find( '.val-message' ) );
                                    }
                                    else 
                                    {
                                        error.insertAfter( element );
                                    }
                                }
                            } );

                            if ( !$( '#form-AddRequest' ).valid() && countRequests < 1 )
                            {
                                validator.focusInvalid();
                                return false;
                            }
                            else if ( countRequests < 1 )
                            {
                                validator.focusInvalid();
                                return false;
                            }
                        }
                    }
                    if ( typeof newindex === "undefined" )
                    {
                        if ( index == 1 ) // Ordini
                        {
                        }
                        if ( index == 2 ) // Richieste
                        {
                            loadTabRichieste();
                            getRiepilogoCestino();
                        }
                        else if ( index == 3 )
                        { // Riepilogo
                            getRiepilogo();
                        }
                    }
                    else
                    {
                        if ( newindex == 1 ) // Ordini
                        {
                            var validator = $( '#form-gestioneordine' ).validate( {
                                errorPlacement: function ( error, element )
                                {
                                    if ( element.hasClass( 'val-custom' ) )
                                    {
                                        error.insertAfter( element.parent().find( '.val-message' ) );
                                    }
                                    else
                                    {
                                        error.insertAfter( element );
                                    }
                                }
                            } );

                            if ( !$( '#form-gestioneordine' ).valid() )
                            {
                                validator.focusInvalid();
                                return false;
                            }
                        }
                        if ( newindex == 2 ) // Richieste
                        {
                            // verifica se ci sono richieste di cestini
                            var countRequests = $( 'tr[class="tr-ric editMode"]' ).length;
                            var validator = $( '#form-AddRequest' ).validate( {
                                errorPlacement: function ( error, element )
                                {
                                    if ( element.hasClass( 'val-custom' ) )
                                    {
                                        error.insertAfter( element.parent().find( '.val-message' ) );
                                    }
                                    else
                                    {
                                        error.insertAfter( element );
                                    }
                                }
                            } );

                            if ( !$( '#form-AddRequest' ).valid() && countRequests < 1 )
                            {
                                validator.focusInvalid();
                                return false;
                            }
                            else if ( countRequests < 1 )
                            {
                                validator.focusInvalid();
                                return false;
                            }

                            loadTabRichieste();
                            getRiepilogoCestino();
                        }
                        else if ( newindex == 3 ) // Riepilogo
                        {
                            getRiepilogo();
                        }
                    }
                },
                onTabClick: function ( tab, navigation, index, newindex )
                {
                    var idOrdine = $( '#ordine_idOrdine' ).val();

                    if ( newindex == 1 && index == 0 )
                    {
                        return false;
                    }
                    else if ( newindex == index + 1 )
                    {
                        var isEmpty = $( '#RichiesteContainer' ).is( ':empty' );

                        if ( isEmpty )
                        {
                            loadTabRichieste();
                            getRiepilogoCestino();
                        }
                        return this.onNext(tab, navigation, index, newindex);
                    } else if (newindex > index + 1) {
                        return false;
                    } else {
                        return true;
                    }
                },
                onTabChange: function ( tab, navigation, index, newindex )
                {
                    var $total = navigation.find('li').length - 1;
                    $w3finish[index != $total ? 'addClass' : 'removeClass']( 'hidden' );
                    $('#w3').find(this.nextSelector)[newindex == $total ? 'addClass' : 'removeClass']('hidden');
                    if ( newindex == 0 )
                    {
                        $( ".cestini.richiesta" ).addClass( "hidden" );
                        $( ".cestini.previous" ).addClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                        $( ".cestini.finish" ).addClass( "hidden" );
                    }
                    if ( newindex == 1 )
                    {
                        $( ".cestini.richiesta" ).addClass( "hidden" );
                        $( ".cestini.previous" ).addClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                        $( ".cestini.finish" ).addClass( "hidden" );
                    }
                    if (newindex == 2) {
                        $( ".cestini.richiesta" ).removeClass( "hidden" );
                        $( ".cestini.previous" ).removeClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                        $( ".cestini.finish" ).addClass( "hidden" );
                    }
                    if (newindex == 3) {
                        $( ".cestini.richiesta" ).addClass( "hidden" );
                        $( ".cestini.previous" ).removeClass( "hidden" );
                        $( ".cestini.finish" ).removeClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                    }
                },
                onTabShow: function ( tab, navigation, index )
                {
                    var $total = navigation.find('li').length - 1;
                    var $current = index;
                    var $percent = Math.floor(($current / $total) * 100);
                    $('#w3').find('.progress-indicator').css({ 'width': $percent + '%' });
                    tab.prevAll().addClass('completed');
                    tab.nextAll().removeClass( 'completed' );
                    if ( index == 0 )
                    {
                        $( ".cestini.next" ).addClass( "hidden" );
                    }
                },
                onPrevious: function ( tab, navigation, index, newindex )
                {
                    $( 'label.error' ).each( function ()
                    {
                        $( this ).remove();
                    } );
                }
            });

            function getRiepilogoCestino() {
                var obj = $("#form-gestioneordine").serialize();
                $.ajax({
                    url: "/Cestini/RiepilogoCestino",
                    type: "POST",
                    data: obj,
                    async: false,
                    success: function (data) {
                        $('#divriepilogo').html(' ');
                        $('#divriepilogo').html(data);
                        $("#divriepilogo").removeAttr("hidden");
                        $( ".cestini.richiesta" ).removeClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                        $( ".cestini.annulla" ).addClass( "hidden" );
                    }
                });
            }

            function getRiepilogo() {
                $.ajax({
                    url: "/Cestini/Riepilogo",
                    type: "POST",
                    async: false,
                    success: function (data) {
                        $('#divRiepilogoFinale').html(' ');
                        $('#divRiepilogoFinale').html(data);
                        $("#divRiepilogoFinale").removeAttr("hidden");
                        $( ".cestini.richiesta" ).removeClass( "hidden" );
                        $( ".cestini.modifica" ).addClass( "hidden" );
                        $( ".cestini.annulla" ).addClass( "hidden" );
                    }
                });
            }
        }
    })
}

function loadTabRichieste( )
{
    $.ajax( {
        url: "/Cestini/loadTabRichieste",
        async: false,
        type: "GET",
        cache: false,
        data: {
        },
        contentType: "application/json; charset=utf-8",
        error: function ( jqXHR, textStatus, errorThrown )
        {
            alert( jqXHR + "-" + textStatus + "-" + errorThrown );
        },
        success: function ( data, textStatus, jqXHR )
        {
            $( '#RichiesteContainer' ).html( data );
        }
    } );
}

function ShowNextButton(selector, value)
{
    $( '#forMe' ).removeClass( 'tiposel' );
    $( '#forInternal' ).removeClass( 'tiposel' );
    $( '#forOthers' ).removeClass( 'tiposel' );
    $( ".cestini.wizard-next" ).removeClass( "disable" );
    $( '#' + selector ).addClass( 'tiposel' );
    $( '#ordine_DestinatarioCestino' ).val( value );

    $.ajax( {
        url: "/Cestini/loadOrdineTab",
        async: false,
        cache: false,
        type: "GET",
        data: {
            tipoDestinatario: value
        },
        contentType: "application/json; charset=utf-8",
        error: function ( jqXHR, textStatus, errorThrown )
        {
            alert( jqXHR + "-" + textStatus + "-" + errorThrown );
        },
        success: function ( data, textStatus, jqXHR )
        {
            $( '#OrdineContainer' ).html( data );
        }
    } );
}

function GetAnag(idelem) {
    var matricola = $("#" + idelem + " :selected").val();
    $('#richiestaCorrente_matricolaRisorsa').val(matricola);
    $('#richiestaCorrente_matricolaRisorsa').text(matricola);
    var cog_nome = $("#" + idelem + " :selected").text().split('/');
    $('#richiestaCorrente_cognomeRisorsa').val(cog_nome[0]);
    $('#richiestaCorrente_nomeRisorsa').val(cog_nome[1]);
}

function RadioRisorsaEsterna(che) {
    $("#divmatricola").attr("hidden", !che);
    $("#divnome").attr("hidden", che);
    $("#divmotesterno").attr("hidden", che);
    if (che == false) {
        $("#richiestaCorrente_matricolaRisorsa").val(null);
        $("#divcogs").removeAttr("hidden");
        $("#divcogs").attr("hidden", true);
        $("#divcogi").removeAttr("hidden");
        $("#divcogi").attr("hidden", false);
        $("#divnome").removeAttr("hidden");
        $("#divnome").attr("hidden", false);
        $('#labcog').text('Cognome risorsa');
        $('#richiestaCorrente_cognomeRisorsa').val(null);
        $('#richiestaCorrente_nomeRisorsa').val(null);
    }
    else {
        $("#richiestaCorrente_motivoEsterno").val(null);
        $("#divcogs").removeAttr("hidden");
        $("#divcogs").attr("hidden", false);
        $("#divcogi").removeAttr("hidden");
        $("#divcogi").attr("hidden", true);
        $("#divnome").removeAttr("hidden");
        $("#divnome").attr("hidden", true);
        $('#labcog').text('Nominativo risorsa');
    }
}

function selectProgram()
{
    $.ajax({
        url: "/eventi/SelectProgramma?id=" + document.getElementById("programma").value,
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {
            if (data.length > 0) {
                $("#titoloins").val(data[0].titolo);
                $("#luogoins").val(data[0].luogo);
                $("#numeroTotale").val(data[0].numerototale);
                $("#numeroMassimo").val(data[0].numeromassimo);
                $("#sedi").val(data[0].matricole_abilitate);
                $("#matricole").val(data[0].sedi_abilitate);
                $("#noteEmail").val(data[0].nota_email);
            } else {
                $("#titoloins").val("");
                $("#luogoins").val("");
                $("#numeroTotale").val("");
                $("#numeroMassimo").val("");
                $("#sedi").val("");
                $("#matricole").val("");
                $("#noteEmail").val("");
            }
        }
    });
}

function selectProgramUpd() {
    $.ajax({
        url: "/eventi/SelectProgramma?id=" + document.getElementById("programmaupd").value,
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {
            if (data.length > 0) {
                $("#titoloupd").val(data[0].titolo);
                $("#luogoupd").val(data[0].luogo);
                $("#numeroTotaleupd").val(data[0].numerototale);
                $("#numeroMassimoupd").val(data[0].numeromassimo);
                $("#sediupd").val(data[0].matricole_abilitate);
                $("#matricoleupd").val(data[0].sedi_abilitate);
                $("#noteEmail").val(data[0].nota_email);
            } else {
                $("#titoloupd").val("");
                $("#luogoupd").val("");
                $("#numeroTotaleupd").val("");
                $("#numeroMassimoupd").val("");
                $("#sediupd").val("");
                $("#matricoleupd").val("");
                $("#noteEmail").val("");
            }
        }
    });
}

function getProduzioneByTitolo() {
    var term = $("#term").val();
    $('#RisultatiRicercaContainer').html('<button class="btn btn-default btn-lg" style="border:none;"><i class="fa fa-spinner fa-spin"></i> Ricerca in corso</button>');
    $.ajax({
        url: '/Cestini/GetProduzioneByTitolo',
        type: "POST",
        data: {
            term: term
        },
        success: function (data) {
            $('#RisultatiRicercaContainer').html('');
            $('#RisultatiRicercaContainer').html(data);
        },
        error: function (jqXHR, exception) {
            $('#RisultatiRicercaContainer').html(' ');
            swal('Oops...',
                "Impossibile continuare con l'operazione desiderata!",
                'error'
            );
            return false;
        }
    });
}

function selezionaProduzione(idx) {
    var matricola = $('#td_produzione_matricola_' + idx).data('matricola');
    var titolo = $('#td_produzione_titolo_' + idx).data('titolo');
    var uorg = $('#td_produzione_uorg_' + idx).data('uorg');

    $('#ordine_centroCosto').val(uorg);
    $('#ordine_matricolaSpettacolo').val(matricola);
    $('#ordine_titoloProduzione').val(titolo);
    $("#ordine_matricolaSpettacolo-error").hide();
    $("#ordine_centroCosto-error").hide();
    $("#ordine_titoloProduzione-error").hide();
    $('#ordine_centroCosto').focusout();
    $('#ordine_matricolaSpettacolo').focusout();
    $('#ordine_titoloProduzione').focusout();
    $('#RisultatiRicercaContainer').html('');
    $('#term').val('');
    $('#modal-ricercaSpettacolo').modal('toggle');
}

function ConfermaCancellazione_Ordine(idOrdine) {
    swal({
        title: "Sicuro di voler cancellare l'ordine selezionato?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText: ' Si',
        cancelButtonText: ' No'
    }).then(function () {
        $('tr[id="trOrdine_' + idOrdine + '"]').addClass('danger');
        $.ajax({
            url: '/Cestini/RimuoviOrdine',
            type: "POST",
            data: {
                idOrdine: idOrdine
            },
            dataType: 'json',
            success: function (data) {
                $('tr[id="trOrdine_' + idOrdine + '"]').animate({
                }, 2500, function () {
                    $('tr[id="trOrdine_' + idOrdine + '"]').slideUp("slow");
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                swal('Oops...',
                        'Si è verificato un errore!',
                        'error'
                    );
            }
        });
    });
}

function DettaglioOrdine(idOrdine) {
    $.ajax({
        url: '/Cestini/DettaglioOrdine',
        type: "POST",
        data: {
            idOrdine: idOrdine
        },
        async: false,
        success: function (data) {
            InitDettaglio( data );
            $( ".cestini.wizard-next" ).removeClass( 'disable' );
        }
    })
}

function DettaglioOrdineReadOnly(idOrdine) {
    $.ajax({
        url: '/Cestini/DettaglioOrdineReadOnly',
        type: "POST",
        data: {
            idOrdine: idOrdine
        },
        async: false,
        success: function (data) {
            $('#modal-cestino').html(' ');
            $('#modal-cestino').html(data);
            UIRai.initHelpers( 'datetimepicker' );
            $( ".cestini.wizard-next" ).removeClass( 'disable' );
        }
    })
}

function editRequest()
{
    var validator = $( '#form-AddRequest' ).validate( {
        errorPlacement: function ( error, element )
        {
            if ( element.hasClass( 'val-custom' ) )
            {
                error.insertAfter( element.parent().find( '.val-message' ) );
            }
            else
            {
                error.insertAfter( element );
            }
        }
    } );

    if ( !$( '#form-AddRequest' ).valid() )
    {
        validator.focusInvalid();
        return false;
    }

    var obj = $("#form-AddRequest").serialize();
    $.ajax({
        url: '/Cestini/CreaModificaRichiesta',
        type: "POST",
        data: obj,
        async: false,
        dataType: 'json',
        success: function ( result )
        {
            var data = result.richiestaCorrente;
            if ( !result.success )
            {
                var errorMessage = 'Si è verificato un errore!';
                if ( $.trim( result.errorMessage ).length > 0 )
                {
                    errorMessage = result.errorMessage;
                }

                swal( 'Oops...',
                    errorMessage,
                    'error'
                );
                return false;
            }
                
            if (data.matricolaRisorsa == null || data.matricolaRisorsa == 'null')
                data.matricolaRisorsa = '';

            var nominativo = data.cognomeRisorsa + ' ' + data.nomeRisorsa;
            if (!$.trim(data.matricolaRisorsa).length == 0) {
                nominativo = nominativo + ' (' + data.matricolaRisorsa + ')';
            }

            if (data.codiceRichiesta == null || data.codiceRichiesta == 'null')
                data.codiceRichiesta = '';

            var row = $('<tr class="tr-ric editMode" id="' + data.idRichiesta + '">' +
                            '<td hidden><label id="idRichiesta" title="' + data.idRichiesta + '"></label>' + data.idRichiesta + '</td>' +
                            '<td hidden><label id="flagRisorsa" title="' + data.flagRisorsa + '">' + data.flagRisorsa + '</label></td>' +
                            '<td hidden><label id="motivoEsterno" title="' + data.motivoEsterno + '">' + data.motivoEsterno + '</label></td>' +
                            '<td hidden><label id="progressivo" title="' + data.progressivo + '">' + data.progressivo + '</label></td>' +
                            '<td hidden><label id="cognomeRisorsa" title="' + data.cognomeRisorsa + '">' + data.cognomeRisorsa + '</label></td>' +
                            '<td hidden><label id="nomeRisorsa" title="' + data.nomeRisorsa + '">' + data.nomeRisorsa + '</label></td>' +
                            '<td hidden><label id="matricolaRisorsa" title="' + data.matricolaRisorsa + '">' + data.matricolaRisorsa + '</label></td>' +
                            '<td><label id="nominativo" title="' + nominativo + '">' + nominativo + '</label></td>' +
                            '<td><label id="tipoCestino" data-toggle="tooltip" title="' + getDescrizioneTipoCestino(data.tipoCestino) + '">' + getCodiceTipoCestino(data.tipoCestino) + '</label></td>' +
                            '<td hidden><label id="codiceRichiesta" title="' + data.codiceRichiesta + '">' + data.codiceRichiesta + '</label></td>' +
                            '<td><a class="icon-trash h4" href="#" onclick="ConfermaCancellazione_Richiesta(' + data.idRichiesta + ')" /></td>' +
                            '<td><a class="btn btn-default btn-scriv text-uppercase bg-puls_dash" onclick="DettaglioRichiesta(' + data.idRichiesta + ');">Modifica</a></td></tr>');

            //if ( !$( '.nessuna-richiesta' ).hasClass( 'hidden' ) )
            //    $( '.nessuna-richiesta' ).addClass( 'hidden' )
            //else if ( $( '.trovata-richiesta' ).hasClass( 'hidden' ) )
            //    $( '.trovata-richiesta' ).removeClass( 'hidden' )

            $( 'tr[class="nessuna-richiesta "]' ).addClass( 'hidden' );
            $( 'tr[class="trovata-richiesta "]' ).removeClass( 'hidden' );

            var exists = $('tr[class="tr-ric editMode"][id="' + data.idRichiesta + '"]');

            if (exists.length) {
                $(exists).replaceWith(row);
                $(row).addClass('success');

                setTimeout(function () {
                    $(row).removeClass('success');
                }, 2500);
            }
            else {
                var nCestini = $('#lbNumeroCestini').data('ncestini');
                nCestini = parseInt(nCestini, 10);
                nCestini++;

                $('#lbNumeroCestini').data('ncestini', nCestini);
                var txt = "";

                if (nCestini == 1) {
                    txt = "1 cestino";
                }
                else {
                    txt = nCestini.toString() + " cestini";
                }

                $('#lbNumeroCestini').text(txt);

                $('#richieste').append(row);
            }

            $( '.cestini.finish' ).removeClass( 'disabled' );

            ripristinaCampi();
        },
        error: function ( jqXHR, textStatus, errorThrown )
        {
        }
    });
}

function salva() {
    // verifica se ci sono richieste di cestini
    var countRequests = $('tr[class="tr-ric editMode"]').length;

    if (countRequests < 1) {
        swal('Oops...',
            "Impossibile continuare con l'operazione desiderata!\nNessun cestino associato alla richiesta.",
            'error'
        );
        return false;
    }

    $.ajax({
        url: '/Cestini/SalvaOrdine',
        type: "POST",
        async: false,
        dataType: 'json',
        success: function ( data )
        {
            if ( !data.success )
            {
                var errorMessage = 'Si è verificato un errore!';
                if ( $.trim( data.errorMessage ).length > 0 )
                {
                    errorMessage = data.errorMessage;
                }

                swal( 'Oops...',
                    errorMessage,
                    'error'
                );
                return false;
            }
            swal(
                'Dati salvati con successo!',
                '',
                'success'
            );
            annulla();
            $('#modal-cestino').modal('toggle');
            LoadElencoOrdini();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            swal('Oops...',
                    'Si è verificato un errore!',
                    'error'
                );
        }
    });
}

function annulla()
{
    var tipo = $( '#tipo_DestinatarioCestino' ).val();

    if ( tipo == 1 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
        $( '#SelectUser' ).select2( "val", "" );
    }
    else if ( tipo == 2 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).val( null );
        $( '#richiestaCorrente_nomeRisorsa' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).text( null );
        $( '#richiestaCorrente_nomeRisorsa' ).text( null )
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).val( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
        $( '#SelectUser' ).select2( "val", "" );
    }
    else if ( tipo == 3 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).val( null );
        $( '#richiestaCorrente_nomeRisorsa' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).text( null );
        $( '#richiestaCorrente_nomeRisorsa' ).text( null )
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).val( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
    }
    $( ".cestini.richiesta" ).removeClass( "hidden" );
    $( ".cestini.modifica" ).addClass( "hidden" );
    $( ".cestini.annulla" ).addClass( "hidden" );
}

function LoadElencoOrdini() {
    window.location.href = "/Cestini/Index";
}

function ConfermaCancellazione_Richiesta(idRichiesta) {
    swal({
        title: "Sicuro di voler cancellare la richiesta selezionata?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText: ' Si',
        cancelButtonText: ' No'
    }).then(function () {
        $('tr[class="tr-ric editMode"][id="' + idRichiesta + '"]').addClass('danger');
        $.ajax({
            url: '/Cestini/RimuoviRichiesta',
            type: "POST",
            data: {
                idRichiesta: idRichiesta
            },
            async: false,
            dataType: 'json',
            success: function (data) {
                $('tr[class="tr-ric editMode danger"][id="' + idRichiesta + '"]').animate({
                }, 2500, function () {
                    $('tr[class="tr-ric editMode danger"][id="' + idRichiesta + '"]').slideUp("slow");
                    var nCestini = $('#lbNumeroCestini').data('ncestini');
                    nCestini = parseInt(nCestini, 10);
                    nCestini--;

                    $('#lbNumeroCestini').data('ncestini', nCestini);
                    var txt = "";

                    if (nCestini == 1) {
                        txt = "1 cestino";
                    }
                    else {
                        txt = nCestini.toString() + " cestini";
                    }

                    $('#lbNumeroCestini').text(txt);
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                swal('Oops...',
                        'Si è verificato un errore!',
                        'error'
                    );
            }
        });
    });
}

function DettaglioRichiesta(idRichiesta) {
    $.ajax({
        url: '/Cestini/DettaglioRichiesta',
        type: "POST",
        data: {
            idRichiesta: idRichiesta
        },
        async: false,
        dataType: 'json',
        success: function ( data )
        {
            var tipo = $( '#tipo_DestinatarioCestino' ).val();

            if ( tipo == 1 )
            {
            }
            else if ( tipo == 2 )
            {
            }
            else if ( tipo == 3 )
            {
                $( '#richiestaCorrente_flagRisorsa[value="True"]' ).prop( 'checked', false );
                $( '#richiestaCorrente_flagRisorsa[value="False"]' ).prop( 'checked', false );

                RadioRisorsaEsterna( data.flagRisorsa );
                if ( data.flagRisorsa )
                    $( '#richiestaCorrente_flagRisorsa[value="True"]' ).prop( 'checked', true );
                else
                    $( '#richiestaCorrente_flagRisorsa[value="False"]' ).prop( 'checked', true );

                $( '#SelectUser' ).select2( 'val', '' );
                $( '#SelectUser' ).select2( 'data', null );
            }

            var date = new Date(parseInt(data.dataInserimento.substr(6)));
            $('#richiestaCorrente_idRichiesta').val(data.idRichiesta);
            $('#richiestaCorrente_idOrdine').val(data.idOrdine);
            $('#richiestaCorrente_codiceRichiesta').val(data.codiceRichiesta);
            $('#richiestaCorrente_progressivo').val(data.progressivo);
            $('#richiestaCorrente_dataInserimento').val(date);

            $('#richiestaCorrente_cognomeRisorsa').val(data.cognomeRisorsa);
            $('#richiestaCorrente_nomeRisorsa').val(data.nomeRisorsa);
            $('#richiestaCorrente_matricolaRisorsa').val(data.matricolaRisorsa);
            $('#richiestaCorrente_motivoEsterno').val(data.motivoEsterno);
            $( "#richiestaCorrente_tipoCestino" ).val( data.tipoCestino );

            var toInsert = {
                id: data.matricolaRisorsa,
                text: data.cognomeRisorsa + ' ' + data.nomeRisorsa
            };

            var newOption = new Option( toInsert.text, toInsert.id, true, true );
            $( '#SelectUser' ).append( newOption ).trigger( 'change' );
            $( '#richiestaCorrente_SelectTipoCestino' ).val( data.tipoCestino ).trigger( 'change' );

            $( ".cestini.richiesta" ).addClass( "hidden" );
            $( ".cestini.modifica" ).removeClass( "hidden" );
            $( ".cestini.annulla" ).removeClass( "hidden" );
        },
        error: function (jqXHR, textStatus, errorThrown) {
            swal('Oops...',
                    'Si è verificato un errore!',
                    'error'
                );
        }
    });
}

function DuplicaCestino(idOrdine, conDestinatari)
{
    $.ajax( {
        url: '/Cestini/DuplicaCestino',
        type: "POST",
        data: JSON.stringify({
            idOrdine: idOrdine,
            conDestinatari: conDestinatari
        }),
        contentType: "application/json; charset=utf-8",
        async: false,
        cache: false,
        success: function ( data )
        {
            InitDettaglio( data );
            $( '#w3 li:eq(0) a' ).prop( 'disabled', true );
            $( ".wizard-next" ).removeClass( 'disable' );
        },
        error: function ( jqXHR, textStatus, errorThrown )
        {
            swal( 'Oops...',
                    'Si è verificato un errore!',
                    'error'
                );
        }
    } );
}

function InitDettaglio( data )
{
    $( '#modal-cestino' ).html( ' ' );
    $( '#modal-cestino' ).html( data );
    UIRai.initHelpers( 'datetimepicker' );

    var $w3finish = $( '#w3' ).find( '.finish' ),
        $w3validator = $( "#w3 form" ).validate( {
            highlight: function ( element )
            {
                $( element ).closest( '.form-group' ).removeClass( 'has-success' ).addClass( 'has-error' );
            },
            success: function ( element )
            {
                $( element ).closest( '.form-group' ).removeClass( 'has-error' );
                $( element ).remove();
            },
            errorPlacement: function ( error, element )
            {
                element.parent().append( error );
            }
        } );

    $( '#w3' ).bootstrapWizard( {
        tabClass: 'wizard-steps',
        nextSelector: '.cestini.wizard-next',
        previousSelector: '.cestini.previous',
        firstSelector: null,
        lastSelector: null,
        onNext: function ( tab, navigation, index, newindex )
        {
            if ( index != 1 )
            {
                if ( index == 2 )
                {
                    var validator = $( '#form-gestioneordine' ).validate( {
                        errorPlacement: function ( error, element )
                        {
                            if ( element.hasClass( 'val-custom' ) )
                            {
                                error.insertAfter( element.parent().find( '.val-message' ) );
                            }
                            else
                            {
                                error.insertAfter( element );
                            }
                        }
                    } );

                    if ( !$( '#form-gestioneordine' ).valid() )
                    {
                        validator.focusInvalid();
                        return false;
                    }

                    var selectedDate = $( "#data_ora_pasto" ).val();

                    var dd = selectedDate.substr( 0, 2 );
                    var mm = selectedDate.substr( 3, 2 );
                    var yyyy = selectedDate.substr( 6, 4 );

                    if ( dd < 10 && dd.length == 1 )
                    {
                        dd = '0' + dd
                    }

                    if ( mm < 10 && mm.length == 1 )
                    {
                        mm = '0' + mm
                    }

                    var _selectedDate = dd + '/' + mm + '/' + yyyy;

                    var tipoPasto = $( '#ordine_tipoPasto' ).val();
                    var idOrdine = $( '#ordine_idOrdine' ).val();
                    var isEmergency = false;

                    var currentDate = new Date();

                    var dd = currentDate.getDate();
                    var mm = currentDate.getMonth() + 1; //January is 0!
                    var yyyy = currentDate.getFullYear();

                    if ( dd < 10 )
                    {
                        dd = '0' + dd
                    }

                    if ( mm < 10 )
                    {
                        mm = '0' + mm
                    }

                    var _current = dd + '/' + mm + '/' + yyyy;

                    var isToday = ( _selectedDate == _current );

                    var startDate = new Date( currentDate.getTime() );
                    var endDate = new Date( currentDate.getTime() );

                    var startTime = null;
                    var endTime = null;

                    if ( tipoPasto == "pranzo" && isToday )
                    {
                        // Se il tipo pasto è pranzo e l'orario corrente è compreso tra le 9:31 e le 12:30 allora è un pasto in emergenza
                        startTime = '09:31:00';
                        endTime = '12:30:00';
                    }
                    else if ( tipoPasto == "cena" && isToday )
                    {
                        startTime = '16:31:00';
                        endTime = '19:30:00';
                    }

                    if ( isToday )
                    {
                        startDate.setHours( startTime.split( ":" )[0] );
                        startDate.setMinutes( startTime.split( ":" )[1] );
                        startDate.setSeconds( startTime.split( ":" )[2] );
                        endDate.setHours( endTime.split( ":" )[0] );
                        endDate.setMinutes( endTime.split( ":" )[1] );
                        endDate.setSeconds( endTime.split( ":" )[2] );

                        isEmergency = ( idOrdine <= 0 ) && ( startDate < currentDate && endDate > currentDate );

                        if ( isEmergency )
                        {
                            swal( "Richiesta in emergenza ", "Non è possibile assicurare la fornitura in quanto l'orario previsto per l'inserimento delle richieste (per il pasto desiderato) è scaduto. Ulteriore riscontro, positivo o negativo, sarà inviato direttamente dal fornitore una volta verificata la possibilità di predisporre comunque il cestino in tempo utile.", "info" );
                        }
                    }
                }
                else if ( index == 3 )
                {
                    // verifica se ci sono richieste di cestini
                    var countRequests = $( 'tr[class="tr-ric editMode"]' ).length;
                    var validator = $( "#form-AddRequest" ).validate( {
                        errorPlacement: function ( error, element )
                        {
                            if ( element.hasClass( 'val-custom' ) )
                            {
                                error.insertAfter( element.parent().find( '.val-message' ) );
                            }
                            else
                            {
                                error.insertAfter( element );
                            }
                        }
                    } );

                    if ( !$( '#form-AddRequest' ).valid() && countRequests < 1 )
                    {
                        validator.focusInvalid();
                        return false;
                    }
                    else if ( countRequests < 1 )
                    {
                        validator.focusInvalid();
                        return false;
                    }
                }
            }
            if ( typeof newindex === "undefined" )
            {
                if ( index == 1 ) // Ordini
                {
                }
                if ( index == 2 ) // Richieste
                {
                    loadTabRichieste();
                    getRiepilogoCestino();
                }
                else if ( index == 3 )
                { // Riepilogo
                    getRiepilogo();
                }
            }
            else
            {
                if ( newindex == 1 ) // Ordini
                {
                    var validator = $( '#form-gestioneordine' ).validate( {
                        errorPlacement: function ( error, element )
                        {
                            if ( element.hasClass( 'val-custom' ) )
                            {
                                error.insertAfter( element.parent().find( '.val-message' ) );
                            }
                            else
                            {
                                error.insertAfter( element );
                            }
                        }
                    } );

                    if ( !$( '#form-gestioneordine' ).valid() )
                    {
                        validator.focusInvalid();
                        return false;
                    }
                }
                if ( newindex == 2 ) // Richieste
                {
                    // verifica se ci sono richieste di cestini
                    var countRequests = $( 'tr[class="tr-ric editMode"]' ).length;
                    var validator = $( '#form-AddRequest' ).validate( {
                        errorPlacement: function ( error, element )
                        {
                            if ( element.hasClass( 'val-custom' ) )
                            {
                                error.insertAfter( element.parent().find( '.val-message' ) );
                            }
                            else
                            {
                                error.insertAfter( element );
                            }
                        }
                    } );

                    if ( !$( '#form-AddRequest' ).valid() && countRequests < 1 )
                    {
                        validator.focusInvalid();
                        return false;
                    }
                    else if ( countRequests < 1 )
                    {
                        validator.focusInvalid();
                        return false;
                    }

                    loadTabRichieste();
                    getRiepilogoCestino();
                }
                else if ( newindex == 3 ) // Riepilogo
                {
                    getRiepilogo();
                }
            }
        },
        onTabClick: function ( tab, navigation, index, newindex )
        {
            var idOrdine = $( '#ordine_idOrdine' ).val();
            if ( newindex == 1 && index == 0 )
            {
                return false;
            }
            else if ( newindex == 0 && index >= 1 && idOrdine > 0 ) // se dal tab 1 si cerca di tornare al tab
            {                                                       // di selezione tipologia del cestino (Me, Altri,
                                                                    // Esterni) la richiesta di tornare al tab precedente
                                                                    // deve essere negata
                return false;
            }
            else if ( newindex == index + 1 )
            {
                if ( newindex == 2 )
                {
                    var isEmpty = $( '#RichiesteContainer' ).is( ':empty' );

                    if ( isEmpty )
                    {
                        loadTabRichieste();
                        getRiepilogoCestino();
                    }
                }
                return this.onNext( tab, navigation, index, newindex );
            } else if ( newindex > index + 1 )
            {
                return false;
            } else
            {
                return true;
            }
        },
        onTabChange: function ( tab, navigation, index, newindex )
        {
            var $total = navigation.find( 'li' ).length - 1;
            $w3finish[index != $total ? 'addClass' : 'removeClass']( 'hidden' );
            $( '#w3' ).find( this.nextSelector )[newindex == $total ? 'addClass' : 'removeClass']( 'hidden' );
            if ( newindex == 0 )
            {
                $( ".cestini.richiesta" ).addClass( "hidden" );
                $( ".cestini.previous" ).addClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
                $( ".cestini.finish" ).addClass( "hidden" );
            }
            if ( newindex == 1 )
            {
                $( ".cestini.richiesta" ).addClass( "hidden" );
                $( ".cestini.previous" ).addClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
                $( ".cestini.finish" ).addClass( "hidden" );
            }
            if ( newindex == 2 )
            {
                $( ".cestini.richiesta" ).removeClass( "hidden" );
                $( ".cestini.previous" ).removeClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
                $( ".cestini.finish" ).addClass( "hidden" );
            }
            if ( newindex == 3 )
            {
                $( ".cestini.richiesta" ).addClass( "hidden" );
                $( ".cestini.previous" ).removeClass( "hidden" );
                $( ".cestini.finish" ).removeClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
            }
        },
        onTabShow: function ( tab, navigation, index )
        {
            var $total = navigation.find( 'li' ).length - 1;
            var $current = index;
            var $percent = Math.floor(( $current / $total ) * 100 );
            $( '#w3' ).find( '.progress-indicator' ).css( { 'width': $percent + '%' } );
            tab.prevAll().addClass( 'completed' );
            tab.nextAll().removeClass( 'completed' );
        },
        onPrevious: function ( tab, navigation, index, newindex )
        {
            $( 'label.error' ).each( function ()
            {
                $( this ).remove();
            } );
        }
    } );

    function getRiepilogoCestino()
    {
        var obj = $( "#form-gestioneordine" ).serialize();
        $.ajax( {
            url: "/Cestini/RiepilogoCestino",
            type: "POST",
            data: obj,
            async: false,
            cache: false,
            success: function ( data )
            {
                $( '#divriepilogo' ).html( ' ' );
                $( '#divriepilogo' ).html( data );
                $( "#divriepilogo" ).removeAttr( "hidden" );
                $( ".cestini.richiesta" ).removeClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
                $( ".cestini.annulla" ).addClass( "hidden" );
            }
        } );
    }

    function getRiepilogo()
    {
        $.ajax( {
            url: "/Cestini/Riepilogo",
            type: "POST",
            async: false,
            cache: false,
            success: function ( data )
            {
                $( '#divRiepilogoFinale' ).html( ' ' );
                $( '#divRiepilogoFinale' ).html( data );
                $( "#divRiepilogoFinale" ).removeAttr( "hidden" );
                $( ".cestini.richiesta" ).removeClass( "hidden" );
                $( ".cestini.modifica" ).addClass( "hidden" );
                $( ".cestini.annulla" ).addClass( "hidden" );
            }
        } );
    }

    $( '#w3 li:eq(1) a' ).tab( 'show' );

    $.ajax( {
        url: "/Cestini/LoadEditOrdineTab",
        async: false,
        type: "GET",
        cache: false,
        data: {
        },
        contentType: "application/json; charset=utf-8",
        error: function ( jqXHR, textStatus, errorThrown )
        {
            alert( jqXHR + "-" + textStatus + "-" + errorThrown );
        },
        success: function ( data, textStatus, jqXHR )
        {
            $( '#OrdineContainer' ).html( data );
        }
    } );
}

function initOrdine()
{
    UIRai.initHelpers( 'datetimepicker' );
    $( "#ordine_cespite option" )[0].disabled = true;
    $( "#ordine_tipoPasto option" )[0].disabled = true;
    $( '#data_ora_pasto' ).data( "DateTimePicker" ).maxDate( moment().add( 30, 'days' ) );
    $( '#data_ora_pasto' ).data( "DateTimePicker" ).minDate( moment() );
    if ( $( '#data_ora_pasto' ).val() == "" )
    { $( '#data_ora_pasto' ).val( moment() ) }
}

function ripristinaCampi()
{
    var tipo = $( '#tipo_DestinatarioCestino' ).val();

    if ( tipo == 1 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
        $( '#SelectUser' ).select2( "val", "" );
    }
    else if ( tipo == 2 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).val( null );
        $( '#richiestaCorrente_nomeRisorsa' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).text( null );
        $( '#richiestaCorrente_nomeRisorsa' ).text( null )
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).val( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
        $( '#SelectUser' ).select2( "val", "" );
    }
    else if ( tipo == 3 )
    {
        $( '#richiestaCorrente_idOrdine' ).val( null );
        $( '#richiestaCorrente_codiceRichiesta' ).val( null );
        $( '#richiestaCorrente_dataInserimento' ).val( null );
        $( '#richiestaCorrente_idRichiesta' ).val( null );
        $( '#richiestaCorrente_progressivo' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).val( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).val( null );
        $( '#richiestaCorrente_nomeRisorsa' ).val( null );
        $( '#richiestaCorrente_motivoEsterno' ).text( null );
        $( '#richiestaCorrente_cognomeRisorsa' ).text( null );
        $( '#richiestaCorrente_nomeRisorsa' ).text( null )
        $( '#SelectUser' ).val( null );
        $( '#SelectUser' ).text( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).val( null );
        $( '#richiestaCorrente_matricolaRisorsa' ).text( null );
        $( '#richiestaCorrente_SelectTipoCestino' ).val( 0 ).trigger( 'change.select2' );
        $( "#richiestaCorrente_tipoCestino" ).val( null );
    }
    $( ".cestini.richiesta" ).removeClass( "hidden" );
    $( ".cestini.modifica" ).addClass( "hidden" );
    $( ".cestini.annulla" ).addClass( "hidden" );
}

function setDataFine()
{
        if( $("#_flagMyWork")[0].checked)
        {
            $("#_dataFine").attr('disabled', 'disabled');
            $("#_annoFine").attr('disabled', 'disabled');
            $("#_meseFine").attr('disabled', 'disabled');
            $("#_giornoFine").attr('disabled', 'disabled');

        }else{
            $("#_dataFine").removeAttr('disabled');
            $("#_annoFine").removeAttr('disabled');
            $("#_meseFine").removeAttr('disabled');
            $("#_giornoFine").removeAttr('disabled');
        }
        $("#_dataFine").val("");
        $("#_annoFine").val("");
        $("#_meseFine").val("");
        $("#_giornoFine").val("");
}
function InserisciAbbonamentoPubb()
{
        if($("#CittaAbbonamentoPubb").val() =="")
        {
            swal('Errore', "Inserire una città per abbonarsi", 'error');
        }else{
            
            window.location.href = "/Abbonamenti/Index?CittaAbbonamento=" + $("#CittaAbbonamentoPubb").val();
    
        }
}

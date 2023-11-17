
function Show_CV_Wizard(tipo) {
    switch (tipo) {
        case 'laurea':
            $("#_tableTarget").val("I"); //imposto la tabella a cui deve fare riferimento I=TCVIstruzione
            $(".frk_codTipoTitolo").show();
            $(".frk_corsoLaurea").show();
            $(".frk_codTipoTitolo_text").text("Tipo Laurea");
            $(".frk_codTitolo_text").text("Classe di Laurea");
            $(".frk_dataInizio").text("Anno Inizio");
            $(".frk_dataFine").text("Anno Conseguimento");
            $(".frk_riconoscimento").show();
            $(".frk_titoloTesi").show(); //da implementare
            $(".frk_titoloSpecializ").hide();
            $(".frk_info").hide(); //da implementare
            //per la data seleziono il formato YYYY
            $(".datainizio").attr("id", "");
            $(".datainizio").attr("name", "");
            $(".datafine").attr("id", "");
            $(".datafine").attr("name", "");
            $(".stile-data").hide();
            $("#tmp_val").removeAttr('selected');
            $(".annoinizio").attr("id", "_dataInizio");
            $(".annoinizio").attr("name", "_dataInizio");
            $(".annofine").attr("id", "_dataFine");
            $(".annofine").attr("name", "_dataFine");
            $(".stile-anno").show();
            PulisciWizard();
            $("#_titoloSpecializ").val('-');
            Spegni_Errori_VotoScala();
            $("#flagChangedInput").val('0');
            $(".change-idDiv").attr("id", "tour-laurea");
            $("#form-inserimentocv .istituto-for-diploma").hide();
            $("#form-inserimentocv .istituto-for-university").show();
            StilizzaSelectCv('stud', '#form-inserimentocv')

            $(".diploma-cod-ist").attr("disabled", "disabled");
          
            StartDiplomaLaureaSpecTour(false);
            //---------------------------------------
            break;
        case 'diploma':
            $("#_tableTarget").val("I"); //imposto la tabella a cui deve fare riferimento I=TCVIstruzione
            $(".frk_codTipoTitolo").hide();
            $(".frk_corsoLaurea").hide();
            $(".frk_codTitolo_text").text("Diploma");
            $(".frk_dataInizio").text("Anno Inizio");
            $(".frk_dataFine").text("Anno Conseguimento");
            $(".frk_riconoscimento").hide();
            $(".frk_titoloTesi").hide(); //da implementare
            $(".frk_titoloSpecializ").hide();
            $(".frk_info").hide(); //da implementare
            //per la data seleziono il formato YYYY
            $(".datainizio").attr("id", "");
            $(".datainizio").attr("name", "");
            $(".datafine").attr("id", "");
            $(".datafine").attr("name", "");
            $(".stile-data").hide();

            $(".annoinizio").attr("id", "_dataInizio");
            $(".annoinizio").attr("name", "_dataInizio");
            $(".annofine").attr("id", "_dataFine");
            $(".annofine").attr("name", "_dataFine");
            $(".stile-anno").show();
            PulisciWizard();
            $("#_corsoLaurea").val('-1');

            $("#_titoloSpecializ").val('-1');

            //$("#_codTipoTitolo").val('00');//.trigger('change');
            //$("#_codTipoTitolo").text('00');
            $("#tmp_val").attr('selected', 'selected');
            //freak test
            $("#_scala").attr("placeholder", "Es. 60 o 100")

            Spegni_Errori_VotoScala();
            $("#flagChangedInput").val('0');
            $(".change-idDiv").attr("id", "tour-diploma");
            // ---------------------------------------------
            $("#form-inserimentocv .istituto-for-diploma").show();
            $("#form-inserimentocv .istituto-for-university").hide();
            StilizzaSelectCv('stud', '#form-inserimentocv')
            //----------------------------------------
            //chiamata ajax per riempire la select di DTitolo
            FillDTitoloByCodTipoTitolo("DI", "_codTitolo");
            $(".diploma-cod-ist").removeAttr("disabled");
            $("#_flagDiploma").prop("checked", true);
            StartDiplomaLaureaSpecTour(false);
            break;
        case 'special':
            $("#_tableTarget").val("S"); //imposto la tabella a cui deve fare riferimento I=TCVSpecializz
            $(".frk_codTipoTitolo").hide();
            //$(".frk_codTitolo").hide();
            $(".frk_corsoLaurea").hide();
            $(".frk_codTitolo_text").text("Tipo specializzazione o titolo di merito");
            $(".frk_titoloSpecializ").show();
            $(".frk_dataInizio").text("Data di Inizio");
            $(".frk_dataFine").text("Data di Conseguimento");
            $(".frk_riconoscimento").show();
            $(".frk_titoloTesi").hide(); //da implementare
            $(".frk_info").show(); //da implementare
            //per la data seleziono il formato DD/MM/YYYY
            $(".annoinizio").attr("id", "");
            $(".annoinizio").attr("name", "");
            $(".annofine").attr("id", "");
            $(".annofine").attr("name", "");
            $(".stile-anno").hide();

            $(".datainizio").attr("id", "_dataInizio");
            $(".datainizio").attr("name", "_dataInizio");
            $(".datafine").attr("id", "_dataFine");
            $(".datafine").attr("name", "_dataFine");
            $(".stile-data").show();
            //$("#_codTipoTitolo").val('00');//.trigger('change');
            //$("#_codTipoTitolo").text('00');
            $("#tmp_val").attr('selected', 'selected');
            //$("#tmp_va_codtitolo").attr('selected', 'selected');
            $("#_corsoLaurea").val('-');
            $("#_voto").val('');
            $("#_voto").text('');
            $("#_scala").val('');
            $("#_scala").text('');
            $("#_titoloSpecializ").val('');
            Spegni_Errori_VotoScala();
            $(".change-idDiv").attr("id", "tour-master");
            // ---------------------------------------------
            StilizzaSelectCv('stud', '#form-inserimentocv')
            //----------------------------------------
            //chiamata ajax per riempire la select di DTitolo
            $("#form-inserimentocv .istituto-for-diploma").hide();
            $("#form-inserimentocv .istituto-for-university").show();

            $("#form-inserimentocv .istituto-for-diploma > #_istituto").val('');
            $("#form-inserimentocv .istituto-for-university > #_istituto").val('');

            FillDTitoloByCodTipoTitolo("MA", "_codTitolo");
            $("#flagChangedInput").val('0');
            $(".diploma-cod-ist").attr("disabled", "disabled");
            StartDiplomaLaureaSpecTour(false);
            break;
    }
}

function FillDTitoloByCodTipoTitolo(cod, id_elem, value_cod) {

    if (value_cod == null) {
        value_cod = "FRK-NOVALUE";
    }
    $.ajax({
        url: "/CV_ajax/GetTitoloByCodTipoTitolo",
        type: "POST",
        dataType: "json",
        data: { codTipoTitolo: cod },
        success: function (data) {
            if (data.result != null) {
                var lista = [];
                lista = data.result;
                var $select = $('#' + id_elem);
                $select.find('option').remove();
                $('<option>').val('').text('').appendTo($select);
                for (var elem in lista) {
                    if (lista[elem].CodTitolo == value_cod) {
                        $('<option>').val(lista[elem].CodTitolo).attr('selected', 'selected').text(lista[elem].DescTitolo).appendTo($select);
                    }
                    else {
                        $('<option>').val(lista[elem].CodTitolo).text(lista[elem].DescTitolo).appendTo($select);
                    }
                }
            }
        },
        error: function (result) {
            alert("Failed");
        }
    });
}

function PulisciWizard() {
    $("#_codTitolo").val(' ').trigger('change');
    $("#_codTipoTitolo").val(' ').trigger('change');
    $("#_corsoLaurea").val('');
    $("#_titoloSpecializ").val(' ');
    $("#_dataInizio").val('');
    $("#_dataFine").val('');
    $("#_voto").val('');
    $("#_voto").text('');
    $("#_scala").val('');
    $("#_scala").text('');
    $("#_riconoscimento").val('');
    $("#_lode").removeAttr("checked");
    $("#_istituto").val('');
    $("#_localitaStudi").val('');
    $("#_codNazione").val('').trigger('change');
    $("#_indirizzoStudi").val('');
    $("#_note").val('');
    $("#_titoloTesi").val('');
    $("#frkmodal").html('');
    Spegni_Errori_VotoScala();
    Spegni_Errori_DataInizioFine();
    $("#clickable").click();
    $("#clickable").trigger("click");
}

function VotoScalaCheck_submit() {
    var voto = parseInt($("#_voto").val());
    var scala = parseInt($("#_scala").val());
    if ((voto > scala) || ((isNaN(voto)) ^ (isNaN(scala)))) {
        Accendi_Errori_VotoScala();
        event.preventDefault();
    }
    else {
        Spegni_Errori_VotoScala();
    }

}

function Spegni_Errori_VotoScala() {
    $("._customVotoScalaError").hide();
    $("#_voto").removeClass("error input-validation-error");
    $("#_scala").removeClass("error input-validation-error");
}
function Accendi_Errori_VotoScala() {
    $("#_voto").addClass("error input-validation-error");
    $("#_scala").addClass("error input-validation-error");
    $("._customVotoScalaError").show();
}
function onlyNumeric(evt) {
    /*Questa condizione ternaria è necessaria per questioni di compatibilità tra browser se "evt.which" non viene preso, usa "event.keyCode" */
    var charCode = (evt.which) ? evt.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57))
        return false;
    return true;
}


function CV_Form_Submit(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbstudies').show();

    if ($("#_flagDiploma").prop("checked") == true)
    {
        $("input.diploma-cod-ist").val("-1");
    }

    var form = $(event.target).parents("form").first();
    var validator = $( form ).validate();

    if ( !$( form ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbstudies').hide();
        return false;
    }

    $('#_istituto').val($('#_codIstituto option:selected').text());
    var x = $('#_istituto').val();
    var obj = $(".form-inserimentocv").serialize();

    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/InsertCV",
        type: "POST",
        //dataType: "JSON",
        //contentType: 'application/json; charset=utf-8',
        data: obj,
        cache: false,
        //type: "GET",
        //data: obj,
        //dataType: "json",
        success: function (data) {
            $("#savedbstudies").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    Spegni_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    ShowAlertInsert('insert');
                    CallActionAjax('studies', 'box-studies', '.form-inserimentocv');
                    //window.location.href = "/CV_Online/";
                    break;
                case "scala":
                    Accendi_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    $(button).removeClass("disable");
                    $('#savedbstudies').hide();
                    break;
                case "scaladatafine":
                    Accendi_Errori_VotoScala();
                    Accendi_Errori_DataInizioFine();
                    $(button).removeClass("disable");
                    $('#savedbstudies').hide();
                    break;
                case "datafine":
                    Accendi_Errori_DataInizioFine();
                    Spegni_Errori_VotoScala();
                    $(button).removeClass("disable");
                    $('#savedbstudies').hide();
                    break;
                case "error":
                    //errore di inserimento
                    swal("L'inserimento non è andato a buon fine. : ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbstudies').hide();
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    Spegni_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    $(button).removeClass("disable");
                    $('#savedbstudies').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbstudies').hide();
        }
    });
}
function getLauree()
{
    var l= ($(".sel-tipo-laurea").val());
    $.ajax({
        type: 'POST',
        url: "/cv_online/getlauree",
        dataType: "json",
        data: { tipoLaurea: l},
        cache: false,
        success: function (data) {
            $(".sel-laurea").empty();
            //alert(JSON.stringify(data));
            for (var i = 0; i < data.length; i++)
            {
                if (data[i].Text == null) continue;
                $(".sel-laurea").append("<option value='"+data[i].Value+"'>"+data[i].Text+"</option>");
            }
        },
        error: function (aa, b, c) {
            swal(aa + b + c, '', 'error');
        },
        complete: function () {

        }
    });
}

function Spegni_Errori_DataInizioFine() {
    $("._customDataInizioFineError").hide();
    $("#_dataInizio").removeClass("error input-validation-error");
    $("#_dataFine").removeClass("error input-validation-error");
}

function Accendi_Errori_DataInizioFine() {
    $("#_dataInizio").addClass("error input-validation-error");
    $("#_dataFine").addClass("error input-validation-error");
    $("._customDataInizioFineError").show();
}

function CV_Modifica_CV(elem) {
    var obj = elem;
    var tipo = elem._logo;
    var codTitolo = elem._codTitolo;
    var codTipoTitolo = elem._codTipoTitolo;
    $("#frkModificaStudies").html(' ');
    $.ajax({
        url: "/CV_Online/ModificaStudies",
        type: "POST",
        data: obj,
        async: false,
        success: function (data) {
            $("#frkModificaStudies").html('');
            $("#frkModificaStudies").html(data);
            UIRai.initHelpers('datetimepicker');
            $(".js-select2").select2({
                placeholder: "Seleziona dalla lista",
            });
            StilizzaSelectCv('stud', '#frkModificaStudies');
            switch (tipo) {
                case "Diploma":
                    //FillDTitoloByCodTipoTitolo("DI", "_codTitolo", codTitolo);
                    Spegni_Errori_DataInizioFine();
                    Spegni_Errori_VotoScala();
                    break;
                case "Laurea":
                    Spegni_Errori_DataInizioFine();
                    Spegni_Errori_VotoScala();
                    break;
                case "Master":
                    Spegni_Errori_DataInizioFine();
                    Spegni_Errori_VotoScala();
                    break;
                default:
                    break;
            }
        },
        error: function (result) {
            alert("Avvenuto errore di modifica");
        }
    });
}

function PulisciPopup(elem) {
    $("#" + elem).html('');
}

function FormModificaCV(button,savedb) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#'+savedb).show();

    var validator = $( ".form-modificacv" ).validate();

    if ( !$( ".form-modificacv" ).valid() )
    {
        validator.focusInvalid();
        return false;
    }

    //freak - continuare ad implementare
    var obj = $(".form-modificacv").serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/ModificaCV",
        type: "POST",
        //dataType: "JSON",
        //contentType: 'application/json; charset=utf-8',
        data: obj,
        cache: false,
        //type: "GET",
        //data: obj,
        //dataType: "json",
        success: function (data) {
            //alert(data);
            $("#"+savedb).fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    Spegni_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    PulisciPopup("frkModificaStudies");
                    ShowAlertInsert('update');
                    CallActionAjax('studies', 'box-studies');
                    //window.location.href = "/CV_Online/";
                    break;
                case "scala":
                    Accendi_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    break;
                case "scaladatafine":
                    Accendi_Errori_VotoScala();
                    Accendi_Errori_DataInizioFine();
                    break;
                case "datafine":
                    Accendi_Errori_DataInizioFine();
                    Spegni_Errori_VotoScala();
                    break;
                case "error":
                    //errore di inserimento
                    alert("La modifica non è andata a buon fine. Errore: " + data);
                    break;
                default:
                    alert("La modifica non è andata a buon fine. Errore: " + data);
                    Spegni_Errori_VotoScala();
                    Spegni_Errori_DataInizioFine();
                    break;
            }
        },
        error: function (result) {
            alert("Modifica non riuscita - Failed" + result);
        }
    });

}

function ConfermaCancellazione_Studies(tipo, matricola, key) {
    swal({
        title: "Sicuro di voler cancellare l'elemento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        if (tipo == "master") {
            $.ajax({
                url: "/CV_Online/DeleteStudiesMaster?matricola=" + matricola + "&prog=" + key,
                type: "GET",
                cache: false,
                success: function () {
                    CallActionAjax('studies', 'box-studies');
                }
            });
            //window.location.href = "/CV_Online/DeleteStudiesMaster?matricola=" + matricola + "&prog=" + key;
        }
        else {
            $.ajax({
                url: "/CV_Online/DeleteStudiesDiplomaLaurea?matricola=" + matricola + "&codTitolo=" + key,
                type: "GET",
                cache: false,
                success: function () {
                    CallActionAjax('studies', 'box-studies');
                }
            });
            //window.location.href = "/CV_Online/DeleteStudiesDiplomaLaurea?matricola=" + matricola + "&codTitolo=" + key;
        }
    });
}

function ConfermaCancellazione_Languages(matricola, codLingua) {
    swal({
        title: "Sicuro di voler cancellare l'elemento selezionato?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText: ' Si',
        cancelButtonText: ' No'
    }).then(function () {
        $.ajax({
            url: "/CV_Online/DeleteLanguages?matricola=" + matricola + "&codLingua=" + codLingua,
            type: "GET",
            cache: false,
            success: function () {
                CallActionAjax('languages', 'box-languages');
            },
            error: function (result) {
                swal('Si è verificato un errore',
                    result,
                    'error');
            }
        });
    });
}

function ConfermaCancellazione_AreeInteresse(matricola, prog) {
    swal({
        title: "Sicuro di voler cancellare l'elemento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        $.ajax({
            url: "/CV_Online/DeleteAreeInteresse?matricola=" + matricola + "&prog=" + prog,
            type: "GET",
            cache: false,
            success: function () {
                CallActionAjax('AreeInteresse', 'box-AreeInteresse');
            }
        });
        //window.location.href = "/CV_Online/DeleteAreeInteresse?matricola=" + matricola + "&prog=" + prog;
    });
}

function ConfermaCancellazione_Formazione(matricola, prog) {
    swal({
        title: "Sicuro di voler cancellare l'elemento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        $.ajax({
            url: "/CV_Online/DeleteFormazione?matricola=" + matricola + "&prog=" + prog,
            type: "GET",
            cache: false,
            success: function () {
                CallActionAjax('Formazione', 'box-Formazione');
            }
        });
        //window.location.href = "/CV_Online/DeleteFormazione?matricola=" + matricola + "&prog=" + prog;
    });
}

function scegliTipoContributo(id_elem) {
    swal({
        title: 'Che tipo di contributo vuoi aggiungere?',
        html: '<div class="col-md-6" ><div class="widget-summary widget-summary-xlg" style="margin-top:20px;">	<div class="widget-summary-col widget-summary-col-icon" style="width:100%;">		<div class="summary-icon item item-rounded text-primary bigborder">			<span class="cursor-pointer" onclick="scegliLink()"><i tabindex="0" role="link" aria-label="Aggiungi un nuovo contributo" class="icons icon-link" title="Aggiungi un link"></i></span></div></div></div><div class="text-center"><a tabindex="-1" onclick="scegliLink()" target="_blank"><p class="text-bold text-primary">Aggiungi un link</p></a></div></div><div class="col-md-6" ><div class="widget-summary widget-summary-xlg" style="margin-top:20px;"><div class="widget-summary-col widget-summary-col-icon" style="width:100%;"><div class="summary-icon item item-rounded text-primary bigborder"><span class="cursor-pointer" ><i tabindex="0" role="link" aria-label="Aggiungi un nuovo contributo" class="icons icon-doc" onclick="click_default(\'' + id_elem + '\')" title="Aggiungi un file"></i></span></div></div></div><div class="text-center"><a tabindex="-1" onclick="click_default(\'' + id_elem + '\')" target="_blank"><p class="text-bold text-primary">Aggiungi un file</p></a></div></div>',
        showConfirmButton: false
    })
}

function scegliLink() {
    swal({
        title: 'Il file è stato caricato correttamente',
        text: "Assegna un nome al documento caricato",
        html: '<p>Inserisci l\'url del link che vuoi aggiungere</p><input tabindex="0" type="text" name="tmp_url" placeholder="Inserisci l\'url del link" id="tmp_url" class="form-control formElements"/><br/><p>Assegna un nome al documento caricato</p><input tabindex="0" type="text" name="tmp_name" placeholder="Inserisci il nome del file" id="tmp_name" class="form-control formElements"/><br/><p>Inserisci una breve descrizione</p><textarea tabindex="0" placeholder="Inserisci una breve descrizione" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important"/>',
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
                if ($("#tmp_url").val() == "") {
                    reject("Inserisci l'url")
                }
                if ($("#tmp_name").val() == "") {
                    reject("Inserisci il nome del contributo")
                }
                else if ($("#tmp_name").val().length > 100) {
                    reject("Il nome può essere lungo al massimo 100 caratteri")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        //var item = result;
        var url = $("#tmp_url").val();
        var item = $("#tmp_name").val();
        var desc = $("#tmp_des").val();
        $("#_name").val(item);
        $("#_note").val(desc);
        $("#_pathName").val(url);
        submit_allegati('frmInsertAllegati');
    })
}

function click_default(id_elem) {
    $("#" + id_elem).click();
}

function scegliTipoAllegato(id_elem) {
    swal({
        title: 'Che tipo di contributo vuoi aggiungere?',
        html: '<div class="col-md-6" ><div class="widget-summary widget-summary-xlg" style="margin-top:20px;">	<div class="widget-summary-col widget-summary-col-icon" style="width:100%;">		<div class="summary-icon item item-rounded text-primary bigborder">			<span class="cursor-pointer" onclick="scegliLinkAll()"><i tabindex="0" role="link" aria-label="Aggiungi un nuovo contributo" class="icons icon-link" title="Aggiungi un link"></i></span></div></div></div><div class="text-center"><a tabindex="-1" onclick="scegliLinkAll()" target="_blank"><p class="text-bold text-primary">Aggiungi un link</p></a></div></div><div class="col-md-6" ><div class="widget-summary widget-summary-xlg" style="margin-top:20px;"><div class="widget-summary-col widget-summary-col-icon" style="width:100%;"><div class="summary-icon item item-rounded text-primary bigborder"><span class="cursor-pointer" ><i tabindex="0" role="link" aria-label="Aggiungi un nuovo contributo" class="icons icon-doc" onclick="click_defaultAll(\'' + id_elem + '\')" title="Aggiungi un file"></i></span></div></div></div><div class="text-center"><a tabindex="-1" onclick="click_defaultAll(\'' + id_elem + '\')" target="_blank"><p class="text-bold text-primary">Aggiungi un file</p></a></div></div>',
        showConfirmButton: false
    }).then(function (result) {
        
    })
}

function click_defaultAll(id_elem) {
    swal.close();
    $("#" + id_elem).click();
}

function scegliLinkAll() {
    swal({
        title: '',
        text: "",
        html: '<p>Inserisci l\'url del link che vuoi aggiungere</p><input tabindex="0" type="text" name="tmp_url" placeholder="Inserisci l\'url del link" id="tmp_url" class="form-control formElements"/>',
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
                if ($("#tmp_url").val() == "") {
                    reject("Inserisci l'url")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        $("#form-editAllegato #_pathName").val($('#tmp_url').val());
        $('#form-editAllegato #iAddFile').hide();
        $('#form-editAllegato #iAddedFile').show();
        $('#form-editAllegato #iAddedFile').attr("class", "icon icon-globe");
        $('#form-editAllegato #spanAddFile').removeClass('cursor-pointer');
        $('#form-editAllegato #boxFile').css("border-style", "solid");
        $('#form-editAllegato #fmtFile').text('Link a sito web');
        $('#form-editAllegato #hrefCanc').show();
    })
}

function PulisciFormLingue() {
    $("#_codLingua").val('').trigger('change');
    $("#_livAscolto").val('').trigger('change');
    $("#_livLettura").val('').trigger('change');
    $("#_livInterazione").val('').trigger('change');
    $("#_livProdOrale").val('').trigger('change');
    $("#_livScritto").val('').trigger('change');
    $("#_setMadrelingua").removeAttr("checked");
    AttivaLivelli(true);
    $("#_note").val('');
}

function PulisciFormAreeInteresse() {
    $("#_codAreaOrg").val('').trigger('change');
    $("#_codServizio").val('').trigger('change');
    $("#_codTipoDispo").val('').trigger('change');
    $("#_codAreaGeo").val('').trigger('change');
    $("#_areeIntDispo").text('');
    $("#_profIntDispo").text('');
    $("#_flagEsteroDispo").removeAttr("checked");
}

function PulisciFormFormazione() {
    $("#_codNazione").val('').trigger('change');
    $("#_corso").text('');
    $("#_anno").text('');
    $("#_durata").text('');
    $("#_localitaCorso").text('');
    $("#_note").text('');
}


function AttivaLivelli(form, activate) {
    if (activate) {
        $("#" + form + " #scelta_livelli select").removeAttr("disabled");
    }
    else {
        $("#" + form + " #scelta_livelli select").attr("disabled", "disabled");
    }
}

function CheckMadrelingua(form) {
    if ($("#" + form + " #_setMadrelingua").is(":checked")) {
        jQuery("input[type='radio']").each(function (i) {
            jQuery(this).attr('disabled', 'disabled');
        });
        jQuery("input[value='C2']").each(function (i) {
            jQuery(this).removeAttr('disabled');
            jQuery(this).trigger('click');
        });
    }
    else {
        jQuery("input[type='radio']").each(function (i) {
            jQuery(this).removeAttr('disabled');
        });
        jQuery("#" + form + " .first-element-checkable").each(function (j) {
            jQuery(this).trigger('click');
        });
    }
}

function submit_language(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedblanguage').show();
    
    var validator = $( "#form-insertlanguages" ).validate();

    if ( !$( "#form-insertlanguages" ).valid() )
    {
        $( '#_codLingua' ).focus();
        $( ".modal-content" ).scrollTop( 0 );
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedblanguage').hide();
        return false;
    }

    if ($(".form-insertlanguages #_setMadrelingua").is(":checked")) {
        $(".form-insertlanguages #_codLinguaLiv").val('09');
    }
    else {
        $(".form-insertlanguages #_codLinguaLiv").val('01');
    }

    var obj = $(".form-insertlanguages").serialize();
    $.ajax({
        url: "/CV_Online/InsertLanguage",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedblanguage").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('languages', 'box-languages', null);
                    //CallActionAjax('languages', 'box-languages', '.form-insertlanguages');
                    ShowAlertInsert('insert');
                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal('Si è verificato un errore',
                        bo,
                        'error');
                    $(button).removeClass("disable");
                    $('#savedblanguage').hide();
                    break;
            }
        },
        error: function (result) {
            swal('Si è verificato un errore',
                'Impossibile inserire i dati relativi alla lingua selezionata',
                'error');
            $(button).removeClass("disable");
            $('#savedblanguage').hide();
        }
    });
}

function ModificaLanguage(elem) {
    var obj = elem;

    RaiOpenAsyncModal("frkmodlingua", "/CV_Online/Create_ViewLanguages", elem, function () {
        CheckMadrelingua('form-modificalanguages');
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
        InizializzaTuttoDopoAjax();
    }, "POST");
}

function submit_modificalanguage(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbmodLang').show();

    var validator = $( "#form-modificalanguages" ).validate();

    if ( !$( "#form-modificalanguages" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbmodLang').hide();
        return false;
    }

    if ($(".form-modificalanguages #_setMadrelingua").is(":checked")) {
        $(".form-modificalanguages #_codLinguaLiv").val('09');
    }
    else {
        $(".form-modificalanguages #_codLinguaLiv").val('01');
    }
    //freak - continuare ad implementare
    var obj = $(".form-modificalanguages").serialize();
    $.ajax({
        url: "/CV_Online/ModificaLanguage",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbmodLang").fadeOut("slow", function () {
            });
        switch (data) {
            case "ok":
                CallActionAjax('languages', 'box-languages', null);
                ShowAlertInsert('insert');
                break;
            default:
                var bo = data.replace(/;/g, '\n');
                swal('Si è verificato un errore',
                    bo,
                    'error');
                $(button).removeClass("disable");
                $('#savedbmodLang').hide();
                break;
            }
        },
        error: function (result) {
            swal('Si è verificato un errore',
                'Impossibile modificare i dati relativi alla lingua selezionata',
                'error');
            $(button).removeClass("disable");
            $('#savedbmodLang').hide();
        }
    });
}

//FUNZIONE CHIAMATA AJAX PER AREEINTERESSE
function ModificaAreeInteresse(elem) {
    RaiOpenAsyncModal("frkModificaAreeInteresse", "/CV_Online/Create_ViewAreeInteresse", elem, function () {
        $("#form-modificaareainteresse #_codAreaOrg").trigger('change');
        $("#form-modificaareainteresse #_listaLocalita").select2({
            maximumSelectionLength: 3,
            placeholder: "Seleziona dalla lista"
        });
    }, "POST");
}

function submit_modificaAreeInteresse(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbmodAreaInt').show();

    var validator = $( "#form-insertareainteresse" ).validate();

    if ( !$( "#form-insertareainteresse" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbmodAreaInt').hide();
        return false;
    }

    //freak - continuare ad implementare
    var obj = $(".form-modificaareainteresse").serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/ModificaAreeInteresse",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbmodAreaInt").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('AreeInteresse', 'box-AreeInteresse');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbmodAreaInt').hide();
                    break;
                case "error":
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbmodAreaInt').hide();
                    break;
                default:
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbmodAreaInt').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Modifica non riuscita - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbmodAreaInt').hide();
        }
    });
}

function FillVDServizioByCodServizio(cod, id_elem, value_cod) {
    if (value_cod == null) {
        value_cod = "FRK-NOVALUE";
    }
    $.ajax({
        url: "/CV_ajax/GetServizioByAreaOrgServCodServizio",
        type: "POST",
        dataType: "json",
        data: { codAreaOrg: cod },
        success: function (data) {
            if (data.result != null) {
                var lista = [];
                lista = data.result;
                var $select = $('#' + id_elem);
                $select.find('option').remove();
                $('<option>').val('').text('').appendTo($select);
                for (var elem in lista) {
                    if (lista[elem].Codice.trim() == value_cod) {
                        $('<option>').val(lista[elem].Codice).attr('selected', 'selected').text(titleCase(lista[elem].Descrizione)).appendTo($select);
                    }
                    else {
                        $('<option>').val(lista[elem].Codice).text(titleCase(lista[elem].Descrizione)).appendTo($select);
                    }
                }
            }
        },
        error: function (result) {
            alert("Failed");
        }
    });
}

function submit_areaInteresse(button) {

    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbareaint').show();

    var validator = $( "#form-insertareainteresse" ).validate();

    if ( !$( "#form-insertareainteresse" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbareaint').hide();
        return false;
    }

    var obj = $( ".form-insertareainteresse" ).serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/InsertAreeInteresse",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbareaint").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('AreeInteresse', 'box-AreeInteresse', '#form-insertareainteresse');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbareaint').hide();
                    break;
                case "error":
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbareaint').hide();
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbareaint').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbareaint').hide();
        }
    });

}

function checkAreeInteresse(count) {
    if (count > 2) {
        swal("Non puoi aggiungere più di tre Aree di Intresse", "", "error");
        $("#aggiungi").removeAttr("data-toggle");
    }
    else {
        $("#aggiungi").attr("data-toggle", "modal");
    }
}

function submit_competenzeDigitali(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbcompdigit').show();

    var compDigit = new Array();
    var compDigitLiv = new Array();
    $( 'input[name="compDigit"]' ).each( function ()
    {
        compDigit.push( $( this ).val() );
    });
    for ( var idx = 0; idx < 5; idx++ )
    {
        var isChecked = $('input[type="radio"][name^="compDigitLiv_' + idx + '"]').is(':checked');
        if (isChecked) {
            var value = $( 'input[type="radio"][name^="compDigitLiv_' + idx + '"]:checked' ).val();
            compDigitLiv.push(value);
        }
        else {
            compDigitLiv.push(" ");
        }
    }

    $.ajax({
        url: "/CV_Online/InsertCompetenzeDigitali",
        type: "POST",
        data: JSON.stringify({
            compDigitLiv: compDigitLiv
        }),
        contentType: 'application/json; charset=UTF-8',
        cache: false,
        success: function (data) {
            $("#savedbcompdigit").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax( 'CompetenzeDigitali', 'box-CompetenzeDigitali' );
                    //CallActionAjax('CompetenzeDigitali', 'box-CompetenzeDigitali', '.form-insertcompetenzeDigitali');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbcompdigit').hide();
                    break;
                case "error":
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbcompdigit').hide();
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbcompdigit').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbcompdigit').hide();
        }
    });

}

function ModificaCompetenzeDigitali(elem, id_modal) {
    var obj = elem;
    $("#" + id_modal).html(' ');
    $.ajax({
        url: "/CV_Online/Create_ViewCompetenzeDigitali",
        type: "POST",
        data: obj,
        async: false,
        success: function (data) {
            $("#frk_modificaFormazione").html('');
            $("#frk_modificaFormazione").html(data);
            UIRai.initHelpers('datetimepicker');
            $(".js-select2").select2({
                placeholder: "Seleziona dalla lista",
            });
        },
        error: function (result) {
            swal("Avvenuto errore di modifica");
        }
    });
}

function submit_modificaFormazione(button) {
    event.preventDefault();
    $(button).addClass('disable');
    $('#savedbmodcorsi').show();

    var validator = $("#form-modificaformazione").validate();

    if (!$("#form-modificaformazione").valid())
    {
        validator.focusInvalid();
        $(button).removeClass('disable');
        $('#savedbmodcorsi').hide();
        return false;
    }

    var obj = $(".form-modificaFormazione").serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/ModificaFormazione",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbmodcorsi").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('Formazione', 'box-Formazione');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass('disable');
                    $('#savedbmodcorsi').hide();
                    break;
                case "error":
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    $(button).removeClass('disable');
                    $('#savedbmodcorsi').hide();
                    break;
                default:
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    $(button).removeClass('disable');
                    $('#savedbmodcorsi').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Modifica non riuscita - Failed", result, "error");
            $(button).removeClass('disable');
            $('#savedbmodcorsi').hide();
        }
    });
}

function ModificaFormazione(elem, id_modal) {
    RaiOpenAsyncModal("frk_modificaFormazione", "/CV_Online/Create_ViewFormazione", elem, function () {
        UIRai.initHelpers('datetimepicker');
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
    }, "POST");
}

function controlRadio(elem) {
    if ($("#" + elem).is(":not(:checked)")) {
        $("#" + elem).prop("checked", true);
    }
}

function controlCheck(check, elem, radio_all) {
    if ($("#" + check).is(":checked")) {
        $("#" + radio_all).prop("checked", false);
        //$("." + radio_all).removeAttr("checked");
        $("#" + elem).prop("checked", true);
    }
    else {
        $("#" + radio_all).prop("checked", false);
        $("#" + check).removeAttr("checked");
        $("#" + radio_all).removeAttr("checked");
    }
}

function SwitchPropChecked(id_elem) {
    //event.preventDefault();
    $(id_elem).prop("checked", true);
    //return false;
}

function ShowDettaglioEsperienza(isGiornalista, id_form, id_check, id_uncheck, type) {
    switch (type) {
        case 'rai':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioRai",
                type: "POST",
                data: "_isGiornalista=" + isGiornalista,
                success: function (data) {
                    //$("#" + id_check).attr("checked", "checked");
                    //$("#" + id_uncheck).removeAttr("checked");
                    //$("#" + id_check).trigger('click');
                    $("#" + id_form).html(' ');
                    $("#" + id_form).html(data);
                    UIRai.initHelpers('datetimepicker');
                    //$("#_flagEspRai").val('1');
                    //$(".dettaglio").html('');
                    //$(".dettaglio").html(data);
                    //$(".next").trigger("click");
                    $(".js-select2").select2({
                        placeholder: "Seleziona dalla lista"
                    });
                    //$("#form-insertexperiences #_flagEspRai").val('1');
                    StilizzaSelectCv('exp');
                    //$('#' + id_form + ' .textable-select').on('select2:closing', function (e) {
                    //    //Do stuff
                    //    var id = e.target.id;
                    //    var text = $(".select2-search__field").val();
                    //    var value = $("#" + e.target.id).val();
                    //    if ((value == "") || (value == null) || (value == 'undefined') || (value == '-1')) {
                    //        $("#" + id + " option[value='-1']").remove();
                    //        var text = $(".select2-search__field").val();
                    //        $("#" + id).append('<option value="-1" selected="selected">' + text + '</option>');
                    //        if (id == "_codSocieta") {
                    //            $("#_societa").val(text);
                    //        }
                    //        else {
                    //            $("#_direzione").val(text);
                    //        }
                    //    }
                    //    else {
                    //        if (id == "_codSocieta") {
                    //            $("#_societa").val(text);
                    //        }
                    //        else {
                    //            $("#_direzione").val(text);
                    //        }
                    //    }

                    //});
                },
                error: function (result) {
                    swal("Avvenuto errore di modifica");
                }
            });
            break;
        case 'extra':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioExtraRai",
                type: "POST",
                data: "_isGiornalista=" + isGiornalista,
                success: function (data) {
                    //$("#" + id_check).attr("checked", "checked");
                    //$("#" + id_uncheck).removeAttr("checked");
                    //$("#" + id_check).trigger('click');
                    $("#" + id_form).html(' ');
                    $("#" + id_form).html(data);
                    UIRai.initHelpers('datetimepicker');
                    var tmpDate;
                    var now = new Date();
                    now.setDate(now.getDate() - 1);
                    tmpDate = now.getDate().toString() + "/" + (now.getMonth() + 1).toString() + "/" + now.getFullYear().toString();
                    //tmpDate = now.toLocaleDateString();
                    //$(".form-insertexperiences #_dataFine").data("DateTimePicker").maxDate(tmpDate);
                    $(".form-insertexperiences #_annoFine").data("DateTimePicker").maxDate(now.getFullYear());
                    //$("#form-insertexperiences #_flagEspRai").val('0');
                    //$("#_flagEspRai").val('0');
                    //$(".dettaglio").html('');
                    //$(".dettaglio").html(data);
                    //$(".next").trigger("click");
                    $(".js-select2").select2({
                        placeholder: "Seleziona dalla lista"
                    });
                    StilizzaSelectCv('exp');
                    //$('#' + id_form + ' .textable-select').on('select2:closing', function (e) {
                    //    //Do stuff
                    //    var id = e.target.id;
                    //    var value = $("#" + e.target.id).val();
                    //    if ((value == "") || (value == null) || (value == 'undefined') || (value == '-1')) {
                    //        $("#" + id + " option[value='-1']").remove();
                    //        var text = $(".select2-search__field").val();
                    //        $("#" + id).append('<option value="-1" selected="selected">' + text + '</option>');
                    //        if (id == "_codSocieta") {
                    //            $("#_societa").val(text);
                    //        }
                    //        else {
                    //            $("#_direzione").val(text);
                    //        }
                    //    }
                    //    else {
                    //        if (id == "_codSocieta") {
                    //            $("#_societa").val(text);
                    //        }
                    //        else {
                    //            $("#_direzione").val(text);
                    //        }
                    //    }

                    //});
                    StartTour("#EsperienzaExtraRai", "#modalExperiencesInserimnento .modal-content", false);
                    //StartTour("#EsperienzaExtraRai", false);
                },
                error: function (result) {
                    swal("Avvenuto errore imprevisto");
                }
            });
            break;
    }
}
function submit_corsiFormazione(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbcorsi').show();

    var form = $(event.target).parents("form").first();
    var validator = $( form ).validate();

    if ( !$( form ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbcorsi').hide();
        return false;
    }

    var obj = $(".form-insertformazione").serialize();
    $.ajax({
        url: "/CV_Online/InsertFormazione",
        type: "POST",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbcorsi").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('Formazione', 'box-Formazione', '.form-insertformazione');
                    //window.location.href = "/CV_Online/";
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbcorsi').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbcorsi').hide();
        }
    });

}

function submit_esperienzeProfessionali(button)
{
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbexp').show();
 

    if ( ( $( ".form-insertexperiences #_dataFine" ).val() == "" ) || ( $( ".form-insertexperiences #_dataFine" ).val() == undefined ) )
    {
        $( ".form-insertexperiences #_dataFine" ).val( "31/12/9999" );
        $( ".form-insertexperiences #_dataFine" ).text( "31/12/9999" );
    }

    $("#flagRai").val("1");
    $("#flagExtraRai").val("0");

    var validator = $( ".form-insertexperiences" ).validate();

    if ( !$( ".form-insertexperiences" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbexp').hide();
        return false;
    }
    //controllo che la data iniziale sia minore della data finale
    var dataInizio = $("#_dataInizio").val();
    var dataFine = $("#_dataFine").val();

    var obj = $(".form-insertexperiences").serialize();
    $.ajax({
        url: "/CV_Online/InsertExperiences",
        type: "POST",
        data: obj,
        cache: false,
        success: function ( data )
        {
            $("#savedbexp").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('experiences', 'box-experiences', '.form-insertexperiences');
                    ShowAlertInsert('insert');
                    break;
                case "error-data":
                    swal("Indicare una data Inizio e Fine nella relativa fascia di appartenenza: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbexp').hide();
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbexp').hide();
                    break;
            }
        },
        error: function ( result )
        {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbexp').hide();
        }
    });
}

function CheckFasciaData(dataInizio, dataFine, elem, type) {

    //setto la variabile nascosta _dataInizio _dataFine
    if (type == "I") {
        var date = $("#" + elem).val();
        $("#_dataInizio").val(date);
    }
    else {
        var date = $("#" + elem).val();
        $("#_dataFine").val(date);
    }
}

function click_default_certificazione() {
}

function ShowDettaglioCertificazione(type) {
    $(".divdettaglio").html(' ');
    switch (type) {
        case 'att':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioAttestato",
                type: "POST",
                success: function (data) {
                    $(".dettaglio").html('');
                    $(".dettaglio").html(data);
                    UIRai.initHelpers('datetimepicker');
                    $('#_datainizio').data("DateTimePicker").maxDate(moment());
                    //$('#_datafine').data("DateTimePicker").maxDate(moment());
                    $('#_datafine').data("DateTimePicker").maxDate("31/12/9999");
                    $("#_tipo").val("1");
                },
                error: function (result) {
                    swal('Si è verificato un errore',
                        'Errore nel caricamento della pagina',
                        'error');
                }
            });
            break;
        case 'pub':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioPubblicazione",
                type: "POST",
                success: function (data) {
                    $(".dettaglio").html('');
                    $(".dettaglio").html(data);
                    UIRai.initHelpers('datetimepicker');
                    $("#_dataPubblicazione").data("DateTimePicker").maxDate(moment());
                    $("#_tipo").val("2");
                    StartTour("#tour-pubb", "#modalCertificazioniInserimento .modal-content", false);
                },
                error: function (result) {
                    swal('Si è verificato un errore',
                        'Errore nel caricamento della pagina',
                        'error');
                }
            });
            break;
        case 'bre':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioBrevetto",
                type: "POST",
                success: function (data) {
                    $(".dettaglio").html('');
                    $(".dettaglio").html(data);
                    UIRai.initHelpers('datetimepicker');
                    $("#_dataBrevetto").data("DateTimePicker").maxDate(moment());
                    $("#_tipo").val("3");
                    $(".radio_default").attr("checked", "checked");
                    StartTour("#tour-brevetto", "#modalCertificazioniInserimento .modal-content", false);
                },
                error: function (result) {
                    swal('Si è verificato un errore',
                        'Errore nel caricamento della pagina',
                        'error');
                }
            });
            break;
        case 'alb':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioAlbo",
                type: "POST",
                success: function (data) {
                    $(".dettaglio").html('');
                    $(".dettaglio").html(data);
                    UIRai.initHelpers('datetimepicker');
                    $("#_dataalbo").data("DateTimePicker").maxDate(moment());
                    $("#_tipo").val("4");
                    $(".js-select2").select2({
                        placeholder: "Seleziona dalla lista"
                    });
                    StartTour("#tour-albo", "#modalCertificazioniInserimento .modal-content", false);
                },
                error: function (result) {
                    swal('Si è verificato un errore',
                        'Errore nel caricamento della pagina',
                        'error');
                }
            });
            break;
        case 'pre':
            $(".dettaglio").html(' ');
            $.ajax({
                url: "/CV_Online/Create_DettaglioPremio",
                type: "POST",
                success: function (data) {
                    $(".dettaglio").html('');
                    $(".dettaglio").html(data);
                    UIRai.initHelpers('datetimepicker');
                    $('#_datainizio').data("DateTimePicker").maxDate(moment());
                    //$('#_datafine').data("DateTimePicker").maxDate(moment());
                    $('#_datafine').data("DateTimePicker").maxDate("31/12/9999");
                    $("#_tipo").val("5");
                },
                error: function (result) {
                    swal('Si è verificato un errore',
                        'Errore nel caricamento della pagina',
                        'error');
                }
            });
            break;
    }
}

function setdatacert(event) {
    var mese = "";
    var anno = "";
    var data = "";
    var d = $(event.currentTarget.form).find("#_datainizio").val();
    var a = $(event.currentTarget.form).find("#_datafine").val();
    var dataini = moment(d);
    var datafin = moment(a);
    if (dataini != null && dataini.isValid()) {
        $(event.currentTarget.form).find('#_datafine').data("DateTimePicker").minDate(d);
        if (moment(d, "MMMM YYYY").isAfter(moment(a, "MMMM YYYY"))) {
            $(event.currentTarget.form).find("#_datafine").val("");
        }
    }
    if ($(event.currentTarget).attr('id') == "_datainizio") {
        mese = "#_meseIni";
        anno = "#_annoIni";
        data = moment(d, "MMMM YYYY");
        inpreq(event);
    }
    else {
        mese = "#_meseFin";
        anno = "#_annoFin";
        data = moment(a, "MMMM YYYY");
        inpreq(event);
    }

    if (data == null || !data.isValid()) {
        $(mese).val(null);
        $(anno).val(null);
    }
    else {
        $(mese).val((data.month() + 1));
        $(anno).val(data.year());
    }
}

function ModificaExperiencesRai(elem, id_modal) {
    RaiOpenAsyncModal(id_modal, '/CV_Online/Create_ModificaDettaglioRai', elem, function () {
        UIRai.initHelpers('datetimepicker');
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
        StilizzaSelectCv('exp', '#frkModificaExperiences ');
    },"POST");
}

function ModificaExperiencesExtraRai(elem, id_modal) {
    RaiOpenAsyncModal(id_modal, '/CV_Online/Create_ModificaDettaglioExtraRai', elem, function () {
        UIRai.initHelpers('datetimepicker');
        var tmpDate;
        var now = new Date();
        now.setDate(now.getDate() - 1);
        tmpDate = now.getDate().toString() + "/" + (now.getMonth() + 1).toString() + "/" + now.getFullYear().toString();
        //tmpDate = now.toLocaleDateString();
        //$(".form-modificaExperiences #_dataFine").data("DateTimePicker").maxDate(tmpDate);
        $(".form-modificaExperiences #_annoFine").data("DateTimePicker").maxDate(now.getFullYear());
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
        StilizzaSelectCv('exp', '#frkModificaExperiences');
    }, "POST");
}

function CopiaExperiencesRai(elem, id_modal) {
    RaiOpenAsyncModal(id_modal, '/CV_Online/Create_CopiaDettaglioRai', elem, function () {
        UIRai.initHelpers('datetimepicker');
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
        StilizzaSelectCv('exp', '#frkModificaExperiences ');
    }, "POST");
}

function CopiaExperiencesExtraRai(elem, id_modal) {
    RaiOpenAsyncModal(id_modal, '/CV_Online/Create_CopiaDettaglioExtraRai', elem, function () {
        UIRai.initHelpers('datetimepicker');
        var tmpDate;
        var now = new Date();
        now.setDate(now.getDate() - 1);
        tmpDate = now.getDate().toString() + "/" + (now.getMonth() + 1).toString() + "/" + now.getFullYear().toString();
        //tmpDate = now.toLocaleDateString();
        //$(".form-modificaExperiences #_dataFine").data("DateTimePicker").maxDate(tmpDate);
        $(".form-modificaExperiences #_annoFine").data("DateTimePicker").maxDate(now.getFullYear());
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista",
        });
        StilizzaSelectCv('exp', '#frkModificaExperiences');
    }, "POST");
}

function submit_modificaExperiences(button, id_form, savedb )
{
    event.preventDefault();
    $(button).addClass('disable');
    $('#' + savedb).show();

    //if ($("#form-modificaExperiences input[name='_flagEspRai']").val() == "1") {
    //    if (($(".form-modificaExperiences #_dataFine").val() == "") || ($(".form-insertexperiences #_dataFine").val() == undefined)) {
    //        $(".form-modificaExperiences #_dataFine").val("31/12/9999");
    //        $(".form-modificaExperiences #_dataFine").text("31/12/9999");
    //    }
    //}
    //else {

    //}

    if ( ( $( ".form-modificaExperiences #_dataFine" ).val() == "" ) || ( $( ".form-insertexperiences #_dataFine" ).val() == undefined ) )
    {
        $( ".form-modificaExperiences #_dataFine" ).val( "31/12/9999" );
        $( ".form-modificaExperiences #_dataFine" ).text( "31/12/9999" );
    }

    var validator = $( id_form ).validate();

    if ( !$( id_form ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass('disable');
        $('#' + savedb).hide();
        return false;
    }

    var obj = $(id_form).serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/ModificaExperiences",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#"+savedb).fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    ShowAlertInsert('update');
                    CallActionAjax('experiences', 'box-experiences');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    break;
                case "error":
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    break;
                default:
                    swal("La modifica non è andata a buon fine. Errore: ", data, "error");
                    break;
            }
        },
        error: function (result) {
            swal("Modifica non riuscita - Failed", result, "error");
        }
    });
}

function ConfermaCancellazione_Experencies(matricola, prog) {
    swal({
        title: "Sicuro di voler cancellare l'elemento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        //window.location.href = "/CV_Online/DeleteExperiences?matricola=" + matricola + "&prog=" + prog;
        $.ajax({
            url: "/CV_Online/DeleteExperiences?matricola=" + matricola + "&prog=" + prog,
            type: "GET",
            cache: false,
            success: function () {
                CallActionAjax('experiences', 'box-experiences');
            }
        });
    });
}

function ResetAggiungiExperencies() {
    PulisciPopup("w3");
    $(".tab-principale").trigger("click");
    $(".anno-inizio").text('');
    $(".anno-inizio").val('');
    $(".anno-fine").text('');
    $(".anno-fine").val('');
}

function SetIntervalDateInizio(anno_inizio, anno_fine, id_elem) {
    $("#" + id_elem).data("DateTimePicker").minDate('01/01/' + anno_inizio);
    $("#" + id_elem).data("DateTimePicker").maxDate('31/12' + anno_fine);
}
function SetIntervalDateFine(anno_inizio, id_elem) {
    $("#" + id_elem).data("DateTimePicker").minDate('01/01/' + anno_inizio);
}

function CheckDataFine(id_dataInizio, id_elem) {
    if (($("#" + id_dataInizio).val() != "") || ($("#" + id_dataInizio).val() != null)) {
        $("#" + id_elem).data("DateTimePicker").minDate($("#" + id_dataInizio).val());
    }
}

function submit_CompetenzeRai(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbcomprai').show();

    var obj = $("#frm-insertCompetenzeRai").serialize();

    $.ajax({
        url: "/CV_Online/EditCompetenzeRai",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbcomprai").fadeOut("slow", function () {
            });
            ShowAlertInsert('');
            CallActionAjax('CompetenzeRai', 'box-CompetenzeRAI');
        }
    });
}

function submit_knowledges(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbknow').show();
    
    var obj = $("#frm-insertConoscenzeInformatiche").serialize();
    
    $.ajax({
        url: "/CV_Online/EditConoscenzeInformatiche",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbknow").fadeOut("slow", function () {
            });
            ShowAlertInsert('');
            CallActionAjax('knowledges', 'box-knowledges');
        }
    });
}

function submit_AltreInfo(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbaltre').show();

    var obj = $("#frm-InsertAltreInfo").serialize();

    $.ajax({
        url: "/CV_Online/InsertAltreInfo",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbaltre").fadeOut("slow", function () {
            });
            switch (data)
            {   
                case "Ok":
                    CallActionAjax('AltreInfo', 'box-AltreInfo');
                    break; 
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal('Si è verificato un errore',
                        bo,
                        'error');
                    $(button).removeClass("disable");
                    $('#savedbaltre').hide();
                    break;
            }
        }
    });
}

function submit_CompetenzeSpecialistiche(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbcompspec').show();

    for (var i = 0; i <=3; i++)
    {
        if ($("input.magg-pres[name=flagPrinc\\[" + i + "\\]]:checked").length == 0)
        {
            var ck = 0;
            $("input.magg-pres[name=flagPrinc\\[" + i + "\\]]").each(function () {
                var tr = $(this).closest("tr");
                ck += tr.find("input:radio:checked").length;
            });
          
            if (ck >0)
            {
                var sezName = $("input.magg-pres[name=flagPrinc\\[" + i + "\\]]:first").closest("tbody").prev("thead").find("th")[0].innerText;
                swal('Sezione ' + sezName, "Competenza maggiormente presidiata non segnalata", 'error');
                $(button).removeClass("disable");
                $('#savedbcompspec').hide();
                return;
            }
        }
    }
    
    var listCompSel = $('#frm-insertCompetenzeSpecialistiche input[type="checkbox"][class^="select-check"]:checked');
    if (listCompSel.length == 0) {
        swal('Attenzione', 'Nessuna competenza selezionata', 'error');
        $(button).removeClass("disable");
        $('#savedbcompspec').hide();
        return;
    }
    else {
        var strError = "";
        for (var i = 0; i < listCompSel.length; i++) {
            var radioName = $(listCompSel[i]).attr('name').replace('_isSelected', '_codConProfLiv');
            if ($('#frm-insertCompetenzeSpecialistiche input[type="radio"][name="' + radioName + '"]:radio:checked').length == 0) {
                if (strError != '')
                    strError += '<br/>';
                strError += $(listCompSel[i]).next('label').text() + ': livello non selezionato';
            }
        }

        if (strError != '') {
            swal({
                title: 'Attenzione',
                html: strError,
                type: 'error'
            });
            $(button).removeClass("disable");
            $('#savedbcompspec').hide();
            return;
        }
    }

    var obj = $("#frm-insertCompetenzeSpecialistiche").serialize();

    $.ajax({
        url: "/CV_Online/EditCompetenzeSpecialistiche",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbcompspec").fadeOut("slow", function () {
            });
            CallActionAjax('CompetenzeSpecialistiche', 'box-CompetenzeSpecialistiche');
        }
    });
}

function submit_certificazioni(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbcert').show();
    var d = $(event.currentTarget.form).find("#_datainizio").val();
    var a = $(event.currentTarget.form).find("#_datafine").val();
    var dataini = moment(d);
    var datafin = moment(a);
    if (dataini == null || !dataini.isValid()) {
        $(event.currentTarget.form).find("#_meseIni").val(null);
        $(event.currentTarget.form).find("#_annoIni").val(null);
    }
    else {
        $(event.currentTarget.form).find("#_meseIni").val(moment(d, "MMMM YYYY").month() + 1);
        $(event.currentTarget.form).find("#_annoIni").val(moment(d, "MMMM YYYY").year());
    }
    if (datafin == null || !datafin.isValid()) {
        $(event.currentTarget.form).find("#_meseFin").val(null);
        $(event.currentTarget.form).find("#_annoFin").val(null);
    }
    else {
        $(event.currentTarget.form).find("#_meseFin").val(moment(a, "MMMM YYYY").month() + 1);
        $(event.currentTarget.form).find("#_annoFin").val(moment(a, "MMMM YYYY").year());
    }
    if (ControlliCertXTipo() == true) {
        $(button).removeClass("disable");
        $('#savedbcert').hide();
        return;
    }

    var validator = $( "#form-insertcertificazioni" ).validate();

    if ( !$( "#form-insertcertificazioni" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbcert').hide();
        return false;
    }

    var myTipo = $('#_tipo').val();
    if (typeof myTipo === "undefined" || myTipo == null || myTipo == "")
        $('#_tipo').val('1');

    var obj = $(".form-insertcertificazione").serialize();
    //var json_obj = JSON.stringify(obj)
    $.ajax({
        url: "/CV_Online/InsertCertificazioni",
        type: "POST",
        //dataType: "JSON",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbcert").fadeOut("slow", function () {});
            switch (data) {
                case "ok":
                    ShowAlertInsert('insert');
                    CallActionAjax('Certificazioni', 'box-Certificazioni', '#form-insertcertificazioni');
                    //window.location.href = "/CV_Online/Index/";
                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal('Si è verificato un errore',
                        bo,
                        'error');
                    $(button).removeClass("disable");
                    $('#savedbcert').hide();
                    break;
            }
        },
        error: function (result) {
            swal('Si è verificato un errore',
                result,
                'error');
            $(button).removeClass("disable");
            $('#savedbcert').hide();
        }
    });

}

function PuliziaCert() {
    //pulizia select
    $("#_codAlboProf").val(' ').trigger('change');
    //pulizia textbox
    $('[name=_nomeCertifica]').val('');
    $('[name=_autCertifica]').val('');
    $('[name=_numLicenza]').val('');
    $('[name=_annoIni]').val('');
    $('[name=_meseIni]').val('');
    $('[name=_meseFin]').val('');
    $('[name=_annoFin]').val('');
    $('#_datainizio').val('');
    $('#_datafine').val('');
    $('[name=_urlCertifica]').val('');
    $('[name=_titoloPubblica]').val('');
    $('[name=_editorePubblica]').val('');
    $('[name=_dataPubblica]').val('');
    $('[name=_urlPubblica]').val('');
    $('[name=_notePubblica]').val('');
    $('[name=_tipoBrevetto]').val('');
    $('[name=_uffBrevetto]').val('');
    $('[name=_numBrevetto]').val('');
    $('[name=_inventore]').val('');
    $('[name=_dataBrevetto]').val('');
    $('[name=_urlBrevetto]').val('');
    $('[name=_noteBrevetto]').val('');
    $('[name=_pressoAlboProf]').val('');
    $('[name=_dataAlboProf]').val('');
    $('[name=_noteAlboProf]').val('');
    $(".radio_default").attr("checked", "checked");
    ShowDettaglioCertificazione('att');
}

function ConfermaCancellazione_Certificazione(matricola, prog) {
    swal({
        title: "Sicuro di voler cancellare l'elemento selezionato?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: true,
        confirmButtonText: ' Si',
        cancelButtonText: ' No'
    }).then(function () {
        $.ajax({
            url: "/CV_Online/DeleteCertificazione?matricola=" + matricola + "&prog=" + prog,
            type: "POST",
            cache: false,
            success: function (data) {
                switch (data) {
                    case "ok":
                        ShowAlertInsert('delete');
                        CallActionAjax('Certificazioni', 'box-Certificazioni');
                        break;
                    default:
                        var bo = data.replace(/;/g, '\n');
                        swal('Si è verificato un errore',
                            bo,
                            'error');
                        break;
                }
            },
            error: function (result) {
                swal('Si è verificato un errore',
                    result,
                    'error');
            }
        });
    });
}

function ModificaCertificazione(elem) {
    var obj = elem;
    const id_Modale = "frkModificaCertificazioni";
    RaiOpenAsyncModal(id_Modale, '/CV_Online/Create_ModificaDettaglioCertificazione', elem, function () {
        if (elem._tipo == "1") {
            var d = $("#frkModificaCertificazioni #_datainizio").val();
            var a = $("#frkModificaCertificazioni #_datafine").val();
            var dataini = moment(d);
            var datafin = moment(a);
        }
        UIRai.initHelpers('datetimepicker');
        switch (elem._tipo) {
            case "2":
                    //$("#frkModificaCertificazioni #_dataPubblicazione").data("DateTimePicker").maxDate(moment());
                break;
            case "3", "5":
                $("#frkModificaCertificazioni #_dataBrevetto").data("DateTimePicker").maxDate(moment());
                break;
            case "4":
                $("#frkModificaCertificazioni #_dataalbo").data("DateTimePicker").maxDate(moment());
                break;
            default:
                if (dataini != null && dataini.isValid()) {
                    $('#frkModificaCertificazioni #_datafine').data("DateTimePicker").minDate(d);
                }
                $('#frkModificaCertificazioni #_datainizio').data("DateTimePicker").maxDate(moment());
                //$('#frkModificaCertificazioni #_datafine').data("DateTimePicker").maxDate(moment());
                $('#frkModificaCertificazioni #_datafine').data("DateTimePicker").maxDate("31/12/9999");
                break;
        }
        $(".js-select2").select2({
            placeholder: "Seleziona dalla lista"
        });
        StilizzaSelectCv('cert', '#frkModificaCertificazioni');
    },
        "POST");
}

function submit_modificaCertificazione(button, savedb) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#'+savedb).show();

    var d = $("#frkModificaCertificazioni #_datainizio").val();
    var a = $("#frkModificaCertificazioni #_datafine").val();
    var dataini = moment(d);
    var datafin = moment(a);
    if (dataini == null || !dataini.isValid()) {
        $("#frkModificaCertificazioni #_meseIni").val(null);
        $("#frkModificaCertificazioni #_annoIni").val(null);
    }
    else {
        $("#frkModificaCertificazioni #_meseIni").val(moment(d, "MMMM YYYY").month() + 1);
        $("#frkModificaCertificazioni #_annoIni").val(moment(d, "MMMM YYYY").year());
    }
    if (datafin == null || !datafin.isValid()) {
        $("#frkModificaCertificazioni #_meseFin").val(null);
        $("#frkModificaCertificazioni #_annoFin").val(null);
    }
    else {
        $("#frkModificaCertificazioni #_meseFin").val(moment(a, "MMMM YYYY").month() + 1);
        $("#frkModificaCertificazioni #_annoFin").val(moment(a, "MMMM YYYY").year());
    }
    if (ControlliCertXTipo() == true) {
        return;
    }

    var validator = $( "#form-modificaCertificazioni" ).validate();

    if ( !$( "#form-modificaCertificazioni" ).valid() )
    {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#' + savedb).hide();
        return false;
    }

    var obj = $(".form-modificaCertificazioni").serialize();
    $.ajax({
        url: "/CV_Online/ModificaCertificazioni",
        type: "POST",
        data: obj,
        cache: false,
        success: function (data) {
            $('#'+savedb).fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    ShowAlertInsert('update');
                    CallActionAjax('Certificazioni', 'box-Certificazioni');
                    break;
                default:
                    var bo = data.replace(/;/g, '\n');
                    swal('Si è verificato un errore',
                        bo,
                        'error');
                    $(button).removeClass("disable");
                    $('#' + savedb).hide();
                    break;
            }
        },
        error: function (result) {
            swal('Si è verificato un errore',
                result,
                'error');
        }
    });
}

function inpreq(event) {
    var el = $(event.currentTarget);
    var name = el.attr('name');
    if (el.val() == "" || el.val() == null) {
        $(event.currentTarget.form).find("span[data-valmsg-for='" + name + "']").removeClass();
        $(event.currentTarget.form).find("span[data-valmsg-for='" + name + "']").addClass("field-validation-error");
    }
    else {
        $(event.currentTarget.form).find("span[data-valmsg-for='" + name + "']").addClass("hidden");
    }
    return;
}

function ControlliCertXTipo() {
    var par = false;
    // var input = $(event.currentTarget.form).find('input .required-min');
    var input = $(event.currentTarget.form.children).find('.required-min');
    input.each(function () {
        if (this.value == "" || this.value == null) {
            $(event.currentTarget.form).find("span[data-valmsg-for='" + this.name + "']").removeClass();
            $(event.currentTarget.form).find("span[data-valmsg-for='" + this.name + "']").addClass("field-validation-error");
            par = true;
        }
        else {
            $(event.currentTarget.form).find("span[data-valmsg-for='" + this.name + "']").addClass("hidden");
        }

    });
    var url = $(event.currentTarget.form).find('input.url');
    var err = par;
    if (url.length > 0) {
        url.each(function () {
            if (ctrUrl(this.id) == true)
            { par = true; }
        });
    }
    return par;
}

function ctrUrl(id) {
    par = false;
    var url = $(event.currentTarget.form).find('#' + id).val();
    var url_validate = /(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
    if (url == "" || url == null) {
        $(event.currentTarget.form).find("span[data-valmsg-for='" + id + "']").addClass("hidden");
        return par;
    }
    if (!url_validate.test(url)) {
        $(event.currentTarget.form).find("span[data-valmsg-for='" + id + "']").removeClass();
        $(event.currentTarget.form).find("span[data-valmsg-for='" + id + "']").addClass("field-validation-error");
        par = true;
    }
    else {
        $(event.currentTarget.form).find("span[data-valmsg-for='" + id + "']").addClass("hidden");
    }
    return par;
}

function submit_allegati(id_form) {
    var obj = $("#frmInsertAllegati").serialize();
    var name = $("#frmInsertAllegati #_name").val();
    var note = $("#frmInsertAllegati #_note").val();
    var formData = new FormData();
    var file = '';
    var dimensione = 0;
    if ($("#_fileUpload")[0].files.length > 0) {
        file = $("#_fileUpload")[0].files[0];
        dimensione = $("#_fileUpload")[0].files[0].size;
    }
    formData.append('_name', name);
    formData.append('_fileUpload', file);
    formData.append('_note', note);
    formData.append('_pathName', $("#frmInsertAllegati #_pathName").val());
    $.ajax({
        url: "/CV_Online/GetParameterAllegati",
        type: "POST",
        data: { dimensioneFile: dimensione },
        cache: false,
        success: function (data) {
            if (data != "") {
                swal({
                    title: data,
                    text: '',
                    type: 'error',
                    confirmButtonText: 'Ok',
                    confirmButtonClass: 'btn btn-lg btn-primary',
                    buttonsStyling: false
                });
            } else {
                $.ajax({
                    url: "/CV_Online/InsertAllegati",
                    type: "POST",
                    dataType: "JSON",
                    contentType: false,
                    processData: false,
                    data: formData,
                    cache: false,
                    success: function (data) {
                        CallActionAjax('Allegati', 'box-Allegati');
                    },
                    error: function () {
                        CallActionAjax('Allegati', 'box-Allegati');
                    }
                });

            }

        },
        error: function () {
            CallActionAjax('Allegati', 'box-Allegati');
        }
    });
    
}

function ConfermaCancellazione_Allegati(id) {
    event.preventDefault();
    swal({
        title: "Sicuro di voler cancellare l'elemento?",
        html: "",
        type: 'question',
        showCloseButton: true,
        showCancelButton: false,
        confirmButtonText: ' OK'
    }).then(function () {
        $.ajax({
            url: "/CV_Online/DeleteAllegati?id=" + id,
            type: "GET",
            cache: false,
            success: function () {
                CallActionAjax('Allegati', 'box-Allegati');
            }
        });
        //window.location.href = "/CV_Online/DeleteAllegati?matricola=" + matricola + "&id=" + id + "&pathName=" + pathName;
    });
}

function UploadActionAllegati(id_elem,des_elem) {
    swal({
        title: 'Il file è stato caricato correttamente',
        text: "Assegna un nome al documento caricato",
        html: '<p>Assegna un nome al documento caricato</p><input tabindex="0" type="text" name="tmp_name" placeholder="Inserisci il nome del file" id="tmp_name" class="form-control formElements"/><br/><p>Inserisci una breve descrizione</p><textarea tabindex="0" placeholder="Inserisci una breve descrizione" name="tmp_des" id="tmp_des" class="form-control formElements" style="height:100px !important"/>',
        //input: 'text',
        //type: 'warning',
        //showCancelButton: true,
        //confirmButtonColor: '#3085d6',
        //cancelButtonColor: '#d33',
        confirmButtonText: 'Salva',
        //cancelButtonText: 'No, cancel!',
        confirmButtonClass: 'btn btn-primary btn-lg',
        //cancelButtonClass: 'btn btn-danger',
        preConfirm: function(){
            return new Promise(function (resolve, reject) {
                if ($("#tmp_name").val() == "") {
                    reject("Inserisci il nome dell'allegato")
                }
                else if ($("#tmp_name").val().length > 100) {
                    reject("Il nome può essere lungo al massimo 100 caratteri")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        //var item = result;
        var item = $("#tmp_name").val();
        var desc = $("#tmp_des").val();
        $("#" + id_elem).val(item)
        $("#" + des_elem).val(desc)
        submit_allegati('frmInsertAllegati');
    })
}

function toggleAngle(id_elem) {
    if ($("#" + id_elem).hasClass('apri-panel')) {
        $("#" + id_elem).removeClass('apri-panel');
        $("#" + id_elem).addClass('chiudi-panel');
        $("#" + id_elem + " i").removeClass('fa-angle-down');
        $("#" + id_elem + " i").addClass('fa-angle-up');
        $("#" + id_elem).attr('aria-label', 'Mostra meno titoli');
    }
    else {
        $("#" + id_elem).removeClass('chiudi-panel');
        $("#" + id_elem).addClass('apri-panel');
        $("#" + id_elem + " i").removeClass('fa-angle-up');
        $("#" + id_elem + " i").addClass('fa-angle-down');
        $('#' + id_elem).attr('aria-label', 'Mostra altri titoli');
    }
}

function FormSubmit(id_form) {
    $("#" + id_form).trigger('submit');
}

function InizializzaTuttoDopoAjax(sect) {
    if (sect = "exp")
    {
        $("#modalExperiencesInserimnento input:text").val("");
        $("#modalExperiencesInserimnento select").each(function () { this.selectedIndex = 0 });
        $("#modalExperiencesInserimnento .select2-selection__rendered").text("");
    }
    $("#modalFormazioneInserimento input:text").val("");
    $("#modalFormazioneInserimento select").each(function () { this.selectedIndex = 0 });
    $("#modalFormazioneInserimento .select2-selection__rendered").text("");
   
    UIRai.initHelpers('datetimepicker');
    UIRai.initHelpers('select2');
    new JsSelect2Ext('.js-select2-custom').init();

    //$('[data-toggle="popover"]').popover();
    // Inizializza Popovers
    
    $('[data-toggle="popover"]').popover({
        template: '<div class="popover"><div class="arrow"></div><div><span style="float:left; width:100%; display:inline;" class="popover-title"></span><span class="customPopOverCloser" style="float:right; width:5%; margin-top: -30px; margin-left: -3px; dispay:inline; cursor:pointer;" aria-hidden="true" >x</span></div><div class="clearfix"></div><div class="popover-content"></div></div>',
        html: true
    });

    $(document).on("click", ".customPopOverCloser", function () {
        $(this).parents(".popover").popover('hide');
    });

    //wizard bootstrap
    $('#rootwizard').bootstrapWizard({
        onTabShow: function (tab, navigation, index) {
            var $total = navigation.find('li').length;
            var $current = index + 1;
            var $percent = ($current / $total) * 100;
            $('#rootwizard').find('.bar').css({ width: $percent + '%' });
        }
    });
    $('#rootwizard .finish').click(function () {
        //alert('Finished!, Starting over!');
        $('#rootwizard').find("a[href*='tab1']").trigger('click');
    });
    //fineee

    console.log("---->" + sect);
    if (sect != undefined)
    {
        StilizzaSelectCv(sect);
    }
    $("#form-insertareainteresse #_listaLocalita").select2({
        maximumSelectionLength: 3,
        placeholder: "Seleziona dalla lista",
        dropdownCssClass : 'bigdrop'
    });

    $("#flagChangedInput").val('0');
    AggiornaUltimaModifica_Percentuale();
}

// FREAK - variabili globali *******
// Source: http://stackoverflow.com/questions/497790
var dates = {
    convert: function (d) {
        // Converts the date in d to a date-object. The input can be:
        //   a date object: returned without modification
        //  an array      : Interpreted as [year,month,day]. NOTE: month is 0-11.
        //   a number     : Interpreted as number of milliseconds
        //                  since 1 Jan 1970 (a timestamp) 
        //   a string     : Any format supported by the javascript engine, like
        //                  "YYYY/MM/DD", "MM/DD/YYYY", "Jan 31 2009" etc.
        //  an object     : Interpreted as an object with year, month and date
        //                  attributes.  **NOTE** month is 0-11.
        return (
            d.constructor === Date ? d :
            d.constructor === Array ? new Date(d[0], d[1], d[2]) :
            d.constructor === Number ? new Date(d) :
            d.constructor === String ? new Date(d) :
            typeof d === "object" ? new Date(d.year, d.month, d.date) :
            NaN
        );
    },
    compare: function (a, b) {
        // Compare two dates (could be of any type supported by the convert
        // function above) and returns:
        //  -1 : if a < b
        //   0 : if a = b
        //   1 : if a > b
        // NaN : if a or b is an illegal date
        // NOTE: The code inside isFinite does an assignment (=).
        return (
            isFinite(a = this.convert(a).valueOf()) &&
            isFinite(b = this.convert(b).valueOf()) ?
            (a > b) - (a < b) :
            NaN
        );
    },
    inRange: function (d, start, end) {
        // Checks if date in d is between dates in start and end.
        // Returns a boolean or NaN:
        //    true  : if d is between start and end (inclusive)
        //    false : if d is before start or after end
        //    NaN   : if one or more of the dates is illegal.
        // NOTE: The code inside isFinite does an assignment (=).
        return (
             isFinite(d = this.convert(d).valueOf()) &&
             isFinite(start = this.convert(start).valueOf()) &&
             isFinite(end = this.convert(end).valueOf()) ?
             start <= d && d <= end :
             NaN
         );
    }
}

function AggiornaUltimaModifica_Percentuale() {
    $.ajax({
        url: '/CV_Online/BoxAnagrafico',
        dataType: "html",
        success: function (data) {
            $("#box-boxAnagrafico").html(' ');
            $("#box-boxAnagrafico").html(data);
        }
    });
}


function CallActionAjax(action, id_elem, idForm) {
    $.ajax({
        url: '/CV_Online/'+action,
        dataType: "html",
        cache:false,
        success: function ( data )
        {
            $("#" + id_elem).html(' ');
            $("#" + id_elem).html(data);
            if ((idForm != null) || (idForm != "")) {
                PulisciFormDefault(idForm);
            }
            InizializzaTuttoDopoAjax();
        }
    });
}

function SetSelect2FocusById(id_elem, id_form) {
    //var el = document.getElementById(id_form);
    //el.addEventListener('mouseover', function () {
    //    $("#" + id_elem).select2('open');
    //});
}

function AttivaTooltip(id_elem) {
    $("#" + id_elem).tooltip();
}

function ShowAlertInsert(type_text) {
    var text;
    switch (type_text) {
        case 'insert':
            text = "L'inserimento è andato a buon fine.";
            break;
        case 'update':
            text = "La modifica è stata effettuata.";
            break;
        case 'delete':
            text = "Elemento eliminato con successo.";
            break;
        default:
            text = "Operazione conclusa con successo";
            break;
    }
    swal({
        title: text,
        text: '',
        type: 'success',
        confirmButtonText: 'Ok',
        confirmButtonClass: 'btn btn-lg btn-primary',
        buttonsStyling: false
    });
}

//freak - funzione utility
function titleCase(str) {
    var newstr = str.split(" ");
    for (i = 0; i < newstr.length; i++) {
        if (newstr[i] == "") continue;
        var copy = newstr[i].substring(1).toLowerCase();
        newstr[i] = newstr[i][0].toUpperCase() + copy;
    }
    newstr = newstr.join(" ");
    return newstr;
}

function PulisciFormDefault(id_form) {
    $(id_form + " input").each(function () {
        $(this).val('');
    });
    $(id_form + " select").each(function () {
        $(this).val('').trigger('change');
    });

}

function submit_autopresentazione(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedbautopres').show();

    var validator = $("#form-editAutopresentazione").validate();

    if (!$("#form-editAutopresentazione").valid()) {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedbautopres').hide();
        return false;
    }

    var obj = $("#form-editAutopresentazione").serialize();
    var name = $("#form-editAutopresentazione #_autoName").val();
    var note = $("#form-editAutopresentazione #_note").val();
    var formData = new FormData();
    var file = '';
    var dimensione = 0;
    if ($("#inputAttachFile")[0].files.length > 0) {
        file = $("#inputAttachFile")[0].files[0];
        dimensione = $("#inputAttachFile")[0].files[0].size;
    }
    formData.append('_id', $("#form-editAutopresentazione #_id").val());
    formData.append('_name', name);
    formData.append('inputAttachFile', file);
    formData.append('_note', note);
    formData.append('_pathName', $("#form-editAutopresentazione #_autoPathName").val());
    
    $.ajax({
        url: "/CV_Online/InsertAutopresentazione",
        type: "POST",
        data: formData,
        dataType: "html",
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            $("#savedbautopres").fadeOut("slow", function () {
            });
            switch (data) {
                case "ok":
                    CallActionAjax('AutoPresentazione', 'box-AutoPres', '#form-editAutopresentazione');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbautopres').hide();
                    break;
                case "error":
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbautopres').hide();
                    break;
                default:
                    swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedbautopres').hide();
                    break;
            }
        },
        error: function (result) {
            swal("Inserimento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedbautopres').hide();
        }
    });
}

function submit_allegato(button) {
    event.preventDefault();
    $(button).addClass("disable");
    $('#savedballegato').show();

    var validator = $("#form-editAllegato").validate();

    if (!$("#form-editAllegato").valid()) {
        validator.focusInvalid();
        $(button).removeClass("disable");
        $('#savedballegato').hide();
        return false;
    }

    var obj = $("#form-editAllegato").serialize();
    var name = $("#form-editAllegato #_name").val();
    var note = $("#form-editAllegato #_note").val();
    var formData = new FormData();
    var file = '';
    var dimensione = 0;
    if ($("#inputAttachFileAll")[0].files.length > 0) {
        file = $("#inputAttachFileAll")[0].files[0];
        dimensione = $("#inputAttachFileAll")[0].files[0].size;
    }
    formData.append('_id', $("#form-editAllegato #_id").val());
    formData.append('_name', name);
    formData.append('_fileUpload', file);
    formData.append('_note', note);
    formData.append('_pathName', $("#form-editAllegato #_pathName").val());

    $.ajax({
        url: "/CV_Online/UpdateAllegato",
        type: "POST",
        data: formData,
        dataType: "html",
        contentType: false,
        processData: false,
        cache: false,
        success: function (data) {
            $("#savedballegato").fadeOut("slow", function () {
            });
            switch (data) {
                case "Ok":
                    CallActionAjax('Allegati', 'box-Allegati', '#form-editAllegato');
                    //window.location.href = "/CV_Online/";
                    break;
                case "invalid":
                    //errore di inserimento
                    //swal("L'inserimento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedballegato').hide();
                    break;
                case "error":
                    swal("L'aggiornamento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedballegato').hide();
                    break;
                default:
                    swal("L'aggiornamento non è andato a buon fine. Errore: ", data, "error");
                    $(button).removeClass("disable");
                    $('#savedballegato').hide();
                    break;
            }
        },
        error: function (result) {
            swal("aggiornamento non riuscito - Failed", result, "error");
            $(button).removeClass("disable");
            $('#savedballegato').hide();
        }
    });
}

function AutopresSelezionata() {
    var obj = $("#inputAttachFile")[0].files[0];

    swal({
        title: 'Il file è stato caricato correttamente',
        text: "Assegna un nome al documento caricato",
        html: '<p>Assegna un nome al documento caricato</p><input tabindex="0" type="text" name="tmp_name"  id="tmp_name" class="form-control formElements"/>',
        confirmButtonText: 'Salva',
        confirmButtonClass: 'btn btn-primary btn-lg',
        preConfirm: function () {
            return new Promise(function (resolve, reject) {
                if ($("#tmp_name").val() == "") {
                    reject("Inserisci il nome dell'allegato")
                }
                else if ($("#tmp_name").val().length > 100) {
                    reject("Il nome può essere lungo al massimo 100 caratteri")
                }
                else {
                    resolve()
                }
            })
        },
        buttonsStyling: false
    }).then(function (result) {
        //var item = result;
        var item = $("#tmp_name").val();
        
        $('#_autoPathName').val($('#inputAttachFile')[0].files[0].name);


        $('#_autoName').val(item);
        $('#iAddFile').hide();
        $('#lblAddFile').hide();

        $('#iAddedFile').show();
        $('#hrefAddedFile').show();
        $('#hrefCanc').show();

        $('#spanAddFile').removeClass('cursor-pointer');

        $('#boxFile').css("border-style", "solid");
    })

}

function AllegatoSelezionato() {
    var obj = $("#form-editAllegato #inputAttachFileAll")[0].files[0];
    $('#form-editAllegato #_pathName').val($(obj).name);
    $('#form-editAllegato #iAddFile').hide();
    $('#form-editAllegato #iAddFile').hide();
    $('#form-editAllegato #iAddedFile').show();
    $('#form-editAllegato #spanAddFile').removeClass('cursor-pointer');

    $('#form-editAllegato #hrefCanc').show();

    $('#form-editAllegato #boxFile').css("border-style", "solid");

    var icon = '';
    var content = '';

    if (obj.type.includes("audio"))
    {
        icon = 'icon icon-music-tone-alt';
        content = 'File audio';
    }
    else if (obj.type.includes("video"))
    {
        icon = 'icon icon-film';
        content = 'File Video';
    }
    else if (obj.type.includes('image'))
    {
        icon = 'icon icon-picture';
        content = 'File immagine';
    }
    else if (obj.type.includes('word'))
    {
        icon = 'icon icon-doc';
        content = 'File Word';
    }
    else if (obj.type.includes('excel')) {
        icon = 'icon icon-doc';
        content = 'File Excel';
    }
    else if (obj.type.includes('pdf')) {
        icon = 'icon icon-doc';
        content = 'File Pdf';
    }
    else {
        icon = 'icon icon-doc';
        content = 'File Generico (altro)';
    }

    $('#form-editAllegato #iAddedFile').attr('class', icon);
    $('#form-editAllegato #fmtFile').text(content);
}

function ConfermaCancellazioneAutopresentazione() {
    $('#inputAttachFile').val('');

    $('#_autoName').val('');
    $('#_autoPathName').val('');
    
    $('#iAddFile').show();
    $('#lblAddFile').show();
    $('#spanAddFile').addClass('cursor-pointer');

    $('#iAddedFile').hide();
    $('#hrefAddedFile').hide();
    $('#hrefCanc').hide();

    $('#boxFile').css("border-style", "dashed");
}

function ConfermaCancellazioneAllegato() {
    $('#form-editAllegato #inputAttachFileAll').val('');

    //$('#form-editAllegato #_name').val('');
    $('#form-editAllegato #_pathName').val('');

    $('#form-editAllegato #fmtFile').val('');

    $('#form-editAllegato #iAddFile').show();
    $('#form-editAllegato #lblAddFile').show();
    $('#form-editAllegato #spanAddFile').addClass('cursor-pointer');

    $('#form-editAllegato #iAddedFile').hide();
    $('#form-editAllegato #hrefAddedFile').hide();
    $('#form-editAllegato #hrefCanc').hide();

    $('#form-editAllegato #fmtFile').text('');

    $('#form-editAllegato #boxFile').css("border-style", "dashed");
}

function ModificaAutopresentazione(id_autopres) {
    RaiOpenAsyncModal("modal-Autopresentazione", "/CV_Online/ModificaAutopresentazione", { idAutopresentazione: id_autopres }, undefined, "POST");
}
function ModificaAllegato(id_allegato) {
    RaiOpenAsyncModal('modal-Allegato', "/CV_Online/ModificaAllegato", { idAllegato: id_allegato }, undefined, "POST")
}

function InitDate() {
    $('div[data-date]').hide();
    $('button[data-date]').removeClass('btn-primary');
}
function InitRoom() {
    $('div[data-room-date]').hide();
    $('button[data-room-date]').removeClass('btn-primary');
}
function InitTime() {
    $('button[data-time]').removeClass('btn-primary');
    $('button[data-time]').attr('data-selected', 'false');

    $('#btnAddpren').addClass('disable');
    $('#btnEditPren').addClass('disable');
}

function ShowSlotRoom(button) {
    InitDate();
    InitRoom();
    InitTime();
    
    $('div[data-date=' + $(button).attr('data-date') + ']').show();
    $(button).addClass('btn-primary');
}

function ShowSlotTime(button) {
    InitRoom();
    InitTime();

    $('div[data-room-date=' + $(button).attr('data-room-date') + '][data-room-id=' + $(button).attr('data-room-id') + ']').show();
    $(button).addClass('btn-primary');
}

function SelectSlotTime(button) {
    InitTime();

    $(button).addClass('btn-primary');
    $(button).attr('data-selected', 'true');

    $('#btnAddPren').removeClass('disable');
    $('#btnEditPren').removeClass('disable');
}

function ShowMappatura() {
    $("#modal-prenMapp").html(' ');
    $.ajax({
        url: "/CV_Online/ShowMappatura",
        type: "POST", 
        async: false,
        success: function (data) {
            $("#modal-prenMapp").html('');
            $("#modal-prenMapp").html(data);
            $("#modal-prenMapp").modal('show');
        },
        error: function (result) {
            swal("Avvenuto errore di modifica");
        }
    });
}

function ShowModificaPrenotazione() {
    $('#editSlot').show();
}

function ModificaPrenotazioneMappatura() {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi modificare la tua prenotazione?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla'
    }).then(function () {
        SubmitPrenotazione(true);
    })
}
function AggiungiPrenotazione() {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi effettuare la tua prenotazione?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla'
    }).then(function () {
        SubmitPrenotazione(false);
    })
}

function SubmitPrenotazione(sovrascrivi) {
    var idSlot = $('button[data-time][data-selected=true]').attr('data-id-slot');
    
    $.ajax({
        url: "/cv_online/SalvaPrenotazione",
        type: "POST",
        //dataType: "json",
        data: { idSlot: idSlot, sovrascrivi: sovrascrivi },
        success: function (data) {
            switch (data) {
                case "OK":
                    swal("Prenotazione effettuata con successo", "", "success");
                    UpdateMappatura();
                    break;
                default:
                    swal("Ops...", data, "error");
                    break;
            }
            
        }
    });
}

function EliminaPrenotazioneMapp(idprenotazione) {
    swal({
        title: 'Sei sicuro?',
        text: "Vuoi eliminare la tua prenotazione?",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì!',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: "/cv_online/CancellaPrenotazione",
            type: "POST",
            //dataType: "json",
            data: { idPrenotazione: idprenotazione },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal("Prenotazione cancellata con successo", "", "success");
                        UpdateMappatura();
                        break;
                    default:
                        swal("Ops...", data, "error");
                        break;
                }
            }
        });
    })
}

function UpdateMappatura() {
    $("#divMappatura").html(' ');
    $.ajax({
        url: "/CV_Online/ShowMappatura",
        type: "POST",
        async: false,
        success: function (data) {
            $("#divMappatura").replaceWith($(data).find("#divMappatura"));
        },
        error: function (result) {
            swal("Avvenuto errore di modifica");
        }
    }); 
}
// <reference path="../../Views/DocumentiDaFirmare/_Giornate.cshtml" />
var ClickDaSelezione = 0; // per il click pre-scelto da selezione popup iniziale
var Fixed_Data_Da = null;
var Fixed_Data_A = null;
var Disabled_Data_Da = false;
var Disabled_Data_A = false;
var PopupCallback = null;

PopupPassatoFuturo = null;

var PuoiTornareHome = false;

function ValidazioneErrore(error) {
    swal('Errore di validazione specifica', error, 'error');
}
function ValidazioneOK() {
    inviaForm($('#form-inserimento'));
}

function Handlers() {
    $(document.body).on("click", '#buttonSalvaEccezione', function (event) {
        event.preventDefault();
        var form = $("#form-eccezioni");

        var cbVal = CheckCustomValidation();
        var formVal = $("#form-eccezioni").valid();
        if (cbVal == false) {
            $("#modal-ecc .modal-content").scrollTop(10);
        }
        else if (formVal == false) {
            var fieldError = $(".field-validation-error:first").attr("data-valmsg-for");
            $("#modal-ecc .modal-content").scrollTop($("#" + fieldError).offset().top)
        }

       
        //if (cbVal == false || formVal == false) event.preventDefault();
        if (cbVal && formVal) {
            $("#buttonSalvaEccezione").attr("disabled", "disabled");
            $("#buttonAggiungiCampo").attr("disabled", "disabled");
            $.ajax({
                url: $(form).attr('action'),
                type: 'POST',
                data: $(form).serialize(),
                success: function (data) {
                    if (data.indexOf("OK") == 0) {
                        sendAllegato(data.replace('OK', ''));
                    }
                    else {
                        swal("Errore", data, "error")
                    }
                },
                error: function (a, b, c) {
                    alert(a + b + c);
                }
            });
        }
    });

    $(document.body).on("click", '.magg-pres', function (e) {
        if ($(e.target).closest("tr").find("input:radio:checked").length <= 1) {
            e.preventDefault();
            e.stopPropagation();
        }

    });

    $(document.body).on("click", '[data-toggle="tabs"] a, .js-tabs a', function (e) {
        e.preventDefault();
        jQuery(this).tab('show');
    });

    try {
        if (getIVEHostname() != undefined) {
            //$("#user").append("<i class='icons icon-arrow-down'></i>");
            //$("#userbox").append("<div class='dropdown-menu'><ul class='list-unstyled'><li class='divider'></li><li><a role='menuitem' tabindex='-1' href='https://www.intranetssl.rai.it/dana-na/auth/logout.cgi'></i> Esci</a></li></ul></div");

        }
    }
    catch (err) {
        console.log(err);
    }

    $(function () {
        $('input[type="time"][value="now"]').each(function () {
            var d = new Date(),
                h = d.getHours(),
                m = d.getMinutes();
            if (h < 10) h = '0' + h;
            if (m < 10) m = '0' + m;
            $(this).attr({
                'value': h + ':' + m
            });
        });
    });

    $(document.body).on('keyup input propertychange paste', '#inputPIN', function (e) {
        PinChanged();
    });
    $(document.body).on('keyup input propertychange paste', '#inputPassword', function (e) {
        PasswordChanged();
    });

    $(document).ajaxComplete(function (event, xhr, settings) {
        if (settings.dataType == "json") return;

        if (xhr.responseText.toLowerCase().indexOf('data-toggle="tabs"') >= 0) {
            jQuery('[data-toggle="tabs"] a, .js-tabs a').click(function (e) {
                e.preventDefault();
                jQuery(this).tab('show');
            });
        }


        if (xhr.responseText.toLowerCase().indexOf('data-toggle="tooltip"') >= 0) {
            $('[data-toggle="tooltip"]').tooltip();
        }

        if ($(xhr.responseText).find('[data-nano="true"]').length > 0) {
            $('[data-nano="true"]').nanoScroller();
        }
    });


    //refresh su tutti i div con attributo data-async :
    refreshDataAsync();

    $('#pdf-modal').on('hidden.bs.modal', function () { $("#pdfcontent").html(""); });

    jQuery("#data_da").on("dp.change", function (e) {
        OnChange_DataDa(e);
    });

    jQuery("#data_a").on("dp.change", function (e) {
        OnChange_DataA(e);
    });

    jQuery("#data-boss").on("dp.change", function (e) {
        OnChange_DataBoss(e);
    });

    //---------abilita bottone btnFilter, se viene scritto o solezionato un dato

    $( "#Visualizzato" ).on( "change keypress paste input", function ()
    {
        $( '#btnFilter' ).removeAttr( 'disabled' );
    } );

    $( "#sede,#stato,#eccezione,#nominativo,#datada,#dataal,#livelloDip" ).on( "change keypress paste input", function ()
    {
        if ( $( "#nominativo" ).val() == "" &&
            $( "#datada" ).val() == "" &&
            $( "#dataal" ).val() == "" &&
            $( "#sede option:selected" ).text() == "Sede" &&
            $( "#stato option:selected" ).text() == "Stato" &&
            $( "#eccezione option:selected" ).text() == "Eccezione" &&
            $( "#livelloDip option:selected").val()=="")
        {
            $( '#btnFilter' ).attr( 'disabled', 'disabled' );
        }
        else
        {
            $( '#btnFilter' ).removeAttr( 'disabled' );
        }
    } );

    $( "#titolo,#luogo,#datada,#dataal" ).on( "change keypress paste input", function ()
    {
        if ( $( "#titolo" ).val() == "" &&
            $( "#luogo" ).val() == "" &&
            $( "#datada" ).val() == "" &&
            $( "#dataal" ).val() == "" )
        {
            $('#btnFilter').attr('disabled', 'disabled');
        }
        else {
            $('#btnFilter').removeAttr('disabled');
        }
    });

    $('#form-evento').submit(function (e) {

        var form = $(this);
        e.preventDefault();

        InserisciEvento(form);

    } );

    $('#form-programma').submit(function (e) {

        var form = $(this);
        e.preventDefault();

        InserisciProgramma(form);

    });


    //----------------

    $('#form-inserimento').submit(function (e) {
        var form = $(this);

        e.preventDefault(); //prevent submit

        if (!$("#param-dalle").hasClass("hide") &&
            !$("#param-alle").hasClass("hide") &&
            !$("#param-quantita").hasClass("hide")
           ) {
            if ($("#param-dalle input").val().trim() != "" && $("#param-alle input").val().trim() != "") {
                var d1 = new Date(2018, 1, 1, $("#param-dalle input").val().split(':')[0], $("#param-dalle input").val().split(':')[1]);
                var d2 = new Date(2018, 1, 1, $("#param-alle input").val().split(':')[0], $("#param-alle input").val().split(':')[1]);
                var minutiDalleAlle = differenzaMinuti(d2, d1);
                var minutiQuantita = (Number($("#param-quantita input").val().split(':')[0]) * 60) + Number($("#param-quantita input").val().split(':')[1]);
                if (minutiDalleAlle != minutiQuantita) {
                    swal('Errore', "I dati inseriti per inizio/fine non sono coerenti con la quantità.", 'error');
                    return;
                }
            }
        }


        var ErroreValidazione = ValidazioneGenericaEccezioni();//da DB generico in param sistema
        if (ErroreValidazione != null) {
            swal('Errore', ErroreValidazione, 'error');
            return;
        }


        var ErroreValidazioneSpecifica = null;

        $.ajax({
            url: '/ajax/getJSvalidazione',
            type: "GET",
            dataType: "html",

            data: { cod: $("#select-eccezioni").find(":selected").val() },
            success: function (data) {

                if (data != null && data != "") {

                    var jscontainer = eval(data);
                    ValidazioneSpecifica();//da DB specifico per eccezione in oggetto
                    // ritorna in ValidazioneErrore() o ValidazioneOK()
                }
                else inviaForm(form);

            },
            error: function (result) {
                alert("Failed");
            }
        });
        return false;
    });



    $("body").on('mouseover', '.dayfirma', function (evt) {
        $(this).css("background-color", "#fafafa");
    });
    $("body").on('mouseout', '.dayfirma', function (evt) {
        $(this).css("background-color", "");
    });



    $("body").on('change', '.cb-ecc-auto', function (evt) {
        OnChange_CBautoEcc($(this));
    });
    $(".content").on('click', 'a[data-refreshurl],button[data-refreshurl]', function (evt) {
        OnClick_refreshurl($(this));
        evt.preventDefault();
        evt.stopPropagation();
        return false;
    });

    $(".content").on('click', 'a[data-filter],button[data-filter]', function (evt) {
        OnClick_filterurl($(this));
        evt.preventDefault();
        evt.stopPropagation();
        return false;
    });
    $(".content").on('click', 'a[data-reset],button[data-reset]', function (evt) {

        OnClick_reseturl($(this));
        evt.preventDefault();
        evt.stopPropagation();
        return false;
    });
    $(".content").on('click', '.button-approva', function (evt) {
        OnClick_buttonApprova($(this));
        evt.stopPropagation();
        evt.preventDefault();
        return false;
    });

    $(".content").on('click', '.button-rifiuta', function (evt) {
        OnClick_buttonRifiuta($(this));
        evt.stopPropagation();
        evt.preventDefault();
        return false;
    });

    $(".content").on('click', '.button-rifiuta-tutti', function (evt) {
        OnClick_buttonApprovaTutti($(this), false);
        evt.stopPropagation();
        evt.preventDefault();
        return false;
    });
    $(".content").on('click', '.button-approva-tutti', function (evt) {
        OnClick_buttonApprovaTutti($(this), true);
        evt.stopPropagation();
        evt.preventDefault();
        return false;
    });


    $(".content").on('click', '.button-rif-att', function (evt) {
        $("#btn-conf-tutti").show();
        $("#btn-chiudi").hide();
        $("#btn-ann").show();
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#valida-rifiuta-tutti").modal("show");
        $("#titolo1").text("Rifiuto richieste");
        $("#messaggio").text("Confermi di rifiutare le richieste selezionate ?");
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#testoprogress").show();
        $("#testoprogress").val("");
        $("#btn-chiudi").attr("onclick", '$("#refresh-attivita").click()');
        fillEcc($(this));
    });
    $(".content").on('click', '.button-appr-att', function (evt) {
     
        $("#btn-chiudi").hide();
        $("#btn-conf-tutti").show();
        $("#btn-ann").show();
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#valida-rifiuta-tutti").modal("show");
        $("#titolo1").text("Validazione richieste");
        $("#messaggio").text("Confermi di validare le richieste selezionate ?");
        $("#testoprogress").hide();
        $("#btn-chiudi").attr("onclick", '$("#refresh-attivita").click()');

        fillEcc($(this));
        //////////////////
    });

    $(".content").on('click', '.button-appr-att-all', function (evt) {
        
        evt.stopPropagation();
        $("#btn-chiudi").hide();
        $("#btn-conf-tutti").show();
        $("#btn-ann").show();
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#valida-rifiuta-tutti").modal("show");
        $("#titolo1").text("Validazione richieste");
        $("#messaggio").text("Confermi di validare le richieste selezionate ?");
        $("#testoprogress").hide();
        $("#btn-chiudi").attr("onclick", '$("#refresh-attivita").click()');

        fillEccAllAttiv($(this));
    });
    $(".content").on('click', '.button-rif-att-all', function (evt) {
        
        evt.stopPropagation();
        $("#btn-chiudi").hide();
        $("#btn-conf-tutti").show();
        $("#btn-ann").show();
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#valida-rifiuta-tutti").modal("show");
        $("#titolo1").text("Rifiuto richieste");
        $("#messaggio").text("Confermi di rifiutare le richieste selezionate ?");
        $("#testoprogress").show();
        $("#testoprogress").val("");
        $("#btn-chiudi").attr("onclick", '$("#refresh-attivita").click()');

        fillEccAllAttiv($(this));
    });

    function fillEccAllAttiv(button)
    {
        $("#rich-cont").html("");
        var trEcc = $(button).closest("tbody").next("tbody").find("table tr");
        var ecc_ric = "";
        for (var i = 0; i < trEcc.length; i++) {
            ecc_ric = ecc_ric + $(trEcc[i]).attr("data-ideccezione") + ",";
        }
        $.ajax({
            url: '/ajax/getEccezioniAttivita',
            type: "GET",
            dataType: "html",
            data: {
                idecc: ecc_ric,
            },
            success: function (data) {
                $("#rich-cont").html(data);
            },
            error: function (a, b, c) {
                alert("Failed info " + a + b + c);
            }
        });
    }

    function fillEcc(button)
    {
        $("#rich-cont").html("");
        var totale_ecc = $(button).attr("data-tot");
        var data_ecc = $(button).attr("data-date");
        var trEcc = $(button).closest(".col-sm-4").next(".col-sm-8").find(".table tr");
        var ecc_ric = "";
        for (var i = 0; i < trEcc.length; i++) {
            ecc_ric = ecc_ric + $(trEcc[i]).attr("data-ideccezione") + ",";
        }
        //////////////////
        $.ajax({
            url: '/ajax/getEccezioniAttivita',
            type: "GET",
            dataType: "html",
            data: {
                idecc: ecc_ric,
            },
            success: function (data) {
                $("#rich-cont").html(data);
            },
            error: function (a, b, c) {
                alert("Failed info " + a + b + c);
            }
        });
    }

    

    $(".content").on('click', '.trtodelete', function (event) {
        var b = event.target;
        OnClick_TRtoDelete(this, b);
    });
    $(document.body).on('keyup', '.ep-obb', function (e) {
        OnChange_notainserimento($(this));
    });
    $(document.body).on('change', '.ep-obb', function (e) {
        OnChange_notainserimento($(this));
    });

    $(document.body).on('keyup', '#DettaglioTextarea', function (e) {
        OnChange_dettaglioTextArea($(this));
    });
    $(document.body).on('keyup', '.nota-su-auto', function (e) {
        OnChange_notasuauto($(this));
    });

    $('#motivo-digitato').bind('input propertychange', function () {
        OnChange_motivodigitato($(this));
    });
    $('#testoprogress').bind('input propertychange', function () {
        if ($("#testoprogress").val().length > 4) {
            $("#btn-conf-tutti").removeAttr("disabled");
        }
        else {
            $("#btn-conf-tutti").attr("disabled", "disabled");
        }
    });


    $('#NotaInserimento').bind('input propertychange', function () {
        OnChange_notainserimento($(this));
    });


    $('#modal-popin').on('hide.bs.modal', function () { if (PuoiTornareHome) { location.href = '/'; } });

    $('#giornata-modal').on('hidden.bs.modal', function () {
        $("#allcontent").empty();
    });

    $('#giornata-modal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var recipient = button.data('day')
        var TRparent = $(button).closest("tr");
        var IdEccezioneRichiesta = $(TRparent).data("ideccezione");

        UIRai.loader('show');
        $.get("/Home/dettagliogiornata?data=" + recipient + "&ideccezionerichiesta=" + IdEccezioneRichiesta, function (data) {

        }).done(function (result) {
            $('#giornata-modal').html(result);
            UIRai.loader('hide');

            $.ajax({
                url: '/ajax/getInfoGiornataAjaxView',
                type: "GET",
                dataType: "html",
                data: {
                    date: recipient,
                    ideccezionerichiesta: IdEccezioneRichiesta
                },
                success: function (data) {
                    var t = $("#InfoGiornataCorrenteContainer");
                    $(t).html(data);
                    var parent = $("#InfoGiornataCorrenteContainerParent");
                    $(parent).removeClass('hide');
                    if (recipient != "01/01/1900" &&
                        !IsDayClosed(recipient) &&
                        !IsDayLockedOrario()) {
                        GetProposteAuto(recipient);
                    }
                },
                error: function (a, b, c) {
                    alert("Failed info " + a + b + c);
                }
            });

            $.ajax({
                url: '/ajax/getSegnalazioniAjaxView',
                type: "GET",
                dataType: "html",
                data: {
                    date: recipient,
                    matricola: '',
                    abilitaApprovazione: '1',
                    ideccezionerichiesta: IdEccezioneRichiesta,
                    hideCurrentDataRow: true
                },
                success: function (data) {
                    var t = $("#InfoGiornataCorrenteContainerApprovazione");
                    $(t).html(data);
                    $(t).data('dataVisualizzata', recipient);
                    $(t).removeClass('hide');
                }
            });
        }).fail(function () {
            UIRai.loader('hide');
        });
        console.log(recipient);
    });

    $('#dettagliodip-modal').on('hidden.bs.modal', function () {
        $("#allcontent").empty();
    })

    $('#dettagliodip-modal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget)
        var recipient = button.data('day')
        var TRparent = $(button).closest("tr");
        var IdEccezioneRichiesta = $(TRparent).data("ideccezione");

        UIRai.loader('show');
        $.get("/Approvazione/dettaglio/dettagliodipendente?data=" + recipient + "&ideccezionerichiesta=" + IdEccezioneRichiesta, function (data) {

        }).done(function (result) {
            //console.log(result);
            $('#dettagliodip-modal').html(result);

            // $('#paginator').datepaginator();
            UIRai.loader('hide');

        }).fail(function () {
            UIRai.loader('hide');
        });
        console.log(recipient);
    });

    $('#div-notifiche').on('show.bs.dropdown', function (event) {
        //alert("ok");
    });
    $('#ripianifica').on('shown.bs.modal', function (event) {
         
       // var date = new Date();
       // var formattedDate = moment(date).format('DD/MM/YYYY');
       // $("#data-ripian").data("DateTimePicker").minDate(formattedDate);
        $("#data-ripian").data("DateTimePicker").maxDate("31/12/" + moment(new Date()).format("YYYY"));
    });

    $('#modal-popup-iniziale').on('shown.bs.modal', function (event) {
        if ($(this).find("ul.nav li").length == 1) {
            $(this).find("ul.nav li a").click();
        }
    });

    $("#modal-form-primario").on('shown.bs.modal', function (event) {

    });

    $("#modal-form-secondario").on('shown.bs.modal', function (event) {
        //$("#form-secondario-content").load("/formadmin/getFormSecondario?id=" + $("#id-form-secondario").val());
    });

    $("#modal-form-question").on('shown.bs.modal', function (event) {

    });

    $('#firma-panel').on('shown.bs.modal', function (event) {
        var a = getCookie();
        $("#inputPassword").val(getCookie());
        if (a != "" && a!=null)
        {
            $("#rememberme").prop("checked", true);
        }
    });

    $('#modal-popin').on('shown.bs.modal', function (event) {
        $("#altregiornate").text("");
        $("#boxleft").html($("#boxleft-skeleton").html());
        PuoiTornareHome = (location.search.toLowerCase().indexOf("idscelta") > -1);
        $("#data_da").val("");
        $("#data_a").val("");
        $("#data_a").attr("disabled", "disabled");
        $("#dateok").addClass("disable");
        if (Fixed_Data_Da != null) {
            //se futuro:
            if (IsDateGreaterThanToday(Fixed_Data_Da)) {
                $("#data_da").val(Fixed_Data_Da);
                if (Fixed_Data_A != null)
                    $("#data_a").val(Fixed_Data_A);
                else
                    $("#data_a").val(Fixed_Data_Da);

                $("#dateok").click();
            }
            else {
                //se passato
                $("#data_da").val("01/01/1900");
                $("#data_da").change();
                $("#data_da").val(Fixed_Data_Da);
                $("#data_da").change();
            }
        }

        if (PopupCallback != null) {
            $('#modal-popin').on('hidden.bs.modal', PopupCallback);
        }
        else
            $('#modal-popin').off('hidden.bs.modal');

        jQuery('#data_da').data("DateTimePicker").minDate(moment(date).format('01/01/1900'));
        jQuery('#data_da').data("DateTimePicker").maxDate(moment(date).format('01/01/2900'));
        jQuery('#data_a').data("DateTimePicker").minDate(moment(date).format('01/01/1900'));
        jQuery('#data_a').data("DateTimePicker").maxDate(moment(date).format('01/01/2900'));
        var date = new Date();
        var formattedDate = moment(date).format('DD/MM/YYYY');
        if (PopupPassatoFuturo == "f") jQuery('#data_da').data("DateTimePicker").minDate(formattedDate);
        else if (PopupPassatoFuturo == "p") jQuery('#data_da').data("DateTimePicker").maxDate(formattedDate);

        if (Disabled_Data_Da)
            $("#data_da").attr("disabled", "disabled");
        else
            $("#data_da").removeAttr("disabled");
        if (Disabled_Data_A)
            $("#data_a").attr("disabled", "disabled");
        else
            $("#data_a").removeAttr("disabled");

        
       // $('.formato-ora').mask('99:99');

        //$('.formato-ora').inputmask({
        //    mask: '99:99',
        //    placeholder:' '
        //});
        $(".formato-ora").val("00:00");
        $(".formato-ora").timepicker({ showMeridian: false, minuteStep: 1,defaultTime:"00:00",maxHours:30 })

        reinitWizard();

        $("#primoTab").removeClass("active");
        $("#primoTab").removeClass("completed");
        $("#secondoTab").removeClass("completed");
        if (Disabled_Data_A && Disabled_Data_Da) {
            $("#titolodate").text("Giustifica la giornata");
            if (AssenzeIngiustificateNow == 1) $("#altregiornate").text("Non hai altre assenze ingiustificate");
            if (AssenzeIngiustificateNow == 2) $("#altregiornate").text("Hai ancora 1 assenza ingiustificata");
            if (AssenzeIngiustificateNow > 2) {
                var rim = AssenzeIngiustificateNow - 1;
                $("#altregiornate").text("Hai ancora " + rim.toString() + " assenze ingiustificate");
            }
        }
        else {
            $("#titolodate").text("Seleziona le date");
        }
        if ($("#data_a").val() == "")
            $("#data_a").attr("disabled", "disabled");
    });

    $(".wizard-next").click(function () {
        OnClick_buttonNextWizard();
    });

    $(".wizard-prev").click(function () {
        OnClick_buttonPrevWizard();
    });

    $("#data_a").on('change keypress paste input', function (e) {
        if ($("#data_a").val() == "")
            $("#dateok").addClass("disable");
        else
            $("#dateok").removeClass("disable");
    });

    $('.enable-next-page').on('change keypress paste input', function (e) {
        CheckPage2Wizard();
    });

    $('#select-eccezioni').on('change', function () {
        OnChange_select_eccezioni($(this));
    });

    $("#data_da,#data_a").on('keydown', function (event) {
        //event.preventDefault();
        //if (event.keyCode == 13 || event.keyCode == 9) {
        //    event.stopPropagation();
        //    var strVal = $(dateEdit).val();
        //    if (strVal != '' && !moment(strVal, "DD/MM/YYYY", true).isValid()) {
        //        event.preventDefault();

        //        alert('La data inserita non è valida');
        //        $(dateEdit).focus();
        //    }
        //}
    });
    
    $("#dalGiorno").on("dp.change", function (e) {

        if ($("#dalGiorno").data('DateTimePicker').date() > $("#alGiorno").data('DateTimePicker').date()) {
            $("#alGiorno").data('DateTimePicker').date(e.date);
        }
    });

    $("#alGiorno").on("dp.change", function (e) {

        if ($("#dalGiorno").data('DateTimePicker').date() > $("#alGiorno").data('DateTimePicker').date()) {
            $("#dalGiorno").data('DateTimePicker').date(e.date);
        }
    });

    Handlers3();
}

function refreshDataAsync() {
    var color = "#1d92f5";

    if (document.getElementsByClassName("bg-cdf").length) {
        var style = window.getComputedStyle(document.getElementsByClassName("bg-cdf")[0], null);
        if (style.length) {
            var rgb = style["background-color"];
            if (rgb.length) {
                var color = rgb.replace('rgb(', '').replace(')', '').split(',');

                var r, g, b;
                r = parseInt(color[0].trim());
                g = parseInt(color[1].trim());
                b = parseInt(color[2].trim());

                color = "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
            }
        }
    }

    $("div[data-async]").each(function () {
        var url = $(this).attr("data-async");
        if (url.toLowerCase().indexOf("/feriepermessi") >= 0) {
            if ($("#anno").val() != "") {
                url += "?anno=" + $("#anno").val();
            }
        }
        var div = $(this);

        $.ajax({
            type: 'GET',
            url: url,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $(div).replaceWith(data);
                $('.chart1').easyPieChart({
                    "barColor": color, "delay": 300, scaleColor: false, lineWidth: 8, size: 125
                });
                $('.chart2').easyPieChart({
                    "barColor": color, "delay": 300, scaleColor: false, lineWidth: 8, size: 125
                });



                $('.chart-small').easyPieChart({
                    "barColor": "#0088CC", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });

                $('.chart-small-fe').easyPieChart({
                    "barColor": "#48bfff", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-pf').easyPieChart({
                    "barColor": "#1d92f5", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-pr').easyPieChart({
                    "barColor": "#3431a4", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-pg').easyPieChart({
                    "barColor": "#d26a5c", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-mn').easyPieChart({
                    "barColor": "#00aa00", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-mr').easyPieChart({
                    "barColor": "#EAA921", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });
                $('.chart-small-mf').easyPieChart({
                    "barColor": "#f88201", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                });

                //jQuery('[data-toggle="tabs"]').each(function (e) {
                //    jQuery(this).tab('show');
                //});
            },
            error: function (a, b, c) {

            }
        });

    });
}

function OnChange_select_eccezioni(element) {
    $("#extra-params-container").html("");
    $('#select_eccezioni_descAggiuntiva').html("");
    var codEccez = ($("#select-eccezioni").find(":selected").val());
    $(".ecc-selez").text(codEccez);

    var selected = $("#select-eccezioni").find(":selected");

    var desc = $(selected).data('infodesc');

    if (typeof desc != "undefined" && desc != null)
        desc = $.trim(desc);

    if (typeof desc != "undefined" && desc != null && desc.length > 0) {
        $('#select_eccezioni_descAggiuntiva').html(desc);
        $('#select_eccezioni_descAggiuntiva_Container').show();
    }

    for (var i = 0; i < ControlliPerEccezione.length; i++) {
        if (ControlliPerEccezione[i].codice == codEccez) {
            AbilitaControlliEccezione(ControlliPerEccezione[i].controlli, ControlliPerEccezione[i].importo_preimpostato);
            if (ControlliPerEccezione[i].caratteriRichiesti == 0) {
                $("#button-invia").removeClass("disable");
                $("#mot-ric").removeClass().addClass("fac");
                $("#red-star").hide();
            }
            else {

                $("#button-invia").addClass("disable");
                $("#mot-ric").removeClass().addClass("req");
                $("#red-star").show();
            }

            //controlla se ha una partial per extra params
           
            if (ControlliPerEccezione[i].partial != "" ) {
                CaricaPartialExtraParams(ControlliPerEccezione[i].partial);
            }
            if (ControlliPerEccezione[i].richiede_attivita_ceiton)
            {
                
                CaricaPartialExtraParams_Ceiton(ControlliPerEccezione[i].codice);
            }
            break;
        }
    }

    CheckPage2Wizard();
}

function CaricaPartialExtraParams(partial) {
    $.ajax({
        type: 'GET',
        url: "/ajax/getPartial?nomePartial=" + partial,
        dataType: "html",
        data: {},
        cache: false,
        success: function (data) {
            $("#extra-params-container").html(data);
        },
        error: function (a, b, c) {

            console.log("Ajax Error:" + a + ',' + b + ',' + c);

        }
    });
}
function CaricaPartialExtraParams_Ceiton(cod) {
    $.ajax({
        type: 'GET',
        url: "/ajax/getPartialAttivitaCeiton?cod=" + cod+"&data="+$("#data_da").val(),
        dataType: "html",
        data: {},
        cache: false,
        success: function (data) {
            $("#extra-params-container").html(data);
        },
        error: function (a, b, c) {

            console.log("Ajax Error:" + a + ',' + b + ',' + c);

        }
    });
}
function inviaForm(form) {

    //verifica se le date erano disabilitate(assente ingiustificato)
    var att = $("#data_da").attr("disabled");
    var DateDisabilitate = false;
    if (typeof att !== typeof undefined && att !== false) {
        DateDisabilitate = true;
    }
    //rimuovi i disabled altrimenti non partono i dati
    $("#data_da").removeAttr("disabled");
    $("#data_a").removeAttr("disabled");

    var serializedForm = form.serialize();

    var htmlInvia = $("#button-invia").html();
    var htmlAttesa = '<i class="fa fa-refresh fa-spin"></i>';

    $("#button-invia").html(htmlAttesa).attr("disabled", "disabled");

    $.ajax({
        url: '/ajax/inserimento',
        type: "GET",
        data: serializedForm,
        complete: function () { $("#button-invia").html(htmlInvia).removeAttr("disabled"); },
        success: function (data) {
            ForceRefreshAll();
            refreshDataAsync();

            $.ajax({
                type: 'GET',
                url: "/scrivania/Ajax_GetWidgetGiornateInEvidenza",
                dataType: "html",
                data: {},
                cache: false,
                success: function (data) {
                    $("#Widget1").replaceWith(data);
                },
                error: function (a, b, c) {}
            });

            if (DateDisabilitate == true) {
                $("#data_da").attr("disabled", "disabled");
                $("#data_a").attr("disabled", "disabled");
            }
            if (data.result == "OK") {
                PuoiTornareHome = false;

                //controlla prima se ci sono altre assenze ingiustificate:
                //se ci sono altre assenze ingiustificate, mostra la prossima
                $.ajax({
                    url: '/ajax/GetAssenzeIngiustificate',
                    type: "GET",
                    dataType: "json",
                    data: {},

                    success: function (data) {
                        var d1 = $("#data_da").val();
                        var d2 = $("#data_a").val();
                        $('#modal-popin').modal('hide');
                        AssenzeIngiustificateNow = data.result.length;
                        if (data.result.length > 0) {
                            //se ci sono altre date ingiustificate:
                            swal({
                                title: 'Inserimento eseguito',
                                type: 'success',
                                html:
                                  ' ',
                                showCloseButton: true,
                                showCancelButton: false,
                                confirmButtonText: ' Continua '
                            }).then(function () {

                                setTimeout(ShowPopup('', '', data.result[0], data.result[0], true, true), 2000);
                            });
                        }
                        else {

                            //Se arrivo dal controllo delle assenze ingiustificate
                            //porto la matricola alla prima maggior presenza
                            if (DateDisabilitate){
                                $.ajax({
                                    url: '/ajax/GetMaggiorPresenza',
                                    type: "GET",
                                    dataType: "json",
                                    data: {},
                                    success: function (data) {
                                        if (data.result.length > 0) {
                                            d1 = data.result[0];
                                            d2 = data.result[0];
                                        }
                                    }, error: function (a, b, c) {
                                        alert("Failed 1 " + a + b + c);
                                    }
                                });
                            }

                            //se non ci sono altre date ingiustificate:
                            $("#data_da").removeAttr("disabled");
                            $("#data_a").removeAttr("disabled");
                            $("#button-invia").html(htmlInvia).removeAttr("disabled");

                            swal({
                                title: 'Inserimento eseguito',
                                type: 'success',
                                html:
                                  ' ',
                                showCloseButton: true,
                                showCancelButton: true,
                                cancelButtonText: '     Esci      ',
                                confirmButtonText: 'Continua'
                            }).then(function () {
                                setTimeout(ShowPopup('', '', d1, d2, false, false), 2000);
                            });

                            if (location.href.toLocaleLowerCase().indexOf("/home") < 0) {
                                $.ajax({
                                    type: 'GET',
                                    url: "/scrivania/index_section1",
                                    dataType: "html",
                                    data: {},
                                    cache: false,
                                    success: function (data) {
                                        $("div[data-async='/scrivania/index_section1']").replaceWith(data);
                                        $('.chart1').easyPieChart({
                                            "barColor": "#25affb", "delay": 300, scaleColor: false, lineWidth: 8, size: 125
                                        });
                                        $('.chart2').easyPieChart({
                                            "barColor": "#2e62e0", "delay": 300, scaleColor: false, lineWidth: 8, size: 125
                                        });
                                        $('.chart-small').easyPieChart({
                                            "barColor": "#0088CC", "delay": 300, scaleColor: false, lineWidth: 4, size: 55
                                        });
                                    },
                                    error: function (a, b, c) {

                                    }
                                });

                            }

                        }
                    },
                    error: function (a, b, c) {

                        alert("Failed 1 " + a + b + c);
                    }
                });




            }
            else { //messaggio di errore dal controller
                ForceRefreshEvidenzeJS();
                ForceRefreshMieRichiesteJS();
                swal({
                    title: 'Anomalie in inserimento',
                    type: data.result.indexOf("*") == 0 ? 'error' : 'warning',
                    html: "<div style='max-width:100%;overflow:auto;font-size:12px'>" + data.result + "</div>",
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText: ' OK'
                })
            }
        },
        error: function (result) { alert("Failed"); }

    });
}

function AbilitaInvioProposteAuto() {
    
    var CBchecked = ($('#table-ecc-auto input[type="checkbox"]:checked').length);
    if (CBchecked == 0) {
        $("#button-conferma-auto").attr("disabled", "disabled");
        return;
    }
    var ceiton_obb_sede = $("#giornata-info-attr").attr("data-ceiton-obbl") == "True";

    var abilita = true;
    $(".nota-su-auto").each(function () {

        var car_minimi = $(this).attr("data-charmin");
        if ($(this).is(":visible") && $(this).val().length < car_minimi) {
            $("#button-conferma-auto").attr("disabled", "disabled");
            abilita = false;
        }
    });
    if (ceiton_obb_sede)
    {
        $("select.ep-obb").each(function () {
            if ($(this).is(":visible") && $(this).val() == "") {
                abilita = false;
            }
        });
    }

    if (abilita)
        $("#button-conferma-auto").removeAttr("disabled");
    else
        $("#button-conferma-auto").attr("disabled", "disabled");
}

function OnChange_notasuauto(textarea) {
    AbilitaInvioProposteAuto();
}

function PropAutoOnlyOne(cb) {
    var codice = $(cb).attr("data-cod");
    if ($(cb).prop("checked")) {
        $(".cb-ecc-auto").each(function () {

            if ($(this).attr("data-cod") == codice) {
                $(this).prop("checked", false);
                $("#txa-" + $(this).val()).val("");
                $("#tdnota-" + $(this).val()).addClass("hide");
            }
        });
        $(cb).prop("checked", true);
        $("#tdnota-" + $(cb).val()).removeClass("hide");
        $("#tdnota-" + $(cb).val()).find("textarea").focus();
    }
    else
        $("#tdnota-" + $(cb).val()).addClass("hide");
}
function OnChange_CBautoEcc(cb) {
    var gruppoClicked = $(cb).attr("data-gruppo");
    PropAutoOnlyOne(cb);
    var richiede_ceiton = ($(cb).attr("data-att-ceiton-obbl") == "True");
    var ec = $(cb).attr("data-cod");

    if ($(cb).prop("checked")) {
        if (richiede_ceiton) {
            var sede_richiede_ceiton = $("#giornata-info-attr").attr("data-ceiton-obbl") == "True";

            var mytr = $(cb).closest("tr");
           
            $(mytr).after("<tr id='tr-ceiton-"+ec+"'><td></td> <td id='att-ceiton-" + ec + "' colspan='4'><i class='fa fa-refresh fa-spin'></i>&nbsp;Ricerca attività Ceiton in corso...</td></tr>");
            $.ajax({
                type: 'GET',
                url: "/ajax/getPartialAttivitaCeiton?cod=" + ec + "&data=" + $("#data_da").val(),
                dataType: "html",
                data: {},
                cache: false,
                success: function (data) {
                    $("#att-ceiton-" + ec).html(data);
                    if (data.indexOf("Non ci sono") >= 0 && sede_richiede_ceiton) {
                        
                        $(cb).prop("checked", false).attr("disabled", "disabled").parent("label").hide();
                        $("#att-ceiton-" + ec).closest("tr").next().find("textarea").attr("disabled", "disabled");
                        $("#tr-ceiton-" + ec + " td").css("border-top", "0px");
                    }
                    else {
                        $("select.ep-obb").attr("onchange", "AbilitaInvioProposteAuto()");
                       
                    }
                    AbilitaInvioProposteAuto();
                },
                error: function (a, b, c) {
                    console.log("Ajax Error:" + a + ',' + b + ',' + c);
                }
            });
        }


        if ($(cb).attr("data-domanda") != "1") {
            $(".cb-ecc-auto").each(function () {
                if ($(this).attr("data-gruppo") == gruppoClicked) return;
                if ($(this).attr("data-domanda") == "1") return;

                $(this).prop("checked", false);
                $("#txa-" + $(this).val()).val("");
                $("#tdnota-" + $(this).val()).addClass("hide");

            });
            }
       

        $(cb).prop("checked", true);
        $("#tdnota-" + $(cb).val()).removeClass("hide");
        $("#tdnota-" + $(cb).val()).find("textarea").focus();
        var caratteri = $("#tdnota-" + $(cb).val()).find("textarea").attr("data-charmin");
        if (caratteri == 0)
            $("#button-conferma-auto").removeAttr("disabled");
        else
            $("#button-conferma-auto").attr("disabled", "disabled");
    }
    else
    {//spento
        $("#tdnota-" + $(cb).val()).addClass("hide");
        if (richiede_ceiton) {
            $("#tr-ceiton-" + ec).remove();
        }
    }

    AbilitaInvioProposteAuto();
}



function OnChange_notainserimento(element) {
    var eccSelezionata = $("#select-eccezioni option:selected").val();
    var arrEccezioniCeiton = $("#giornata-info-attr").attr("data-eccezioni-ceiton").split(',')
    var NecessariaAttivita =(arrEccezioniCeiton.indexOf(eccSelezionata) >=0);

    var minChars = 0;
    for (var i = 0; i < ControlliPerEccezione.length; i++) {
        if (ControlliPerEccezione[i].codice == $("#select-eccezioni option:selected").val()) {
            minChars = ControlliPerEccezione[i].caratteriRichiesti;
            break;
        }
    }
    if (minChars == 0) {
        $("#mot-ric").removeClass("req");
        $("#red-star").hide();
    }
    else {
        $("#mot-ric").addClass("req");
        $("#red-star").show();
    }

    var CeitonObbligatorioPerSede = $("#giornata-info-attr").attr("data-ceiton-obbl");
    if (CeitonObbligatorioPerSede == "False" || ! NecessariaAttivita)
        $("#idattivita").removeClass("ep-obb");
    

    var EP = true;
    $("input.ep-obb").each(function () {
        if ($.trim($(this).val()) == "") EP = false;
    });
    $("select.ep-obb").each(function () {
        if ($.trim($(this).val()) == "") EP = false;
    });

   

    var noCeitonActivityBlock = ($("#no-ceiton-activity").length>0);
    var ButtonEnabled = false;

    if ($.trim($("#NotaInserimento").val()).length >= minChars && EP) {
        if (CeitonObbligatorioPerSede == "False")
        {
            ButtonEnabled = true;
        }
        if (CeitonObbligatorioPerSede == "True" && ! noCeitonActivityBlock)
        {
            ButtonEnabled = true;
        }
    }
    if (ButtonEnabled==true)
        $("#button-invia").removeClass("disable");
    else
        $("#button-invia").addClass("disable");
}

function OnClick_buttonNextWizard() {
    var currentPage = $("#currentpage").val();
    var NextPage = Number(currentPage) + 1;
    $("#currentpage").val(NextPage);
    $("#tab" + NextPage).click();
    CheckPage();
}

function OnClick_buttonPrevWizard() {
    AzzeraVincoliWizard();
    var currentPage = $("#currentpage").val();
    var PrevPage = Number(currentPage) - 1;
    $("#currentpage").val(PrevPage);
    $("#tab" + PrevPage).click();
    CheckPage();
}

function OnChange_motivodigitato(textarea) {
    if ($(textarea).val().length > 0) {
        $("#conferma-rifiuto").removeAttr("disabled");
    }
    else {
        $("#conferma-rifiuto").attr("disabled", "disabled");
    }
}

function OnChange_dettaglioTextArea(textarea) {
    if ($.trim($(textarea).val()) == "") {
        $("#DettaglioRifiutaButton").attr("disabled", "disabled");
        $("#DettaglioRifiutaButton").prop("disabled", true);
    }

    else {
        $("#DettaglioRifiutaButton").removeAttr("disabled");
        $("#DettaglioRifiutaButton").prop("disabled", false);
    }
}

function OnClick_TRtoDelete(tr, clickedbutton) {
    if ($(clickedbutton).hasClass("button-osserva") || $(clickedbutton).parent().hasClass("button-osserva")) {
        return;
    }
    else DeleteTr(tr);
}

function OnClick_buttonRifiuta(button) {
    var TRparent = button.closest("tr");
    var IdEccezioneRichiesta = $(TRparent).data("ideccezione");
    $("#motivo-digitato").val("");
    $("#conferma-rifiuto").attr("disabled", "disabled");
    $("#conferma-rifiuto").attr("onclick", "RifiutaEccezione(" + IdEccezioneRichiesta + ",$('#motivo-digitato').val())");
    $("#motivo-digitato").attr("placeholder", "Motivo del rifiuto...");
    $("#titolo").text("Rifiuta richiesta");
    $('#motivo-rifiuto').modal('show');
}

function OnClick_buttonApprova(button) {
    var TRparent = button.closest("tr");
    var IdEccezioneRichiesta = $(TRparent).data("ideccezione");
    ApprovaEccezione(IdEccezioneRichiesta, "", false, button);
}

function OnClick_buttonRifiutaTutti(button) {
    $("#testoprogress").val("");
    var q = $(button).closest("div.tab-pane").find("input[type=checkbox].seltr:checked").length;
    if (q == 1) $("#titolo1").text("Confermi di rifiutare la richiesta selezionata?");
    else $("#titolo1").text("Confermi di rifiutare le " + q + " richieste selezionate?");

    $('#valida-rifiuta-tutti').modal('show');
}

function OnClick_buttonDateok() {
    ShowWizard();
}

function OnChange_DataA(e) {
    $("#wiz-tit").addClass("hide");
    $("#wiz-body").addClass("hide");

    $("#timbratureday").html("");
    $("#segnalazioniday").html("");

    $("#dateok").removeClass("disable");
    $(".ecc-selez").text("Scegli l'eccezione...");
    reinitWizard();
}

function OnChange_DataDa(e) {
    $("#data_a").removeAttr("disabled");
    $("#wiz-tit").addClass("hide");
    $("#wiz-body").addClass("hide");

    $("#timbratureday").html("");
    $("#segnalazioniday").html("");
    jQuery('#data_a').data("DateTimePicker").minDate(e.date);
    $("#data_a").val($("#data_da").val());


    $("#dateok").removeClass("disable");
    $(".ecc-selez").text("Scegli l'eccezione...");
    reinitWizard();

    if (IsDateGreaterThanToday($("#data_da").val())) {
        $("#data_a").removeAttr("disabled");
    }
    else {
        if ($("#data_da").val().length == 10) {
            $("#data_a").attr("disabled", "disabled");
            $("#dateok").click();
        }
    }
}


function OnClick_refreshurl(button) {
    var url = button.attr("data-refreshurl");
    var elements = button.attr("data-refreshelements");
    var parentdiv = button.attr("data-parentdiv");
    //if (url.toLowerCase().indexOf("getdaappratt") > 0)
    //{debugger
    //    var nominativo = $("#nominativo").val();
    //    if (nominativo.trim() != "")
    //    {
    //        url = url + "?nom=" + nominativo;
    //    }
    //}
    RefreshPartial(url, elements, parentdiv);

}

function OnClick_filterurl(button) {
    var url = button.attr("data-filter");
    var elements = button.attr("data-filterelements");
    var parentdiv = button.attr("data-parentdiv");

    RefreshPartial(url, elements, parentdiv);
}

function OnClick_reseturl(button) {
    var url = button.attr("data-reset");
    var elements = button.attr("data-resetelements");
    var parentdiv = button.attr("data-parentdiv");

    RefreshPartial(url, elements, parentdiv);
}

function AbilitaControlliEccezione(controlli,importo_preimp) {
    console.log(controlli);
    $(".params").addClass("hide");
    $(".params input").attr("disabled", "disabled").val("");
    if (controlli == null || controlli === undefined) return;

    if (controlli.indexOf("[dalle]") > -1) {
        $("#param-dalle").removeClass("hide");
        $("#param-dalle input").removeAttr("disabled");
    }
    if (controlli.indexOf("[alle]") > -1) {
        $("#param-alle").removeClass("hide");
        $("#param-alle input").removeAttr("disabled");
    }
    if (controlli.indexOf("[quantita]") > -1) {
        $("#param-quantita").removeClass("hide");
        $("#param-quantita input").removeAttr("disabled");
    }
    if (controlli.indexOf("[importo]") > -1) {
        $("#param-importo").removeClass("hide");
        $("#param-importo input").removeAttr("disabled");
        if (importo_preimp != null && importo_preimp != "") {
            $("#param-importo input").val(importo_preimp);
        }
    }
    if (controlli.indexOf("[df]") > -1) {
        $("#param-df").removeClass("hide");
        $("#param-df input").removeAttr("disabled");
    }
    if (controlli.indexOf("[uorg]") > -1) {
        $("#param-uorg").removeClass("hide");
        $("#param-uorg input").removeAttr("disabled");
    }
    if (controlli.indexOf("[matrspett]") > -1) {
        $("#param-matrspett").removeClass("hide");
        $("#param-matrspett input").removeAttr("disabled");
    }
}

function GetMinDalle() {
    var d = $("#param-dalle").find("input").val();
    if (d.indexOf("_") < 0 && $.trim(d) != "") {
        var h = Number(d.substring(2, 0));
        var m = Number(d.substring(5, 3));
        var mindalle = (h * 60) + m;
        return mindalle;
    }
    else
        return -1;
}
function GetMinAlle() {
    var d = $("#param-alle").find("input").val();
    if (d.indexOf("_") < 0 && $.trim(d) != "") {
        var h = Number(d.substring(2, 0));
        var m = Number(d.substring(5, 3));
        var minalle = (h * 60) + m;
        return minalle;
    }
    else
        return -1;
}

function CheckLimiteUMH(d, a) {
    if (a.length == 5 && $("#select-eccezioni").val() == "UMH") {
        var lastTimb = $("#timbraturetoday .timbratura-out").last().text().trim();
        if (lastTimb != null && lastTimb != "") {
            if (d == lastTimb) {
                var uscita = $("#timbraturetoday").attr("data-proiezione-uscita-min");
                var alle = toMinutes(a);
                var dalle = toMinutes(d);

                if (alle > uscita) {
                    alle = uscita;
                    if (alle > dalle) {
                        $("#param-alle").find("input").val(ConvertiMinutiToHHMM(alle));
                        $("#modif-quantita").text("Periodo modificato in " + d + "/" + $("#param-alle").find("input").val() + " per compatibilità con orario");
                        $("#param-quantita").find("input").val(ConvertiMinutiToHHMM(alle - dalle));
                    }
                }
            }
        }
    }
}

function CheckQuantita() {
    var d = $("#param-dalle").find("input").val();
    var a = $("#param-alle").find("input").val();
    $("#modif-quantita").text("");

    if (d.indexOf("_") < 0 && a.indexOf("_") < 0 && $.trim(a) != "" && $.trim(d) != "") {
        var h = Number(d.substring(2, 0));
        var m = Number(d.substring(5, 3));
        var mindalle = (h * 60) + m;

        h = Number(a.substring(2, 0));
        m = Number(a.substring(5, 3));
        var minalle = (h * 60) + m;

        var diffmin = minalle - mindalle;
        var diffh = parseInt(diffmin / 60);
        var diffm = diffmin - (diffh * 60);
        if (diffh >= 0 && diffm >= 0 && m <= 59) {
            var diffh_s = diffh < 10 ? '0' + diffh.toString() : diffh.toString();
            var diffm_s = diffm < 10 ? '0' + diffm.toString() : diffm.toString();
            $("#param-quantita").find("input").val(diffh_s + ":" + diffm_s);
            CheckLimiteUMH(d, a);
           
        }
        else {
            $("#param-quantita").find("input").val("");
        }

    }
    else {
        $("#param-quantita").find("input").val("");
    }

}


function reinitWizard() {

    $("#enabledpage").val("1");
    $("#currentpage").val("1");
    $(".enable-next-page").val("");
    $("#NotaInserimento").val("");

    $('#select-eccezioni').empty();
    $("#button-invia").addClass("disable");
    // $("#wiz-tit").addClass("hide");
    //$("#wiz-body").addClass("hide");

    //$("#timbratureday").html("");
    //$("#segnalazioniday").html("");
    $(".ecc-selez").text("Scegli l'eccezione...");
    $(".params").addClass("hide");
    $(".params input").attr("disabled", "disabled").val("");
    $("#proposteday").html("");

    $("#extra-params-container").html("");

    CheckPage();

    $("#tab1").click();

    DisabilitaProsegui();
    DisablilitaRadio();
    DisabilitaTipi();
}

function RefreshPartial(url, elements, parentdiv) {
    $("#" + parentdiv + ">.block").addClass("rai-loader");
    var fullUrl = url;
    fullUrl += (url.indexOf('?') == -1) ? "?" : "&";
    fullUrl += 'nome=' + $("#nominativo").val() + '&sede=' + $("#sede").val() + '&stato=' + $("#stato").val() + '&eccezione=' + $("#eccezione").val() + '&data_da=' + $("#datada").val() + '&data_a=' + $("#dataal").val() + '&titolo=' + $("#titolo").val() + '&luogo=' + $("#luogo").val() + '&visualizzati=' + $("#Visualizzato").val() + '&livelloDip=' + $('#livelloDip').val(),

    $.ajax({
        type: 'GET',
        url: fullUrl,
        dataType: "html",
        data: {},
        cache: false,
        success: function (data) {

            var arr = elements.split(',');

            for (var i = 0; i < arr.length; i++) {
                var d = $(data).find("#" + arr[i]);
                $("#" + arr[i]).html($(d).html());
            }
            if (parentdiv == "daapprovare" || parentdiv == "mierichieste" || parentdiv == "panel-docdipendente" || parentdiv == "listaprogrammi" || parentdiv == "listaeventi") {
                jQuery('[data-toggle="tabs"] a, .js-tabs a').click(function (e) {
                    e.preventDefault();
                    jQuery(this).tab('show');
                });
            }

            $("#" + parentdiv + " div").removeClass("rai-loader");
            $("#" + parentdiv).removeClass("rai-loader");
            if (url.indexOf("refreshMieNotifiche") >= 0) {
                if (data.indexOf("NON CI SONO") >= 0) {
                    $("#delnotifiche").attr("disabled", "disabled");
                    $("#delnotifiche").addClass("disable");
                }
                else {
                    $("#delnotifiche").removeAttr("disabled");
                    $("#delnotifiche").removeClass("disable");
                }

            }
        },
        error: function (a, b, c) {
            $("#" + parentdiv + " div").removeClass("rai-loader");
            $("#" + parentdiv).removeClass("rai-loader");
            console.log("Ajax Error:" + a + ',' + b + ',' + c);

        }
    });
    return false;
}

function RifiutaEccezione(idRichiestaEccezione, NotaSuRifiuto) {
    var url = "/ajax/rifiutaeccezione?id=" + idRichiestaEccezione + "&nota=" + NotaSuRifiuto;
    var button = $(".btn-rif-" + idRichiestaEccezione + ":visible");

    var icon = $(button).find("i")[0];
    var classicon = $(icon).attr("class");
    $(icon).attr("class", "fa fa-spin fa-refresh");
    var classIconRifBoss = "";
    var iconRifBoss = null;
    if ($("#segnalazioniday-boss").is(":visible")) {

        var btnRifBoss = $("#btn_rifiuta_" + idRichiestaEccezione);
        iconRifBoss = $(btnRifBoss).find("i")[0];
        classIconRifBoss = $(iconRifBoss).attr("class");
        $(iconRifBoss).attr("class", "fa fa-spin fa-refresh");
    }

    $(".btn-app-" + idRichiestaEccezione).attr("disabled", "disabled");
    $(".btn-rif-" + idRichiestaEccezione).attr("disabled", "disabled");
    $.ajax({
        type: 'GET',
        url: url,
        dataType: "json",
        data: {},
        cache: false,
        success: function (data) {

            if (data.result == "OK") {
                //chiamata controller OK
                swal({
                    title: 'Richiesta rifiutata',
                    type: 'error',
                    html:
                      ' ',
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText:
                      ' OK'
                });

                RigaRifiuta(idRichiestaEccezione);
                ForceRefreshMieRichiesteJS();

                $(icon).attr("class", classicon);
                if ($("#segnalazioniday-boss").is(":visible")) {
                    $(iconRifBoss).attr("class", classIconRifBoss);
                }
                AggiornaTotRichieste();
            }
            else {//errore chiamata controller
                swal('Oops...', data.result, 'error');
                $(icon).attr("class", classicon);
                //riabilita i tasti
                $(".btn-app-" + idRichiestaEccezione).removeAttr("disabled");
                $(".btn-rif-" + idRichiestaEccezione).removeAttr("disabled");
            }
        },
        error: function (a, b, c) { }
    });
}


function ApprovaEccezione(idRichiestaEccezione, NotaSuApprovazione, showConferma, button) {
    var url = "/ajax/validaeccezione?id=" + idRichiestaEccezione + "&nota=" + NotaSuApprovazione;
    var button = $(".btn-app-" + idRichiestaEccezione + ":visible");

    var icon = $(button).find("i")[0];
    var classicon = $(icon).attr("class");
    $(icon).attr("class", "fa fa-spin fa-refresh");

    
    var classIconAppBoss = "";
    var iconAppBoss = null;
    if ($("#segnalazioniday-boss").is(":visible")) {
        var btnAppBoss = $("#btn_approva_" + idRichiestaEccezione);
        iconAppBoss = $(btnAppBoss).find("i")[0];
        classIconAppBoss = $(iconAppBoss).attr("class");
        $(iconAppBoss).attr("class", "fa fa-spin fa-refresh");
    }


    $(".btn-app-" + idRichiestaEccezione).attr("disabled", "disabled");
    $(".btn-rif-" + idRichiestaEccezione).attr("disabled", "disabled");
    $.ajax({
        type: 'GET',
        url: url,
        dataType: "json",
        data: {},
        cache: false,
        success: function (data) {

            if (data.result == "OK") {
                //chiamata controller OK
                if (showConferma) {
                    swal({
                        title: 'Richiesta approvata',
                        type: 'success',
                        html:
                          ' ',
                        showCloseButton: true,
                        showCancelButton: false,
                        confirmButtonText:
                          ' OK'
                    });
                }
                RigaApprova(idRichiestaEccezione, button);
                ForceRefreshMieRichiesteJS();
                if ($("#segnalazioniday-boss").is(":visible")) {
                    $(iconAppBoss).attr("class", classIconAppBoss);
                }
                $(icon).attr("class", classicon);
                AggiornaTotRichieste();
            }
            else if (data.result == "SESSIONE SCADUTA") {
                swal(
                    'Oops...',
                    data.result,
                    'error'
                  ).then(function () { location.reload("/home/clear") });
            }
            else {//errore chiamata controller
                swal(
                     'Oops...',
                     data.result,
                     'error'
                   );

                $(icon).attr("class", classicon);
                //riabilita i tasti
                $(".btn-app-" + idRichiestaEccezione).removeAttr("disabled");
                $(".btn-rif-" + idRichiestaEccezione).removeAttr("disabled");
            }
        },
        error: function (a, b, c) { }
    });
}

function RigaRifiuta(IdRichiestaEccezione) {
    var tr = $("tr[data-ideccezione='" + IdRichiestaEccezione + "']");
    var widthtr = $(tr).parent("tbody").prev("tbody").width();

    var heighttr = $(tr).height();

    var trAll = $("tr[data-ideccezione='" + IdRichiestaEccezione + "']");
    $(trAll).find("td,button").css("opacity", "0.3");

    $(".button-osserva").css("opacity", "1");
    var h = '<div style="position:relative">' +
        '<div style="background-color:transparent;pointer-events:none;text-align:center;position: absolute; top:0; left:0; height: '
            + heighttr + 'px;width:' + widthtr + 'px;">' +
        '<i class="si si-close fa-3x text-danger riga-validata-icon" ></i>';
    '</div></div>';

    var td = $(trAll).find("td:first").each(function () {
        //  $(this).css("opacity", "1");
        $(this).html(h);
    });

    $(trAll).addClass("trtodelete");

    $(trAll).each(function () {
        //individua label numero richieste

        var tbody = $(this).closest("tbody");

        var tbodyPrev = $(tbody).prev("tbody");
        var sp = $(tbodyPrev).find("span.label-info")[0];

        var count = 0;
        //conta quante tr non marcate con .trtodelete
        var q = $(tbody).find("tr:visible").each(function () {
            if (!$(this).hasClass("trtodelete") && ($(this).hasClass("hidden-xs") || $(this).hasClass("visible-xs"))) count++;
        });
        //count = (count / 2); //toglie ultima riga 

        //aggiorna label numero richieste
        $(sp).text(count.toString() + " richiest" + (count == 1 ? "a" : "e"));

        if (count == 0) {
            var trfirst = $(tbodyPrev).find("tr:first");
            var tdfirst = $(trfirst).find("td:first");
            $(tdfirst).html("");
        }

    });

    var dataVisualizzata = $("#InfoGiornataCorrenteContainerApprovazione").data('dataVisualizzata');
    $.ajax({
        url: '/ajax/getSegnalazioniAjaxView',
        type: "GET",
        cache: false,
        dataType: "html",
        data: {
            date: dataVisualizzata,
            matricola: '',
            abilitaApprovazione: '1',
            ideccezionerichiesta: IdRichiestaEccezione
        },
        success: function (data) {
            if ($("#btnModalApprovaEccezione_" + IdRichiestaEccezione).length > 0) {
                $('#DettaglioTextarea').prop('disabled', true);
            }
            $("#btnModalApprovaEccezione_" + IdRichiestaEccezione).prop('disabled', true);
            $("#btnModalRifiutaEccezione_" + IdRichiestaEccezione).prop('disabled', true);

            var t = $("#InfoGiornataCorrenteContainerApprovazione");
            $(t).html(data);
            $(t).data('dataVisualizzata', dataVisualizzata);
            $(t).removeClass('hide');
        }
    });

    if ($("#segnalazioniday-boss").is(":visible")) {

        $("#btn_rifiuta_" + IdRichiestaEccezione).addClass("disable");
        $("#btn_approva_" + IdRichiestaEccezione).addClass("disable");
        var trBoss = $("tr[data-idecc-rich='" + IdRichiestaEccezione + "']");
        $(trBoss).css("opacity", "0.2");
    }

}

function PropostaAutoDisattiva(index) {
    var tr = $("tr[data-indexprop='" + index + "']");
    var widthtr = $(tr).parent("tbody").width();
    var heighttr = $(tr).height() + $(tr).next().height();

    var trAll = $("tr[data-indexprop='" + index + "']");
    $(trAll).find("td,button").css("opacity", "0.3");

    var h = '<div style="position:relative">' +
      '<div style="background-color:transparent;pointer-events:none;text-align:center;position: absolute; top:0; left:0; height: ' + heighttr + 'px;width:' + widthtr + 'px;">' +
      '<i class="si si-check fa-3x text-success riga-validata-icon" ></i>';
    '</div></div>';

    var td = $(trAll).find("td:first").each(function () {
        $(this).css("opacity", "1");
        $(this).html(h);
    });

    $("#button-conferma-auto").attr("disabled", "disabled");
    $("#txa-" + index).attr("disabled", "disabled");

}

function RigaApprova(IdRichiestaEccezione, button) {

    var tr = $(button).closest("TR");// $("tr[data-ideccezione='" + IdRichiestaEccezione + "']");
    var widthtr = $(tr).parent("tbody").prev("tbody").width();

    var heighttr = $(tr).height();

    var trAll = $("tr[data-ideccezione='" + IdRichiestaEccezione + "']");
    $(trAll).find("td,button").css("opacity", "0.3");

    $(".button-osserva").css("opacity", "1");
    var h = '<div style="position:relative">' +
        '<div style="background-color:transparent;pointer-events:none;text-align:center;position: absolute; top:0; left:0; height: ' + heighttr + 'px;width:' + widthtr + 'px;">' +
        '<i class="si si-check fa-3x text-success riga-validata-icon" ></i>';
    '</div></div>';
    //var htmlRifiuta = '<i class="si si-close fa-2x text-danger riga-validata-icon" ></i>';

    var td = $(trAll).find("td:first").each(function () {
        //   $(this).css("opacity", "1");
        $(this).html(h);
    });

    $(trAll).addClass("trtodelete");

    $(trAll).each(function () {

        //individua label numero richieste

        var tbody = $(this).closest("tbody");

        var tbodyPrev = $(tbody).prev("tbody");
        var sp = $(tbodyPrev).find("span.label-info")[0];

        var count = 0;
        //conta quante tr non marcate con .trtodelete
        var q = $(tbody).find("tr:visible").each(function () {
            if (!$(this).hasClass("trtodelete") && ($(this).hasClass("hidden-lg") || $(this).hasClass("visible-lg"))) count++;
        });


        //aggiorna label numero richieste
        $(sp).text(count.toString() + " richiest" + (count == 1 ? "a" : "e"));
        if (count == 0) {
            var trfirst = $(tbodyPrev).find("tr:first");
            var tdfirst = $(trfirst).find("td:first");
            $(tdfirst).html("");
        }
    });

    var divtab = $(button).closest("div.tab-pane");
    var tabs = ["tab-1", "tab-2"];
    for (var i = 0; i < tabs.length; i++) {
        if (divtab.attr("id") == tabs[i]) {
            var totalTR = $("#" + tabs[i] + " tbody.tbodydata tr.trdata").not(".trtodelete").length / 2;
            if (totalTR > 0)
                $("#badge-tot-" + (i == 0 ? "u" : "s")).text(totalTR).show();
            else
                $("#badge-tot-" + (i == 0 ? "u" : "s")).hide();
        }
    }

    if ($("#segnalazioniday-boss").is(":visible")) {
        
        $("#btn_rifiuta_" + IdRichiestaEccezione).addClass("disable");
        $("#btn_approva_" + IdRichiestaEccezione).addClass("disable");
        var trBoss = $("tr[data-idecc-rich='" + IdRichiestaEccezione + "']");
        $(trBoss).css("opacity", "0.2");
    }
}

function DeleteTr(tr) {
    var ndoc = $(tr).attr("data-tr");
    $("#daapprovare").find("tr[data-tr='" + ndoc + "']").each(function () {
        $(this).fadeOut(1000);
    });
    setTimeout(function () {
        $("#daapprovare").find("tr[data-tr='" + ndoc + "']").each(function () {
            $(this).remove();
        });
    }, 1000);

    $("#listaprogrammi").find("tr[data-tr='" + ndoc + "']").each(function () {
        $(this).fadeOut(1000);
    });
    setTimeout(function () {
        $("#listaprogrammi").find("tr[data-tr='" + ndoc + "']").each(function () {
            $(this).remove();
        });
    }, 1000);

    $("#listaeventi").find("tr[data-tr='" + ndoc + "']").each(function () {
        $(this).fadeOut(1000);
    });
    setTimeout(function () {
        $("#listaeventi").find("tr[data-tr='" + ndoc + "']").each(function () {
            $(this).remove();
        });
    }, 1000);
}
 
function CancellaRichiestaDaSegnalazioni(IdRichiestaPadre, confermaSN) {
    $("#segnalazionitoday").addClass("rai-loader");

    //precedente
    // CancellaRichiesta(IdRichiestaPadre, confermaSN);
    
    //nuovo:
    var tr = $("tr[data-idric-parent='" + IdRichiestaPadre + "']");
    var idEccRich = $(tr).attr("data-idecc-rich");

    $.ajax({
        type: 'GET',
        url: "/ajax/getInfoRichiesta",
        dataType: "json",
        data: { IdRichiestaPadre: IdRichiestaPadre },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                if (data.dal == data.al) {
                    CancellaRichiesta(IdRichiestaPadre, confermaSN);
                }
                else {
                    swal({
                        title: 'Sei sicuro?',
                        text: "Il giorno è compreso in una richiesta con periodo " + data.dal + "/" + data.al + ". L'operazione procederà soltanto per il giorno visualizzato e non per tutto il periodo. I restanti giorni verranno modificati e rappresentati ciascuno con una propria richiesta singola.",
                        type: 'warning',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'Procedi',
                        cancelButtonText: 'Annulla'
                    }).then(function () {
                        $.ajax({
                            type: 'GET',
                            url: "/ajax/getRichiestaMultipla",
                            dataType: "json",
                            data: { IdRichiestaPadre: IdRichiestaPadre, IDEccRich: idEccRich },
                            cache: false,
                            success: function (data) {

                                if (data.result == "OK") {
                                    IdRichiestaPadre = data.newID;
                                    CancellaRichiesta(IdRichiestaPadre, confermaSN);
                                }
                                else {
                                    swal('Errore', data.result, 'error');
                                }
                            }
                        });
                    })
                }
            }
            else {
                swal('Errore', data.result, 'error');
            }
        }
    });
}

function CancellaRichiestaInApprovazione(IdRichiestaPadre) {

    swal({
        title: 'Sei sicuro?',
        text: "La richiesta verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla'
    }).then(function () {

        CancellaRichiestaSend(IdRichiestaPadre,'');
    })

}

function CancellaRichiestaInApprovazioneSend(IdRichiestaPadre) {
    var notaCancellazione = "";
    $.ajax({
        type: 'GET',
        url: "/ajax/cancellarichiesta",
        dataType: "json",
        data: { IdRichiestaPadre: IdRichiestaPadre, NotaCancellazione: notaCancellazione },
        cache: false,
        success: function (data) {

            if (data.result == "OK") {
                ForceRefreshMieRichiesteJS();
                ForceRefreshDaApprovareJS();
                ForceRefreshListaProgrammiJS();
                ForceRefreshListaEventiJS();
                ForceRefreshEvidenzeJS();
                swal("Cancellazione effettuata", "La richiesta è stata cancellata", "success");

                //var isVisible = $('#segnalazioniday').is(':visible');
                //if (isVisible) RefreshBoxSegnalazioniInPopupRichieste($("#data_da").val());
            }
            else {
                swal(
                   'Errore',
                   data.result,
                   'error'
                 );
            }
        },
        error: function (a, b, c) { }
    });
}

 


function CheckDaRipianificare(IdRichiestaPadre) {
    $.ajax({
        type: 'GET',
        url: "/pianoferie/isFeLocked",
        dataType: "json",
        data: { id_richiesta: IdRichiestaPadre },
        cache: false,
        success: function (data) {
            if (data.error != null) {
               // swal("Attenzione", data.error, "error");
                swal({
                    title: 'Conferma',
                    text: "La richiesta è parte del piano ferie annuale ed è composta da più giorni. Cliccando 'Procedi' verrà rappresentata come singole richieste, ciascuna con un unico giorno. Successivamente il singolo giorno potrà essere ripianificato.",
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Procedi',
                    cancelButtonText: 'Annulla'
                }).then(function () {
                                        $.ajax({
                                            type: 'GET',
                                            url: "/ajax/getRichiestaMultipla",
                                            dataType: "json",
                                            data: { IdRichiestaPadre: IdRichiestaPadre, IDEccRich: -1 },
                                            cache: false,
                                            success: function (data) {

                                                if (data.result == "OK") {
                                                    location.reload();
                                                }
                                                else {
                                                    swal('Errore', data.result, 'error');
                                                }
                                            }
                                        });
                    });
                return;
            }

            if (data.result == true) {
                CancellaRichiesta_Ripianifica(IdRichiestaPadre);
            }
            if (data.result == false) {
                CancellaRichiesta_NonRipianifica(IdRichiestaPadre);
            }
        }
    });
}

function CancellaRichiesta_Ripianifica(IdRichiestaPadre) {
    $("#ripianifica").modal("show");
    $("#id-richiesta").val(IdRichiestaPadre);

}

function CancellaRichiesta_NonRipianifica(IdRichiestaPadre) {
    $("#motivo-digitato").val("");
    $("#conferma-rifiuto").attr("disabled", "disabled");
    $("#conferma-rifiuto").attr("onclick", "CancellaRichiestaSend(" + IdRichiestaPadre + ",$('#motivo-digitato').val())");
    $("#motivo-digitato").attr("placeholder", "Motivo della cancellazione...");
    $("#titolo").text("Cancella richiesta");
    $('#motivo-rifiuto').modal('show');
}

function CancellaRichiesta(IdRichiestaPadre, confermaSN) {
    CheckDaRipianificare(IdRichiestaPadre);
}

function ForceRefreshAll() {
    $("a[data-refreshurl]").each(function () { $(this).click() });
    $("button[data-refreshurl]").each(function () { $(this).click() });

}
function ForceRefreshEvidenzeJS() {
    $("#button-refresh-evid").click();
}
function ForceRefreshDaApprovareJS() {
    $("#daapprovare button[data-refreshurl]").click();
    $("#daapprovare a[data-refreshurl]").click();
}
function ForceRefreshListaProgrammiJS() {
    $("#listaprogrammi button[data-refreshurl]").click();
    $("#listaprogrammi a[data-refreshurl]").click();
}
function ForceRefreshListaEventiJS() {
    $("#listaeventi button[data-refreshurl]").click();
    $("#listaeventi a[data-refreshurl]").click();
}
function ForceRefreshMieRichiesteJS() {
    $("#mierichieste button[data-refreshurl]").click();
}
function CancellaRichiestaSend(IdRichiestaPadre, notaCancellazione, dataRipian) {
    var buttonCancella = $("tr[data-richparent='" + IdRichiestaPadre + "']").find("button")[0];
    $(buttonCancella).html('<i class="fa fa-refresh fa-spin"></i>').attr("disabled", "disabled");
    $.ajax({
        type: 'GET',
        url: "/ajax/cancellarichiesta",
        dataType: "json",
        data: { IdRichiestaPadre: IdRichiestaPadre, NotaCancellazione: notaCancellazione, dataRipian:dataRipian },
        cache: false,
        complete: function () {
            $(buttonCancella).html('<i class="fa fa-trash text-danger"></i>').removeAttr("disabled");
        },
        success: function (data) {
            if (data.result == "OK") {
                ForceRefreshMieRichiesteJS();
                ForceRefreshDaApprovareJS();
                ForceRefreshListaProgrammiJS();
                ForceRefreshListaEventiJS();
                ForceRefreshEvidenzeJS();
                var isVisible = $('#segnalazioniday').is(':visible');
                if (isVisible) RefreshBoxSegnalazioniInPopupRichieste($("#data_da").val());
                swal('OK', 'Cancellazione confermata', 'info');
                if ($("#dateok").is(":visible")) {
                    $("#dateok").click();
                }
            }
            else {
                swal(
                   'Errore',
                   data.result,
                   'error'
                 );
            }
        },
        error: function (a, b, c) { }
    });
}

function RefreshBoxSegnalazioniInPopupRichieste(date1) {
    $.ajax({
        url: '/ajax/getSegnalazioniAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: date1 },
        success: function (data) {
            var t = $("#segnalazioniday");
            $(t).html(data);
            $("#segnalazionitoday").removeClass("rai-loader");
            UIRai.initblock('#segnalazioniday');
        },
        error: function (a, b, c) {
            alert("Failed 1 " + a + b + c);
        }
    });
}

var ControlliPerEccezione = [];

var EccezioniJsAll = null;
var FerieJsAll = null;

function LeggiEccezioniJS(callback) {

    var date1 = $("#data_da").val();
    EccezioniJsAll = [];
    $.ajax({
        type: 'GET',
        url: "/ajax/geteccezioni?idragg=0" + "&date=" + date1,
        dataType: "json",
        data: {},
        cache: false,
        success: function (data) {

            EccezioniJsAll = data.result;
            FerieJsAll = data.ferie;

            callback();
        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });
}



function AbilitaPagina(page, raggruppamento, nomeraggr) {
    $(".params").addClass("hide");
    $(".params input").attr("disabled", "disabled").val("");
    if (page == 2) {
        $("#wiz-parent").addClass("rai-loader");
        $(".raggr-selez").text(nomeraggr);

        var date1 = $("#data_da").val();

        $.ajax({
            type: 'GET',
            url: "/ajax/geteccezioni?idragg=" + raggruppamento + "&date=" + date1,
            dataType: "json",
            data: {},
            cache: false,
            success: function (data) {
                $("#select-eccezioni").empty();
                $('#select_eccezioni_descAggiuntiva').text('');
                $('#select_eccezioni_descAggiuntiva_Container').hide();
                $("#select-eccezioni").append("<option value='' data-content=''></option>");
                for (var i = 0; i < data.result.length; i++) {
                    var _htmlDescFromJson = data.result[i].desclunga;

                    var option = GetOptionData(data.ferie, data.result[i].cod, data.result[i].desc, "", _htmlDescFromJson);
                    $("#select-eccezioni").append(option);
                    ControlliPerEccezione.push({
                        codice: data.result[i].cod,
                        controlli: data.result[i].controlli,
                        caratteriRichiesti: data.result[i].chars,
                        partial: data.result[i].partial,
                        desclunga: _htmlDescFromJson,
                        richiede_attivita_ceiton: data.result[i].richiede_attivita_ceiton,
                        importo_preimpostato:data.result[i].importo_preimpostato
                    });
                }

                var li_visibili = 1; //PG per tutti

                if (data.ferie.visualizzaFerie == true) {
                    $("#li-fe").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-fe").addClass("hide");

                if (data.ferie.visualizzaFC == true) {
                    $("#li-pf").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-pf").addClass("hide");

                if (data.ferie.visualizzaPermessi == true) {
                    $("#li-pr").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-pr").addClass("hide");

                if (data.ferie.visualizzaPermessiGiornalisti == true) {
                    $("#li-px").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-px").addClass("hide");

                if (data.ferie.visualizzaRecuperoRiposi == true) {
                    $("#li-rr").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-rr").addClass("hide");

                if (data.ferie.visualizzaRecuperoNonLavorati == true) {
                    $("#li-rn").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-rn").addClass("hide");

                if (data.ferie.visualizzaRecuperoFestivi == true) {
                    $("#li-rf").removeClass("hide");
                    li_visibili++;
                }
                else
                    $("#li-rf").addClass("hide");

                var wdt = 100 / li_visibili;
                $(".li-ecc").css("width", wdt + "%");

                var f = parseFloat(data.ferie.ferieRimanenti).toFixed(2);
                $("#tot-ferie").text(f);
                if (data.ferie.ferieRimanenti > 0) {
                    $("#sel-ferie").removeClass("disable"); $("#sel-ferie").attr("aria-disabled", "false");
                    $("#select-eccezioni").attr("data-fe-rimanenti", data.ferie.ferieRimanenti);
                } else { $("#sel-ferie").addClass("disable"); $("#sel-ferie").attr("aria-disabled", "true"); }

                $("#tot-permessi").text(data.ferie.exFestivitaRimanenti);
                if (data.ferie.exFestivitaRimanenti > 0) {
                    $("#sel-permessi").removeClass("disable"); $("#sel-permessi").attr("aria-disabled", "false");

                } else { $("#sel-permessi").addClass("disable"); $("#sel-permessi").attr("aria-disabled", "true") }

                $("#tot-permessi-ret").text(data.ferie.permessiRimanenti);
                if (data.ferie.permessiRimanenti > 0) {
                    $("#select-eccezioni").attr("data-pr-rimanenti", data.ferie.permessiRimanenti);
                    $("#sel-permessi-ret").removeClass("disable"); $("#sel-permessi-ret").attr("aria-disabled", "false")

                } else { $("#sel-permessi-ret").addClass("disable"); $("#sel-permessi-ret").attr("aria-disabled", "true") }

                $("#tot-permessi-giorn").text(data.ferie.permessiGiornalistiRimanenti);
                if (data.ferie.permessiGiornalistiRimanenti > 0) { $("#sel-permessi-giorn").removeClass("disable"); $("#sel-permessi-giorn").attr("aria-disabled", "false") } else { $("#sel-permessi-giorn").addClass("disable"); $("#sel-permessi-giorn").attr("aria-disabled", "true") }

                $("#tot-permessi-rr").text(data.ferie.recuperiMancatiRiposiRimanenti);
                if (data.ferie.recuperiMancatiRiposiRimanenti > 0) { $("#sel-rr").removeClass("disable"); $("#sel-rr").attr("aria-disabled", "false") } else { $("#sel-rr").addClass("disable"); $("#sel-rr").attr("aria-disabled", "true") }

                $("#tot-permessi-rn").text(data.ferie.recuperiNonLavoratiRimanenti);
                if (data.ferie.recuperiNonLavoratiRimanenti > 0) { $("#sel-rn").removeClass("disable"); $("#sel-rn").attr("aria-disabled", "false") } else { $("#sel-rn").addClass("disable"); $("#sel-rn").attr("aria-disabled", "true") }

                $("#tot-permessi-rf").text(data.ferie.recuperiMancatiFestiviRimanenti);
                if (data.ferie.recuperiMancatiFestiviRimanenti > 0) { $("#sel-rf").removeClass("disable"); $("#sel-rf").attr("aria-disabled", "false") } else { $("#sel-rf").addClass("disable"); $("#sel-rf").attr("aria-disabled", "true") }

                if (data.pgq == 1) $("#tot-pg").text("0.5");
                else if (data.pgq == 2) $("#tot-pg").text(".25");
                else $("#tot-pg").text("0");

                $("#select-eccezioni").attr("data-pg-rimanenti", $("#tot-pg").text());

                if (data.pgq == 1 || data.pgq == 2) { $("#sel-pg").removeClass("disable"); $("#sel-pg").attr("aria-disabled", "false") } else { $("#sel-pg").addClass("disable"); $("#sel-pg").attr("aria-disabled", "true") }

                $("#select-eccezioni").select2({
                    templateResult: formatState,
                    placeholder: 'Seleziona eccezione...'
                });

                if (raggruppamento == 1) {
                    $("#feriedetail").removeClass("hide");
                    $("#select-eccezioni").addClass("hide");
                    $(".select2-container").addClass("hide");
                }
                else {
                    $("#feriedetail").addClass("hide");
                    $("#select-eccezioni").removeClass("hide");
                    $(".select2-container").removeClass("hide");
                }

                $('#select-eccezioni').on('select2:open', function (e) {
                    setTimeout(function () {
                        $(".myChart").each(function () {
                            if ($(this).attr("data-valorichart") != null && $.trim($(this).attr("data-valorichart")) != "") {
                                var v1 = $(this).attr("data-valorichart").split(',')[0];
                                var v2 = $(this).attr("data-valorichart").split(',')[1];
                                var myDoughnutChart = new Chart($(this), {
                                    type: 'doughnut',
                                    data: {
                                        labels: [
                                            "Fruite", "Residue"
                                        ],
                                        datasets: [
                                            {
                                                data: [v1, v2],
                                                backgroundColor: [
                                                    "#FF6384",
                                                    "#36A2EB"

                                                ],
                                                hoverBackgroundColor: [
                                                    "#FF6384",
                                                    "#36A2EB"
                                                ]
                                            }]
                                    },
                                    options: {
                                        legend: {
                                            display: false
                                        },
                                        rotation: 1 * Math.PI,
                                        circumference: 1 * Math.PI
                                    }
                                });
                            }
                        });
                    }, 300);
                });
                $("#wiz-parent").removeClass("rai-loader");
                $("#tab2").click();
            },
            error: function (a, b, c) {
                alert(a + b + c);
                $("#wiz-parent").removeClass("rai-loader");
            }
        });
    }
    $("#enabledpage").val(page);
    $("#tab" + page).parent("li").removeClass("disable");
    CheckPage();
}

function GetFerieMezzaGiornataPossibile(datiFerie) {
    if (datiFerie.ferieRimanenti.indexOf(".") > -1 || datiFerie.ferieRimanenti.indexOf(",") > -1)
        return " disabled ";
    else return "";
}

function GetOptionData(datiFerie, codiceEccezione, descEccezione, desclunga, infoDesc) {
    var OptionDisabled = "";
    var Dettagli = "";
    var colore = "";
    var ValoriCharts = "";

    if (codiceEccezione.startsWith("FE")) {
        if (($.trim(codiceEccezione) == "FEM" || $.trim(codiceEccezione) == "FEP") && datiFerie.ferieRimanenti.toString().indexOf(".5") < 0)
            return "";

        if (datiFerie.ferieRimanenti <= 0) {
            colore = "color:#f00";
        }

        ValoriCharts = (datiFerie.ferieSpettanti - datiFerie.ferieRimanenti).toString() + ","
           + datiFerie.ferieRimanenti.toString();

        Dettagli = "<br /><span style='font-size:85%;" + colore + "'>Spettanti:<b>" + datiFerie.ferieSpettanti + "</b>, Anni Prec:<b>" + datiFerie.ferieAnniPrecedenti + "</b>" +
        " ,Fruite:<b>" + datiFerie.ferieUsufruite + "</b>, Residue:<b>" + datiFerie.ferieRimanenti + "</b></span>";
    }

    if (codiceEccezione.startsWith("PF") || codiceEccezione == "PF") {
        if (datiFerie.exFestivitaRimanenti <= 0) {
            colore = "color:#f00";
        }
        Dettagli = "<br /><span style='font-size:85%;" + colore + "'>Spettanti:<b>" + datiFerie.permessiSpettanti + "</b>" +
           " ,Fruiti:<b>" + datiFerie.permessiUsufruiti + "</b>" +
           " ,Pianificati:<b>" + datiFerie.permessiPianificati + "</b>, Residui:<b>" + datiFerie.permessiRimanenti + "</b></span>";

        ValoriCharts = datiFerie.permessiUsufruiti.toString() + "," + datiFerie.permessiRimanenti.toString();
    }

    if (codiceEccezione.startsWith("PR")) {
        var fattore = 1;
        if (codiceEccezione == "PRM" || codiceEccezione == "PRP") fattore = 2;
        if (codiceEccezione == "PRQ") fattore = 4;
        if (($.trim(codiceEccezione) == "PRM" || $.trim(codiceEccezione) == "PRP") && datiFerie.permessiRimanenti < 0.5) return "";

        if ($.trim(codiceEccezione) == "PRQ" && datiFerie.permessiRimanenti < 0.25) return "";
        if (datiFerie.permessiRimanenti <= 0) {
            colore = "color:#f00";
        }
        Dettagli = "<br /><span style='font-size:85%;" + colore + "'>Spettanti:<b>" + datiFerie.permessiSpettanti + "</b>" +
           " ,Fruiti gg:<b>" + datiFerie.permessiUsufruiti + "</b>" +
           " ,Pianificati gg:<b>" + datiFerie.permessiPianificati + "</b>, Residui:<b>" + datiFerie.permessiRimanenti + "</b></span>";
        ValoriCharts = datiFerie.permessiUsufruiti.toString() + "," + datiFerie.permessiRimanenti.toString();
    }

    var option = "<option " + OptionDisabled + " value='" + codiceEccezione + "' data-charts='" + ValoriCharts + "' data-desclunga='" + desclunga + "' data-infodesc='" + infoDesc + "' data-content=\"" + Dettagli + "\">" + descEccezione + "</option>";
    return option;
}

function CheckPage() {
    var currentPage = $("#currentpage").val();
    var enabledPage = $("#enabledpage").val();
    $(".tabhead").addClass("disable");
    for (var i = 0; i <= Number(enabledPage) ; i++) {
        $("#tab" + i).parent().removeClass("disable");
    }
    if (enabledPage > currentPage) {
        $(".wizard-next").removeClass("disable");
    }
    else {
        console.log('prima pagina');
        $("#primoTab").removeClass("active");
        $(".wizard-next").addClass("disable");
    }
    if (currentPage > 1) {
        $(".wizard-prev").removeClass("disable");
    }
    else {
        $(".wizard-prev").addClass("disable");
    }
    if (currentPage == 3) {
        $("#button-invia").show();
        $(".wizard-next").hide();
    }
    else {
        $("#button-invia").hide();
        $(".wizard-next").show();
    }
    if (currentPage == 2) {
        $("#primoTab").addClass("completed");
    }
    if (currentPage == 3) {
        $("#primoTab").addClass("completed");
        $("#secondoTab").addClass("completed");
    }
    if (currentPage == 3) OnChange_notainserimento();

    switch (currentPage) {
        case "1":
            $('#tab1').attr('aria-selected', 'true'); $('#tab1').attr('tabindex', '0');
            $('#tab2').attr('aria-selected', 'false'); $('#tab2').attr('tabindex', '-1');
            $('#tab3').attr('aria-selected', 'false'); $('#tab3').attr('tabindex', '-1');
            break;

        case "2":
            $('#tab1').attr('aria-selected', 'false'); $('#tab1').attr('tabindex', '-1');
            $('#tab2').attr('aria-selected', 'true'); $('#tab2').attr('tabindex', '0');
            $('#tab3').attr('aria-selected', 'false'); $('#tab3').attr('tabindex', '-1');
            break;

        case "3":
            $('#tab1').attr('aria-selected', 'false'); $('#tab1').attr('tabindex', '-1');
            $('#tab2').attr('aria-selected', 'false'); $('#tab2').attr('tabindex', '-1');
            $('#tab3').attr('aria-selected', 'true'); $('#tab3').attr('tabindex', '0');
            break;

        default:
            $('#tab1').attr('aria-selected', 'false'); $('#tab1').attr('tabindex', '0');
            $('#tab2').attr('aria-selected', 'false'); $('#tab2').attr('tabindex', '-1');
            $('#tab3').attr('aria-selected', 'false'); $('#tab3').attr('tabindex', '-1');
    }
}

function CheckPage2Wizard() {

    var currentPage = $("#currentpage").val();
    var NextPage = Number(currentPage) + 1;
    var validated = true;
    $(".enable-next-page:visible").each(function () {
        if ($(this).is("input") || $(this).is("select")) {
            if ($(this).val().length == 0) {
                validated = false;
            }
        }
    });

    if (!validated) {
        $("#enabledpage").val(currentPage);
        $("#tab" + currentPage).parent("li").removeClass("disable");
        CheckPage();
        return;
    }
    else {
        $("#enabledpage").val(NextPage);
        $("#tab" + NextPage).parent("li").removeClass("disable");
        CheckPage();
    }
}

function ClickedTab(index) {
    $("#currentpage").val(index);
    CheckPage();
}

function EmptyGiornataChiusa() {
    $("#giornata-chiusa").html("");
}

function GetGiornataChiusaView() {
    $.ajax({
        url: '/ajax/GetGiornataChiusaView',
        type: "GET",
        dataType: "html",
        data: {},
        async: true,
        success: function (data) {
            $("#giornata-chiusa").html(data);
            $(".giorno-display").text($("#data_da").val());
        },
        error: function (a, b, c) {
            alert("Failed GetGiornataChiusaView " + a + b + c);
        }
    });
}

function GetGiornataChiusaOrarioView() {
    $.ajax({
        url: '/ajax/GetGiornataChiusaOrarioView',
        type: "GET",
        dataType: "html",
        data: {},
        async: true,
        success: function (data) {
            $("#giornata-chiusa").html(data);
            $(".giorno-display").text($("#data_da").val());
        },
        error: function (a, b, c) {
            alert("Failed GetGiornataChiusaOrarioView " + a + b + c);
        }
    });
}

function GetProposteAuto(date, forceProp) {
   
    if (typeof date != "undefined" && date != null && date != "") {
        var date1 = date;
    }
    else {
        var date1 = $("#data_da").val();
    }

    $("#modal-popin .block-themed").addClass("rai-loader");

    $.ajax({
        url: '/ajax/GetProposteAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: date1, forceProposte:forceProp },
        async: true,
        success: function (data) {
            if ($.trim(data) != "") {
                var t = $("#proposteday");
                $(t).html(data);
                $(".giorno-display").text($("#data_da").val());
                $("#proposteday").show();
                UIRai.initblock('#propostetoday');
                $("#button-passa-auto").show();
                $("#wiz-tit").addClass("hide");
                $("#wiz-body").addClass("hide");
            }
            else {
                $("#wiz-tit").removeClass("hide");
                $("#wiz-body").removeClass("hide");
                $("#proposteday").html("");
                $("#button-passa-auto").hide();
            }
            $("#form-inserimento>div>div").removeClass("rai-loader");
        },
        error: function (a, b, c) {
            alert("Failed getprop " + a + b + c);
        }
    });
}

function IsDayClosed(date) {
    if (typeof date != "undefined" && date != null && date != "") {
        var date1 = date;
    }
    else {
        var date1 = $("#data_da").val();
    }

    var D = moment(date1, 'DD/MM/YYYY').toDate();
    var Dconv = moment(DataUltimaConvalida, 'DDMMYYYY').toDate();
    return D < Dconv;
}

function IsDayLockedOrario()
{
    if (moment($("#data_da").val(), "DD/MM/YYYY").isAfter(moment()))
        return false;
    else
    {
        var orario = parseInt($("#cod-orario-reale").val());
        return (orario >= 90 && orario <= 99);
    }
}

function ShowWizard() {
    ShowWizard2();
}

function ShowWizard2() {
    AggiornaGiornoDisplay();
    EmptyGiornataChiusa();
    reinitWizard();
    var date1 = $("#data_da").val();
    var date = $("#data_a").val();

    if (date1 == '01/01/1900')
    {
        var t = $("#infogiornata-content");
        $(t).html('');
        ShowWizard3();
        $("#modal-popin .block-themed").removeClass("rai-loader");
        return;
    }

    $("#modal-popin .block-themed").addClass("rai-loader");
    $.ajax({
        url: '/ajax/getInfoGiornataAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: date1 },
        success: function (data) {
            var t = $("#infogiornata-content");
            $(t).html(data);
            if (date1 != "01/01/1900" && !IsDayClosed() && !IsDayLockedOrario()) {
                GetProposteAuto();
            }
            ShowWizard3();
            $("#modal-popin .block-themed").removeClass("block-opt-refresh");
        },
        error: function (a, b, c) {
            alert("Failed info " + a + b + c);
        }
    });
}

/*------------------------------------*/

function ShowWizard3() {
    var date1 = $("#data_da").val();
    var date = $("#data_a").val();

    if (!IsDateGreaterThanToday(date1) && date1 != "01/01/1900") {
        $("#modal-popin .block-themed").addClass("block-opt-refresh");
        $.ajax({
            url: '/ajax/getTimbratureAjaxView',
            type: "GET",
            dataType: "html",
            data: { date: date1 },
            success: function (data) {
                var t = $("#timbratureday");
                $(t).html(data);
                UIRai.initblock('#timbratureday');

                $.ajax({
                    url: '/ajax/getSegnalazioniAjaxView',
                    type: "GET",
                    dataType: "html",
                    data: { date: date1 },
                    success: function (data) {
                        var t = $("#segnalazioniday");
                        $(t).html(data);
                        $("#modal-popin .block-themed").removeClass("block-opt-refresh");
                        UIRai.initblock('#segnalazioniday');

                        if (IsDayClosed())
                            GetGiornataChiusaView();
                        else {
                            if (IsDayLockedOrario()) {
                                GetGiornataChiusaOrarioView();
                            }
                            else {
                                EmptyGiornataChiusa();
                                GetRaggruppamentiDisponibili();
                            }
                        }
                    },
                    error: function (a, b, c) {
                        alert("Failed 1 " + a + b + c);
                    }
                });


            },
            error: function (a, b, c) {
                alert("Failed 2 " + a + b + c);
            }
        });
    }

    else if (IsDateGreaterThanToday(date1) && date1 != "01/01/1900") {
        $("#modal-popin .block-themed").addClass("block-opt-refresh");
        $.ajax({
            url: '/ajax/getSegnalazioniAjaxView',
            type: "GET",
            dataType: "html",
            data: { date: date1 },
            success: function (data) {
                var t = $("#segnalazioniday");
                $(t).html(data);
                $("#modal-popin .block-themed").removeClass("block-opt-refresh");
                UIRai.initblock('#segnalazioniday');

                GetRaggruppamentiDisponibili();
            },
            error: function (a, b, c) {
                alert("Failed 1 " + a + b + c);
            }
        });
    }
    else {
        GetRaggruppamentiDisponibili();
    }
}


function GoHome() {
    location.href = '/';
}
var AssenzeIngiustificateNow = 0;

function ShowPopupIniziale(msgSeAssenzeIngiustificate, ProvieneDaBoxHome) {
    $("#page-loaderR").show();

    $.ajax({
        url: '/ajax/GetAssenzeIngiustificate',
        type: "GET",
        dataType: "json",
        data: {},
        success: function (data) {

            $("#page-loaderR").hide();
            AssenzeIngiustificateNow = data.result.length;

            if (data.result.length > 0) // se ci sono assenze ingiustificate
            {
                if (ProvieneDaBoxHome) {
                    ShowPopup('', '', data.result[0], data.result[0], true, true);
                }
                else {
                    $('#modal-popin').off('hide.bs.modal');
                    swal({
                        title: 'Nota',
                        type: 'info',
                        html: msgSeAssenzeIngiustificate,
                        showCloseButton: true,
                        showCancelButton: false,
                        confirmButtonText: '<i class="fa fa-thumbs-up"></i> OK'

                    }).then(function () { ShowPopup('', '', data.result[0], data.result[0], true, true); })
                }
            }
            else ShowPopup('', 0);
            //$('#modal-popup-iniziale').modal('show');
        },
        error: function (a, b, c) {
            $("#page-loaderR").hide();
            alert("Failed 1 " + a + b + c);
        }
    });
}

function ShowPopupInizialeGoDate(msgSeAssenzeIngiustificate, ProvieneDaBoxHome, dateToGo) {
    $("#page-loaderR").show();

    $.ajax({
        url: '/ajax/GetAssenzeIngiustificate',
        type: "GET",
        dataType: "json",
        data: {},
        success: function (data) {

            $("#page-loaderR").hide();
            AssenzeIngiustificateNow = data.result.length;

            if (data.result.length > 0) // se ci sono assenze ingiustificate
            {
                if (ProvieneDaBoxHome) {
                    ShowPopup('', '', data.result[0], data.result[0], true, true);
                }
                else {
                    $('#modal-popin').off('hide.bs.modal');
                    swal({
                        title: 'Nota',
                        type: 'info',
                        html: msgSeAssenzeIngiustificate,
                        showCloseButton: true,
                        showCancelButton: false,
                        confirmButtonText: '<i class="fa fa-thumbs-up"></i> OK'

                    }).then(function () { ShowPopup('', '', data.result[0], data.result[0], true, true); })
                }
            }
            else ShowPopup('', '', dateToGo, dateToGo, false, false);
            //$('#modal-popup-iniziale').modal('show');
        },
        error: function (a, b, c) {
            $("#page-loaderR").hide();
            alert("Failed 1 " + a + b + c);
        }
    });
}

function RefreshSegnalazioni(date1) {
    $.ajax({
        url: '/ajax/getSegnalazioniAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: date1 },
        success: function (data) {
            var t = $("#segnalazioniday");
            $(t).html(data);
            $("#modal-popin .block-themed").removeClass("block-opt-refresh");
            UIRai.initblock('#segnalazioniday');
        },
        error: function (a, b, c) {
            alert("Failed 1 " + a + b + c);
        }
    });
}

function GetRaggruppamentiDisponibili() {
    var date1 = $("#data_da").val();
    if (date1 == "01/01/1900") return;
    $("#modal-popin .block-themed").addClass("block-opt-refresh");

    $.ajax({
        url: '/ajax/getRaggruppamentiDisponibili',
        type: "GET",
        dataType: "json",
        data: { date: date1 },
        success: function (data) {

            if (data.result.length == 0) {
                swal({
                    title: 'Attenzione',
                    type: 'warning',
                    html: '<span>Nessuna eccezione possibile per il ' + date1 + ', inserire un\'altra data</span>',
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText:
                      ' OK'
                }).then(function () {
                    setTimeout(ShowPopup('', '', data.result[0], data.result[0], false, false), 2000);
                });
            }
            $(".icone-raggr").addClass("disable");

            for (var i = 0; i < data.result.length; i++) {
                $("#icona-raggr-" + data.result[i]).removeClass("disable");
            }



            $("#modal-popin .block-themed").removeClass("block-opt-refresh");
            if (ClickDaSelezione != 0) $("#icona-raggr-" + ClickDaSelezione).click();

            //if (data.result.length > 1) {
            //    $(".wizard-next").attr("disabled", "disabled");
            //    $("#tab2").attr("disabled", "disabled");
            //    $("#tab3").attr("disabled", "disabled");
            //}
            //else {
            //    $(".wizard-next").removeAttr("disabled");
            //    $("#tab2").removeAttr("disabled");
            //    $("#tab3").removeAttr("disabled");
            //}
        },
        error: function (a, b, c) {
            $("#modal-popin .block-themed").removeClass("block-opt-refresh");
            alert("Failed 3 " + a + b + c);
        }
    });
}
function ScegliDaWizard() {


    $("#proposteday").hide();
    $("#wiz-tit").removeClass("hide");
    $("#wiz-body").removeClass("hide");

    $('#wiz-parent').focus();
}
function ScegliDaProposteAuto() {
    $("#proposteday").show();

    $("#wiz-tit").addClass("hide");
    $("#wiz-body").addClass("hide");

    $('#proposteday').focus();
}

//richiede data in string gg/MM/yyyy
function ConvertToDate(d) {
    var date = d.substring(0, 2);
    var month = d.substring(3, 5);
    var year = d.substring(6, 10);

    var myDate = new Date(year, month - 1, date);
    return myDate;
}

function ConvertToDateFromDDMMYYYY(d) {
    var date = d.substring(0, 2);
    var month = d.substring(2, 4);
    var year = d.substring(4, 8);

    var myDate = new Date(year, month - 1, date);
    return myDate;
}

function DaysDifference(dstart, dend) {
    var start = Math.floor(dstart.getTime() / (3600 * 24 * 1000));
    var end = Math.floor(dend.getTime() / (3600 * 24 * 1000));
    var daysDiff = end - start;
    return daysDiff;
}

function IsDateGreaterThanToday(EnteredDate) {
    var date = EnteredDate.substring(0, 2);
    var month = EnteredDate.substring(3, 5);
    var year = EnteredDate.substring(6, 10);

    var myDate = new Date(year, month - 1, date);

    var today = new Date();

    return (myDate > today);
}



function testCallback() {

    $('#modal-popin').off('hidden.bs.modal');
}



function ShowPopup(PassatoFuturo, IdRaggruppamentoForzato, dataIniziale, dataFinale, dataInizialeLocked, dataFinaleLocked, cb) {

    $("#data_da").datetimepicker("date", null);
    $("#data_a").datetimepicker("date", null);
    $('#modal-popup-iniziale').modal('hide');
    if (dataIniziale != '')
        Fixed_Data_Da = dataIniziale;
    else
        Fixed_Data_Da = null;
    if (dataFinale != '')
        Fixed_Data_A = dataFinale;
    else
        Fixed_Data_A = Fixed_Data_Da;

    PopupPassatoFuturo = PassatoFuturo;
    ClickDaSelezione = IdRaggruppamentoForzato;

    $("#page-loaderR").show();
    setTimeout(function () {
        reinitWizard();
        $('#modal-popin').modal('show');
        $("#page-loaderR").hide();
    }, 1000);
    Disabled_Data_Da = dataInizialeLocked;
    Disabled_Data_A = dataFinaleLocked;
    if (cb)
        PopupCallback = cb;
    else
        PopupCallback = null;
}

function AzzeraVincoliWizard() {
    jQuery('#data_da').data("DateTimePicker").minDate("01/01/1900");
    jQuery('#data_da').data("DateTimePicker").maxDate("01/01/2900");
    ClickDaSelezione = 0;
}

function ChangeMonth(mese, anno) {
    var codSede = $("li.sedesel").attr("data-codsede");

    $("#page-loaderR").show();

    $.ajax({
        url: 'DocumentiDaFirmare/GetMeseAjax',
        type: "GET",
        dataType: "html",
        data: { mese: mese, anno: anno, cod: codSede },
        success: function (data) {
            $("#meseFirma").html(data);
            ChangeMonth2(mese, anno, codSede);
        },
        error: function (a, b, c) {
            $("#page-loaderR").hide();
            alert("Failed 1 " + a + b + c);
        }
    });
}

function ChangeMonth2(mese, anno, codSede) {
    $.ajax({
        url: 'DocumentiDaFirmare/GetGiornateAjax',
        type: "GET",
        dataType: "html",
        data: { mese: mese, anno: anno, cod: codSede },
        success: function (data) {
            $("#giornateFirma").html(data);
            $("#page-loaderR").hide();

            DaysDisponibiliFirma();
        },
        error: function (a, b, c) {
            $("#page-loaderR").hide();
            alert("Failed 2 " + a + b + c);
        }
    });
}

function ChangeSede($sede) {
    $("#page-loaderR").show();
    $('li.sedefirma').removeClass('sedesel');
    $sede.parent('li').addClass('sedesel');
    var anno = $("#annoselez").val();
    var mese = $("#meseselez").val();
    var sede = $sede.parent('li').attr("data-codsede");
    ChangeMonth2(mese, anno, sede);
}


function toggleGiorno(day) {
    var selezionato = $("div[data-day='" + day + "']").hasClass("giornoON");
    var nuovoStato = !selezionato;
    var isFirst = $("div[data-day='" + (day - 1) + "']").length < 1;
    var prevGreen = $("div[data-day='" + (day - 1) + "']").hasClass("giornoON");

    $(".dayfirma").each(function () {
        var divday = Number($(this).attr("data-day"));
        if (divday >= day) {
            if (nuovoStato == false) {
                $(this).addClass("giornoOFF").removeClass("giornoON");
                if (divday > day)
                    $(this).css("cursor", "default");
                else
                    $(this).css("cursor", "pointer");

            }
            else {
                if (isFirst || prevGreen) {
                    $(this).addClass("giornoON").removeClass("giornoOFF");
                    $(this).css("cursor", "pointer");
                }
            }
        }
    });
    DaysDisponibiliFirma();
}



function DaysDisponibiliFirma() {
    var totAbilitati = $(".dayfirma.giornoON").length;
    var tot = $(".dayfirma").length;

    $("#giornisel").text(totAbilitati);
    $("#giornitot").text(tot);
    if (totAbilitati == 0)
        $("#button-vedipdf").attr("disabled", "disabled");
    else
        $("#button-vedipdf").removeAttr("disabled");
}
function ShowPdfFromFirmaPF(idPdf) {
    $.ajax({
        url: '/Firma/getpdfPianoFerie',
        type: "GET",
        dataType: "html",
        data: {  idPdf: idPdf },
        success: function (data) {
            if (data.indexOf("*") == 0) {
                swal({
                    title: 'Errore',
                    type: 'error',
                    html: '<div style="overflow:scroll">' + data + '</div>',
                    showCloseButton: true,
                    confirmButtonText: ' OK'
                });
            }
            else {
                $("#pdf-modal-pf").modal("show");
                $("#pdfcontent-pf").html(data);

                UpdateButtonPF(idPdf);
                UpdateTotaliCarrello();
            }
        },
        error: function (a, b, c) {
            swal('Errore', a + b + c, 'error');
        }
    });
}

function UpdateButtonPF(idPdf) {
    var codSede = $("tr[data-idpdf=" + idPdf + "]").attr("data-sede");
    var descSede = $("tr[data-idpdf=" + idPdf + "]").attr("data-descsede");
    var descRep = $("tr[data-idpdf=" + idPdf + "]").attr("data-descrep");
    $("#int1").html(codSede + " - " + descSede);
    if (descRep != null && descRep.trim() != "") $("#int2").html(descRep);

    var rows = [];
    $("tr[data-ispf]").each(function () { rows.push($(this).attr("data-idpdf")) });
    var tot = rows.length;
    var prog = rows.indexOf(idPdf.toString());
    $("#int3").html((prog + 1).toString() + "/" + tot);

    if (prog == 0)
        $("#pdfprev-pf").addClass("disable");
    else
        $("#pdfprev-pf").removeClass("disable");

    if (prog + 1 == tot)
        $("#pdfnext-pf").addClass("disable");
    else
        $("#pdfnext-pf").removeClass("disable");
}

function ShowPdfFromFirma(idPdf, dateStart, dateEnd, dataPub, nota, sede) {
    $.ajax({
        beforeSend: function () { $("#page-loaderR").show(); },
        complete: function () { $("#page-loaderR").hide(); },
        url: '/Firma/getpdf',
        type: "GET",
        dataType: "html",
        data: { datainizio: dateStart, datafine: dateEnd, codsede: sede, idPdf: idPdf, datapubblicazione: dataPub, nota: nota },
        success: function (data) {
            if (data.indexOf("*") == 0) {
                swal({
                    title: 'Errore',
                    type: 'error',
                    html: '<div style="overflow:scroll">' + data + '</div>',
                    showCloseButton: true,
                    confirmButtonText: ' OK'
                });
            }
            else {
                $("#pdf-modal").modal("show");
                $("#pdfcontent").html(data);
            }
        },
        error: function (a, b, c) {
            swal('Errore', a + b + c, 'error');
        }
    });
}

function ShowPdf() {

    var highest = 0;
    var lowest = 100;
    $(".dayfirma").each(function () {
        if ($(this).hasClass("giornoON") == false) return;
        highest = Math.max(highest, parseInt($(this).attr("data-day")));
        lowest = Math.min(lowest, parseInt($(this).attr("data-day")));
    });
    if (highest < 10) highest = "0" + highest;
    if (lowest < 10) lowest = "0" + lowest;

    var anno = $("#annoselez").val();
    var mese = $("#meseselez").val();
    if (mese < 10) mese = "0" + mese;

    var codSede = $("li.sedesel").attr("data-codsede");

    $.ajax({
        beforeSend: function () { $("#page-loaderR").show(); },
        complete: function () { $("#page-loaderR").hide(); },
        url: 'DocumentiDaFirmare/getpdf',
        type: "GET",
        dataType: "html",
        data: { datainizio: lowest + mese + anno, datafine: highest + mese + anno, codsede: codSede },
        success: function (data) {
            if (data.indexOf("*") == 0) {
                swal({
                    title: 'Errore',
                    type: 'error',
                    html: '<div style="overflow:scroll">' + data + '</div>',
                    showCloseButton: true,
                    confirmButtonText: ' OK'
                });
            }
            else {
                $("#pdf-modal").modal("show");
                $("#pdfcontent").html(data);
            }
        },
        error: function (a, b, c) {
            swal('Errore', a + b + c, 'error');
        }
    });
}

function ShowEccezioniGiornata(data) {
    var sede = $(".sedesel").attr("data-codsede");
    var eticsede = $.trim($(".sedesel").text());

    $.ajax({
        beforeSend: function () { $("#page-loaderR").show(); },
        complete: function () { $("#page-loaderR").hide(); },
        url: 'DocumentiDaFirmare/geteccezioniGiornata',
        type: "GET",
        dataType: "html",
        data: { giorno: data, eticsede: eticsede, codsede: sede },
        success: function (data) {
            if (data.indexOf("*") == 0) {
                swal({
                    title: 'Errore',
                    type: 'error',
                    html: '<div style="overflow:scroll">' + data + '</div>',
                    showCloseButton: true,
                    confirmButtonText: ' OK'
                });
            }
            else {
                $("#ecc-cont").html(data);
                $("#modal-eccgio").modal("show");
            }
        },
        error: function (a, b, c) {
            swal('Errore', a + b + c, 'error');
        }
    });
}

function SelTutti( element, sede )
{
    var v = $(element).prop("checked");
    $(element).closest("div.tab-pane").find(".seltr." + $.trim(sede)).prop("checked", v);
    $(element).closest("div.tab-pane").find(".seltr." + $.trim(sede)).change();
}

function SelTutteSediGapp( element )
{
    var v = $( element ).prop( "checked" );
    $( 'input[class*="seltr"][data-selezionatutte="1"]' ).prop( "checked", v );
    $( 'input[class*="seltr"][data-selezionatutte="1"]' ).change();
}

function SelTr(element) {
    var v = $(element).prop("checked");
    if (v) {
        $(element).closest("tr").find(".button-approva").addClass("disable");
        $(element).closest("tr").find(".button-rifiuta").addClass("disable");
    }
    else {
        $(element).closest("tr").find(".button-approva").removeClass("disable");
        $(element).closest("tr").find(".button-rifiuta").removeClass("disable");
    }

    var q = $(element).closest("div.tab-pane").find("input[type=checkbox].seltr:checked").length;
    if (q > 0) $(element).closest("div.tab-pane").find(".divall").show();
    else $(element).closest("div.tab-pane").find(".divall").hide();
}

function OnClick_buttonApprovaTutti(button, validazione) {
   
    var q = $(button).closest("div.tab-pane").find("input[type=checkbox].seltr:checked").length;

    if (validazione == true) {
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#testoprogress").hide();
        $("#titolo1").text("Validazione richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di validare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di validare la richiesta selezionata?");
    }
    else {
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#testoprogress").show();
        $("#titolo1").text("Rifiuto richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di rifiutare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di rifiutare la richiesta selezionata?");
    }

    RunningSerie = false;
    AbortSerie = false;
    $("#testoprogress").val("");
    $('#valida-rifiuta-tutti').modal('show');
    $("#btn-conf-tutti").show();
    $("#btn-ann").show();
    $("#btn-chiudi").hide();
    $("#rich-table").empty();
    $(".progress-bar").css("width", "0%");
    var items = [];

    $(button).closest("div.tab-pane").find("input[type=checkbox].seltr:checked").each(function () {
        var tr = $(this).closest("tr");
        var td2 = $(tr).find("td:nth-child(2)");
        var eccez = $.trim($(td2).clone().children().remove().end().text());

        var td3 = $(tr).find("td:nth-child(3)");
        var nome = $.trim($(td3).find("a").text());

        var text = $(tr).attr("data-eccez-data");

        var idecc = $(tr).attr("data-ideccezione");
        items.push({ eccez: eccez, nome: nome, data: text, id: idecc });

    });
    //for (var i = 0; i < items.length; i++) {
    //    var g = "";
    //    if (i % 2 == 0) g = " class='grayed' ";
    //    $("#rich-table").append("<tr data-id-ec='" + items[i].id + "'" + g + ">" +
    //            "<td style='width:15%'>" + items[i].data + "</td><td><span class='text-info'>" + items[i].eccez + "</span><br /><a><b>" + items[i].nome + "</b></a></td>" +
    //            "<td></td>" +
    //        "</tr>");
    //}
    for (var i = 0; i < items.length; i++) {
        var tr = $("tr[data-ideccezione='" + items[i].id + "']:visible").clone();
        $(tr).find("td:first-child").remove();
        $(tr).find('td:eq(3)').html("");
        $(tr).attr("data-id-ec", items[i].id);
        $("#rich-table").append(tr);
    }
}

//variabile globale per interruzione validazione in serie
var AbortSerie = false;
var RunningSerie = false;

function EseguiApprovaTutti(RequestedId) {
    RunningSerie = true;
    $("#btn-conf-tutti").attr("disabled", "disabled");

    if (RequestedId == null) RequestedId = $("#rich-table").children("tbody").children('tr:first').attr("data-id-ec");
    var nextId = $("#rich-table tr[data-id-ec='" + RequestedId + "']").next("tr").attr("data-id-ec");

    //indice e pos della tr
    var ind = $("#rich-table tr[data-id-ec='" + RequestedId + "']").index();
    var pos = $("#rich-table tr[data-id-ec='" + RequestedId + "']").height() * ind;
    var visibleYfrom = $("#rich-cont").scrollTop();
    var visibleYto = visibleYfrom + $("#rich-cont").height();
    if (pos + $("#rich-table tr[data-id-ec='" + RequestedId + "']").height() > visibleYto) $("#rich-cont").scrollTop(visibleYfrom + 100);
    var td = $("#rich-table tr[data-id-ec='" + RequestedId + "'] td:nth-child(4)");
    $(td).html("<i class='text-danger fa fa-2x fa-spin fa-refresh'></i>");

    if (AbortSerie == false)
        ApprovaEccezioneDaSerie(RequestedId, $("#testoprogress").val(), $(td), nextId, ind);
    else {
        AbortSerie = false;// resetta per prox volta
        RunningSerie = false;
    }
}

function ApprovaEccezioneDaSerie(idRichiestaEccezione, NotaSuApprovazione, TDinprogress, nextId, indice) {

    var IsValidazione = $("#btn-conf-tutti").attr("data-validaTF") == "true";
    var esito = "Validata";

    var url = "/ajax/validaeccezione?id=" + idRichiestaEccezione + "&nota=" + NotaSuApprovazione;
    if (IsValidazione == false) {
        url = "/ajax/rifiutaeccezione?id=" + idRichiestaEccezione + "&nota=" + NotaSuApprovazione;
        esito = "Rifiutata";
    }
    $.ajax({
        type: 'GET',
        url: url,
        dataType: "json",
        data: {},
        cache: false,
        success: function (data) {
            var totalTR = $("#rich-table TR").length;
            var perc = ((indice + 1) / totalTR) * 100;
            var perc1 = (1 / totalTR) * 100;

            if (data.result == "OK") {
                ValidaTR(idRichiestaEccezione);
                //  $(TDinprogress).html("<i class='text-success fa fa-check'></i>" + esito);
                $(".progress").append("<div class='progress-bar progress-bar-success' style='width: " + perc1 + "%'></div>");
            }
            else {
                RifiutaTR(idRichiestaEccezione, data.result);
                // $(TDinprogress).html("<i class='text-danger fa fa-remove'></i>" + data.result);
                $(".progress").append("<div class='progress-bar progress-bar-danger' style='width: " + perc1 + "%'></div>");
            }

            if (nextId !== undefined) {
                EseguiApprovaTutti(nextId);
            }
            else {
                $('[data-toggle="tooltip"]').tooltip();
                ForceRefreshDaApprovareJS();
                ForceRefreshListaProgrammiJS();
                ForceRefreshListaEventiJS();
                $("#btn-chiudi").show();
                $("#btn-ann").hide();
                $("#btn-conf-tutti").hide();
                AbortSerie = false;
                RunningSerie = false;
            }
        },
        error: function (a, b, c) { }
    });
}
function ValidaTR(idre) {
    var tr = $("#rich-table").find("tr[data-ideccezione='" + idre + "']");
    $(tr).find('td:eq(3)').html("<i  class='icons icon-check fa-3x' style='color:#0f0'></i>");
}
function RifiutaTR(idre, causa) {
    var tr = $("#rich-table").find("tr[data-ideccezione='" + idre + "']");
    $(tr).find('td:eq(3)').html("<i data-toggle='tooltip' title='" + causa + "' class='icons icon-close fa-3x' style='color:#f00'></i>");
}

function AbortTutti() {
    if (RunningSerie) {
        swal({
            title: 'Interrompi l\'operazione in corso?',
            type: 'warning',
            html:
              ' ',
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: 'Continua',
            confirmButtonText: 'Interrompi'
        }).then(function () {

            AbortSerie = true;
            RunningSerie = false;
            ForceRefreshDaApprovareJS();
            ForceRefreshListaProgrammiJS();
            ForceRefreshListaEventiJS();
            $("#valida-rifiuta-tutti").modal('hide');
        });
    }
    else {

        $("#valida-rifiuta-tutti").modal('hide');
    }
}

function formatState(state) {

    var riga2 = "";
    if (state.element != null && state.element.attributes != null && state.element.attributes["data-content"] != null)
        riga2 = state.element.attributes["data-content"].value;

    var valoricharts = "";
    if (state.element != null && state.element.attributes != null && state.element.attributes["data-charts"] != null) {
        valoricharts = state.element.attributes["data-charts"].value;

    }

    var riga3 = "";
    if (state.element != null && state.element.attributes != null && state.element.attributes["data-desclunga"] != null && $.trim(state.element.attributes["data-desclunga"].value) != "null") {

        if (valoricharts != "") {
            riga3 = "<table style='width:100%'><tr><td class='ecc-td1'>" + state.element.attributes["data-desclunga"].value + "</td>" +
               "<td class='ecc-td2'><div class='graph-div'><canvas data-valorichart='" + valoricharts + "' class='myChart' ></canvas>" +
               "<div class='graph-label'><span>"
               +
               (valoricharts != "" ? valoricharts.split(",")[1] : "")
               + "</span></div>" +
               "</div></td></tr></table>";
        }
        else {
            riga3 = "<table style='width:100%'><tr><td style='width:100%'>" + state.element.attributes["data-desclunga"].value + "</td>" +
            "</tr></table>";
        }
    }

    var $state = $(
       '<span>' + state.text + '</span>' + riga2 + riga3
     );
    return $state;
};

function GeneralSearch() {
    $("#idEccLoader>.block").addClass("block-opt-refresh");
    var nominativo = $("#RicercaNominativo").val();
    var sede = $("#ddlSedi").val();
    var stato = $("#ddlStato").val();
    var dal = $("#dalGiorno").val();
    var al = $("#alGiorno").val();
    $.ajax({
        type: 'GET',
        url: "/ajax/RicercaGenerale",
        dataType: "html",
        data: { nominativo: nominativo, sede: sede, stato: stato, dal: dal, al: al },
        cache: false,
        success: function (data) {
            $('#idEccezioniRicerca').empty();
            $('#idEccezioniRicerca').append(data);
        },
        error: function (a) {
            a = "a";
        },
        complete: function () {
            $("#idEccLoader>.block").removeClass("block-opt-refresh");
        }
    });
    return false;
}

function GeneralUserSearch() {
    $("#idEccLoader>.block").addClass("block-opt-refresh");

    var stato = $("#ddlStato").val();
    var tipoEcc = $("#ddlTipiEcc").val();
    var dal = $("#dalGiorno").val();
    var al = $("#alGiorno").val();
    $.ajax({
        type: 'GET',
        url: "/ajax/RicercaGeneraleUtente",
        dataType: "html",
        data: { stato: stato, tipoEcc: tipoEcc, dal: dal, al: al },
        cache: false,
        success: function (data) {
            $('#idEccezioniRicerca').empty();
            $('#idEccezioniRicerca').append(data);
        },
        error: function (a) {
            a = "a";
        },
        complete: function () {
            $("#idEccLoader>.block").removeClass("block-opt-refresh");
        }
    });
    return false;
}


function CancellaNotifica(id, tr, table, a) {
    $(a).html('<i class="fa fa-spinner fa-spin"></i>');

    $.ajax({
        type: 'GET',
        url: "/scrivania/cancellanotifica",
        dataType: "json",
        data: { id: id },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                UpdateNotifiche(data.totalNow, data.totalNow1);
                $(tr).remove();
                if ($(table).find("tr").length == 0) {
                    $(table).removeClass("table")
                        .removeClass("table-striped")
                        .append(
                        '<tr>' +
                            '<td style="padding:15px" class="text-center" colspan="2"><span>NON CI SONO DATI DA VISUALIZZARE</span></td>' +
                        '</tr>');
                }
            }
            else {
                $(a).text('x');
                swal(data.result, '', 'error');
            }
        },
        error: function (aa) {
            $(a).text('x');
        },
        complete: function () {

        }
    });
}
function AbilitaRadioKeyDown(event, tipo) {
    if (event.keyCode == 13 || event.keyCode == 32) {
        event.preventDefault();
        AbilitaRadio(tipo);
    }
}
function AbilitaRadio(tipo) {
    DisabilitaProsegui();
    DisablilitaRadio();
    $('#radio-cont').show();
    DisabilitaTipi();

    if (tipo == "PG") {
        $("#sel-pg").addClass("tiposel");
        if ($("#tot-pg").text() == "0.5" || $("#tot-pg").text() == ".25") {
            $("#ma12").removeAttr("disabled");
        }
        if ($("#tot-pg").text() == "0.5") {
            if (SelectContains("PGM")) {
                $("#matt").removeAttr("disabled");
            }
            if (SelectContains("PGP")) {
                $("#pome").removeAttr("disabled");
            }
        }
    }
    if (tipo == "PX") {
        $("#sel-permessi-giorn").addClass("tiposel");
        if (SelectContains("PX")) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("PXM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("PXP")) {
            $("#pome").removeAttr("disabled");
        }
    }
    if (tipo == "RR") {
        $("#sel-rr").addClass("tiposel");
        //var disp = Number($("#tot-permessi-rr").text());
        //if (disp >=1)
        //{
        //    $("#gior").removeAttr("disabled");
        //}
        //if (disp >= 0.5) {
        //    $("#matt").removeAttr("disabled");
        //    $("#pome").removeAttr("disabled");
        //}
        if (SelectContains("RR")) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("RRM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("RRP")) {
            $("#pome").removeAttr("disabled");
        }
    }
    if (tipo == "RN") {
        $("#sel-rn").addClass("tiposel");
        if (SelectContains("RN")) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("RNM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("RNP")) {
            $("#pome").removeAttr("disabled");
        }
    }
    if (tipo == "RF") {
        $("#sel-rf").addClass("tiposel");

        var gg = parseFloat($("#tot-permessi-rf").text());

        if (gg>=1) {
            $("#gior").removeAttr("disabled");
        }
        if (gg>=0.5) {
            $("#matt").removeAttr("disabled");
            $("#pome").removeAttr("disabled");
        }
        //if (SelectContains("RFP")) {
        //    $("#pome").removeAttr("disabled");
        //}
    }



    if (tipo == "FE") {
        $("#sel-ferie").addClass("tiposel");

        if (SelectContains("FE")) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("FEM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("FEP")) {
            $("#pome").removeAttr("disabled");
        }
    }

    if (tipo == "PF") {
        $("#sel-permessi").addClass("tiposel");

        if (SelectContains("PF")) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("PFM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("PFP")) {
            $("#pome").removeAttr("disabled");
        }
        if (SelectContains("PFQ")) {
            $("#ma12").removeAttr("disabled");
            $("#ma22").removeAttr("disabled");
            $("#po12").removeAttr("disabled");
            $("#po22").removeAttr("disabled");
        }
    }
    if (tipo == "PR") {
        $("#sel-permessi-ret").addClass("tiposel");
        if (SelectContains("PR") && $("#select-eccezioni").attr("data-pr-rimanenti")>0.5 ) {
            $("#gior").removeAttr("disabled");
        }
        if (SelectContains("PRM")) {
            $("#matt").removeAttr("disabled");
        }
        if (SelectContains("PRP")) {
            $("#pome").removeAttr("disabled");
        }
        if (SelectContains("PRQ")) {
            $("#ma12").removeAttr("disabled");
            $("#ma22").removeAttr("disabled");
            $("#po12").removeAttr("disabled");
            $("#po22").removeAttr("disabled");
        }
    }

    $('#radio-cont input:enabled:first').focus();
}

function DisabilitaProsegui() {
    $(".wizard-next").addClass("disable");
}

function DisabilitaTipi() {
    $("#sel-ferie").removeClass("tiposel");
    $("#sel-permessi").removeClass("tiposel");
    $("#sel-permessi-ret").removeClass("tiposel");
    $("#sel-permessi-giorn").removeClass("tiposel");

    $("#sel-pg").removeClass("tiposel");
    $("#sel-rr").removeClass("tiposel");
    $("#sel-rn").removeClass("tiposel");
    $("#sel-rf").removeClass("tiposel");
}

function DisablilitaRadio() {
    $('#radio-cont input:radio').each(function () {
        $(this).attr("disabled", "disabled");
        $(this).prop("checked", false);
    });
    $('#radio-cont').hide();
}

function SelectContains(eccez) {
    var exists = false;
    $('#select-eccezioni option').each(function () {
        if (this.value == eccez) {
            exists = true;
            return false;
        }
    });
    return exists;
}

function ForceSelect(r) {
    if ($(r).prop("checked") == true) {
        var eccez = "";
        if ($("#sel-ferie").hasClass("tiposel")) {
            eccez = "FE";
        }
        if ($("#sel-permessi").hasClass("tiposel")) {
            eccez = "PF";
        }
        if ($("#sel-permessi-ret").hasClass("tiposel")) {
            eccez = "PR";
        }
        if ($("#sel-pg").hasClass("tiposel")) {
            eccez = "PG";
        }
        if ($("#sel-permessi-giorn").hasClass("tiposel")) {
            eccez = "PX";
        }
        if ($("#sel-rr").hasClass("tiposel")) {
            eccez = "RR";
        }
        if ($("#sel-rn").hasClass("tiposel")) {
            eccez = "RN";
        }
        if ($("#sel-rf").hasClass("tiposel")) {
            eccez = "RF";
        }

        if ($(r).val() == "matt") {
            eccez += "M";
        }
        if ($(r).val() == "pome") {
            eccez += "P";
        }
        if ($(r).val().indexOf("2") > 0) {
            eccez += "Q";
        }
        if (eccez.indexOf("RF") == 0 && ! SelectContains(eccez)) {
            $("#select-eccezioni").append("<option value='"+eccez+"'>"+eccez+"</option>")
        }
        $("#select-eccezioni").val(eccez);
        $("#select-eccezioni").change();
    }
}

function Info(help) {
    $.ajax({
        url: "/Guida/Index",
        type: "GET",
        data: { viewhelp: help },
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log(data);
            $('#modal-help').html(data);
            $('#modal-help').modal("show");
        },
        error: function (a, b, c) { alert(a + b + c); }
    })
}

function RefreshDettSett() {
    $.ajax({
        url: "/Home/RefreshDettSettWidget",
        type: "GET",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            $('#dettagliosettimanale').html(data);
        }
    })
}

function RefreshDettSett2() {
    DettSettimanaleScorri('c');
}

function cambiaGiorno(dir) {
    var labdata = document.getElementById("labdata");
    var str = labdata.attributes["data-date"].value;
    if (dir == "oggi") {
        var nuovadata = moment().format('L').toString();
    }
    else {
        var datam = moment(str, "DD/MM/YYYY", "it");
        var nuovadata = moment(datam.add(dir, 'd')).format('L').toString();
    }
    $.ajax({
        url: '/ajax/getTimbratureAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: nuovadata, nocalendar: false },
        success: function (data) {
            var t = $("#sectimb");
            $(t).html(data);
        }
    });
}

function AggiornaTotRichieste() {
    $(".tab-filtro:visible").find("tbody.tbodydata").each(function () {
        var trtot = $(this).find("tr.trdata").not(".trtodelete").length / 2;
        var ric = " richiesta";
        if (trtot != 1) ric = " richieste";
        $(this).prev("tbody").find(".totale-rich").text(trtot + ric);
    })
}

/*
*   dir:            Quantità che dovrà essere aggiunta o sottratta. Es +1 , -1
*   target:         identificativo dell'elemento html che riceverà la view in risposta al metodo ajax
*   url:            url del metodo da chiamare
*   onSuccess:      Callback da lanciare onSuccess della chiamata ajax
*   onFailure:      Callback da lanciare onFailure della chiamata ajax
*   onCompleted:    Callback da lanciare onCompleted della chiamata ajax
*/
function cambiaMese(dir, target, url, onSuccess, onFailure, onCompleted) {
    var labdata = document.getElementById("labdata");
    var str = labdata.attributes["data-date"].value;
    var datam = moment(str, "DD/MM/YYYY", "it");
    var nuovadata = moment(datam.add(dir, 'M')).format('L').toString();

    $.ajax({
        url: url,
        type: "GET",
        dataType: "html",
        data: { date: nuovadata },
        success: function (data) {
            var t = $("#" + target);
            $(t).html(data);
            if (typeof onSuccess !== "undefined" &&
                onSuccess != null &&
                $.trim(onSuccess).length > 0) {
                onSuccess();
            }
        },
        error: function (errorInfo) {
            if (typeof onFailure !== "undefined" &&
                onFailure != null &&
                $.trim(onFailure).length > 0) {
                onFailure();
            }
        },
        complete: function () {
            if (typeof onCompleted !== "undefined" &&
                onCompleted != null &&
                $.trim(onCompleted).length > 0) {
                onCompleted();
            }
        }
    });
}

//change theme
function changecss(cssFile) {
    console.log(cssFile);
    $("#savedb").show();
    $.ajax({
        type: 'GET',
        url: "/ProfiloPersonale/TemaUpdate",
        dataType: "json",
        data: { nuovoTema: cssFile },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                var head = document.getElementsByTagName('head')[0];
                var link = document.createElement('link');

                link.rel = 'stylesheet';
                link.type = 'text/css';
                link.href = '/assets/stylesheets/skins/' + cssFile + '.css';
                link.media = 'all';
                head.appendChild(link);
                localStorage.setItem("tema", cssFile);

            }
            else {
                swal(data.result, '', 'error');
            }

            $("#savedb").fadeOut("slow", function () {
            });
        },
        error: function (aa, b, c) {
            $("#savedb").fadeOut("slow", function () {
            });
            swal(aa + b + c, '', 'error');
        },
        complete: function () {
            $("#savedb").fadeOut("slow", function () {
            });
        }
    });
}

(function ($) {

    //re-set all client validation given a jQuery selected form or child
    $.fn.resetValidation = function () {

        var $form = this.closest('form');

        //reset jQuery Validate's internals
        $form.validate().resetForm();

        //reset unobtrusive validation summary, if it exists
        $form.find("[data-valmsg-summary=true]")
            .removeClass("validation-summary-errors")
            .addClass("validation-summary-valid")
            .find("ul").empty();

        //reset unobtrusive field level, if it exists
        $form.find("[data-valmsg-replace]")
            .removeClass("field-validation-error")
            .addClass("field-validation-valid")
            .empty();

        return $form;
    };

    //reset a form given a jQuery selected form or a child
    //by default validation is also reset
    $.fn.formReset = function (resetValidation) {
        var $form = this.closest('form');

        $form[0].reset();

        if (resetValidation == undefined || resetValidation) {
            $form.resetValidation();
        }

        return $form;
    }
})(jQuery);

function controlRadioMaggPres(elem, radio) {
}

function differenzaMinuti(dt2, dt1) {

    var diff = (dt2.getTime() - dt1.getTime()) / 1000;
    diff /= 60;
    return Math.abs(Math.round(diff));

}

function ex(url, title) {
    $("#external-url").modal("show");
    $("#title-url").text(title);

    var h = $("#external-url").height();

    $("#iframe-external").attr("src", url);
    $("#iframe-external").css("height", h + "px");
}

function ActionApprovaEccezione(id) {
  
    var id = id;
    var btn = $('.remoteclick.btn-app-' + id);
    
        $(btn)[0].click();
}

function ActionRifiutaEccezione(id) {
    var id = id;
    var btn = $('.remoteclick.btn-rif-' + id);
    $(btn)[0].click();
}
function getMaxMinutiEccezioni(ecc) {
    for (var i = 0; i < arrayEccezioni.length; i++) {
        if (arrayEccezioni[i].nome == ecc)
            return arrayEccezioni[i].maxminuti;
    }
    return 0;
}
function GetMinutiCoperti() {
    var minutiCoperti = 0;
    $(".ecc-cop").each(function () {
        if ($(this).val() != "") {
            var opt = $(this);

            minutiCoperti += Number($(this).attr("data-minuti"));
        }
    });
    return minutiCoperti;
}
function CheckMinutiEcc(carGiornata, selectclicked) {
    var eccezioneScelta = $(selectclicked).find("option:selected").text();
    var maxMinutiPerEccezione = getMaxMinutiEccezioni(eccezioneScelta);
    var totMinutiEccezione = 0;
    var quantiURH = 0;

    $(".ecc-cop").each(function () {
        if ($(this).find("option:selected").text() == eccezioneScelta) {
            totMinutiEccezione += Number($(this).attr("data-minuti"));
        }
        if ($(this).find("option:selected").text() == "URH") {
            quantiURH++;
        }
    });
    if (quantiURH > 1)
    {
        swal('URH può essere selezionato una sola volta', '', 'error');
        $(selectclicked).val("");
        return;
    }
    if (maxMinutiPerEccezione > 0 && totMinutiEccezione > maxMinutiPerEccezione) {
        swal('Superato limite per eccezione ' + eccezioneScelta + ' (max ' + maxMinutiPerEccezione + ' min)', '', 'error');
        $(selectclicked).val("");
    }

    var minutiCoperti = GetMinutiCoperti();

    var diff = carGiornata - minutiCoperti;
    $(".ecc-cop").each(function () {
        if ($(this).val() == "") {
            var min = Number($(this).attr("data-minuti"));
            if (min > diff)
                $(this).attr("disabled", "disabled");
            else
                $(this).removeAttr("disabled");
        }
    });
    if (minutiCoperti > 0)
        $("#button-conferma-timbra").removeAttr("disabled");
    else
        $("#button-conferma-timbra").attr("disabled", "disabled");
}
function part2() {
    $("#part2").show();
    $("#table-ecc-auto").hide();

    var ecc = [];
    $(".ecc-cop").each(function () {

        if ($(this).val() != "") {
            if (ecc.indexOf($(this).val()) == -1)
                ecc.push($(this).val());
        }
    });


    for (var i = 0; i < ecc.length; i++) {

        var minuti = 0;
        var timbrature = "";
        $(".ecc-cop").each(function () {
            if ($(this).val() == ecc[i]) {
                minuti += Number($(this).attr("data-minuti"));
                timbrature += $(this).attr("data-intervallo") + ",";
            }
        } );

        var txNota = "* facoltativo";

        if ( ecc[i] == "SEH" )
            txNota = "* obbligatorio";

        $('<tr class="trecc"><td>' +
            '<input type="hidden" class="intervallo-ecc' + i + '" value="' + timbrature + '" />' +
            '<span class="nome-ecc' + i + '"><b>' + ecc[i] + '</b></span></td><td><span class="durata-ecc' + i + '"><b>' + minuti + '</b></span><span><b>&nbsp;min</b></span></td></tr>' +
             '<tr><td colspan="2"><div id="view-' + i + '"></div></td></tr>' +
            '<tr class="trnote"><td colspan="2"><em>Nota: ' + txNota +' </em>' +
            '<textarea onkeyup="checkNoteEcc()" data-car-obb="' + carobb(ecc[i]) + '" class="nota-per-ecc nota-ecc' + i + '" rows="3" style="width:100%"></textarea> </td></tr>'
            ).insertBefore('#part2 > tbody > tr:first');

        $("#view-" + i).load("/ajax/getpartial?nomePartial=" + ecc[i] + ".cshtml");
    }
    checkNoteEcc();
}
function checkNoteEcc() {
    var carSufficienti = true;
    $(".nota-per-ecc").each(function () {
        if ($(this).val().length < $(this).attr("data-car-obb"))
            carSufficienti = false;

    });
    if (carSufficienti)
        $("#button-conferma-timbra2").removeClass("disable");
    else
        $("#button-conferma-timbra2").addClass("disable");
}
function carobb(ecc) {
    for (var i = 0; i < arrayEccezioni.length; i++) {
        if (arrayEccezioni[i].nome == ecc)
            return arrayEccezioni[i].caratteriObbligatori;
    }
    return 0;
}
function part1() {
    $("#part2").find("tr.trecc").remove();
    $("#part2").find("tr.trnote").remove();
    $("#part2").hide();
    $("#table-ecc-auto").show();
}

function changeApprView(button) {
    var toShow = $(button).attr('data-view-show');
    var toHide = $(button).attr('data-view-hide');
    var visibleText = $(button).text();

    toHide.split(',').forEach(function (item) {
        $('#' + item).hide();
    });
    $('#' + toShow).show();

    $('#viewButton').text(visibleText);
}
function changeApprView2(button) {
    var toShow = $(button).attr('data-view-show');
    var toHide = $(button).attr('data-view-hide');
    var visibleText = $(button).text();

    toHide.split(',').forEach(function (item) {
        $('#' + item).hide();
    });
    $('#' + toShow).show();
}

$('.btn-action-icon-switch > .btn-action-icon').on('click', function (event) { $(this).parent().children().removeClass("active"); $(this).addClass("active"); })

/** Funzioni pro ARIA**/
function onFocusRow(row) {
    $(row).find('[role=link]').attr('tabindex', '0');

}
function onBlurRow(row) {
    $(row).find('[role=link]').attr('tabindex', '-1');
}


function onFocusRowShowCtrl(row) {
    $(row).find('[role=link]').attr('tabindex', '0');
    $(row).find('[role=toolbar]').removeClass('actions-hover');
    $(row).find('[role=toolbar]').removeClass('actions-fade');
}
function onBlurRowShowCtrl(row) {
    $(row).find('[role=link]').attr('tabindex', '-1');
    $(row).find('[role=toolbar]').addClass('actions-hover');
    $(row).find('[role=toolbar]').addClass('actions-fade');

}



$("section").on('click', '[data-panel-toggle][aria-expanded]', function (e) {
    e.preventDefault();
    $(this).closest('section').hasClass('panel-collapsed') ? $(this).attr('aria-expanded', "true") : $(this).attr('aria-expanded', "false");
    $(this).focus();
})

$("section").on('keydown', '[data-panel-toggle][aria-expanded]', function (e) {
    if (e.keyCode == 13 || e.keyCode == 32) {
        e.preventDefault();
        $(this).click();
    }
})

$(document.body).on("click", '.js-table-sections-header[aria-expanded] > tr', function (e) {
    var tbody = $(this).closest('tbody');
    var expanded = $(tbody).attr('aria-expanded');
    switch (expanded) {
        case 'true':
            $(tbody).attr('aria-expanded', "false");
            $(this).focus();
            break;
        case 'false':
            $(tbody).attr('aria-expanded', "true");
            $(this).focus();
            break;
        default:
            ;
    }
});
$(document.body).on("shown.bs.modal", "div[role = dialog]", function (e) {
    var elem = $(this).find("input:visible:first");
    if (!elem.hasClass('js-datetimepicker') && !elem.hasClass('js-daterangepicker'))
        elem.focus();
    else {
        $(this).find("button:visible:first").focus();
    }
});

$(document.body).on("click", ".edit-sedegapp", function (event) {
    editSedeGapp(event);
});
$(document.body).on("click", ".edit-news-content", function (event) {
    editNews(event);
});
$(document.body).on("click", '.js-table-sections-header > tr', function (e) {
   
    var $row = jQuery(this);
    var $tbody = $row.parent('tbody');
    var $table = $(this).parent('tbody');
    if (!$tbody.hasClass('open')) {
        jQuery('tbody', $table).removeClass('open');
    }

    $tbody.toggleClass('open');
});

$(document.body).on("keydown", '.titolettoAcc', function (e) {
    if (e.keyCode == 13 || e.keyCode == 32) {
        e.preventDefault();
        var divId = $(this).attr("href");
        var obj = $(divId);
        $(this).click();

    }
});

$(document.body).on("keydown", '.js-table-sections-header', function (e) {
    if (e.keyCode == 13 || e.keyCode == 32) {
        e.preventDefault();
        if (!$(this).hasClass('open')) {
            jQuery('tbody', $(this)).removeClass('open');
        }

        $(this).toggleClass('open');

        if ($(this).hasClass('open')) {
            $(this).attr('aria-expanded', "true")
            $(this).next('tbody').find('tr:first').focus();
        }
        else {
            $(this).attr('aria-expanded', "false");
            $(this).focus();
        }
    }
    else if (e.keyCode == 40) {
        if ($(this).hasClass('open')) {
            e.preventDefault();
            $(this).next('tbody').find('tr:first').focus();
        }
    }
})

$(document.body).on('keydown', '[role=row]', function (e) {
    var rows = $(this).closest('[role=grid]').find('[role=row][tabindex]:visible');
    var tools = $(this).find('[role=toolbar]');
    if (e.keyCode == 36 && !e.ctrlKey && !e.shiftKey) {
        var index = rows.index(this);
        if (index > 0) {
            e.preventDefault();
            if (tools.length == 0) {
                onBlurRow(this);
            } else {
                onBlurRowShowCtrl(this);
            }
            $(this).attr('tabindex', '-1');
            $(rows[0]).attr('tabindex', '0');
            rows[0].focus();
        }
    }
    else if (e.keyCode == 35 && !e.ctrlKey && !e.shiftKey) {
        var index = rows.index(this);
        if (index < rows.length - 1) {
            e.preventDefault();
            if (tools.length == 0) {
                onBlurRow(this);
            } else {
                onBlurRowShowCtrl(this);
            }
            $(this).attr('tabindex', '-1');
            $(rows[rows.length - 1]).attr('tabindex', '0');
            rows[rows.length - 1].focus();
        }
    }
    else if (e.keyCode == 38) {
        var index = rows.index(this);
        if (index > 0) {
            e.preventDefault();
            if (tools.length == 0) {
                onBlurRow(this);
            } else {
                onBlurRowShowCtrl(this);
            }

            $(this).attr('tabindex', '-1');
            $(rows[index - 1]).attr('tabindex', '0');
            rows[index - 1].focus();
        }
    }
    else if (e.keyCode == 40) {
        var index = rows.index(this);
        if (index < rows.length - 1) {

            e.preventDefault();
            if (tools.length == 0) {
                onBlurRow(this);
            } else {
                onBlurRowShowCtrl(this);
            }

            $(this).attr('tabindex', '-1');
            $(rows[index + 1]).attr('tabindex', '0');
            rows[index + 1].focus();

        }
    }
})

$(document.body).on('keydown', '[role=tablist]', function (event) {
    var keyCode = event.keyCode;
    if (keyCode == 37 || keyCode == 39) {
        event.preventDefault();
        var listTab = $(this).find('a[role=tab]');
        for (var i = 0; i < listTab.length; i++) {
            if ($(listTab[i]).is(':focus')) {
                if (keyCode == 37) {
                    if (i > 0) $(listTab[i - 1]).focus();
                    break;
                }
                else {
                    if (i < listTab.length - 1) $(listTab[i + 1]).focus();
                    break;
                }
            }
        }
    }
    else if (keyCode == 13 || keyCode == 32) {

        event.preventDefault();
        var listTab = $(this).find('a[role=tab]');
        for (var i = 0; i < listTab.length; i++) {
            if (listTab[i] == document.activeElement) {
                $(listTab[i]).attr('aria-selected', 'true');
                $(listTab[i]).attr('tabindex', '0');
                $(listTab[i]).click();
                var control = "#" + $(listTab[i]).attr('aria-controls');
                if (control != '')
                    $(control).find('[tabindex=0]').focus();
            }
            else {
                $(listTab[i]).attr('aria-selected', 'false');
                $(listTab[i]).attr('tabindex', '-1');
            }
        }
    }
})

$('ul[role=menubar]').on('keydown', function (e) {
    var keycode = e.keyCode;

    if (e.keyCode == 35 || e.keyCode == 36
        || e.keyCode == 38 || e.keyCode == 40) {
        e.preventDefault();
        var list = $(this).find('a[role=menuitem]:visible');
        for (var i = 0; i < list.length; i++) {
            if ($(list[i]).is(':focus')) {
                if (e.keyCode == 38) {
                    if (i > 0) { $(list[i - 1]).focus(); break; }
                }
                else if (e.keyCode == 40) {
                    if (i < list.length - 1) { $(list[i + 1]).focus(); break; }
                }
                else if (e.keyCode == 35) {
                    $(list[list.length - 1]).focus(); break;
                }
                else if (e.keyCode == 36) {
                    $(list[0]).focus(); break;
                }
            }
        }
    }
    else if (e.keyCode == 13 || e.keyCode == 32) {
        //e.preventDefault();
        $(this).find('a[role=menuitem]:focus').click();
    }
})
$('[role=radio]').change(function () {
    $(this).closest('[role=radiogroup]').find('[role=radio][aria-checked=true]').attr('aria-checked', 'false');
    $(this).attr('aria-checked', 'true');
})
/** Fine Funzioni pro ARIA **/

$(".content").on('click', '.button-rifiuta-turno', function (evt) {
    OnClick_buttonApprovaTurno($(this), false);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});
$(".content").on('click', '.button-approva-turno', function (evt) {
    OnClick_buttonApprovaTurno($(this), true);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});
$(".content").on('click', '.button-approva-codiceEcc', function (evt) {
    OnClick_buttonApprovaCodiceEcc($(this), true);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});
$(".content").on('click', '.button-rifiuta-codiceEcc', function (evt) {
    OnClick_buttonApprovaCodiceEcc($(this), false);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});
$(".content").on('click', '.button-rifiuta-matricola', function (evt) {
    OnClick_buttonApprovaMatricola($(this), false);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});
$(".content").on('click', '.button-approva-matricola', function (evt) {
    OnClick_buttonApprovaMatricola($(this), true);
    evt.stopPropagation();
    evt.preventDefault();
    return false;
});

function OnClick_buttonApprovaTurno(button, validazione) {
    var q = $(button).closest('tbody').next("tbody.tbodydata").find("tr[data-idrichiesta]:visible").length;

    if (validazione == true) {
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#testoprogress").hide();
        $("#titolo1").text("Validazione richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di validare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di validare la richiesta selezionata?");
    }
    else {
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#testoprogress").show();
        $("#titolo1").text("Rifiuto richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di rifiutare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di rifiutare la richiesta selezionata?");
    }

    RunningSerie = false;
    AbortSerie = false;
    $("#testoprogress").val("");
    $('#valida-rifiuta-tutti').modal('show');
    $("#btn-conf-tutti").show();
    $("#btn-ann").show();
    $("#btn-chiudi").hide();
    $("#rich-table").empty();
    $(".progress-bar").css("width", "0%");
    var items = [];

    $(button).closest('tbody').next("tbody.tbodydata").find('tr').each(function () {

        var nome = $(this).find('.matricola-nome').text();
        var imgName = $(this).find('img').attr('src');

        $(this).find('tr[data-idrichiesta]:visible').each(function () {
            var eccez = $(this).attr("data-eccezione");
            var text = $(this).attr("data-eccez-data");
            var idecc = $(this).attr("data-ideccezione");
            items.push({ eccez: eccez, nome: nome, data: text, id: idecc, image: imgName });
        });

    });

    for (var i = 0; i < items.length; i++) {
        var tr = $("tr[data-ideccezione='" + items[i].id + "']:visible").clone();
        $(tr).find('[title=VISUALIZZATO]').remove();
        $('<br/>').insertBefore($(tr).find('.descr-ecc'));

        var periodoEcc = $(tr).find('.periodo-ecc').text();
        $(tr).find('.periodo-ecc').remove();

        var motivoNode = $(tr).find('.motivo-ecc');
        var motivoEcc = '&nbsp;';
        if (motivoNode.length > 0) {
            motivoEcc = motivoNode.text();
            motivoNode.remove();
        }

        $(tr).find('td:eq(2)').remove();

        $('<td><div class="row font-w600" style="font-weight: bold;">' + items[i].data + '</div><div class="text-muted"><span>'+periodoEcc+'</span></div></td>' +
          '<td colspan="2"><div class="row"><div class="col-sm-2"><div class="widget-profile-info"><div class="profile-picture"> ' +
		  '<img style="width: 45px; height: 45px" src="' + items[i].image + '" />' +
          '</div></div></div><div class="col-sm-8" style="padding-left:30px">' +
          '<a tabindex="-1" class="font-w600" style="font-weight: bold;" data-toggle="modal" data-target="#giornata-modal" data-day="' + items[i].data + '" href="#">' + items[i].nome + '</a>' +
		  '<div class="text-muted">' +
		  '<span>' + motivoEcc + '</span>' +
		  '</div></div></div></td>').insertBefore($(tr).find('td:eq(1)'));

        $(tr).append('<td class="text-right actions-hover actions-fade" role="toolbar" +=""></td>');
        $(tr).attr("data-id-ec", items[i].id);
        $("#rich-table").append(tr);
    }
}

function OnClick_buttonApprovaCodiceEcc(button, validazione) {
    var q = $(button).closest('tbody').next("tbody.tbodydata").find("tr[data-idrichiesta]:visible").length;

    if (validazione == true) {
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#testoprogress").hide();
        $("#titolo1").text("Validazione richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di validare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di validare la richiesta selezionata?");
    }
    else {
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#testoprogress").show();
        $("#titolo1").text("Rifiuto richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di rifiutare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di rifiutare la richiesta selezionata?");
    }

    RunningSerie = false;
    AbortSerie = false;
    $("#testoprogress").val("");
    $('#valida-rifiuta-tutti').modal('show');
    $("#btn-conf-tutti").show();
    $("#btn-ann").show();
    $("#btn-chiudi").hide();
    $("#rich-table").empty();
    $(".progress-bar").css("width", "0%");
    var items = [];

    $(button).closest('tbody').next("tbody.tbodydata").find('tr').each(function () {

        var nome = $(this).find('.matricola-nome').text();
        var imgName = $(this).find('img').attr('src');

        $(this).find('tr[data-idrichiesta]:visible').each(function () {
            var eccez = $(this).attr("data-eccezione");
            var text = $(this).attr("data-eccez-data");
            var idecc = $(this).attr("data-ideccezione");
            items.push({ eccez: eccez, nome: nome, data: text, id: idecc, image: imgName });
        });

    });

    for (var i = 0; i < items.length; i++) {
        var tr = $("tr[data-ideccezione='" + items[i].id + "']:visible").clone();
        $(tr).find('[title=VISUALIZZATO]').remove();
        $('<br/>').insertBefore($(tr).find('.descr-ecc'));

        var periodoEcc = $(tr).find('.periodo-ecc').text();
        $(tr).find('.periodo-ecc').remove();

        var motivoNode = $(tr).find('.motivo-ecc');
        var motivoEcc = '&nbsp;';
        if (motivoNode.length > 0) {
            motivoEcc = motivoNode.text();
            motivoNode.remove();
        }

        $(tr).find('td:eq(2)').remove();

        $('<td><div class="row font-w600" style="font-weight: bold;">' + items[i].data + '</div><div class="text-muted"><span>' + periodoEcc + '</span></div></td>' +
            '<td colspan="2"><div class="row"><div class="col-sm-2"><div class="widget-profile-info"><div class="profile-picture"> ' +
            '<img style="width: 45px; height: 45px" src="' + items[i].image + '" />' +
            '</div></div></div><div class="col-sm-8" style="padding-left:30px">' +
            '<a tabindex="-1" class="font-w600" style="font-weight: bold;" data-toggle="modal" data-target="#giornata-modal" data-day="' + items[i].data + '" href="#">' + items[i].nome + '</a>' +
            '<div class="text-muted">' +
            '<span>' + motivoEcc + '</span>' +
            '</div></div></div></td>').insertBefore($(tr).find('td:eq(1)'));

        $(tr).append('<td class="text-right actions-hover actions-fade" role="toolbar" +=""></td>');
        $(tr).attr("data-id-ec", items[i].id);
        $("#rich-table").append(tr);
    }
}
function OnClick_buttonApprovaMatricola(button, validazione) {
    var elenco = $(button).closest('div.row').find("tr[data-idrichiesta]:visible");
    var q = elenco.length;

    if (validazione == true) {
        $("#btn-conf-tutti").removeAttr("disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "true");
        $("#testoprogress").hide();
        $("#titolo1").text("Validazione richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di validare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di validare la richiesta selezionata?");
    }
    else {
        $("#btn-conf-tutti").attr("disabled", "disabled");
        $("#btn-conf-tutti").attr("data-validaTF", "false");
        $("#testoprogress").show();
        $("#titolo1").text("Rifiuto richieste");
        if (q > 1)
            $("#messaggio").text("Confermi di rifiutare le " + q.toString() + " richieste selezionate?");
        else
            $("#messaggio").text("Confermi di rifiutare la richiesta selezionata?");
    }

    RunningSerie = false;
    AbortSerie = false;
    $("#testoprogress").val("");
    $('#valida-rifiuta-tutti').modal('show');
    $("#btn-conf-tutti").show();
    $("#btn-ann").show();
    $("#btn-chiudi").hide();
    $("#rich-table").empty();
    $(".progress-bar").css("width", "0%");
    var items = [];

    var nome = $(button).closest('div.row').find('.matricola-nome').text();
    var imgName = $(button).closest('div.row').find('img').attr('src');

    elenco.each(function () {
        var eccez = $(this).attr("data-eccezione");
        var text = $(this).attr("data-eccez-data");
        var idecc = $(this).attr("data-ideccezione");
        items.push({ eccez: eccez, nome: nome, data: text, id: idecc, image: imgName });

    });
    for (var i = 0; i < items.length; i++) {
        var tr = $("tr[data-ideccezione='" + items[i].id + "']:visible").clone();
        $(tr).find('[title=VISUALIZZATO]').remove();
        $('<br/>').insertBefore($(tr).find('.descr-ecc'));

        var periodoEcc = $(tr).find('.periodo-ecc').text();
        $(tr).find('.periodo-ecc').remove();

        var motivoNode = $(tr).find('.motivo-ecc');
        var motivoEcc = '&nbsp;';
        if (motivoNode.length > 0) {
            motivoEcc = motivoNode.text();
            motivoNode.remove();
        }

        $(tr).find('td:eq(2)').remove();

        $('<td><div class="row font-w600" style="font-weight: bold;">' + items[i].data + '</div><div class="text-muted"><span>' + periodoEcc + '</span></div></td>' +
          '<td colspan="2"><div class="row"><div class="col-sm-2"><div class="widget-profile-info"><div class="profile-picture"> ' +
		  '<img style="width: 45px; height: 45px" src="' + items[i].image + '" />' +
          '</div></div></div><div class="col-sm-8" style="padding-left:30px">' +
          '<a tabindex="-1" class="font-w600" style="font-weight: bold;" data-toggle="modal" data-target="#giornata-modal" data-day="' + items[i].data + '" href="#">' + items[i].nome + '</a>' +
		  '<div class="text-muted">' +
		  '<span>' + motivoEcc + '</span>' +
		  '</div></div></div></td>').insertBefore($(tr).find('td:eq(1)'));

        $(tr).append('<td class="text-right actions-hover actions-fade" role="toolbar" +=""></td>');
        $(tr).attr("data-id-ec", items[i].id);
        $("#rich-table").append(tr);
    }
}

function ShowNextActivity(indexDay) {
    $('#prevAct'+indexDay).show();
    var elenco = $('#activities-container-day'+indexDay).find('[data-activities-item]');
    var index = elenco.index($('#activities-container-day'+indexDay).find('[data-activities-item]:visible'));
    if (index == elenco.length-2) {
        $('#nextAct' + indexDay).hide();
    }
    $(elenco[index]).hide();
    $(elenco[index + 1]).show();
}
function ShowPrevActivity(indexDay) {
    $('#nextAct' + indexDay).show();
    var elenco = $('#activities-container-day' + indexDay).find('[data-activities-item]');
    var index = elenco.index($('#activities-container-day' + indexDay).find('[data-activities-item]:visible'));
    if (index == 1) {
        $('#prevAct' + indexDay).hide();
    }
    $(elenco[index]).hide();
    $(elenco[index - 1]).show();
}
function ChooseRangeActivity() {
    $('#ElencoAttivita').addClass('css-input-disabled');
    $("#ElencoAttivitaExt").addClass("block block-opt-refresh");
    var dataDa = $('#datada').val();
    var dataA = $('#dataal').val();
    $.ajax({
        url: "/attivita/getAttivita",
        type: "GET",
        dataType: "html",
        data: { dataDa: dataDa, dataA:dataA },
        success: function (data) {
            $('#ElencoAttivita').html(data);
            $('#ElencoAttivita').removeClass('css-input-disabled');
            $("#ElencoAttivitaExt").removeClass("block block-opt-refresh");
        }
    });
}
function attSettimanaPrec() {
    var dataDaStr = $('#datada').val();
    var dataDa = new Date(dataDaStr.substr(6, 4), dataDaStr.substr(3, 2)-1, dataDaStr.substr(0, 2));

    dataDa.setDate(dataDa.getDate() - 7);
    var dataA = new Date(dataDa);
    dataA.setDate(dataDa.getDate() + 6);

    $('#datada').val((dataDa.getDate() < 10 ? "0" + dataDa.getDate(): dataDa.getDate()) + "/" + (dataDa.getMonth() + 1 < 10 ? "0" + (dataDa.getMonth() + 1) : (dataDa.getMonth() + 1)) + "/" + dataDa.getFullYear());
    $('#dataal').val((dataA.getDate()  < 10 ? "0" + dataA.getDate() : dataA.getDate())  + "/" + (dataA.getMonth()  + 1 < 10 ? "0" + (dataA.getMonth() + 1)  : (dataA.getMonth() + 1))  + "/" + dataA.getFullYear());

    ChooseRangeActivity();
}
function attSettimanaSucc() {
    var dataAStr = $('#dataal').val();
    var dataA = new Date(dataAStr.substr(6, 4), dataAStr.substr(3, 2)-1, dataAStr.substr(0, 2));

    var dataDa = new Date(dataA);

    dataDa.setDate(dataDa.getDate() + 1);
    dataA.setDate(dataDa.getDate() + 6);

    $('#datada').val((dataDa.getDate() < 10 ? "0" + dataDa.getDate() : dataDa.getDate()) + "/" + (dataDa.getMonth() + 1 < 10 ? "0" + (dataDa.getMonth() + 1) : (dataDa.getMonth() + 1)) + "/" + dataDa.getFullYear());
    $('#dataal').val((dataA.getDate()  < 10 ? "0" + dataA.getDate()  : dataA.getDate())  + "/" + (dataA.getMonth()  + 1 < 10 ? "0" + (dataA.getMonth() + 1)  : (dataA.getMonth() + 1))  + "/" + dataA.getFullYear());

    ChooseRangeActivity();
}

function AttivaAccordion() {
    $(".accordion-sintesi").on("click", function () {
        var sel = this;
        $(sel).next(".accordion-dettagli").siblings(".accordion-dettagli").hide();
        $(sel).siblings(".accordion-sintesi").find("blockquote").removeClass("primary");
        $(sel).siblings(".accordion-sintesi").find(".accordion-showdetail").removeClass("icon-arrow-up").addClass("icon-arrow-down");
        $(sel).find(".accordion-showdetail").toggleClass("icon-arrow-down icon-arrow-up");
        $(sel).find("blockquote").toggleClass("primary");
        $(sel).next(".accordion-dettagli").toggle(100);
    } );
};


/*
    Inserimento di una nota legata all'eccezione con id "idEcc" destinata alla segreteria
    idRichiesta = identificativo univoco della richiesta
*/
function AggiungiNotaEccezione()
{
    swal({
        title: 'Invia messaggio alla segreteria',
        //type: 'info',
        html:
            '<div id="swal2-content" class="swal2-content" style="display: block;">Inserisci il contenuto del messaggio</div>' +
            '<textarea id="txAggiungiNotaEccezione" class="swal2-textarea" placeholder="" style="display: block;"></textarea>' +
            '<div id="errAggiungiNotaEccezione" class="swal2-validationerror" style="display: none;">Inserisci il testo</div>' +
            '<br>'+
            '<button type="button" class="btn btn-primary btn-lg" id="btnAggiungiNotaEccezione" >Salva</button>' +
            '<button type="button" class="swal2-confirm btn btn-primary btn-lg" style="display: none;">Salva</button>' +
            '<button type="button" class="swal2-cancel" style="display: none;">Cancel</button>' +
            '<span class="swal2-close" style="display: none;">×</span>',
        showCloseButton: false,
        showCancelButton: false,
        showConfirmButton: false,
        confirmButtonText: 'Salva',
        //confirmButtonClass: 'btn btn-primary btn-lg',
        buttonsStyling: false,
    });

    $('#txAggiungiNotaEccezione').on('keypress', function () {
        $('#errAggiungiNotaEccezione').css('display', 'none');
    });

    $('#btnAggiungiNotaEccezione').on('click', function () {
        var nota = $('#txAggiungiNotaEccezione').val();
        if ($.trim(nota) === "") {
            $('#errAggiungiNotaEccezione').css('display', 'block');
            return false;
        }
        $('#errAggiungiNotaEccezione').css('display', 'none');

        var data = $('#datagiornata').val();
        $.ajax( {
            url: "/ajax/AggiungiNotaEccezione",
            type: "POST",
            dataType: "html",
            data: { data: data, nota: nota },
            success: function ( data )
            {
                switch ( data )
                {
                    case "OK":
                        swal( 'Inserimento nota', "Nota aggiunta con successo", "success" );
                        GetNoteRichiesta();
                        break;
                    default:
                        swal("Oops...", data, 'error');
                        break;
                }
            },
            error: function ( parm1, parm2, parm3 )
            {
            }
        } );
    });

    //swal( {
    //    title: 'Invia messaggio alla segreteria',
    //    text: "Inserisci il contenuto del messaggio",
    //    input: 'textarea',
    //    confirmButtonText: 'Salva',
    //    confirmButtonClass: 'btn btn-primary btn-lg',
    //    preConfirm: function ( value )
    //    {
    //        return new Promise(function (resolve, reject) {
    //            if (value == "") {
    //                reject("Inserisci il testo");
    //            }
    //            else {
    //                resolve();
    //            }
    //        });
    //    },
    //    buttonsStyling: false
    //} ).then( function ( result )
    //{
    //    var nota = result;
    //    var data = $('#datagiornata').val();

    //    $.ajax( {
    //        url: "/ajax/AggiungiNotaEccezione",
    //        type: "POST",
    //        dataType: "html",
    //        data: { data: data, nota: nota },
    //        success: function ( data )
    //        {
    //            switch ( data )
    //            {
    //                case "OK":
    //                    swal( 'Inserimento nota', "Nota aggiunta con successo", "success" );
    //                    GetNoteRichiesta();
    //                    break;
    //                default:
    //                    swal( "Oops...", data, 'error' )
    //                    break;
    //            }
    //        },
    //        error: function ( parm1, parm2, parm3 )
    //        {
    //        }
    //    } );
    //} );
}

function ModificaNotaRichiesta( idNota )
{
    var tx = $('#tx_nota_richieste_data_' + idNota).find('.message:first').text();

    swal({
        title: 'Modifica la nota',
        //type: 'info',
        html:
            '<div id="swal2-content" class="swal2-content" style="display: block;">Inserisci il contenuto del messaggio</div>' +
            '<textarea id="txAggiungiNotaEccezione" class="swal2-textarea" placeholder="" style="display: block;">' + tx + '</textarea>' +
            '<div id="errAggiungiNotaEccezione" class="swal2-validationerror" style="display: none;">Inserisci il testo</div>' +
            '<br>' +
            '<button type="button" class="btn btn-primary btn-lg" id="btnAggiungiNotaEccezione" >Salva</button>' +
            '<button type="button" class="swal2-confirm btn btn-primary btn-lg" style="display: none;">Salva</button>' +
            '<button type="button" class="swal2-cancel" style="display: none;">Cancel</button>' +
            '<span class="swal2-close" style="display: none;">×</span>',
        showCloseButton: false,
        showCancelButton: false,
        showConfirmButton: false,
        confirmButtonText: 'Salva',
        //confirmButtonClass: 'btn btn-primary btn-lg',
        buttonsStyling: false,
    });

    $('#txAggiungiNotaEccezione').on('keypress', function () {
        $('#errAggiungiNotaEccezione').css('display', 'none');
    });

    $('#btnAggiungiNotaEccezione').on('click', function () {
        var nota = $('#txAggiungiNotaEccezione').val();
        if ($.trim(nota) === "") {
            $('#errAggiungiNotaEccezione').css('display', 'block');
            return false;
        }
        $('#errAggiungiNotaEccezione').css('display', 'none');

        $.ajax({
            url: "/ajax/ModificaNotaRichiesta",
            type: "POST",
            dataType: "html",
            data: { idNota: idNota, nota: nota },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Modifica nota', "Nota modificata con successo", "success");
                        GetNoteRichiesta();
                        break;
                    default:
                        swal("Oops...", data, 'error')
                }
            },
            error: function (parm1, parm2, parm3) {
            }
        });
    });    

    //swal( {
    //    title: 'Modifica la nota',
    //    text: "Inserisci il contenuto della nota",
    //    input: 'textarea',
    //    inputValue: tx,
    //    confirmButtonText: 'Salva',
    //    confirmButtonClass: 'btn btn-primary btn-lg',
    //    preConfirm: function ( value )
    //    {
    //        return new Promise( function ( resolve, reject )
    //        {
    //            if ( value == "" )
    //            {
    //                reject( "Inserisci il testo" );
    //            }
    //            else
    //            {
    //                resolve();
    //            }
    //        } )
    //    },
    //    buttonsStyling: false
    //} ).then( function ( result )
    //{
    //    var nota = result;
    //    $.ajax( {
    //        url: "/ajax/ModificaNotaRichiesta",
    //        type: "POST",
    //        dataType: "html",
    //        data: { idNota: idNota, nota: nota },
    //        success: function ( data )
    //        {
    //            switch ( data )
    //            {
    //                case "OK":
    //                    swal( 'Modifica nota', "Nota modificata con successo", "success" );
    //                    GetNoteRichiesta();
    //                    break;
    //                default:
    //                    swal( "Oops...", data, 'error' )
    //            }
    //        },
    //        error: function ( parm1, parm2, parm3 )
    //        {
    //        }
    //    } );
    //} );
}

function EliminaNotaRichiesta( idNota )
{
    swal( {
        title: 'Sicuro di voler cancellare la nota selezionata?',
        text: "La nota verrà cancellata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla'
    } ).then( function ()
    {
        $.ajax( {
            url: "/ajax/EliminaNotaRichiesta",
            type: "POST",
            dataType: "html",
            data: { idNota: idNota },
            success: function ( data )
            {
                switch ( data )
                {
                    case "OK":
                        swal( 'Cancellazione nota', "Nota cancellata con successo", "success" );
                        GetNoteRichiesta();
                        break;
                    default:
                        swal( "Oops...", data, 'error' )
                }
            }
        } );
    } );
}


function GetNoteRichiesta( )
{
    $( '#noteGiornataContainer' ).html( '' );
    var data = $( '#datagiornata' ).val();
    $.ajax( {
        url: "/ajax/GetNoteRichiesta",
        type: "POST",
        dataType: "html",
        data: { data: data },
        success: function ( result )
        {
            $( '#noteGiornataContainer' ).html( result );
        }
    } );
}
function FiltraSchedeEcc() {
    var ck = $('.tematica:checkbox:checked');
    var le = ck.length;
    var arrTematiche = [];
    for (var i = 0; i < le; i++) {
        arrTematiche.push(Number($(ck[i]).val()));
    }

    var ta = $('.tipo-assenza:checkbox:checked');
    le = ta.length;
    var arrTipi = [];
    for (var i = 0; i < le; i++) {
        arrTipi.push(Number($(ta[i]).val()));
    }


    $("#schede-eccez").addClass("block-opt-refresh");

    $.ajax({
        url: '/VisualizzaEccezioni/getecc',
        type: "GET",
        dataType: "html",
        data: { tem: arrTematiche.toString(), tipiassenza: arrTipi.toString() },
        success: function (data) {
            $("#schede-eccez").removeClass("block-opt-refresh");
            $("#panel-schede-ecc").replaceWith(data);
        }
    });
}

function showDettaglioEccezione(codice) {
    $("#eccezione-cont").html("");
    $("#dettaglio-eccezione-modal").modal("show");
    $("#ecc-codice").text("" );

    $.ajax({
        url: '/schedaEccezioni/getEccezioneSchedaPopup?codice=' + codice,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            $("#eccezione-cont").html(data);
            var eccSpettanti = $("#eccezioni-spettanti").val();
            if (eccSpettanti != null && eccSpettanti != "") {
                var arr = eccSpettanti.split(',');
                $(".rinvio-ecc").each(function () {
                    var t = $(this).text();
                    if (arr.indexOf(t) < 0) {
                        $(this).remove();
                    }
                });
            }

            if ($("#descrittiva-estesa").length) 
                $("#ecc-codice").text($("#descrittiva-estesa").val() + " (" + codice + ")");
                 else
                $("#ecc-codice").text($("#descrittiva-breve").val() );
             
        }
    });



    //$("#eccezione-cont").load("/schedaeccezioni/getEccezioneSchedaPopup?codice=" + codice, function () {
    //  $("#ecc-codice").text("Eccezione " + codice);

    //});


}

// $(document.body).on("submit", '#form-eccezioni', function (e) {


//});
function updatePresSede(codiceSedeGapp, desSedeGapp) {
    $.ajax({
        url: "/Home/PresenzaDipedentiSede",
        type: "GET",
        data: { codiceSedeGapp: codiceSedeGapp, desSedeGapp: desSedeGapp },
        async: true,
        success: function (data) {
            $('#cont' + codiceSedeGapp).replaceWith($(data));
        },
        error: function (result) {
        }
    });
}

function apriModale(idFinestra) {
    $('#' + idFinestra).modal('show');
}

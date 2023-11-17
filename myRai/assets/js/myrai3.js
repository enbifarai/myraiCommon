var ModificaEvento = false;
$("#button-pren-anag").click(function (event) {
    event.preventDefault();
    event.stopPropagation();
    settaBottoni(event);
    // Do something
});
$("#button-mod-anag").click(function (event) {

    event.preventDefault();
    event.stopPropagation();
    settaBottoni(event);
    // Do something
});

function AzzeraMP(minFine,data)
{
      swal({
            title: 'Attenzione',
            text: "Stai annullando la tua maggior presenza, confermi ?",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Conferma',
            cancelButtonText: 'Annulla'
        }).then(function () {

                     
               $.ajax({
                    url: '/ajax/azzeraMP',
                    type: 'POST',
                    cache: false,
                    data: { minfine:minFine, data:data },
                    dataType: "json",
                    success: function (data) { 
                        if (data.result=="OK")
                            {
                                ShowWizard();
                                swal('OK', 'Inserimento eseguito', 'info');
                            }
                            else
                            {
                                   swal("Errore", data.result, "error")
                            }
                    }  
               });

        });
}


function settaBottoni(evt) {
    var errore = 0;
    if ($("#nome").val() == "") {
        $('span[data-valmsg-for="nome"]').removeClass("hidden");
        errore = 1;
    }
    if ($("#cognome").val() == "") {
        $('span[data-valmsg-for="cognome"]').removeClass("hidden");
        errore = 1;
    }
    if ($("#sedeInsediamento").val() == "True") {
        if ($("#specificaInsediamento").val() == "") {
            $('span[data-valmsg-for="specificaInsediamento"]').removeClass("hidden");
            errore = 1;
        }

    }

    $.ajax({
        url: '/events/SettaCampi',
        type: 'GET',
        cache: false,
        data: { idevento: $("#idevento").val() },
        dataType: "json",
        success: function (data) {


            if (data.result.length > 0) {
                for (var i = 0; i < data.result.length; i++) {
                    if (data.result[i].obbligatorio == true) {

                        if (data.result[i].descrizione == "doc") {
                            if ($("#tipoDocumento").val() == "") {
                                $('span[data-valmsg-for="tipoDocumento"]').removeClass("hidden");
                                errore = 1;
                            }
                            if ($("#numeroDocumento").val() == "") {
                                $('span[data-valmsg-for="numeroDocumento"]').removeClass("hidden");
                                errore = 1;
                            }
                        } else {
                            if ($("#" + data.result[i].descrizione).val() == "") {
                                $('span[data-valmsg-for="' + data.result[i].descrizione + '"]').removeClass("hidden");
                                errore = 1;
                            }
                        }
                    }

                }

            }
            if (errore == 0) {
                $("#form-events").submit();
            }

        }
    });




}
function ShowPage1() {
    if ($(".tabcart").hasClass("active")) {
        if (!$(".page1").hasClass("hide")) return;

        $(".icone-timb i").each(function () {
            if ($(this).hasClass("icon-login")) {
                $(this).removeClass("icon-login");
                $(this).addClass("icon-logout");
            }
            else if ($(this).hasClass("icon-logout")) {
                $(this).removeClass("icon-logout");
                $(this).addClass("icon-login");
            }
        });

        $(".page2").addClass("hide");
        $(".page1").removeClass("hide");
    }
    if ($(".tabpres").hasClass("active")) {
        $(".page2pres").addClass("hide");
        $(".page1pres").removeClass("hide");
    }
}
function ShowPage2() {
    if ($(".tabcart").hasClass("active")) {
        if (!$(".page2").hasClass("hide")) return;
        $(".icone-timb i").each(function () {
            if ($(this).hasClass("icon-login")) {
                $(this).removeClass("icon-login");
                $(this).addClass("icon-logout");
            }
            else if ($(this).hasClass("icon-logout")) {
                $(this).removeClass("icon-logout");
                $(this).addClass("icon-login");
            }
        });
        $(".page2").removeClass("hide");
        $(".page1").addClass("hide");
    }
    if ($(".tabpres").hasClass("active")) {
        $(".page2pres").removeClass("hide");
        $(".page1pres").addClass("hide");
    }
}
function ShowModalBoarding(iditem) {

    $.ajax({
        url: '/cv_online/onboarding?iditem=' + iditem,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            $("#modal-boarding").html("<div class='modal-backdrop fade in'></div>" + data);
            if (iditem == 0) $('#modal-boarding').modal('show');
        }
    });
}

function EndModalBoarding() {
    $("#modal-boarding").modal("hide");
    EndBoarding();
}

function ShowPopupBoss(matr, data, nominativo) {
    hidePopover();
    $("#modal-boss").modal("show");
    $("#tit-modal-boss").text("Scheda dipendente");
    $("#data-boss").val(data);

    $("#popup-boss-matr").val(matr);
    $("#anagrafica-boss").html("");
    $("#evidenze-boss").html("");
    OnChange_DataBoss(null);

    $.ajax({
        url: '/PopupBoss/getanagrafica?matricola=' + matr,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            var t = $("#anagrafica-boss");
            $(t).html(data);
        }
    });

    $.ajax({
        url: '/PopupBoss/GetEvidenzePerPopupBoss?matricola=' + matr + '&data=' + data,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            var t = $("#evidenze-boss");
            $(t).html(data);
        }
    });

    $.ajax({
        url: '/PopupBoss/getperiodosw?matricola=' + matr,
        type: "GET",
        dataType: "json",
        data: {},
        success: function (data) {
            if (data.result == true) {
                $("#sw-boss").show();
                $("#sw-period").text(data.period);
            }
            else {
                $("#sw-boss").hide();
                $("#sw-period").text("");
            }
           
        }
    });

    $('#popup-boss-actionEnabled').val('1');
}

function OnChange_DataBoss(e) {
    $("#sectimb").addClass("block-opt-refresh");
    $.ajax({
        url: '/ajax/getTimbratureAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: $("#data-boss").val(), matr: $("#popup-boss-matr").val() },
        success: function (data) {
            var t = $("#timbratureday-boss");
            $(t).html(data);

            $("#sectimb").removeClass("block-opt-refresh");
        }
    });

    $("#sectsegn").addClass("block-opt-refresh");
    $.ajax({
        url: '/ajax/getSegnalazioniAjaxView',
        type: "GET",
        dataType: "html",
        data: {
            date: $("#data-boss").val(),
            matricola: $("#popup-boss-matr").val(),
            abilitaApprovazione: $('#popup-boss-actionEnabled').val()
        },
        success: function (data) {
            var t = $("#segnalazioniday-boss");
            $(t).html(data);
            $("#sectsegn").removeClass("block-opt-refresh");
        }
    });

    var recipient = $("#data-boss").val();
    $.ajax({
        url: '/ajax/getInfoGiornataAjaxView',
        type: "GET",
        dataType: "html",
        data: {
            date: recipient,
            ideccezionerichiesta: null,
            matr: $("#popup-boss-matr").val()
        },
        success: function (data) {
            var t = $("#infogiornata-day-boss");
            $(t).html(data);
            var parent = $("#infogiornata-day-bossParent");
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

    $("#prossimeTrasferte-boss-spinner").html('<div class="text-center"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #ebebeb;"></i></div>');
    $("#divProssimaTrasferta").html('');
    $("#divProssimaTrasferta").html('<div><b>Ricerca prossima trasferta in corso...<i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #ebebeb;"></i></b></div>');

    $.ajax({
        url: '/ajax/getProssimeTrasferte',
        type: "GET",
        dataType: "html",
        data: {
            date: $("#data-boss").val(),
            matricola: $("#popup-boss-matr").val()
        },
        success: function (data) {
            var t = $("#prossimeTrasferte-boss");
            $(t).html(data);
        },
        error: function (e1, e2, e3)
        {
            $("#prossimeTrasferte-boss-spinner").html('<table class="table" id="table-prox-trasferte"><tbody><tr><td class="text-center no-border" colspan="100"><span class="rai-font-md-neutral">Non ci sono dati da visualizzare</span></td></tr></tbody ></table>');
        }
    });

    $.ajax({
        url: '/ajax/getProssimaTrasferta',
        type: "GET",
        dataType: "html",
        data: {
            date: $("#data-boss").val(),
            matricola: $("#popup-boss-matr").val()
        },
        success: function (data) {
            if (data != null &&
                typeof data != "" &&
                data != "") {
                $("#divProssimaTrasferta").html('');
                data = $.trim(data);
                if (data.length > 0) {
                    $("#divProssimaTrasferta").html('');
                    var t = $("#divProssimaTrasferta");
                    $(t).html(data);
                    $('b[data-toggle="tooltip"]').tooltip();
                }
                else {
                    var t = $("#divProssimaTrasferta");
                    $(t).html('<div><span class="rai-font-md-neutral">Non ci sono dati da visualizzare</span></div>');
                }
            }
            else {
                var t = $("#divProssimaTrasferta");
                $(t).html('<div><span class="rai-font-md-neutral">Non ci sono dati da visualizzare</span></div>');
            }
        },
        error: function (e1, e2, e3) {
            $("#divProssimaTrasferta").html('');
            var t = $("#divProssimaTrasferta");
            $(t).html('<div><span class="rai-font-md-neutral">Non ci sono dati da visualizzare</span></div>');
        }
    });

    $.ajax({
        url: '/DashboardResponsabile/GetStatoFerieBoss',
        type: "GET",
        dataType: "html",
        data: {
            matricola: $("#popup-boss-matr").val()
        },
        success: function (data) {
            var t = $("#statoFerie-boss");
            $(t).html(data);
        },
        error: function (e1, e2, e3) {
            $("#prossimeTrasferte-boss-spinner").html('<table class="table" id="table-prox-trasferte"><tbody><tr><td class="text-center no-border" colspan="100"><span>NON CI SONO DATI DA VISUALIZZARE</span></td></tr></tbody ></table>');
        }
    });
}

function databossBefore() {
    var startdate = $("#data-boss").val();
    var new_date = moment(startdate, "DD/MM/YYYY");
    var thing = new_date.add(-1, 'days').format('DD/MM/YYYY');
    $("#data-boss").val(thing);
    $("#data-boss").change();
}

function databossAfter() {
    var startdate = $("#data-boss").val();
    var new_date = moment(startdate, "DD/MM/YYYY");
    var thing = new_date.add(1, 'days').format('DD/MM/YYYY');
    $("#data-boss").val(thing);
    $("#data-boss").change();
}
function showPopoverNotification(span, id) {

    $('[data-toggle="popover"]').popover("hide");
    $.ajax({
        url: '/ajax/getDettaglioRichiesta',
        type: "GET",
        dataType: "html",
        data: { IdEccezioneRichiesta: id },
        success: function (data) {

            $(span).attr("data-content", data);

            $(span).popover().on("show.bs.popover", function () { $(this).data("bs.popover").tip().css("max-width", "800px"); });
            //$(span).popover({ template: '<div class="popover" role="tooltip" style="width: 800px;"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"><div class="data-content"></div></div></div>' });
            $(span).popover("show");
        }
    });

}
function showPopoverDetail(span, elem) {
    var obj = elem;
    $('[data-toggle="popover"]').popover("hide");
    $.ajax({
        url: "/notifiche/DettaglioNotifica",
        type: "POST",
        dataType: "html",
        data: obj,
        success: function (data) {

            $(span).attr("data-content", data);

            $(span).popover().on("show.bs.popover", function () { $(this).data("bs.popover").tip().css("max-width", "800px"); });
            //$(span).popover({ template: '<div class="popover" role="tooltip" style="width: 800px;"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"><div class="data-content"></div></div></div>' });
            $(span).popover("show");
        }
    });

}
function showPopover(div, matr, data) {

    $('[data-toggle="popover"]').popover("hide");
    var d = data;

    $.ajax({
        url: '/ajax/getTimbratureAjaxView',
        type: "GET",
        dataType: "html",
        data: { date: data, matr: matr },
        success: function (data) {

            $(div).attr("data-content", data);

            //"<div class='header-timbr-popover'><b>Timbrature del " + d + "</b> <i onclick='hidePopover()' style='cursor:pointer;position: absolute;right: 2px;top: 6px;' class='text-primary icons icon-close'></i></div>" +
            //    $(data).html()
            //.find(".panel-body").html()
            //  );
            $(div).popover('show')
            var actions = $(".popover-content").find(".panel-actions");
            $(actions).html("<a onclick='hidePopover()' class='panel-action panel-action-dismiss' ></a>")
            var title = $(".popover-content").find(".panel-title");
            $(title).text("Timbrature del " + d);
            $(title).css("font-size", "18px");

            var cont = $(".popover-content").find("#timbraturetoday");
            var codOrario = $(cont).attr("data-codice-orario");
            var desOrario = $(cont).attr("data-desc-turno");

            var content = $(".popover-content").find("#timbraturecontent");
            $(content).prepend('<div class="row"><div class="col-sm-12 text-center text-italic">'+codOrario+' - '+desOrario+'</div></div>');
        }
    });
}
function hidePopover() {
    $('[data-toggle="popover"]').popover("hide");
}
function UpdateNotifiche(totali, totali1) {
    $("#badge-not").show();

    if (totali > 99)
        //$("#badge-not").text("99+");
        $("#badge-not").text(totali);
    else {
        if (totali == 0) {
            $("#badge-not").text("");
            $("#badge-not").hide();
        }
        else
            $("#badge-not").text(totali);
    }

    $("#badge-not1").show();
    if (totali1 == 0) {
        $("#badge-not1").text("");
        $("#badge-not1").hide();
    }
    else
        $("#badge-not1").text(totali1);

}
function DelNotifica(id, idbutton) {

    $("#" + idbutton).html('<i class="fa fa-spinner fa-spin"></i>');


    $.ajax({
        type: 'GET',
        url: "/scrivania/cancellanotifica",
        dataType: "json",
        data: { id: id },
        cache: false,
        success: function (data) {
            if (data.result == "OK") {
                $("#" + idbutton).closest("tr").remove();
                if ($("#table-notifiche tbody tr").length == 0) {
                    $("#delnotifiche").attr("disabled", "disabled");
                    $("#delnotifiche").addClass("disable");
                }
                else {
                    $("#delnotifiche").removeAttr("disabled");
                    $("#delnotifiche").removeClass("disable");
                }
                UpdateNotifiche(data.totalNow, data.totalNow1);




            }
            else {
                $("#" + idbutton).text('CANCELLA');
                swal(data.result, '', 'error');
            }
        },
        error: function (aa, b, c) {
            $("#" + idbutton).text('CANCELLA');
            swal(aa + b + c, '', 'error');
        },
        complete: function () {

        }
    });
}
function DelNotificheAll(matr, tipo) {


    swal({
        title: 'Sei sicuro?',
        text: "Tutte le notifiche saranno cancellate",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla'
    }).then(function () {

        $("#delnotifiche").html('<i class="fa fa-spinner fa-spin"></i>');

        $.ajax({
            type: 'GET',
            url: "/scrivania/CancellaNotificheAll",
            dataType: "json",
            data: { matricola: matr, tipo: tipo },
            cache: false,
            success: function (data) {
                $("#delnotifiche").text('CANCELLA');
                if (data.result == "OK") {
                    $("#button-refresh-not").click();
                    $("#delnotifiche").attr("disabled", "disabled");

                    UpdateNotifiche(data.totalNow, data.totalNow1);
                }
                else {
                    swal(data.result, '', 'error');
                }
            },
            error: function (aa, b, c) {
                $("#delnotifiche").text('CANCELLA');
                swal(aa + b + c, '', 'error');
            },
            complete: function () {

            }
        });


    })



}
function ShowDoc(idDoc) {
    $("#doc-modal").modal("show");
    $("#docframe").css("height", ($(window).height() - 50) + "px");
    $("#docframe").attr("src", "/docDipendente/getPdfBinary?idpdf=" + idDoc);
}
function DelDoc(idDoc) {
    swal({
        title: 'Sei sicuro?',
        text: "Il documento sarà cancellato",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, cancella!',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            type: 'GET',
            url: "/docDipendente/deldoc",
            dataType: "json",
            data: { id: idDoc },
            cache: false,
            success: function (data) {
                if (data.result == "OK") {
                    $("#tr" + idDoc).remove();
                }
            },
            error: function (aa, b, c) {
                swal(aa + b + c, '', 'error');
            },
            complete: function () {

            }
        });
    });
}


function checkUpload() {
    if ($.trim($("#note").val()) == "" || $.trim($("#filename").val()) == "")
        return false;
    else return true;
}


function allowUpload() {
    return;
    if ($.trim($("#note").val()) == "" || $.trim($("#filename").val()) == "")
        $("#buttonUpload").addClass("disable");
    else
        $("#buttonUpload").removeClass("disable");
}
function CheckCustomValidation() {
    $("#tematica-error").hide();
    $("#utenti-error").hide();
    $("#destinatari-error").hide();
    var valid = true;
    if ($("input[name=tematiche]:checked").length == 0) { $("#tematica-error").show(); valid = false; }
    if ($("input[name=utenti]:checked").length == 0) { $("#utenti-error").show(); valid = false; }
    if ($("input[name=destinatari]:checked").length == 0) { $("#destinatari-error").show(); valid = false; }
    return valid;
}






$(document).ready(function () {

    $(document.body).on("click", '#button-test-ecc', function (event) {
         
        event.preventDefault();
        var form = $("#form-testecc");
        
            $.ajax({
                url: $(form).attr('action'),
                type: 'GET',
                dataType: "json",
                data: $(form).serialize(),
                success: function (data) {
                   
                    var editor = new JsonEditor('#json-display',data);
                    editor.load(data);
                },
                error: function (a, b, c) {
                    alert(a + b + c);
                }
            });
        
    });


    $('.custom-checkbox').mousedown(function () {
        changeCheck($(this));
    });
    $('.custom-checkbox').each(function () {
        changeCheckStart($(this));
    });






    $(".progress-bar").each(function () {
        each_bar_width = $(this).attr('aria-valuenow');
        $(this).width(each_bar_width + '%');
    });

});
function changeCheck(el) {
    var el = el, input = el.find('input').eq(0);
    if (input.attr('disabled')) return;

    if (!input.attr('checked')) {
        $('.custom-checkbox').each(function () {
            cInput = $(this).find('input').eq(0);
            if (cInput.attr('name') == input.attr('name')) {
                $(this).removeClass('active');
                cInput.attr("checked", false);
            }
        });
        el.addClass('active');
        input.attr("checked", true);
        input.change();
    }
    return true;
}
function changeCheckStart(el) {
    var el = el, input = el.find('input').eq(0);
    if (input.attr('checked')) {
        el.addClass('active');
    }
    return true;
}


function DisableRadioDipGiorni(appr_rif_scad) {
    $("#ul-dip-" + appr_rif_scad + " input[type=radio]").attr("disabled", "disabled");
    $("#ul-dip-" + appr_rif_scad + " span").addClass("disable-day");
    $("#dip-" + appr_rif_scad + "-s-oremin select").attr("disabled", "disabled");
}
function EnableRadioDipGiorni(appr_rif_scad) {
    $("#ul-dip-" + appr_rif_scad + " input[type=radio]").removeAttr("disabled");
    $("#ul-dip-" + appr_rif_scad + " span").removeClass("disable-day");
    $("#dip-" + appr_rif_scad + "-s-oremin select").removeAttr("disabled");
}
function DisableComboDipOreMin(appr_rif_scad) {
    $("#oremin-dip-" + appr_rif_scad + " select").attr("disabled", "disabled");
}
function EnableComboDipOreMin(appr_rif_scad) {
    $("#oremin-dip-" + appr_rif_scad + " select").removeAttr("disabled");
}


function DisableRadioL1Giorni(tipoEV) {
    $("#ul-l1-" + tipoEV + " input[type=radio]").attr("disabled", "disabled");
    $("#ul-l1-" + tipoEV + " span").addClass("disable-day");
    $("#l1-" + tipoEV + "-s-oremin select").attr("disabled", "disabled");
}
function EnableRadioL1Giorni(tipoEV) {
    $("#ul-l1-" + tipoEV + " input[type=radio]").removeAttr("disabled");
    $("#ul-l1-" + tipoEV + " span").removeClass("disable-day");
    $("#l1-" + tipoEV + "-s-oremin select").removeAttr("disabled");
}
function DisableComboL1OreMin(tipoEV) {
    $("#oremin-l1-" + tipoEV + " select").attr("disabled", "disabled");
}
function EnableComboL1OreMin(tipoEV) {
    $("#oremin-l1-" + tipoEV + " select").removeAttr("disabled");
}


function ImpostazioneChange(radio) {
    var idRadio = $(radio).attr("id");
    var ck = $(radio).prop('checked');

    switch (idRadio) {
        case "radio-dip-app-i":
            if (ck) { DisableRadioDipGiorni("appr"); DisableComboDipOreMin("appr"); }
            break;
        case "radio-dip-app-i-n":
            if (ck) { DisableRadioDipGiorni("appr"); DisableComboDipOreMin("appr"); }
            break;
        case "radio-dip-app-g":
            if (ck) { DisableRadioDipGiorni("appr"); EnableComboDipOreMin("appr"); }
            break;
        case "radio-dip-app-s":
            if (ck) { EnableRadioDipGiorni("appr"); DisableComboDipOreMin("appr"); }
            break;

        case "radio-dip-rif-i":
            if (ck) { DisableRadioDipGiorni("rif"); DisableComboDipOreMin("rif"); }
            break;
        case "radio-dip-rif-i-n":
            if (ck) { DisableRadioDipGiorni("rif"); DisableComboDipOreMin("rif"); }
            break;
        case "radio-dip-rif-g":
            if (ck) { DisableRadioDipGiorni("rif"); EnableComboDipOreMin("rif"); }
            break;
        case "radio-dip-rif-s":
            if (ck) { EnableRadioDipGiorni("rif"); DisableComboDipOreMin("rif"); }
            break;

        case "radio-dip-scad-i":
            if (ck) { DisableRadioDipGiorni("scad"); DisableComboDipOreMin("scad"); }
            break;
        case "radio-dip-scad-i-n":
            if (ck) { DisableRadioDipGiorni("scad"); DisableComboDipOreMin("scad"); }
            break;
        case "radio-dip-scad-g":
            if (ck) { DisableRadioDipGiorni("scad"); EnableComboDipOreMin("scad"); }
            break;
        case "radio-dip-scad-s":
            if (ck) { EnableRadioDipGiorni("scad"); DisableComboDipOreMin("scad"); }
            break;

        case "radio-l1-insr-i":
            if (ck) { DisableRadioL1Giorni("insr"); DisableComboL1OreMin("insr"); }
            break;
        case "radio-l1-insr-i-n":
            if (ck) { DisableRadioL1Giorni("insr"); DisableComboL1OreMin("insr"); }
            break;
        case "radio-l1-insr-g":
            if (ck) { DisableRadioL1Giorni("insr"); EnableComboL1OreMin("insr"); }
            break;
        case "radio-l1-insr-s":
            if (ck) { EnableRadioL1Giorni("insr"); DisableComboL1OreMin("insr"); }
            break;

        case "radio-l1-inss-i":
            if (ck) { DisableRadioL1Giorni("inss"); DisableComboL1OreMin("inss"); }
            break;
        case "radio-l1-inss-i-n":
            if (ck) { DisableRadioL1Giorni("inss"); DisableComboL1OreMin("inss"); }
            break;
        case "radio-l1-inss-g":
            if (ck) { DisableRadioL1Giorni("inss"); EnableComboL1OreMin("inss"); }
            break;
        case "radio-l1-inss-s":
            if (ck) { EnableRadioL1Giorni("inss"); DisableComboL1OreMin("inss"); }
            break;

        case "radio-l1-scad-i":
            if (ck) { DisableRadioL1Giorni("scad"); DisableComboL1OreMin("scad"); }
            break;
        case "radio-l1-scad-i-n":
            if (ck) { DisableRadioL1Giorni("scad"); DisableComboL1OreMin("scad"); }
            break;
        case "radio-l1-scad-g":
            if (ck) { DisableRadioL1Giorni("scad"); EnableComboL1OreMin("scad"); }
            break;
        case "radio-l1-scad-s":
            if (ck) { EnableRadioL1Giorni("scad"); DisableComboL1OreMin("scad"); }
            break;

        case "radio-l1-urg-i":
            if (ck) { DisableRadioL1Giorni("urg"); DisableComboL1OreMin("urg"); }
            break;
        case "radio-l1-urg-i-n":
            if (ck) { DisableRadioL1Giorni("urg"); DisableComboL1OreMin("urg"); }
            break;
        case "radio-l1-urg-g":
            if (ck) { DisableRadioL1Giorni("urg"); EnableComboL1OreMin("urg"); }
            break;
        case "radio-l1-urg-s":
            if (ck) { EnableRadioL1Giorni("urg"); DisableComboL1OreMin("urg"); }
            break;
    }

}
function SomethingChanged(input) {
    var id = $(input).attr("id");

    if ($(input).is("select")) {
        var divContainer = $(input).closest(".dominio-radio");
        var radiobutton = $(divContainer).find("input[type=radio]");
        id = $(radiobutton).attr("id");
    }
    if ($(input).is("input[type=radio]") && id.indexOf("-day-") >= 0) {
        var divContainer = $(input).closest(".dominio-radio");
        var radiobutton = $(divContainer).find("input[type=radio]");
        id = $(radiobutton).attr("id");
    }
    if (id.indexOf('radio') == 0) {
        if (id.split('-')[3] == "i") {
            ProfiloUpdate(id, null, null, null);
        }
        if (id.split('-')[3] == "g") {
            //DIP
            if (id == "radio-dip-app-g") {
                ProfiloUpdate(id, null, $("#ore-appr-g").val(), $("#min-appr-g").val());
            }
            if (id == "radio-dip-rif-g") {
                ProfiloUpdate(id, null, $("#ore-rif-g").val(), $("#min-rif-g").val());
            }
            if (id == "radio-dip-scad-g") {
                ProfiloUpdate(id, null, $("#ore-scad-g").val(), $("#min-scad-g").val());
            }
            // LIVELLO 1
            if (id == "radio-l1-insr-g") {
                ProfiloUpdate(id, null, $("#ore-insr-g").val(), $("#min-insr-g").val());
            }
            if (id == "radio-l1-inss-g") {
                ProfiloUpdate(id, null, $("#ore-inss-g").val(), $("#min-inss-g").val());
            }
            if (id == "radio-l1-scad-g") {
                ProfiloUpdate(id, null, $("#ore-scadl1-g").val(), $("#min-scadl1-g").val());
            }
            if (id == "radio-l1-urg-g") {
                ProfiloUpdate(id, null, $("#ore-urg-g").val(), $("#min-urg-g").val());
            }
        }
        if (id.split('-')[3] == "s") {
            //DIP

            if (id == "radio-dip-app-s") {
                var giorno = $("input:radio[name='appr'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-appr-s").val(), $("#min-appr-s").val());
            }
            if (id == "radio-dip-rif-s") {
                var giorno = $("input:radio[name='rif'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-rif-s").val(), $("#min-rif-s").val());
            }
            if (id == "radio-dip-scad-s") {
                var giorno = $("input:radio[name='scad'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-scad-s").val(), $("#min-scad-s").val());
            }
            //LIVELLO 1
            if (id == "radio-l1-insr-s") {
                var giorno = $("input:radio[name='insr'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-insr-s").val(), $("#min-insr-s").val());
            }
            if (id == "radio-l1-inss-s") {
                var giorno = $("input:radio[name='inss'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-inss-s").val(), $("#min-inss-s").val());
            }
            if (id == "radio-l1-scad-s") {
                var giorno = $("input:radio[name='scadl1'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-scadl1-s").val(), $("#min-scadl1-s").val());
            }
            if (id == "radio-l1-urg-s") {
                var giorno = $("input:radio[name='urg'][checked=checked]").val();
                if (giorno === undefined) giorno = 1;
                ProfiloUpdate(id, giorno, $("#ore-urg-s").val(), $("#min-urg-s").val());
            }
        }
    }


}

function ProfiloUpdate(ClickedObjectID, day, ore, min) {
    $("#savedb").show();
    $.ajax({
        type: 'GET',
        url: "/ProfiloPersonale/ProfiloUpdate",
        dataType: "json",
        data: { ClickedObject: ClickedObjectID, ore: ore, min: min, day: day },
        cache: false,
        success: function (data) {
            $("#savedb").fadeOut("slow", function () {
            });
            if (data.result != "OK") {
                swal(data.result, '', 'error');
            }
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
function CancellaDelega(da, a, matr, nome) {
    swal({
        title: 'Sei sicuro?',
        text: "La delega sarà eliminata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {

        $.ajax({
            type: 'POST',
            url: "/deleghe/elimina",
            dataType: "json",
            data: { da: da, a: a, matr: matr },
            cache: false,
            success: function (data) {
                if (data.result == "OK") {
                    $("#button-refresh-del").click();
                }
                else {
                    swal({
                        title: 'Errore',
                        text: data.result,
                        type: 'error',
                        showCancelButton: false,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function (aa, b, c) {
                swal(aa + b + c, '', 'error');
            },
            complete: function () {

            }
        });

    });
}
function AggiungiDelega() {
    $("#modal-deleghe").modal('show');
}

/*$("#deleghe-header").height()
28
$("#modal-deleghe").height()
$("#header-right").height()
544*/



function CanSubmitDelega() {
    var i = $("#dataInizioDelega").val().trim();
    var f = $("#dataFineDelega").val().trim();
    var del = $('input:checkbox.cb-dip:checked').length;
    if (i != "" && f != "" && del > 0) {
        $("#submit-delega").removeAttr("disabled");
        $("#submit-delega").removeClass("disable");
    }
    else {
        $("#submit-delega").attr("disabled", "disabled");
        $("#submit-delega").addClass("disable");
    }
}
function Nascondi(data, element) {
    swal({
        title: 'Confermi ?',
        text: "La giornata verrà considerata 'quadrata' e non apparirà più tra le evidenze",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {

        $.ajax({
            url: '/ajax/nascondi',
            type: "POST",
            data: { data: data },
            dataType: "json",

            success: function (data) {
                if (data.result == "OK") {
                    $(element).closest("tr").remove();
                }
                else {
                    swal("Errore 2", data.result, "error");
                }

            }
        });

    });
}

//ISTANZIA CLASSE CustomTour ///////////////////////////////////////////////
var t;
function StartTour(selector, scroller, forceStart) {
    t = new CustomTour(selector);
    t.forceStart = forceStart;
    t.scrollingContainer = scroller;
    t.begin();
}
///////////////////////////////////////////////////////////////////////////


//ISTANZIA CLASSE CustomTourIndex ///////////////////////////////////////////////
var tindex;
function StartTourIndex(selector) {
    $("#open-tour").hide();

    setTimeout(function () {

        introJs().setOption("nextLabel", "Avanti")
                 .setOption("prevLabel", "Indietro")
                 .setOption("skipLabel", "Esci")
                 .setOption("doneLabel", "Ok")
                 .onexit(function () { $("#open-tour").show(); })
                 .oncomplete(function () { $("#open-tour").show(); })
                 .start();
    }, 1000);

    // tindex = new CustomTourIndex(selector);
    // tindex.begin();
}
///////////////////////////////////////////////////////////////////////////

function StartTitoliTour(forced) {
    if ($("#flag-dip").prop("checked") == true)
        StartTour("#form-inserimentocv-dip", "#tit-studio .modal-content", forced);

    if ($("#flag-lau").prop("checked") == true)
        StartTour("#form-inserimentocv-lau", "#tit-studio .modal-content", forced);

    if ($("#flag-mas").prop("checked") == true)
        StartTour("#form-inserimentocv-mas", "#tit-studio .modal-content", forced);
}
function StartDiplomaLaureaSpecTour(forced) {

    $("#form-inserimentocv [data-tourstep]").each(function () { $(this).removeAttr("data-tourstep").removeAttr("data-tourtext"); });

    if ($("#_flagDiploma").prop("checked")) {

        $("#_codTitolo").closest("div").attr("data-tourstep", "1");
        $("#_codTitolo").closest("div").attr("data-tourtext", "Seleziona dal menu a tendina il tuo diploma, se non lo trovi nella lista segnalalo all'Academy");

        $("#_scala").closest("div").attr("data-tourstep", "20");
        $("#_scala").closest("div").attr("data-tourtext", "Segnala la scala di riferimento relativa alla tua votazione in cifre");

        $("#_istituto").closest("div").attr("data-tourstep", "30");
        $("#_istituto").closest("div").attr("data-tourtext", "Aprendo il menù a tendina, puoi selezionare l'ente erogatore in cui hai ottenuto il titolo di studio. Se non la trovi nell’elenco inseriscila a testo libero e successivamente premi il tasto tabulatore.");


        if (!forced
            &&
            document.cookie.indexOf("cv-titolo-dip=1") >= 0) return;

        StartTour("#form-inserimentocv", "#modalStudiesInserimnento .modal-content", true);
        createCookie("cv-titolo-dip", "1", 365);

    }

    if ($("#_flagLaurea").prop("checked")) {

        $("#_codTipoTitolo").closest("div").attr("data-tourstep", "1");
        $("#_codTipoTitolo").closest("div").attr("data-tourtext", "Seleziona dal menu a tendina il tuo tipo di laurea, se non lo trovi nella lista segnalalo all'Academy");


        $("#_corsoLaurea").closest("div").attr("data-tourstep", "20");
        $("#_corsoLaurea").closest("div").attr("data-tourtext", "Inserisci a testo libero il tuo corso di laurea (es. corso di Laurea in Lettere Moderne)");

        $("#_scala").closest("div").attr("data-tourstep", "30");
        $("#_scala").closest("div").attr("data-tourtext", "Segnala la scala di riferimento relativa alla tua votazione (110)");

        $("#_riconoscimento").closest("div").attr("data-tourstep", "40");
        $("#_riconoscimento").closest("div").attr("data-tourtext", "Inserisci a testo libero il tuo riconoscimento (es. con dignità di stampa, summa cum laude, ecc..)");


        $("#_titoloTesi").closest("div").attr("data-tourstep", "50");
        $("#_titoloTesi").closest("div").attr("data-tourtext", "Inserisci il titolo della tua tesi di laurea");

        $("#_codIstituto").closest("div").attr("data-tourstep", "60");
        $("#_codIstituto").closest("div").attr("data-tourtext", "Aprendo il menù a tendina, puoi selezionare l'ente erogatore in cui hai ottenuto il titolo di studio. Se non la trovi nell’elenco inseriscila a testo libero e successivamente premi il tasto tabulatore.");

        if (!forced
            &&
            document.cookie.indexOf("cv-titolo-lau=1") >= 0) return;

        StartTour("#form-inserimentocv", "#modalStudiesInserimnento .modal-content", true);
        createCookie("cv-titolo-lau", "1", 365);

    }

    if ($("#_flagSpecial").prop("checked")) {

        $("#_codTitolo").closest("div").attr("data-tourstep", "1");
        $("#_codTitolo").closest("div").attr("data-tourtext", "Seleziona dal menu a tendina la specializzazione, il titolo di merito o la borsa di studio ottenuta, se non lo trovi nella lista segnalalo all'Academy");


        $("#_titoloSpecializ").closest("div").attr("data-tourstep", "20");
        $("#_titoloSpecializ").closest("div").attr("data-tourtext", "Inserisci a testo libero il titolo della tua specializzazione post laurea (es. Gestione di impresa cinematografica e televisiva, Comunicazione e nuove tecnologie, ecc..)");

        $("#_scala").closest("div").attr("data-tourstep", "30");
        $("#_scala").closest("div").attr("data-tourtext", "Segnala la scala di riferimento relativa alla tua votazione, se presente");

        $("#_riconoscimento").closest("div").attr("data-tourstep", "40");
        $("#_riconoscimento").closest("div").attr("data-tourtext", "Inserisci il riconoscimento e/o il giudizio ricevuto (es. eccellente, summa cum laude, ecc..)");

        $("#_codIstituto").closest("div").attr("data-tourstep", "50");
        $("#_codIstituto").closest("div").attr("data-tourtext", "Aprendo il menù a tendina, puoi selezionare l'ente erogatore in cui hai ottenuto il titolo di studio. Se non la trovi nell’elenco inseriscila a testo libero e successivamente premi il tasto tabulatore.");

        if (!forced
          &&
          document.cookie.indexOf("cv-titolo-spe=1") >= 0) return;

        StartTour("#form-inserimentocv", "#modalStudiesInserimnento .modal-content", true);
        createCookie("cv-titolo-spe", "1", 365);
    }
}

function Handlers3() {
    $(document).keyup(function (e) {
        if (t != null && t.running) {
            if (e.keyCode == 39) t.gotoNextStep();
            if (e.keyCode == 37) t.gotoPrevStep();
        }
    });
    //aperture tour

    $(document).on('shown.bs.modal', '#frkModalAltreInfo', function () {

        StartTour("#dati-personali", "#frkModalAltreInfo .modal-content", false);
    });

    $(document).on('shown.bs.modal', '#modalStudiesInserimnento', function () {
        StartDiplomaLaureaSpecTour(false)
    });

    $(document).on('shown.bs.modal', '#modalExperiencesInserimnento', function () {
        StartTour("#EsperienzaRai", "#modalExperiencesInserimnento .modal-content", false);
    });

    $(document).on('shown.bs.modal', '#modalCertificazioniInserimento', function () {
        PuliziaCert();
        StartTour("#tour-attestato", "#modalCertificazioniInserimento .modal-content", false);
    });

    $(document).on('shown.bs.modal', '#modalFormazioneInserimento', function () {
        StartTour("#frmInserimentoFormazione", "#modalFormazioneInserimento .modal-content", false);
    });

    $(document).on('shown.bs.modal', '#modalAreeInteresseInserimnento', function () {
        StartTour("#form-insertareainteresse", "#modalAreeInteresseInserimnento .modal-content", false);
    });
    $(document).on('shown.bs.modal', '#tit-studio', function () {
        StartTitoliTour(false);
    });

    var eventiInterval = 0;
    var DataStartTimer = null;

    $('#modal-eventi').on('shown.bs.modal', function (event) {
        $(".pren-data").val('');
        if (eventiInterval != 0) clearInterval(eventiInterval);

        DataStartTimer = new Date();
        $("#eventi-timer").text($("#timeout-minuti").val() + ":00");
        var idevento = $("#idevento").val();
        getPostiPrenotati(idevento);

        $.ajax({
            url: '/events/SettaCampi',
            type: 'GET',
            cache: false,
            data: { idevento: idevento },
            dataType: "json",
            success: function (data) {
                if (data.result.length == 0) {
                    $("#nome_prenota").show();
                    $("#nome_info").show();
                    $("#cognome_prenota").show();
                    $("#cognome_info").show();
                    $("#datanascita_prenota").show();
                    $("#datanascita_info").show();
                    $("#citta_prenota").show();
                    $("#citta_info").show();
                    $("#genere_prenota").show();
                    $("#genere_info").show();
                    $("#email_prenota").show();
                    $("#email_info").show();
                    $("#telefono_prenota").show();
                    $("#telefono_info").show();
                    $("#tipodoc_prenota").show();
                    $("#tipodoc_info").show();
                    $("#numerodoc_prenota").show();
                    $("#numerodoc_info").show();
                    $("#grado_prenota").show();
                    $("#grado_info").show();
                    $('#nota_prenota').show();
                    $('#nota_info').show();
                } else {
                    $("#nome_prenota").show();
                    $("#nome_info").show();
                    $("#cognome_prenota").show();
                    $("#cognome_info").show();
                    for (var i = 0; i < data.result.length; i++) {
                        $("#" + data.result[i].descrizione + "_prenota").show();
                        $("#" + data.result[i].descrizione + "_info").show();
                        if (data.result[i].visibilie == false) {
                            $("#" + data.result[i].descrizione + "_prenota").hide();
                            $("#" + data.result[i].descrizione + "_info").hide();
                        }
                        if (data.result[i].visibilie == false) {
                            $("#" + data.result[i].descrizione + "_prenota").hide();
                            $("#" + data.result[i].descrizione + "_info").hide();
                        }
                        if (data.result[i].obbligatorio == false) {

                            if (data.result[i].descrizione == "doc") {
                                $('#numeroDocumento').rules('remove');
                                $('#tipoDocumento').rules('remove');
                                $('span[data-valmsg-for="tipoDocumento"]').remove()
                                $('span[data-valmsg-for="numeroDocumento"]').remove()

                            } else {
                                $("#" + data.result[i].descrizione).rules('remove');
                                $('span[data-valmsg-for="' + data.result[i].descrizione + '"]').remove();
                            }
                        }
                    }


                }
            }
        });

        $.ajax({
            url: '/events/SedeInsediamentoPossibile',
            type: 'GET',
            data: { idevento: idevento },
            dataType: "json",
            success: function (data) {
                if (data.result == true) {
                    $("#sedeInsediamento").val("True");
                    $("#sede_insediamento_prenota").show();
                    $("#sede_insediamento_info").show();
                } else {
                    $("#sedeInsediamento").val("False");
                    $("#sede_insediamento_prenota").hide();
                    $("#sede_insediamento_info").hide();
                }
            }
        });

        $.ajax({
            url: '/events/prenotazionePossibile',
            type: 'GET',
            data: { idevento: idevento },
            dataType: "json",
            success: function (data) {
                $("#datanascita").datetimepicker({
                    viewMode: 'years',
                    format: 'DD/MM/YYYY',
                    maxDate: 0
                });

                $('#datanascita').on('dp.hide', function (event) {
                    setTimeout(function () {
                        $('#datanascita').data('DateTimePicker').viewMode('years');
                    }, 1);
                });
                $('#datanascita').data("DateTimePicker").maxDate(new Date());

                if (data.result == true && OpzionaPostiAperturaPopup()==true) {
                    $(".pren-data").removeAttr("disabled");
                    $("#button-pren-anag").removeAttr("disabled");
                    eventiInterval = setInterval(
                        function () { AggiornaTimer(); }, 1000);
                }
                else {
                    $(".pren-data").attr("disabled", "disabled");
                    $("#button-pren-anag").attr("disabled", "disabled");
                }
            }
        });
    });
    $("#aggiornatimer").on("click", function () {
        DataStartTimer = new Date();
        var idevento = $("#idevento").val();
        $.ajax({
            url: '/events/AggiornaDataPrenotazione',
            type: "POST",
            data: { idevento: idevento },
            dataType: "json",
            complete: function () { },
            success: function (data) {
            }
        });
        $("#eventi-timer").text($("#timeout-minuti").val() + ":00");
        $("#button-pren-anag").show();
        $("#button-mod-anag").hide();
        $("#button-ann-anag").hide();
        DatiPrenotazione();
    }
    );

    $('#modal-eventi').on('hide.bs.modal', function () {

        RimuoviOpzioniPostiEvento();
        if (eventiInterval != 0) clearInterval(eventiInterval);
        var backurl = $("#backurl").val();
        if (backurl.trim() != "") {
            location.href = decodeURIComponent(backurl);
        }
    });

    $('#modal-infoeventi').on('shown.bs.modal', function (event) {
        $(".pren-data").val('');
        var idevento = $("#idevento").val();
        getInfoPostiPrenotati(idevento);
    });

    $('#modal-infoeventi').on('hide.bs.modal', function () {

        var backurl = $("#backurl").val();
        if (backurl.trim() != "") {
            location.href = decodeURIComponent(backurl);
        }
    });
    $("#form-events").submit(function (e) {

        e.preventDefault();
        var form = $(this);

        $("#datanascita").datetimepicker({
            viewMode: 'years',
            format: 'DD/MM/YYYY',
            maxDate: 0
        });


        $('#datanascita').on('dp.hide', function (event) {
            setTimeout(function () {
                $('#datanascita').data('DateTimePicker').viewMode('years');
            }, 1);
        });
        $('#datanascita').data("DateTimePicker").maxDate(new Date());

        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data.trim() == "OK") {
                    if ($("#idPrenotazione").val() == "0") {
                        $("#npostiprenotati").text(+$("#npostiprenotati").text() - 1);
                        $("#posti-miei-" + $("#idevento").val()).text($("#npostiprenotati").text());
                        $("#posti-mieipre-" + $("#idevento").val()).text($("#npostiprenotati").text());
                        $("#postirimanenti").text(+$("#postirimanenti").text() - 1);
                        ModificaEvento = true;

                    }
                    DatiPrenotazione();
                    $("#nome").val("");



                    swal({
                        title: 'Inserimento avvenuto con successo',
                        //text: "Vuoi inserire un'altra prenotazione?",
                        type: 'success',
                        showCancelButton: true,
                        //confirmButtonColor: '#3085d6',
                        //cancelButtonColor: '#d33',
                        cancelButtonText: 'Continua',
                        confirmButtonText: 'Ho finito'
                    }).then(function (value) {
                        EsciEvento();
                    }, function () {
                        if (OpzionaPostiAperturaPopup() == true) {
                            $(".pren-data").removeAttr("disabled");
                            $("#button-pren-anag").removeAttr("disabled");
                            eventiInterval = setInterval(
                                function () { AggiornaTimer(); }, 1000);
                        }
                        else {
                            $(".pren-data").attr("disabled", "disabled");
                            $("#button-pren-anag").attr("disabled", "disabled");
                        }
                    });
                }
                else {
                    swal("Errore", data + "(" + data.length + ")", "error")
                }
            }
        });
    });

    $("#form-savequest").submit(function (e) {
        e.preventDefault();
        $("li.previous").addClass("disabled disable");
        $("li.finish").addClass("disabled disable");

        var form = $(this);
        var msg = $("#messaggio-feedback").val();
        var idform = $("#id-form").val();
        var vediStat = $("#vedi-stat").val();
        var idTipologia = $("#id-tipologia").val();
        var formAlbergo = $("#form-albergo").val();
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data == "OK") {
                    
                    swal('OK',
                    $.trim(msg) == "" ? 'Dati salvati con successo' : msg,
                    'info')
                        .then(function () {
                            if (vediStat.toLowerCase() == "true") {
                                if (formAlbergo == "false")
                                    location.href = "/formstats/getstats?idform=" + idform;
                                else {
                                    var idDomanda = $('div[data-domanda-albergo=true]').attr('data-domanda-id');
                                    var idAlbergo = $('input[data-domandaid=' + idDomanda + ']:checked').val();
                                    location.href = "/formstats/getstats?idform=" + idform + "&idHotel=" + idAlbergo;
                                }
                            }
                            else {
                                location.href = "http://www.raiplace.rai.it/";
                            } 

                        });
                }
                else {
                    $("li.previous").addClass("disabled disable");
                    $("li.finish").addClass("disabled disable");
                    swal("Errore", data, "error")
                }
            }
        });
    });

    $(document.body).on("click", ".quest-link", function (event) {
        event.cancelBubble = true; if (event.stopPropagation) { event.stopPropagation(); }
    });





    $(document.body).on("click", ".taglia-tab", function (e) {
        var idtable = $(e.target).attr("data-tab");
        $("#" + idtable).find("tr").each(function () { $(this).removeClass("hi") })
        $(e.target).parent("td").find("a").addClass("gray-a").removeClass("pointer");
        $(e.target).parent("td").next().find("a").removeClass("gray-a").addClass("pointer").addClass("tornasu");

        var target = e.target;

        $("#" + idtable).parents('.collapse').on('hidden.bs.collapse', function (e) {
            limitaTR(idtable, parseInt('3', 10));
            $(target).parent("td").find("a").removeClass("gray-a").addClass("pointer");
            $(target).parent("td").next().find("a").addClass("gray-a").removeClass("pointer").removeClass("tornasu");
        });
    });

    $(document.body).on("click", ".tornasu", function (e) {
        if ($(e.target).hasClass("gray-a")) return;
        var iddiv = $(e.target).attr("data-div");
        $(document).scrollTop($("#" + iddiv).offset().top - 70);
    });
    $(document.body).on("click", ".stop-prop-execute", function (e) {
        e.stopPropagation();
        e.preventDefault();
        eval($(this).attr("data-execute"))
    });

    $(document.body).on("change", ".select-altro", function (e) {
        if ($(this).find("option:selected").text() == "Altro...") {
            $(this).closest("div").find("input:text").each(function () {
                $(this).removeAttr("disabled");
                $(this).show();
            });
        }
        else {
            $(this).closest("div").find("input:text").each(function () {
                $(this).attr("disabled", "disabled");
                $(this).hide();
            });
        }
    });

    $(document.body).on("change", ".radio-risposta", function (e) {
        var iddom = $(this).attr("data-domandaid");
        $("#tb-altro-" + iddom).attr("disabled", "disabled");
        $("#tb-altro-" + iddom).val("");
        $("#tb-altro-" + iddom).hide();
    });

    $(document.body).on("change", ".checkbox-altro,.radio-altro", function (e) {
        var id = $(this).attr("id").split('-')[2];
        var tb = $("#tb-altro-" + id);
        if ($(this).is(":checked")) {
            $(tb).show();
            $(tb).removeAttr("disabled");
        }
        else {
            $(tb).hide();
            $(tb).attr("disabled", "disabled");
        }
    });

    $(document.body).on("submit", '#form-primario', function (e) {
        e.preventDefault();

        var validator = $("#form-primario").validate();

        if (!$("#form-primario").valid()) {
            validator.focusInvalid();
            return false;
        }

        var form = $(this);
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data == "OK") {
                    location.href = "/formadmin";
                }
                else {
                    swal("Errore", data, "error")
                }
            }
        });
    });

    $(document.body).on("submit", '#form-secondario', function (e) {
        e.preventDefault();

        var validator = $("#form-secondario").validate();

        if (!$("#form-secondario").valid()) {
            validator.focusInvalid();
            return false;
        }

        var form = $(this);
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data == "OK") {
                    location.href = "/formadmin";
                }
                else {
                    swal("Errore", data, "error")
                }
            }
        });
    });

   

    $(document.body).on("submit", '#form-question', function (e) {
        e.preventDefault();

        var validator = $("#form-question").validate();

        if (!$("#form-question").valid()) {
            validator.focusInvalid();
            return false;
        }

        var form = $(this);
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data == "OK") {
                    location.href = "/formadmin";
                }
                else {
                    swal("Errore", data, "error")
                }
            }
        });
    });

    $("#form-profili").submit(function (e) {
        e.preventDefault();
        var form = $(this);
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data == "OK") {
                    location.href = "/profilimenu/";
                }
                else {
                    swal("Errore", data, "error")
                }
            }
        });
    });



    function DatiPrenotazione() {
        resettaTimer();
        $(".pren-data").val("");
        var idevento = $("#idevento").val();
        $("#idPrenotazione").val("0");
        getPostiPrenotati(idevento);
        CheckIfPrenPossibile();
        $("#button-refresh-eventi").click();
    }

    function resettaTimer() {
        DataStartTimer = new Date();
        $("#button-pren-anag").show();
        $("#button-mod-anag").hide();
        $("#button-ann-anag").hide();
        var idevento = $("#idevento").val();
        $.ajax({
            url: '/events/AggiornaDataPrenotazione',
            type: "POST",
            data: { idevento: idevento },
            dataType: "json",
            complete: function () { },
            success: function (data) {
            }
        });
    }

    function OpzionaPostiAperturaPopup() {
        var isOk = true;
        var idevento = $("#idevento").val();
        $("#button-pren-anag").show();
        $("#button-mod-anag").hide();
        $("#button-ann-anag").hide();
        $.ajax({
            async: false,
            url: '/events/opzionaPostiEvento',
            type: "POST",
            data: { idevento: idevento },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.result == "OK") {
                    isOk = true;
                }
                else {
                    isOk = false;
                    //swal("Errore", data.result, "error").then(function () { location.reload(); });
                    swal("Errore", data.result, "error").then(function () {
                        
                    });
                }
            }
        });
        return isOk;
    }

    function RimuoviOpzioniPostiEvento() {
        var idevento = $("#idevento").val();
        $.ajax({
            url: '/events/rimuoviOpzioniPostiEvento',
            type: "POST",
            data: { idevento: idevento },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.result == "OK") {

                }
                else {
                    swal("Errore", data.result, "error");
                }
            }
        });
    }
    function AggiornaTimer() {
        var dataNow = new Date();
        var msecRimanenti = ($("#timeout-minuti").val() * 60000) - (dataNow - DataStartTimer);
        if (msecRimanenti <= 0) {

            if (eventiInterval != 0) clearInterval(eventiInterval);
            swal("Errore", "Il tempo a tua disposizione è scaduto!", "error").then(function () {
                RimuoviOpzioniPostiEvento();
                if (window.location.href.toLowerCase().indexOf("backurl") >= 0) {
                    var url = window.location.href.split('?')[1];
                    var params = url.split('&');
                    for (var i = 0; i < params.length; i++) {
                        var p = params[i].split('=');
                        if (p[0].toLowerCase() == "backurl") {
                            location.href = decodeURIComponent(p[1]);
                        }
                    }
                }
                else
                    location.reload();
            });
        }
        else {
            var min = parseInt(msecRimanenti / 60000);
            var sec = Math.round((msecRimanenti - (min * 60000)) / 1000);
            $("#eventi-timer").text(min.toString() + ":" + (sec < 10 ? "0" + (sec.toString()) : sec.toString()));
        }
    }

    function AggiornaPosti() {
        $.ajax({
            url: '/events/getPostiDisponibili',
            type: "GET",
            data: { idevento: $("#idevento").val() },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.errore != "") {
                    if (eventiInterval != 0) clearInterval(eventiInterval);
                    swal("Errore", data.errore, "error").then(function () { location.reload(); });
                }
                else {
                    $("#eventi-posti").text(data.result);
                    if (data.result == "0" || data.result == 0) {
                        swal("Errore", "I posti disponibili sono terminati", "error").then(function () { location.reload(); });
                    }
                }
            }
        });
    }

    $('#form-delega').submit(function (e) {
        e.preventDefault();
        var form = $(this);

        var delegati = "";
        $('input:checkbox.cb-dip:checked').each(function () {

            var nome = $(this).closest("div").parent("div").prev("div").text().trim();
            if (delegati == "") delegati += nome;
            else delegati += ", " + nome;
        });
        swal({
            title: 'Confermi delega?',
            text: "Stai delegando per il periodo " + $("#dataInizioDelega").val() + " - " + $("#dataFineDelega").val() + ": " + delegati,
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Conferma',
            cancelButtonText: 'Annulla'
        }).then(function () {


            $("#submit-delega").attr("disabled", "disabled");


            var serializedForm = form.serialize();

            $.ajax({
                url: '/deleghe/save',
                type: "POST",
                data: serializedForm,
                dataType: "json",
                complete: function () { },
                success: function (data) {

                    $("#submit-delega").removeAttr("disabled");
                    if (data.result == "OK") {
                        $('#modal-deleghe').modal("hide");
                        $("#button-refresh-del").click();
                    }
                    else {
                        swal({
                            title: 'Errore',
                            text: data.result,
                            type: 'error',
                            showCancelButton: false,
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'OK'
                        });
                    }
                }
            });
        });



    });

    $(document.body).on("change", '.cb-dip', function (e) {
        CanSubmitDelega();
    });

    $("#dataInizioDelega,#dataFineDelega").on("dp.change", function () {
        CanSubmitDelega();
    });

    $('#modal-deleghe').on('shown.bs.modal', function (event) {
        $("#dipendenti-deleghe").addClass("block-opt-refresh");
        var h = $("#modal-deleghe").outerHeight() - $("#deleghe-header").outerHeight() - $("#header-right").outerHeight();

        $("#dataInizioDelega").val("");
        $("#dataFineDelega").val("");
        $("#submit-delega").attr("disabled", "disabled").addClass("disable");
        $("#dipendenti-deleghe").css("height", h - 100 + "px");
        $("#dipendenti-deleghe").load("/deleghe/dipendenti", function () {
            $("#dipendenti-deleghe").css("height", "");
            $("#dipendenti-deleghe").removeClass("block-opt-refresh");
        });
    });




    $("[data-enabled-if]").each(function () {
        var att = ($(this).attr("data-enabled-if"));

        var arrElements = getElements(att);

        for (var i = 0; i < arrElements.length; i++) {
            var $el = $("#" + arrElements[i].trim());

            $el.attr("data-enabling-el", $(this).attr("id"));
            $el.attr("data-enabling-expr", att);

            $el.on('input propertychange paste change', function () {
                changed(this);
            });
            $el.change();
        }
    });

    function changed(el) {
        var str = $(el).attr("data-enabling-expr");
        var arrEl = getElements(str);
        for (var k = 0; k < arrEl.length; k++) {
            var v = isElementVal(arrEl[k]);
            var reg = new RegExp(arrEl[k], "g");
            str = str.replace(reg, v);
        }

        if (eval(str) == true) {
            $("#" + $(el).attr("data-enabling-el")).removeAttr("disabled");
            $("#" + $(el).attr("data-enabling-el")).removeClass("disable");
        }
        else {
            $("#" + $(el).attr("data-enabling-el")).attr("disabled", "disabled");
            $("#" + $(el).attr("data-enabling-el")).addClass("disable");
        }
    }

    function getElements(expr) {
        var mod = expr.replace(new RegExp("[&|()]", "g"), '');
        var r = mod.split(' ');
        r = r.filter(function (item) { return item.trim() != ''; });
        return r;
    }
    function isElementVal(id) {
        var $el = $("#" + id);
        if ($el.is("input[type=checkbox]")) {
            return $el.prop("checked");
        }
        else
            return $.trim($el.val()) != "";
    }

    $(".tb-precomp").each(function () {

        var res = GetPrecompilati();
        var prop = $(this).attr("data-precomp");
        $(this).val(res[prop]);
    });

}

function JsSelect2Ext(classValue) {
    this.classValue = classValue;
}

JsSelect2Ext.prototype.init = function () {
    $(this.classValue).select2({ minimumResultsForSearch: -1 });
    $(this.classValue).on("select2:select", function (event) {
        var value = $(event.currentTarget).find("option:selected").attr('title');
        $(event.currentTarget).parent().find('.select2-selection__rendered').html(value);
    });
    $(this.classValue).each(function () {
        var value = $(this).find("option:selected").attr('title');
        $(this).parent().find('.select2-selection__rendered').html(value);
    });

    $('.select2-results__option--highlighted[aria-selected]').each(function () {
        $(this).parent().parent().addClass('select2-results__option--highlighted').attr('aria-selected', 'true');
    });
}

function PrenotaEvento(id) {
    var privacy = $("#id_visione").val();

    if (privacy != "") {
        $('#modal-privacy-general').modal('show');
        $('#modal-privacy-general').modal({ backdrop: 'static', keyboard: false });
    }

    $.ajax({
        url: '/events/controlEvento',
        type: "GET",
        data: { id: id },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            if (data.result == -1) {
                $('#modal-noteventi').modal('show');
                $("#nota-evento").text("La sua sede non è abilitata alla prenotazione");
            }
            else if (data.result == -2){
                $('#modal-noteventi').modal('show');
                $("#nota-evento").text("La sua matricola non è abilitata alla prenotazione");
            }
            else if (data.result != "0") {
                $("#idevento").val(id);
                $('#modal-eventi').modal('show');
                getInfoEvento(id);
            } else {
                $('#modal-noteventi').modal('show');
                $('#modal-noteventi').modal("L'evento richiesto risulta chiuso o non disponibile");
            }
        }
    });
}

function InfoEvento(id) {
    $("#idevento").val(id);
    $('#modal-infoeventi').modal('show');
    getInfoInfoEvento(id);
}

function getInfoEvento(id) {
    $.ajax({
        url: '/events/getInfoEvento',
        type: "GET",
        data: { id: id },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            ModificaEvento = false;
            $("#info-evento").html(data);
        }
    });
}

function getInfoInfoEvento(id) {
    $.ajax({
        url: '/events/getInfoEvento',
        type: "GET",
        data: { id: id },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#infoinfo-evento").html(data);
            $(".pren-data").attr("disabled", "disabled");
        }
    });
}

function AggiornaPostiDisponibiliAll() {
}

function getPosti() {

    $.ajax({
        url: '/events/getPostiDisponibiliAll',
        type: "GET",
        data: {},
        dataType: "json",
        complete: function () { },
        success: function (data) {
        }
    });
}

function getPostiPrenotati(idevento) {
    $.ajax({
        url: '/events/getPostiPrenotati',
        type: "GET",
        data: { idevento: idevento },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#posti-prenotati").html(data);
        }
    });
}

function getInfoPostiPrenotati(idevento) {
    $.ajax({
        url: '/events/getInfoPostiPrenotati',
        type: "GET",
        data: { idevento: idevento },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#infoposti-prenotati").html(data);
        }
    });
}

function EliminaPrenotazioneIdAn(id, idevento) {

    swal({
        title: 'Sei sicuro?',
        text: "La prenotazione sarà eliminata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: '/events/eliminaAnagraficaPrenotazione',
            type: "POST",
            data: { id: id },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.result == "OK") {

                    $("#aggiornatimer").click();
                    $("#npostiprenotati").text(+$("#npostiprenotati").text() + 1);
                    $("#posti-miei-" + $("#idevento").val()).text($("#npostiprenotati").text());
                    $("#posti-mieipre-" + $("#idevento").val()).text($("#npostiprenotati").text());
                    $("#postirimanenti").text(+$("#postirimanenti").text() + 1);
                    getPostiPrenotati(idevento);
                    CheckIfPrenPossibile();
                    AggiornaTotaliPren();
                    $("#button-refresh-eventi").click();
                    ModificaEvento = true;

                    swal({
                        title: 'Cancellazione avvenuta con successo',
                        //text: "Vuoi inserire un'altra prenotazione?",
                        type: 'success',
                        showCancelButton: true,
                        //confirmButtonColor: '#3085d6',
                        //cancelButtonColor: '#d33',
                        cancelButtonText: 'Continua',
                        confirmButtonText: 'Ho finito'
                    }).then(function (value) {
                        EsciEvento();
                    });
                }
                else {
                    swal("Errore 1", data.result, "error")
                }
            }
        });
    });
}

function EliminaPrenIdAnWidget(id, idevento) {

    swal({
        title: 'Sei sicuro?',
        text: "La prenotazione sarà eliminata",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {


        $.ajax({
            url: '/events/eliminaAnagraficaPrenotazione',
            type: "POST",
            data: { id: id },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.result == "OK") {

                    //getPostiPrenotati(idevento);
                    //CheckIfPrenPossibile();
                    //AggiornaTotaliPren();
                    //$("#button-refresh-eventi").click();
                    //ModificaEvento = true;

                    swal({
                        title: 'Cancellazione avvenuta con successo',
                        type: 'success'
                    }).then(function (value) {
                        EsciEvento();
                    });
                }
                else {
                    swal("Errore 1", data.result, "error")
                }
            }
        });

    });
}

function EliminaPrenEvento(idEvento) {
    swal({
        title: 'Sei sicuro?',
        text: "Le prenotazioni per questo evento saranno eliminate",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {

        $.ajax({
            url: '/events/eliminaPrenEvento',
            type: "POST",
            data: { idEvento: idEvento },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.result == "OK") {

                    //getPostiPrenotati(idevento);
                    //CheckIfPrenPossibile();
                    //AggiornaTotaliPren();
                    //$("#button-refresh-eventi").click();
                    //ModificaEvento = true;

                    swal({
                        title: 'Cancellazione avvenuta con successo',
                        type: 'success'
                    }).then(function (value) {
                        EsciEvento();
                    });
                }
                else {
                    swal("Errore 1", data.result, "error")
                }
            }
        });

    });
}

function CheckIfPrenPossibile() {
    $.ajax({
        url: '/events/prenotazionePossibile',
        type: 'GET',
        data: { idevento: $("#idevento").val() },
        dataType: "json",
        success: function (data) {

            if (data.result == true) {
                $(".pren-data").removeAttr("disabled");
                $("#button-pren-anag").removeAttr("disabled");
            }
            else {
                $(".pren-data").attr("disabled", "disabled");
                $("#button-pren-anag").attr("disabled", "disabled");
            }
        }
    });
}
function AggiornaTotaliPren() {
    $.ajax({
        url: '/events/getTot',
        type: 'GET',
        data: {},
        dataType: "json",
        success: function (data) {
            $("#pren-attive").text(data.totPren);
            $("#eventi-tot").text(data.totEventi);
        }
    });

}
function newProfilo() {
    $("#modal-profili").modal("show");
    $("#idprofilo").val("");
    $.ajax({
        url: '/profilimenu/datiprofilo',
        type: 'GET',
        data: { id: 0 },
        dataType: "html",
        success: function (data) {
            $("#datiprofilo").html(data);
        }
    });
    $.ajax({
        url: '/profilimenu/datimenu',
        type: 'GET',
        data: { id: 0 },
        dataType: "html",
        success: function (data) {
            $("#datimenu").html(data);
        }
    });
}

function modProfilo(idProfilo) {
    $("#modal-profili").modal("show");
    $("#idprofilo").val(idProfilo);
    $.ajax({
        url: '/profilimenu/datiprofilo',
        type: 'GET',
        data: { id: idProfilo },
        dataType: "html",
        success: function (data) {
            $("#datiprofilo").html(data);
        }
    });

    $.ajax({
        url: '/profilimenu/datimenu',
        type: 'GET',
        data: { id: idProfilo },
        dataType: "html",
        success: function (data) {
            $("#datimenu").html(data);
        }
    });
}
function delProfilo(idProfilo) {
    swal({
        title: 'Sei sicuro?',
        text: "Il profilo sarà eliminato",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {

        $.ajax({
            url: '/profilimenu/delprofilo',
            type: 'GET',
            data: { id: idProfilo },
            dataType: "json",
            success: function (data) {
                if (data.result == "OK")
                    $(".id-" + idProfilo).remove();
                else
                    swal(data.result, '', 'error');
            }
        });
    });
}


function showCalAnnuale(anno) {
    RaiOpenAsyncModal('modal-calendarioAnnuale', "/FeriePermessi/GetCalendarioAnnuale", { anno: anno });

    //$("#calendarioAnnuale-content").load("/FeriePermessi/GetCalendarioAnnuale?anno=" + anno);
    //$("#calendarioAnnuale-div").modal("show");
}

function showCalAnnualePF(anno) {

   
    if ($("#esentato-ferie").val() == "1") {
        swal("Sei stato esentato dalla compilazione del piano ferie. Per informazioni puoi contattare la Segreteria", '', 'error');
        return;
    }

    var isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);
    if (!isChrome) {

        swal("La compilazione del Piano Ferie richiede il browser Chrome", '', 'error');
        return;
    }
    $("#calendarioAnnuale-pf-content").addClass("rai-loader");

    //  $("#calendarioAnnuale-pf-content").html('<div id="wait-piano" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');

    $("#calendarioAnnuale-pf-content").load("/FeriePermessi/GetCalendarioAnnualePF?anno=" + anno, function () { $("#calendarioAnnuale-pf-content").removeClass("rai-loader"); });

   $("#calendarioAnnuale-pf").modal("show");
}


// DETASSAZIONE
function showDetassazioneForm(anno, codice, codiceDetassazione) {
    $("#moduloDetassazione-content").html('<div id="wait-detassazione" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#moduloDetassazione-content").load("/Detassazione/GetFormDetassazione?anno=" + anno + "&codice=" + codice + "&codiceDetassazione=" + codiceDetassazione);
    $("#moduloDetassazione").modal("show");
}

function showDetassazionePDF(anno, codice, codiceDetassazione) {
    $("#moduloDetassazione-content").html('<div id="wait-detassazione" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#moduloDetassazione-content").load("/Detassazione/GetPDFViewer?anno=" + anno + "&codice=" + codice + "&codiceDetassazione=" + codiceDetassazione);
    $("#moduloDetassazione").modal("show");
}

function getDetassazionePDF(anno, matricola, codiceDetassazione) {
    $("#moduloDetassazione-content").html('<div id="wait-detassazione" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#moduloDetassazione-content").load("/GestioneDetassazione/GetPDFViewer?anno=" + anno + "&matricola=" + matricola + "&codiceDetassazione=" + codiceDetassazione);
    $("#moduloDetassazione").modal("show");
}


// BOX BONUS 100 EURO
function showBoxBonus100Form(anno) {
    $("#boxbonus100-content").html('<div id="wait-boxbonus100" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#boxbonus100-content").load("/Bonus/GetFormBonus100?anno=" + anno);
    $("#boxbonus100").modal("show");
}

function showBoxBonus100ReadOnly(anno, scelta) {
    $("#boxbonus100-content").html('<div id="wait-boxbonus100" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#boxbonus100-content").load("/Bonus/GetFormBonus100ReadOnly?anno=" + anno + "&scelta=" + scelta);
    $("#boxbonus100").modal("show");
}

function showBoxBonus100PDF(anno, scelta) {
    $("#boxbonus100-content").html('<div id="wait-boxbonus100" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#boxbonus100-content").load("/Bonus/GetFormBonus100PDFViewer?anno=" + anno);
    $("#boxbonus100").modal("show");
}

// BOX MODULI
function showBoxModulo(url) {
    $("#boxModuli-content").html('<div id="wait-modulo" style="margin-top:50px;margin-left:55px"><i class="fa fa-refresh fa-spin" style="font-size: 22px; color: #aaa;margin-right:8px"></i> &nbsp;ATTENDERE... </div>');
    $("#boxModuli-content").load(url);
    $("#boxModuli").modal("show");
}

// FORM ADMIN

function newForm(id) {
    //if (id == 0) $("#tit-prim").text("Nuovo Form Primario");
    //else $("#tit-prim").text("Modifica Form");

    //$("#form-primario-content").load("/formadmin/getFormPrimario?id=" + id);
    //$("#modal-form-primario").modal("show");

    RaiOpenAsyncModal('modal-form-primario', '/formadmin/getFormPrimario', { id: id });
}

function newFormSec(id, idPrimario) {
    //if (id == 0) $("#tit-sec").text("Nuovo Form Secondario");
    //else $("#tit-sec").text("Modifica Form");

    //$("#form-secondario-content").load("/formadmin/getFormSecondario?id=" + id + "&idPrimario=" + idPrimario);
    //$("#modal-form-secondario").modal("show");

    RaiOpenAsyncModal('modal-form-secondario', '/formadmin/getFormSecondario', { id: id, idPrimario:idPrimario });
}

function newQuestion(id, idSecondario) {

    //if (id == 0) $("#tit-que").text("Nuova voce");
    //else $("#tit-que").text("Modifica voce");

    //$("#form-question-content").load("/formadmin/getFormQuestion?id=" + id + "&idSecondario=" + idSecondario,
    //    function () {
    //        var selez = $("#precomp-selez").val();
    //        var a = Object.keys(GetPrecompilati());
    //        a.forEach(function (item) {
    //            $("#select-precomp").append("<option value='" + item + "' " + (item == selez ? "selected" : "") + ">" + item + "</option>");
    //        });
    //    });
    //$("#modal-form-question").modal("show");

    RaiOpenAsyncModal('modal-form-question', '/formadmin/getFormQuestion', { id: id, idSecondario: idSecondario }, function () {
        var selez = $("#precomp-selez").val();
        var a = Object.keys(GetPrecompilati());
        a.forEach(function (item) {
            $("#select-precomp").append("<option value='" + item + "' " + (item == selez ? "selected" : "") + ">" + item + "</option>");
        });
    });
}

function copyForm(id) {
    swal({
        title: 'Sei sicuro?',
        text: "Il form sarà duplicato interamente",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: '/formadmin/formPrimarioCopy',
            type: 'GET',
            data: { idFormPrimario: id },
            dataType: "html",
            success: function (data) {
                if (data == "OK")
                    location.reload();
                else
                    swal(data, '', 'error');
            }
        });
    });
}

function delForm(id, prim_sec) {
    swal({
        title: 'Sei sicuro?',
        text: "Il form sarà marcato come non attivo",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: prim_sec == 1 ? '/formadmin/formPrimarioDelete' : '/formadmin/formSecondarioDelete',
            type: 'GET',
            data: { id: id },
            dataType: "html",
            success: function (data) {
                if (data == "OK")
                    location.reload();
                else
                    swal(data, '', 'error');
            }
        });
    });
}
function delQuestion(id) {
    swal({
        title: 'Sei sicuro?',
        text: "Questa domanda sarà marcata come non attiva",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: '/formadmin/FormQuestionDelete',
            type: 'GET',
            data: { id: id },
            dataType: "html",
            success: function (data) {
                if (data == "OK")
                    location.reload();
                else
                    swal(data, '', 'error');
            }
        });
    });
}
function addResp() {
    event.preventDefault();
    var es = $("#risposte-container input").length;
    $("#risposte-container button").remove();
    $("#risposte-container").append('<input type="text" class="rai form-control" style="width:96%;margin-bottom:6px;display:inline" id="risposta_' + (es + 1) + '" name="risposte"/>');
    $("#risposte-container").append('<button class="btn-action-icon" onclick="addResp()" style="margin-left:4px;"><i class="fa fa-plus"></i></button>');
}

function checkMaxScelte(cbox) {
    if ($(cbox).prop("checked") == false) return;
    if ($('input[name="' + $(cbox).attr("name") + '"]:checked').length > $(cbox).attr("data-maxscelte")) {
        $(cbox).prop("checked", false);
        swal("Questa domanda prevede un massimo di " + $(cbox).attr("data-maxscelte") + " risposte", '', 'error');
    }
}

function checkTipologia() {
    var tipo = $("#id_tipologia").val();
    if (tipo == "2" || tipo == "3" || tipo == "4" || tipo == "7" || tipo == "9") {
        $("#risposte-container").show();
        $("#rm").show();
    }
    else {
        $("#risposte-container").hide();
        $("#rm").hide();
    }
    if (tipo == "7") {
        $("#select-precomp").show();
        $("#select-precomp").removeAttr("disabled");
        $("#div-sceltarisp").hide();
    }
    else {
        $("#select-precomp").hide();
        $("#select-precomp").attr("disabled", "disabled");
        $("#div-sceltarisp").show();
    }
    if (tipo == "9" || tipo == "10") {
        $("#id_domanda_parent").show();
        $("#id_domanda_parent").removeAttr("disabled");
        $("#dom-rif").show();
        $("#div-dom-parent").show();
    }
    else {
        $("#id_domanda_parent").hide();
        $("#dom-rif").hide();
        $("#div-dom-parent").hide();
        $("#id_domanda_parent").attr("disabled", "disabled");
    }
    if (tipo == "4" || tipo == "8") {
        $("#max-scelte").show();
        $("#max_scelte").removeAttr("disabled");
    }
    else {
        $("#max-scelte").hide();
        $("#max_scelte").attr("disabled", "disabled");
    }
    PermettiAltro();
}

function PermettiAltro() {
    var tipo = $("#id_tipologia").val();
    if (tipo == "2" || tipo == "3" || tipo == "4")
        $("#PermettiAltro").removeAttr("disabled");
    else
        $("#PermettiAltro").attr("disabled", "disabled");
}

function GetPrecompilati() {
    var result = { nome: 'Paolo', cognome: 'Rossi', indirizzo: 'Via Bianchi' };
    return result;
}

function Riepilogo() {
    $("#riepilogo").html("");
    $(".testo-domanda").each(function () {
        var domanda = ($(this).text());
        var domid = $(this).attr("data-domanda-id");
        var tipologia = $(this).attr("data-domanda-tipologia");
        var albergo = $(this).attr("data-domanda-albergo");
        var risposta;
        switch (tipologia) {
            case "4": //checkbox
            case "2": //radio list
            case "1": // SI/NO
            case "9": // SLAVE
            case "10": // SLAVE rating
                risposta = "";
                $("input[name='dom-id-" + domid + "']").not(".checkbox-altro").not(".radio-altro").each(function () {
                    if ($(this).prop('checked')) risposta += $(this).attr("data-text") + "<br />";
                });
                $("input[name='dom-id-" + domid + "-altro']").each(function () {
                    if ($(this).val() != "") risposta += $(this).val() + "<br />";
                });

                RiepilogoAppend(domanda, risposta, tipologia);
                break;

            case "3": //tendina list
                risposta = "";
                $("select[name='dom-id-" + domid + "']").each(function () {
                    var t = $.trim($(this).find("option:selected").text());
                    if (t != "") risposta += t + "<br />";
                });

                RiepilogoAppend(domanda, risposta, tipologia);
                break;

            case "5": //textbox short
                risposta = "";
                if (albergo == "false") {
                    $("input[name='dom-id-" + domid + "']").each(function () {
                        if ($(this).val() != "") risposta += $(this).val() + "<br />";
                    });
                }
                else {
                    $("input[name='dom-id-" + domid + "']").not(".checkbox-altro").not(".radio-altro").each(function () {
                        if ($(this).prop('checked')) risposta += $(this).attr("data-text") + "<br />";
                    });
                    $("input[name='dom-id-" + domid + "-altro']").each(function () {
                        if ($(this).val() != "") risposta += $(this).val() + "<br />";
                    });
                }

                RiepilogoAppend(domanda, risposta, tipologia);
                break;
            case "7": //textbox precompilato
                risposta = "";
                $("input[name='dom-id-" + domid + "']").each(function () {
                    if ($(this).val() != "") risposta += $(this).val() + "<br />";
                });

                RiepilogoAppend(domanda, risposta, tipologia);
                break;

            case "6": //textbox long
                risposta = "";
                $("textarea[name='dom-id-" + domid + "']").each(function () {
                    if ($(this).val() != "") risposta += $(this).val() + "<br />";
                });

                RiepilogoAppend(domanda, risposta, tipologia);
                break;

            case "8": //master
                RiepilogoAppend(domanda, "", tipologia);
                break;
        }
    })
}

function RiepilogoAppend(domanda, risposta, tipologia) {
    var sep = '<div class="row"><div class="col-sm-12" style="width: 100%; border-bottom: solid 1px #eee; margin-bottom: 20px; margin-top: 20px"></div> </div>';
    $("#riepilogo").append(
        (tipologia != "9" ? sep : "")
                + "<div class='row'>" +
                       "<div class='col-sm-6' " + (tipologia == "9" ? "style='padding-left:30px'" : "''") + ">" +
                             "<b>" + domanda + "</b>" +
                       "</div>" +
                       "<div class='col-sm-6'>" +
                           risposta +
                       "</div>" +
                  "</div>");
}


//function StilizzaSelectCv(sezione, id_elem) {
//    if (id_elem == null) {
//        id_elem = "";
//    }

//    switch (sezione) {
//        case "exp":
//            $(id_elem + " #_codiceFiguraProf").select2({ containerCssClass: "formElements" });


//            $(id_elem + " #_codSocieta").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
//            $(id_elem + " #_codDirezione").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
//            $(id_elem + " #_codRedazione").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_nazione").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_procura").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_risorseGest").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_budgetGest").select2({ containerCssClass: "formElements" });
//            //$(id_elem + " #_descrizioneEsp").select2({ containerCssClass: "formElements" });

//            /* FREAK - PLUGIN DRAG&DROP */
//            //var jselectDirezione = $("#_codDirezione");
//            var jselectTextable = $(".textable-select");

//            jselectTextable.on('select2:closing', function (e) {
//                var check_item = false;
//                var id = e.target.id;
//                //$("#select2-_codDirezione-results")[0].childNodes[0].textContent
//                var resultSelect = $(".select2-results__options")[0];
//                var countElement = resultSelect.childElementCount;
//                if (countElement == 1) {
//                    if (resultSelect.childNodes[0].textContent == "No results found") {
//                        check_item = true;
//                    }
//                }
//                var text = $(".search-field_custom .select2-search__field").val();
//                var value = $(id_elem +" #" + e.target.id).val();
//                if ((value == "") || (value == null) || (value == 'undefined') || (value == '-1') || (check_item)) {
//                    $(id_elem + " #" + id + " option[value='-1']").remove();
//                    var text = $(".search-field_custom .select2-search__field").val();
//                    $(id_elem + " #" + id).append('<option value="-1" selected="selected">' + text + '</option>');
//                    if (id == "_codSocieta") {
//                        $(id_elem + " #_societa").val(text);
//                    }
//                    else {
//                        $(id_elem + " #_direzione").val(text);
//                    }
//                }
//                else {
//                    if (id == "_codSocieta") {
//                        $(id_elem + " #_societa").val(text);
//                    }
//                    else {
//                        $(id_elem + "#_direzione").val(text);
//                    }
//                }

//            });
//            break;
//        case "stud":
//            $(id_elem + " #_codTipoTitolo").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_codTitolo").select2({ containerCssClass: "formElements" });
//            $(id_elem + " #_codNazione").select2({ containerCssClass: "formElements" });
//            break;
//        case "contatti":
//            $("#frm-InsertAltreInfo #tipoPatente").select2({ containerCssClass: "formElementsSelectMultiple" });
//            break;
//        default:
//            $(id_elem + " .js-select2.formElements").select2({ containerCssClass: "formElements" });
//            break;

//    }
//}

function labelFormatter(label, series) {
    return "<div style='font-size:8pt; text-align:center; padding:2px; color:#555;'>" + label + "<br/>" + Math.round(series.percent) + "%</div>";
}

function BuildCVmenu() {
    var menuobj = [];

    $("h2.TitoloSezioni").each(function () {
        var codsezione = $(this).attr("data-sezione");

        var sezioneobj = { sez: $(this).text(), voci: [] };

        $(".titoli-menu").each(function () {
            if ($(this).attr("data-sez") == codsezione) {
                var d = $(this).find("div:first-child");
                sezioneobj.voci.push({ id: $(d).attr("id"), titolo: $(this).attr("data-titolo") });
            }
        });
        menuobj.push(sezioneobj);
    });

    $.ajax({
        url: '/ajax/getmenucv',
        type: 'GET',
        dataType: "html",
        data: { testo: JSON.stringify(menuobj) },
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            $("#menu-new").html(data);
        },
        error: function (a, b, c) { alert(a + b + c); }
    });
}
function limitaTR(idTabella, rows) {
    var counter = 0;
    $("#" + idTabella).find("tr").each(function () {
        counter++;
        if (counter > rows) $(this).addClass("hi");
    })
}

function fixBarCV(formName) {

    $(".modal-content").scroll(function () {

        var cont = this;

        var isIE = false || !!document.documentMode;
        if (isIE) // If Internet Explorer, return version number
        {
            if ($("#" + formName).find(".modal-header-fixed").length > 0)
                $("#" + formName).find(".modal-header-fixed").css('top', $(cont).scrollTop() + 'px');
        }

        $("#" + formName + " header[data-fixat]:visible").each(function () {

            var myposY = $(this).offset().top - $(document).scrollTop();

            var fixat = parseInt($(this).attr("data-fixat"), 10);

            var sganciaY = parseInt($(this).attr("data-sgancia"), 10);

            var newtop = 0;
            if (parseInt($(cont).scrollTop(), 10) < sganciaY) {
                $(this).removeClass("fix-dettagli");
                $(this).attr("data-sgancia", "0");

                $(".tappo").remove();
                restoreLabel();
                return;
            }
            if (myposY < fixat) {
                if (!$(this).hasClass("fix-dettagli")) $(this).after("<div style='height:" + $(this).innerHeight() + "px' class='tappo'></div>");
                $(this).addClass("fix-dettagli");
                $(this).attr("data-sgancia", $(cont).scrollTop());
                newtop = $(cont).scrollTop() + 58;
                changeLabel();
            }

            if (isIE) {
                $(this).css('top', newtop + "px");
            }

        });
    });
}
function changeLabel() {
    $("h2[data-label-from]:visible").each(function () {

        var t = this;
        if (!$(this).hasClass("move1")) {
            $(this).addClass("move1");

            setTimeout(function () {
                $(t).text($("input[name='" + $(t).attr("data-label-from") + "']:checked").attr("data-label"));
            }, 500);
        }
    });
}
function restoreLabel() {
    $("h2[data-label-from]:visible").each(function () {

        if ($(this).hasClass("move1")) {
            $(this).removeClass("move1");
            $(this).text("Dettagli");
        }
    });
}

function ajaxCallsCompleted() {
    //introJs().setOption("nextLabel", "Avanti").setOption("prevLabel", "Indietro").setOption("skipLabel", "Esci").setOption("doneLabel", "OK").goToStep(1).start();
}
var dateHidden = false;
function popupponeScrollData() {

    $(".scroll-container > .block > .block-content").scroll(function () {

        if ($("#data_da").offset().top - $(document).scrollTop() < 35) {
            setHidden();
        }
        else setShown();


    });

    function setHidden() {
        if (dateHidden == false) {
            dateHidden = true;
            OnDateHidden();
        }
    }
    function setShown() {
        if (dateHidden == true) {
            dateHidden = false;
            OnDateShown();
        }
    }
    function OnDateShown() {
        $("#periodo-scroll").text("");
        $("#periodo-scroll").removeClass("move1");
    }
    function OnDateHidden() {
        var d1 = $("#data_da").val();
        var d2 = $("#data_a").val();
        if (d1 == "" && d2 == "") {
            $("#periodo-scroll").removeClass("move1");
            $("#periodo-scroll").text("");
            return;
        }
        $("#periodo-scroll").addClass("move1");
        setTimeout(function () {
            if (d1 == d2)
                $("#periodo-scroll").text(" per il " + d1);
            else
                $("#periodo-scroll").text(" per il periodo " + d1 + "-" + d2);
        }, 500);

    }
}

function changeMese() {
    var a = $("#anno-sel").val();
    var m = $("#mese-sel").val();
    getMeseTimbratureMA(m, a);
}
function disableControls() {
    $("#anno-sel").attr("disabled", "disabled")
    $("#mese-sel").attr("disabled", "disabled");
}
function enableControls() {
    $("#anno-sel").removeAttr("disabled");
    $("#mese-sel").removeAttr("disabled");
}
function getMeseTimbratureMA(mese, anno) {
    //$("#waitmese").show();
    var tabcart = $(".tabcart").hasClass("active");

    disableControls();
    $("#getcal").load("/timbrature/getcalendario?mese=" + mese + "&anno=" + anno,
        function () {
            $("#waitmese").hide();
            enableControls();
            if (tabcart) {
                $(".tabpres").removeClass("active");
                $(".tabcart").addClass("active");

                $("#schedapres").removeClass("active");
                $("#schedacart").addClass("active");
            }
        }
        );
}

function getMeseTimbrature(mese, anno, next) {
    //$("#waitmese").show();
    var tabcart = $(".tabcart").hasClass("active");
    disableControls();
    $("#getcal").load("/timbrature/getcalendario?mese=" + mese + "&anno=" + anno + "&next=" + next,
        function () {
            $("#waitmese").hide();
            enableControls();
            if (tabcart) {
                $(".tabpres").removeClass("active");
                $(".tabcart").addClass("active");

                $("#schedapres").removeClass("active");
                $("#schedacart").addClass("active");
            }
        }
        );
}

function createCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + value + expires + "; path=/";
}


///////////////////////////////////////
//Boarding
///////////////////////////////////////

function boardingStart() {
    ShowModalBoarding(0);
    //$("#boarding-1").click();
}
function boardingCreateCookie() {
    createCookie("boarding", "1", 365);
}
function boardingNeverDone() {
    return document.cookie.indexOf("boarding=1") < 0;
}

//////////////////////////////////////////////////////////////////////////////////////
//CustomTour - Classe per tour CV                                                     
//////////////////////////////////////////////////////////////////////////////////////

function CustomTour(sel) {
    this.selector = sel;
    this.totalElements = $(sel).find("[data-tourstep]");
    this.current = 0;
    this.frame = 6;
    this.running = false;
    this.forceStart = false;
    this.scrollingContainer = "";

    this.begin = function () {
        var thisClass = this;
        if (this.totalElements.length > 0 && (this.tourNeverDone() || this.forceStart)) {
            this.running = true;
            $(document).scrollTop(0);
            $(this.selector).scrollTop(0);
            $(this.selector).css("overflow", "hidden");
            this.tourOverlayOn();
            this.gotoNextStep();
            this.setCookie();

        }
        else
            console.log("Nessun elemento tour per " + this.selector);
    }

    this.setCookie = function () {
        createCookie(this.selector, "1", 365);
    }

    this.tourNeverDone = function () {
        return document.cookie.indexOf(this.selector + "=1") < 0;
    }
    function createCookie(name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    }
    this.getBullets = function () {
        var html = "<div style='text-align:center;margin-top:20px;margin-bottom:20px'>";

        for (var k = 0; k < this.totalElements.length; k++) {
            var att = $(this.totalElements[k]).attr("data-tourstep");
            if (att <= this.current)
                html += "<a><i class=' text-primary fa fa-circle bulletTour'></i></a>";
            else
                html += "<a><i  class='fa fa-circle bulletTour' style='color:#eee'></i></a>";
        }
        return html + "</div>";
    }

    this.gotoPrevStep = function () {
        var prevElement = this.getPrevElement();

        if (prevElement != null) {
            this.tourStepOff();
            this.tourOnElement(prevElement);
        }
        else
            this.tourExit();
    }

    this.getPrevElement = function () {

        for (var i = Number(this.current) - 1; i > 0; i--) {
            var el = $(this.selector + " [data-tourstep='" + i + "']");
            if (el.length == 0)
                continue;
            else {
                this.current = i;
                return el;
            }
        }
        return null;
    }

    this.gotoNextStep = function () {
        var nextElement = this.getNextElement();

        if (nextElement != null) {
            this.tourStepOff();
            this.tourOnElement(nextElement);
        }
        else
            this.tourExit();
    }

    this.getNextElement = function (fixIndex) {

        for (var i = Number(this.current) + 1; i < 1000; i++) {
            var el = $(this.selector + " [data-tourstep='" + i + "']");
            if (el.length == 0) {

                continue;
            }
            else {
                if (!fixIndex) this.current = i;
                console.log(el);
                return el;
            }
        }
        return null;
    }

    this.tourOverlayOn = function () {
        var overlay = jQuery('<div id="tour-overlay"></div>');
        overlay.appendTo(document.body)
    }

    this.tourOverlayOff = function () {
        $("#tour-overlay").remove();
    }
    this.tourExit = function () {
        this.running = false;
        this.tourOverlayOff();
        this.tourStepOff();
        $(this.selector).css("overflow", "");
    }

    this.tourStepOff = function () {
        $(".tourHighlight").fadeOut(200, function () { $(this).remove(); })
        $(".tourText").fadeOut(200, function () { $(this).remove(); })
    }
    this.isCloseToBottom = function (element) {
        var top = $(element).offset().top;
        var h = $(element).innerHeight() + this.frame * 2;
        h += 280;
        console.log("latezza" + h);
        var wh = $(window).height();
        var cl = (top + h > wh);
        console.log($(element).attr("data-tourdb") + " close to bottom:" + cl);
        return cl;
    }
    this.isCloseToTop = function (element) {
        var top = $(element).offset().top;
        var cl = (top < 150);
        console.log($(element).attr("data-tourdb") + " close to top:" + cl);
        return cl;
    }

    this.scrollUp = function (px, element) {
        var thisClass = this;
        $(this.scrollingContainer).animate({ scrollTop: $(this.scrollingContainer).scrollTop() + px }, 'slow', function () { thisClass.drawElement(element); });
    }
    this.scrollDown = function (px, element) {
        console.log("scrolldown");
        var thisClass = this;
        $(this.scrollingContainer).animate({ scrollTop: $(this.scrollingContainer).scrollTop() - px }, 'slow', function () { thisClass.drawElement(element); });
    }

    this.tourOnElement = function (element) {
        $(document).scrollTop(0);
        console.log("element.top:" + $(element).offset().top);

        if (this.isCloseToBottom(element))
            this.scrollUp(200, element);
        else {
            if (this.isCloseToTop(element))
                this.scrollDown(200, element);
            // else
            this.drawElement(element);
        }
    }
    this.drawElement = function (element) {
        var frame = this.frame;

        var top = $(element).offset().top;
        var left = $(element).offset().left - frame;
        var w = $(element).innerWidth() + frame * 2;
        var h = $(element).innerHeight() + frame * 2;

        var diff = 0;

        $(this.selector).css("overflow", "hidden");
        var div = "<div class='tourHighlight tourHighlight-top' style='top:" + (top - frame - diff) + "px;left:" + (left - frame) + "px;width:" + w + "px;height:" + h + "px'></div>";

        var divText =
        "<div class='tourText tourText-top' style='top:" + (top - frame - diff + h + 10) + "px;left:" + (left - frame) + "px;width:" + w + "px;'>" +
        "<div style='width:100%;text-align:right'><span onclick='t.tourExit()' class='text-primary text-bold pointer'>ESCI</span></div>" +
            "<h5 class='text-primary text-bold'>" + $(element).attr("data-tourtitle") + "</h5>" +
              $(element).attr("data-tourtext") +
                '<div style="text-align:right">' +
                    this.getBullets() +
                    "<div style='width:100%;text-align:center'>" +
                     "<div style='width:200px;border-bottom:solid 1px #eee;margin:auto;margin-bottom:20px'> </div>" +
                    (this.current > 1 ? '<a class="btn btn-default bg-puls_dash btn-scriv" style="margin-right: 10px;" onclick="t.gotoPrevStep()">INDIETRO</a>' : '') +
                    (this.getNextElement(true) != null ? '<a class="btn btn-default bg-puls_dash btn-scriv" style="margin-right: 10px;" onclick="t.gotoNextStep()">AVANTI</a>' : '') +

                    "</div>" +
                '</div>'
        "</div>";

        $(document.body).append(div).append($(divText).hide().fadeIn(200));
        var topTesto = $("div.tourText").offset().top;
        var altTesto = $("div.tourText").outerHeight();
        //console.log("div testo ad y :" + topTesto);
        //console.log("div testo altezza :" + altTesto);
        //console.log("div testo termina a " + (topTesto + altTesto));
        if ((topTesto + altTesto) > $(window).innerHeight()) {
            $(".tourHighlight").remove();
            $(".tourText").remove();
            var over = (topTesto + altTesto) - $(window).innerHeight();
            this.scrollDown(over, element);
        }
        var topElement = $(element).offset().top;
        if (topElement < 150 &&
            $(element).attr("data-tourdb") != "tour-form-corso-titolo"
            &&
            $(element).attr("data-tourdb") != "tour-interesse-area"
            &&
            $(element).attr("data-tourdb") != "tour-interesse-direzione"
            &&
            $(element).attr("data-tourdb") != "tour-datipers-contatto"
            ) {
            $(".tourHighlight").remove();
            $(".tourText").remove();
            this.scrollDown(150 - topElement, element);
        }
    }
}

function cvIndexTourNeverDone() {
    //return true;
    return document.cookie.indexOf("cvindextour=1") < 0;
}
function cvIndexTourCreateCookie() {
    createCookie("cvindextour", "1", 365);
}
//////////////////////////////////////////////////////////////////////////////////////

function CustomTourIndex(sel) {
    this.selector = sel;
    this.totalElements = $(sel).find("[data-tourstep][data-tourdb^='tour-index']");
    this.current = 0;
    this.frame = 8;
    this.running = false;

    this.begin = function () {
        var thisClass = this;
        if (this.totalElements.length > 0 && this.tourNeverDone()) {
            this.running = true;
            $(document).scrollTop(0);
            $(this.selector).scrollTop(0);
            $(this.selector).css("overflow", "hidden");
            this.tourOverlayOn();
            this.gotoNextStep();
            this.setCookie();

        }
        else
            console.log("Nessun elemento tour per " + this.selector);
    }

    this.setCookie = function () {
        createCookie(this.selector, "1", 365);
    }

    this.tourNeverDone = function () {
        return true;
        //return document.cookie.indexOf(this.selector + "=1") < 0;
    }
    function createCookie(name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    }
    this.getBullets = function () {
        var html = "<div style='text-align:center;margin-top:20px;margin-bottom:20px'>";

        for (var k = 0; k < this.totalElements.length; k++) {
            var att = $(this.totalElements[k]).attr("data-tourstep");
            if (att <= this.current)
                html += "<a><i class='fa fa-circle bulletTour'></i></a>";
            else
                html += "<a><i style='color:#ccc' class='fa fa-circle bulletTour'></i></a>";
        }
        return html + "</div><br />";
    }

    this.gotoPrevStep = function () {
        var prevElement = this.getPrevElement();

        if (prevElement != null) {
            this.tourStepOff();
            this.tourOnElement(prevElement);
        }
        else
            this.tourExit();
    }

    this.getPrevElement = function () {

        for (var i = Number(this.current) - 1; i > 0; i--) {
            var el = $(this.selector + " [data-tourstep='" + i + "']");
            if (el.length == 0)
                continue;
            else {
                this.current = i;
                return el;
            }
        }
        return null;
    }

    this.gotoNextStep = function () {
        var nextElement = this.getNextElement();

        if (nextElement != null) {
            this.tourStepOff();
            this.tourOnElement(nextElement);
        }
        else
            this.tourExit();
    }

    this.getNextElement = function (fixIndex) {
        for (var i = Number(this.current) + 1; i < 1000; i++) {
            var el = $(this.selector + " [data-tourdb^='tour-index'][data-tourstep='" + i + "']");
            if (el.length == 0) {

                continue;
            }
            else {
                if (!fixIndex) this.current = i;
                return el;
            }
        }
        return null;
    }

    this.tourOverlayOn = function () {
        var overlay = jQuery('<div id="tour-overlay"></div>');
        overlay.appendTo(document.body)
    }

    this.tourOverlayOff = function () {
        $("#tour-overlay").remove();
    }

    this.tourExit = function () {
        this.running = false;
        this.tourOverlayOff();
        this.tourStepOff();
        $(this.selector).css("overflow", "");
        $(this.selector).find(".tourindex").removeClass("tourindex");
    }

    this.tourStepOff = function () {
        $(".tourHighlight").fadeOut(200, function () { $(this).remove(); })
        $(".tourText,.tourTextLeft,.tourTextRight").fadeOut(200, function () { $(this).remove(); })
    }
    this.isCloseToBottom = function (element) {
        var top = $(element).offset().top;
        var h = $(element).innerHeight() + this.frame * 2;
        h += 200;
        var wh = $(window).height();
        return (top + h > wh)
    }
    this.isCloseToTop = function (element) {
        var top = $(element).offset().top;
        return (top < 150);
    }

    this.scrollUp = function (px, element) {
        var thisClass = this;
        $(this.selector).animate({ scrollTop: $(this.selector).scrollTop() + px }, 'slow', function () { thisClass.drawElement(element); });
    }
    this.scrollDown = function (px, element) {
        var thisClass = this;
        $(this.selector).animate({ scrollTop: $(this.selector).scrollTop() - px }, 'slow', function () { thisClass.drawElement(element); });
    }

    this.tourOnElement = function (element) {
        $(document).scrollTop(0);
        console.log("element.top:" + $(element).offset().top);
        if (this.isCloseToBottom(element))
            this.scrollUp(100, element);
        else {
            if (this.isCloseToTop(element))
                this.scrollDown(100, element);
            else {
                this.drawElement(element);
            }
        }
    }

    this.drawElement = function (element) {
        var frame = this.frame;

        var top = $(element).offset().top;
        var left = $(element).offset().left - frame;
        var w = $(element).innerWidth() + frame * 2;
        var h = $(element).innerHeight();

        $(this.selector).find(".tourindex").removeClass("tourindex");
        $(element).addClass("tourindex");
        var diff = 0;
        $(this.selector).css("overflow", "hidden");
        var div = "<div class='tourHighlight tourHighlight-top' style='position:relative;top: -" + (h + this.frame) + "px;left:-" + this.frame + "px;width:" + w + "px;height:" + (h + (2 * this.frame)) + "px;'></div>";
        var cl = "tourText";
        if ($(element).attr("data-tourposition") == "left") cl = "tourTextLeft";
        if ($(element).attr("data-tourposition") == "right") cl = "tourTextRight";

        var divText =
        "<div id='tour-t' class='" + cl + " tourText-top' style='position:relative;top:-" + (h - 10) + "px;left:-" + this.frame + "px;width:" + w + "px;'>" +
            $(element).attr("data-tourtext") +
                '<div style="text-align:right">' +
                    this.getBullets() +
                    (this.current > 1 ? '<a class="btn btn-default bg-puls_dash btn-scriv" style="margin-right: 10px;" onclick="tindex.gotoPrevStep()">INDIETRO</a>' : '') +
                    (this.getNextElement(true) != null ? '<a class="btn btn-default bg-puls_dash btn-scriv" style="margin-right: 10px;" onclick="tindex.gotoNextStep()">AVANTI</a>' : '') +
                    '<a class="btn btn-default bg-puls_dash btn-scriv" style="margin-right: 10px;" onclick="tindex.tourExit()">ESCI</a>' +
                '</div>'
        "</div>";
        $(element).after(div + divText);//.hide().fadeIn(200);
        $(document).scrollTop($("#tour-t").offset().top - 500)
    }
}

function StartTourEsperienze() {
    $("#modalExperiencesInserimnento .modal-content").scrollTop(0);
    if ($('#flagRai').prop("checked"))
        StartTour("#EsperienzaRai", "#modalExperiencesInserimnento .modal-content", true);

    if ($('#flagExtraRai').prop("checked"))
        StartTour("#EsperienzaExtraRai", "#modalExperiencesInserimnento .modal-content", true);
}

function StartTourCertificazioni() {
    $("#modalCertificazioniInserimento .modal-content").scrollTop(0);

    if ($('#flagAttestato').prop("checked"))
        StartTour("#tour-attestato", "#modalCertificazioniInserimento .modal-content", true);
    if ($('#flagBrevetto').prop("checked"))
        StartTour("#tour-brevetto", "#modalCertificazioniInserimento .modal-content", true);
    if ($('#flagPubblicazione').prop("checked"))
        StartTour("#tour-pubb", "#modalCertificazioniInserimento .modal-content", true);
    if ($('#flagAlbo').prop("checked"))
        StartTour("#tour-albo", "#modalCertificazioniInserimento .modal-content", true);
}

function StartTourFormazione() {
    $("#modalFormazioneInserimento .modal-content").scrollTop(0);
    StartTour("#frmInserimentoFormazione", "#modalFormazioneInserimento .modal-content", true);
}

function StartTourAreeInteresse() {
    $("#modalAreeInteresseInserimnento .modal-content").scrollTop(0);
    StartTour("#form-insertareainteresse", "#modalAreeInteresseInserimnento .modal-content", true);
}

function AggiornaTempoRimanente() {
    setInterval(function () { getTempoRimanente(); }, 3000);
}

function getTempoRimanente() {
    $(".temporimanente").html('<i class="fa fa-spinner fa-spin"></i>');

    $.ajax({
        url: '/events/getPostiDisponibiliAll',
        type: "GET",
        data: {},
        dataType: "json",
        complete: function () { },
        success: function (data) {
           /* setTimeout(function () {
                for (var i = 0; i < data.result.length; i++) {
                    var idevento = data.result[i].idevento;
                    $("#posti-disp-" + idevento).text(data.result[i].posti_disp);
                    $("#posti-disppre-" + idevento).text(data.result[i].posti_disp);
                    if (data.result[i].posti_disp <= 0)
                        $("#prenota-evento-" + idevento).hide();
                    else
                        $("#prenota-evento-" + idevento).show();
                }
            }, 1000);*/
        }
    });
}

function ModificaPrenotazione(id, idEvento) {
    $("#datanascita").datetimepicker({
        viewMode: 'years',
        format: 'DD/MM/YYYY',
        maxDate: 0
    });

    $('#datanascita').on('dp.hide', function (event) {
        setTimeout(function () {
            $('#datanascita').data('DateTimePicker').viewMode('years');
        }, 1);
    });
    $('#datanascita').data("DateTimePicker").maxDate(new Date());

    $.ajax({
        url: "/events/GetPrenotazioneByID?idPrenotazione=" + id,
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {
            $(".pren-data").removeAttr("disabled");
            $("#nome").val(data.nome);
            $("#cognome").val(data.cognome);
            $("#datanascita").val(data.datanascita);
            $("#citta").val(data.citta);
            $("#genere").val(data.genere);
            $("#email").val(data.email);
            $("#telefono").val(data.telefono);
            $("#tipoDocumento").val(data.tipo_documento);
            $("#numeroDocumento").val(data.documento);
            $("#grado").val(data.grado);
            $("#specificaInsediamento").val(data.specificaInsediamento);
            $('#nota').val(data.note);
            $("#idPrenotazione").val(id);
            $("#button-mod-anag").show();
            $("#button-ann-anag").show();
            $("#button-pren-anag").hide();
            ModificaEvento = true;
        }
    });
}

function VediPrenotazione(id, idEvento) {

    try {
        $("#datanascita").datetimepicker({
            viewMode: 'years',
            format: 'DD/MM/YYYY',
            maxDate: 0
        });

        $('#datanascita').on('dp.hide', function (event) {
            setTimeout(function () {
                $('#dataNascita').data('DateTimePicker').viewMode('years');
            }, 1);
        });
        $('#datanascita').data("DateTimePicker").maxDate(0);
    }
    catch(ex)
    {

    }


    $.ajax({
        url: "/events/GetPrenotazioneByID?idPrenotazione=" + id,
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {
            $("#info_nome").val(data.nome);
            $("#info_cognome").val(data.cognome);
            $("#info_dataNascita").val(data.datanascita);
            $("#info_citta").val(data.citta);
            $("#info_genere").val(data.genere);
            $("#info_email").val(data.email);
            $("#info_telefono").val(data.telefono);
            $("#info_tipoDocumento").val(data.tipo_documento);
            $("#info_numeroDocumento").val(data.documento);
            $("#info_grado").val(data.grado);
            $("#info_specificaInsediamento").val(data.specificaInsediamento);
            $("#idPrenotazione").val(id);
            $('#info_nota').val(data.note);

        }
    });

}
function AnnullaModificaPrenotazione() {
    $("#aggiornatimer").click();
}
function EsciEvento() {
    var idevento = $("#idevento").val();
    $.ajax({
        url: '/events/rimuoviOpzioniPostiEvento',
        type: "POST",
        data: { idevento: idevento },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            if (data.result == "OK") {

            }
            else {
                swal("Errore", data.result, "error");
            }
        }
    });
    if (ModificaEvento == true) {
        swal("OK", "Modifiche Eventi Effettuate", "info").then(function () {
            $.ajax({
                url: '/events/GestisciNotifiche',
                type: 'POST',
                data: { idEvento: idevento },
                dataType: "json",
                complete: function () { },
                success: function (data) {
                }
            });

            if (window.location.href.toLowerCase().indexOf("backurl") >= 0) {
                var url = window.location.href.split('?')[1];
                var params = url.split('&');
                for (var i = 0; i < params.length; i++) {
                    var p = params[i].split('=');
                    if (p[0].toLowerCase() == "backurl") {
                        location.href = decodeURIComponent(p[1]);
                    }
                }
            }
            else
                location.reload();
        });
    } else {
        if (window.location.href.toLowerCase().indexOf("backurl") >= 0) {
            var url = window.location.href.split('?')[1];
            var params = url.split('&');
            for (var i = 0; i < params.length; i++) {
                var p = params[i].split('=');
                if (p[0].toLowerCase() == "backurl") {
                    location.href = decodeURIComponent( p[1]);
                }
            }
        }
        else
            location.reload();

    }

}


function UpdateTit(codTitolo, tableTarget, progressivo) {
    RaiOpenAsyncModal("modal-tit-studio", "/cv_online/modificaStudi",
        {
            codTitolo: codTitolo,
            tableTarget: tableTarget,
            progressivo: progressivo
        });

    //$.ajax({
    //    url: '/cv_online/modificaStudi',
    //    type: "GET",
    //    cache: false,
    //    dataType: "html",
    //    data: {
    //        codTitolo: codTitolo,
    //        tableTarget: tableTarget,
    //        progressivo: progressivo
    //    },
    //    success: function (data) {
    //        $("#tit-studio-content").html(data)
    //        $("#tit-st").text("Modifica un Titolo di Studio");
    //        $("#tit-studio").modal("show");
    //    }
    //});
}

function ShowTit() {
    RaiOpenAsyncModal("modal-tit-studio", "/cv_ajax/getTitoli", {});

    //$.ajax({
    //    url: '/cv_ajax/getTitoli',
    //    type: "GET",
    //    dataType: "html",
    //    data: {},
    //    success: function (data) {
    //        $("#tit-studio-content").html(data)
    //        $("#tit-st").text("Aggiungi un Titolo di Studio");
    //        $("#tit-studio").modal("show");
    //    }
    //});
}

function ShowStudy(t) {
    switch (t) {
        case 'dip':
            $("#panel-diploma").removeClass("hide");
            $("#panel-laurea").addClass("hide");
            $("#panel-master").addClass("hide");
            break;

        case 'lau':
            $("#panel-diploma").addClass("hide");
            $("#panel-laurea").removeClass("hide");
            $("#panel-master").addClass("hide");
            break;

        case 'spe':
            $("#panel-diploma").addClass("hide");
            $("#panel-laurea").addClass("hide");
            $("#panel-master").removeClass("hide");
            break;
    }
    StartTitoliTour(false);
}

function SubmitStudies(button) {
    event.preventDefault();
    $(button).addClass('disable');
    $('#savedbmodstudies').show();

    var type = "";
    var url = "";
    if ($("#flag-dip").prop("checked") == true) {
        type = "dip";
        url = "/CV_Online/saveDiploma";
    }
    if ($("#flag-lau").prop("checked") == true) {
        type = "lau";
        url = "/CV_Online/saveLaurea";
        if ($(".lode-lau").prop("checked") == true) $(".lode-lau").val("S");
    }
    if ($("#flag-mas").prop("checked") == true) {
        type = "mas";
        url = "/CV_Online/saveMaster";
        if ($(".lode-mas").prop("checked") == true) $(".lode-mas").val("S");
    }
    var form = $("#form-inserimentocv-" + type);

    var validator = $(form).validate();

    if (!$(form).valid()) {
        validator.focusInvalid();
        $(button).removeClass('disable');
        $('#savedbmodstudies').hide();
        return false;
    }

    var obj = $(form).serialize();
    $.ajax({
        url: url,
        type: "POST",
        data: obj,
        cache: false,
        success: function (data) {
            $("#savedbmodstudies").fadeOut("slow", function () {
            });
            if (data != "ok") {
                swal(data, '', 'error');
                $(button).removeClass('disable');
                $('#savedbmodstudies').hide();
            }
            else {
                $("#flagChangedInput").val("");
                $("#tit-studio").modal("hide");
                //container_CV_Online_studies_refresh();
                CallActionAjax('studies', 'box-studies');
            }
        }
    });
}

//utilizzate nei titoli di studio nuova versione - Inizializzano Select2 e DateTimePicker
function InitSelect2(container) {
    jQuery(container + '.js-select2').select2();
    var jselectTextable = $(container + ".js-select2");

    jselectTextable.on('select2:closing', function (e) {

        var name = $(e.target).next("span").find(".select2-selection").attr("aria-owns");

        var text = $("#" + name).closest("span").prev("span").find(".select2-search__field").val();

        if ($.trim(text) == "") return;

        var ClassPerTestoLibero = $(e.target).attr("data-hidden-testolibero");
        if (ClassPerTestoLibero != undefined) {
            $("." + ClassPerTestoLibero).val(text);
            $(e.target).find("option[value='-1']").remove();
            $(e.target).append('<option value="-1" selected="selected">' + text + '</option>');
        }

    });
}

function InitDatePicker() {
    jQuery('.js-datetimepicker').each(function () {

        var $input = $(this);

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
                time: 'fa fa-clock-o',
                date: 'si si-calendar',
                up: 'fa fa-arrow-up',
                down: 'fa fa-arrow-down',
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

function keyAltro() {
    var text = $("#_altraConInfo").val();

    if ($.trim(text) != "") {
        var n = $("#table_extra").find("input:radio").first().attr("name");
        var escaped = n.replace(/(:|\.|\[|\])/g, '\\$1');
        if ($("input[name=" + escaped + "]:checked").length == 0)
            $("#table_extra").find("input:radio").first().prop("checked", true)
    }
    $("#_altraConInfo").closest("td").prev("td").find("input:checkbox").prop("checked", true);
}
function AggiungiCampoDin() {
    var ex = false;
    $("#contenitore-dinamico .campodin").each(function () {
        if ($(this).val().trim() == "") {
            swal("Errore", "Non puoi aggiungere campi dinamici se non hai riempito i precedenti.", "error");
            ex = true;
        }
    });
    if (ex) return;

    $("#contenitore-dinamico .valoredin").each(function () {
        if ($(this).val().trim() == "") {
            swal("Errore", "Non puoi aggiungere campi dinamici se non hai riempito i precedenti.", "error");
            ex = true;
        }
    });
    if (ex) return;

    CampoDin();
}
function CampoDin() {
    $.ajax({
        url: '/schedaEccezioni/getCampoDin',
        type: "GET",
        dataType: "html",
        data: {   },
        success: function (data) {
            $("#contenitore-dinamico").append(data);
            AttivaSummernote();
        }
    });
    //var row = '<div class ="col-sm-12" style="margin-top:30px">   <p><b>Nome campo</b>:</p>' +
    //         '<input type="text" name="campodinamico" class="form-control campodin" style="width:100%" />' +
    //        '</div>' +
    //        '<div class ="col-sm-12" style="margin-top:10px">   <p>Valore campo:</p>' +
    //            '<textarea   name="valoredinamico" class="form-control valoredin" style="width:100%" rows="5"/>' +
    //    '</div>' +
    //    '<div class="col-sm-12" style="margin-top:10px">'+
    //        '<p>Posizione campo:</p>'+

    //        '<select class="form-control valid" id="posizionedinamico_0" name="posizionedinamico" aria-invalid="false">'+
    //            '<option>Scegli posizione...</option>'+
    //            '<option value="0">Prima del campo DEFINIZIONE</option>'+
    //            '<option value="1">Prima del campo CRITERI DI INSERIMENTO</option>'+
    //            '<option value="2">Prima del campo TRATTAMENTO ECONOMICO</option>'+
    //            '<option value="3">Prima del campo DOCUMENTAZIONE</option>'+
    //            '<option value="4">Prima del campo PRESUPPOSTI E PROCEDURE</option>'+
    //            '<option value="5">Prima del campo NOTE</option>'+
    //            '<option value="6">Prima del campo ALLEGATI</option>'+
    //            '<option value="7">Prima del campo FONTI DELLA DISCIPLINA</option>'+
    //            '<option value="8">Prima del campo ULTERIORI INFORMAZIONI</option>'+
    //        '</select>'+
    //    '</div>';
    //$("#contenitore-dinamico").append(row);
}
function AggiungiFonteDin() {
    var ex = false;
    $(".fontedin").each(function () {
        if ($(this).val().trim() == "") {
            swal("Errore", "Non puoi aggiungere fonti se non hai specificato le precedenti.", "error");
            ex = true;
        }
    });
    if (ex) return;

    $("#buttonAggiungiFonte").remove();
    var html = '<div class="col-sm-12">' +
         '<p>Fonte:</p>' +
    '</div>' +

     '<div class="col-sm-12">' +
            '<input type="text" name="fonti" class="form-control fontedin" style="width:100%">' +
     '</div>' +
    '<div class="col-sm-12">' +
       '<p>Url:</p>' +
     '</div>' +

      '<div class="col-sm-10">' +
            '<input type="text" name="urlfonti" class="form-control urldin" style="width:100%">' +
     '</div>' +
     '<div class="col-sm-2" style="text-align:right">' +
         '<input id="buttonAggiungiFonte" class="btn btn-primary" onclick="AggiungiFonteDin()" style="margin:0px" type="button" value="+ Fonte">' +
     '</div>';

    $("#fontidinamiche").append(html);

}
function EccezioneScelta() {
    var cod = $("#CodiceEccezione").val();
    $.ajax({
        type: 'GET',
        url: "/schedaeccezioni/getdettaglieccezione",
        dataType: "json",
        data: { codice: cod },
        cache: false,
        success: function (data) {
            $("#DescrittivaEccezione").val(data.desc);
            if (data.unit == "H") $("#TipoAssenza").val("1");
            else
                if (data.unit == "G") $("#TipoAssenza").val("2");
                else
                    $("#TipoAssenza").val("")
        },
        error: function (aa, b, c) {
            swal(aa + b + c, '', 'error');
        },
        complete: function () {

        }
    });

}
function UploadAllegati(indice) {
    swal({
        title: 'Il file è stato caricato correttamente',
        //text: "Vuoi dare un nome al documento caricato?",
        html: '<p>Assegna un nome al documento caricato</p><input type="text" name="tmp_name" id="tmp_name" class="form-control formElements" />',
        //type: 'warning',
        //showCancelButton: true,
        //confirmButtonColor: '#3085d6',
        //cancelButtonColor: '#d33',
        confirmButtonText: 'Salva',
        //cancelButtonText: 'No, cancel!',
        confirmButtonClass: 'btn btn-primary btn-lg',
        //cancelButtonClass: 'btn btn-danger',
        buttonsStyling: false
    }).then(function () {
        var item = $("#tmp_name").val();
        if (item.trim() == "") {
            swal("Errore", "Devi indicare una descrizione per il file per una successiva individuazione.", "error")
            .then(function () {
                UploadAllegati(indice);
                return;
            });
        }
        else {
            $("#hidden-" + indice).val(item);
            $("#filename-" + indice).text(item);
            $(".icon-plus-" + indice).hide();
            $(".icon-doc-" + indice).show();
            var quanti = $("#allegati-container .to-send").length;
            $.ajax({
                url: '/schedaEccezioni/getPartialAllegato',
                type: "GET",
                dataType: "html",
                data: { indice: quanti },
                success: function (data) {
                    $("#alleg-box").append(data);
                }
            });
        }

    })
}
function sendAllegato(idecc) {
    var quanti = $("#allegati-container .to-send").length - 1;
    if (quanti == 0) location.reload();

    var formData = new FormData();
    var name = "_fileUpload";
    formData.append('idecc', idecc);
    for (var i = 0; i < quanti; i++) {
        var file = $("#fileUpload-" + i)[0].files[0];
        formData.append('nomefile', $("#filename-" + i).text());
        formData.append('_fileUpload', file);
    }


    $.ajax({
        url: "/schedaEccezioni/AddAllegati",
        type: "POST",
        dataType: "html",
        contentType: false,
        processData: false,
        data: formData,
        cache: false,
        success: function (data) {
            if (data == "OK")
                location.reload();
            else
                swal("Errore", data, "error")
        },
        error: function () {

        }
    });
}
function CancellaAllegatoEcc(idDoc) {
    swal({
        title: 'Confermi ?',
        text: "L'allegato verrà cancellato.",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
        $.ajax({
            url: "/schedaEccezioni/DelAllegati",
            type: "POST",
            dataType: "html",
            data: { id: idDoc },
            cache: false,
            success: function (data) {
                if (data == "OK")
                    {
                        $("#divall-" + idDoc).remove();
                        $("#tr-allegato-"+idDoc).remove();
                        if ($("#tabella-allegati tr").length <=2)
                        {
                            $("#tabella-allegati").remove();
                        }
                    }
                else
                    swal("Errore", data, "error")
            },
            error: function () { }
        });
    });
}
function filterEcc() {
    // Declare variables 
    var input, filter, table, tr, td, i;
    input = document.getElementById("filter");
    filter = input.value.toUpperCase();
    table = document.getElementById("table-ecc");
    tr = table.getElementsByTagName("tr");

    var cl = [];
    if ($("#cb1").prop("checked"))
        cl.push("tipo-1");
    if ($("#cb2").prop("checked"))
        cl.push("tipo-2");
    if ($("#cb3").prop("checked"))
        cl.push("tipo-3");

    for (i = 0; i < tr.length; i++) {
        var cla = tr[i].className;
        if (cl.indexOf(cla) >= 0) {
            tr[i].style.display = "";
            if (input == "") continue;
            td = tr[i].getElementsByTagName("td")[0];
            if (td) {
                if (td.innerText.toUpperCase().indexOf(filter) > -1) {
                    tr[i].style.display = "";
                } else {
                    tr[i].style.display = "none";
                }
            }
        }
        else {
            tr[i].style.display = "none";
            continue;
        }

    }

}
function FiltraEcc(tipo) {
    filterEcc();
    //var p = ($("#cb" + tipo).prop("checked"));
    //if (p)
    //    $("#table-ecc tr.tipo-" + tipo).show();
    //else
    //    $("#table-ecc tr.tipo-" + tipo).hide();


}



function RegioneScelta() {
    var idreg = $("#idRegione").val();
    $("#ricerca-alb").addClass("disable");
    $("#aumenta-raggio").addClass("disable");

    if (idreg == "") {
        $("#prov").html('<option value="">Provincia...</option>');
        $("#comuni").html('<option value="">Comune...</option>');
    }
    else {
        $.ajax({
            url: "/Hotel/getprovince",
            type: "GET",
            dataType: "json",
            data: { idreg: idreg },
            cache: false,
            success: function (data) {
                $("#comuni").html('<option value="">Comune...</option>');
                $("#prov").html('<option value="">Provincia...</option>');
                for (var i = 0; i < data.result.length; i++) {
                    $("#prov").append('<option value="' + data.result[i].Value + '">' + data.result[i].Text + '</option>');
                }
            },
            error: function () { }
        });
    }
}
function ProvinciaScelta() {
    $("#ricerca-alb").addClass("disable");
    $("#aumenta-raggio").addClass("disable");
    var idprov = $("#prov").val();
    if (idprov == "") {
        $("#comuni").html('<option value="">Comune...</option>');
        $("#ricerca-alb").addClass("disable");
    }
    else {
        $.ajax({
            url: "/Hotel/getcomuni",
            type: "GET",
            dataType: "json",
            data: { idprov: idprov },
            cache: false,
            success: function (data) {
                $("#comuni").html('<option value="">Comune...</option>');
                for (var i = 0; i < data.result.length; i++) {
                    $("#comuni").append('<option value="' + data.result[i].Value + '">' + data.result[i].Text + '</option>');
                }
            },
            error: function () { }
        });
    }
}
function ComuneScelto() {
    var idcom = $("#comuni").val();
    if (idcom == "") {
        $("#ricerca-alb").addClass("disable");
        $("#aumenta-raggio").addClass("disable");
    }
    else {
        $("#ricerca-alb").removeClass("disable");
        $("#aumenta-raggio").removeClass("disable");
    }
}
function getAlberghi(km) {
    if (km == 0) $("#currDistance").val('0');
    var idcomune = $("#comuni").val();
    var currDistance = $("#currDistance").val();

    if (km == 0) {
        $.ajax({
            url: "/Hotel/getHotels",
            type: "GET",
            dataType: "html",
            data: { idcomune: idcomune },
            cache: false,
            success: function (data) {
                $("#alberghi-result").html(data);
            },
            error: function () { }
        });
    }
    else {
        $("#waithotel").css("display", "inline-block")
        $.ajax({
            url: "/Hotel/getHotelsEntro",
            type: "GET",
            dataType: "html",
            data: { idcomune: idcomune, km: km },
            cache: false,
            success: function (data) {
                $("#waithotel").css("display", "none");
                $("#alberghi-result").html(data);
                if (km >= 90) {
                    $("#aumenta-raggio").addClass("disable");
                }
            },
            error: function () { }
        });
    }

}
function getRadioListAlberghi(idDomanda, km) {
    var idcomune = $("#comuni").val();
    var currDistance = $("#currDistance").val();

    $.ajax({
        url: "/Hotel/getRadioListHotels",
        type: "GET",
        dataType: "html",
        data: { idDomanda: idDomanda, idcomune: idcomune, km: km },
        cache: false,
        success: function (data) {
            $("#alberghi-result").html(data);
        },
        error: function () { }
    });

}
function aumentaRaggio() {
    var currDistance = Number($("#currDistance").val());

    if (currDistance >= 90) return;

    currDistance += 30;
    $("#currDistance").val(currDistance);

    if (currDistance >= 90) {
        $("#aumenta-raggio").addClass("disable");
    }

    getAlberghi(currDistance);

}

function CopiaProfilo() {
    $("#datimenu input:checkbox").each(function () {
        $(this).prop("checked", false);
    });
    var voci = $("#copiaprofilo").val().split(',');
    for (var i = 0; i < voci.length; i++) {
        $("#datimenu input:checkbox").each(function () {
            if ($(this).val() == voci[i]) {
                $(this).prop("checked", true);
            }
        });
    }
}



function togg(divpanel) {
    $($(divpanel).find('.internal')[0]).collapse('toggle')
    $(divpanel).find("i").toggleClass("flip");
}



function getMeseAgenda(mese, anno, next) {
    $('#agenda-mese').hide();
    $("#waitmese").show();
    //var tabcart = $(".tabcart").hasClass("active");
    //disableControls();
    //$("#getcal").load("/raiacademy/GetAgenda?mese=" + mese + "&anno=" + anno,
    //    function () {
    //        $("#waitmese").hide();
    //        enableControls();
    //        if (tabcart) {
    //            $(".tabpres").removeClass("active");
    //            $(".tabcart").addClass("active");

    //            $("#schedapres").removeClass("active");
    //            $("#schedacart").addClass("active");
    //        }
    //    }
    //    );

    $.ajax({
        type: 'GET',
        url: "/raiacademy/GetAgenda?mese=" + mese + "&anno=" + anno,
        dataType: "html",
        data: {},
        cache: false,
        success: function (data) {
            $("#waitmese").hide();

            $("#agendaCorsi").replaceWith(data);


        }
    });
}

$(document.body).on("change", '.hiddenSlider', function (event) {

    var prog = $(this).attr("data-hidden-prog");
    var dalle = $(this).attr("data-hidden-dalle");
    var alle = $(this).attr("data-hidden-alle");
    var maxx = $(this).attr("data-hidden-max");
    var childof = $(this).attr("data-hidden-childof");

    $('#td-minuti-' + prog).text(this.value + " min");

    var fieldArray = dalle.split(":");

    var minutes = (Number(fieldArray[0]) * 60) + Number(fieldArray[1]);

    minutes += Number(this.value);
    var h = Math.floor(minutes / 60);
    var m = minutes % 60;
    h = h < 10 ? '0' + h : h;
    m = m < 10 ? '0' + m : m;
    $("#td-end-" + prog).text(h + ':' + m);
    $("#ecc-intervallo-" + prog).attr("data-minuti", this.value.toString());
    $("#ecc-intervallo-" + prog).attr("data-intervallo", dalle + "/" + h + ":" + m);



    if (Number(this.value) < Number(maxx)) {
        if (childof) $("#slider-" + childof).addClass("disable");
        $(this).closest("tr").addClass("tr-micro bg-cel");
        $(this).closest("tr").prev("tr").addClass("tr-micro bg-cel");
        AddSlider(prog, h + ":" + m, alle);
    }
    else {
        if (childof) $("#slider-" + childof).removeClass("disable");
        if ($(this).hasClass("father")) {
            $(this).closest("tr").removeClass("tr-micro bg-cel");
            $(this).closest("tr").prev("tr").removeClass("tr-micro bg-cel");
        }
        DelSlider(prog);
    }
    DisableSelectOverCarenza();

});
function DisableSelectOverCarenza() {
    var minutiCoperti = GetMinutiCoperti();
    $(".ecc-cop").each(function () {
        if ($(this).val() == "") {

            var minutes = Number($(this).attr("data-minuti"));
            var carenzaTotale = Number($("#carenza-giornata-totale").val());
            if (minutes > carenzaTotale - minutiCoperti)
                $(this).attr("disabled", "disabled");
            else
                $(this).removeAttr("disabled");
        }
    });
}
function AddSlider(prog, dalle, alle) {



    var fi = dalle.split(":");
    var minutesDalle = (Number(fi[0]) * 60) + Number(fi[1]);

    var fi2 = alle.split(":");
    var minutesAlle = (Number(fi2[0]) * 60) + Number(fi2[1]);

    var minutes = minutesAlle - minutesDalle;

    var progmax = $("#table-ecc-auto tr").length + 1;
    if ($(".child-" + prog).length == 0) {

        var periodoMensa = "";
        if ($("#tr-data-" + prog).hasClass("rowmensa")) {
            periodoMensa = '<span class="span-mensa">PERIODO MENSA</span>';
        }


        $("#tr-slider-" + prog).after("<tr  id='tr-data-" + progmax + "' class='tr-micro bg-cel child-" + prog + "'>" +
            "<td id='td-start-" + progmax + "' style='position:relative' class='infor top-dash padleft'>" +
            periodoMensa
            + "<div>" + dalle + "</div></td>" +
            "<td id='td-end-" + progmax + "'  class='infor top-dash'>" + alle + "</td>" +
            "<td id='td-minuti-" + progmax + "'  class='infor top-dash' style='font-weight:bold'>" + minutes + " min</td>" +
            "<td class='top-dash'>" +
            "<select id='ecc-intervallo-" + progmax + "' class='form-control ecc-cop'" +
            " data-progressivo='" + progmax + "' " +
            " data-minuti='" + minutes + "'" +
            " data-intervallo='" + dalle + "/" + alle + "'></select>"
            + "</td></tr>");
        var $options = $("#ecc-intervallo-" + prog + " > option").clone();
        $('#ecc-intervallo-' + progmax).append($options);
        $('#ecc-intervallo-' + progmax).attr("onchange", $("#ecc-intervallo-" + prog).attr("onchange"));
        $("#tr-data-" + progmax).after("<tr class='tr-micro bg-cel child-slider-" + prog + "' data-counter='" + progmax + "' id='tr-slider-" + progmax + "'>" +
            "<td class='padleft' colspan='3' style='height:20px;border-top:none'>" +

            GetSliderHtml(progmax, dalle, alle, minutes, prog) +

            "</td><td style='border-top:none'></td></tr>");

        $('#slider-' + progmax).each(function () {
            var $this = $(this),
                opts = {};

            var pluginOptions = $this.data('plugin-options');
            if (pluginOptions) {
                opts = pluginOptions;
            }

            $this.themePluginSlider(opts);
        });

    }
    else {
        $("tr.child-" + prog).find("td:first-child div").text(dalle);
        $("tr.child-" + prog).find("td:nth-child(2)").text(alle);
        $("tr.child-" + prog).find("td:nth-child(3)").text(minutes + " min");
        $("tr.child-" + prog + " select").attr("data-minuti", minutes);
        $("tr.child-" + prog + " select").attr("data-intervallo", dalle + "/" + alle);

        var trSlider = $(".child-of-" + prog).closest("tr");
        var currentCounter = $(trSlider).attr("data-counter");

        var sliderUpdated = GetSliderHtml(currentCounter, dalle, alle, minutes, prog);
        console.log(sliderUpdated);
        $(".child-of-" + prog).replaceWith(sliderUpdated);

        $('.child-of-' + prog).each(function () {
            var $this = $(this),
                opts = {};

            var pluginOptions = $this.data('plugin-options');
            if (pluginOptions) {
                opts = pluginOptions;
            }

            $this.themePluginSlider(opts);
        });

    }

}
function GetSliderHtml(progmax, dalle, alle, minutes, progParent) {
    var sl = "<div style='margin-top:-18px' id='slider-" + progmax + "'" +
           ' class="child-of-' + progParent + ' mt-3 mb-3 slider-info" data-plugin-slider' +
           " data-plugin-options='{ \"value\": " + minutes + ", \"range\": \"min\",\"min\":0, \"max\": " + minutes + "}' " +
           "data-plugin-slider-output='#listenSlider-" + progmax + "'>" +
           ' <input class="hiddenSlider" ' +
           "data-hidden-prog='" + progmax + "'" +
           "data-hidden-dalle='" + dalle + "'" +
           "data-hidden-alle='" + alle + "'" +
           "data-hidden-max='" + minutes + "'" +
           " data-hidden-childof='" + progParent + "'" +
           "id='listenSlider-" + progmax + "' type='hidden' value='" + minutes + "' />" +
           " </div>";
    return sl;
}
function DelSlider(prog) {
    $("tr.child-" + prog).remove();
    $("tr.child-slider-" + prog).remove();

}

function ShowPdfEvento(idEvento, matricola) {
    $.ajax({
        beforeSend: function () { $("#page-loaderR").show(); },
        complete: function () { $("#page-loaderR").hide(); },
        url: '/Events/getpdf',
        type: "GET",
        dataType: "html",
        data: { idEvento: idEvento, matricola: matricola },
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
                $("#modal-pdf").modal("show");
                $("#modal-pdf-internal").html(data);
            }
        },
        error: function (a, b, c) {
            swal('Errore', a + b + c, 'error');
        }
    });
}

function ComprendeTimbratureInOut(dalle, alle) {

    if (dalle.trim() == "" || alle.trim() == "") return false;
    if ($("#modal-popin .timbratura-row").length == 0) return false;

    var data = $("#data_da").val();
    var errore = false;
    $("#modal-popin .timbratura-row").each(function () {

        var tdin = $(this).find(".timbratura-in");
        var ingresso = "";
        if ($(tdin).length > 0) {
            ingresso = $(tdin)[0].innerText.trim();
        }
        var tdout = $(this).find(".timbratura-out");
        var uscita = "";
        if ($(tdout).length > 0) {
            uscita = $(tdout)[0].innerText.trim();
        }
        if (ingresso == "Si" || uscita == "Si") {
            return;
        }
        var dalleMoment = moment(data + " " + dalle, "DD/MM/YYYY HH:mm");
        var alleMoment = moment(data + " " + alle, "DD/MM/YYYY HH:mm");
        var ingressoMoment = moment(data + " " + ingresso, "DD/MM/YYYY HH:mm");
        var uscitaMoment = moment(data + " " + uscita, "DD/MM/YYYY HH:mm");
        if (dalleMoment.isSameOrBefore(ingressoMoment) && alleMoment.isSameOrAfter(uscitaMoment)) {
            errore = true;
        }

    });

    return errore;


}

function OrarioEccezioneAmmessoInCopertura(oraminuti) {


    if (oraminuti.length != 5) return false;

    var data = $("#data_da").val();
    var oraminutiMoment = moment(data + " " + oraminuti, "DD/MM/YYYY HH:mm");
    if (!oraminutiMoment.isValid()) return false;

    var ammesso = true;
    $("#modal-popin .timbratura-row").each(function () {

        var tdin = $(this).find(".timbratura-in");
        var ingresso = "";
        if ($(tdin).length > 0) {
            ingresso = $(tdin)[0].innerText.trim();
        }
        var tdout = $(this).find(".timbratura-out");
        var uscita = "";
        if ($(tdout).length > 0) {
            uscita = $(tdout)[0].innerText.trim();
        }

        if (ingresso != "Si" && uscita != "Si") {
            var ingressoMoment = moment(data + " " + ingresso, "DD/MM/YYYY HH:mm");
            var uscitaMoment = moment(data + " " + uscita, "DD/MM/YYYY HH:mm");
            if (ingressoMoment.isValid() && uscitaMoment.isValid()) {
                if (uscita == "") {
                    if (oraminutiMoment.isAfter(ingressoMoment))
                        ammesso = false;
                }
                if (oraminutiMoment.isAfter(ingressoMoment) && oraminutiMoment.isBefore(uscitaMoment)) {
                    ammesso = false;
                }
            }

        }
    });
    return ammesso;
}

function OrarioEccezioneAmmessoInServizio(oraminuti) {

    if (oraminuti.length != 5) return false;

    var data = $("#data_da").val();
    var oraminutiMoment = moment(data + " " + oraminuti, "DD/MM/YYYY HH:mm");
    if (!oraminutiMoment.isValid()) return false;

    var ammesso = false;
    $("#modal-popin .timbratura-row").each(function () {

        var tdin = $(this).find(".timbratura-in");
        var ingresso = "";
        if ($(tdin).length > 0) {
            ingresso = $(tdin)[0].innerText.trim();
        }
        var tdout = $(this).find(".timbratura-out");
        var uscita = "";
        if ($(tdout).length > 0) {
            uscita = $(tdout)[0].innerText.trim();
        }

        if (ingresso != "Si" && uscita != "Si") {
            var ingressoMoment = moment(data + " " + ingresso, "DD/MM/YYYY HH:mm");
            var uscitaMoment = moment(data + " " + uscita, "DD/MM/YYYY HH:mm");
            if (ingressoMoment.isValid() && uscitaMoment.isValid()) {
                if (
                    oraminutiMoment.isSameOrAfter(ingressoMoment)
                    &&
                    oraminutiMoment.isSameOrBefore(uscitaMoment)
                    ) {
                    ammesso = true;
                }
            }
        }
        else
            ammesso = true;


    });
    return ammesso;
}
function SuperaPrevistaPresenza(dalle, alle) {

    var data = $("#data_da").val();
    var dalleMoment = moment(data + " " + dalle, "DD/MM/YYYY HH:mm");
    var alleMoment = moment(data + " " + alle, "DD/MM/YYYY HH:mm");

    if (!dalleMoment.isValid() || !alleMoment.isValid()) return true;


    var minPrevistaPresenza = $("#giornata-info-attr").attr("data-prevista-presenza");
    if (minPrevistaPresenza == "" || minPrevistaPresenza == "0")
        return true;

    return (alleMoment.diff(dalleMoment, "minutes") > minPrevistaPresenza);

}

//per DB:
function ValidaFuoriServizio() {

    if (!$("#param-dalle").hasClass("hide")) {
        var d = $("#param-dalle").find("input").val();
        if (d.trim() != "" && !OrarioEccezioneAmmessoInCopertura(d)) {
            ValidazioneErrore("Orario iniziale non ammesso per l'eccezione scelta");
            return;
        }
    }
    if (!$("#param-alle").hasClass("hide")) {
        var a = $("#param-alle").find("input").val();
        if (a.trim() != "" && !OrarioEccezioneAmmessoInCopertura(a)) {
            ValidazioneErrore("Orario finale non ammesso per l'eccezione scelta");
            return;
        }
    }
    if (!$("#param-quantita").hasClass("hide")) {
        var q = $("#param-quantita").find("input").val();
        if (q.trim() != "" && SuperaPrevistaPresenza(d, a)) {
            ValidazioneErrore("Il periodo dell'eccezione è troppo ampio");
            return;
        }
    }

    if (!$("#param-dalle").hasClass("hide") && !$("#param-alle").hasClass("hide")) {
        var dalle = $("#param-dalle").find("input").val();
        var alle = $("#param-alle").find("input").val();
        if (ComprendeTimbratureInOut(dalle, alle)) {
            ValidazioneErrore("Il periodo richiesto nell'eccezione non è ammesso");
            return;
        }
    }
    ValidazioneOK();
    // console.log("ok");
}

//per DB:
function ValidaInServizio() {
    if (!$("#param-dalle").hasClass("hide")) {
        var d = $("#param-dalle").find("input").val();
        if (d.trim() != "" && !OrarioEccezioneAmmessoInServizio(d)) {
            ValidazioneErrore("Orario iniziale non ammesso per l'eccezione scelta");
            return;
        }
    }
    if (!$("#param-alle").hasClass("hide")) {
        var a = $("#param-alle").find("input").val();
        if (a.trim() != "" && !OrarioEccezioneAmmessoInServizio(a)) {
            ValidazioneErrore("Orario finale non ammesso per l'eccezione scelta");
            return;
        }
    }
    if (!$("#param-quantita").hasClass("hide")) {
        var q = $("#param-quantita").find("input").val();
        if (q.trim() != "" && SuperaPrevistaPresenza(d, a)) {
            ValidazioneErrore("Il periodo dell'eccezione è troppo ampio");
            return;
        }
    }

    ValidazioneOK();
    // console.log("ok");
}

function ApplicaFiltriApprovatore( sedeGapp, nominativo )
{
    $('#nominativo').val(nominativo);
    $('#sede').val(sedeGapp);
    $('#btnFilter').removeAttr('disabled');
    $('html,body').scrollTop(0);
    $('#btnFilter').click();
}

function reset_ric()
{
    $("#nominativo").val("");
    $("#titolo").val("");
    $("#stato").val("");
    $("#eccezione").val("");
    $("#datada").val("");
    $("#dataal").val("");
}

/////////////////////////////////////////

function GetMinutiLavorati() {
    var minLavorati = [];
    $("#modal-popin .timbratura-row").each(function () {

        var tdin = $(this).find(".timbratura-in");
        var ingresso = "";
        if ($(tdin).length > 0) {
            ingresso = $(tdin)[0].innerText.trim();
        }
        var tdout = $(this).find(".timbratura-out");
        var uscita = "";
        if ($(tdout).length > 0) {
            uscita = $(tdout)[0].innerText.trim();
        }
        if (ingresso == "Si" || uscita == "Si") {
            return true;
        }
        for (var i = toMinutes(ingresso) ; i <= toMinutes(uscita) ; i++) {
            minLavorati.push(i);
        }
    });
    return minLavorati;
}
function IntervalloInServizio(dalle, alle) {
    if (dalle.trim() == "" || alle.trim() == "") return false;
    if ($("#modal-popin .timbratura-row").length == 0) return false;

    var minLavorati = GetMinutiLavorati();

    var startInterval = toMinutes(dalle);
    var endInterval = toMinutes(alle);
    var interval = [];
    for (var i = startInterval; i <= endInterval; i++) {
        interval.push(i);
    }
    var result = intersection(interval, minLavorati);

    return (result.length == interval.length);
}
function IntervalloFuoriServizio(dalle, alle) {
    if (dalle.trim() == "" || alle.trim() == "") return false;
    if ($("#modal-popin .timbratura-row").length == 0) return false;

    var minLavorati = GetMinutiLavorati();

    var startInterval = toMinutes(dalle);
    var endInterval = toMinutes(alle);
    var interval = [];
    for (var i = startInterval + 1 ; i <= endInterval - 1 ; i++) {
        interval.push(i);
    }
    var result = intersection(interval, minLavorati);

    return (result.length == 0);
}
function intersection(arr1, arr2) {
    var result = [];
    for (var i = 0; i < arr1.length; i++) {
        if (arr2.indexOf(arr1[i]) >= 0) {
            result.push(arr1[i]);
        }
    }
    return result;
}
function toMinutes(ora) {
    var fieldArray = ora.split(":");
    var minutes = (Number(fieldArray[0]) * 60) + Number(fieldArray[1]);
    return minutes;
}
function getname(matr, id) {
    $("#" + id).attr("data-original-title", "Attendi...");
    $("#" + id).tooltip("show");

    $.ajax({
        url: '/ajax/getnome?matricola=' + matr,
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            $("#" + id).attr("data-original-title", data);
            $("#" + id).tooltip("show");
        }
    });
}

function GestMatrixSlaveClick(radio, idchecker) {
    var currentId = $(radio).attr('id');
    var isChecked = $(radio).attr('is-checked');
    var alreadyChecked = $('#matrix-' + idchecker).val();
    if (isChecked=="true") {
        alreadyChecked = alreadyChecked.replace(currentId + ',', '');
        $(radio)[0].checked = false;
        $(radio).attr("is-checked", 'false');
    }
    else {
        if (alreadyChecked === undefined || alreadyChecked == '')
            alreadyChecked = currentId+"," ;
        else if (alreadyChecked.indexOf(currentId) < 0) {
            alreadyChecked += currentId+",";
        };
        $(radio).attr("is-checked", 'true');
    }
    $('#matrix-' + idchecker).val(alreadyChecked);
}
function CheckRadioLimit(value, element) {
    var minLim = $(element).attr("data-min-select");
    var maxLim = $(element).attr("data-max-select");
    var elems = value.split(',');
    if ((minLim > 0 && elems.length-1 < minLim)|| (maxLim > 0 && elems.length-1 > maxLim))
        return false;
    else
        return true;
}

function uplPDF() {

    $.ajax({
        url: $("#formpdf-up").attr('action'),
        type: 'POST',
        data: new FormData($("#formpdf-up")[0]),// $("#formpdf-up").serialize(),
        processData: false,
        contentType: false,
        success: function (data) {
           
            if (data == "OK" || data == "") {
                swal('OK', 'PDF caricato', 'info');
                $("#container-pdf").load("/tech/getdoc");
                $("#formpdf-up").get(0).reset();
            }
            else {
                swal('Errore', data, 'error');
                $("#swal2-content").css("overflow", "auto");
            }
        }
    });
}
function createPDF() {
    $("#wwwaaa").show();
    $.ajax({
        url: $("#formpdf").attr('action'),
        type: 'POST',
        data: $("#formpdf").serialize(),
        success: function (data) {
            $("#wwwaaa").hide();
            if (data == "OK" || data == "") {
                swal('OK', 'PDF creato', 'info');
                $("#container-pdf").load("/tech/getdoc");
            }
            else {
                swal('Errore', data, 'error');
                $("#swal2-content").css("overflow", "auto");
            }
        }
    });
}
function GetSede(sede) {
    $.ajax({
        url: "/sedi/getsede",
        type: 'POST',
        data: { sede: sede },
        success: function (data) {
            if (data.result == "ok") {
                swal('OK', 'Sede aggiunta', 'info').then(function () { location.reload(); });
            }
            else
                swal('Errore', data.result, 'error');
        }
    });
}


var SediNonConvalidate = "";

function checkConvalide() {
    var data = $("#datacheck").val();
    if (data == "") {
        swal('Errore', "Inserisci una data", 'error');
        return;
    }

    SediNonConvalidate = "";
    var sedi = $("#table-sedi .nome-sede");
    checkConvalida(sedi, 0);
    //.each(function () {
    //    var sede = ($(this).text());

    //});
}

function checkConvalida(sedi, index) {
    var data = $("#datacheck").val();
    if (index > sedi.length - 1) {
        $("#btn-check-conv").removeAttr("disabled");
        $("#btn-check-conv").text("CHECK CONVALIDE");
        if (SediNonConvalidate != "") SediNonConvalidate = " - Non conv:<br/>" + SediNonConvalidate;
        swal('OK', 'Controllo terminato per ' + data + SediNonConvalidate, 'info');
        $('[data-toggle="tooltip"]').tooltip();
        return;
    }

    var nomesede = $(sedi[index]).text();
    if (nomesede.length > 5) {
        index++;
        checkConvalida(sedi, index);
        return;
    }
    $("#btn-check-conv").attr("disabled", "disabled");
    $("#btn-check-conv").text("CHECKING " + nomesede + "...");

    var dataconv = data;
    $.ajax({
        url: "/sedi/checkconv",
        type: 'POST',
        data: { datacheck: data, sede: nomesede },
        success: function (data) {
            var h = $(sedi[index]).html();

            if (data.result == true) {
                $(sedi[index]).html(h + '&nbsp;<i data-toggle="tooltip" title="' + dataconv + ' Convalidato" class="fa fa-circle circle-green" aria-hidden="true"></i>');
            }
            else if (data.result == false) {
                $(sedi[index]).html(h + '&nbsp;<i data-toggle="tooltip" title="' + dataconv + ' NON convalidato" class="fa fa-circle circle-red" aria-hidden="true"></i>');
                SediNonConvalidate += "<br/>" + h;
            }
            else {
                $(sedi[index]).html(h + '&nbsp;<i data-toggle="tooltip" title="' + dataconv + ' Errore risposta CICS" class="fa fa-circle circle-orange" aria-hidden="true"></i>');
                SediNonConvalidate += "<br/>" + h;
            }

            $("#div-sedi").scrollTop(index * $(sedi[index]).outerHeight());


            index++;
            checkConvalida(sedi, index);


        }
    });



}
function editSedeGapp(event) {
    $("#modal-sede").modal("show");
    $("#content-sede").html("");
    event.preventDefault();
    event.stopPropagation();
    $("#content-sede").load("/sedi/getsededb?id=" + $(event.target).attr("data-id-sedegapp"));
}

function UpdateSede() {
    var form = $("#form-sede-gapp");
    if (!form.valid()) return;
    
    $.ajax({
        url: $(form).attr('action'),
        type: 'POST',
        data: $(form).serialize(),
        success: function (data) {
            if (data.result.trim() == "ok") {
                    swal("OK", "Sede aggiornata", "info").then(function () {
                        $("#modal-sede").modal("hide");
                        location.reload();
                });
            }
            else {
                swal("Errore", data.result, "error")
            }
        }
    });
}

function NewsBanner()
{
    $.ajax({
        url: '/scrivania/getnews',
        type: "GET",
        dataType: "html",
        data: {},
        success: function (data) {
            if (data != "NONEWS")
            {
                $("#modal-news").html("<div class='modal-backdrop fade in'></div>" + data);
                $('#modal-news').modal('show');
            }
        }
    });
}

function EndNews()
{
    $('#modal-news').modal('hide');
}
function ShowNextNews()
{
    var curnews = Number($("#current-news").val());
    curnews++;
    UpdateNewsPanel(curnews);
}
function ShowPreviousNews()
{
    var curnews = Number($("#current-news").val());
    curnews--;
    UpdateNewsPanel(curnews);
}
function UpdateNewsPanel(curnews)
{
    $("#current-news").val(curnews);
    $(".newsbox").hide();
    $("#news-item-" + curnews).show();
    $(".bulletn").removeClass("boarding-item-sel").addClass("boarding-item");
    $("#bullet-news-" + curnews).addClass("boarding-item-sel");

    var totnews = Number($("#total-news").val());
    if (curnews+1 < totnews)
        $("#arrow-right-news").show();
    else
        $("#arrow-right-news").hide();

    if (curnews+1>1)
        $("#arrow-left-news").show();
    else
        $("#arrow-left-news").hide();

    if (curnews+1==totnews)
        $("#end-news").show()
    else
        $("#end-news").hide()
}

function AccettaProposte(skipIpotesiNoCarenza) {
    var proposteAuto = [];
    var DifferenzeIpotesiNoCarenza = [];
    var quad = $("#giornata-info-attr").attr("data-quadratura");
    var car = $("#giornata-info-attr").attr("data-carenza");
    var tuttoOk = true;

    $('#table-ecc-auto input[type="checkbox"]:checked').each(function () {
        if (!tuttoOk) return false;

        $("#button-conferma-auto").attr("disabled", "disabled");

        var ind = 0;
        //se sono eccezioni proposte da prontuario, si riconosceranno per index=-1
        if ($(this).val().indexOf("ECPR") >= 0)
            ind = -1;
        else
            ind = $(this).val();

        var MatricolaApprovatoreProduzione = null;
        var idAttivita = null;

        if ($(this).attr("data-att-ceiton-obbl") == "True")
        {
            idAttivita = $('#att-ceiton-' + $(this).attr('data-cod') + ' select[id="idattivita"]').val();
            MatricolaApprovatoreProduzione = $('#att-ceiton-' + $(this).attr('data-cod') + ' select[id="select-richiestaeccezione-idApprovatore-' + $(this).attr('data-cod') + '"]').val();

            var idAttivitaVisibile = true;

            // verifica se il campo attività è visibile ed ha più di un elemento
            // il primo elemento è il campo di default con la dicitura: "Seleziona attività..."
            if ($('#att-ceiton-' + $(this).attr('data-cod') + ' select[id="idattivita"]').is(':visible') &&
                $('#att-ceiton-' + $(this).attr('data-cod') + ' select[id="idattivita"]').length > 1) {
                idAttivitaVisibile = true;
            }
            else {
                idAttivitaVisibile = false;
            }

            if (idAttivitaVisibile && (idAttivita == "undefined" || idAttivita == null || idAttivita == "")) {
                tuttoOk = false;
                return false;
            }

            var approvatoreVisibile = true;

            // verifica se il campo approvatore è visibile
            if ($('#att-ceiton-' + $(this).attr('data-cod') + ' select[id="select-richiestaeccezione-idApprovatore-' + $(this).attr('data-cod') + '"]').is(':visible'))
            {
                approvatoreVisibile = true;
        }
            else
            {
                approvatoreVisibile = false;
            }

            if (approvatoreVisibile && (MatricolaApprovatoreProduzione == "undefined" || MatricolaApprovatoreProduzione == null || MatricolaApprovatoreProduzione == "")) {
                tuttoOk = false;
                return false;
            }
        }

        var quantita_no_carenza = $(this).attr("data-quantita-ipotesinocarenza");
        var quantita = $(this).attr("data-quan");

        if (skipIpotesiNoCarenza != true) {
            if (car != "0" && quad == "Settimanale" && quantita != null && quantita_no_carenza != null && quantita.trim() != "1" && quantita.trim() != quantita_no_carenza.trim()) {
                if ($(this).attr("data-cod") != "FMH")
                DifferenzeIpotesiNoCarenza.push({ codice: $(this).attr("data-cod"), quantita: quantita, quantita_no_car: quantita_no_carenza });
            }
        }


        proposteAuto.push({
            d: $(this).attr("data-date"),
            cod: $(this).attr("data-cod"),
            index: ind,
            nota: $("#txa-" + $(this).val()).val(),
            dalle: $(this).attr("data-dalle"),
            alle: $(this).attr("data-alle"),
            quantita: $(this).attr("data-quan"),
            idAttivitaCeiton: idAttivita,
            MatricolaApprovatoreProduzione: MatricolaApprovatoreProduzione
        });
    });

    if (!tuttoOk) {
        swal("Impossibile continuare", "Alcuni dati risultano incompleti.\nSelezionare una attività ed un approvatore di produzione per poter continuare", "error");
        return false;
    }

    if (DifferenzeIpotesiNoCarenza.length > 0) {
        swal({
            title: 'Attenzione',
            html: "<span>Il sistema ha rilevato una ipotesi di maggiorazione più alta se la tua carenza per la giornata venisse sanata prima di questa richiesta. <br /> Eccezione: <b>" +
                DifferenzeIpotesiNoCarenza[0].codice + "</b><br />Quantita proposta: <b>" + DifferenzeIpotesiNoCarenza[0].quantita + "</b><br />" +
                "Quantita con carenza zero: <b>" + DifferenzeIpotesiNoCarenza[0].quantita_no_car +
                "</b><br /></span>" +
                "<br/><span><b>Proseguendo saranno inseriti i valori proposti attualmente.</b></span>"
            ,

            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            cancelButtonText: 'Esci',
            confirmButtonText: 'Prosegui'
        }).then(function (isConfirm) {
            SendProposteAuto(proposteAuto);
        }, function (dismiss) {
            ScegliDaWizard();
            ClickedTab(1);
            AbilitaPagina(2, 3, 'Altre segnalazioni');
                })
    }
    else {
        SendProposteAuto(proposteAuto);
    }
  
}

function SendProposteAuto(proposteAuto) {
    $.ajax({
        type: 'POST',
        url: "/ajax/saveProposteAuto",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(proposteAuto),
        cache: false,
        success: function (data) {
            $("#button-conferma-auto").removeAttr("disabled");
            if (data.result == "OK") {
                swal({
                    title: 'Richiesta inserita',
                    type: 'success',
                    html: ' ',
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText:
                        ' OK'
                });
                RefreshSegnalazioni(proposteAuto[0].d);
                ForceRefreshMieRichiesteJS();

                for (var i = 0; i < proposteAuto.length; i++) {
                    PropostaAutoDisattiva(proposteAuto[i].index);
                }
                ForceRefreshAll();
                refreshDataAsync();
            }
            else
                swal('Errore', data.result, 'error');
        },
        error: function (a, b, c) {
            $("#button-conferma-auto").removeAttr("disabled");
            alert(a + b + c);
        }
    });
}


$("#pianoferie-app-div").on('shown.bs.modal', function () {
});

function ShowPfApprovatore(sede,anno) {
    $.ajax({
        url: '/pianoferie/getpfapp',
        type: "GET",
        dataType: "html",
        data: {sede:sede,anno:anno},
        success: function (data) {
            $("#pianoferie-app-div").html(data);
            $("#pianoferie-app-div").modal("show");
        }
    });
}

function CompletaResoconti() {

    var sedi = [];

    $("#tabresoconti tbody.js-table-sections-header").each(function () {
        sedi.push($(this).attr("id"));
    });


    $.ajax({
        url: '/resoconti/getresoconti',
        type: "GET",
        dataType: "html",
        data: { onlypreview: false, sedi: sedi.join() },
        success: function (data) {
            
            $(data).find("div[id^='div-tot-']").each(function () {
                //debugger
                var id = $(this).attr("id");
                $("#" + id).replaceWith($(this));
            });
            $(data).find("tr[id^='tr-tot-']").each(function () {
                //debugger
                var id = $(this).attr("id");
                $("#" + id).replaceWith($(this));
            });
        }
    });
}
function SalvaOrdineAllegati()
{
var seq="";
    $("#allegati-container input.ordine-allegati").each(function(){ 
         seq += ($(this).attr("data-id-allegato")) +":"+ $(this).val()  +",";
    })

 
    $.ajax({
        type: 'POST',
        url: "/ajax/salvaordine",
       dataType:"html",
        data: {seq:seq},
        cache: false,
        success: function (data) {
            
            if (data == "OK") {
              swal({
                    title: 'Ordine salvato con successo',
                    type: 'success',
                    html: ' ',
                    showCloseButton: true,
                    showCancelButton: false,
                    confirmButtonText:
                      ' OK'
                });
            }
            else
                swal('Errore', data, 'error');
        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });

}
function Assistente() {
    $.ajax({
        type: 'GET',
        url: "/ajax/getGiornateProposteAssistente",
        dataType: "json",
        data: {   },
        cache: false,
        success: function (data) {

           debugger
            if (data.result == false) {
                $("#wait-h3").text("");
                $("#wait-pa").text("");
                $("#wait-ecce").hide();
                $("#wait-pa-spin").hide();
                return;
            }
            if (data.model.length == 0) {
                $("#wait-h3").text("");
                $("#wait-pa").text("");
                $("#wait-ecce").hide();
                $("#wait-pa-spin").hide();
                return;
            }
            debugger
            if (data.model.length == 1) {
                $("#range-prop").text("(" + data.model[0] + ")")
                $("#last-data").val(data.model[0]);
            }
            else {
                $("#range-prop").text("(" + data.model[0] + " - " + data.model[data.model.length - 1] + ")");
                $("#last-data").val(data.model[data.model.length - 1]);
            }

            AssistenteDay(data.model, 0);
        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });
}
function AssistenteDay(daylist, index) {

    if (index >= daylist.length) {
        $("#wait-h3").text("");
        $("#wait-pa").text("");
        $("#wait-ecce").hide();
        $("#wait-pa-spin").hide();

        if ($("#row-cont tr.prop-auto-head").length > 0) {
            $("#assistente-conf").removeClass("disable");
            $("#assistente-ann").removeClass("disable");
        }

        return;
    }
        

    var d = daylist[index];
    $("#wait-h3").text("Ricerca in corso per " + d + " ...");
    $("#wait-pa").text("Ricerca in corso per " + d + " ...");

    $.ajax({
        type: 'GET',
        url: "/ajax/getProposteAssistente",
        dataType: "json",
        data: { date: d },
        cache: false,
        success: function (dataresp) {
           
            debugger
            if (dataresp != null && dataresp.model != null && dataresp.model.EccezioniProposte != null) {
                var tot = dataresp.model.EccezioniProposte.length;
                if (tot > 0) {
                    var htmlString = "";

                    $("#vis-prop").removeClass("disable");
                    var totProp = parseInt($("#tot-prop").text());
                    totProp += tot;
                    $("#tot-prop").text(totProp);

                    var inte = '<tr class="prop-auto-head">' +
                        '<td style="border-bottom:2px solid #ddd">' +
                        '<label class="css-input css-checkbox css-checkbox-rounded css-checkbox-sm css-checkbox-info">' +
                        '<input type="checkbox" checked="checked" id="cb-all-assis" onchange="checkPropAssis()"  name="CBprop" value="" /> <span></span></label>' +
                        '</td> ' +
                        '<td style="font-weight:normal;border-bottom:2px solid #ddd"> DATA</td>' +
                        '<td style="font-weight:normal;border-bottom:2px solid #ddd"> SEGNALAZIONE</td>' +
                        '<td style="font-weight:normal;border-bottom:2px solid #ddd;text-align:center" class="hidden-xs">QUANTITA</td>' +
                        '<td style="font-weight:normal;border-bottom:2px solid #ddd;text-align:center" class="hidden-xs">STATO</td>' +
                        '</tr>';

                    if ($("#row-cont tr.prop-auto-head").length > 0)
                        inte = "";

                    htmlString += inte;



                    for (var i = 0; i < tot; i++) {
                        htmlString += ('<tr><td style="width:5%;border-bottom: solid 1px #ddd;"><label class="css-input css-checkbox css-checkbox-rounded css-checkbox-sm css-checkbox-info">' +
                            '<input type="checkbox" data-eccezione="' + dataresp.model.EccezioniProposte[i].cod + '" data-date="' + dataresp.model.EccezioniProposte[i].data + '" data-qta="' + dataresp.model.EccezioniProposte[i].qta + '" checked="checked" class="cb-assis"  name="CBprop" value="" /><span></span></label> </td>' +
                            '<td  style="width:15%;text-align:left;border-bottom: solid 1px #ddd;">  <span class=""><b>' +
                            dataresp.model.EccezioniProposte[i].data.substring(0, 2) + "/" +
                            dataresp.model.EccezioniProposte[i].data.substring(2, 4) + "/" +
                            dataresp.model.EccezioniProposte[i].data.substring(4, 8)
                            + '</b></span></td>' +
                            '<td  style="width:50%;border-bottom: solid 1px #ddd;"><b>' + dataresp.model.EccezioniProposte[i].cod + '</b> ' + '<br/>' + dataresp.model.EccezioniProposte[i].descrittiva_lunga + '<br/></td>' +
                            '<td  style="width:20%;text-align:center;border-bottom: solid 1px #ddd;">  <span class="label label-info">' + dataresp.model.EccezioniProposte[i].qta + '</span></td>' +
                            '<td  style="line-height:1em;width:10%;text-align:center;border-bottom: solid 1px #ddd;">  <span class="stato" style="font-size:70%;color:#ccc">NON INSERITA</span></td>' +
                            '</tr> ');


                    }
                    $("#assis-table").append(htmlString);
                }
            }
           


            debugger
            index++;
            AssistenteDay(daylist, index);
        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });
}
function checkPropAssis() {
    var p = $("#cb-all-assis").prop("checked");
    $(".cb-assis").prop("checked", p);
}

function checkAssisChecked() {
    var q = $("input.cb-assis:checked").length;
    if (q == 0) {
        swal("Nessuna eccezione selezionata per inserimento");
    }

    $("#assistente-conf").addClass("disable");
    $("#assistente-ann").addClass("disable");

    var inCorso = false;
    $('#assis-table input.cb-assis:checkbox').each(function () {
        if ($(this).is(':checked') && inCorso==false) {
            SendProposta($(this));
            inCorso = true;
        }
    });
}
function SendProposta(cb) {
    var proposteAssistente = [];
    proposteAssistente.push({
        d: $(cb).attr("data-date"),
        cod: $(cb).attr("data-eccezione"),
        dalle: "00:00",
        alle:"00:00",
        quantita: $(cb).attr("data-qta")
    });
    debugger
    $(cb).closest("tr").find("span.stato").html(' <i  class="fa fa-refresh fa-spin" style="font-size: 12px; color: #aaa;"></i> IN CORSO...')
    
    $.ajax({
        type: 'POST',
        url: "/ajax/saveProposteAuto",
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(proposteAssistente),
        cache: false,
        success: function (data) {
           
            if (data.result == "OK") {
                $(cb).closest("tr").find("span.stato").html('<i class="fa fa-check" style="font-size: 18px;color: #18e21f;"></i>');
                DecrementaTotaleProp();
            }
            else {
                var spanStato = $(cb).closest("tr").find("span.stato");
                spanStato.text("ERRORE");
                spanStato.css("color", "red");
                spanStato.css("cursor", "pointer");
                spanStato.attr("data-toggle", "tooltip");
                spanStato.attr("title", data.result);
                spanStato.tooltip();
            }
            FindNextCB(cb);
        },
        error: function (a, b, c) {
            $("#button-conferma-auto").removeAttr("disabled");
            alert(a + b + c);
        }
    });
}

function FindNextCB(cb) {

    var w = true;
    while (w) {
        var tr = $(cb).closest("tr");
        tr = $(tr).next("tr");
        debugger
        if (tr.length == 0 || tr == null || tr == undefined) {
            debugger
            w = false;
            $('#assistente-close').show();
            return;
        }

        cb = tr.find("input.cb-assis");
        
        if (cb.prop("checked") == true) {
            w = false;
            SendProposta(cb);
        }
    }
    
    
}

function ConfermaAnnullaAssist() {
    swal({
        title: 'Confermi ?',
        text: "Le giornate visualizzate non saranno più elaborate dalla funzione Assistente. Le eccezioni andranno inserite aprendo le rispettive giornate.",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Conferma',
        cancelButtonText: 'Annulla'
    }).then(function () {
          $('#assistente-prop').modal('hide')
        });
}
function AbilitaAssistente() {
   

    $.ajax({
        type: 'POST',
        url: "/ajax/AbilitaAssistente",
        dataType: "json",
        data: {},
        cache: false,
        success: function (data) {

            if (data.result == "OK") {

                $("#assistente-icon").css("opacity", "1");
                $("#assistente-block").show();
                $("#assis-tit").text("Proposte automatiche");
                $("#ab-ass").hide();
                $("#vis-prop").show();

                Assistente();
            }
            else
                swal(data.result);

        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });
}
function VisualizzaAssistente() {


    $.ajax({
        type: 'POST',
        url: "/ajax/AggiornaDataAssistente",
        dataType: "json",
        data: {  data:$("#last-data").val()  },
        cache: false,
        success: function (data) {

            if (data.result == "OK") {

                $('#assistente-prop').modal('show');

                $('#assistente-close').hide();
                $('#assistente-conf').removeClass("disable");
                $('#assistente-ann').removeClass("disable");

            }
            else
                swal(data.result);

        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });


    
}

function DecrementaTotaleProp() {
    var totProp = parseInt($("#tot-prop").text());
    totProp--;
    if (totProp <= 0) 
        $("#vis-prop").addClass("disable");
    else
        $("#vis-prop").removeClass("disable");

    $("#tot-prop").text(totProp);
}

function wrCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}



function ShowPianificazione(id, fromApprovazione) {
    debugger
    $("#popup-view-pianificazione").modal("show");
    if (fromApprovazione == null) fromApprovazione = false;

    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetPianificazioneContent",
        dataType: "html",
        data: { idRichiesta: id, isAppr: fromApprovazione },
        cache: false,
        success: function (data) {
            $("#calendario-pianificazione").html(data);
            $("#control-string").val(getControlString());

        }
    });
}
function ShowCalendario(id) {
    debugger
    $("#popup-view-cal").modal("show");
   

    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetCalendarioContent",
        dataType: "html",
        data: { idRichiesta: id },
        cache: false,
        success: function (data) {
            $("#calendario-pianificazione").html(data);

        }
    });
}
function AjaxFillPopupVisualizza(idRichiesta, isFromAppr) {

    $("#popupview-visualizza").modal("show");
    $("#giorni-pian").html("");
    $("#stati-container").html("");
    $("#dettagli-container").html("");

    $("#stati-container").addClass("rai-loader");
    $("#dettagli-container").addClass("rai-loader");
    $("#giorni-pian").addClass("rai-loader");

    if (!isFromAppr) {
        $.ajax({
            type: 'GET',
            url: "/maternitacongedi/GetStatiVisualizzazione",
            dataType: "html",
            data: { idRichiesta: idRichiesta },
            cache: false,
            success: function (data) {
                $("#stati-container").html(data);
                $("#stati-container").removeClass("rai-loader");

            }
        });
    }
    else {
        $("#stati-section").remove();
    }
  
    
    GetDettagliVisualizzazione(idRichiesta, isFromAppr);

    if (!isFromAppr) {
        GetAnnullamentoBox(idRichiesta);
        GetGiorniBox(idRichiesta);
        $("#indietro-section").hide();
    }
    else {
        $("#annullamento").removeClass("rai-loader");
        $("#giorni-pian").removeClass("rai-loader");
        $("#indietro-section").show();
    }
    if (isFromAppr) {
        GetStatoRichiestaBox(idRichiesta);
    }
     


}
function GetStatoRichiestaBox(idRichiesta) {
    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/getStatoRichiestaBox",
        dataType: "html",
        data: { idRichiesta: idRichiesta },
        cache: false,
        success: function (data) {
            
            $("#statorichiesta-container").html(data);
        }
    });
}
function GetAnnullamentoBox(idRichiesta) {
    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetAnnullamentoBox",
        dataType: "html",
        data: { idRichiesta: idRichiesta },
        cache: false,
        success: function (data) {
            $("#annullamento").removeClass("rai-loader");
            $("#annullamento").html(data);
        }
    });
}
function GetGiorniBox(idRichiesta) {
    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetGiorniBox",
        dataType: "html",
        data: { idRichiesta: idRichiesta },
        cache: false,
        success: function (data) {
            $("#giorni-pian").removeClass("rai-loader");
            $("#giorni-pian").html(data);
            $('.pianificazione-congedi').easyPieChart({
                "barColor": "#008A09", "delay": 300, scaleColor: false, lineWidth: 8, size: 100, trackColor: '#ddd',
            });
        }
    });
}
function GetDettagliVisualizzazione(idRichiesta, isFromAppr) {
    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetDettagliVisualizzazione",
        dataType: "html",
        data: { idRichiesta: idRichiesta },
        cache: false,
        success: function (data) {
            $("#dettagli-container").html(data);
            $("#dettagli-container").removeClass("rai-loader");
            
        }
    });
}
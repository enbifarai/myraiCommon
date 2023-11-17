﻿////////INDEX

function GetDifferenzeDescrittive() {
    window.location = "/maternitacongedi/GetDifferenzeDescrittive";
}
//function ShowCalendarioTracciati(id) {
//    $("#popup-view-calendario-tracciati").modal("show");
//    $("#calendario-tracciati").html("");
//    $.ajax({
//        type: 'GET',
//        url: "/maternitacongedi/GetCalendarioContent",
//        dataType: "html",
//        data: { idRichiesta: id },
//        cache: false,
//        success: function (data) {
//            $("#calendario-tracciati").html(data);

//        }
//    });
//}
function SospendiPratica(id, sospendi) {

    var az = "sospesa";
    var tit = "Sospendi pratica";
    var dom = "Confermi di sospendere la pratica visualizzata?";

    if (sospendi == 0) {
        az = "riavviata";
        tit = "Riavvia pratica";
        dom = "Confermi di riavviare la pratica visualizzata?";
    }

    swal({
        title: tit,
        type: 'question',
        html: dom,
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: '/MaternitaCongedi/SospendiPratica',
            type: "GET",
            data: { idrich: id, sospesa: sospendi },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.esito == true) {
                    swal({
                        title: 'Pratica ' + az + ' correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    }).then(function () {
                        $("#popupview-amm").modal("hide");
                        GetContent();

                    });

                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
            }
        });

    });



}


function PeriodoValido(d1, d2) {
    return moment(d1, "DD/MM/YYYY", true) <= moment(d2, "DD/MM/YYYY", true);
}
function DataValida(data) {
    return moment(data, 'DD/MM/YYYY', true).isValid();
}

function SalvaPeriodi(idric) {
    var error = false;
    $(".cf-bam").each(function () {
        if (error) return;

        var cf = $(this).val().trim();
        if (cf != "") {
            if (!CheckCF(cf)) {
                error = true;
                swal({
                    title: "Codice fiscale " + cf + " non valido",
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }
        }
    });
    $(".per-da,.per-a").each(function () {
        if (error) return;
        var d = $(this).val();
        if (d.trim() != "" && !DataValida(d)) {

            error = true;
            swal({
                title: "Data " + d + " non valida",
                type: "error",
                confirmButtonText: 'OK',
                customClass: 'rai'
            });
        }
    });
    $(".per-da").each(function () {
        if (error) return;
        var d1 = $(this).val();
        var prog = $(this).closest("tr").attr("data-progr");
        var d2 = $("#p2-" + prog).val();
        if (d1.trim() != "" && d2.trim() != "") {
            if (!PeriodoValido(d1, d2)) {
                error = true;
                swal({
                    title: "Periodo " + d1 + " - " + d2 + " non valido",
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }
        }


    });
    if (error) return;
    var form = $("#gest-form");





    swal({
        title: "Salvataggio dati",
        type: 'question',
        html: "Confermi di salvare le informazioni visualizzate?",
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $("#salva-per").addClass("disable");
        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            dataType: "json",
            data: $(form).serialize(),
            success: function (data) {
                $("#salva-per").removeClass("disable");
                debugger
                if (data.esito == true) {

                    swal({
                        title: 'Dati caricati correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    }).then(function () {

                        $("#popupview-gestioneperiodi").modal("hide");
                        ShowPeriodioDipendente(idric);
                    });
                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
            },
            error: function (a, b, c) {
                $("#salva-per").removeClass("disable");
                swal({
                    title: a + b + c,
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }
        });


    });

}
function GestisciPeriodi(idrich) {
    $("#popupview-gestioneperiodi").modal("show");
    $("#current-id-richiesta").val(idrich);
    $("#dettagli-per").addClass("rai-loader");
    $("#dettagli-per").html("");
    $.ajax({
        url: '/MaternitaCongedi/GetGestionePeriodi',
        type: "GET",
        contentType: false,
        data: { idrich: idrich },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-per").removeClass("rai-loader");
            debugger
            if (data == null || data == "") {
                swal({
                    title: "Nessun dato presente",
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                }).then(function () { $("#popupview-gestioneperiodi").modal("hide"); });
            }
            else {
                $("#dettagli-per").html(data);
            }

        }
    });
}
function AggiornaScadenze() {
    $("#refresh-scadenze").show();
    var scad = [];
    $("label.scadenza").each(function () {
        scad.push({
            idrich: $(this).attr("data-idrichiesta"), meseinvio: $(this).attr("data-meseinvio")
            , annoinvio: $(this).attr("data-annoinvio")
        });
    });
    var st = JSON.stringify(scad);
    $.ajax({
        url: '/MaternitaCongedi/GetScadenzeAjax',
        type: "POST",
        data: { dati: st },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            console.log("Scadenze changed:" + JSON.stringify(data.dati));
            $("#refresh-scadenze").hide();
            for (var i = 0; i < data.dati.length; i++) {
                var id = data.dati[i].idrich;
                var d = data.dati[i].dataScadenza;
                $("#label-scadenza-" + id).text(d);
            }
        }
    });
}
function attivaContatore(mesi, giorni, mesiconiuge, giorniconiuge) {

    //$('#congedi').data('plugin_contatore').giorniRimanenti();

    debugger
    var MF = "M";
    var mesiSpettanti = 6;
    if (MF == "M") mesiSpettanti = 7;

    var mesiPersi = 0;
    var giorniPersi = 0;
    if (mesiconiuge >= 4) {
        mesiPersi = mesiconiuge - 4;
        giorniPersi = giorniconiuge;
    }

    if ($("#congedi").html().trim() == "") {

        var valori = {
            sesso: MF,
            mesifruiti: mesi,
            giornifruiti: giorni,
            mesipratica: 0,
            giornipratica: 0,
            giorniconiuge: giorniconiuge,
            mesi: 6,
            mesibonuspadre: 1,
            mesiperbonuspadre: 3,
            mesiindennizati: 6,
            debug: false,
            alert: 10,
            onComplete: function () {

            }
        }
        console.log(JSON.stringify(valori));
        var instance =
            $('#congedi').contatore(valori);
    }
    else {
        $('#congedi').data('plugin_contatore').cambiaValore({ mesifruiti: mesi, giornifruiti: giorni, giorniconiuge: giorniconiuge });
    }
}
function IsPage1() {
    return $("#page1-amm").is(":visible")
}
function ShowViewCedolinoPagato(json) {

    console.log(json);
    $("#popupview-cedolino-pagato").modal("show")
    $("#dettagli-cedolino-pagato").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetDettaglioCedolinoPagato',
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(json),
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-cedolino-pagato").removeClass("rai-loader");
            $("#dettagli-cedolino-pagato").html(data);

        }
    });
}

function ShowPeriodioDipendente(idric) {
    $.ajax({
        url: "/maternitacongedi/GetPeriodiDipendente?idrichiesta=" + idric,
        type: 'GET',
        processData: false,
        contentType: false,
        dataType: "html",
        data: {},
        success: function (data) {
            $("#container-periodi-dip").html(data);
            debugger
            var g = $("#gen-solo").val();
            if (g.toLowerCase() == "true")
                $("#su-mesi").text(" / 6 mesi");
            else
                $("#su-mesi").text(" / 10 mesi");

            $.ajax({
                url: "/maternitacongedi/GetPeriodiConiuge?idrichiesta=" + idric,
                type: 'GET',
                processData: false,
                contentType: false,
                dataType: "html",
                data: {},
                success: function (data) {
                    $("#container-periodi-altro-genitore").html(data);
                    var giornidip = 0;
                    if ($("#totale-giorni-d").length > 0) {
                        if ($("#totale-giorni-d").val().trim() != "") {
                            giornidip = parseFloat($("#totale-giorni-d").val().trim());
                        }
                    }
                    var giornicon = 0;
                    if ($("#totale-giorni-c").length > 0) {
                        if ($("#totale-giorni-c").val().trim() != "") {
                            giornicon = parseFloat($("#totale-giorni-c").val().trim());
                        }
                    }
                    debugger
                    var mesidipContatore = parseInt(giornidip / 30);
                    var giornidipContatore = giornidip % 30;

                    attivaContatore(mesidipContatore, giornidipContatore, 0, giornicon);
                    var Rimanenti = $('#congedi').data('plugin_contatore').giorniRimanenti();
                    var giorniRimanenti = "";
                    if (Rimanenti.mesi > 0) {
                        if (Rimanenti.mesi == 1) giorniRimanenti = "1 mese e ";
                        else giorniRimanenti = Rimanenti.mesi + " mesi e ";
                    }
                    if (Rimanenti.giorni == 1) giorniRimanenti += "1 giorno ";
                    else giorniRimanenti += Rimanenti.giorni + " giorni";
                    $("#giorni-rimanenti").text(giorniRimanenti);
                }
            });




        }
    });
}
function ShowPeriodi(idric) {
    $("#popupview-periodi").modal("show")


    $("#dettagli-richiesta-2").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetDettaglioRichiestaAmmBoxPeriodi',
        type: "GET",
        data: { idrichiesta: idric },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-richiesta-2").removeClass("rai-loader");
            $("#dettagli-richiesta-2").html(data);

        }
    });

    ShowPeriodioDipendente(idric);



}
function InviaFileExcel() {
    var form = $("#form-excel");
    var fdata = new FormData();
    var fileInput = $('#file-excel');
    if (fileInput[0].files.length > 0) {
        var file = fileInput[0].files[0];
        fdata.append("file", file);
    }
    fdata.append("stato", $("#stato-pratica").val());
    $.ajax({
        url: "/maternitacongedi/UploadExcel",
        type: 'POST',
        processData: false,
        contentType: false,
        dataType: "json",
        data: fdata,
        success: function (data) {
            if (data.esito == true) {

                swal({
                    title: 'Pratiche caricate correttamente',
                    type: "success",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                }).then(function () {
                    GetContent();
                });
            }
            else {
                swal({
                    title: data.errore,
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }

        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    })

}
function showname() {
    var name = document.getElementById('file-excel');
    var nomefile = name.files.item(0).name;

    if (nomefile.toUpperCase().indexOf(".XLSX") < 0) {
        swal({
            title: "Sono ammessi soltanto file excel (.xlsx)",
            type: "error",
            confirmButtonText: 'OK',
            customClass: 'rai'
        });
        return;
    }
    $("#invia-file").removeClass("disable");

};

function GetContentApprovazioni() {
    $.ajax({
        url: '/MaternitaCongedi/GetContentApprovazioni',
        type: "GET",
        data: {},
        dataType: "html",
        complete: function () { $("#content-approvazioni").removeClass("rai-loader") },
        success: function (data) {

            $("#appr-container").html(data);
            $("#content-approvazioni").removeClass("rai-loader");
        }
    });

}

function GetContent(ordine, checkscadenze) {
    debugger
    var seltab = 1;
    if ($("#li-2").hasClass("active")) {
        seltab = 2;
    }
    if ($("#li-3").hasClass("active")) {
        seltab = 3;
    }
    $("#content-container .panel-body").first().addClass("rai-loader")
    $.ajax({
        url: '/MaternitaCongedi/GetContent',
        type: "GET",
        data: { ordine: ordine },
        dataType: "html",
        complete: function () { $("#content-container .panel-body").first().removeClass("rai-loader") },
        success: function (data) {
            $("#content-container .panel-body").first().removeClass("rai-loader")
            $("#content-container").html(data);
            $("#li-" + seltab + " a").click();
            if (checkscadenze == true) AggiornaScadenze();
            $("#tab2").addClass("rai-loader");
            $("#tab3").addClass("rai-loader");



            $.ajax({
                url: '/MaternitaCongedi/GetContent2',
                type: "GET",
                data: { ordine: ordine },
                dataType: "html",
                complete: function () { $("#content-container .panel-body").first().removeClass("rai-loader") },
                success: function (data) {
                    debugger
                    $("#li-" + seltab + " a").click();
                    if (checkscadenze == true) AggiornaScadenze();
                    $("#tab2").html($(data).find("#tab2"));
                    $("#tab3").html($(data).find("#tab3"));
                    $("#tab2").removeClass("rai-loader");
                    $("#tab3").removeClass("rai-loader");
                }
            });



        }
    });
}
function ShowPianificazione(idRichiesta) {
    $("#popup-view-pianificazione").modal("show");
    $.ajax({
        type: 'GET',
        url: "/maternitacongedi/GetPianificazioneContent",
        dataType: "html",
        data: { idRichiesta: idRichiesta },
        cache: false,
        success: function (data) {
            $("#calendario-pianificazione").html(data);
            $("#control-string").val(getControlString());

        }
    });
}
function reset() {
    $("#datamese").val("");
    $("#datatask").val("");
    $("#matricola").val("");
    $("#sede").val("");
    $("#tipo").val("");
    $("#stato").val("");

}
function isHC() {
    return $("#IsHC").length > 0;
}
function search() {
    var mese = $("#datamese").val();
    var meseTask = $("#datatask").val();
    var matr = $("#matricola").val();
    var sede = $("#sede").val();
    var tipo = $("#tipo").val();
    var stato = $("#stato").val();
    var assenza = $("#assenza").val();
    $("table.maternita").addClass("rai-loader");
    // $("table.matapp").addClass("rai-loader");
    var liactive = "1";
    if ($("#li-2").hasClass("active")) liactive = "2";
    if ($("#li-3").hasClass("active")) liactive = "3";
    var listone = $("#cb-listone-storni").prop("checked") ? "1" : "0";

    $.ajax({
        url: '/MaternitaCongedi/GetContent',
        type: "GET",
        data: { mese: mese, matr: matr, sede: sede, tipo: tipo, stato: stato, mesetask: meseTask , assenza:assenza, listone:listone},
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("table.maternita").removeClass("rai-loader");
            $("#content-container").html(data);
            debugger
            $("#li-" + liactive + " a").click();

            var seltab = liactive;

            $("#tab2").addClass("rai-loader");
            $("#tab3").addClass("rai-loader");



            $.ajax({
                url: '/MaternitaCongedi/GetContent2',
                type: "GET",
                data: { mese: mese, matr: matr, sede: sede, tipo: tipo, stato: stato, mesetask: meseTask, assenza: assenza, listone: listone },
                dataType: "html",
                complete: function () { $("#content-container .panel-body").first().removeClass("rai-loader") },
                success: function (data) {
                    debugger
                    $("#li-" + seltab + " a").click();
                    //if (checkscadenze == true) AggiornaScadenze();
                    $("#tab2").html($(data).find("#tab2"));
                    $("#tab3").html($(data).find("#tab3"));
                    $("#tab2").removeClass("rai-loader");
                    $("#tab3").removeClass("rai-loader");
                }
            });





        }
    });
    //$.ajax({
    //    url: '/MaternitaCongedi/GetContentApprovazioni',
    //    type: "GET",
    //    data: { mese: mese, matr: matr, sede: sede, tipo: tipo, stato: stato },
    //    dataType: "html",
    //    complete: function () { },
    //    success: function (data) {
    //        $("table.matapp").removeClass("rai-loader");
    //        $("#appr-container").html(data);

    //    }
    //});
}
function VisualizzaUffPersonale(idRichiesta) {
    VisualizzaGestione(idRichiesta);
}

function VisualizzaAmmInternal(titolo, idrichiesta, isAvviata,isCollegata) {
    $.ajax({
        url: '/MaternitaCongedi/getDettaglioAmm',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            debugger
            $("#dettagli-amm").removeClass("rai-loader");
            if (!isAvviata)
            {
                $("#dettagli-amm").html(data);
               // if (isCollegata) {
               //     ConfermaModificheCedolino(idrichiesta);
               // }
            }
            else {
                var html = $("#task-recap-container").html();
               
                $("#dettagli-amm").html(data);
                $("#task-recap-container").html(html);

                $("#page3-amm").show();
                $("#page1-amm").hide();
                $("#page2-amm").hide();
            }
            $("#cat-domanda").text(titolo);
            debugger
            ShowCedolino(idrichiesta, false, isCollegata);

          //  if ($(data).find("#operazioni-avviate").length > 0)
          //     AmmPage(3);

        }
    });
    $.ajax({
        url: '/MaternitaCongedi/GetDettaglioRichiestaAmmBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-richiesta-amm").removeClass("rai-loader");
            $("#dettagli-richiesta-amm").html(data);
            CaricaNote(idrichiesta);
        }
    });

    $("#promemoria").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetPromemoriaBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#promemoria").removeClass("rai-loader");
            $("#promemoria").html(data);
        }
    });


    $.ajax({
        url: '/MaternitaCongedi/GetAssegnazioneAmmBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#assegnazione-amm").removeClass("rai-loader");
            $("#assegnazione-amm").html(data);

        }
    });

    $.ajax({
        url: '/MaternitaCongedi/GetAnnullamentoAmmBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            console.log(data);
            $("#annullamento-amm").removeClass("rai-loader");
            $("#annullamento-amm").html(data);

        }
    });


}

function VisualizzaAmm(titolo, idrichiesta, isAvviata, isCollegata) {
    debugger
    $("#popupview-amm").modal("show");
    $("#alert-lavorata").hide();
    $("#current-id-richiesta").val(idrichiesta);

    $("#dettagli-amm").html("");
    $("#dettagli-amm").addClass("rai-loader");

    $("#dettagli-richiesta-amm").html("");
    $("#dettagli-richiesta-amm").addClass("rai-loader");

    $("#assegnazione-amm").html("");
    $("#assegnazione-amm").addClass("rai-loader");

    $("#cat-domanda").text(titolo);


    /*
     <div class="row rai-loader" style="margin-top:20px;height:50px" id="div-buttons"></div>
                    <div class="col-sm-12">
                        <div class="rai-btn-fill">

                            <button id="indietro-concludi-pratica" class="btn rai-btn-secondary " type="button" onclick="AmmPage(1)">Indietro </button>

                            <button id="concludi-pratica" type="button" class="btn rai-btn-primary disable" onclick="ConcludiPratica(4296)">Concludi la pratica</button>
                        </div>
                    </div>
                </div>
     */
    if (isAvviata) {
       debugger
        $("#dettagli-amm").html("<div id='page3-amm'><div id='task-recap-container'></div>" +
            ' <div class="row rai-loader" style="margin-top:20px;height:50px" id="div-buttons"></div>' +
            '</div>');
       AmmPage(3);

        VisualizzaAmmInternal(titolo, idrichiesta, isAvviata, isCollegata);
    }
    else
        VisualizzaAmmInternal(titolo, idrichiesta, isAvviata, isCollegata);
   

    //CaricaNote(idrichiesta);
}
function AmmPage(page) {
    $("#refresh-operazioni").hide();

    for (var i = 1; i <= 3; i++) {
        if (page == i) {
            //$("#amm" + i + "-ok").hide();
            //$("#amm" + i + "-cur").show();
            //$("#amm" + i + "-todo").hide();

            $('#amm' + i + "-ok").closest('li').removeClass('completed').addClass('active');
        }
        if (i < page) {
            debugger
            //$("#amm" + i + "-ok").show();
            //$("#amm" + i + "-cur").hide();
            //$("#amm" + i + "-todo").hide();

            $('#amm' + i + "-ok").closest('li').removeClass('active').addClass('active');
        }
        if (i > page) {
            //$("#amm" + i + "-ok").hide();
            //$("#amm" + i + "-cur").hide();
            //$("#amm" + i + "-todo").show();

            $('#amm' + i + "-ok").closest('li').removeClass('completed').removeClass('active');
        }
    }
    debugger
    $("#page1-amm").hide();
    $("#page2-amm").hide();
    $("#page3-amm").hide();
    $("#page" + page + "-amm").show();

    var idric = $("#current-id-richiesta").val();

    if (page == 2) {


        var incongruenze = 0;
        if ($("#giorni-incongruenti").length > 0) {
            incongruenze = $("#giorni-incongruenti").attr("data-giorni");
        }
        var giornicedolino = $("#tot-giorni").text();

        $("#task-container").html("<div style='height:200px'></div>");
        $("#task-container").addClass("rai-loader");
        $("#popup-view-amm-content").scrollTop(0);
        var Importo13ma = $("#13ma").text();
        var Importo14ma = $("#14ma").text();
        var ImportoPremio = $("#premio").text();
        var ImportoTotale = $("#importo-accordion").text();
        var TotaleGiornaliero = $("#totale-giornaliero").text();
        var G26 = [];
        $(".tot-giorni").each(function () {
            debugger
            var totalNonEccezioneRichiesta = 0;

            var eccTrovate = [];
            $(this).closest("table").find("span.codice-ecc").each(function () {
                var codecc = $(this).text();
                if (codecc != $("#eccezione-ris").val() && codecc.indexOf($("#eccezione-ris").val()) != 0) {
                    var rowcedo = $(this).closest(".row-cedo");
                    var v = $(rowcedo).find(".num-giorni").text();
                    //alert(v);
                    totalNonEccezioneRichiesta += parseFloat(v.replace(",", "."));

                }
            });
            debugger
            if (totalNonEccezioneRichiesta > 0) {
                console.log("Tolte ecc non pertinenti per " + totalNonEccezioneRichiesta);
                var total = parseFloat($(this).text().split('/')[0].replace(",", ".")) - totalNonEccezioneRichiesta;
                var text = total.toString().replace(".", ",") + "/" + $(this).text().split('/')[1];
                G26.push($(this).attr("data-meseanno") + ":" + text);
            }
            else {
                G26.push($(this).attr("data-meseanno") + ":" + $(this).text());
            }

        });


        debugger
        $.ajax({
            url: '/MaternitaCongedi/getTasks',
            type: "GET",
            data: {
                idrichiesta: idric, incongruenze: incongruenze, giornicedolino: giornicedolino, Importo13ma: Importo13ma, Importo14ma: Importo14ma, ImportoPremio: ImportoPremio, ImportoTotale: ImportoTotale, TotaleGiornaliero: TotaleGiornaliero,
                meseanno: $("#mese-anno-comp").val(), giorniforzati: $("#giorni-forzati").val(),
                g26mesi: G26.join('|')
            },
            dataType: "html",
            complete: function () { },
            success: function (data) {
                $("#task-container").removeClass("rai-loader");
                $("#task-container").html(data);
                //$("#avvia-pratica").removeClass("disable");
            }
        });

    }
    if (page == 3) {
        $("#refresh-operazioni").show();
        $("#task-recap-container").addClass("rai-loader");
        $.ajax({
            url: '/MaternitaCongedi/getTasksRecap',
            type: "GET",
            data: { idrichiesta: idric },
            dataType: "html",
            complete: function () { },
            success: function (data) {
                $("#task-recap-container").removeClass("rai-loader");
                $("#task-recap-container").html(data);
                $("#dettagli-amm").removeClass("rai-loader");
            }
        });
    }
}
function CaricaNote(idrichiesta) {
    $(".note-amm:visible").addClass("rai-loader");
    $(".note-amm:visible").html("");
    $("#badge-note").hide();

    $.ajax({
        url: '/MaternitaCongedi/GetNoteBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            debugger
            $(".note-amm:visible").removeClass("rai-loader");
            $(".note-amm:visible").html(data);

        }
    });
}
function AbilitaButtonAssegna() {

    var operatore = $("#assegna-operatori").val();

    if (operatore == null || operatore == "")
        $("#button-assegna").addClass("disable");
    else
        $("#button-assegna").removeClass("disable");
}
function PrendiCar(idrichiesta, risposta) {
    if (risposta == 3) swal.close();

    if (risposta == 1) PrendiInCaricoAmmInternal(idrichiesta, true);

    if (risposta == 2) PrendiInCaricoAmmInternal(idrichiesta, false);
}

function PrendiInCaricoAmm(idrichiesta) {
    $.ajax({
        url: '/MaternitaCongedi/PrendiInCaricoAmmCheckStessoMese',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            debugger
            if (data.data != "") {

                //$(document).on("click", '#b-annulla', function (event) {
                //    swal.close();
                //});


                //$(document).on("click", '#b-ok', function (event) {
                //    PrendiInCaricoAmmInternal(idrichiesta, true);
                //});
                //$(document).on("click", '#b-no', function (event) {
                //    PrendiInCaricoAmmInternal(idrichiesta, false);
                //});
                swal({
                    title: "Presa in carico",
                    type: 'question',

                    html: "Per la matricola selezionata risultano ulteriori periodi : " + data.data +
                        "<div style='margin-top:40px'>" +
                        "<a id='b-no' onmousedown='PrendiInCaricoAmmInternal(" + idrichiesta + ", false);'  type='button' class='btn rai-btn-secondary' style='margin-right:6px' >No, prendi solo questa</a>" +
                        "<a id='b-ok' onmousedown='PrendiInCaricoAmmInternal(" + idrichiesta + ", true);' type='button' class='btn rai-btn-primary' style='margin-right:6px'>Ok, prendile tutte</a>" +
                        "<br /><br /><a onmousedown='swal.close();' id='b-annulla' type='button' class='' >Annulla</a></div> ",

                    showConfirmButton: false,
                    showCancelButton: false,
                    //confirmButtonText: '<i class="fa fa-check"></i> Conferma',
                    //cancelButtonText: '<i class="fa fa-times"></i> Annulla',
                    reverseButtons: true,
                    customClass: 'rai rai-confirm-cancel'
                });
            }
            else {
                swal({
                    title: "Presa in carico",
                    type: 'question',
                    html: "Confermi di prendere in carico la richiesta visualizzata?",
                    showCancelButton: true,
                    confirmButtonText: '<i class="fa fa-check"></i> Conferma',
                    cancelButtonText: '<i class="fa fa-times"></i> Annulla',
                    reverseButtons: true,
                    customClass: 'rai rai-confirm-cancel'
                }).then(function () {
                    PrendiInCaricoAmmInternal(idrichiesta, false);
                });
            }
        }
    });
}

function PrendiInCaricoAmmInternal(idrichiesta, comprese) {
    $.ajax({
        url: '/MaternitaCongedi/PrendiInCaricoAmministrazione',
        type: "GET",
        data: { idrichiesta: idrichiesta, comprese: comprese },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            if (data.esito == true) {

                swal({
                    title: 'Richiesta presa in carico correttamente',
                    type: "success",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                }).then(function () {
                    VisualizzaAmm('', idrichiesta);
                    GetContent();
                });

            }
            else {
                swal({
                    title: data.errore,
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }
        }
    });

}
function RilasciaAmm(idrichiesta) {
    swal({
        title: "Rilascio richiesta",
        type: 'question',
        html: "Confermi di rilasciare la richiesta visualizzata?",
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: '/MaternitaCongedi/RilasciaAmministrazione',
            type: "GET",
            data: { idrichiesta: idrichiesta },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.esito == true) {

                    swal({
                        title: 'Richiesta rilasciata correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    }).then(function () {
                        VisualizzaAmm('', idrichiesta);
                        GetContent();
                    });

                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
            }
        });
    });



}
function AssegnaRichiesta(idrichiesta) {
    AssegnaRichiestaAmm(idrichiesta);
}
function AssegnaRichiestaAmm(idrichiesta) {
    var operatore = $("#assegna-operatori").val();

    swal({
        title: "Riassegnazione richiesta",
        type: 'question',
        html: "Confermi di riassegnare la richiesta visualizzata?",
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: '/MaternitaCongedi/assegna',
            type: "GET",
            data: { operatore: operatore, idRichiesta: idrichiesta },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.esito == true) {
                    $("#popupview-amm").modal("hide");
                    GetContent();
                    swal({
                        title: 'Richiesta assegnata correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
            }
        });
    });


}

function ShowCedolino(idrichiesta, modifica, isCollegata) {
    var mese = $("#mese-competenza").attr("data-mesecomp");
    var anno = $("#mese-competenza").attr("data-annocomp");
    debugger
    if (modifica == undefined) modifica = false;

    // $("#popupview-cedolino").modal("show");
    $.ajax({
        url: '/MaternitaCongedi/GetDettaglioCedolinoAmm',
        type: "GET",
        data: { idrichiesta: idrichiesta, modifica: modifica, mese: mese, anno: anno },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            debugger
            $("#ced-cont").html(data);
            //if (isCollegata) {
            //    ConfermaModificheCedolino(idrichiesta);
            //}
        }
    });
}
function CambiaMeseImportiCedolino(idric) {
    debugger
    var meseanno = $("#mese-anno-comp").val();
    var mese = meseanno.split("/")[0];
    var anno = meseanno.split("/")[1];
    var modifica = false;

    $.ajax({
        url: '/MaternitaCongedi/GetDettaglioCedolinoCambioMese',
        type: "GET",
        data: { idrichiesta: idric, modifica: modifica, mese: mese, anno: anno },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            debugger
            $("#ced-cont").html(data);

        }
    });
}
function PredisponiNuovaNota() {
    $("#pre-nota").hide();
    $("#new-nota").show();
}
function CancellaNotaCongedi(idnota, idrichiesta) {
    swal({
        title: "Conferma cancellazione",
        type: 'question',
        html: "Confermi di cancellare la nota?",
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        $.ajax({
            url: "/maternitacongedi/cancellanota",
            type: 'POST',
            dataType: "json",
            data: { idnota: idnota },
            success: function (data) {
                if (data.esito == true) {

                    swal({
                        title: 'Nota eliminata correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    }).then(function () {
                        CaricaNote(idrichiesta);
                    });
                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }

            },
            error: function (a, b, c) {
                alert(a + b + c);
            }
        });
    });
}
function HideNote() {
    $("#note-hidden").hide();
    $("#show-note").show();
}
function ShowNoteHidden() {
    $("#note-hidden").show();
    $("#show-note").hide();

}
function ModificaNotaCongedi(idnota, idrichiesta) {
    $.ajax({
        url: "/maternitacongedi/getnota",
        type: 'GET',
        dataType: "html",
        data: { idnota: idnota },
        success: function (data) {
            $("#nota-container-" + idnota).html(data);
        }
    });

}
function ModificaNota(idnota) {
    var fdata = new FormData();
    var fileInput = $('#file-nota-' + idnota);
    if (fileInput[0].files.length > 0) {
        var file = fileInput[0].files[0];
        fdata.append("file", file);
        fdata.append("nomefile", fileInput[0].files[0].name);
    }
    fdata.append("testo", $("#nota-mod-" + idnota).val());
    fdata.append("idrichiesta", $("#id-richiesta").val());
    fdata.append("idnota", idnota);
    fdata.append("visibilita", $('input[name="visibilita-' + idnota + '"]:checked').val());

    $.ajax({
        url: "/maternitacongedi/salvanota",
        type: 'POST',
        processData: false,
        contentType: false,
        dataType: "json",
        data: fdata,
        success: function (data) {
            if (data.esito == true) {

                swal({
                    title: 'Nota modificata correttamente',
                    type: "success",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                }).then(function () {
                    CaricaNote($("#id-richiesta").val());
                });
            }
            else {
                swal({
                    title: data.errore,
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }

        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });
}
function AggiungiNota() {

    var fdata = new FormData();
    var fileInput = $('#file-nota');
    if (fileInput[0].files.length > 0) {
        var file = fileInput[0].files[0];
        fdata.append("file", file);
        fdata.append("nomefile", fileInput[0].files[0].name);
    }


    fdata.append("testo", $("#nota-amm").val());
    fdata.append("idrichiesta", $("#id-richiesta").val());
    fdata.append("idnota", $("#id-nota").val());
    fdata.append("visibilita", $('input[name="visibilita"]:checked').val());


    $.ajax({
        url: "/maternitacongedi/salvanota",
        type: 'POST',
        processData: false,
        contentType: false,
        dataType: "json",
        data: fdata,
        success: function (data) {
            if (data.esito == true) {

                swal({
                    title: 'Nota registrata correttamente',
                    type: "success",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                }).then(function () {
                    CaricaNote($("#id-richiesta").val());
                });
            }
            else {
                swal({
                    title: data.errore,
                    type: "error",
                    confirmButtonText: 'OK',
                    customClass: 'rai'
                });
            }

        },
        error: function (a, b, c) {
            alert(a + b + c);
        }
    });


}
function FileNotaChanged(inp) {
    $("#nomefile-nota").text($(inp)[0].files[0].name);
    $("#nomefile-container").show();
    $("#get-alle").hide();
}
function FileNotaChangedModificata(idnota, inp) {
    $("#nomefile-nota-" + idnota).text($(inp)[0].files[0].name);
    $("#nomefile-container-" + idnota).show();
    $("#get-alle-" + idnota).hide();
}
function AllegaDocNota() {
    $("#file-nota").click();
}
function AllegaDocNotaModificata(idnota) {
    $("#file-nota-" + idnota).click();
}

function AbilitaButtonAggNota() {
    var t = $("#nota-amm").val().trim().length;
    if (t > 0) {
        $("#agg-nota").removeClass("disable");
    }
    else {
        $("#agg-nota").addClass("disable");
    }
}
function AbilitaButtonModNota(idnota) {
    var t = $("#nota-mod-" + idnota).val().trim().length;
    if (t > 0) {
        $("#mod-nota-" + idnota).removeClass("disable");
    }
    else {
        $("#mod-nota-" + idnota).addClass("disable");
    }
}


function CarattereValido(tipoCampo, carattereDaVerificare) {
    var ammessi = getRangeCharTipoTesto(tipoCampo);
    return (ammessi.indexOf(carattereDaVerificare) >= 0);
}


function getRangeCharTipoTesto(TipoTesto) {
    var chValS = "$%&='_#()[]{}/\\><-£";
    var chValSP = " ";
    var chValAT = "@@";
    var chValP = ".,:;!?";
    var chValN = "123456789";
    var chValSN = "-+";
    var chValAP = "'";
    var chValNZ = "0";
    var chValAst = "*";
    var chValVirg = ",";
    var chValPunt = ".";
    var chValA = "'’abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZèéàòìù°€";
    var chValCAP = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    var chValNEG = "{}ABCDEFGHIJKLMNOPQR";
    var chValMA = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    var chCFis = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var chPersonale = "";
    var chValVari = "-_()";
    var chVbCrLf = String.fromCharCode(13, 10);
    var appoRet = "";

    switch (TipoTesto) {
        case "A":
        case "LIBERO":
            appoRet = chValS + chValSP + chValAT + chValP + chValN + chValSN + chValAP + chValNZ + chValAst + chValPunt + chValVirg + chValA + chValCAP + "/";
            break;
        case "B":
        case "ALFANUMERICO":
            appoRet = chValMA + chValN + chValSP + chValNZ;
            break;
        case "C":
        case "ALFANUMERICO ESTESO":
            appoRet = chValMA + chValN + chValSP + chValS + chValP + chValAT + chValAst + chValSN + chValNZ;
            break;
        case "D":
        case "SOLO TESTO":
            appoRet = chValMA + chValSP;
            break;
        case "K":
        case "TESTO ESTESO":
            appoRet = chValCAP + chValSP + chValS + chValP + chValAT + chValAst + chValSN;
            break;
        case "E":
        case "SOLO NUMERO":
            appoRet = chValNZ + chValN + chValSN;
            break;
        case "L":
        case "NUMERO ESTESO":
            appoRet = chValNZ + chValN + chValS + chValP + chValAT + chValAst + chValSN + chValSP;
            break;
        case "F":
        case "IMPORTO SEGNATO":
            appoRet = chValNZ + chValN + chValNEG;
            break;
        case "G":
        case "GIORNO":
            appoRet = chValNZ + chValN;
            break;
        case "H":
        case "MESE":
            appoRet = chValNZ + chValN;
            break;
        case "P":
        case "J":
        case "X":
        case "Y":
        case "IMPORTO SEGNATO ESTESO":
            appoRet = chValNZ + chValN + chValNEG + chValSP;
            break;
        case "Z":
            appoRet = "";
            break;
        default:
            appoRet = null;
            break;
    }
    return appoRet;
}

function ApriPratica(categoria, idpratica) {
    $("#popupviewAmm").modal("hide");
    VisualizzaAmm(categoria, idpratica);
}


/////CONTENT
function vediTutte() {
    location.href = "/maternitaCongedi";
}

function SelezTuttiPresaCarico() {
    var c = $("#sel-tutti-pc").prop("checked");
    $(".sel-idrichieste").prop("checked", c);
    $(".group-master").prop("checked", c);
    CheckSelez();
}
$(".group-master").click(function (e) {
    debugger
    var stato = $(this).prop("checked");
    var groupnumber = $(this).closest("tbody").attr("data-master");
    $("tbody[data-slave=" + groupnumber + "] input[type=checkbox]").each(function () {
        $(this).prop("checked", stato);
    });
    CheckSelez();
    // alert("lll");
    // e.preventDefault();
    e.stopPropagation();
    //return false;
});

function PrendiInCaricoSelez() {
    swal({
        title: "Prendi in carico",
        type: 'question',
        html: "Confermi di prendere in carico tutte le richieste selezionate?",
        showCancelButton: true,
        confirmButtonText: '<i class="fa fa-check"></i> Conferma',
        cancelButtonText: '<i class="fa fa-times"></i> Annulla',
        reverseButtons: true,
        customClass: 'rai rai-confirm-cancel'
    }).then(function () {
        var idric = [];
        $(".sel-idrichieste:checked").each(function () {
            idric.push($(this).closest("tr").attr("data-id"));
        });
        var ids = idric.toString();
        $.ajax({
            url: '/MaternitaCongedi/PrendiInCaricoMazzo',
            type: "GET",
            data: { ids: ids },
            dataType: "json",
            complete: function () { },
            success: function (data) {
                if (data.esito == true) {

                    swal({
                        title: 'Richieste prese in carico correttamente',
                        type: "success",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    }).then(function () {
                        GetContent();
                    });
                }
                else {
                    swal({
                        title: data.errore,
                        type: "error",
                        confirmButtonText: 'OK',
                        customClass: 'rai'
                    });
                }
            }
        });
    });

}
function CheckSelez() {
    var qs = $(".sel-idrichieste:checked").length;
    if (qs > 0) {
        $("#bar-selez").show();
        $("#cont-selez").text(qs + " selezionat" + (qs == 1 ? "o" : "i"));
    }
    else {
        $("#bar-selez").hide();
    }
}
$("#annulla-ordine").click(function (e) {
    e.stopPropagation();
    GetContent();
});
function OrdinaPratiche(ordine) {
    if (ordine == "N2") {
        for (var i = 1; i <= 3; i++) {
            var t = $('#table-mat' + i + ' .tbody' + i).sort(function (a, b) {
                var a1 = ($(a).attr('data-nom'));
                var a2 = ($(b).attr('data-nom'));
                return a1 <= a2 ? 1 : -1;
            });
            $("#table-mat" + i + " thead").after(t)
        }
        $(".N1,.N2,.T1,.T2,.S1,.S2,.A1,.A2").removeClass("freccia-ord-ena").addClass("freccia-ord-dis");

        $(".N1").attr("onclick", "OrdinaPratiche('N1')");
        $(".N2").removeClass("freccia-ord-dis").addClass("freccia-ord-ena").attr("onclick", "OrdinaPratiche('N2')");
        return;
    }
    if (ordine == "N1") {
        for (var i = 1; i <= 3; i++) {
            var t = $('#table-mat' + i + ' .tbody' + i).sort(function (a, b) {
                var a1 = ($(a).attr('data-nom'));
                var a2 = ($(b).attr('data-nom'));
                return a1 >= a2 ? 1 : -1;
            });
            $("#table-mat" + i + " thead").after(t)
        }
        $(".N1,.N2,.T1,.T2,.S1,.S2,.A1,.A2").removeClass("freccia-ord-ena").addClass("freccia-ord-dis");
        $(".N1").removeClass("freccia-ord-dis").addClass("freccia-ord-ena").attr("onclick", "OrdinaPratiche('N1')");
        $(".N2").attr("onclick", "OrdinaPratiche('N2')");
        return;
    }
    //if (ordine = "T1") {
    //    for (var i = 1; i <= 3; i++) {
    //        var t = $('#table-mat' + i + ' .tbody' + i).sort(function (a, b) {
    //            var a1 = ($(a).attr('data-tip'));
    //            var a2 = ($(b).attr('data-tip'));

    //            var es = a1 < a2 ? 1 : -1;
    //            console.log(a1, a2, es);
    //            return es;
    //        });
    //        $("#table-mat" + i + " thead").after(t)
    //    }
    //    $(".N1,.N2,.T1,.T2,.S1,.S2,.A1,.A2").removeClass("freccia-ord-ena").addClass("freccia-ord-dis");

    //    $(".T2").attr("onclick", "OrdinaPratiche('T2')");
    //    $(".T1").removeClass("freccia-ord-dis").addClass("freccia-ord-ena").attr("onclick", "OrdinaPratiche('T1')");
    //    return;
    //}
    //if (ordine == "T2") {
    //    for (var i = 1; i <= 3; i++) {
    //        var t = $('#table-mat' + i + ' .tbody' + i).sort(function (a, b) {
    //            var a1 = ($(a).attr('data-tip'));
    //            var a2 = ($(b).attr('data-tip'));
    //            var es = a1 >= a2 ? 1 : -1;
    //            console.log(a1, a2,es);
    //            return es;
    //        });
    //        $("#table-mat" + i + " thead").after(t)
    //    }
    //    $(".N1,.N2,.T1,.T2,.S1,.S2,.A1,.A2").removeClass("freccia-ord-ena").addClass("freccia-ord-dis");

    //    $(".T1").attr("onclick", "OrdinaPratiche('T1')");
    //    $(".T2").removeClass("freccia-ord-dis").addClass("freccia-ord-ena").attr("onclick", "OrdinaPratiche('T2')");
    //    return;
    //}
    GetContent(ordine);
}
function VisualizzaGestione(idrichiesta, FromApprovatoreGestione) {
    
    debugger
    $("#popupview-gestione").modal("show");
    CaricaNote(idrichiesta);
    if (!FromApprovatoreGestione)
        $("#cont-gio").hide();
    else
        $("#cont-gio").show();

    $("#promemoria").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetPromemoriaBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#promemoria").removeClass("rai-loader");
            $("#promemoria").html(data);
        }
    });

    $("#button-prendi").attr("onclick", "PrendiInCarico(" + idrichiesta + ")");
    $("#dettagli-container").html("");
    $("#dettagli-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneGestione',
        type: "GET",
        data: { idrichiesta: idrichiesta, FromApprovatoreGestione: FromApprovatoreGestione },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-container").removeClass("rai-loader");
            $("#dettagli-container").html(data);
            AnnullaModificaEccezione();
        }
    });

    $("#congedi").html("");
    $("#dettagli-richiesta-container").html("");
    $("#dettagli-richiesta-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneDettaglioRichiesta',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#dettagli-richiesta-container").removeClass("rai-loader");
            $("#dettagli-richiesta-container").html(data);
            debugger
            var isAF = $("#ecc-risu").length > 0 && $("#ecc-risu").text() != null &&
                ($("#ecc-risu").text().startsWith("AF") || $("#ecc-risu").text().startsWith("BF") || $("#ecc-risu").text().startsWith("CF"));

            if (FromApprovatoreGestione && isAF) {
                AjaxAttivaContatore(idrichiesta);
            }
            else
                $("#cont-gio").remove();
        }
    });

    $("#info-uffperscontainer").html("");
    $.ajax({
        url: '/MaternitaCongedi/GetUffPersonaleInfoBox',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#info-uffperscontainer").html(data);
        }
    });


    $("#assegnazione-container").html("");
    $("#assegnazione-container").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneAssegnazione',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#assegnazione-container").removeClass("rai-loader");
            $("#assegnazione-container").html(data);
        }
    });


    $.ajax({
        url: '/MaternitaCongedi/GetVisualizzazioneAnnullamento',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "html",
        complete: function () { },
        success: function (data) {
            $("#annullamento-container").html(data);
        }
    });

   

}
function AjaxAttivaContatore(idrichiesta) {
    //##contatore
    $("#congedi").addClass("rai-loader");
    $.ajax({
        url: '/MaternitaCongedi/GetValoriContatore',
        type: "GET",
        data: { idrichiesta: idrichiesta },
        dataType: "json",
        complete: function () { },
        success: function (data) {

            //function attivaContatore(mesi, giorni, mesiconiuge, giorniconiuge, MF) {
            //esempio attivacontatore
            attivaContatore(data.mesi, data.giorni, data.mesiconiuge, data.giorniconiuge, data.sesso);
            $("#congedi").removeClass("rai-loader");
        }
    });

}



$(".promemoria").popover({
    content: function () {
        return CreaPopoverPromemoria($(this).attr("data-promemoria"), $(this).attr("id"));
    }
});
$(".scaduta").popover({
    content: function () {
        return CreaPopoverScaduta($(this).attr("data-idrichiesta"));
    }
});
$(".promemoria").on('shown.bs.popover', function () {
    InitDatePicker();
});

function CreaPopoverPromemoria(data, id) {
    var r = "<span class='rai-font-md-bold'>PROMEMORIA RICHIESTA</span>" +
        "<span style='cursor:pointer;float: right;margin-top: -2px;' onclick='$(\"#" + id + "\").popover(\"hide\")'>x</span>" +
        "<div style='width:100%;height:1px;background-color:#ddd;margin-top:12px;margin-bottom:8px'></div>" +
        "<div class='row' style='margin-bottom:20px;margin-top:20px'>" +
        "<div class='col-sm-12 rai-font-sm-neutral' style='padding-left:5px'><span>Questa richiesta compare in alto perchè hai impostato un promemoria</span></div>" +
        "<div class='col-sm-12 rai-font-md-bold' style='padding-top:16px;padding-left:5px'><span>CAMBIA DATA PROMEMORIA</span></div>" +

        "</div>" +
        "<div class='row' style='margin-bottom:0px'>" +
        "<div class='input-group mb-md'>" +
        "<span onclick='$('#datadal').datetimepicker('show')' class='input-group-addon'><i class='icons icon-calendar'></i></span>" +
        "<input value='" + data + "' class='js-datetimepicker form-control' data-format='DD/MM/YYYY' data-locale='it' type='text' id='datapro-" + id + "' placeholder='Data promemoria'>" +
        "</div>"
        + "<a class='btn rai-btn-small full-width' onclick='CambiaDataPromemoria(\"" + id + "\")'>Salva</a>"
    "</div>";
    return r;
}
function CreaPopoverScaduta(idrichiesta) {
    var r = "<span class='rai-font-md-bold'>RICHIESTA NON LAVORATA</span>" +
        "<span style='cursor:pointer;float: right;margin-top: -2px;' onclick='$(\"#alert-" + idrichiesta + "\").popover(\"hide\")'>x</span>" +
        "<div style='width:100%;height:1px;background-color:#ddd;margin-top:12px;margin-bottom:8px'></div>" +
        "<div class='row' style='margin-bottom:20px;margin-top:20px'>" +
        "<div class='col-sm-12 rai-font-sm-neutral' style='padding-left:5px'><span>La richiesta non è stata presa in carico prima della scadenza. Clicca sul pulsante sotto per nasconderla dalla visualizzazione su questo computer.</span></div>" +

        "</div>" +
        "<div class='row' style='margin-bottom:0px'>" +

        "<a class='btn rai-btn-small full-width' onclick='NascondiRichiesta(" + idrichiesta + ")'>Nascondi richiesta</a>"
    "</div>";
    return r;
}
function NascondiRichiesta(idrichiesta) {

    var t = getCookieMat();
    var idRichiestaNascosti = t.split(',');
    idRichiestaNascosti.push(idrichiesta);
    setCookieMat(idRichiestaNascosti.toString(), 1000)
    GetContent();
}

function getCookieMat() {
    var name = "maternita-cookie" + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
function setCookieMat(cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = "maternita-cookie" + "=" + cvalue + ";" + expires + ";path=/";
}

function SalvaPromemoria(idrichiesta, data) {
    debugger
    $.ajax({
        url: '/MaternitaCongedi/cambiaPromemoria',
        type: "GET",
        data: { idrichiesta: idrichiesta, data: data },
        dataType: "json",
        complete: function () { },
        success: function (data) {

            swal({
                title: 'Promemoria salvato correttamente',
                type: "success",
                confirmButtonText: 'OK',
                customClass: 'rai'
            });
            GetContent();
        }
    });
}
function CambiaDataPromemoria(idspan) {
    debugger
    var idrich = $("#" + idspan).attr("data-idrichiesta");

    var dataPro = $("#datapro-" + idspan).val();
    $.ajax({
        url: '/MaternitaCongedi/cambiaPromemoria',
        type: "GET",
        data: { idrichiesta: idrich, data: dataPro },
        dataType: "json",
        complete: function () { },
        success: function (data) {
            $("#" + idspan).attr("data-promemoria", dataPro);
            $("#" + idspan).popover("hide");
            swal({
                title: 'Promemoria salvato correttamente',
                type: "success",
                confirmButtonText: 'OK',
                customClass: 'rai'
            });
        }
    });
}

////_allegatoReview
function SbloccaTasti() {
    var totale = $(".valuta-allegato").length;
    var ok = $(".ok-selected").length;
    var nok = $(".nok-selected").length;
    debugger
    if (ok + nok >= totale / 2) {
        if (nok == 0) {

            $("#approva-richiesta").removeClass("disable");
            var nota = $("#nota-risposta").val().trim();
            if (nota.length > 4) {
                $("#respingi-richiesta").removeClass("disable");
            }
            else {
                $("#respingi-richiesta").addClass("disable");
            }
        }
        else {
            $("#approva-richiesta").addClass("disable");
            var nota = $("#nota-risposta").val().trim();
            if (nota.length > 4) {
                $("#respingi-richiesta").removeClass("disable");

            }
            else {
                $("#respingi-richiesta").addClass("disable");

            }

        }
    }
}

function attivaContatore(mesi, giorni, mesiconiuge, giorniconiuge, MF) {


    var mesiSpettanti = 6;
    if (MF == "M") mesiSpettanti = 7;

    var mesiPersi = 0;
    var giorniPersi = 0;
    if (mesiconiuge >= 4) {
        mesiPersi = mesiconiuge - 4;
        giorniPersi = giorniconiuge;
    }

    if ($("#congedi").html().trim() == "") {

        var valori = {
            sesso: MF,
            mesifruiti: mesi,
            giornifruiti: giorni,
            mesipratica: 0,
            giornipratica: 0,
            giorniconiuge: giorniconiuge,
            mesi: 6,
            mesibonuspadre: 1,
            mesiperbonuspadre: 3,
            mesiindennizati: 6,
            debug: false,
            alert: 10,
            onComplete: function () {

            }
        }
        console.log(JSON.stringify(valori));
        var instance =
            $('#congedi').contatore(valori);
    }
    else {
        $('#congedi').data('plugin_contatore').cambiaValore({ mesifruiti: mesi, giornifruiti: giorni, giorniconiuge: giorniconiuge });
    }
}
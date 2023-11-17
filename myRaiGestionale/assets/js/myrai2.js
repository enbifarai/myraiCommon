//function fromJsonDataStringtoJs(dateString) {
//    var seconds = parseInt(dateString.split(/\/Date\(([0-9]+)[^+]\//, "$1"));
//    var date = new Date(seconds);
//    console.log(date);
//    return date.toLocaleDateString();
//}

function currentDateFormatted() {
    var today = new Date();
    var dd = today.getDate();

    var mm = today.getMonth() + 1;
    var yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }

    if (mm < 10) {
        mm = '0' + mm;
    }
    today = dd + '/' + mm + '/' + yyyy;
    return today;
}


$('#pdf-modal').on('shown.bs.modal', function (event) {

    resizeIframe();
    CheckInCarrello();
});

$('#pdf-modalDoc').on('shown.bs.modal', function (event) {
    resizeIframe();
});

function resizeIframe() {
    $("#myframe").css("height", window.innerHeight - $("#pdfok").height() - (2 * (document.getElementById("myframe").getBoundingClientRect().top)).toString() + "px");

}

function showAlberoExt(idsuba, tema) {
    $('#organigrammaIndex').addClass('rai-loader');
    var color = "#039BE5";
    if (document.getElementsByClassName("rai-bg-primary").length) {
        var style = window.getComputedStyle(document.getElementsByClassName("rai-bg-primary")[0], null);
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

    $.ajax({
        url: '/AlberoStruttura/ShowAlberoByModal',
        type: 'POST',
        data: { idRoot: idsuba, viewEmployee:'true' },
        dataType: "json",
        success: function (data) {
            var timer = setTimeout(function () {
                OrgChart.templates.ula.field_0 = '<text width="230" text-overflow="multiline" class="field_0"  text-anchor="left" style="font-size: 14px;" fill="' + color + '" x="10" y="98"><tspan >{val}</tspan></text>';
                OrgChart.templates.ula.field_1 = '<text width="230" class="field_1" style="font-weight:bold; font-size: 14px;" text-overflow="multiline"  text-anchor="middle" fill="' + color + '" x = "125" y = "48" ><tspan x="125" y="48" >{val}</tspan></text>';
                //OrgChart.templates.ula.field_2 = '<image preserveAspectRatio="xMidYMid slice" xlink:href="/assets/img/staff-' + tema + '.png" x ="6" y ="98" width="14" height="14"></image><text x ="25" y ="110" style="font-weight:bold; font-size: 14px;"  fill="' + color + '" >{val}</text>';
                OrgChart.templates.ula.field_3 = '<text x ="125" y ="85"  style=" font-size: 14px;"  text-anchor="middle" fill="' + color + '" text-overflow="multiline">{val}</text>';
                $('#organigrammaIndex').append(
                    '<div>' +
                    '<image preserveAspectRatio="xMidYMid slice" xlink:href="/assets/img/rai.png" width="24" heigth="24"></image>' +
                    '</div>'
                );
                $('#organigrammaIndex').removeClass('rai-loader');
               
                var chart = new OrgChart(document.getElementById("organigrammaIndex"), {
                    toolbar: {
                        layout: true,
                        zoom: true,
                        fit: true,
                        expandAll: true
                    },
                    showXScroll: OrgChart.scroll.visible,
                    mouseScrool: OrgChart.action.zoom,
                    layout: OrgChart.tree,
                    align: OrgChart.ORIENTATION,
                    enableDragDrop: false,
                    collapse: {
                        level: 2,
                        allChildren: true,
                    },
                    expand: {
                        allChildren: true
                    },
                    scaleInitial: OrgChart.match.boundary,
                    enableSearch: false,
                    template: "ula",
                    nodeBinding: {
                        field_0: "Nominativo",
                        field_1: "Direzione",
                        //field_2: "Numero_Risorse_Strut",
                        field_3: "Mansione",
                        //img_0: "img",
                    },
                    tags: {
                        "S": {
                            template: "polina"
                        },
                    },
                    nodes: data.jsonNodes

                });
                clearInterval(timer);
            }, 1000);
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        },
        complete: function () {
            $('#organigrammaIndex').html('');
            $('#organigrammaIndex').addClass('rai-loader');
        }
    });

    
}

function showAlbero(idsuba, tema) {
    console.log(idsuba);

    var color = "#039BE5";
    if (document.getElementsByClassName("rai-bg-primary").length) {
        var style = window.getComputedStyle(document.getElementsByClassName("rai-bg-primary")[0], null);
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


    $.ajax({
        url: '/AlberoStruttura/ShowAlberoByModal',
        type: 'POST',
        data: { idRoot: idsuba },
        dataType: "json",
        success: function (data) {
            $('#tree').modal('show');
            setTimeout(function () {
                console.log(data.jsonNodes)
                getOrgChartStrutturaAziendale(data);
            }, 3000);
            //OrgChart.templates.ula.img_0= '<image preserveAspectRatio="xMidYMid slice" xlink:href="{val}" x="10" y="30" width="60" height="60"></image>';
            //OrgChart.toolbarUI.expandAllIcon = '<img width="32" src=https://balkangraph.com/js/img/expand.png />';
            //OrgChart.toolbarUI.fitIcon = '<img width="32" src=https://balkangraph.com/js/img/plan.png />';
            //OrgChart.toolbarUI.zoomInIcon = '<img width="32" src=https://balkangraph.com/js/img/zoom-out.png />';
            //OrgChart.toolbarUI.zoomOutIcon = '<img width="32" src=https://balkangraph.com/js/img/zoom-in.png />';
            //OrgChart.toolbarUI.layoutIcon = '<img width="32" src=https://balkangraph.com/js/img/layout.png />';
            //$('#organigramma').html(data.jsonNodes);
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        },
        complete: function () {
            $('#organigramma').html('');
            $('#organigramma').addClass('rai-loader');
        }
    });

    function getOrgChartStrutturaAziendale(data) {
        OrgChart.templates.ula.field_0 = '<text width="230" text-overflow="multiline" class="field_0"  text-anchor="middle" style="font-size: 14px;" fill="' + color + '" x="125" y="80"><tspan x="125" y="90" >{val}</tspan></text>';
        OrgChart.templates.ula.field_1 = '<text width="230" class="field_1" style="font-weight:bold; font-size: 14px;" text-overflow="multiline"  text-anchor="middle" fill="' + color + '" x = "125" y = "38" ><tspan x="125" y="48" >{val}</tspan></text>';
        OrgChart.templates.ula.field_2 = '<image preserveAspectRatio="xMidYMid slice" xlink:href="/assets/img/staff-' + tema + '.png" x ="6" y ="98" width="14" height="14"></image><text x ="25" y ="110" style="font-weight:bold; font-size: 14px;"  fill="' + color + '" >{val}</text>';
        OrgChart.templates.ula.field_3 = '<text x ="125" y ="100"  style=" font-size: 14px;"  text-anchor="middle" fill="' + color + '" text-overflow="multiline">{val}</text>';
        $('#organigramma').append(
            '<div>' +
            '<image preserveAspectRatio="xMidYMid slice" xlink:href="/assets/img/rai.png" width="24" heigth="24"></image>' +
            '</div>'
        );
        $('#organigramma').removeClass('rai-loader');
        var chart = new OrgChart(document.getElementById("organigramma"), {
            toolbar: {
                layout: true,
                zoom: true,
                fit: true,
                expandAll: true
            },
            //menu: {
            //    pdf: { text: "Export PDF", onClick: pdf },
            //    png: { text: "Export PNG" },
            //    svg: { text: "Export SVG" },
            //    csv: { text: "Export CSV" },

            //},
            //nodeMenu: {
            //    details: { text: "Details" },
            //    edit: { text: "Edit" },
            //    add: { text: "Add" },
            //    remove: { text: "Remove" }
            //},
            nodeContextMenu: {
                edit: { text: "Edit", icon: OrgChart.icon.edit(18, 18, '#039BE5') },
                add: { text: "Add", icon: OrgChart.icon.add(18, 18, '#FF8304') }
            },
            dragDropMenu: {
                addInGroup: { text: "Add in group" },
                addAsChild: { text: "Add as child" }
            },
            showXScroll: OrgChart.scroll.visible,
            mouseScrool: OrgChart.action.zoom,
            layout: OrgChart.tree,
            align: OrgChart.ORIENTATION,
            enableDragDrop: true,
            collapse: {
                level: 2,
                allChildren: true,
            },
            expand: {
                allChildren: true
            },
            scaleInitial: OrgChart.match.boundary,
            enableSearch: false,
            template: "ula",
            nodeBinding: {
                field_0: "Nominativo",
                field_1: "Direzione",
                field_2: "Numero_Risorse_Strut",
                field_3: "Mansione",
                //img_0: "img",
            },
            tags: {
                "S": {
                    template: "polina"
                },
            },
            nodes: data.jsonNodes

        });

        chart.on('update', function (sender, oldNode, newNode) {
            console.log(oldNode);
            console.log(newNode);
            $.post(("/AlberoStruttura/UpdateStrutturaGrafo"), newNode).done(function () {
                swal("Modifiche eseguite con successo", "", "success");
                sender.updateNode(newNode);
            }).fail(function () {
                swal("Non è possibile spostare questo elemento", "", "error");
                return false;
            });
        });
        //function pdf(nodeId) {
        //    chart.exportPDF({
        //        filename: "Rai-Struttura_Aziendale.pdf",
        //        expandChildren: true,
        //        nodeId: nodeId,
        //        footer: ' Data ' + currentDateFormatted(),
        //        header: + 'Struttura Aziendale',

        //    });
        //}
    }



    function CheckInCarrello() {
        $.ajax({
            type: 'GET',
            url: "/firma/isincarrello",
            dataType: "json",
            data: { idPdf: $("#idpdfcurrent").val() },
            cache: false,
            async: false,
            success: function (data) {
                if (data.result == true) {
                    $("#pdfcarrello").hide();
                    $("#pdfremove").show();
                }
                else {
                    $("#pdfcarrello").show();
                    $("#pdfremove").hide();
                    CheckPdfConsecutivo();
                }
            },
            error: function (a) {
                a = "a";
            },
            complete: function () {

            }
        });
    }

    function PdfNext() {
        var idPdf = $("#idpdfcurrent").val();
        var ind = getIndexModelByIdPdf(idPdf);

        PdfMessaggioCambioSedeNext(ind + 1);

        if (ind == jsmodel.length - 1) {
            $("#pdfnext").attr("disabled", "disabled");
            var tot = $("#totdoc").text();
            if (tot != "" && tot != "0") {
                FirmaDoc();
            }
            return;
        }
        $("#pdfprev").removeAttr("disabled");

        $("#idpdfcurrent").val(jsmodel[ind + 1].idPdf);

        CheckInCarrello();
        UpdateIframeSource();

        if ($("#pdfcarrello").is(':visible')) CheckPdfConsecutivo(); else hidepdfbars();
    }

    function PdfMessaggioCambioSedePrev(idx) {
        if (idx < 0)
            return false;

        if (idx >= 0) {
            var codiceSedePrecedente = jsmodel[idx + 1].codSede;
            var sede = jsmodel[idx].codSede;

            // cambio sede
            if (codiceSedePrecedente != sede) {
                $("#int1").css('background-color', '#FDE6EA');
                var tempDiv = $('<div class="row pdfbar" id="tempMessage" style=""><i style="color:#ec042a;font-size: 130%;" class="icons icon-exclamation"></i>Stai cambiando la sede di riferimento per i prossimi documenti</div>');
                $(tempDiv).css('background-color', '#FDE6EA');
                $('#pdfok').before($(tempDiv));

                setTimeout(function () {
                    if ($(tempDiv).length > 0) {
                        $(tempDiv).remove();
                    }
                }, 3000)

            }
            else {
                $("#int1").css('background-color', '#fafafa');
            }
        }
    }

    function PdfMessaggioCambioSedeNext(idx) {
        if (idx > jsmodel.length - 1)
            return false;

        if (idx > 0) {
            var codiceSedePrecedente = jsmodel[idx - 1].codSede;
            var sede = jsmodel[idx].codSede;

            // cambio sede
            if (codiceSedePrecedente != sede) {
                $("#int1").css('background-color', '#FDE6EA');
                var tempDiv = $('<div class="row pdfbar" id="tempMessage" style=""><i style="color:#ec042a;font-size: 130%;" class="icons icon-exclamation"></i>Stai cambiando la sede di riferimento per i prossimi documenti</div>');
                $(tempDiv).css('background-color', '#FDE6EA');
                $('#pdfok').before($(tempDiv));

                setTimeout(function () {
                    if ($(tempDiv).length > 0) {
                        $(tempDiv).remove();
                    }
                }, 3000)

            }
            else {
                $("#int1").css('background-color', '#fafafa');
            }
        }
    }

    function VaiAPdf(idPdf) {
        $("#idpdfcurrent").val(idPdf);
        CheckInCarrello();
        UpdateIframeSource();
    }

    function PdfPrev() {
        var idPdf = $("#idpdfcurrent").val();
        var ind = getIndexModelByIdPdf(idPdf);

        PdfMessaggioCambioSedePrev(ind - 1);

        if (ind == 0) {
            $("#pdfprev").attr("disabled", "disabled");
            return;
        }
        $("#pdfnext").removeAttr("disabled");

        $("#idpdfcurrent").val(jsmodel[ind - 1].idPdf);

        CheckInCarrello();
        UpdateIframeSource();

        if ($("#pdfcarrello").is(':visible')) CheckPdfConsecutivo(); else hidepdfbars();
    }

    function getIndexModelByIdPdf(idPdf) {
        for (var i = 0; i < jsmodel.length; i++) {
            if (jsmodel[i].idPdf == idPdf) return i;
        }
        return 0;
    }

    $('#pdf-modal').on('shown.bs.modal', function () { UpdateIntestazioneIframe(); });

    function UpdateIntestazioneIframe() {
        var sede = "";
        for (var i = 0; i < jsmodel.length; i++) {
            if (jsmodel[i].idPdf == $("#idpdfcurrent").val()) {
                sede = jsmodel[i].codSede;

                if (i > 0) {
                    var codiceSedePrecedente = jsmodel[i - 1].codSede;
                    if (codiceSedePrecedente != sede) {
                        $("#int1").css('background-color', '#FDE6EA');
                    }
                    else {
                        $("#int1").css('background-color', '#fafafa');
                    }
                }

                $("#int1").html("<b>" + jsmodel[i].codSede + "</b> - " + jsmodel[i].descSede);
                $("#int2").html("dal " + moment(jsmodel[i].DateStart).format('DD/MM/YYYY') + " al " + moment(jsmodel[i].DateEnd).format('DD/MM/YYYY'));
                if (jsmodel[i].statoPDF == "C_OK") {
                    $("#pdfcarrello").text("DOCUMENTO CONVALIDATO");
                    $("#pdfcarrello").attr("disabled", "disabled");
                }
                else {
                    $("#pdfcarrello").text("METTI IN FIRMA E MOSTRAMI IL SUCCESSIVO");
                    $("#pdfcarrello").removeAttr("disabled");
                }
                break;
            }
        }

        var totsede = 0;
        var currentDoc = 0;
        for (var i = 0; i < jsmodel.length; i++) {
            if (jsmodel[i].codSede == sede) {
                totsede++;
                if (jsmodel[i].idPdf == $("#idpdfcurrent").val()) currentDoc = totsede;
            }
        }
        $("#int3").html("<b>" + currentDoc + "/" + totsede + "</b>");
    }

    function UpdateIframeSource() {
        $("#myframe").attr("src", "/FIRMA/getpdfbinary?idpdf=" + $("#idpdfcurrent").val());
        UpdateIntestazioneIframe();
    }

    function AggiornaButtonFirmaDoc(items) {
        if (items <= 0) {
            $("#button-firmadoc").attr("disabled", "disabled");
            $("#button-firmadoc").text("NESSUN DOCUMENTO")
        }
        else $("#button-firmadoc").removeAttr("disabled");

        if (items == 1) {
            $("#button-firmadoc").text("FIRMA 1 DOCUMENTO");
            $("#button-firmadoc").removeAttr("disabled");
        }
        if (items > 1) {
            $("#button-firmadoc").text("FIRMA " + items + " DOCUMENTI");
            $("#button-firmadoc").removeAttr("disabled");
        }
    }

    function AggiungiCarrello() {
        var idPdf = $("#idpdfcurrent").val();


        $.ajax({
            type: 'GET',
            url: "/firma/checkconsecutivo",
            dataType: "json",
            data: { idPdf: idPdf },
            cache: false,
            success: function (data) {
                if (data.result == false) {
                    swal('Attenzione', 'Il pdf non rispetta la consecutività delle convalide, inserire nel carrello uno o più periodi antecedenti a quello selezionato', 'error');
                }
                else {
                    $.ajax({
                        type: 'GET',
                        url: "/firma/aggiungicarrello",
                        dataType: "json",
                        data: { idPdf: idPdf },
                        cache: false,
                        success: function (data) {

                            if (data.result == "OK") {
                                $("#pdfcount").text("(" + data.items + ")");
                                if (data.items == 0)
                                    $("#pdf-firma").attr("disabled", "disabled");
                                else
                                    $("#pdf-firma").removeAttr("disabled");
                                $("#totdoc").text(data.items);
                                AggiornaButtonFirmaDoc(data.items);
                                $("#pdfcarrello").hide();
                                $("#pdfremove").show();
                                RefreshCarrello();
                                var idPdf = $("#idpdfcurrent").val();
                                var riga = $("tr");
                                var bo = riga.filter("[data-idpdf='" + idPdf + "']");
                                bo.addClass("infirma");
                                PdfNext();
                            }
                        },
                        error: function (a) {
                        },
                        complete: function () {
                        }
                    });
                }
            },
            error: function (a) {

            },
            complete: function () {

            }
        });






    }
    function CancellaDaCarrello() {
        var idPdf = $("#idpdfcurrent").val();

        $.ajax({
            type: 'GET',
            url: "/firma/checkremovibile",
            dataType: "json",
            data: { idPdf: idPdf },
            cache: false,
            success: function (data) {
                if (data.result == "OK") {
                    $.ajax({
                        type: 'GET',
                        url: "/firma/removedacarrello",
                        dataType: "json",
                        data: { idPdf: idPdf },
                        cache: false,
                        success: function (data) {
                            if (data.result == "OK") {
                                $("#pdfcount").text("(" + data.items + ")");
                                if (data.items == 0)
                                    $("#pdf-firma").attr("disabled", "disabled");
                                else
                                    $("#pdf-firma").removeAttr("disabled");
                                $("#totdoc").text(data.items);
                                AggiornaButtonFirmaDoc(data.items);

                                $("#pdfcarrello").show();

                                $("#pdfremove").hide();
                                RefreshCarrello();
                                var idPdf = $("#idpdfcurrent").val();
                                var riga = $("tr");
                                var bo = riga.filter("[data-idpdf='" + idPdf + "']");
                                bo.removeClass("infirma");
                                CheckPdfConsecutivo();
                            }
                        },
                        error: function (a) {

                        },
                        complete: function () {

                        }
                    });
                }
                else {

                    swal('Attenzione', 'Non è possibile rimuovere il documento dal carrello perchè verrebbero generate ' +
                        'discontinuità nelle convalide, rimuovere prima il documento con date ' + data.result,
                        'error');
                }
            },
            error: function (a) {
            },
            complete: function () {
            }
        });
    }
    function FirmaDoc() {
        if ($("#totdoc").text() == "0") return;
        $("#pdf-modal").modal("hide");
        $("#firma-panel").modal("show");
        $("#inputPIN").val("");
        $("#quantifirmati").val("");
        $("#inputPassword").val("");
        $("#waitfirma").hide();
        $("#errorfirma").hide();
        $("#okfirma").hide();
        $("#conf-btn").attr("disabled", "disabled");
        $("#close-btn").removeAttr("disabled");

        $("#inputPasswordLastAuthFail").val("");
        $("#inputPINLastAuthFail").val("");
    }

    function FirmaDocEsegui() {
        var CurrentPIN = $("#inputPIN").val();
        var CurrentPassword = $("#inputPassword").val();
        var LastPIN = $("#inputPINLastAuthFail").val();
        var LastPassword = $("#inputPasswordLastAuthFail").val();

        if (CurrentPIN == LastPIN && CurrentPassword == LastPassword && CurrentPIN != "" && CurrentPassword != "") {
            swal('Attenzione', 'Per i valori di PIN/Password inviati risulta già una precedente autenticazione fallita, si prega di controllare per evitare un blocco dell\'account dovuto ad eccessivi errori.', 'error');
            return;
        }

        $("#quantifirmati").val("");
        $("#waitfirma").show();
        $("#errorfirma").hide();
        $("#okfirma").hide();
        $("#conf-btn").attr("disabled", "disabled");
        $("#close-btn").attr("disabled", "disabled");

        var p = $.trim($("#inputPIN").val());
        var w = $.trim($("#inputPassword").val());

        if ($("#rememberme").prop("checked") == false) {
            cancelCookie();
        }

        $.ajax({
            type: 'POST',
            url: "/firma/firma",
            dataType: "json",
            data: { pin: p, pwd: w },
            cache: false,
            success: function (data) {
                $('#quantifirmati').val(data.firmati);

                $("#totdoc").text(data.incarrello);
                AggiornaButtonFirmaDoc(data.incarrello);
                $("#conf-btn").removeAttr("disabled");
                $("#close-btn").removeAttr("disabled");
                $("#waitfirma").hide();
                if (data.result == "OK") {

                    if ($("#rememberme").prop("checked") == true) {
                        setCookie(w);
                    }

                    $("#okfirma").show();
                    $("#conf-btn").attr("disabled", "disabled");

                }
                else {
                    $("#errorfirma").show();
                    $("#errorspan").text(data.erroridocs);
                    if (data.isautherror) {
                        $("#inputPasswordLastAuthFail").val($("#inputPassword").val());
                        $("#inputPINLastAuthFail").val($("#inputPIN").val());
                    }
                }
            },
            error: function (a, b, c) {
                $("#waitfirma").hide();
                $("#errorfirma").show();
                $("#errorspan").text(a + " " + b + " " + c);
                $("#conf-btn").removeAttr("disabled");
                $("#close-btn").removeAttr("disabled");
            },
            complete: function () {

            }
        });
    }

    function PinChanged() {
        var p = $.trim($("#inputPIN").val());
        var w = $.trim($("#inputPassword").val());
        if (p != "" && w != "")
            $("#conf-btn").removeAttr("disabled");
        else
            $("#conf-btn").attr("disabled", "disabled");

    }
    function PasswordChanged() {
        var p = $.trim($("#inputPIN").val());
        var w = $.trim($("#inputPassword").val());
        if (p != "" && w != "")
            $("#conf-btn").removeAttr("disabled");
        else
            $("#conf-btn").attr("disabled", "disabled");
    }


    function PopupFirmaClose() {
        $("#firma-panel").modal("hide");
        if ($("#quantifirmati").val() != "" && $("#quantifirmati").val() != "0") location.reload();
    }


    function RefreshCarrello() {
        $.ajax({
            type: 'GET',
            url: "/firma/getcarrello",
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $("#tot-car").replaceWith(data);
            },
            error: function (a) {
            },
            complete: function () {
            }
        });

    }

    function RefreshIndex(data1, data2, sede, stato) {
        $("#my-block").addClass("block-opt-refresh");
        var url = "/firma/getdaesaminare?data1=" + data1 + "&data2=" + data2 + "&sede=" + sede + "&stato=" + stato;
        $.ajax({
            type: 'GET',
            url: url,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) { $("#daapprovare").replaceWith(data); },
            error: function (a) { },
            complete: function () { $("#my-block").removeClass("block-opt-refresh"); }
        });

        $("#my-block-pf").addClass("block-opt-refresh");
        url = "/firma/getdaesaminarePF?data1=" + data1 + "&data2=" + data2 + "&sede=" + sede + "&stato=" + stato;
        $.ajax({
            type: 'GET',
            url: url,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) { $("#dafirmarePF").replaceWith(data); },
            error: function (a) { },
            complete: function () { $("#my-block-pf").removeClass("block-opt-refresh"); }
        });
    }

    function RicercaPdf() {
        if ($("#btnRicerca").attr("disabled") == "disabled") return;

        var data1 = $("#data1ric").val();
        var data2 = $("#data2ric").val();
        var sede = $("#sede option:selected").val();
        var stato = $("#stato option:selected").val();
        RefreshIndex(data1, data2, sede, stato);
    }

    jQuery("#data1ric").on("dp.change", function (e) {
        CheckPuls();
    });

    jQuery("#data2ric").on("dp.change", function (e) {
        CheckPuls();
    });

    function CheckPuls() {
        var data1 = $("#data1ric").val();
        var data2 = $("#data2ric").val();
        var sede = $("#sede option:selected").val();
        if ($.trim(data1) != "" || $.trim(data2) != "" || $.trim(sede) != "")
            $("#btnRicerca").removeAttr("disabled");
        else
            $("#btnRicerca").removeAttr("disabled");
        //$("#btnRicerca").attr("disabled", "disabled");
    }

    function ResetRicercaPdf() {
        $("#data1ric").val("");
        $("#data2ric").val("");
        $("#sede").val("");
        $("#stato").val("S");
        $("#btnRicerca").click();
        // $("#btnRicerca").attr("disabled", "disabled");
    }
    function AjaxNotifications(type, mini) {
        if (typeof mini == "undefined") {
            mini = false;
        }

        var name = (mini ? "_mini" : "");


        if (type == 2) //notifiche
        {
            if ($('#menu-notifiche' + name).hasClass('open') == false) {
                $.ajax({
                    type: 'GET',
                    url: "/scrivania/getnotifiche?tipo=2",
                    dataType: "html",
                    data: {},
                    cache: false,
                    success: function (data) {
                        $("#div-notifiche" + name).html(data);
                        var t = $("#tota-2").text();
                        if (t > 0) $("#badge-not" + name).show(); else $("#badge-not" + name).hide();

                        // if (t > 99) t = "99+";
                        $("#badge-not" + name).text(t);

                    },
                    error: function (a) { }
                });
            }
        }
        if (type == 1) //notifiche
        {
            if ($('#menu-notifiche1' + name).hasClass('open') == false) {
                $.ajax({
                    type: 'GET',
                    url: "/scrivania/getnotifiche?tipo=1",
                    dataType: "html",
                    data: {},
                    cache: false,
                    success: function (data) {
                        $("#div-notifiche-1" + name).html(data);
                        var t = $("#tota-1").text();
                        if (t > 0) $("#badge-not1" + name).show(); else $("#badge-not1" + name).hide();

                        // if (t > 99) t = "99+";
                        $("#badge-not1" + name).text(t);

                    },
                    error: function (a) { }
                });
            }
        }
    }

    function CercaRichieste() {
        var idstato = $("#stato option:selected").val();
        var ecc = $("#eccezione option:selected").val();
        var datada = $("#datada").val();
        var dataal = $("#dataal").val();

        var url = "/home/refreshmierichieste?idstato=" + idstato + "&ecc=" + ecc + "&dal=" + datada + "&al=" + dataal + "&search=1";

        $("#mrcs").addClass("block-opt-refresh");

        $.ajax({
            type: 'GET',
            url: url,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                var d = $(data).find("#btabswo-static-home");
                $("#btabswo-static-home").html($(d).html());

                d = $(data).find("#btab4");
                $("#btab4").html($(d).html());

                d = $(data).find("#tab1text");
                $("#tab1text").html($(d).html());

                d = $(data).find("#tab2text");
                $("#tab2text").html($(d).html());

                $("#mrcs").removeClass("block-opt-refresh");
            },
            error: function (a) { }
        });
    }

    function setCookie(cvalue) {
        var cname = "myrai.ess";
        var d = new Date();
        d.setTime(d.getTime() + (60 * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + en(cvalue) + ";" + expires + ";path=/";
    }

    function getCookie() {
        var cname = "myrai.ess";
        var name = cname + "=";
        var con = document.cookie;
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) == 0) {
                return de(c.substring(name.length, c.length));
            }
        }
        return "";
    }
    function cancelCookie() {
        var cname = "myrai.ess";
        document.cookie = cname + '=; Max-Age=0'
    }
    function en(p) {
        var encrypted = CryptoJS.AES.encrypt(p, "SmM3hkE9sCG9Hpu2taPnyzLF");
        return encrypted.toString();
    }
    function de(p) {
        var decrypted = CryptoJS.AES.decrypt(p.toString(), "SmM3hkE9sCG9Hpu2taPnyzLF");
        return decrypted.toString(CryptoJS.enc.Utf8);
    }

    //var encrypted = CryptoJS.AES.encrypt("Message", "Secret Passphrase");

    //var decrypted = CryptoJS.AES.decrypt(encrypted.toString(), "Secret Passphrase");

    //decrypted.toString(CryptoJS.enc.Utf8)

    function showpdfok() {
        $("#pdfok").show();
        $("#pdfnok").hide();
        $("#pdfcarrello").removeClass("bg-puls_dash").removeAttr("disabled");
    }
    function showpdfnok() {
        $("#pdfok").hide();
        $("#pdfnok").show();
        $("#pdfcarrello").addClass("bg-puls_dash").attr("disabled", "disabled");
    }
    function hidepdfbars() {
        $("#pdfnok").hide();
        $("#pdfok").hide();
    }
    function reduceIframe() {
        var h = $("#pdfok").height();
        var h_iframe = $("#myframe").height();
        $("#myframe").height(h_iframe - h);
    }


    function CheckPdfConsecutivo() {
        var idPdf = $("#idpdfcurrent").val();


        $.ajax({
            type: 'GET',
            url: "/firma/checkconsecutivo",
            dataType: "json",
            data: { idPdf: idPdf },
            cache: false,
            success: function (data) {
                if (data.result == false) showpdfnok();
                else showpdfok();
            },
            error: function (a) {

            },

        });
    }

    function DettSettimanaleScorri(avanti) {

        var d1 = $("#detsetwidget").attr("data-from");
        var d2 = $("#detsetwidget").attr("data-to");

        var url = "/home/RefreshDettSettWidget";
        if (avanti != 't') {
            url = url + "?currentFrom=" + d1 + "&currentTo=" + d2 + "&dir=" + avanti;
        }

        $("#det-set-range").hide();
        $("#det-set-wait").show();
        $.ajax({
            type: 'GET',
            //url: "/home/RefreshDettSettWidget?currentFrom="+d1+"&currentTo="+d2+"&dir="+ (avanti==true?"a":"i") ,
            //url: "/home/RefreshDettSettWidget?currentFrom=" + d1 + "&currentTo=" + d2 + "&dir=" + avanti,
            url: url,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $("#detsetwidget").replaceWith(data);
                $("#det-set-range").show();
                $("#det-set-wait").hide();
            },
            error: function (a) {
                $("#det-set-range").show();
                $("#det-set-wait").hide();
            }
        });
    }
    function ShowCal(mese, anno) {
        $("#cal-mese").hide();
        $("#cal-wait").show();

        $.ajax({
            type: 'GET',
            url: "/feriepermessi/getcalendario?meserichiesto=" + mese + "&annorichiesto=" + anno,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {

                $("#div-cal").replaceWith(data);


            }
        });
    }
    function ShowCalNoFerie(mese, anno) {
        $("#cal-mese").hide();
        $("#cal-wait").show();

        $.ajax({
            type: 'GET',
            url: "/feriepermessi/getcalendario?meserichiesto=" + mese + "&annorichiesto=" + anno + "&fromscrivania=true",
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {

                $("#div-cal").replaceWith(data);


            }
        });
    }
    function GeneraPdfPresenze(sede, datada, dataa, button, spantotale) {
        $("#" + button).addClass("disable");
        $.ajax({
            type: 'GET',
            url: "/resoconti/generapresenze?sede=" + sede + "&da=" + datada + "&a=" + dataa,
            dataType: "json",
            data: {},
            cache: false,
            success: function (data) {
                if (data.result == true) {
                    $("#" + button).addClass("disable");
                    QuantiPdfDaGenerare(sede, spantotale);
                    swal('OK', 'Documento generato e predisposto per la firma', 'info');
                }
                else {
                    $("#" + button).removeClass("disable");
                    swal('Attenzione', data.errore, 'error');
                }
            },
            error: function () { $("#" + button).removeClass("disable"); }
        });
    }
    function QuantiPdfDaGenerare(sede, spantotale) {
        var h = $("#" + spantotale).html();

        $("#" + spantotale).html('<i class="fa fa-spinner fa-spin"></i> Attendi...');

        $.ajax({
            type: 'GET',
            url: "/resoconti/getPdfMissing?sede=" + sede,
            dataType: "json",
            data: {},
            cache: false,
            success: function (data) {
                if (data.result == 0) {
                    $("#" + spantotale).html(h);
                    $("#" + spantotale).text("");
                }
                else {
                    $("#" + spantotale).html(h);
                    $("#" + spantotale).text(data.result + " da approvare");
                }
            }

        });
    }

    function ResSettimanaleScorri(avanti, tbody, datada, dataa, spanwait) {
        $("#" + spanwait).html('<i class="fa fa-spinner fa-spin"></i> Attendi...');

        $.ajax({
            type: 'GET',
            url: "/resoconti/getResocontiParziale?da=" + datada + "&a=" + dataa + "&avanti=" + avanti,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $("#" + tbody).replaceWith($(data).find("#" + tbody));

                $('[data-resoconti].active').click();
            }
        });
    }
    function changeResocontiView(button) {
        event.preventDefault();

        $(button).parent().find('a.btn-action-icon').removeClass('active');
        $(button).addClass('active');


        $('.rai-resoconti').removeClass('rai-resoconti-visible');
        $('.rai-resoconti-' + $(button).data('resoconti')).addClass('rai-resoconti-visible');
    }

    function TaskEsegui(url, div) {
        swal({
            title: 'Confermi task?',
            type: 'success',
            html:
            ' ',
            showCloseButton: true,
            showCancelButton: true,
            cancelButtonText: 'No',
            confirmButtonText: 'Si'
        }).then(function () {

            $("#" + div).html('<i class="fa fa-spinner fa-spin"></i>Attendi...');

            $.ajax({
                type: 'GET',
                url: url,
                dataType: "text",
                data: {},
                cache: false,
                success: function (data) {
                    $("#" + div).html(data);
                }
            });
        });
    }

    function AggiornaGiornoDisplay() {
        $(".div-data").show();
        $(".giorno-display").text($("#data_da").val());
        if (GiornoDisplayPuoScorrereAvanti()) {
            $(".dayafter").show();
        }
        else {
            $(".dayafter").hide();
        }
        if (GiornoDisplayPuoScorrereIndietro()) {
            $(".daybefore").show();
        }
        else {
            $(".daybefore").hide();
        }
    }

    function GiornoDisplayPuoScorrereAvanti() {
        var startdate = $(".giorno-display").text();
        var new_date = moment(startdate, "DD/MM/YYYY");
        var thing = new_date.add(1, 'days').format('DD/MM/YYYY');

        var b = IsDateGreaterThanToday(thing);
        return (AssenzeIngiustificateNow == 0 && !b);
    }

    function GiornoDisplayPuoScorrereIndietro() {
        return (AssenzeIngiustificateNow == 0);
    }

    function ScorriBack() {
        var startdate = $(".giorno-display").text();
        var new_date = moment(startdate, "DD/MM/YYYY");
        var thing = new_date.add(-1, 'days').format('DD/MM/YYYY');
        $("#data_da").val(thing);
        $("#data_a").val(thing);
        $(".giorno-display").text(thing);
        $("#data_da").change();
    }

    function ScorriAhead() {
        var startdate = $(".giorno-display").text();
        var new_date = moment(startdate, "DD/MM/YYYY");
        var thing = new_date.add(1, 'days').format('DD/MM/YYYY');
        $("#data_da").val(thing);
        $("#data_a").val(thing);
        $(".giorno-display").text(thing);
        $("#data_da").change();
    }

    //Busta paga
    function PdfDocNext() {

        var idPdf = $("#idpdfcurrent").val();
        var ind = getIndexModelLByIdDocumento(idPdf);

        $("#pdfnext").removeAttr("disabled");

        if (parseInt(ind) == 1) {
            $("#pdfprev").attr("disabled", "disabled");

        }



        $("#idpdfcurrent").val(jsmodell[parseInt(ind) - 1].ID);
        UpdateIframeDocSource(parseInt(ind) - 1, $("#tipodoc").val());
    }

    function PdfDocPrev() {

        var idPdf = $("#idpdfcurrent").val();
        var ind = getIndexModelLByIdDocumento(idPdf);
        var anno = ind[1];
        var mese = ind[3];

        $("#pdfprev").removeAttr("disabled");

        if (parseInt(ind) == (jsmodell.length - 2)) {
            $("#pdfnext").attr("disabled", "disabled");

        }


        $("#idpdfcurrent").val(jsmodell[parseInt(ind) + 1].ID);

        UpdateIframeDocSource(parseInt(ind) + 1, $("#tipodoc").val());
    }

    function retrievenext(idpdf, prev) {
        var ind = getIndexModelByIdDocumento(idPdf).split('|');

        var annoact = ind[1];
        var meseact = ind[3];
        var annotot = ind[0];
        var mesetot = ind[2];

        var mesesuc = 0;

        if (prev) {
            if ((meseact + 1) < mesetot) {
                mesesuc = meseact + 1;
            }

            if ((meseact + 1) == mesetot) {
                $("#pdfprev").attr("disabled", "disabled");
            }

            if ((meseact + 1) == mesetot) {
                $("#pdfprev").attr("disabled", "disabled");
            }
        }
        else {
        }
    }

    function getIndexModelByIdDocumento(IdDocumento) {
        console.log(jsmodel);
        for (var x = 0; x < jsmodel.length; x++) {
            for (var i = 0; i < jsmodel[x].length; i++) {

                if (jsmodel[x][i].ID == IdDocumento) return jsmodel.length + '|' + x.toString() + '|' + jsmodel[x].length + '|' + i.toString();
            }
        }
        return '0|0|0|0';
    }

    function getIndexModelLByIdDocumento(IdDocumento) {
        console.log(jsmodel);
        for (var x = 0; x < jsmodell.length; x++) {

            if (jsmodell[x].ID == IdDocumento) return x.toString();

        }
        return '0';
    }


    function UpdateIframeDocSource(ind, tipodoc) {

        if (tipodoc == "00") {
            $("#myframe").attr("src", "/BUSTAPAGA/getpdfbinary?idDocumento=" + $("#idpdfcurrent").val() + "&nomefile=" + jsmodell[ind].DataContabile.substring(0, 6));
            UpdateIntestazioneDocIframe(ind, tipodoc);

        }
        else {
            $("#myframe").attr("src", "/documentiamministrativi/getpdfbinary?idDocumento=" + $("#idpdfcurrent").val() + "&nomefile=" + jsmodell[ind].DataContabile.substring(0, 6));
            UpdateIntestazioneDocIframe(ind, tipodoc);
        }
    }

    function UpdateIntestazioneDocIframe(ind, tipodoc) {
        $("#attesa").show();
        moment.locale('IT');
        if (tipodoc == "00") {
            $("#int2").html(moment(jsmodell[ind].DataContabile.substring(0, 6) + '01').format('MMMM YYYY'));
            $("#dataCompetenza").html("Data Competenza: <b>" + moment(jsmodell[ind].DataCompetenza.substring(0, 6) + '01').format('MMMM YYYY') + "</b>");
            $("#dataPubblicazione").html("Data Pubblicazione: <b>" + moment(jsmodell[ind].DataPubblicazione).format('DD/MM/YYYY') + "</b>");
            $("#Nota").html(jsmodell[ind].Nota);
        }
        else {
            $("#int2").html(jsmodell[ind].DescrittivaTipoDoc);
            var competenza = "";
            if (jsmodell[ind].DataCompetenza.length == 4) {

                competenza = jsmodell[ind].DataCompetenza;
            }

            if (jsmodell[ind].DataCompetenza.length == 6) {
                if (jsmodell[ind].DataContabile.substring(4) == "00") {

                    competenza = jsmodell[ind].DataContabile.substring(0, 4);
                }
                else {

                    competenza = moment(jsmodell[ind].DataCompetenza.substring(0, 6) + '01').format('MMMM YYYY');
                }

            }

            if (jsmodell[ind].DataCompetenza.length == 8) {
                if (jsmodell[ind].DataContabile.substring(4) == "0000") {
                    competenza = jsmodell[ind].DataContabile.substring(0, 4);
                }
                else if (jsmodell[ind].DataContabile.substring(6) == "00") {
                    competenza = moment(jsmodell[ind].DataContabile.substring(0, 6) + '01').format('MMMM YYYY');
                }
                else {
                    competenza = moment(jsmodell[ind].DataContabile).format('DD/MM/YYYY')
                }
            }

            $("#dataCompetenza").html("Data Competenza: <b>" + competenza + "</b>");
            $("#dataPubblicazione").html("Data Pubblicazione: <b>" + moment(jsmodell[ind].DataPubblicazione).format('DD/MM/YYYY') + "</b>");
            $("#Nota").html(jsmodell[ind].Nota);
        }
    }

    function onMyFrameLoad() {
        $("#attesa").hide();
    };

    function ShowPdfBustaPaga(idPdf, datacompetenza, datacontabile, datapubblicazione, nota, nomeController, titolo, nomefile) {
        $.ajax({
            beforeSend: function () { $("#page-loaderR").show(); },
            complete: function () { $("#page-loaderR").hide(); },
            url: '/' + nomeController + '/getpdf',
            type: "GET",
            dataType: "html",
            data: { idPdf: idPdf, datacompetenza: datacompetenza, datacontabile: datacontabile, datapubblicazione: datapubblicazione, nota: nota, titolo: titolo, nomefile: nomefile },
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
                    $("#pdf-modalDoc").modal("show");
                    $("#pdfcontent").html(data);
                    console.log(jsmodell);
                    console.log("--2-- " + idPdf + " --2-- ");
                    console.log("--3-- " + jsmodell[0].ID + " --3-- ");
                    console.log(jsmodell[jsmodell.length - 1].ID);
                    $("#idpdfcurrent").val(idPdf);

                    if (jsmodell[0].ID == idPdf) {
                        console.log(" Scritto ");
                        $("#pdfprev").attr("disabled", "disabled");
                    }
                    if (jsmodell[jsmodell.length - 1].ID == idPdf)
                        $("#pdfnext").attr("disabled", "disabled");

                }



            },
            error: function (a, b, c) {
                swal('Errore', a + b + c, 'error');
            }
        });


    }

    function vediStorico(ultimoAnno, nomeController) {
        $("#bspg").addClass("block-opt-refresh");
        $.ajax({
            type: 'GET',
            url: "/" + nomeController + "/GetBustaPaga?UltimoAnno=" + ultimoAnno,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $('#bstot').html(data);
            },
            error: function (a) {
                a = "a";
            },
            complete: function () {
                $("#bspg").removeClass("block-opt-refresh");
            }
        });
        return false;
    }




    function vediDocumenti(tipoDoc, nomeController) {
        $("#bspg").addClass("block-opt-refresh");
        $.ajax({
            type: 'GET',
            url: "/" + nomeController + "/GetDocPerTipo?tipoDoc=" + tipoDoc,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $('#bstot').html(data);
            },
            error: function (a) {
                a = "a";
            },
            complete: function () {
                $("#bspg").removeClass("block-opt-refresh");
            }
        });
        return false;
    }

    function vediDocumentiNonLetti(nomeController) {
        $("#bspg").addClass("block-opt-refresh");
        $.ajax({
            type: 'GET',
            url: "/" + nomeController + "/GetDocDaLeggere",
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {
                $('#bstot').html(data);
            },
            error: function (a) {
                a = "a";
            },
            complete: function () {
                $("#bspg").removeClass("block-opt-refresh");
            }
        });
        return false;
    }



    function StilizzaSelectCv(sezione, id_elem) {
        if (id_elem == null) {
            id_elem = "";
        }
        switch (sezione) {
            case "exp":
                $(id_elem + " #_codiceFiguraProf").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_codSocieta").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                $(id_elem + " #_codDirezione").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                $(id_elem + " #_codRedazione").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_nazione").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_procura").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_risorseGest").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_budgetGest").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_codIndustry").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                $(id_elem + " #_codLocalitaEsp.js-select2").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                $(id_elem + " #_codFigProExtra").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                var jselectTextable = $(".textable-select");

                jselectTextable.on('select2:closing', function (e) {
                    var check_item = false;
                    var id = e.target.id;
                    var resultSelect = $(".select2-results__options")[0];
                    var countElement = resultSelect.childElementCount;
                    if (countElement == 1) {
                        if (resultSelect.childNodes[0].textContent == "No results found") {
                            check_item = true;
                        }
                    }
                    var text = $(".search-field_custom .select2-search__field").val();
                    var value = $(id_elem + " #" + e.target.id).val();
                    if ((value == "") || (value == null) || (value == 'undefined') || (value == '-1') || (check_item)) {
                        $(id_elem + " #" + id + " option[value='-1']").remove();
                        var text = $(".search-field_custom .select2-search__field").val();
                        $(id_elem + " #" + id).append('<option value="-1" selected="selected">' + text + '</option>');

                        switch (id) {
                            case "_codSocieta":
                                $(id_elem + " #_societa").val(text);
                                break;
                            case "_codDirezione":
                                $(id_elem + " #_direzione").val(text);
                                break;
                            case "_codIndustry":
                                $(id_elem + " #_industry").val(text);
                                break;
                            case "_codLocalitaEsp":
                                $(id_elem + " #_localitaEsp").val(text);
                                break;
                            case "_codFigProExtra":
                                $(id_elem + " #_figProExtra").val(text);
                                break;
                        }
                    }
                    else {
                        switch (id) {
                            case "_codSocieta":
                                $(id_elem + " #_societa").val(text);
                                break;
                            case "_codDirezione":
                                $(id_elem + " #_direzione").val(text);
                                break;
                            case "_codIndustry":
                                $(id_elem + " #_industry").val(text);
                                break;
                            case "_codLocalitaEsp":
                                $(id_elem + " #_localitaEsp").val(text);
                                break;
                            case "_codFigProExtra":
                                $(id_elem + " #_figProExtra").val(text);
                                break;

                        }
                    }

                });
                break;
            case "stud":
                $(id_elem + " #_codTipoTitolo").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_codTitolo").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_codNazione").select2({ containerCssClass: "formElements" });
                $(id_elem + " #_codIstituto").select2({ containerCssClass: "formElements", dropdownCssClass: "search-field_custom" });
                var jselectTextable = $(".textable-select");

                jselectTextable.on('select2:closing', function (e) {
                    var check_item = false;
                    var id = e.target.id;
                    var resultSelect = $(".select2-results__options")[0];
                    var countElement = resultSelect.childElementCount;
                    if (countElement == 1) {
                        if (resultSelect.childNodes[0].textContent == "No results found") {
                            check_item = true;
                        }
                    }
                    var text = $(".search-field_custom .select2-search__field").val();
                    var value = $(id_elem + " #" + e.target.id).val();
                    if ((value == "") || (value == null) || (value == 'undefined') || (value == '-1') || (check_item)) {
                        $(id_elem + " #" + id + " option[value='-1']").remove();
                        var text = $(".search-field_custom .select2-search__field").val();
                        $(id_elem + " #" + id).append('<option value="-1" selected="selected">' + text + '</option>');

                        switch (id) {
                            case "_codIstituto":
                                $(id_elem + " #_istituto").val(text);
                                break;
                        }
                    }
                    else {
                        switch (id) {
                            case "_codIstituto":
                                $(id_elem + " #_istituto").val(text);
                                break;
                        }
                        0
                    }

                });
                break;
            case "contatti":
                $("#frm-InsertAltreInfo #tipoPatente").select2({ containerCssClass: "formElementsSelectMultiple" });
                break;
            default:
                $(id_elem + " .js-select2.formElements").select2({ containerCssClass: "formElements" });
                break;
        }
    }

    function testapi() {

        $.ajax({
            url: 'http://raiperme.intranet.rai.it/api/raiplace/getinfomatr?m=103650',
            type: "GET",
            dataType: "json",
            data: {},
            headers: { "keystring": "33540117672688738685" },
            success: function (data) {
                console.log(JSON.stringify(data));
            }
        });
    }

    function newEccezione(codice) {
        $("#tit-modal-ecc").text("Nuova eccezione");

        $("#idecc").val(0);

        $("#idecc-presel").val(codice);

        $.ajax({
            url: '/schedaeccezioni/eccezionecontent',
            type: "GET",
            dataType: "html",
            data: {},
            success: function (data) {
                $("#eccezione-content").html(data);
                AttivaSummernote();
                $("#modal-ecc .modal-content").scrollTop(0);
                $.validator.unobtrusive.parse($("#form-eccezioni"))
                if (codice != null) {
                    $("#CodiceEccezione").val(codice);
                    $("#CodiceEccezione").change();
                }
            }
        });


        $("#modal-ecc").modal("show");
    }
    function modificaEccezione(id) {
        $("#eccezione-content").html("");
        $("#tit-modal-ecc").text("Modifica eccezione");
        $("#idecc").val(id);
        $("#idecc-presel").val("");

        $("#modal-ecc").modal("show");
        $.ajax({
            url: '/schedaeccezioni/eccezionecontent',
            type: "GET",
            dataType: "html",
            data: { id: id },
            success: function (data) {
                $("#eccezione-content").html(data);
                AttivaSummernote();
                $("#modal-ecc .modal-content").scrollTop(0);
                $.validator.unobtrusive.parse($("#form-eccezioni"))
            }
        });
    }
    function AttivaSummernote() {
        $(".html-editor").summernote({
            height: 400,
            toolbar: toolBar
        });
    }
    function AttivaSummernoteNews() {
        $(".html-editor").summernote({
            height: 300,
            toolbar: toolBar
        });
    }
    function FitTimbrature() {
        //var h = $(window).innerHeight() - 200;
        // $("#divcont").css("max-height", h + "px");
    }

    function ShowCalCorsi(mese, anno) {
        $("#cal-mese").hide();
        $("#cal-wait").show();

        $.ajax({
            type: 'GET',
            url: "/raiacademy/getcalendario?meserichiesto=" + mese + "&annorichiesto=" + anno,
            dataType: "html",
            data: {},
            cache: false,
            success: function (data) {

                $("#div-cal").replaceWith(data);


            }
        });
    }

    function submitEccezioniDaCarenze(dataecc) {
        var vaitranquillo = true;
        $(".ep-obb").each(function () {
            if ($(this).val() == "") {
                swal('Manca un parametro obbligatorio', '', 'error');
                vaitranquillo = false;
            }
        });
        if (!vaitranquillo) return;

        var eccsubmit = [];


        var numtr = $("#part2 tr.trecc").length;
        for (var i = 0; i < numtr; i++) {

            var ob = {
                nome: $("#part2").find(".nome-ecc" + i).text(),
                durata: $("#part2").find(".durata-ecc" + i).text(),
                intv: $("#part2").find(".intervallo-ecc" + i).val(),
                nota: $("#part2").find(".nota-ecc" + i).val(),
                eccezione_collegata: $("#view-" + i).find("input[name='eccezione_collegata']").val(),
                parametro_passato: $("#view-" + i).find("input[name='parametro_passato']").val(),
                parametro_ricevuto: $("#view-" + i).find("input[name='parametro_ricevuto']").val()
            };
            if (ob.parametro_passato != null && ob.parametro_passato != undefined) {
                ob.valore_parametro = $("#view-" + i).find("[name='ep-" + ob.parametro_passato.toLowerCase() + "']").val();
            }

            eccsubmit.push(ob);

        }

        var htmlInvia = $("#button-conferma-timbra2").html();
        var htmlAttesa = '<i class="fa fa-refresh fa-spin"></i>CONFERMA';
        $("#button-conferma-timbra2").html(htmlAttesa).attr("disabled", "disabled");
        $("#button-back").attr("disabled", "disabled");

        $.ajax({
            url: '/ajax/inserimentoDaTimbr',
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify({
                model: eccsubmit,
                dataecc: dataecc
            }),
            success: function (data) {
                if (data.result == "OK") {
                    $("#button-conferma-timbra2").html(htmlInvia).removeAttr("disabled");
                    $("#button-back").removeAttr("disabled");
                    swal({
                        title: 'Richiesta inserita',
                        type: 'success',
                        html: ' ',
                        showCloseButton: true,
                        showCancelButton: false,
                        confirmButtonText:
                        ' OK'
                    }).then(function () { $("#dateok").click(); });
                }
                else {
                    $("#button-conferma-timbra2").html(htmlInvia).removeAttr("disabled");
                    $("#button-back").removeAttr("disabled");
                    swal(data.result, '', 'error')
                }
            },
            error: function (a, b, c) {
                $("#button-conferma-timbra2").html(htmlInvia).removeAttr("disabled");
                $("#button-back").removeAttr("disabled");
                swal(a + b + c, '', 'error')
            }
        });

    }
    function ConvertiMinutiToHHMM(minutes) {
        var h = Math.floor(minutes / 60);
        var m = minutes % 60;
        h = h < 10 ? '0' + h : h;
        m = m < 10 ? '0' + m : m;
        return h + ':' + m;
    }

    function AddNews() {
        $("#modal-news-editor").modal("show");
        $("#content-news").load("/news/addnews");

    }

    $(document.body).on("submit", '#form-news', function (e) {
        e.preventDefault();
        var form = $("#form-news");

        $.ajax({
            url: $(form).attr('action'),
            type: 'POST',
            data: $(form).serialize(),
            success: function (data) {
                if (data.result == "ok") {
                    swal('OK', 'Salvato con successo', 'info').then(function () { location.reload() });
                    $("#modal-news-editor").modal("hide");
                }
                else {
                    swal('Attenzione', data.result, 'error');
                }
            }
        });

    });
    function tipodestChanged() {
        var t = $("input[name='tipodest']:checked").val();
        $("#form-news input[type='text'] :not(js-datetimepicker)").attr("disabled", "disabled");
        $(".textfor-" + t).removeAttr("disabled");

    }
    $(document.body).on("change", "#form-news input[type='radio']", function (e) {
        tipodestChanged();
    });
    function editNews(event) {
        event.preventDefault();
        event.stopPropagation();

        var idnews = $(event.target).attr("data-id-news");
        $("#modal-news-editor").modal("show");
        $("#content-news").load("/news/modnews?id=" + idnews);
    }

    function delNews(id) {
        swal({
            title: 'Confermi?',
            text: "La news verrà cancellata.",
            type: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Conferma',
            cancelButtonText: 'Annulla'
        }).then(function () {
            $.ajax({
                url: "/news/Delnews",
                type: "POST",

                data: { id: id },
                cache: false,
                success: function (data) {
                    if (data.result == "ok")
                        swal('OK', 'Cancellato con successo', 'info').then(function () { location.reload() });
                    else
                        swal('Attenzione', data.result, 'error');

                },
                error: function () { }
            });
        });
    }

    function EccezioneCollegataChanged() {
        var idEcc = $("#EccezioneSelezionata").val();
        var cod = $("#EccezioneSelezionata").find("option:selected").text();

        if (!idEcc)
            $("#buttonAggiungiEcc").addClass("disable");
        else
            $("#buttonAggiungiEcc").removeClass("disable");



    }
    function IsPresentEccColl(codice) {
        var eccezioni = $("#EccezioniCollegate").val().split(',');
        for (var i = 0; i < eccezioni.length; i++) {
            if (codice.toUpperCase() == eccezioni[i].trim().toUpperCase()) return true;
        }
        if ($("#CodiceEccezione").val() == codice) return true;

        return false;
    }
    function AggiungiEccColl() {
        var cod = $("#EccezioneSelezionata").find("option:selected").text();
        if (!IsPresentEccColl(cod)) {
            if ($("#EccezioniCollegate").val().trim() == "")
                $("#EccezioniCollegate").val(cod);
            else
                $("#EccezioniCollegate").val($("#EccezioniCollegate").val().trim() + "," + cod);
        }
    }






}

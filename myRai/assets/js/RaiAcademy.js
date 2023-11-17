function ShowTab(tab) {
    $('.a_obiettivi').removeClass('panel-featured-primary');
    $('.a_dettagli').removeClass('panel-featured-primary');
    $('.a_corsi').removeClass('panel-featured-primary');
    $('.a_calendario').removeClass('panel-featured-primary');
    $('.a_risorse').removeClass('panel-featured-primary');

    $('.a_obiettivi').removeClass('text-primary');
    $('.a_dettagli').removeClass('text-primary');
    $('.a_corsi').removeClass('text-primary');
    $('.a_calendario').removeClass('text-primary');
    $('.a_risorse').removeClass('text-primary');

    var elem = $('#' + tab);
    var pos = elem.offset().top - 175;
    if ($('#btnTab').css("position") != "fixed")
        pos = pos - 40;

    $('html,body').animate({ scrollTop: pos }, '400', function () {
        $('.a_' + tab).addClass('panel-featured-primary');
        $('.a_' + tab).addClass('text-primary');
        $('#btnGroupDrop1').text($('.a_' + tab).first().text());
    });
    $('#' + tab).focus();
    //e.preventDefault();


}


function ButtonCat(ind) {
    $("a.cors").removeClass("panel-featured-primary text-primary");
    $(".button-cat-" + ind).addClass("panel-featured-primary text-primary");

    $('#btnGroupDrop1').text($(".button-cat-" + ind).first().text());

    var rifPlaceHolder = $('.button-cat-' + ind).attr('data-tipo-corsi');
    if (rifPlaceHolder == undefined || rifPlaceHolder == "")
        rifPlaceHolder = "Cerca nel catalogo";

    $('#filtri_SearchString').attr('placeholder', rifPlaceHolder);

    if (ind == 0)
        $('#filtro-corsi-container').find('[data-stato]').show();
    else {
        $('#filtro-corsi-container').find('[data-stato]').each(function () {
            var stato = $(this).attr('data-stato');
            if (stato == '' || stato.includes(ind)) {
                $(this).show();
            }
            else
                $(this).hide();
        });
    }

    $('#div_btn').attr('data-cat', ind);

    resetFiltri();
    ManageCatPage(1);
}


function resetFiltri() {
    $('#filtro-corsi-container input:checkbox').prop('checked', false);
    $('#filtri_OrderBy').val('');
    $('#filtri_SelectedTag').val('');
    $('#filtri_SelectedFilter').val('');

    if ($('#filtri_SearchString').val() != '') {
        $('#filtri_SearchString').val('');
        submitFiltri();
    }

    CheckSelectedFilter2(null, false);

    var currentState = $('.cors.panel-featured-primary').attr('id').replace('button-cat-', '');
    if (currentState > 0) {
        $('#row_pagination').show();
        $('#row_show_more').hide();
    }
    else {
        $('#row_pagination').hide();
        $('#row_show_more').show();
    }
}

function preSubmit() {
    $("#ElencoCorsiExt").addClass("block block-opt-refresh");
    $("#ElencoCorsi").addClass("academy-not-authorized");
}

function submitFiltri() {
    $('#filtro-corsi-container input:checkbox').prop('checked', false);
    preSubmit();
    $("#form-filtri").submit();
}

function checkSelectedTab() {

    $("#ElencoCorsiExt").removeClass("block block-opt-refresh");
    $("#ElencoCorsi").removeClass("academy-not-authorized");
    
    //ManageCatPage(1);
    CheckSelectedFilter2(null, false);

    if ($('#filtri_SearchString').val() != '') {
        $('#row_pagination').show();
        $('#row_show_more').hide();
    }
}



function CheckSelected(area, gruppo, opback) {

    if ($('[data-sort]').length == 0)
        return;

    //var urlParams = new URLSearchParams(window.location.search);
    var checkLocalStorage = false;

    //if (urlParams.has('op') && urlParams.get('op') == 'back') {
    if (opback){
        var checkPreviousFilter = window.localStorage.getItem('checkPreviousFilter');
        checkLocalStorage = checkPreviousFilter == "true";
    }
    else {
        window.localStorage.setItem('tematicaFilter', '');
        window.localStorage.setItem('metodoFilter', '');
        window.localStorage.setItem('gruppoFilter', '');
        window.localStorage.setItem('checkPreviousFilter', 'false');
    }

    if (area != '' || gruppo!='' || checkLocalStorage) {
        CheckSelectedFilter2(null, checkLocalStorage);
    }
    else {
        UpdateNumCorsi();
    }

    //$("#ElencoCorsiExt").removeClass("block");
    //$("#ElencoCorsiExt").removeClass("block-opt-refresh");
    //$("#ElencoCorsi").removeClass("academy-not-authorized");
}

function ManageCatPage(pageNum) {
    var pageSize = $('[data-list]').attr('data-list-page-size');
    var numItems = $('[data-list]').attr('data-list-num-items');

    var currentState = 0;
    var tabStato = $('.cors.panel-featured-primary');
    if (tabStato.length>0)
        currentState = $('.cors.panel-featured-primary').attr('id').replace('button-cat-', '');

    var minIndex = (pageNum - 1) * pageSize;
    var maxIndex = pageNum * pageSize;

    var numNotFiltered = 0;

    var valTabIndex = '0';

    $('[data-list-item]').each(function (index) {
        if (currentState == 0 || $(this).attr('data-stato') == currentState) {
            if ($(this).attr('data-list-filtered') == '') {
                numNotFiltered++;
                if (numNotFiltered == 1) {
                    $(this).attr('tabindex', '0');
                }

                if (numNotFiltered > minIndex && numNotFiltered <= maxIndex) {
                    $(this).show();
                    $(this).attr('tabindex', valTabIndex);
                    valTabIndex = '-1';
                }
                else {
                    $(this).hide();
                    $(this).attr('tabindex', '-1');
                }
            }
            else {
                $(this).hide();
                $(this).attr('tabindex', '-1');
            }
        }
        else {
            $(this).hide();
            $(this).attr('tabindex', '-1');
        }
    });

    $('.pagination').html('');
    if (numNotFiltered > 0) {
        $('#list-no-item').hide();
        var n = parseInt(numNotFiltered / pageSize);
        if (n * pageSize < numNotFiltered) n++;
        var content = "";
        if (n > 1) {

            var pageLimit = 4;
            var posIndex = pageNum;

            if (n <= pageLimit) {
                content += '<li class="page-item ' + (pageNum == 1 ? 'disable disabled' : '') + '" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link" onclick="ManageCatPage(' + (pageNum - 1) + ')" aria-label="Pagina precedente">&laquo;</a></li>';
                for (var i = 1; i <= n; i++) {
                    content += '<li class="page-item" role="presentation"><a role="tab" tabindex="' + (i == pageNum ? '0' : '-1') + '" aria-selected="' + (i == pageNum ? 'true' : 'false') + '" class="page-link" onclick="ManageCatPage(' + i + ')" aria-label="Pagina ' + i + '">' + i + '</a></li>';
                }
                content += '<li class="page-item ' + (pageNum == n ? 'disable disabled' : '') + '" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link" onclick="ManageCatPage(' + (pageNum + 1) + ')" aria-label="Pagina successiva">&raquo;</a></li>';

                posIndex += 1;
            }
            else {
                var blockVisible = parseInt((pageNum-1) / pageLimit, 10);
                var numBlock = parseInt(n / pageLimit, 10);
                if (numBlock*pageLimit<n){
                    numBlock+=1;
                }

                var minIndex = blockVisible * pageLimit + 1;
                var maxIndex = blockVisible == numBlock - 1 ? n : (blockVisible + 1) * pageLimit;

                content += '<li class="page-item ' + (pageNum == 1 ? 'disable disabled' : '') + '" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage(' + (pageNum - 1) + ')" aria-label="Pagina precedente">&laquo;</a></li>';
                if (blockVisible > 0) {
                    //content += '<li class="page-item" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage(1)" aria-label="Prima pagina">1</a></li>';
                    content += '<li class="page-item" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage(' + (minIndex - 1) + ')" aria-label="Pagine precedenti">...</a></li>';
                }
                for (var i = minIndex; i <= maxIndex; i++) {
                    content += '<li class="page-item" role="presentation"><a role="tab" tabindex="' + (i == pageNum ? '0' : '-1') + '" aria-selected="' + (i == pageNum ? 'true' : 'false') + '" class="page-link academy-page-item" onclick="ManageCatPage(' + i + ')" aria-label="Pagina ' + i + '">' + i + '</a></li>';
                }
                if (blockVisible < numBlock-1) {
                    content += '<li class="page-item" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage(' + (maxIndex + 1) + ')" aria-label="Pagine successive">...</a></li>';
                    //content += '<li class="page-item" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage('+n+')" aria-label="Ultima pagina">'+n+'</a></li>';
                }
                content += '<li class="page-item ' + (pageNum == n ? 'disable disabled' : '') + '" role="presentation"><a role="tab" tabindex="-1" aria-selected="false" class="page-link academy-page-item" onclick="ManageCatPage(' + (pageNum + 1) + ')" aria-label="Pagina successiva">&raquo;</a></li>';
                

                posIndex = (pageNum-blockVisible*pageLimit) + (blockVisible==0?1:2);
            }

            $('.pagination').html(content);
            $('.page-item:nth-child(' + (posIndex) + ')').addClass('active');
        }

        //$('[data-list-item][tabindex=0]').focus();
    }
    else {
        $('#list-no-item').show();
    }

    UpdateNumCorsi();

    $('[data-page-more]').attr('data-page-visible', pageNum);
}

function checkValidValue(valueSelected, elem, dataAttribute){
    return valueSelected=='' || isSelectedValue(valueSelected, elem ,dataAttribute);
}

function isSelectedValue(valueSelected, elem, dataAttribute) {
    return valueSelected.includes('"' + $(elem).attr('data-' + dataAttribute) + '"');
}

function setCheckPreSelectedValue(filterName, filterValue, selectedAreas) {
    filterValue.split(', ').forEach(function (element) {
        var check = $('[data-filter="' + filterName + '"][data-filter-value=' + element + ']');
        $(check).attr('checked', 'checked');
        var selectedArea = $(check).closest('.panel').attr('data-filter-title');
        if (!selectedAreas.includes(selectedArea))
            selectedAreas += selectedArea + ",";
    });

    return selectedAreas;
}

function CheckSelectedFilter2(check, checkLocalStorage) {
    var tematicaFilter = '';
    var metodoFilter = '';
    var gruppoFilter = '';
    var somethingSelected = false;

    //if (check != null) {
    //    var isChecked = $(check)[0].checked;
    //    if (isChecked)
    //        $('[data-filter="' + $(check).attr('data-filter') + '"][data-filter-value="' + $(check).attr('data-filter-value') + '"]').attr('checked', 'checked');
    //    else
    //        $('[data-filter="' + $(check).attr('data-filter') + '"][data-filter-value="' + $(check).attr('data-filter-value') + '"]').removeAttr('checked');
    //}

    if (checkLocalStorage) {

        tematicaFilter = window.localStorage.getItem('tematicaFilter');
        metodoFilter = window.localStorage.getItem('metodoFilter');
        gruppoFilter = window.localStorage.getItem('gruppoFilter');

        var selectedAreas = '';
        if (tematicaFilter != '') {
            selectedAreas = setCheckPreSelectedValue("Tematica", tematicaFilter, selectedAreas);
            somethingSelected = true;
        }
        if (metodoFilter != '') {
            selectedAreas = setCheckPreSelectedValue("MetodoFormativo", metodoFilter, selectedAreas);
            somethingSelected = true;
        }
        if (gruppoFilter != '') {
            selectedAreas = setCheckPreSelectedValue("Gruppo", gruppoFilter, selectedAreas);
            somethingSelected = true;
        }

        if (somethingSelected && selectedAreas != '') {
            selectedAreas.split(',').forEach(function (element) {
                togg($('[data-filter-title="' + element + '"]'));
            });
        }
    }
    else if ($('[data-filter]:checked').length > 0) {
        $('[data-filter]:checked').each(function () {
            if ($(this).attr('data-filter') == 'Tematica') {
                if (tematicaFilter != '') tematicaFilter = tematicaFilter + ', ';
                tematicaFilter = tematicaFilter + '"' + $(this).attr('data-filter-value') + '"';
                somethingSelected = true;
            }
            else if ($(this).attr('data-filter') == 'MetodoFormativo') {
                if (metodoFilter != '') metodoFilter = metodoFilter + ', ';
                metodoFilter = metodoFilter + '"' + $(this).attr('data-filter-value') + '"';
                somethingSelected = true;
            }
            else if ($(this).attr('data-filter') == 'Gruppo') {
                if (gruppoFilter != '') gruppoFilter = gruppoFilter + ', ';
                gruppoFilter = gruppoFilter + '"' + $(this).attr('data-filter-value') + '"';
                somethingSelected = true;
            }
        });
    }


    if (somethingSelected){
        $('[data-list]').find('[data-corso]').each(function () {
            if (
                (
                 (tematicaFilter!='' && gruppoFilter!='' && (isSelectedValue(tematicaFilter, this, "Tematica") || isSelectedValue(gruppoFilter, this, "Gruppo")))
                 ||
                 (checkValidValue(tematicaFilter, this, "Tematica") && checkValidValue(gruppoFilter, this, "Gruppo"))
                )
                && checkValidValue(metodoFilter, this, "MetodoFormativo")
                ) {
                $(this).attr('data-list-filtered', '');
            }
            else
                $(this).attr('data-list-filtered', 'true');
        });
    }
    else {
        $('[data-list]').find('[data-corso]').attr('data-list-filtered', '');
    }

    $('#row_pagination').show();
    $('#row_show_more').hide();

    window.localStorage.setItem('tematicaFilter', tematicaFilter);
    window.localStorage.setItem('metodoFilter', metodoFilter);
    window.localStorage.setItem('gruppoFilter', gruppoFilter);
    window.localStorage.setItem('checkPreviousFilter', tematicaFilter != '' || metodoFilter != '' || gruppoFilter != '');

    ManageCatPage(1);
}

function sortCorsiByString(elem, attributeName) {

    var sortOrder = parseInt($(elem).attr('data-sort-order'));

    var cc = $('[data-list-item]').sort(function (a, b) {
        return String.prototype.localeCompare.call($(a).attr('data-' + attributeName).toLowerCase(), $(b).attr('data-' + attributeName).toLowerCase())*sortOrder;
    });

    $('[data-list]').html(cc);
    $('[data-sort]').attr('data-sort-order', 1);
    $(elem).attr('data-sort-order', sortOrder * -1);
   ManageCatPage(1);
}
function sortCorsiByNumber(elem, attributeName) {

    var sortOrder = parseInt($(elem).attr('data-sort-order'));

    var cc = $('[data-list-item]').sort(function (a, b) {
        var aVal = parseInt($(a).attr('data-' + attributeName));
        var bVal = parseInt($(b).attr('data-' + attributeName));

        if (aVal == bVal)
            return 0;
        else if (aVal > bVal)
            return 1*sortOrder;
        else
            return -1*sortOrder;
    });

    $('[data-list]').html(cc);
    $('[data-sort]').attr('data-sort-order', 1);
    $(elem).attr('data-sort-order', sortOrder * -1);
    ManageCatPage(1);
}

function showMoreCourses() {
    var pageSize = $('[data-list]').attr('data-list-page-size');

    var pageIndex = Number($('[data-page-more]').attr('data-page-visible'))+1;

    var minIndex = 0;
    var maxIndex = pageIndex * pageSize;
    var anyHidden = false;

    $('[data-list-item]').each(function (index) {
        if (index >= minIndex && index < maxIndex)
            $(this).show();
        else {
            anyHidden = true;
            $(this).hide();
        }
    });

    var pos = 0;
    $('[data-page-more]').attr('data-page-visible', pageIndex);
    if (!anyHidden) {
        $('#row_show_more').hide();
        pos = $('#row_show_more').offset().top+150;
    }
    else {
        pos = $('#row_show_more').offset().top;
    }
    $('html,body').animate({ scrollTop: pos }, { duration: 1000 });
    
    //$('[data-list-item][tabindex=0]').focus();
}

function Iscriviti(idCorso) {
    RichiediIscrizione(idCorso);
    return;
    swal({
        title: 'Richiesta iscrizione',
        text: "Per poter procedere con l'iscrizione, inserisci la mail del tuo responsabile",
        input: 'email',
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-primary btn-lg push-5-r',
        showCancelButton: true,
        cancelButtonText: "Annulla",
        cancelButtonClass: 'btn btn-lg btn-default push-5-l',
        buttonsStyling: false,

    }).then(function (result) {
        $.ajax({
            url: "/RaiAcademy/IscrizioneCorso",
            type: "POST",
            //dataType: "json",
            data: { idCorso: idCorso, mailResp: result},
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Richiesta inviata con successo');
                        //Per il momento, appena effettuata la richiesta, rimuoviamo gli elementi dell'autorizzazione
                        AuthCorso();
                        break;
                    default:
                        swal('Non è stato possibile completare l\'iscrizione');
                        break;
                }
                
            },
            error: function (a,b,c) {
                swal(a+' '+b+' '+c);
            }
        });
    })
}

function AuthCorso() {
    $(document).find('.academy-overlay-auth').hide();
    $(document).find('.academy-not-authorized').removeClass('academy-not-authorized');
    $(document).find('.academy-auth-required').hide();
    $(document).find('.academy-main-action').show();
}

function BookMarkCorso(event, anchor, idCorso) {

    event.preventDefault();
    event.stopPropagation();

    var elem = $(anchor).find('[data-bookmark-corso]')
    var hasPrimary = $(elem).hasClass('text-primary');

    $.ajax({
        url: "/RaiAcademy/ToggleIncuriosisce",
        type: "POST",
        //dataType: "json",
        data: { idCorso: idCorso },
        success: function (data) {
            switch (data) {
                case "OK":
                    if (!hasPrimary) {
                        swal({
                            title: 'Aggiunto tra i preferiti',
                            type: 'success'
                        });
                        $(anchor).attr('aria-label', "Rimuovi dai miei preferiti");
                        $(elem).addClass('text-primary');
                    }
                    else {
                        swal({
                            title: 'Rimosso dai preferiti',
                            type: 'info',
                        });
                        $(anchor).attr('aria-label', "Aggiungi ai miei preferiti");
                        $(elem).removeClass('text-primary');

                        if ($('#MiIncuriosiscono').length > 0) {
                            location.reload();
                        }
                    }
                    break;
                default:
                    swal(data);
                    break;
            }

        },
        error: function (a, b, c) {
            swal(a + ' ' + b + ' ' + c);
        }
    });
}

function moveToListItem(event, elem) {
    if (event.keyCode == 40) {
        var elems = $(elem).closest('[data-list]').find('[data-list-item]:visible');
        var index = elems.index(elem);
        if (index < elems.length - 1) {
            event.preventDefault();
            $(elem).attr('tabindex', "-1");
            var nextElem = $(elems[index + 1]);
            nextElem.attr('tabindex', '0');
            nextElem.focus();
        }
    }
    else if (event.keyCode == 38) {
        var elems = $(elem).closest('[data-list]').find('[data-list-item]:visible');
        var index = elems.index(elem);
        if (index > 0) {
            event.preventDefault();
            $(elem).attr('tabindex', "-1");
            var prevElem = $(elems[index-1]);
            prevElem.attr('tabindex', '0');
            prevElem.focus();
        }
    }
    else if (event.keyCode == 13) {
        event.preventDefault();
        var rif = $(elem).find('[data-list-detail]').attr('href');
        window.location.href = rif;
    }
    else if (event.keyCode == 32) {
        event.preventDefault();
        $(elem).find('[data-list-toggle-fav]').click();
    }
}

function enterSezFilter(headerSez) {
    if (event.keyCode == 40) {
        $(headerSez).closest('div').find('[role][tabindex]').first().focus();
    }
}

function UpdateNumCorsi() {
    
    var cat = $('#div_btn').attr('data-cat');
    var catSelector = cat == 0 ? '' : '[data-stato*="' + cat + '"]';

    var numElemTot = $('[data-list-item]' + catSelector).length;

    var visible = $('[data-list-item]' + catSelector + '[data-list-filtered=""]').length;
    var testo = visible;
    if (numElemTot != visible)
        testo += " su " + numElemTot;
    if (numElemTot == 1)
        testo += " corso trovato";
    else
        testo += " corsi trovati";
    $('#lblNumCorsi').text(testo);

    UpdateUrlImage();
}

function UpdateUrlImage() {
    try {
        if (getIVEHostname() != undefined) {
            $('[data-corso-id]').each(function () {
                var prop = $(this).css("background-image");
                var idCorso = $(this).attr('data-corso-id');
                prop = "url('" + location.protocol + "//" + location.hostname + (location.port ? ":" + location.port : "") + "/RaiAcademy/,DanaInfo=raiperme.intranet.rai.it+GetCourseImage2?idCorso=" + idCorso + "')";
                $(this).css("background-image", prop);
            });
        }
    } catch (e) {

    }
}


function RichiediIscrizione(id_corso) {
    RichiediIscrizioneCorsoEdiz(id_corso, 0);
}

function RichiediIscrizioneCorsoEdiz(id_corso, id_edizione) {
    if (window.localStorage.getItem("rich_" + id_corso + "_" + id_edizione) == "true") {
        swal("Hai già richiesto l'iscrizione");
        $('[data-rich-id=' + id_corso + '][data-rich-idEdiz=' + id_edizione + ']').addClass('disable');
        $('[data-rich-id=' + id_corso + '][data-rich-idEdiz=' + id_edizione + ']').text("Richiesta effettuata");
        return;
    }

    $("#modal-richCorso").html(' ');
    $.ajax({
        url: "/RaiAcademy/RichiediIscrizione",
        type: "GET",
        data: { idCorso: id_corso, idEdiz: id_edizione },
        async: false,
        success: function (data) {
            $("#modal-richCorso").html('');
            $("#modal-richCorso").html(data);
            $("#modal-richCorso").modal('show');
        },
        error: function (result) {
            swal("Errore: " + result);
        }
    });
}

function SubmitIscrizione(idCorso,idEdizione) {
    event.preventDefault();

    $('#btnSubmitRich').addClass("disable");

    var form = $(event.target).parents("form").first();
    var validator = $(form).validate();

    if (!$(form).valid()) {
        swal('Non è stato possibile completare l\'iscrizione');
        $('#btnSubmitRich').removeClass("disable");
        return false;
    }

    var riepilogo = $('#riepilogoCorso');
    $(riepilogo).addClass("text-left");
    //$(riepilogo).css("border", "1px solid lightgray");
    //$(riepilogo).css("margin", "10px");

    swal({
        title: 'Sei sicuro?',
        html: "<label>Stai per inviare una richiesta per richiedere la tua partecipazione al corso:</label><hr/>" + $(riepilogo)[0].outerHTML+"<hr/><label>Confermi?</label>",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sì, confermo!',
        cancelButtonText: 'Annulla'
    }).then(function () {
        var obj = $("#form-submitIscrizione").serialize();
        $.ajax({
            url: "/RaiAcademy/SubmitIscrizione",
            type: "POST",
            //dataType: "json",
            data: obj,
            success: function (data) {
                switch (data) {
                    case "ok":
                        swal('Richiesta inviata con successo');
                        $("#modal-richCorso").modal('hide');

                        //window.localStorage.setItem("rich_" + idCorso + "_" + idEdizione, true);
                        $('[data-rich-id=' + idCorso + '][data-rich-idEdiz=' + idEdizione + ']').addClass('disable');
                        $('[data-rich-id=' + idCorso + '][data-rich-idEdiz=' + idEdizione + ']').text("Richiesta effettuata");

                        //Per il momento, appena effettuata la richiesta, rimuoviamo gli elementi dell'autorizzazione
                        //AuthCorso();
                        break;
                    default:
                        swal('Non è stato possibile completare l\'iscrizione');
                        break;
                }

            },
            error: function (a, b, c) {
                swal(a + ' ' + b + ' ' + c);
            }
        });
    }, function (dismiss) {
        $('#btnSubmitRich').removeClass("disable");
    })


}

function ShowImageCourse() {
    var swalHtml = "";

    var cloneElem = $('div.academy-preview').clone();
    $(cloneElem).css("background-size", "contain");
    $(cloneElem).attr('onclick', '');
    $(cloneElem).removeClass('');
    swalHtml = $(cloneElem)[0].outerHTML;
    
    var cloneElemCaption = $('label.academy-preview-caption').clone();
    $(cloneElemCaption).removeClass('academy-preview-caption');
    $(cloneElemCaption).removeClass('cursor-pointer');
    $(cloneElemCaption).attr('onclick', '');

    if ($(cloneElemCaption).length > 0) {
        $(cloneElemCaption).attr('style', 'text-italic');
        
        swalHtml+="<br/>"+$(cloneElemCaption)[0].outerHTML
    }

    swal({
        width:800, 
        html: swalHtml
    });
}

function gotoDetail(url) {
    if (url != "") {
        return window.location = url;
    }
}

function RefreshIlias() {
    console.log('Refresh Ilias...');
    var frame = document.getElementById("iliasLogger");
    if (frame != null) {
        frame.src = frame.src;
    }
}

function openScorm(event, idCorso, titolo, url) {
    $('#scormIdCorso').val(idCorso);
    RefreshIlias();
    
    $.ajax({
        url: "/RaiAcademy/OpenScorm",
        type: "GET",
        dataType: "json",
        async: false,
        data: { idCorso: idCorso, url: url },
        success: function (data) {
            if (data.checkIliasIE) {
                event.preventDefault();
                swal("Errore", "Utilizzare il browser Chrome per la fruizione del corso", "error");
            }
            else {
                switch (data.typeOpen) {
                    case "modal":
                        $('#titoloScorm').text(titolo);
                        $('#iFrameScorm').attr('src', url);
                        $('#modal-openScorm').modal('show');
                        $('#modal-openScorm').on('hidden.bs.modal', function (e) {
                            CloseScorm();
                        });
                        event.preventDefault();
                        //return false;
                        break;
                    case "redirect":
                        //return true;
                        //var win = window.open(url, "newwin", "width=800,height=600,titlebar=0,menubar=0,location=0");
                        //if (data.checkPopupBlock)
                        //    popupBlockerChecker.check(win);

                        //if (win) {
                        //    var timer = setInterval(function () {
                        //        if (!win || win.closed) {
                        //            clearInterval(timer);
                        //            CloseScorm();
                        //        }
                        //    }, 1000);
                        //}
                        if (url.indexOf('ilias')>0) {
                            $(window).on('focus', function () {
                                var extCourse = $('#corsoExt').val();
                                var statoCorso = $('#corsoStato').val();
                                $('#corsi').addClass('rai-loader');
                                $.ajax({
                                    url: '/raiacademy/ElencoPillole',
                                    type: "GET",
                                    data: { idCorso: idCorso, extCourse:extCourse, iscritto:'iscritto', stato:statoCorso },
                                    dataType: "html",
                                    cache: false,
                                    success: function (data) {
                                        $('#corsi').replaceWith($(data).find('#corsi'));
                                    }
                                });
                            })
                        }

                        break;
                    default:
                        event.preventDefault();
                        swal("Errore", data.messageError, "error");
                        //return false;
                        break;
                }
            //}
        }
        }
    });
}
function openResource(event, idCorso, titolo, url) {
    $('#scormIdCorso').val(idCorso);
    RefreshIlias();

    $.ajax({
        url: "/RaiAcademy/OpenResource",
        type: "GET",
        async: false,
        dataType: "json",
        data: { idCorso: idCorso, url: url },
        success: function (data) {
            if (data.checkIliasIE) {
                event.preventDefault();
                swal("Errore", "Utilizzare il browser Chrome per la fruizione del corso", "error");
            }
            else {
                switch (data.typeOpen) {
                    case "modal":
                        $('#titoloScorm').text(titolo);
                        $('#iFrameScorm').attr('src', url);
                        $('#modal-openScorm').modal('show');
                        event.preventDefault();
                        break;
                    case "redirect":

                        //var win = window.open(url, "newwin");
                        //if (data.checkPopupBlock)
                        //    popupBlockerChecker.check(win);
                        break;
                    default:
                        swal("Errore", data.messageError, "error");
                        event.preventDefault();
                        break;
                }
            //}
        }
        }
    });
}

function CloseScorm() {
    $('#iFrameScorm').attr('src', '');
    var idCorso = $('#scormIdCorso').val();
    $.ajax({
        url: "/RaiAcademy/CloseScorm",
        type: "GET",
        data: { idCorso: idCorso },
        success: function (data) {
            switch (data) {
                case "OK":
                    break;
                default:
                    swal("Si è verificato un errore durante la chiusura del corso", "", "error");
                    break;
            }

        }
    });
}

function ToggleFilterBox() {
    var state = $('#FilterBox').attr('aria-expanded');
    if (state == 'true') {
        $('#FilterBox').attr('aria-expanded', 'false');
        $('#FilterBox').addClass('collapse');
        $('#FilterBox').removeClass('expanded');
        $('#FilterBoxToggle').text("Mostra tutti i filtri");
    }
    else {
        $('#FilterBox').attr('aria-expanded', 'true');
        $('#FilterBox').removeClass('collapse');
        $('#FilterBox').addClass('expanded');
        $('#FilterBoxToggle').text("Nascondi i filtri");
    }
}

var popupBlockerChecker = {
    check: function (popup_window) {
        var _scope = this;
        if (popup_window) {
            if (/chrome/.test(navigator.userAgent.toLowerCase())) {
                setTimeout(function () {
                    _scope._is_popup_blocked(_scope, popup_window);
                }, 200);
            } else {
                popup_window.onload = function () {
                    _scope._is_popup_blocked(_scope, popup_window);
                };
            }
        } else {
            _scope._displayError();
        }
    },
    _is_popup_blocked: function (scope, popup_window) {
        if ((popup_window.innerHeight > 0) == false) { scope._displayError(); }
    },
    _displayError: function () {
        swal({
            title: "",
            type: "warning",
            html: "Il blocco dei pop-up è attivo.<br/><br/>Per visualizzare correttamente i contenuti è necessario modificare le impostazioni del browser. Scopri come nella sezione “Domande frequenti” oppure chiama il 2550"
        });
    }
};

function EnableCourseButton() {
    $('.a_corsi').removeClass('disable');
    $('#gotoCorsi').removeClass('disable');
}
function EnableResourceButton() {
    $('.a_risorse').removeClass('disable');
}


function PlayerPlay() {
    var player = document.getElementById("playerTest");
    player.ontimeupdate = function () {
        $('#currentTime')[0].innerText = player.currentTime;
    }
    player.play();
}
function PlayerPause() {
    var player = document.getElementById("playerTest");
    player.pause();
}
function PlayerGoto() {
    var player = document.getElementById("playerTest");
    player.currentTime = 120;
}
function UpdateTime() {
    $('#currentTime')[0].innerText = player.currentTime;
}

function ShowDettaglioRichiesta(idRich) {
    event.preventDefault();
    $('#modal-richiesta-internal').html(' ');
    $('#modal-richiesta-internal').load('/ApprovazioneFormazione/DettaglioRichiesta?idRichiesta='+idRich);
    $('#modal-richiesta').modal('show');
}

function RichFormRifiuta(updateDett, idRich, idRichStep) {
    var richMotivo = $('#richMotivo_'+idRich).val();

    if (richMotivo==null || richMotivo == '') {
        swal({
            title: "Inserire la motivazione del rifiuto",
            type: "error"
        });
        return;
    }

    swal({
        title: 'Rifiuta richiesta',
        text: "Sei sicuro di voler rifiutare la richiesta?",
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-primary btn-lg push-5-r',
        showCancelButton: true,
        cancelButtonText: "Annulla",
        cancelButtonClass: 'btn btn-lg btn-default push-5-l',
        buttonsStyling: false,

    }).then(function (result) {
        $.ajax({
            url: "/ApprovazioneFormazione/RifiutaRichiesta",
            type: "POST",
            //dataType: "json",
            data: { idRichiestaStep: idRichStep, motivo: richMotivo },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Richiesta rifiutata con successo');
                        if (updateDett > 0) {
                            RefreshRichiesta(idRich);
                        }
                        $('#form-ricerca-ElencoRichieste').submit();
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
    
function RichFormAccetta(updateDett, idRich, idRichStep) {
    var destinatario = $('#Destinatario_'+idRich).val();
    var richMotivo = $('#richMotivo_'+idRich).val();

    if (destinatario==null || destinatario == '') {
        swal({
            title: "Inserire il destinatario della richiesta",
            type: "error"
        });
        return;
    }

    swal({
        title: 'Approva richiesta',
        text: "Sei sicuro di voler approvare la richiesta?",
        confirmButtonText: 'OK',
        confirmButtonClass: 'btn btn-primary btn-lg push-5-r',
        showCancelButton: true,
        cancelButtonText: "Annulla",
        cancelButtonClass: 'btn btn-lg btn-default push-5-l',
        buttonsStyling: false,

    }).then(function (result) {
        $.ajax({
            url: "/ApprovazioneFormazione/ApprovaRichiesta",
            type: "POST",
            //dataType: "json",
            data: { idRichiestaStep: idRichStep, motivo: richMotivo, dest: destinatario },
            success: function (data) {
                switch (data) {
                    case "OK":
                        swal('Richiesta approvata con successo');
                        if (updateDett > 0) {
                            RefreshRichiesta(idRich);
                        }
                        $('#form-ricerca-ElencoRichieste').submit();
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

function RefreshRichiesta(idRich) {
    $('#richiestaContent').addClass('css-input-disabled');
    $("#richiestaContentExt").addClass("block block-opt-refresh");

    $.ajax({
        url: "/ApprovazioneFormazione/DettaglioRichiesta",
        type: "GET",
        data: { idRichiesta: idRich },
        success: function (data) {
            $("#richiestaContent").replaceWith($(data).find("#richiestaContent"));
            $('#richiestaContent').removeClass('css-input-disabled');
            $("#richiestaContentExt").removeClass("block block-opt-refresh");
        }
    });
}

function AcaPulisciFiltri(formID, submit) {
    event.preventDefault();

    $('.' + formID + '.form-control-value').val('');
    $('.' + formID + '.form-control-value-int').val('0');
    $('.' + formID + '.form-control-bool').val('false');
    $('.' + formID + '.form-control-bool-true').val('true');

    if (submit)
        $('#' + formID).submit();
}
function AcaCheckHasFilter(formID) {
    $('#' + formID + ' > #HasFilter').val('false');
    $('.' + formID + '.form-control-value').each(function () {
        if ($(this).val() != '') {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
    $('.' + formID + '.form-control-value-int').each(function () {
        if ($(this).val() != 0) {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
}
function AcaPreSubmit(destBox) {
    $("#" + destBox).addClass("rai-loader");
}
function AcaSuccessSubmit(formId, destBox) {
    $("#" + destBox).removeClass("rai-loader");

    //gestSetFilterDescr(formId, destBox);
}

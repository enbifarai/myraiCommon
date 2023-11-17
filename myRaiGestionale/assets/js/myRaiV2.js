function createGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

//$('input[type=range]').on('input', function () {
//    setWidthRange($(this));
//})
//$('input[type=range]').on('afterprint', function () {
//    setWidthRange($(this));
//})

function setWidthRange(element) {
    var min = $(element).attr('min') || 0;
    var max = $(element).attr('max') || 100;

    var rangeVal = $(element).val() || 1;

    var perc = ($(element).val() - min) / (max - min) * 100;

    var val = perc + '% 100%';

    var id = $(element).attr('id');
    if (id == undefined || id == "") {
        id = createGuid();
        $(element).attr('id', id);
    }

    var styleEl = $('#' + id + '_style');
    if (styleEl.length == 0) {
        $('head').append('<style id="' + id + '_style"></style>');
        styleEl = $('#' + id + '_style');
    }

    $(styleEl).html('#' + id + '::webkit-slider-runnable-track{ background-size: ' + val + '}');

    //$(element).find('::-webkit-slider-runnable-track').css('background-size', val);
}

function manageAccordionCollapse() {
    $('.collapse').on('shown.bs.collapse', function (e) {
        var $panel = $(this).closest('.panel');
        $('html,body').animate({
            scrollTop: $panel.offset().top - 70
        }, 500);
    });
}

function RaiOpenAsyncModal(id, url, parameters, action, callType, onCloseCheckDiff, contentType, reloadonlyifempty) {

    if (event != undefined)
        event.preventDefault();

    if (callType === undefined || callType===null)
        callType = "GET";

    if (contentType === undefined || contentType === null)
        contentType = 'application/x-www-form-urlencoded; charset=UTF-8';

    //$('.modal').css('z-index', '1050');
    //$('#' + id).css('z-index', '2000');
    if (reloadonlyifempty === undefined || reloadonlyifempty === null)
        reloadonlyifempty = false;
    if (reloadonlyifempty == false || $('#' + id + '-internal').attr("caricato") != "true") {
        $('#' + id + '-internal').html('<div class="rai-loader" style="height:100vh"></div>');
        $('#' + id).modal('show');
        $.ajax({
            url: url,
            type: callType,
            dataType: "html",
            data: parameters,
            contentType: contentType,
            cache: false,
            success: function (data) {
                $('#' + id + '-internal').html(data);
                $('#' + id + '-internal').attr("caricato", "true");
                if (onCloseCheckDiff === true) {
                    RaiModalAddCheckListener(id);
                }

                if (action != undefined) {
                    action();
                }
            },
            error: function (a, b, c) {
                swal({
                    title: "Ops...",
                    text: "Si è verificato un errore imprevisto\n" + c,
                    type: 'error',
                    customClass: 'rai'
                });

                $('#' + id + '-internal').html('');
                $('#' + id).modal('hide');
            }
        });
    }
    else {
        $('#' + id).modal('show');
        if (onCloseCheckDiff === true) {
            RaiModalAddCheckListener(id);
        }

        if (action != undefined) {
            action();
        }
    }
}


function RaiModalAddCheckListener(modalName) {
    var _name = '#' + modalName;

    $(_name).append('<input type="hidden" value="0" id="flagChanged-' + modalName + '" />');

    $(_name).on('hide.bs.modal', function () {
        var check = $("#flagChanged-" + modalName).val();
        if (check == '1') {
            event.preventDefault();
            event.stopImmediatePropagation();
            swal({
                title: 'Sei sicuro?',
                text: "Vuoi uscire senza salvare?",
                type: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Continua a modificare',
                cancelButtonText: 'Esci',
                customClass: 'rai rai-confirm-cancel',
                reverseButtons: true
            }).then(function (result) {
                $(_name).modal('toggle');
            });
        }
    });

    $(_name).on('change', 'input', function () {
        $("#flagChanged-" + modalName).val('1');
    });
    $(_name).on('change', 'select', function () {
        $("#flagChanged-" + modalName).val('1');
    });
}
function RaiDisableCheckListener(modalName) {
    $("#flagChanged-" + modalName).val('0');
}


function RaiUpdateWidget(idParam, url, updateType, parameters, asSubmitForm, successCallback, preventRaiLoader, ajaxType) {
    var _id = "#" + idParam;

    if (preventRaiLoader != undefined && preventRaiLoader === true) {

    } else {
        $('#' + idParam).addClass("rai-loader");
    }

    if (ajaxType == undefined || ajaxType == null || ajaxType == "")
        ajaxType = "GET";

    if (asSubmitForm === undefined || asSubmitForm == false) {
        $.ajax({
            async: true,
            url: url,
            type: ajaxType,
            data: parameters,
            dataType: "html",
            cache: false,
            success: function (data) {
                if (updateType == 'replaceId') {
                    $('#' + idParam).replaceWith($(data).find(_id));
                }
                else if (updateType == 'replace') {
                    $('#' + idParam).replaceWith(data);
                }
                else if (updateType == 'html') {
                    $('#' + idParam).html(data);
                    $('#' + idParam).removeClass("rai-loader");
                }
                if (successCallback != undefined && successCallback != null) {
                    successCallback();
                }
            },
            error: function (a, b, c) {
                swal({
                    title: "Ops...",
                    text: "Si è verificato un errore imprevisto\n" + c,
                    type: 'error',
                    customClass: 'rai'
                });
            }
        });
    }
    else {
        $.ajax({
            async: true,
            url: url,
            processData: false,
            contentType: false,
            type: "POST",
            data: parameters,
            success: function (data) {
                if (updateType == 'replaceId') {
                    $('#' + idParam).replaceWith($(data).find(_id));
                }
                else if (updateType == 'replace') {
                    $('#' + idParam).replaceWith(data);
                }
                else if (updateType == 'html') {
                    $('#' + idParam).html(data);
                    $('#' + idParam).removeClass("rai-loader");
                }
                if (successCallback != undefined && successCallback != null) {
                    successCallback();
                }
            },
            error: function (a, b, c) {
                swal({
                    title: "Ops...",
                    text: "Si è verificato un errore imprevisto\n" + c,
                    type: 'error',
                    customClass: 'rai'
                });
            }
        });
    }
}
function RaiToggleCheckBoxAll(groupName) {
    var checkAll = '[data-check-group-all="' + groupName + '"]';
    var otherCheck = '[data-check-group="' + groupName + '"]';

    if ($(checkAll).hasClass('not-all')) {
        $(checkAll)[0].checked = true;
    }

    if ($(checkAll)[0].checked) {
        $(otherCheck).each(function () {
            $(this)[0].checked = true;
        });
        $(checkAll).removeClass('not-all');
    }
    else {
        $(otherCheck).each(function () {
            $(this)[0].checked = false;
        });
        $(checkAll).removeClass('not-all');
    }
}
function RaiUpdateCheckBoxAll(groupName) {
    var checkAll = '[data-check-group-all="' + groupName + '"]';
    var otherCheck = '[data-check-group="' + groupName + '"]';

    var allElem = $(otherCheck).length;
    var checked = $(otherCheck + ':checked').length;
    if (checked == 0) {
        $(checkAll)[0].checked = false;
        $(checkAll).removeClass('not-all');
    }
    else if (checked < allElem) {
        $(checkAll)[0].checked = true;
        $(checkAll).addClass('not-all');
    }
    else {
        $(checkAll)[0].checked = true;
        $(checkAll).removeClass('not-all');
    }
}
function RaiDeleteRecord(alertMsg, action, parameters, successMsg, successAction, actionType, isHtmlSwal, isJsonResult) {
    var _actionType = actionType || "GET";
    var _isJsonResult  = isJsonResult || false;

    //var swalParam = {
    //    title: 'Sei sicuro?',
    //    type: 'warning',
    //    showCancelButton: true,
    //    confirmButtonText: 'Sì, cancella!',
    //    cancelButtonText: 'Annulla',
    //    reverseButtons: true,
    //    customClass: 'rai rai-confirm-cancel'
    //};

    //if (isHtmlSwal) {
    //    swalParam.html = alertMsg;
    //}
    //else {
    //    swalParam.text = alertMsg;
    //}

    var func = function () {

    };

    if (isHtmlSwal) {
        swal({
            title: 'Sei sicuro?',
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sì, cancella!',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            customClass: 'rai rai-confirm-cancel',
            html: alertMsg
        }).then(function () {
            $.ajax({
                url: action,
                type: _actionType,
                dataType: _isJsonResult?"json":"html",
                data: parameters,
                cache: false,
                success: function (data) {
                    if (_isJsonResult) {
                        if (data.esito) {
                            swal({
                                title: successMsg,
                                type: "success",
                                customClass: 'rai'
                            });
                            successAction();
                        } else {
                            swal({
                                title: "Ops...",
                                text: data,
                                type: 'error',
                                customClass: 'rai'
                            })
                        }
                    }
                    else {
                        switch (data) {
                            case "OK":
                                swal({
                                    title: successMsg,
                                    type: "success",
                                    customClass: 'rai'
                                });
                                successAction();
                                break;
                            default:
                                swal({
                                    title: "Ops...",
                                    text: data,
                                    type: 'error',
                                    customClass: 'rai'
                                })
                        }
                    }
                },
                error: function (a, b, c) {
                    swal({
                        title: "Ops...",
                        text: "Si è verificato un errore imprevisto\n" + c,
                        type: 'error',
                        customClass: 'rai'
                    })
                }
            });
        });
    } else {
        swal({
            title: 'Sei sicuro?',
            type: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sì, cancella!',
            cancelButtonText: 'Annulla',
            reverseButtons: true,
            customClass: 'rai rai-confirm-cancel',
            text: alertMsg
        }).then(function () {
            $.ajax({
                url: action,
                type: _actionType,
                dataType: _isJsonResult ? "json" : "html",
                data: parameters,
                cache: false,
                success: function (data) {
                    if (_isJsonResult) {
                        if (data.esito) {
                            swal({
                                title: successMsg,
                                type: "success",
                                customClass: 'rai'
                            });
                            successAction();
                        } else {
                            swal({
                                title: "Ops...",
                                text: data,
                                type: 'error',
                                customClass: 'rai'
                            })
                        }
                    }
                    else {
                        switch (data) {
                            case "OK":
                                swal({
                                    title: successMsg,
                                    type: "success",
                                    customClass: 'rai'
                                });
                                successAction();
                                break;
                            default:
                                swal({
                                    title: "Ops...",
                                    text: data,
                                    type: 'error',
                                    customClass: 'rai'
                                })
                        }
                    }
                },
                error: function (a, b, c) {
                    swal({
                        title: "Ops...",
                        text: "Si è verificato un errore imprevisto\n" + c,
                        type: 'error',
                        customClass: 'rai'
                    })
                }
            });
        });
    }
}
function RaiClearForm(idForm) {
    var form = $('#' + idForm);
    $(form).find('input[type=text]').val('');
    $(form).find('select:not([data-rai-select])').val('');
    $(form).find('select[data-rai-select]').each(function () { RaiSelectClear($(this)[0].id); });
    $(form).find('input[type=checkbox]').each(function () { $(this)[0].checked = false; });
    $(form).find('input[type=radio]').each(function () { $(this)[0].checked = false; });
}
function RaiSubmitForm(button, idForm, parametersGetter, processData, contentType, successMsg, successAction, clearForm) {
    event.preventDefault();
    $(button).addClass("disable");

    var form = $("#" + idForm).first();
    var validator = $(form).validate();

    if (!$(form).valid()) {
        $(button).removeClass("disable");
        return false;
    }

    var parameters = parametersGetter();

    $(form).parent().addClass("rai-loader");

    $.ajax({
        url: $(form).attr("action"),
        processData: processData,
        contentType: contentType,
        type: "POST",
        data: parameters,
        success: function (data) {
            switch (data) {
                case "OK":
                    swal({ title: successMsg, type: 'success', customClass: 'rai' });
                    successAction();
                    if (clearForm) {
                        RaiClearForm(idForm);
                    }
                    break;
                default:
                    swal({ title: "Ops...", text: data, type: 'error', customClass: 'rai' });
                    break;
            }
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        },
        error: function (a, b, c) {
            swal({ title: "Ops...", text: ' ' + b + ' ' + c, type: 'error', customClass: 'rai' });
            $(button).removeClass("disable");
            $(form).parent().removeClass("rai-loader");
        }
    })
}
function RaiUpdateSelectValue(action, parameters, selectId, addDefault, defaultValue, defaultText) {
    $.ajax({
        url: action,
        type: "GET",
        dataType: "json",
        data: parameters,
        cache: false,
        success: function (data) {
            if (addDefault) {
                $("#" + selectId).html('<option value="' + defaultValue + '">' + defaultText + '</option>');
            }
            for (var i = 0; i < data.result.length; i++) {
                $("#" + selectId).append('<option value="' + data.result[i].Value + '" ' + (data.result.length == 1 ? 'selected' : '') + '>' + data.result[i].Text + '</option>');
            }
            $("#" + selectId).removeClass("disable");
        },
        error: function () { }
    });
}
function RaiSearchData(input, groupSearch, attribute) {
    var valueRif = "";
    var inputType = $(input).attr('type');
    if (inputType == 'checkbox') {
        if ($(input)[0].checked == true) {
            valueRif = 'on';
        } else {
            valueRif = '';
        }
    } else {
        valueRif = $(input).val();
    }

    var dataGroup = "[data-search='" + groupSearch + "']";
    var dataAttr = "[data-search-" + attribute + "]";
    var dataAttr_name = "search-" + attribute;

    var filterType = $(input).attr('data-search-filter-type');
    if (filterType == null || filterType == '') {
        filterType = 'Contains';
    }


    if (valueRif == "") {
        $(dataGroup + dataAttr).show();
        $(dataGroup + dataAttr).attr('aria-hidden', 'false');
    }
    else {
        $(dataGroup + dataAttr).hide();
        $(dataGroup + dataAttr).attr('aria-hidden', 'true');

        $(dataGroup + dataAttr).filter(function () {
            if (filterType == 'StartsWith')
                return $(this).data(dataAttr_name).toLowerCase().startsWith(String(valueRif).toLowerCase());
            else
                return $(this).data(dataAttr_name).toLowerCase().indexOf(String(valueRif).toLowerCase()) >= 0;
        }).show().attr('aria-hidden','false');

    }
}
function RaiSearchDataMulti(input, groupSearch, externalContainer) {
    var dataGroup = "[data-search='" + groupSearch + "']";

    if (externalContainer != null && externalContainer != '') {
        dataGroup = externalContainer + ' ' + dataGroup;
    }


    var dataGroupMulti = '[data-search-multi="on"]';

    var dataObjSearch = dataGroup;

    var arrayInfo = new Array();

    var hasFilter = false;

    $(dataGroup + dataGroupMulti).each(function () {
        var valueRif = ""
        var inputType = $(this).attr('type');
        if (inputType == 'checkbox') {
            if ($(this)[0].checked == true) {
                valueRif = 'on';
            } else {
                valueRif = '';
            }
        } else {
            valueRif = $(this).val();
        }

        hasFilter = hasFilter || valueRif != '';

        var attribute = $(this).attr('data-search-attr');
        dataObjSearch += "[data-search-" + attribute + "]";

        var filterType = $(this).attr('data-search-filter-type');
        if (filterType == null || filterType == '') {
            filterType = 'Contains';
        }

        arrayInfo.push({
            Value: valueRif,
            Attr: attribute,
            DataAttr: "[data-search-" + attribute + "]",
            DataAttr_Name: "search-" + attribute,
            FilterType: filterType
        });
    });

    var labelCounter = '';

    if (!hasFilter) {
        $(dataObjSearch).show().attr('aria-hidden', 'false');
        labelCounter = $(dataObjSearch).length + '';
    } else {
        $(dataObjSearch).hide().attr('aria-hidden', 'true');

        $(dataObjSearch).filter(function () {
            var showElement = true;

            for (var i = 0; i < arrayInfo.length; i++) {
                var item = arrayInfo[i];
                if (item.Value == '') {
                    showElement = showElement && true;
                } else {
                    if (item.FilterType == 'StartsWith') {
                        showElement = showElement && String($(this).data(item.DataAttr_Name)).toLowerCase().startsWith(String(item.Value).toLowerCase());
                    } else {
                        showElement = showElement && String($(this).data(item.DataAttr_Name)).toLowerCase().indexOf(String(item.Value).toLowerCase()) >= 0;
                    }
                }
            }

            //arrayInfo.forEach(item => {
            //    if (item.Value == '') {
            //        showElement = showElement && true;
            //    } else {
            //        showElement = showElement && String($(this).data(item.DataAttr_Name)).toLowerCase().indexOf(String(item.Value).toLowerCase()) >= 0;
            //    }
            //});

            return showElement;
        }).show().attr('aria-hidden', 'false');

        labelCounter = $(dataObjSearch + '[aria-hidden="false"]').length + ' su ' + $(dataObjSearch).length + '';
    }

    if ($('[data-search-counter="' + groupSearch + '"]').length > 0) {
        $('[data-search-counter="' + groupSearch + '"]').text(labelCounter);
    }
}
function RaiSearchFormClear(formID, submit) {
    event.preventDefault();

    $('.' + formID + '.form-control-value').val('');
    $('.' + formID + '.form-control-value-int').val('0');
    $('.' + formID + '.form-control-bool').val('false');
    $('.' + formID + '.form-control-bool-true').val('true');
    $('.' + formID + '.form-control-value-int-null').val('null');


    var strSelect = $('.' + formID + '.form-control-value[data-rai-select]');
    for (var i = 0; i < strSelect.length; i++) {
        var valSelect = $(strSelect[i]).find('option[selected]').val();
        RaiSelectOption(valSelect, $(strSelect[i]).attr('data-rai-select'));
    }
    var strSelect = $('.' + formID + '.form-control-value-int[data-rai-select]');
    for (var i = 0; i < strSelect.length; i++) {
        var valSelect = $(strSelect[i]).find('option[selected]').val();
        RaiSelectOption(valSelect, $(strSelect[i]).attr('data-rai-select'));
    }
    var strSelect = $('.' + formID + '.form-control-value-int-null[data-rai-select]');
    for (var i = 0; i < strSelect.length; i++) {
        var valSelect = $(strSelect[i]).find('option[selected]').val();
        RaiSelectOption(valSelect, $(strSelect[i]).attr('data-rai-select'));
    }
    
    
    if (submit)
        $('#' + formID).submit();
}
function RaiSearchFormCheckHasFilter(formID) {
    $('#' + formID + ' > #HasFilter').val('false');
    $('.' + formID + '.form-control-value').each(function () {
        if ($(this).val() != undefined && $(this).val() != null && $(this).val() != 'null' && $(this).val() != '') {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
    $('.' + formID + '.form-control-value-int').each(function () {
        if ($(this).val() != undefined && $(this).val() != null && $(this).val() != 'null' && $(this).val() > 0) {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
    $('.' + formID + '.form-control-value-int-null').each(function () {
        if ($(this).val() != undefined && $(this).val() != null && $(this).val() != 'null') {
            $('#' + formID + ' > #HasFilter').val('true');
            return false;
        };
    });
}
function RaiSearchFormPreSubmit(destBox) {
    $("#" + destBox).addClass("rai-loader");
}

function RaiSearchFormSuccessSubmit(formId, destBox) {
    $("#" + destBox).removeClass("rai-loader");
}

function RaiSortData(anchor) {
    var tagName = $(anchor).data('order-name');
    var selector = $(anchor).data('order-container');

    var oldDir = $(anchor).find('i').attr('data-order-dir');
    var dir = oldDir == 'down' ? 'up' : 'down';

    var container = $(selector);
    for (var i = 0; i < container.length; i++) {
        $(container[i]).addClass('rai-loader');

        var result = $(container[i]).find('[data-order-item][data-' + tagName + ']').sort(function (a, b) {
            if (dir == 'up')
                return $(a).attr('data-' + tagName) < $(b).attr('data-' + tagName) ? 1 : -1;
            else
                return $(a).attr('data-' + tagName) > $(b).attr('data-' + tagName) ? 1 : -1;

        });
        $(container[i]).html('');
        $(container[i]).append(result);

        $(container[i]).removeClass('rai-loader');
    }
    $(anchor).find('i').attr('data-order-dir', dir);

    var group = $(anchor).data('order-group');
    if (group != '') {
        $('[data-order-group="' + group + '"]:not([data-order-name="' + tagName + '"])>i').attr('data-order-dir', '');
    }
}

$('div.modal').on('shown.bs.modal', function () {
    $('html')[0].style.setProperty("overflow-y", "hidden", "important");
    $('html')[0].style.setProperty("padding-right", "17px");

    //$('.btn-action-icon-switch > .btn-action-icon').on('click', function () {
    //    $(this).parent().find('.btn-action-icon').removeClass('active');
    //    $(this).addClass('active');
    //});
});

$('div.modal').on('hide.bs.modal', function () {
    $('html')[0].style.setProperty("overflow-y", "");
    $('html')[0].style.setProperty("padding-right", "");
});

$(document).on('click', '.btn-action-icon-switch > .btn-action-icon', function () {
    $(this).parent().find('.btn-action-icon').removeClass('active');
    $(this).addClass('active');
});

function RaiChangeView(button) {
    var container = $(button).data('view-container');
    var toShow = $(button).data('view-show');

    $('[data-view-container=' + container + '][data-view-block]').hide();
    $('[data-view-container=' + container + '][data-view-block=' + toShow + ']').show();

    $(button).blur();
}

function _RaiSelectIdentifier(selectId) {
    try {
        let internalid = selectId;
        if (internalid.substr(0, 1) == '#')
            internalid = internalid.substr(1);

        let control = $('#' + internalid);
        if (control.hasClass('rai-select-container'))
            return internalid;
        else if (control[0].hasAttribute('data-rai-select'))
            return control.data('rai-select');
        else
            return selectId;
    }
    catch (err) {
        return selectId;
    }
}

function RaiSelectToggle(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    var isReadOnly = $('#' + selectId).attr('data-rai-select-readonly');
    if (isReadOnly == "true")
        return;

    var isExpanded = $('#' + selectId + ' .rai-select').attr('aria-expanded');
    if (isExpanded == 'true') {
        RaiSelectClose(selectId);
    } else {
        RaiSelectOpen(selectId);
    }
}

function RaiSelectFocusOut() {
    var select = $('.rai-select[aria-expanded="true"]').parent()[0];
    if (!select.contains(event.target)) {
        RaiSelectClose($(select).attr('id'));
    }
}

function RaiSelectClose(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    document.body.removeEventListener('click', RaiSelectFocusOut);
    $('#popover-' + selectId).hide();
    $('#popover-' + selectId).removeClass('rai-select-popover-top');
    $('#' + selectId + ' .rai-select').attr('aria-expanded', false);
}
function RaiSelectOpen(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    $('.rai-select[aria-expanded="true"]+.rai-select-popover').hide();
    $('.rai-select[aria-expanded="true"]').attr('aria-expanded', false);

    var winHeight = window.innerHeight;
    var selHeight = $('#' + selectId + ' .rai-select')[0].getBoundingClientRect().bottom;

    if (winHeight - selHeight < 250) {
        $('#popover-' + selectId).addClass('rai-select-popover-top');
    }


    $('#popover-' + selectId).show();
    $('#' + selectId + ' .rai-select').attr('aria-expanded', true);

    $('#popover-' + selectId + ' .rai-select-search input').focus();

    document.body.addEventListener('click', RaiSelectFocusOut);

    if ($('#popover-' + selectId + ' .rai-select-option[aria-selected="true"]').length > 0) {
        $('#popover-' + selectId + ' .rai-select-option[aria-selected="true"]')[0].scrollIntoView({ behavior: 'instant', block: 'nearest', inline: 'start' });
    }
}

function RaiSelectAzzera(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    let element = $('#' + selectId).find('.rai-select-option[data-option-value]:first');
    RaiSelectOption(element.data('option-value'), selectId, false, true);
}

function RaiSelectOption(elemValue, selectId, clickParent, preventOnChange) {
    selectId = _RaiSelectIdentifier(selectId);

    var element = $('#' + selectId).find('.rai-select-option[data-option-value="' + elemValue + '"]');
    var isSelected = $(element).attr('aria-selected');
    var isMulti = $('#' + selectId).attr('data-rai-select-multiple') == "true";

    if (!preventOnChange)
        preventOnChange = false;

    if (isSelected == "true") {
        $(element).attr('aria-selected', 'false');
        var select = $('[data-rai-select="' + selectId + '"]');

        if (!isMulti) {
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-placeholder', 'true');
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value', '');
            $('#' + selectId + ' .rai-select .rai-select-value span').html($('#' + selectId).data('rai-select-placeholder'));
        }
        else {
            var unselNode = $('#' + selectId + ' .rai-select .rai-select-value span[data-sel-value="' + elemValue + '"]');
            unselNode.remove();
        }

        if (isMulti) {
            $(element).find('.rai-checkbox input[type="checkbox"]').prop('checked', false).removeAttr('checked');
            $(select).find('option[value="' + $(element).attr('data-option-value') + '"]').prop('selected','');
        } else {
            $(select).html('<option value=""></option>');
        }
    }
    else {
        if (!isMulti) {
            $(element).parent().find('.rai-select-option').attr('aria-selected', 'false');
        }

        $(element).attr('aria-selected', 'true');
        var select = $('[data-rai-select="' + selectId + '"]');

        if (!isMulti) {
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-placeholder', 'false');
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value', $(element).data('option-value'));
            $('#' + selectId + ' .rai-select .rai-select-value span').html($(element).data('search-text'));
        }
        else {
            
            var newSpan = '<span data-placeholder="false" data-sel-value="' + $(element).attr('data-option-value') + '"  data-toggle="tooltip" data-html="true"  title="' + $(element).attr('data-search-text') + '">' + $(element).attr('data-search-text') + '</span>';
            $('#' + selectId + ' .rai-select .rai-select-value').append(newSpan);
        }

        if (isMulti) {
            //$(select).append('<option value="' + $(element).data('option-value') + '" selected>' + $(element).data('search-text') + '</option>');
            $(select).find('option[value="' + $(element).attr('data-option-value') + '"]').prop('selected','selected');
            $(element).find('.rai-checkbox input[type="checkbox"]').prop('checked', true).attr('checked','checked');
        }
        else {
            $(select).html('<option value="' + $(element).data('option-value') + '" selected>' + $(element).data('search-text') + '</option>');
        }
    }

    if (!isMulti && (clickParent == undefined || clickParent === true))
        $('#' + selectId + ' .rai-select').click();

    if (isMulti) {
        if ($('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="false"]').length > 0)
            $('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="true"]').hide();
        else
            $('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="true"]').show();
    }

    $('#' + selectId + ' .rai-select').focus();

    var actionOnChange = $('#' + selectId).attr('data-rai-select-onchange');
    if (!preventOnChange && actionOnChange != undefined && actionOnChange != '') {
        var func = new Function(actionOnChange);
        func();
    }

    $('[data-toggle="tooltip"]').tooltip();
}

function RaiSelectExchangePopoverModal(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    var modalInternal = $('#modal-rai-search-select-internal');
    var visibleElems = $('#popover-' + selectId + ' .rai-select-option');
    $(modalInternal).find('#table-' + selectId + '>tbody [data-search][data-search-text]').remove();

    var inputPop = $('input[type="text"][data-search="search-' + selectId + '"]');
    var inputModal = $(modalInternal).find('input[type="text"][data-search="search-modal-' + selectId + '"][data-search-attr="text"]');
    var asyncSearch = $(inputModal).data('search-async');
    if (!asyncSearch) {
        if (visibleElems.length > 0) {
            $(modalInternal).find('[data-search="search-modal-' + selectId + '"][data-search-role="lessinput"]').hide();
            for (var i = 0; i < visibleElems.length; i++) {
                var item = visibleElems[i];
                $('#table-' + selectId + '>tbody').append(
                    RaiSelectModalMakeTr(selectId, $(item).data('option-value'), $(item).data('search-text'), i, $(item).attr('aria-selected'))
                );
            }
        } else {
            $(modalInternal).find('[data-search="search-modal-' + selectId + '"][data-search-role="lessinput"]').show();

        }
    }

    $(inputModal).val('');//$(inputPop).val());
    $(inputModal).attr('data-search-previous', '');//$(inputPop).data('search-previous'));

    var asyncSearch = $(inputModal).data('search-async');
    if (asyncSearch) {
        var groupSearch = $(inputModal).data('search');
        var urlSearch = $(inputModal).data('search-url');
        var optionCont = $(inputModal).data('search-container');
        var optionStyle = $(inputModal).data('search-style');
        var parameterFunction = $(inputModal).attr('data-search-func-param');
        var dataGroup = "[data-search='" + groupSearch + "']";
        var dataAttr = '[data-search="text"]';
        var parameters = { filter: '' };
        if (parameterFunction != undefined && parameterFunction != "") {
            var func = new Function("return " + parameterFunction);
            parameters = func();
            parameters.filter = '';
        }

        RaiSelectLoadAsyncData(selectId, urlSearch, parameters, optionCont, dataGroup, dataAttr, optionStyle, inputModal);
    }


    RaiSelectSearch($(inputModal), 'search-modal-' + selectId, 'text', selectId, false, true);
}
function RaiSelectExchangeModalPopover(selectId) {
    selectId = _RaiSelectIdentifier(selectId);

    var modalInternal = $('#modal-rai-search-select-internal');
    var visibleElems = $(modalInternal).find('#table-' + selectId + ' [data-search][data-search-text]');
    $('#popover-' + selectId + ' .rai-select-options .rai-select-option').remove();


    var inputPop = $('input[type="text"][data-search="search-' + selectId + '"]');
    var inputModal = $(modalInternal).find('input[type="text"][data-search="search-modal-' + selectId + '"][data-search-attr="text"]');
    var asyncSearch = $(inputModal).data('search-async');

    if (visibleElems.length > 0) {
        $('[data-search="search-' + selectId + '"][data-search-role="lessinput"]').hide();
        for (var i = 0; i < visibleElems.length; i++) {
            var item = visibleElems[i];
            $('#popover-' + selectId + ' .rai-select-options').append(
                RaiSelectPopoverMakeOption(selectId, $(item).find('[data-option-value]').data('option-value'), $(item).data('search-text'), false)
            );
        }
    } else {
        $('[data-search="search-modal-' + selectId + '"][data-search-role="lessinput"]').show();
    }


    //$(inputPop).val($(inputModal).val());
    $(inputPop).attr('data-search-previous', $(inputModal).data('search-previous'));
    RaiSelectSearch($(inputPop), 'search-' + selectId, 'text', selectId);
}

function RaiSelectModalMakeTr(selectId, optionValue, optionText, optionCounter, checked) {
    var stringresult = '';
    selectId = _RaiSelectIdentifier(selectId);

    //A differenza del Select semplice, i campi qui vanno sempre separati
    var showCode = $('#' + selectId).attr('data-rai-select-show-value');
    var showText = optionText;
    var optText = optionText;
    if (showCode == 'true') {
        if (optionText.startsWith(optionValue)) {
            showText = showText.substr((optionValue + ' - ').length);
        } else {
            optText = optionValue + " - " + optionText;
        }
    }

    stringresult += '<tr data-search="search-modal-' + selectId + '" data-search-value="' + optionValue + '" data-search-text="' + optText + '">';
    stringresult += '<td><div class="rai-radio">';
    stringresult += '<input type="radio" name="' + selectId + '" data-option-value="' + optionValue + '" id="radio-' + optionCounter + '-' + selectId + '" ' + (checked == 'true' ? 'checked' : '') + '/>';
    stringresult += '<label for="radio-' + optionCounter + '-' + selectId + '">' + optionValue + '</label>';
    stringresult += '</div></td>';
    stringresult += '<td>' + showText + '</td>';
    stringresult += "</tr>";

    return stringresult;
}
function RaiSelectPopoverMakeOption(selectId, optionValue, optionText, optionSelected) {
    var stringresult = '';
    selectId = _RaiSelectIdentifier(selectId);
    var showCode = $('#' + selectId).attr('data-rai-select-show-value');
    var tipoFiltro = $('#' + selectId).attr('data-rai-select-filter');

    stringresult += '<div class="rai-select-option"';
    stringresult += ' onclick="RaiSelectOption(\'' + optionValue + '\', \'' + selectId + '\')"';
    if (optionSelected === true) {
        stringresult += ' aria-selected="true" ';
    } else {
        stringresult += ' aria-selected="false" ';
    }
    stringresult += ' data-option-value="' + optionValue + '" ';
    stringresult += ' data-toggle="tooltip" data-html="true" title="' + optionText + '" ';
    stringresult += ' data-search="search-' + selectId + '" ';

    var optText = '';
    switch (tipoFiltro) {
        case 'StartsWith':
        case 'Contains':
            optText = optionText;
            break;
        case 'ValueStartsWith':
        case 'ValueContains':
            optText = optionValue;
            break;
        case 'AllStartsWith':
        case 'AllContains':
            if (optionText.indexOf(optionValue + ' - ') === -1 && showCode === 'true') {
                optText = optionValue + " - " + optionText;
            } else {
                optText = optionText;
            }
            break;
        default:
    }
    stringresult += ' data-search-text="' + optText + '">';
    stringresult += '<span>' + (showCode === 'true' && optionText.indexOf(optionValue + ' - ') ===  -1 ? optionValue + ' - ' : '') + optionText + '</span>';
    stringresult += "</div>";

    $('[data-toggle="tooltip"]').tooltip();

    return stringresult;
}

function RaiSelectModalOpen(selectId, isAsync) {
    selectId = _RaiSelectIdentifier(selectId);

    var modalInternal = $('#modal-rai-search-select-internal');

    $(modalInternal).html($('#modal-' + selectId).html());

    if (!isAsync) {
        var selectedElem = $('#popover-' + selectId + ' .rai-select-option[aria-selected="true"]');
        if (selectedElem.length > 0) {
            $(selectedElem).each(function () {
                $(modalInternal).find('[data-option-value="' + $(this).data('option-value') + '"]').attr('checked', 'checked');
            })
        }
    } else {
        RaiSelectExchangePopoverModal(selectId)
    }

    $('#' + selectId + ' .rai-select').click();
    $('#modal-rai-search-select').modal('show');
}

const validateEmailCustom = (email) => {
    return email.match(
        /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
    );
};

function RaiSelectAddEmailCustom(itemId, selectId, idEmail) {
    try {
        var _selectId = _RaiSelectIdentifier(selectId);
        //debugger;

        var tx = $('#' + idEmail).val();
        if (!validateEmailCustom(tx)) {
            swal("Oops", "Mail non corretta", "error");
        }
        else {
            var optionListUfficio = $('#' + itemId);
            optionListUfficio.append('<option value="' + tx + '">' + tx + '</option>');

            var optionList = $('#options-' + _selectId);

            optionList.append('<div class="rai-select-option" data-rai-select-multiple="true" aria-selected="false" data-option-value="' + tx + '" onclick="RaiSelectOption(\'' + tx + '\', \'' + _selectId + '\')" data-search="search-' + _selectId + '" data-search-text=\"' + tx + '\"><div class="rai-checkbox"><input type="checkbox" id="email-' + _selectId + '" checked="checked"><label forx="email-' + _selectId + '">' + tx + '</label></div></div >');

            RaiSelectOption(tx, _selectId);
        }
    } catch (e) {
        Console.log("Errore in RaiSelectAddEmailCustom -> " + e);
    }

}



function RaiSelectModalClose(selectId, apply, isAsync) {
    selectId = _RaiSelectIdentifier(selectId);

    if (apply) {
        var modalInternal = $('#modal-rai-search-select-internal');

        if (isAsync) {
            RaiSelectExchangeModalPopover(selectId);
        }
        var selValue = $(modalInternal).find('#table-' + selectId + ' [data-option-value]:checked').data('option-value');
        if (selValue != undefined)
            //RaiSelectOption($('#popover-' + selectId + ' .rai-select-option[data-option-value="' + selValue + '"]')[0], selectId, false);
            RaiSelectOption(selValue, selectId, false);
    }

    $('#modal-rai-search-select').modal('hide');
    $('#modal-rai-search-select-internal').html('');
}

function RaiSelectSearch(input, groupSearch, attribute, selectId, checkEscape, fromModal) {
    selectId = _RaiSelectIdentifier(selectId);

    if (checkEscape && event.keyCode == 27) {
        RaiSelectToggle(selectId);
        return;
    }

    var asyncSearch = $(input).data('search-async');

    if (!fromModal) {
        if (asyncSearch) {
            var valueRif = $(input).val();
            var dataGroup = "[data-search='" + groupSearch + "']";
            var dataAttr = "[data-search-" + attribute + "]";

            var minInput = $(input).data('search-mininput');
            var urlSearch = $(input).data('search-url');
            var optionCont = $(input).data('search-container');
            var optionStyle = $(input).data('search-style');

            if (valueRif.length < minInput) {
                $(dataGroup + '[data-search-role="lessinput"]').show();
                $(dataGroup + dataAttr).hide();
            } else {
                $(dataGroup + '[data-search-role="lessinput"]').hide();
                var dataPrecSearch = $(input).data('search-previous');
                if (dataPrecSearch != '' && valueRif.startsWith(dataPrecSearch)) {
                    asyncSearch = false;
                }
                else {
                    var parameterFunction = $(input).attr('data-search-func-param');
                    var parameters = { filter: valueRif };
                    if (parameterFunction != undefined && parameterFunction != "") {
                        let func = new Function("return " + parameterFunction);
                        parameters = func();
                        parameters.filter = valueRif;
                    }
                    RaiSelectLoadAsyncData(selectId, urlSearch, parameters, optionCont, dataGroup, dataAttr, optionStyle, input);
                }
            }
        }

        if (!asyncSearch) {
            RaiSearchData(input, groupSearch, attribute);
        }
    } else {

        var modalCont = '#modal-rai-search-select-internal';

        RaiSearchDataMulti(input, groupSearch, modalCont);
    }
}

function RaiSelectExtLoadAsyncData(modelId, urlSearch, parameters, asyncCall, afterLoadFunc) {
    let selectId = _RaiSelectIdentifier(modelId);

    var optionsCont = 'options-' + selectId;
    var groupSearch = 'search-' + selectId;
    var dataGroup = "[data-search='" + groupSearch + "']";
    var dataAttr = '[data-search-text]';
    
    RaiSelectOption($('#' + modelId).val(), selectId, false);
    RaiSelectLoadAsyncData(selectId, urlSearch, parameters, optionsCont, dataGroup, dataAttr, 'option', null, asyncCall, afterLoadFunc);
}

function DEM_RaiSelectExtLoadAsyncData(valoreDefault, modelId, urlSearch, parameters, asyncCall, afterLoadFunc) {
    let selectId = _RaiSelectIdentifier(modelId);
    var optionsCont = 'options-' + selectId;
    var groupSearch = 'search-' + selectId;
    var dataGroup = "[data-search='" + groupSearch + "']";
    var dataAttr = '[data-search-text]';
    //RaiSelectOption($('#' + modelId).val(), selectId, false);
    DEM_RaiSelectLoadAsyncData(valoreDefault, selectId, urlSearch, parameters, optionsCont, dataGroup, dataAttr, 'option', null, asyncCall, afterLoadFunc);
}


function DEM_RaiSelectLoadAsyncData(valoreDefault, selectId, urlSearch, parameters, optionCont, dataGroup, dataAttr, optionStyle, input, asyncCall, afterLoadFunc) {
    selectId = _RaiSelectIdentifier(selectId);

    var _async = false;
    if (asyncCall !== undefined)
        _async = asyncCall;

    $('#' + optionCont).addClass('rai-loader');
    $.ajax({
        async: _async,
        url: urlSearch,
        type: "GET",
        dataType: "json",
        data: parameters,
        cache: false,
        success: function (data) {
            //var selValue = $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value');
            $('#' + optionCont + ' ' + dataGroup + dataAttr).remove();

            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                var newOption = "";
                if (optionStyle === 'option') {
                    newOption = RaiSelectPopoverMakeOption(selectId, item.Value, item.Text, item.Selected);
                }
                else if (optionStyle === 'tablerow') {
                    newOption += RaiSelectModalMakeTr(selectId, item.Value, item.Text, i, item.Selected);
                }
                $('#' + optionCont).append(newOption);
            }

            if (input !== undefined && input !== null) {
                var valueRif = $(input).val();
                $(input).data('search-previous', valueRif);
            }
        },
        error: function (a, b, c) {
            swal({
                title: "Ops...",
                text: "Si è verificato un errore imprevisto\n" + c,
                type: 'error',
                customClass: 'rai'
            });
        },
        complete: function () {
            $('#' + optionCont).removeClass('rai-loader');

            if (afterLoadFunc !== undefined)
                afterLoadFunc();
        }
    });
}

function DEM_RaiSelectOption(elemValue, selectId, clickParent, preventOnChange) {
    selectId = _RaiSelectIdentifier(selectId);

    var element = $('#' + selectId).find('.rai-select-option[data-option-value="' + elemValue + '"]');
    //var isSelected = $(element).attr('aria-selected');
    var isSelected = false;
    var isMulti = $('#' + selectId).attr('data-rai-select-multiple') == "true";

    if (!preventOnChange)
        preventOnChange = false;

    if (isSelected == "true") {
        $(element).attr('aria-selected', 'false');
        var select = $('[data-rai-select="' + selectId + '"]');

        if (!isMulti) {
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-placeholder', 'true');
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value', '');
            $('#' + selectId + ' .rai-select .rai-select-value span').html($('#' + selectId).data('rai-select-placeholder'));
        }
        else {
            var unselNode = $('#' + selectId + ' .rai-select .rai-select-value span[data-sel-value="' + elemValue + '"]');
            unselNode.remove();
        }

        if (isMulti) {
            $(element).find('.rai-checkbox input[type="checkbox"]').prop('checked', false).removeAttr('checked');
            $(select).find('option[value="' + $(element).attr('data-option-value') + '"]').prop('selected', '');
        } else {
            $(select).html('<option value=""></option>');
        }
    }
    else {
        if (!isMulti) {
            $(element).parent().find('.rai-select-option').attr('aria-selected', 'false');
        }

        $(element).attr('aria-selected', 'true');
        var select = $('[data-rai-select="' + selectId + '"]');

        if (!isMulti) {
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-placeholder', 'false');
            $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value', $(element).data('option-value'));
            $('#' + selectId + ' .rai-select .rai-select-value span').html($(element).data('search-text'));
        }
        else {

            var newSpan = '<span data-placeholder="false" data-sel-value="' + $(element).attr('data-option-value') + '"  data-toggle="tooltip" data-html="true"  title="' + $(element).attr('data-search-text') + '">' + $(element).attr('data-search-text') + '</span>';
            $('#' + selectId + ' .rai-select .rai-select-value').append(newSpan);
        }

        if (isMulti) {
            //$(select).append('<option value="' + $(element).data('option-value') + '" selected>' + $(element).data('search-text') + '</option>');
            $(select).find('option[value="' + $(element).attr('data-option-value') + '"]').prop('selected', 'selected');
            $(element).find('.rai-checkbox input[type="checkbox"]').prop('checked', true).attr('checked', 'checked');
        }
        else {
            $(select).html('<option value="' + $(element).data('option-value') + '" selected>' + $(element).data('search-text') + '</option>');
        }
    }

    if (!isMulti && (clickParent == undefined || clickParent === true))
        $('#' + selectId + ' .rai-select').click();

    if (isMulti) {
        if ($('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="false"]').length > 0)
            $('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="true"]').hide();
        else
            $('#' + selectId + ' .rai-select .rai-select-value span[data-placeholder="true"]').show();
    }

    $('#' + selectId + ' .rai-select').focus();

    var actionOnChange = $('#' + selectId).attr('data-rai-select-onchange');
    if (!preventOnChange && actionOnChange != undefined && actionOnChange != '') {
        var func = new Function(actionOnChange);
        func();
    }

    $('[data-toggle="tooltip"]').tooltip();
}







function RaiSelectOptionSelector(modelId, filterValue) {
    //var selectId = $('#' + modelId).parent().attr('id');
    let selectId = _RaiSelectIdentifier(modelId);

    let listObj = null;
    if (filterValue === undefined || filterValue === null ) {
        listObj = $('#' + selectId).find('.rai-select-option');
    } else {
        listObj = $('#' + selectId).find('.rai-select-option[data-option-value="' + filterValue + '"]');
    }

    return listObj;
}

function RaiSelectOptionAttr(modelId, filterValue, attrName, attrValue) {
    let listObj = RaiSelectOptionSelector(modelId, filterValue);

    if (attrValue === undefined || attrValue === null) {
        $(listObj).removeAttr(attrName);
        $('#' + modelId).removeAttr(attrValue);
    }
    else {
        $(listObj).attr(attrName, attrValue);
        $('#' + modelId).attr(attrName, attrValue);
    }
}

function RaiSelectOptionAddClass(modelId, filterValue, classValue) {
    let listObj = RaiSelectOptionSelector(modelId, filterValue);
    $(listObj).addClass(classValue);
    $('#' + modelId).addClass(classValue);
}

function RaiSelectOptionRemoveClass(modelId, filterValue, classValue) {
    let listObj = RaiSelectOptionSelector(modelId, filterValue);
    $(listObj).removeClass(classValue);
    $('#' + modelId).removeClass(classValue);
}

function RaiSelectLoadAsyncData(selectId, urlSearch, parameters, optionCont, dataGroup, dataAttr, optionStyle, input, asyncCall, afterLoadFunc) {
    selectId = _RaiSelectIdentifier(selectId);

    var _async = false;
    if (asyncCall != undefined)
        _async = asyncCall;

    $('#' + optionCont).addClass('rai-loader');
    $.ajax({
        async: _async,
        url: urlSearch,
        type: "GET",
        dataType: "json",
        data: parameters,
        cache: false,
        success: function (data) {
            var selValue = $('#' + selectId + ' .rai-select .rai-select-value span').attr('data-sel-value');
            $('#' + optionCont + ' ' + dataGroup + dataAttr).remove();            
            for (var i = 0; i < data.length; i++) {
                var item = data[i];
                var newOption = "";
                if (optionStyle == 'option') {
                    newOption = RaiSelectPopoverMakeOption(selectId, item.Value, item.Text, selValue == item.Value);
                }
                else if (optionStyle == 'tablerow') {
                    newOption += RaiSelectModalMakeTr(selectId, item.Value, item.Text, i, selValue == item.Value);
                }
                $('#' + optionCont).append(newOption);
            }

            if (input != undefined && input != null && typeof input !== "undefined" && input !== "") {
                var valueRif = $(input).val();
                $(input).data('search-previous', valueRif);
            }

            if (afterLoadFunc != undefined)
                afterLoadFunc();
        },
        error: function (a, b, c) {
            swal({
                title: "Ops...",
                text: "Si è verificato un errore imprevisto\n" + c,
                type: 'error',
                customClass: 'rai'
            })
        },
        complete: function () {
            $('#' + optionCont).removeClass('rai-loader');
        }
    });
}
function ShowHideOrarioSelect() {
    //alert($('#orarioSelect').find('.rai-select-option').length);
    let selectId = _RaiSelectIdentifier('orarioSelect');
    var optionsCont = 'options-' + selectId;
    if ($('#' + optionsCont).find('.rai-select-option').length > 1) {
        $('#oreTipoMinimo').show();
    } else {
        $('#oreTipoMinimo').hide();
    }
}

function RaiSelectLoadDefaultValue(selectId, urlAction, selValue) {
    selectId = _RaiSelectIdentifier(selectId);

    $.ajax({
        async: false,
        url: urlAction,
        type: "GET",
        dataType: "json",
        data: { filter: '', value: selValue },
        cache: false,
        success: function (data) {
            if (data.length > 0) {
                let item = data[0];
                if ($('#' + selectId + ' .rai-select-options').find('[data-option-value="' + item.Value + '"]').length == 0) {
                    let newOption = RaiSelectPopoverMakeOption(selectId, item.Value, item.Text, false);
                    $('#' + selectId + ' .rai-select-options').append(newOption);
                }

                if ($('#' + selectId + ' .rai-select-options').find('[data-option-value="' + item.Value + '"][aria-selected="true"]').length == 0)
                    RaiSelectOption(item.Value, selectId, false);
            }
        },
        error: function (a, b, c) {
            swal({
                title: "Ops...",
                text: "Si è verificato un errore imprevisto\n" + c,
                type: 'error'
            })
        }
    });
}
function RaiSelectClear(selectId) {
    let _lSelectId = _RaiSelectIdentifier(selectId);

    $("#" + _lSelectId).find('option[selected]').each(function () {
        let valSelect = $(this).val();
        if (valSelect != undefined)
            RaiSelectOption(valSelect, _lSelectId);
    });
    
    let input = $('#' + _lSelectId + ' .rai-select-search input');
    if ($(input).attr('data-search-async') == 'true') {
        $("#" + _lSelectId + ' [data-search="search-' + _lSelectId + '"][data-search-text]').remove();
        $(input).data('search-previous', '');
        $(input).val('');
    }
}

function RaiOPNavGoToNext(idCont, newPageId, newPageTitle, url, parameters, ajaxType, asSubmitForm, onLoaded) {
    $(window).scrollTop(0);
    //aggiungo il breadcrumb
    var hashIdCont = '#' + idCont;

    if ($(hashIdCont + ' .rai-op-breadcrumb span[data-op-path="sub"][data-target="' + newPageId + '"]').length == 0) {
        $(hashIdCont + ' .rai-op-breadcrumb').append(
            '<span class="fa fa-chevron-right" data-op-path="sub" data-target="' + newPageId + '" data-op-arrow=""></span><span data-op-path="sub" data-target="' + newPageId + '" onclick="RaiOPGotoThisPage(\'' + idCont + '\',\'' + newPageId + '\')">' + newPageTitle + '</span>'
        );
    } else {
        $(hashIdCont + ' .rai-op-breadcrumb span[data-op-path="sub"][data-target="' + newPageId + '"][onclick]').text(newPageTitle);
    }

    $(hashIdCont + ' .rai-op-breadcrumb').show();

    $(hashIdCont + ' .rai-op-main').hide();
    $(hashIdCont + ' .rai-op-sub').show();


    if ($('.rai-op-sub-page#' + newPageId).length > 0) {
        $('.rai-op-sub-page#' + newPageId).remove();
    }

    $(hashIdCont + ' .rai-op-sub').append(
        '<div class="rai-op-sub-page" id="' + newPageId + '"></div>'
    );

    asSubmitForm = asSubmitForm || false;
    RaiUpdateWidget(newPageId, url, 'html', parameters, asSubmitForm, onLoaded, null, ajaxType);
}

function RaiOPGotoMain(idCont, removePage) {
    var hashIdCont = '#' + idCont;

    $(hashIdCont + ' .rai-op-breadcrumb').hide();

    if (removePage == undefined || removePage == true) {
        $(hashIdCont + ' .rai-op-breadcrumb').find('span[data-op-path="sub"]').remove();
        $(hashIdCont + ' .rai-op-sub').find('.rai-op-sub-page').remove();
    } else {
        $(hashIdCont + ' .rai-op-breadcrumb').find('span[data-op-path="sub"]').hide();
        $(hashIdCont + ' .rai-op-sub').find('.rai-op-sub-page').hide();
    }


    $(hashIdCont + ' .rai-op-sub').hide();
    $(hashIdCont + ' .rai-op-main').show();

}

function RaiOPGotoThisPage(idCont, id, removePage, action) {
    var hashIdCont = '#' + idCont;
    var breadLen = $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]').length;
    var indThis = $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]').index($(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"][data-target="' + id + '"]'));

    for (var i = breadLen - 1; i > indThis + 1; i--) {
        if (removePage == undefined || removePage == true) {
            $($(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]')[i]).remove();
        } else {
            $($(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]')[i]).hide();
        }
    }

    var pageLen = $(hashIdCont + ' .rai-op-sub .rai-op-sub-page').length;
    var indPage = $(hashIdCont + ' .rai-op-sub .rai-op-sub-page').index($(hashIdCont + ' .rai-op-sub .rai-op-sub-page[id="' + id + '"]'));

    for (var i = pageLen - 1; i > indPage; i--) {
        if (removePage == undefined || removePage == true) {
            $($(hashIdCont + ' .rai-op-sub .rai-op-sub-page')[i]).remove();
        } else {
            $($(hashIdCont + ' .rai-op-sub .rai-op-sub-page')[i]).hide();
        }
    }

    if (removePage == false) {
        for (var i = 0; i < indPage; i++) {
            $($(hashIdCont + ' .rai-op-sub .rai-op-sub-page')[i]).hide();
        }
    }

    $(hashIdCont + ' .rai-op-breadcrumb').show();

    $(hashIdCont + ' .rai-op-main').hide();
    $(hashIdCont + ' .rai-op-sub').show();

    $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"][data-target="' + id + '"]').show();
    $(hashIdCont + ' .rai-op-sub .rai-op-sub-page[id="' + id + '"]').show();

    if (action != undefined) {
        action();
    }
}

function RaiOPRemovePage(idCont, id, removePage, action) {
    var hashIdCont = '#' + idCont;
    var breadLen = $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]').length;

    if (breadLen == 1) {
        RaiOPGotoMain(idCont, removePage);
    }
    else {
        var indThis = $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]').index($(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"][data-target="' + id + '"]'));
        var prevPage = $(hashIdCont + ' .rai-op-breadcrumb [data-op-path="sub"]:eq(' + (indThis - 1) + ')').attr('data-target');
        RaiOPGotoThisPage(idCont, prevPage, removePage, action);
    }
}

function RaiClickOnKeyUp(idButton) {
    if (event.keyCode == 13) {
        $('#' + idButton).click();
    }
}

function RaiUploadClick(fileElem) {
    event.preventDefault();
    $('#' + fileElem).find('[type="file"]').click();
}

function RaiUploadOnChange(fileElem, isAsync, emptyLabel) {
    var files = $('#' + fileElem).find('[type="file"]')[0].files;

    if (files.length == 1) {

        var obj = $('#' + fileElem).find('[type="file"]')[0].files[0];

        if (isAsync === true) {
            var action = $('#' + fileElem).attr('data-uploader-url');

            $('#' + fileElem).find('[data-uploader-loading]').show();
            $('#' + fileElem).find('[data-uploader-file]').text("Caricamento in corso...");
            $('#' + fileElem).find('[data-uploader-file]').addClass("neutrals-md-40-color");

            var formData = new FormData();
            formData.append($('#' + fileElem).find('[type="file"]').attr('id'), obj);

            $.ajax({
                url: action,
                async: true,
                type: "POST",
                dataType: "html",
                contentType: false,
                processData: false,
                data: formData,
                cache: false,
                success: function (data) {
                    RaiUploadOnChangeSuccess(fileElem, files);
                    $('#' + fileElem).find('[data-uploader-loading]').hide();
                    $('#' + fileElem).find('[data-uploader-file]').removeClass("neutrals-md-40-color");
                },
                error: function () {
                    swal('ops');
                    return;
                }
            });
        } else {
            RaiUploadOnChangeSuccess(fileElem, files);
        }
    }
    else if (files.length > 1) {
        RaiUploadOnChangeSuccess(fileElem, files);
    }
    else {
        RaiUploadRemove(fileElem, emptyLabel);
    }
}

function RaiUploadOnChangeSuccess(fileElem, files) {
    if (files.length == 1) {
        var obj = files[0];
        $('#' + fileElem).find('[data-uploader-file]').text(obj.name);
        $('#' + fileElem).find('[data-uploader-file]').attr('title', obj.name);
        $('#' + fileElem).find('[data-uploader-remove]').show();
    } else {

        var listSpanFile = ""
        for (var i = 0; i < files.length; i++) {
            listSpanFile += '<span><span title="Elimina file" class="fa fa-times cursor-pointer" onclick="RaiUploadRemoveSingle(\'' + fileElem + '\',' + i + ')"></span> ' + files[i].name + '</span>';
        }

        $('#' + fileElem).find('[data-uploader-file]').text('');
        $('#' + fileElem).find('[data-uploader-file]').append(listSpanFile);
    }



    //var icon = "";
    //var content = "";
    //if (obj.type.includes("audio")) {
    //    icon = 'icon icon-music-tone-alt';
    //    content = 'File audio';
    //}
    //else if (obj.type.includes("video")) {
    //    icon = 'icon icon-film';
    //    content = 'File Video';
    //}
    //else if (obj.type.includes('image')) {
    //    icon = 'icon icon-picture';
    //    content = 'File immagine';
    //    //RaiUploadLoadPreview(fileElem);
    //}
    //else if (obj.type.includes('word')) {
    //    icon = 'icon icon-doc';
    //    content = 'File Word';
    //}
    //else if (obj.type.includes('excel')) {
    //    icon = 'icon icon-doc';
    //    content = 'File Excel';
    //}
    //else if (obj.type.includes('pdf')) {
    //    icon = 'icon icon-doc';
    //    content = 'File Pdf';
    //}
    //else {
    //    icon = 'icon icon-doc';
    //    content = 'File Generico (altro)';
    //}

    //$('#' + fileElem + '-iAddFile').hide();
    //$('#' + fileElem + '-iAddedFile').addClass(icon);
    //$('#' + fileElem + '-iAddedFile').show();

    //$('#' + fileElem).addClass('file-selected');
}

function RaiUploadRemove(fileElem, emptyLabel) {
    $('#' + fileElem).find('[type="file"]').val('');
    $('#' + fileElem).find('[data-uploader-file]').text(emptyLabel);
    $('#' + fileElem).find('[data-uploader-remove]').hide();

    $('#' + fileElem + '-iAddFile').show();
    $('#' + fileElem + '-iAddedFile').attr('');
    $('#' + fileElem + '-iAddedFile').hide();

    $('#' + fileElem).removeClass('file-selected');
}
function RaiUploadRemoveSingle(fileElem, fileIndex) {
    var files = $('#' + fileElem).find('[type="file"]')[0].files;

    const dt = new DataTransfer()

    for (var i = 0; i < files.length; i++) {
        if (i != fileIndex)
            dt.items.add(files[i]);
    }

    $('#' + fileElem).find('[type="file"]')[0].files = dt.files;

    $('#' + fileElem).find('[type="file"]').change();
}

function RaiUploadPreview(fileElem) {
    var obj = $('#' + fileElem).find('[type="file"]')[0].files[0];
    var obj_url = URL.createObjectURL(obj);
    window.open(obj_url, '_blank');
    URL.revokeObjectURL(obj_url);
}

function RaiUploadLoadPreview(fileElem) {
    var reader = new FileReader();
    reader.onload = function (e) {
        $('#' + fileElem).find('[data-uploader-preview]').attr('src', e.target.result);
    }
    reader.readAsDataURL($('#' + fileElem).find('[type="file"]')[0].files[0]);
}


function RaiSTAvvia() {
    $('#st-table-log tbody').html('');
    $('#st-table-counter tbody').html('');

    $('#st-btn-start').enable(false);
    $('#st-btn-stop').enable(true);

    var numProc = $('#st-num-proc').val();
    var numCallPerProc = $('#st-num-call-per-proc').val();

    if (numProc == 0 || numCallPerProc == 0) {
        swal("Inserire numeri validi", "Errore", "error");
        return;
    }


    for (var i = 0; i < numProc; i++) {
        $('#st-table-counter tbody').append('<tr><td>' + i + '</td><td id="row-proc-counter-' + i + '">0</td></tr>');
        var worker = new Worker('/assets/js/myRaiV2_worker.js');
        worker.addEventListener("message", function (event) {
            if (event.data.Esito) {
                RaiSTLog(event.data.IdProc, event.data.IdCall, 'success', event.data.Message, event.data.Durata);
            }
            else {
                RaiSTLog(event.data.IdProc, event.data.IdCall, 'error', event.data.Message, event.data.Durata);
            }
            var currentNumber = parseInt($('#row-proc-counter-' + event.data.IdProc).html()) + 1;
            $('#row-proc-counter-' + event.data.IdProc).html(currentNumber);
        });
        worker.postMessage("start_" + i + '_' + numCallPerProc);
    }

    var tempInt = setInterval(function () {
        for (var i = 0; i < numProc; i++) {
            var strOpe = parseInt($('#row-proc-counter-' + i).html());
            var strLen = strOpe.length;
            if (strOpe < numCallPerProc) {
                return;
            }
        }

        $('#st-btn-start').enable(true);
        $('#st-btn-stop').enable(false);

        clearInterval(tempInt);
    }, 1000);
}


function RaiSTLog(idProc, idCall, esito, message, durata) {
    var logrow = '<tr>' +
        '<td>' + (parseInt(idProc) + 1) + '</td>' +
        '<td>' + (parseInt(idCall) + 1) + '</td>' +
        '<td>' + esito + '</td>' +
        '<td>' + durata + '</td>' +
        '<td>' + message + '</td>' +
        '</tr>'

    $('#st-table-log tbody').append(logrow);
}

function RaiSTInterrompi() {
    $('#st-stop-flag').val('true');
    var numProc = $('#st-num-proc').val();
    var tempIntS = setInterval(function () {
        for (var i = 0; i < numProc; i++) {
            var strOpe = $('#row-proc-counter-' + i).text();
            if (strOpe.indexOf('#') < 0) {
                return;
            }
        }

        $('#st-btn-start').enable(true);
        $('#st-btn-stop').enable(false);
        $('#st-stop-flag').val('false');

        clearInterval(tempIntS);
    }, 1000);
}

//$('.rai-table-collapsable-header').on('click', function () {
$(document.body).on("click", '.rai-table-collapsable-header', function (e) {
    //Se ho un button all'interno della riga, non deve necessariamente triggerare l'header
    if (e.target.tagName.toUpperCase() == "BUTTON" || $(e.target).data('table-collapsable-toggle') == 'ignore' || $(e.target.parentElement).data('table-collapsable-toggle') == 'ignore') {
        return;
    }

    if (!$(this).hasClass('open') && $(this).closest('table.rai-table-collapsable').attr('data-rai-table-collapsable-accordion') == 'true') {
        $(this).closest('table.rai-table-collapsable').find('tbody.rai-table-collapsable-header.open').removeClass('open');
    }

    $(this).toggleClass('open');

    if ($(this).hasClass('open')) {
        var $panel = $(this);

        var parentContainer = 'html,body';
        var offset = 70;

        let ignoreScroll = $(this).data('ignore-scroll');
        if (ignoreScroll != 'true') {
            var customParentContainer = $(this).closest('table.rai-table-collapsable').data('table-collapsable-parent');
            if (customParentContainer == 'none') {
            }
            else if (customParentContainer != undefined && customParentContainer != '') {
                parentContainer = customParentContainer;

                $(parentContainer).animate({
                    scrollTop: getRelativePos($panel[0]).top
                }, 500);
            }
            else {
                $('html,body').animate({
                    scrollTop: $panel.offset().top - 70
                }, 500);
            }
        }


        var funcOnOpen = $(this).attr('data-rai-collapsable-onopen');
        if (funcOnOpen != undefined && funcOnOpen != null && funcOnOpen != '') {
            window[funcOnOpen]($(this));
        }
    }
    else {

    }
});

function getRelativePos(elm) {
    var pPos = elm.parentNode.getBoundingClientRect(), // parent pos
        cPos = elm.getBoundingClientRect(), // target pos
        pos = {};

    pos.top = cPos.top - pPos.top + elm.parentNode.scrollTop,
        pos.right = cPos.right - pPos.right,
        pos.bottom = cPos.bottom - pPos.bottom,
        pos.left = cPos.left - pPos.left;

    return pos;
}

function formatNumber(n) {
    var thousandSeparator = '.';
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, thousandSeparator);
}
function formatCurrency(input, blur) {
    var decimalSeparator = ',';
    var currencySym = "";


    // appends $ to value, validates decimal side
    // and puts cursor back in right position.

    // get input value
    var input_val = input.val();

    // don't validate empty input
    if (input_val === "") { return; }

    // original length
    var original_len = input_val.length;

    // initial caret position
    var caret_pos = input.prop("selectionStart");

    // check for decimal
    if (input_val.indexOf(decimalSeparator) >= 0) {

        // get position of first decimal
        // this prevents multiple decimals from
        // being entered
        var decimal_pos = input_val.indexOf(decimalSeparator);

        // split number by decimal point
        var left_side = input_val.substring(0, decimal_pos);
        var right_side = input_val.substring(decimal_pos);

        // add commas to left side of number
        left_side = formatNumber(left_side);

        // validate right side
        right_side = formatNumber(right_side);

        // On blur make sure 2 numbers after decimal
        if (blur === "blur") {
            right_side += "00";
        }

        // Limit decimal to only 2 digits
        right_side = right_side.substring(0, 2);

        // join number by .
        input_val = currencySym + left_side + decimalSeparator + right_side;

    } else {
        // no decimal entered
        // add commas to number
        // remove all non-digits
        input_val = formatNumber(input_val);
        input_val = currencySym + input_val;

        // final formatting
        if (blur === "blur") {
            input_val += decimalSeparator + "00";
        }
    }

    // send updated string to input
    input.val(input_val);

    // put caret back in the right position
    var updated_len = input_val.length;
    caret_pos = updated_len - original_len + caret_pos;
    input[0].setSelectionRange(caret_pos, caret_pos);
}
function formatForAjax(input) {
    return input.replace('.', '').replace(',', '.');
}

function RaiCalcolCF(cognome, nome, sesso, datanascita, luogoNascita) {
    var codice = '';

    //cognome
    codice += (cognome.replace(/[^BCDFGHJKLMNPQRSTVWXYZ]/gi, '') + cognome.replace(/[^AEIOU]/gi, '') + 'XXX').substr(0, 3).toUpperCase();
    //codice += ()
}

function getCaretPosition(ctrl) {
    var CaretPos = 0;    // IE Support
    if (document.selection) {
        ctrl.focus();
        var Sel = document.selection.createRange();
        Sel.moveStart('character', -ctrl.value.length);
        CaretPos = Sel.text.length;
    }
    // Firefox support
    else if (ctrl.selectionStart || ctrl.selectionStart == '0') {
        CaretPos = ctrl.selectionStart;
    }

    return CaretPos;
}

function setCaretPosition(ctrl, pos) {
    if (ctrl.setSelectionRange) {
        ctrl.focus();
        ctrl.setSelectionRange(pos, pos);
    }
    else if (ctrl.createTextRange) {
        var range = ctrl.createTextRange();
        range.collapse(true);
        range.moveEnd('character', pos);
        range.moveStart('character', pos);
        range.select();
    }
}

function forceInputUpperCase(input) {
    let charInput = event.keyCode;
    let exclChar = [8, 13, 16, 18,27, 32, 35, 36, 46];

    if (!event.ctrlKey && !event.metaKey && !event.altKey && exclChar.indexOf(charInput) < 0) {

        // Remember original caret position
        var caretPosition = getCaretPosition(input);

        // Uppercase-ize contents
        input.value = input.value.toLocaleUpperCase();

        // Reset caret position
        // (we ignore selection length, as typing deselects anyway)
        setCaretPosition(input, caretPosition);
    }
}

$(document.body).on('keydown', 'input[data-filterchar]', function (e) {
    let exclChar = [8, 9, 13, 16, 18, 27, 32, 35, 36, 46];
    let charInput = e.keyCode;
    //if (!e.ctrlKey && !e.metaKey && !e.altKey && exclChar.indexOf(charInput) < 0) {
    if (!e.ctrlKey && !e.metaKey && !e.altKey && charInput >= 48) {
        let tRegex = $(this).data('filterchar');
        if (tRegex) {
            let regex = new RegExp(tRegex);
            if (regex.test(event.key)) {
                return false;
            }
        }
    }
});

$(document.body).on('keydown', 'input[data-allowedchar]', function (e) {
    let exclChar = [8, 9, 13, 16, 18, 27, 32, 35, 36, 46];
    let charInput = e.keyCode;
    //if (!e.ctrlKey && !e.metaKey && !e.altKey && exclChar.indexOf(charInput) < 0) {
    if (!e.ctrlKey && !e.metaKey && !e.altKey && charInput >= 48) {
        let tRegex = $(this).data('allowedchar');
        if (tRegex) {
            let regex = new RegExp(tRegex);
            if (!regex.test(event.key)) {
                return false;
            }
        }
    }
});

function _keydownSubmit(control) {
    if (event.keyCode == 13) {
        let form = $(control).closest('form');
        if (form.length > 0) {
            $(form).submit();
        }
    }
}
$.ajaxTransport("+binary", function (options, originalOptions, jqXHR) {
    // check for conditions and support for blob / arraybuffer response type
    if (window.FormData && ((options.dataType && (options.dataType == 'binary')) || (options.data && ((window.ArrayBuffer && options.data instanceof ArrayBuffer) || (window.Blob && options.data instanceof Blob))))) {
        return {
            // create new XMLHttpRequest
            send: function (headers, callback) {
                // setup all variables
                var xhr = new XMLHttpRequest(),
                    url = options.url,
                    type = options.type,
                    async = options.async || true,
                    // blob or arraybuffer. Default is blob
                    dataType = options.responseType || "blob",
                    data = options.data || null,
                    username = options.username || null,
                    password = options.password || null;

                xhr.addEventListener('load', function () {
                    var data = {};
                    data[options.dataType] = xhr.response;
                    // make callback and send data
                    callback(xhr.status, xhr.statusText, data, xhr.getAllResponseHeaders());
                });

                xhr.open(type, url, async, username, password);

                // setup custom headers
                for (var i in headers) {
                    xhr.setRequestHeader(i, headers[i]);
                }

                xhr.responseType = dataType;
                xhr.send(data);
            },
            abort: function () {
                jqXHR.abort();
            }
        };
    }
});
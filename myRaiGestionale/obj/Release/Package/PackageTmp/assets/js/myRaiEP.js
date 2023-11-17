function RaiEPClearForm(idForm) {
    var form = $('#form-ricerca');
    $(form).find('input[type=text]').val('');
    $(form).find('.select2-selection__choice').remove();
    var opt = $(form).find('.js-select2');
    for (var i = 0; i < opt.length; i++) {
       opt[i].options.selectedIndex = 0;
    }
    var sel = $(form).find('select[id]');
    for (var i = 0; i < sel.length; i++) {
        var selmul = $('#' + sel[i].id);
        $('#' + sel[i].id).select2({
            placeholder: 'Selezionare dalla lista'
        });
     
    }
    $(form).find('input[type=checkbox]').each(function () { $(this)[0].checked = false; });
    $(form).find('input[type=radio]').each(function () { $(this)[0].checked = false; });
   
}
function GestCheckHasFilter(formID) {
   return false;
}





var BaseFormValidation = function() {
    // Inizializza Bootstrap Forms Validation, https://github.com/jzaefferer/jquery-validation
    var initValidationBootstrap = function(){
        jQuery('.js-validation-bootstrap').validate({
            ignore: [],
            errorClass: 'help-block animated fadeInDown',
            errorElement: 'div',
            errorPlacement: function(error, e) {
                jQuery(e).parents('.form-group > div').append(error);
            },
            highlight: function(e) {
                var elem = jQuery(e);

                elem.closest('.form-group').removeClass('has-error').addClass('has-error');
                elem.closest('.help-block').remove();
            },
            success: function(e) {
                var elem = jQuery(e);

                elem.closest('.form-group').removeClass('has-error');
                elem.closest('.help-block').remove();
            },
            rules: {
                'val-username': {
                    required: true,
                    minlength: 3
                },
                'val-email': {
                    required: true,
                    email: true
                },
                'val-password': {
                    required: true,
                    minlength: 5
                },
                'val-confirm-password': {
                    required: true,
                    equalTo: '#val-password'
                },
                'val-select2': {
                    required: true
                },
                'val-select2-multiple': {
                    required: true,
                    minlength: 2
                },
                'val-suggestions': {
                    required: true,
                    minlength: 5
                },
                'val-skill': {
                    required: true
                },
                'val-currency': {
                    required: true,
                    currency: ['$', true]
                },
                'val-website': {
                    required: true,
                    url: true
                },
                'val-phoneus': {
                    required: true,
                    phoneUS: true
                },
                'val-digits': {
                    required: true,
                    digits: true
                },
                'val-number': {
                    required: true,
                    number: true
                },
                'val-range': {
                    required: true,
                    range: [1, 5]
                },
                'val-terms': {
                    required: true
                }
            },
            messages: {

                
                'val-select2': 'Per favore seleziona un valore!',
                'val-select2-multiple': 'Per favore seleziona almeno 2 valori!',
                'val-suggestions': 'Cosa posso fare per te?',
                'val-skill': 'Per favore seleziona una skill!',
      
                'val-website': 'Per favore inserisci un link!',
   
                'val-digits': 'Per favore solo numeri!',
                'val-number': 'Per favore solo numeri!',
                'val-range': 'Per favore seleziona nel range!',
                'val-terms': 'Devi accettare i termini per proseguire!'
            }
        });
    };

    // Inizializza Material Forms Validation,  https://github.com/jzaefferer/jquery-validation
    var initValidationMaterial = function(){
        jQuery('.js-validation-material').validate({
            ignore: [],
            errorClass: 'help-block text-right animated fadeInDown',
            errorElement: 'div',
            errorPlacement: function(error, e) {
                jQuery(e).parents('.form-group > div').append(error);
            },
            highlight: function(e) {
                var elem = jQuery(e);

                elem.closest('.form-group').removeClass('has-error').addClass('has-error');
                elem.closest('.help-block').remove();
            },
            success: function(e) {
                var elem = jQuery(e);

                elem.closest('.form-group').removeClass('has-error');
                elem.closest('.help-block').remove();
            },
            rules: {
                'val-username2': {
                    required: true,
                    minlength: 3
                },
                'val-email2': {
                    required: true,
                    email: true
                },
                'val-password2': {
                    required: true,
                    minlength: 5
                },
                'val-confirm-password2': {
                    required: true,
                    equalTo: '#val-password2'
                },
                'val-select22': {
                    required: true
                },
                'val-select2-multiple2': {
                    required: true,
                    minlength: 2
                },
                'val-suggestions2': {
                    required: true,
                    minlength: 5
                },
                'val-skill2': {
                    required: true
                },
                'val-currency2': {
                    required: true,
                    currency: ['$', true]
                },
                'val-website2': {
                    required: true,
                    url: true
                },
                'val-phoneus2': {
                    required: true,
                    phoneUS: true
                },
                'val-digits2': {
                    required: true,
                    digits: true
                },
                'val-number2': {
                    required: true,
                    number: true
                },
                'val-range2': {
                    required: true,
                    range: [1, 5]
                },
                'val-terms2': {
                    required: true
                }
            },
            messages: {


                'val-select2': 'Per favore seleziona un valore!',
                'val-select2-multiple': 'Per favore seleziona almeno 2 valori!',
                'val-suggestions': 'Cosa posso fare per te?',
                'val-skill': 'Per favore seleziona una skill!',

                'val-website': 'Per favore inserisci un link!',

                'val-digits': 'Per favore solo numeri!',
                'val-number': 'Per favore solo numeri!',
                'val-range': 'Per favore seleziona nel range!',
                'val-terms': 'Devi accettare i termini per proseguire!'
            }
        });
    };

    return {
        init: function () {
            // Init Bootstrap Forms Validation
            initValidationBootstrap();

            // Init Material Forms Validation
            initValidationMaterial();

            // Init Validation al change di Select2
            jQuery('.js-select2').on('change', function(){
                jQuery(this).valid();
            });
        }
    };
}();

// Inizializza quando la pagina parte
jQuery(function(){ BaseFormValidation.init(); });

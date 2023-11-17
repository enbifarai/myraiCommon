

var BaseFormWizard = function() {
    
    var initWizardSimple = function(){
        jQuery('.js-wizard-simple').bootstrapWizard({
            'tabClass': '',
            'firstSelector': '.wizard-first',
            'previousSelector': '.wizard-prev',
            'nextSelector': '.wizard-next',
            'lastSelector': '.wizard-last',
            'onTabShow': function($tab, $navigation, $index) {
		var $total      = $navigation.find('li').length;
		var $current    = $index + 1;
		var $percent    = ($current/$total) * 100;

                // Get vital wizard elements
                var $wizard     = $navigation.parents('.block');
                var $progress   = $wizard.find('.wizard-progress > .progress-bar');
                var $btnPrev    = $wizard.find('.wizard-prev');
                var $btnNext    = $wizard.find('.wizard-next');
                var $btnFinish  = $wizard.find('.wizard-finish');

                // Update progress bar if there is one
		if ($progress) {
                    $progress.css({ width: $percent + '%' });
                }

		// If it's the last tab then hide the last button and show the finish instead
		if($current >= $total) {
                    $btnNext.hide();
                    $btnFinish.show();
		} else {
                    $btnNext.show();
                    $btnFinish.hide();
		}
            }
        });
    };

    

    return {
        init: function () {
            // Init simple wizard
            initWizardSimple();

 
        }
    };
}();


jQuery(function(){ BaseFormWizard.init(); });
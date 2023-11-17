/*
 *  Document   : JSRai.js
 *  Author     : Vincenzo Bifano
 *  Desscrizione: Framework per le funzionalità della UI
 *
 */

 (function($) {
     'use strict';

     /**
     * UIRai.
      * @constructor
      * @property {string}  VERSION      - Build Version.
      * @property {string}  AUTHOR       - Author.
      * @property {string}  SUPPORT      - Support Email.
      * @property {string}  pageScrollElement  - Scroll Element in Page.
      * @property {object}  $body - Cache Body.
      */
     var UIRai = function() {
         this.VERSION = "0.1.0";
         this.AUTHOR = "enbifa";


         this.pageScrollElement = 'html, body';
         this.$body = $('body');

         this.setUserOS();
         this.setUserAgent();
     }

     /** @function setUserOS
     * @description SET User Operating System eg: mac,windows,etc
     * @returns {string} - Appends OSName to UIRai.$body
     */
     UIRai.prototype.setUserOS = function() {
         var OSName = "";
         if (navigator.appVersion.indexOf("Win") != -1) OSName = "windows";
         if (navigator.appVersion.indexOf("Mac") != -1) OSName = "mac";
         if (navigator.appVersion.indexOf("X11") != -1) OSName = "unix";
         if (navigator.appVersion.indexOf("Linux") != -1) OSName = "linux";

         this.$body.addClass(OSName);
     }

     /** @function setUserAgent
     * @description SET User Device Name to mobile | desktop
     * @returns {string} - Appends Device to UIRai.$body
     */
     UIRai.prototype.setUserAgent = function() {
         if (navigator.userAgent.match(/Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile/i)) {
             this.$body.addClass('mobile');
         } else {
             this.$body.addClass('desktop');
             if (navigator.userAgent.match(/MSIE 9.0/)) {
                 this.$body.addClass('ie9');
             }
         }
     }

     /** @function isVisibleXs
     * @description Checks if the screen size is XS - Extra Small i.e below W480px
     * @returns {$Element} - Appends $('#pg-visible-xs') to Body
     */
     UIRai.prototype.isVisibleXs = function() {
         (!$('#pg-visible-xs').length) && this.$body.append('<div id="pg-visible-xs" class="visible-xs" />');
         return $('#pg-visible-xs').is(':visible');
     }

     /** @function isVisibleSm
     * @description Checks if the screen size is SM - Small Screen i.e Above W480px
     * @returns {$Element} - Appends $('#pg-visible-sm') to Body
     */
     UIRai.prototype.isVisibleSm = function() {
         (!$('#pg-visible-sm').length) && this.$body.append('<div id="pg-visible-sm" class="visible-sm" />');
         return $('#pg-visible-sm').is(':visible');
     }

     /** @function isVisibleMd
     * @description Checks if the screen size is MD - Medium Screen i.e Above W1024px
     * @returns {$Element} - Appends $('#pg-visible-md') to Body
     */
     UIRai.prototype.isVisibleMd = function() {
         (!$('#pg-visible-md').length) && this.$body.append('<div id="pg-visible-md" class="visible-md" />');
         return $('#pg-visible-md').is(':visible');
     }

     /** @function isVisibleLg
     * @description Checks if the screen size is LG - Large Screen i.e Above W1200px
     * @returns {$Element} - Appends $('#pg-visible-lg') to Body
     */
     UIRai.prototype.isVisibleLg = function() {
         (!$('#pg-visible-lg').length) && this.$body.append('<div id="pg-visible-lg" class="visible-lg" />');
         return $('#pg-visible-lg').is(':visible');
     }

     /** @function isVisibleJunosMenu
     * @description Checks if is visible the Junos Pulse menu - Internet Zone
     * @returns {$Element} - Appends class internet to Body
     */
     UIRai.prototype.isVisibleJunosMenu = function() {
         this.$body.addClass('internet');
         return $('#dsl0').is(':visible');

     }

     /** @function removeJunosMenu
     * @description Remove from the Page the Junos Pulse menu - Internet Zone
     */
     UIRai.prototype.removeJunosMenu = function() {
        $("#dsl0").remove();
     }

     /** @function getUserAgent
     * @description Get Current User Agent.
     * @returns {string} - mobile | desktop
     */
     UIRai.prototype.getUserAgent = function() {
         return $('body').hasClass('mobile') ? "mobile" : "desktop";
     }


     /** @function isIVEAmbient
     * @description Check if there is the IVE domain - Internet Zone
     */
     UIRai.prototype.getIVEHostname = function() {

        if (getIVEHostname() != undefined)
        {
          return true;
        }
        else{
          return false;
        }
     }


     /** @function checkVisible
     * @description check current visibility of an element.
     * @returns {string} - true | false
     */
     UIRai.prototype.checkVisibleComplete = function(elm, threshold, mode) {
      threshold = threshold || 0;
      mode = mode || 'visible';

      var rect = elm.getBoundingClientRect();
      var viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight);
      var above = rect.bottom - threshold < 0;
      var below = rect.top - viewHeight + threshold >= 0;

      return mode === 'above' ? above : (mode === 'below' ? below : !above && !below);
    }
    UIRai.prototype.checkVisible = function(elm) {
      var rect = elm.getBoundingClientRect();
      var viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight);
      return !(rect.bottom < 0 || rect.top - viewHeight >= 0);
    }


     $.UIRai = new UIRai();
     $.UIRai.Constructor = UIRai;
})(window.jQuery);

 var JSRai = function() {
     // Helper (variabili - settate in jsInit())
     var $lToggle, $lHtml, $lBody, $lPage, $lSidebar, $lSidebarScroll, $lSideOverlay, $lSideOverlayScroll, $lHeader, $lMain, $lFooter;

     /*
      ********************************************************************************************
      *
      * FUNZIONALITA' DI BASE DELLA UI
      *
      * Funzioni fondamentali per la funzionalità della UI come la navigazione e il layout
      * che sono autoinizializzate in ogni pagina
      *
      *********************************************************************************************
      */

     // Inizializzazione User Interface
     var jsInit = function() {
         // settaggio variabili
         $lToggle            = jQuery('#toggle');
         $lHtml              = jQuery('html');
         $lBody              = jQuery('body');
         $lPage              = jQuery('#page-container');
         $lSidebar           = jQuery('#sidebar');
         $lSidebarScroll     = jQuery('#sidebar-scroll');
         $lSideOverlay       = jQuery('#side-overlay');
         $lSideOverlayScroll = jQuery('#side-overlay-scroll');
         $lHeader            = jQuery('#header-navbar');
         $lMain              = jQuery('#main-container');
         $lFooter            = jQuery('#page-footer');

         // Inizializza tooltip
         jQuery('[data-toggle="tooltip"], .js-tooltip').tooltip({
             container: 'body',
             animation: false
         });


         // Inizializza Tabs
         jQuery('[data-toggle="tabs"] a, .js-tabs a').click(function(e){
             e.preventDefault();
             jQuery(this).tab('show');
         });

         // Init form placeholder (per IE9)
         jQuery('.form-control').placeholder();
     };

     // Funzionalità del Layout
     var jsLayout = function() {
         // Redimensiona il #main-container min height (pusha il footer in basso)
         var $resizeTimeout;

         if ($lMain.length) {
             jsHandleMain();

             jQuery(window).on('resize orientationchange', function(){
                 clearTimeout($resizeTimeout);

                 $resizeTimeout = setTimeout(function(){
                     jsHandleMain();
                 }, 150);
             });
         }

         // Inizializza lo scrolling custom della sidebar e l'overlay laterale
         jsHandleScroll('init');

        if ($lPage.hasClass('header-navbar-fixed') && $lPage.hasClass('header-navbar-transparent')) {
             jQuery(window).on('scroll', function(){
                 if (jQuery(this).scrollTop() > 20) {
                     $lPage.addClass('header-navbar-scroll');
                 } else {
                     $lPage.removeClass('header-navbar-scroll');
                 }
             });
         }

         // Chiama le API di layout alla pressione del click di un pulsante
         jQuery('[data-toggle="layout"]').on('click', function(){
             var $btn = jQuery(this);

             jsLayoutApi($btn.data('action'));

             if ($lHtml.hasClass('no-focus')) {
                 $btn.blur();
             }
         });

     };

     // Ridimensiona il  #main-container per riempire gli spazi qualora ci fossero
     var jsHandleMain = function() {
         var $hWindow     = jQuery(window).height();
         var $hHeader     = $lHeader.outerHeight();
         var $hFooter     = $lFooter.outerHeight();

         if ($lPage.hasClass('header-navbar-fixed')) {
             $lMain.css('min-height', $hWindow - $hFooter);
         } else {
             $lMain.css('min-height', $hWindow - ($hHeader + $hFooter));
         }
     };

     // Handles sidebar and side overlay custom scrolling functionality
     var jsHandleScroll = function($mode) {
         var $windowW = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;

         // Init scrolling
         if ($mode === 'init') {
             // Init scrolling solo se richiesto la prima volta
             jsHandleScroll();

             // Handle scrolling al resize o all'orientation change
             var $sScrollTimeout;

             jQuery(window).on('resize orientationchange', function(){
                 clearTimeout($sScrollTimeout);

                 $sScrollTimeout = setTimeout(function(){
                     jsHandleScroll();
                 }, 150);
             });
         } else {
             // If screen width is greater than 991 pixels and .side-scroll is added to #page-container
             if ($windowW > 991 && $lPage.hasClass('side-scroll')) {
                 // Turn scroll lock off (sidebar and side overlay - slimScroll will take care of it)
                 jQuery($lSidebar).scrollLock('disable');
                 jQuery($lSideOverlay).scrollLock('disable');

                 // If sidebar scrolling does not exist init it..
                 if ($lSidebarScroll.length && (!$lSidebarScroll.parent('.slimScrollDiv').length)) {
                     $lSidebarScroll.slimScroll({
                         height: $lSidebar.outerHeight(),
                         color: '#fff',
                         size: '5px',
                         opacity : .35,
                         wheelStep : 15,
                         distance : '2px',
                         railVisible: false,
                         railOpacity: 1
                     });
                 }
                 else { // ..else resize scrolling height
                     $lSidebarScroll
                         .add($lSidebarScroll.parent())
                         .css('height', $lSidebar.outerHeight());
                 }

                 // If side overlay scrolling does not exist init it..
                 if ($lSideOverlayScroll.length && (!$lSideOverlayScroll.parent('.slimScrollDiv').length)) {
                     $lSideOverlayScroll.slimScroll({
                         height: $lSideOverlay.outerHeight(),
                         color: '#000',
                         size: '5px',
                         opacity : .35,
                         wheelStep : 15,
                         distance : '2px',
                         railVisible: false,
                         railOpacity: 1
                     });
                 }
                 else { // ..else resize scrolling height
                     $lSideOverlayScroll
                         .add($lSideOverlayScroll.parent())
                         .css('height', $lSideOverlay.outerHeight());
                 }
             } else {
                 // Turn scroll lock on (sidebar and side overlay)
                 jQuery($lSidebar).scrollLock('enable');
                 jQuery($lSideOverlay).scrollLock('enable');

                 // If sidebar scrolling exists destroy it..
                 if ($lSidebarScroll.length && $lSidebarScroll.parent('.slimScrollDiv').length) {
                     $lSidebarScroll
                         .slimScroll({destroy: true});
                     $lSidebarScroll
                         .attr('style', '');
                 }

                 // If side overlay scrolling exists destroy it..
                 if ($lSideOverlayScroll.length && $lSideOverlayScroll.parent('.slimScrollDiv').length) {
                     $lSideOverlayScroll
                         .slimScroll({destroy: true});
                     $lSideOverlayScroll
                         .attr('style', '');
                 }
             }
         }
     };

     // API del layout
     var jsLayoutApi = function($mode) {
         var $windowW = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;

         // Selezione del modo
         switch($mode) {
             case 'sidebar_pos_toggle':
                 $lPage.toggleClass('sidebar-l sidebar-r');
                 break;
             case 'sidebar_pos_left':
                 $lPage
                     .removeClass('sidebar-r')
                     .addClass('sidebar-l');
                 break;
             case 'sidebar_pos_right':
                 $lPage
                     .removeClass('sidebar-l')
                     .addClass('sidebar-r');
                 break;
             case 'sidebar_toggle':
                 if ($windowW > 991) {
                     $lPage.toggleClass('sidebar-o');
                 } else {
                     $lPage.toggleClass('sidebar-o-xs');
                 }
                 break;
             case 'sidebar_open':
                 $lPage
                 .removeClass('sidebar-l')
                 .addClass('sidebar-r');
                 if ($windowW > 991) {
                     $lPage.addClass('sidebar-o');
                 } else {
                     $lPage.addClass('sidebar-o-xs');
                 }
                 break;
             case 'sidebar_close':
                 if ($windowW > 991) {
                     $lPage.removeClass('sidebar-o');
                 } else {
                     $lPage.removeClass('sidebar-o-xs');
                 }
                 break;
             case 'sidebar_mini_toggle':
                 //console.log($lToggle);
                 $lToggle.removeClass('fa-circle-o');
                 $lToggle.toggleClass('fa-dot-circle-o text-success');
                 if ($windowW > 991) {
                     $lToggle.toggleClass('fa-circle-o');
                     $lPage.toggleClass('sidebar-mini');
                 }
                 break;
             case 'sidebar_mini_on':
                 if ($windowW > 991) {
                     $lPage.addClass('sidebar-mini');
                 }
                 break;
             case 'sidebar_mini_off':
                 if ($windowW > 991) {
                     $lPage.removeClass('sidebar-mini');
                 }
                 break;
             case 'side_overlay_toggle':
                 $lPage.toggleClass('side-overlay-o');
                 break;
             case 'side_overlay_open':
                 $lPage.addClass('side-overlay-o');
                 break;
             case 'side_overlay_close':
                 $lPage.removeClass('side-overlay-o');
                 break;
             case 'side_overlay_hoverable_toggle':
                 $lPage.toggleClass('side-overlay-hover');
                 break;
             case 'side_overlay_hoverable_on':
                 $lPage.addClass('side-overlay-hover');
                 break;
             case 'side_overlay_hoverable_off':
                 $lPage.removeClass('side-overlay-hover');
                 break;
             case 'header_fixed_toggle':
                 $lPage.toggleClass('header-navbar-fixed');
                 break;
             case 'header_fixed_on':
                 $lPage.addClass('header-navbar-fixed');
                 break;
             case 'header_fixed_off':
                 $lPage.removeClass('header-navbar-fixed');
                 break;
             case 'side_scroll_toggle':
                 $lPage.toggleClass('side-scroll');
                 uiHandleScroll();
                 break;
             case 'side_scroll_on':
                 $lPage.addClass('side-scroll');
                 uiHandleScroll();
                 break;
             case 'side_scroll_off':
                 $lPage.removeClass('side-scroll');
                 uiHandleScroll();
                 break;
             default:
                 return false;
         }
     };
     // Funzionalità della navigazione principale
     var jsNav = function() {
         // quando un link di un submenu è cliccato
         jQuery('[data-toggle="nav-submenu"]').on('click', function(e){
             // prendo il link
             var $link = jQuery(this);

             // prendo il link del padre
             var $parentLi = $link.parent('li');

             if ($parentLi.hasClass('open')) { // se il sottomenu è aperto, lo chiuso..
                 $parentLi.removeClass('open');
             } else { // .. invece se è chiuso, chiudo tutti gli altri che sono allo stesso livello prima di aprirlo
                 $link
                     .closest('ul')
                     .find('> li')
                     .removeClass('open');

                 $parentLi
                     .addClass('open');
             }

             // Tolgo il focus dal link
             if ($lHtml.hasClass('no-focus')) {
                 $link.blur();
             }

             return false;
         });
     };

    var jsSingleBlockInit = function($block)
    {
      var $iconContent            = 'si si-arrow-up';
      var $iconContentActive      = 'si si-arrow-down';
        jQuery($block + ' [data-toggle="block-option"]').html('<i class="' + (jQuery($block + ' [data-toggle="block-option"]').closest('.block').hasClass('block-opt-hidden') ? $iconContentActive : $iconContent) + '"></i>');

      // Chiama le API alla pressione di un pulsante
        jQuery($block + ' [data-toggle="block-option"]').on('click', function () {
            jsBlocksApi(jQuery($block + ' [data-toggle="block-option"]').closest('.block'), jQuery($block + ' [data-toggle="block-option"]').data('action'));
      });
    }


     // Funzionalità dei blocchi
     var jsBlocks = function() {

         // Inizializza le icone di default a fullscreen e il pulsante toggle del contenuto
         jsBlocksApi(false, 'init');

         // Chiama le API alla pressione di un pulsante
         jQuery('[data-toggle="block-option"]').on('click', function(){
             jsBlocksApi(jQuery(this).closest('.block'), jQuery(this).data('action'));
         });
     };

     // API per i blocchi
     var jsBlocksApi = function($block, $mode) {
         // Inizializza le icone di default a fullscreen e il pulsante toggle del contenuto
         var $iconFullscreen         = 'si si-size-fullscreen';
         var $iconFullscreenActive   = 'si si-size-actual';
         var $iconContent            = 'si si-arrow-up';
         var $iconContentActive      = 'si si-arrow-down';

         if ($mode === 'init') {
             // Auto aggiunta le icone di default a fullscreen e il pulsante toggle del contenuto
             jQuery('[data-toggle="block-option"][data-action="fullscreen_toggle"]').each(function(){
                 var $this = jQuery(this);

                 $this.html('<i class="' + (jQuery(this).closest('.block').hasClass('block-opt-fullscreen') ? $iconFullscreenActive : $iconFullscreen) + '"></i>');
             });

             jQuery('[data-toggle="block-option"][data-action="content_toggle"]').each(function(){
                 var $this = jQuery(this);

                 $this.html('<i class="' + ($this.closest('.block').hasClass('block-opt-hidden') ? $iconContentActive : $iconContent) + '"></i>');
             });
         } else {
             // Recupero l'elemento blocco
             var $elBlock = ($block instanceof jQuery) ? $block : jQuery($block);

             // Se esite, procedo con le funzionalità
             if ($elBlock.length) {

                 // Prendo i pulsanti di opzione se esistono
                 var $btnFullscreen  = jQuery('[data-toggle="block-option"][data-action="fullscreen_toggle"]', $elBlock);
                 var $btnToggle      = jQuery('[data-toggle="block-option"][data-action="content_toggle"]', $elBlock);

                 // Modo di selezione
                 switch($mode) {
                     case 'fullscreen_toggle':
                         $elBlock.toggleClass('block-opt-fullscreen');

                         // Abilito/Disabilito lo scroll lock al blocco
                         if ($elBlock.hasClass('block-opt-fullscreen')) {
                             jQuery($elBlock).scrollLock('enable');
                         } else {
                             jQuery($elBlock).scrollLock('disable');
                         }

                         // Aggiorno il blocco icone di opzione
                         if ($btnFullscreen.length) {
                             if ($elBlock.hasClass('block-opt-fullscreen')) {
                                 jQuery('i', $btnFullscreen)
                                     .removeClass($iconFullscreen)
                                     .addClass($iconFullscreenActive);
                             } else {
                                 jQuery('i', $btnFullscreen)
                                     .removeClass($iconFullscreenActive)
                                     .addClass($iconFullscreen);
                             }
                         }
                         break;
                     case 'fullscreen_on':
                         $elBlock.addClass('block-opt-fullscreen');

                         // Abilito lo scroll lock al blocco
                         jQuery($elBlock).scrollLock('enable');


                         // Aggiorno il blocco icone di opzione
                         if ($btnFullscreen.length) {
                             jQuery('i', $btnFullscreen)
                                 .removeClass($iconFullscreen)
                                 .addClass($iconFullscreenActive);
                         }
                         break;
                     case 'fullscreen_off':
                         $elBlock.removeClass('block-opt-fullscreen');

                          // Disabilito lo scroll lock al blocco
                         jQuery($elBlock).scrollLock('disable');

                         // Aggiorno il blocco icone di opzione
                         if ($btnFullscreen.length) {
                             jQuery('i', $btnFullscreen)
                                 .removeClass($iconFullscreenActive)
                                 .addClass($iconFullscreen);
                         }
                         break;
                     case 'content_toggle':
                         $elBlock.toggleClass('block-opt-hidden');

                         // Aggiorno il blocco icone di opzione
                         if ($btnToggle.length) {
                             if ($elBlock.hasClass('block-opt-hidden')) {
                                 jQuery('i', $btnToggle)
                                     .removeClass($iconContent)
                                     .addClass($iconContentActive);
                             } else {
                                 jQuery('i', $btnToggle)
                                     .removeClass($iconContentActive)
                                     .addClass($iconContent);
                             }
                         }
                         break;
                     case 'content_hide':
                         $elBlock.addClass('block-opt-hidden');

                         // Aggiorno il blocco icone di opzione
                         if ($btnToggle.length) {
                             jQuery('i', $btnToggle)
                                 .removeClass($iconContent)
                                 .addClass($iconContentActive);
                         }
                         break;
                     case 'content_show':
                         $elBlock.removeClass('block-opt-hidden');

                        // Aggiorno il blocco icone di opzione
                         if ($btnToggle.length) {
                             jQuery('i', $btnToggle)
                                 .removeClass($iconContentActive)
                                 .addClass($iconContent);
                         }
                         break;
                     case 'refresh_toggle':
                         $elBlock.toggleClass('block-opt-refresh');
                         // Riporta il blocco allo stato normale se è in demo
                         if (jQuery('[data-toggle="block-option"][data-action="refresh_toggle"][data-action-mode="demo"]', $elBlock).length) {
                             setTimeout(function(){
                                 $elBlock.removeClass('block-opt-refresh');
                             }, 2000);
                         }
                         break;
                     case 'state_loading':
                         $elBlock.addClass('block-opt-refresh');
                         break;
                     case 'state_normal':
                         $elBlock.removeClass('block-opt-refresh');
                         break;
                     case 'close':
                         $elBlock.hide();
                         break;
                     case 'open':
                         $elBlock.show();
                         break;
                     default:
                         return false;
                 }
             }
         }
     };
     // Helper per gli input di form
     var jsForms = function() {
         jQuery('.form-material.floating > .form-control').each(function(){
             var $input  = jQuery(this);
             var $parent = $input.parent('.form-material');

             setTimeout(function() {
                 if ($input.val() ) {
                     $parent.addClass('open');
                 }
             }, 150);

             $input.on('change', function(){
                 if ($input.val()) {
                     $parent.addClass('open');
                 } else {
                     $parent.removeClass('open');
                 }
             });
         });
     };



     // Helper per l'animazione di scrolling
     var jsScrollTo = function() {
         jQuery('[data-toggle="scroll-to"]').on('click', function(){
             var $this           = jQuery(this);
             var $target         = $this.data('target');
             var $speed          = $this.data('speed') ? $this.data('speed') : 1000;
             var $headerHeight   = ($lHeader.length && $lPage.hasClass('header-navbar-fixed')) ? $lHeader.outerHeight() : 0;

             jQuery('html, body').animate({
                 scrollTop: jQuery($target).offset().top - $headerHeight
             }, $speed);
         });
     };

     // Helper per la classe di toggle
     var jsToggleClass = function() {
         jQuery('[data-toggle="class-toggle"]').on('click', function(){
             var $el = jQuery(this);

             jQuery($el.data('target').toString()).toggleClass($el.data('class').toString());

             if ($lHtml.hasClass('no-focus')) {
                 $el.blur();
             }
         });
     };

     // Funzionalità per il loader di intera pagina
     var jsLoader = function($mode) {
         var $lpageLoader = jQuery('#page-loaderR');

         if ($mode === 'show') {
             if ($lpageLoader.length) {
                 $lpageLoader.fadeIn(250);
             } else {
                 $lBody.prepend('<div id="page-loaderR"><span class="loaderR"><span class="loader-innerR">S</span></span></div>');
             }
         } else if ($mode === 'hide') {
             if ($lpageLoader.length) {
                 $lpageLoader.fadeOut(250);
             }
         }

         return false;
     };
     // Funzionalità per il loader di div
     var jsLoaderDiv = function($mode,$div) {
         var $lpageLoader = jQuery('#'+$div);

         if ($mode === 'show') {
             if ($lpageLoader.length) {
                 $lpageLoader.fadeIn(250);
             } else {
                 $lpageLoader.prepend('<div id=page-loaderR"><span class="loaderR"><span class="loader-innerR">S</span></span></div>');
             }
         } else if ($mode === 'hide') {
             if ($lpageLoader.length) {
                 $lpageLoader.fadeOut(250);
             }
         }

         return false;
     };
     /*
      ********************************************************************************************
      *
      * UI HELPERS (A RICHIESTA)
      *
      *
      ********************************************************************************************
      */

     /*
      * Funzionalita di tabella custom tipo toggling o righe checkabili
      * JSRai.initHelper('strumenti-tabelle');
      *
      */

     // Table sections functionality
     var jsHelperSezioneStrumentiTabelle = function(){
         // Per ogni tabella
         jQuery('.js-table-sections').each(function(){
             var $table = jQuery(this);

             // Quando una riga è cliccata in .js-table-sections-header
             jQuery('.js-table-sections-header > tr', $table).on('click', function (e) {
                 return;
                 var $row    = jQuery(this);
                 var $tbody  = $row.parent('tbody');

                 if (! $tbody.hasClass('open')) {
                     jQuery('tbody', $table).removeClass('open');
                 }

                 $tbody.toggleClass('open');
             });
         });
     };

     /*
    * Bootstrap Datetimepicker, 
    *
    * JSRai.initHelper('datetimepicker');
    *
    */
     var jsHelperDatetimepicker = function () {
         // Init Bootstrap Datetimepicker (with .js-datetimepicker class)
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
     };

     /*
      * jQuery Appear, per esempi https://github.com/bas2k/jquery.appear
      *
      * JSRai.initHelper('appear');
      *
      */
     var jsHelperAppear = function(){
         // Aggiungi una classe specifica ad un elemento (quando diventa visible allo scrolling)
         jQuery('[data-toggle="appear"]').each(function(){
             var $windowW    = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
             var $this       = jQuery(this);
             var $class      = $this.data('class') ? $this.data('class') : 'animated fadeIn';
             var $offset     = $this.data('offset') ? $this.data('offset') : 0;
             var $timeout    = ($lHtml.hasClass('ie9') || $windowW < 992) ? 0 : ($this.data('timeout') ? $this.data('timeout') : 0);

             $this.appear(function() {
                 setTimeout(function(){
                     $this
                         .removeClass('visibility-hidden')
                         .addClass($class);
                 }, $timeout);
             },{accY: $offset});
         });
     };

     /*
      * jQuery Appear + jQuery countTo, per far partire un conteggio quando un oggetto è visibile https://github.com/mhuggins/jquery-countTo
      *
      * JSRai.initHelper('appear-countTo');
      *
      */
     var jsHelperAppearCountTo = function(){
         // Init counter functionality
         jQuery('[data-toggle="countTo"]').each(function(){
             var $this       = jQuery(this);
             var $after      = $this.data('after');
             var $before     = $this.data('before');
             var $speed      = $this.data('speed') ? $this.data('speed') : 1500;
             var $interval   = $this.data('interval') ? $this.data('interval') : 15;

             $this.appear(function() {
                 $this.countTo({
                     speed: $speed,
                     refreshInterval: $interval,
                     onComplete: function() {
                         if($after) {
                             $this.html($this.html() + $after);
                         } else if ($before) {
                             $this.html($before + $this.html());
                         }
                     }
                 });
             });
         });
     };

     /*
      * jQuery SlimScroll, per esempi http://rocha.la/jQuery-slimScroll
      *
      * JSRai.initHelper('slimscroll');
      *
      */
     var jsHelperSlimscroll = function(){
         // Inizializza funzionalità slimScroll
         jQuery('[data-toggle="slimscroll"]').each(function(){
             var $this       = jQuery(this);
             var $height     = $this.data('height') ? $this.data('height') : '200px';
             var $size       = $this.data('size') ? $this.data('size') : '5px';
             var $position   = $this.data('position') ? $this.data('position') : 'right';
             var $color      = $this.data('color') ? $this.data('color') : '#000';
             var $avisible   = $this.data('always-visible') ? true : false;
             var $rvisible   = $this.data('rail-visible') ? true : false;
             var $rcolor     = $this.data('rail-color') ? $this.data('rail-color') : '#999';
             var $ropacity   = $this.data('rail-opacity') ? $this.data('rail-opacity') : .3;

             $this.slimScroll({
                 height: $height,
                 size: $size,
                 position: $position,
                 color: $color,
                 alwaysVisible: $avisible,
                 railVisible: $rvisible,
                 railColor: $rcolor,
                 railOpacity: $ropacity
             });
         });
     };

     /*
      ********************************************************************************************
      *
      * I seguenti helpers hanno bisogno dei loro file specifici (JS, CSS)
      *
      ********************************************************************************************
      */



     /*
      * CKEditor init, se serve http://ckeditor.com/
      *
      * JSRai.initHelper('ckeditor');
      *
      */
     var jsHelperCkeditor = function(){

         CKEDITOR.disableAutoInline = true;

         // Init inline text editor
         if (jQuery('#js-ckeditor-inline').length) {
             CKEDITOR.inline('js-ckeditor-inline');
         }

         // Init full text editor
         if (jQuery('#js-ckeditor').length) {
             CKEDITOR.replace('js-ckeditor');
         }
     };



     /*
      * Select2, esempi https://github.com/select2/select2
      *
      * JSRai.initHelper('select2');
      *
      */
     var jsHelperSelect2 = function(){
         // Init Select2 (with .js-select2 class)
         jQuery( '.js-select2' ).select2( {
             placeholder: "Seleziona dalla lista"
         } );
     };

     /*
      * Highlight.js, esempi https://highlightjs.org/usage/
      *
      * JSRai.initHelper('highlightjs');
      *
      */
     var jsHelperHighlightjs = function(){
         // Init Highlight.js
         hljs.initHighlightingOnLoad();
     };

     /*
      * Bootstrap Notify,  http://bootstrap-growl.remabledesigns.com/
      *
      * JSRai.initHelper('notify');
      *
      */
     var jsHelperNotify = function(){
         // Init notifications (with .js-notify class)
         jQuery('.js-notify').on('click', function(){
             var $notify         = jQuery(this);
             var $notifyMsg      = $notify.data('notify-message');
             var $notifyType     = $notify.data('notify-type') ? $notify.data('notify-type') : 'info';
             var $notifyFrom     = $notify.data('notify-from') ? $notify.data('notify-from') : 'top';
             var $notifyAlign    = $notify.data('notify-align') ? $notify.data('notify-align') : 'right';
             var $notifyIcon     = $notify.data('notify-icon') ? $notify.data('notify-icon') : '';
             var $notifyUrl      = $notify.data('notify-url') ? $notify.data('notify-url') : '';

             jQuery.notify({
                     icon: $notifyIcon,
                     message: $notifyMsg,
                     url: $notifyUrl
                 },
                 {
                     element: 'body',
                     type: $notifyType,
                     allow_dismiss: true,
                     newest_on_top: true,
                     showProgressbar: false,
                     placement: {
                         from: $notifyFrom,
                         align: $notifyAlign
                     },
                     offset: 20,
                     spacing: 10,
                     z_index: 1033,
                     delay: 5000,
                     timer: 1000,
                     animate: {
                         enter: 'animated fadeIn',
                         exit: 'animated fadeOutDown'
                     }
                 });
         });
     };



     /*
      * Easy Pie Chart, esempi http://rendro.github.io/easy-pie-chart/
      *
      * JSRai.initHelper('easy-pie-chart');
      *
      */
     var jsHelperEasyPieChart = function(){
         // Init Easy Pie Charts (with .js-pie-chart class)
         jQuery('.js-pie-chart').easyPieChart({
             barColor: jQuery(this).data('bar-color') ? jQuery(this).data('bar-color') : '#777777',
             trackColor: jQuery(this).data('track-color') ? jQuery(this).data('track-color') : '#eeeeee',
             lineWidth: jQuery(this).data('line-width') ? jQuery(this).data('line-width') : 3,
             size: jQuery(this).data('size') ? jQuery(this).data('size') : '80',
             animate: 750,
             scaleColor: jQuery(this).data('scale-color') ? jQuery(this).data('scale-color') : false
         });
     };

     /*
      * Bootstrap Maxlength, esempi https://github.com/mimo84/bootstrap-maxlength
      *
      * JSRai.initHelper('maxlength');
      *
      */
     var jsHelperMaxlength = function(){
         // Init Bootstrap Maxlength (with .js-maxlength class)
         jQuery('.js-maxlength').each(function(){
             var $input = jQuery(this);

             $input.maxlength({
                 alwaysShow: $input.data('always-show') ? true : false,
                 threshold: $input.data('threshold') ? $input.data('threshold') : 10,
                 warningClass: $input.data('warning-class') ? $input.data('warning-class') : 'label label-warning',
                 limitReachedClass: $input.data('limit-reached-class') ? $input.data('limit-reached-class') : 'label label-danger',
                 placement: $input.data('placement') ? $input.data('placement') : 'bottom',
                 preText: $input.data('pre-text') ? $input.data('pre-text') : '',
                 separator: $input.data('separator') ? $input.data('separator') : '/',
                 postText: $input.data('post-text') ? $input.data('post-text') : ''
             });
         });
     };

     /*
      * Time Update of Moment clock
      *
      * JSRai.initHelper('timeupdate');
      *
      */

     var jsTimeUpdate = function() {
            //$(".actual-date .actual-day").text(moment().locale('it').format('DD'));
            //$(".actual-date .actual-month").text(moment().locale('it').format('MMMM'));
            $(".actual-day").text(moment().locale('it').format('DD'));
            $(".actual-month").text(moment().locale('it').format('MMMM'));
            $(".actual-hour").text(moment().locale('it').format('HH:mm:ss'));
         //   updateClock();
            setTimeout(jsTimeUpdate, 1000);
     };

     var updateClock = function() {
          var now = moment(), second = now.seconds() * 6, minute = now.minutes() * 6 + second / 60, hour = ((now.hours() % 12) / 12) * 360 + 90 + minute / 12;
          //console.log(now);
          $('#hour').css({
            "-webkit-transform": "rotate(" + hour + "deg)",
            "-moz-transform": "rotate(" + hour + "deg)",
            "-ms-transform": "rotate(" + hour + "deg)",
            "transform": "rotate(" + hour + "deg)"
          });
          $('#minute').css({
            "-webkit-transform": "rotate(" + minute + "deg)",
            "-moz-transform": "rotate(" + minute + "deg)",
            "-ms-transform": "rotate(" + minute + "deg)",
            "transform": "rotate(" + minute + "deg)"
          });
          $('.clock #second').css({
            "-webkit-transform": "rotate(" + second + "deg)",
            "-moz-transform": "rotate(" + second + "deg)",
            "-ms-transform": "rotate(" + second + "deg)",
            "transform": "rotate(" + second + "deg)"
          });
     };
     return {
         init: function($func) {
             switch ($func) {
                 case 'jsInit':
                     jsInit();
                     break;
                 case 'jsLayout':
                     jsLayout();
                     break;
                 case 'jsNav':
                     jsNav();
                     break;
                 case 'jsBlocks':
                     jsBlocks();
                     break;
                 case 'jsForms':
                     jsForms();
                     break;

                 case 'jsToggleClass':
                     jsToggleClass();
                     break;
                 case 'jsScrollTo':
                     jsScrollTo();
                     break;
                 case 'jsLoader':
                     jsLoader('hide');
                     break;
                 default:
                     // Init all vital functions
                    
                    jsInit();
                    jsLayout();
                    jsNav();
                    jsBlocks();
                    jsForms();

                    jsToggleClass();
                    jsScrollTo();
                    jsLoader('hide');
                   // jsTimeUpdate();
             }
         },
         layout: function($mode) {
             jsLayoutApi($mode);
         },
         loader: function($mode) {
             jsLoader($mode);
         },
         loaderDiv: function($mode,$div) {
             jsLoaderDiv($mode,$div);
         },
         blocks: function($block, $mode) {
             jsBlocksApi($block, $mode);
         },
         initblock: function($block)
         {
             jsSingleBlockInit($block);
         },
         initHelper: function($helper) {
             switch ($helper) {

                 case 'strumenti-table':
                     jsHelperSezioneStrumentiTabelle();
                     break;
                 case 'appear':
                     jsHelperAppear();
                     break;
                 case 'appear-countTo':
                     jsHelperAppearCountTo();
                     break;
                 case 'slimscroll':
                     jsHelperSlimscroll();
                     break;

                 case 'ckeditor':
                     jsHelperCkeditor();
                     break;
                 case 'select2':
                     jsHelperSelect2();
                     break;
                 case 'highlightjs':
                     jsHelperHighlightjs();
                     break;
                 case 'notify':
                     jsHelperNotify();
                     break;
                 case 'datetimepicker':
                     jsHelperDatetimepicker();
                     break;
                 case 'easy-pie-chart':
                     jsHelperEasyPieChart();
                     break;

                 case 'maxlength':
                     jsHelperMaxlength();
                     break;

                 default:
                     return false;
             }
         },
         initHelpers: function($helpers) {
             if ($helpers instanceof Array) {
                 for(var $index in $helpers) {
                     JSRai.initHelper($helpers[$index]);
                 }
             } else {
                 JSRai.initHelper($helpers);
             }
         }
     };
 }();

 // Alias per JSRai (si puà usare UIRai invece di JSRai )
 var UIRai = JSRai;

 // Inizializza JSRai quando la pagina si avvia
 jQuery(function(){

         JSRai.init();

 });

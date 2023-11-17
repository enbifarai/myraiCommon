/*
*  jQuery contatore - v0.01
*  Un plugin per fare il contatore dei congedi
*
*  GitHub: 
 *  Copyright (c) 2021 - Vincenzo Bifano
*/

; (function ($, window, document) {
    'use strict';

    var debugLog = false;
    var IdElement = "";
    var Name = 'contatore';
    var fruitoperc = 0;
    var nonfruitoperc = 0;
    var persoperc = 0;
    var giorni = 0;
    var mesi = 0;

    var dataPlugin = 'plugin_' + Name,

        // default options
        defaults = {
            sesso: 'F',
            mesifruiti: 0,
            giornifruiti: 0,
            mesipratica: 0,
            giornipratica: 0,
            mesitotali: 10,
            giorniconiuge: 0,
            mesi: 0,
            mesipersi: 0,
            giornipersi: 0,
            mesiconiuge: 0,
            giorniconiuge: 0,
            mesibonuspadre: 1,
            mesiperbonuspadre: 3,
            mesiindennizati: 6,

            alert: 10,


            debug: false,
            onComplete: function () {
                _logger('default onComplete() event');
            }
        };

    var Contatore = function (element, options) {

        IdElement = $(element).attr('id');


        // salva elemento
        this.element = $(element);

        // override default options


        this.options = $.extend({}, defaults, options);
        debugLog = this.options.debug;


        this.element = element;
        this.$container = $(element);



        this.init();



    };
    //#region 
    /////////////////////////////////////
    //         Private methods         //
    /////////////////////////////////////
    /**
     *  Creo rem da pixel
     *
     *  @param   {Integer}  length  
     */

    var _makeRem = function (length) {
        return (parseInt(length) / _rem());
    }

    /**
     *  Leggo font size del document
     *
     *  @param   {}    
     */

    var _rem = function () {
        var html = document.getElementsByTagName('html')[0];

        return function () {
            return parseInt(window.getComputedStyle(html)['fontSize']);
        }
    }();

    /**
     *  Creo la log
     *
     *  @param   {string}  message 
     */
    var _logger = function (message) {

        if (debugLog) {
            console.log(message);
        }
    }

    var percentage = function (partialValue, totalValue) {
        return (100 * partialValue) / totalValue;
    }

    /**
     *  Creo contatore con stato zero
     *
     *  @param   {Object}  _this 
     */

    var _maketheCounterZero = function (_this) {
        $(_this.element).html('');
        var addDivSpan = "";
        addDivSpan += "<div class='grid-container'>";
        addDivSpan += "<div class='item1'><div class='summary-icon'><i class='icons icon-calendar'> </i></div></div>";

        addDivSpan += "<div class='item3 zerodata' >Non hai ancora preso periodi di congedo.</div>";

        addDivSpan += "</div>";

        $(_this.element).append((addDivSpan));
    }

    //#endregion

    /**
     *  Creo contatore
     *
     *  @param   {Object}  _this 
     */

    var _maketheCounter = function (_this) {
        var giorniConiuge = 0;
        var mesiConiuge = 0;
        nonfruitoperc = 0;

        //#region calcoli vari
        var mesigenitore = _this.options.mesi;
        var mesitotali = _this.options.mesitotali;
        var mesipersi = _this.options.mesipersi;
        var giornipersi = _this.options.giornipersi;



        if (_this.options.sesso == 'M') {

            if (parseInt(_this.options.mesifruiti) >= parseInt(_this.options.mesiperbonuspadre)) {

                // mesigenitore = _this.options.mesi+ _this.options.mesibonuspadre;
                mesitotali += 1;
                mesigenitore += _this.options.mesibonuspadre;;
            }
        }

        var mesirimanenzaind = 0;
        var giornirimanenzaind = 0;


        var meseinperc = 100 / mesigenitore;
        var giorniinperc = (meseinperc / 30);



        mesi = mesigenitore




        var mesiconiuge = Math.floor(_this.options.giorniconiuge / 30);
        var giorniconiuge = _this.options.giorniconiuge % 30;


        if (mesiconiuge >= (mesitotali - mesi)) {
            mesipersi = mesiconiuge - (mesitotali - mesi);
            giornipersi = giorniconiuge;
        }


        //#endregion


        if (mesipersi > 0)
            mesi = mesigenitore - mesipersi;

        giorni = 0;

        if (giornipersi > 0) {
            if (mesi > 0) {
                mesi = mesi - 1;
                giorni = 30 - giornipersi;
            }
            else {
                mesi = 0;
                giorni = 0
            }
            mesi = mesi - 1;
            giorni = 30 - giornipersi;
            _logger("giorni " + giorni);
        }
        // console.log("mesi " + mesi);
        // console.log("giorni " + giorni);
        _logger("mesi " + mesi);


        if ((_this.options.giornifruiti > 0)) {

            if (_this.options.giornifruiti > giorni) {
                mesi = mesi - 1;
                giorni = 30 - (_this.options.giornifruiti - giorni);
            }
            else {
                giorni = (giorni - _this.options.giornifruiti);
            }
        }

        if (_this.options.mesifruiti > 0) {
            mesi = mesi - _this.options.mesifruiti;
        }

        if (mesi < 0) {
            giorni = 0;
        }

        // console.log("mesi " + mesi);
        // console.log("giorni " + giorni);

        _logger("mesi - " + (mesi + " mesi  " + giorni + " giorni"));

        if (mesi > 0) {
            nonfruitoperc = ((mesi) * meseinperc);
            _logger("nonfruitoperc - " + (nonfruitoperc));
        }



        if (giorni > 0)
            nonfruitoperc += ((giorni) * giorniinperc);


        var versofine = "";
        _logger(_this.options.alert);
        if ((mesi == 0) && (giorni <= _this.options.alert))
            versofine = "-versofine";

        _logger("fruito - " + (fruitoperc));
        _logger("--> nonfruito - " + (nonfruitoperc));


        persoperc = (mesipersi * meseinperc) + (giornipersi * giorniinperc);
        _logger("persoperc - " + (persoperc));



        var mesifruiti = _this.options.mesifruiti + _this.options.mesipratica;
        var giornifruiti = _this.options.giornifruiti + _this.options.giornipratica;

        if ((mesifruiti) > _this.options.mesi) {
            mesifruiti = mesigenitore - mesipersi;
            giornifruiti = 0;
            console.log(mesifruiti);
        }

        fruitoperc = ((mesifruiti) * meseinperc) + ((giornifruiti) * giorniinperc);


        //verifico percentuale fruito

        if (fruitoperc > (100 - persoperc))
            fruitoperc = (100 - persoperc);

        var coeff = 20 + ((_this.options.mesi - 6) * 2);
        //coeff = coeff - 20;
        //console.log("coeff " + coeff);


        console.log("*******************************");
        console.log("mesitotali " + (_this.options.mesifruiti + _this.options.mesipratica));
        console.log("giornitotali " + (_this.options.giornifruiti + _this.options.giornipratica));

        console.log("sesso " + _this.options.sesso);
        console.log("mesigenitore " + mesigenitore);
        console.log("_this.options.mesifruiti " + _this.options.mesifruiti);
        console.log("_this.options.mesipratica " + _this.options.mesipratica);
        console.log("mesiconiuge " + mesiconiuge);
        console.log("giorniconiuge " + giorniconiuge);
        console.log("mesipersi " + mesipersi);
        console.log("giornipersi " + giornipersi);
        console.log("fruitoperc " + fruitoperc);
        console.log("nonfruitoperc " + nonfruitoperc);
        console.log("persoperc " + persoperc);
        console.log("*******************************");


        _logger(_this.options);
        $(_this.element).html('');



        //var addDiv = "<div class='intestazione' style='width: 100%'>Ti restano</div>";
        var addDiv = "<div class='intestazione' style='width: 100%'>Hai fruito di</div>";
        addDiv += "<div class='progress-contatore' >";

        addDiv += "<div class='progress-contatore-bar bg-fruito" + versofine + " first' style='width: " + fruitoperc + "%'></div>";
        addDiv += "<div class='progress-contatore-bar bg-non-fruito last' style='width: " + nonfruitoperc + "%'></div>";


        if (persoperc > 0)
            addDiv += "<div class='progress-contatore-bar progress-contatore-bar-perso' style='width: " + persoperc + "%'></div>";
        addDiv += "</div>";
        $(_this.element).append((addDiv));
        var addDivSpan = "<div style='white-space: nowrap; width: auto; >";
        var span = "";
        span = "<span subscript-line='0' class='span firstelement'>|</span>"
        addDivSpan += span;

        _logger(($(_this.element).width() - 10));
        for (var i = 1; i <= _this.options.mesi; i++) {
            _logger(_makeRem(($(_this.element).width() - 14) / _this.options.mesi));

            var ggspu = 0;
            if (_this.options.giornipersi != 0) { ggspu = 1; }
            //  console.log(_this.options.mesi);
            //  console.log(_this.options.mesipersi);
            //  console.log(_this.options.ggspu);
            if (i > ((_this.options.mesi - _this.options.mesipersi) - ggspu)) {
                // span = "<span class='span fuorielement' subscript-line='"+i+"' style='padding-left: "+(($( _this.element ).width()-coeff ) / _this.options.mesi )+"px'>|</span>"
                span = "<span class='span fuorielement' subscript-line='" + i + "' style='padding-left: " + percentage((($(_this.element).width() - coeff) / _this.options.mesi), $(_this.element).width() - ((10 + ((_this.options.mesi - 6) * 2)))) + "%'>|</span>"

            }
            else {
                span = "<span class='span' subscript-line='" + i + "' style='padding-left: " + percentage((($(_this.element).width() - coeff) / _this.options.mesi), $(_this.element).width() - ((10 + ((_this.options.mesi - 6) * 2)))) + "%'>|</span>"
            }

            addDivSpan += span;
        }
        addDivSpan += "</div>";
        addDivSpan = "";
        if (_this.options.mesifruiti > 0) {
            addDivSpan += "<br><div class='grid-container'>";

            addDivSpan += "<div class='item2 data' >" + _this.options.mesifruiti + "</div>";
            addDivSpan += "<div class='item3 text'>mesi</div>";
            addDivSpan += "<div class='item4 data'>" + _this.options.giornifruiti + "</div>";
            addDivSpan += "<div class='item5 text'>giorni</div>";
            addDivSpan += "<div class='item2 data'></div>";
            addDivSpan += "<div class='item3 scala contright'>di</div>";
            addDivSpan += "<div class='item4 scala contleft' style='padding-left: 5px;'>" + mesigenitore + " mesi</div>";
            addDivSpan += "<div class='item5 text'></div>";
            addDivSpan += "</div>";
        }
        else {
            addDivSpan += "<br><div class='grid-container'>";

            addDivSpan += "<div class='item2 data'></div>";
            addDivSpan += "<div class='item3 data'>" + _this.options.giornifruiti + "</div>";
            addDivSpan += "<div class='item4 text'>giorni</div>";
            addDivSpan += "<div class='item5 text'></div>";
            addDivSpan += "<div class='item3 scala contright'>di</div>";
            addDivSpan += "<div class='item4 scala contleft' style='padding-left: 5px;'>" + _this.options.mesi + " mesi</div>";
            addDivSpan += "<div class='item5 text'></div>";
            // scala
            addDivSpan += "</div>";
        }
        addDivSpan += "<div class='ituoicongedi' style='width: 100%'>I tuoi congedi</div>";

        addDivSpan += "<div class='grid-table'>";

        var strResiduo = ' mesi';



        addDivSpan += "<div class='titolo'>Totali</div>";
        addDivSpan += "<div class='titolo contright'>" + mesigenitore + " mesi</div>";






        strResiduo = '';
        if (mesi > 0)
            strResiduo = mesi + ' mesi';


        if (giorni > 0)
            strResiduo += ' ' + giorni + ' giorni';

        //  addDivSpan += "<div class='titolo'>Restanti</div>";
        //  addDivSpan += "<div class='titolo right'>" + strResiduo + " </div>";
        addDivSpan += "</div>";

        addDivSpan += "<div class='ituoicongedi' style='width: 100%'>Congedi altro genitore</div>";
        addDivSpan += "<div class='grid-table'>";

        strResiduo = '';
        if (mesiconiuge > 0)
            strResiduo = mesiconiuge + ' mesi';


        if (giorniconiuge > 0)
            strResiduo += ' ' + giorniconiuge + ' giorni';

        addDivSpan += "<div class='titolo'>Utilizzati</div>";
        addDivSpan += "<div class='titolo contright'>" + strResiduo + "</div>";

        addDivSpan += "</div>";

        addDivSpan += "<div class='ituoicongedi' style='width: 100%'>Riferimenti per i due genitori</div>";
        addDivSpan += "<div class='grid-table'>";

        strResiduo = '0';
        if (_this.options.mesipersi > 0)
            strResiduo = mesi + ' mesi';


        if (_this.options.giornipersi > 0)
            strResiduo += ' ' + giorni + ' giorni';

        addDivSpan += "<div class='titolo'>Mesi indennizati</div>";
        addDivSpan += "<div class='titolo contright'>" + _this.options.mesiindennizati + "</div>";
        addDivSpan += "<div class='titolo'>Mesi Totali</div>";
        addDivSpan += "<div class='titolo contright'>" + mesitotali + "</div>";
        addDivSpan += "</div>";


        if ((mesipersi > 0) || (giornipersi > 0)) {
            addDivSpan += "<div class='grid-container-white'>";

            var mesico = "<b>" + (mesiconiuge) + "</b> mesi ";
            if (mesico == 0)
                mesico = "";



            var giornico = "<b>" + (giorniconiuge) + "</b> giorni ";
            if (giorniconiuge == 0)
                giornico = "";

            if (giornico != "") {
                if (mesico != "")
                    mesico = mesico + " e ";
            }



            addDivSpan += "<div class='item3 textdata'>Il tuo tetto massimo non &egrave; pi&ugrave; di <b>" + _this.options.mesi + "</b> mesi  perch&egrave; l'altro genitore ha gi&agrave; usufruito  di pi&ugrave; di  <b>" + (_this.options.mesitotali - _this.options.mesi) + "</b> mesi di congedo.</div>";

            addDivSpan += "</div>";

        }


        $(_this.element).append((addDivSpan));

        var grid = (($(_this.element).width() - coeff) / _this.options.mesi) + "px ";
        grid = "auto ";
        for (var i = 1; i <= _this.options.mesi; i++) {

            grid += "auto ";

        }




        addDivSpan = "<br><div style='display: grid;grid-template-columns: " + grid + "'; background: #F6F8F8;'>";

        addDivSpan += "<div class='span'>0</div>";
        for (var i = 1; i <= _this.options.mesi; i++) {

            addDivSpan += "<div class='span' style='align-self: flex-end; text-align: right;'>" + i + "</div>";
        }

        addDivSpan += "</div>";
        // $( _this.element ).append((addDivSpan));
        /*  addDivSpan = "<br><hr class='divisore'>";
  
          addDivSpan += "<button class='accordionCont'>Come funziona?</button>";
          addDivSpan += "<div class='panelCont'>";
          addDivSpan += "  <p>Lorem ipsum...</p>";
          addDivSpan += "</div>";
  
          $( _this.element ).append((addDivSpan));*/


    }

    ////////////////////////////////////
    //         Public methods         //
    ////////////////////////////////////
    Contatore.prototype = {

        /**
         *  Inizializza il contatore.
         *  Chiamato automaticamente quando si crea il plugin
         *
         *  @private
         */
        init: function () {

            var _this = this.element;

            // if ((this.options.mesifruiti == 0) && (this.options.giornifruiti == 0)) {
            _maketheCounterZero(this);
            // }
            //else {
            _maketheCounter(this);
            //}


            // trigger onComplete callback
            if (this.options.onComplete && typeof (this.options.onComplete) == "function")
                this.options.onComplete();
        },
        cambiaValore: function (options) {
            $(this.element).html('');
            this.options = $.extend({}, this.options, options);
            _maketheCounter(this);
        },

        giorniRimanenti: function () {

            if ((mesi < 0) || (giorni < 0)) {
                mesi = -1
                giorni = -1;
            }

            var rimanenti =
            {
                mesi: mesi,
                giorni: giorni
            }
            return rimanenti;


        }


    };

    //////////////////////////////////////////////////
    //         Plugin wrapper                       //
    //////////////////////////////////////////////////

    // A plugin wrapper around the constructor, preventing against multiple instantiations
    $.fn[Name] = function (options) {
        var instance;

        // If the first parameter is an object (options), or was omitted,
        // call Plugin.init()
        if (typeof options === 'undefined' || typeof options === 'object') {
            return this.each(function () {
                // prevent multiple instantiations
                if (!$.data(this, dataPlugin)) {
                    $.data(
                        this,
                        dataPlugin,
                        new Contatore(this, options)
                    );
                }

                instance = $(this).data(dataPlugin);

                if (typeof instance['init'] === 'function') {
                    instance.init();
                }
            });

            // checks that the requested public method exists
        } else if (typeof options === 'string') {
            var methodName = arguments[0],
                args = Array.prototype.slice.call(arguments, 1),
                returnVal;

            this.each(function () {
                var instance = $(this).data(dataPlugin);

                // Check that the element has a plugin instance, and that
                // the requested public method exists.
                if ($.data(this, dataPlugin) && typeof $.data(this, dataPlugin)[methodName] === 'function') {
                    // Call the method of the Plugin instance, and Pass it
                    // the supplied arguments.
                    returnVal = $.data(this, dataPlugin)[methodName].apply(instance, args);
                } else {
                    console.info('Method ' + options + ' does not exist on jQuery.' + Name);
                }
            });

            if (typeof returnVal !== 'undefined') {
                // If the method returned a value, return the value
                return returnVal;
            } else {
                // Otherwise, returning 'this' preserves chainability
                return this;
            }
        } else {
            console.info('Method ' + options + ' does not exist on jQuery.' + Name);
        }
    };

})(jQuery, window, document);



/*
$( window ).bind("resize", function(){
    // Change the width of the div
    $("#yourdiv").width( 600 );
});


        <div id="congedi" style="padding: 25%;">

        </div>



 var instance = $('#congedi').contatore({
                    mesifruiti:2,
                    giornifruiti:0,
                    debug: false,
                    alert:10,
                    mesipersi: 2,
                    giornipersi:20,
                    mesi: 6,
                    onComplete: function() {

                    }
                  });

*/
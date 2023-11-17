function ValidazioneSpecifica() {

    //UMH
    var d = $("#param-dalle").find("input").val();
    var a = $("#param-alle").find("input").val();
    var mindalle = 0;
    var minalle = 0;

    if (d.indexOf("_") < 0 && $.trim(d) != "") {
        var h = Number(d.substring(2, 0));
        var m = Number(d.substring(5, 3));
        mindalle = (h * 60) + m;
    }
    if (a.indexOf("_") < 0 && $.trim(a) != "") {
        var h = Number(a.substring(2, 0));
        var m = Number(a.substring(5, 3));
        minalle = (h * 60) + m;
    }
    if (mindalle == 0 || minalle == 0 || minalle < mindalle) {
        ValidazioneErrore("Parametri non impostati correttamente");
        return;
    }
    var minutiMensa = 0;
    var intervalloMensa = $(".modal-content #timbraturetoday").attr("data-minuti-mensa");
    if (intervalloMensa.length != 5 || $.trim(intervalloMensa) == "" || intervalloMensa.indexOf(":") < 0) {
        ValidazioneErrore("Parametro intervallo mensa non corretto");
        return;
    }
    else {
        var h = Number(intervalloMensa.substring(2, 0));
        var m = Number(intervalloMensa.substring(5, 3));
        minutiMensa = (h * 60) + m;
    }
    var diffMinuti = minalle - mindalle;
    if (diffMinuti > minutiMensa)
    {
        ValidazioneErrore("Il periodo richiesto è piu esteso dell\'intervallo mensa");
        return;
    }

    if (mindalle >= 720 && mindalle <= 915 && minalle >= 720 && minalle <= 915) {
        ValidazioneOK();
        return;
    }
    if (mindalle >= 1140 && mindalle <= 1260 && minalle >= 1140 && minalle <= 1260) {
        ValidazioneOK();
        return;
    }
    ValidazioneErrore("Gli orari devono essere compresi negli intervalli 12:00/15:15 o 19:00/21:00");

}
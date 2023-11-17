self.onmessage= function (event) {
    var elem = event.data.split('_');
    if (elem[0] == 'start') {

        RaiSTExecCall(elem[1], elem[2]);
        //self.close();
    }
};

function RaiSTExecCall(idProc, numCall) {
    var url = '/RaiDesign/RaiSTWebServiceCall';

    
    for (var i = 0; i < numCall; i++) {
        var req = new XMLHttpRequest();
        req.open("POST", url, true);
        req.responseType = 'json';
        req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        req.onreadystatechange = function () {
            if (this.readyState === 4 && this.status === 200) {
                return self.postMessage(this.response);
            }
        };

        req.send('idProc='+idProc+'&idCall='+i);

        //$.ajax({
        //    async: 'true',
        //    url: url,
        //    type: "GET",
        //    dataType: "json",
        //    data: { idProc: idProc, idCall: i },
        //    cache: false,
        //    success: function (data) {
        //        self.postMessage(data);
        //    },
        //    error: function (a, b, c) {
        //        self.postMessage({ IdProc: idProc, IdCall: 0, Esito: false, Message: 'CALL ERROR: '+ a + '<br/>+' + b + '<br/>' + c, Durata:0});
        //    },
        //    complete: function () {
                
        //    }
        //});
    }
}


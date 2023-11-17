

var MapsAlberghi = function() {
    // Gmaps.js,  https://hpneo.github.io/gmaps/

    // Inizializza la ricerca nella mappa
    var initMapSearch = function(){
        // Inizializzo la mappa

        GMaps.geolocate({
            success: function (position) {

                $.getJSON("../assets/data/alberghi.json", function (data) {
                    var items = [];
                    
                    //  console.log(data.valore.TuttigliHotelCompleto);
                    $.each(data.valore.TuttigliHotelCompleto, function (i, item) {

                        var it = {
                            lat: item.lat,
                            lng: item.lng,
                            title: item.Nome,
                            animation: google.maps.Animation.DROP,
                           // infoWindow: { content: '<div class="block"><div class="block-header"><h3 class="block-title">' + item.Nome + '</h3></div><div class="block-content"><p>Subset..</p></div></div>' }
                            infoWindow: { content: '<div class="block"><div class="block-content"><div class="push"><span class="text-primary font-w600">' + item.Stelle + '</span></div><h4 class="pull-t">' + item.Nome + '</h4><address>' + item.Indirizzo + '<br>' + item.Cap + ' '+item.NomeCitta+ ' ('+item.SiglaProvincia+')<br><abbr title="Phone">T:</abbr> '+item.Telefono+'</address></div></div>' }
                        }

                        //console.log(it);
                        items.push(it);
                    });

                    
                        var gmapGeolocation = new GMaps({
                            div: '#js-map-search',
                            lat: 0,
                            lng: 0,
                            scrollwheel: true
                        });





                    gmapGeolocation.setCenter(position.coords.latitude, position.coords.longitude);
                    gmapGeolocation.addMarker({
                        lat: position.coords.latitude,
                        lng: position.coords.longitude,
                        animation: google.maps.Animation.DROP,
                        icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png',
                        title: 'Sei qui...',
                        infoWindow: {
                            content: '<div class="text-success"><i class="fa fa-map-marker"></i> <strong>La tua posizione!</strong></div>'
                        }
                    });
                    gmapGeolocation.addMarkers(items);

                    // Quando il form è submitted
                    jQuery('.js-form-search').on('submit', function () {
                        GMaps.geocode({
                            address: jQuery('.js-search-address').val().trim(),
                            callback: function ($results, $status) {
                                if (($status === 'OK') && $results) {
                                    var $latlng = $results[0].geometry.location;
                                    console.log($results[0].geometry);
                                    
                                    gmapGeolocation.addMarker({
                                        lat: $latlng.lat(), lng: $latlng.lng(),
                                        icon: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png',
                                        infoWindow: {
                                            content: '<div class="text-info"><i class="fa fa-map-marker"></i> <strong>Ricerca!</strong></div>'
                                        }
                                    });
                                    gmapGeolocation.fitBounds($results[0].geometry.viewport);
                                } else {
                                    alert('Indirizzo non trovato!');
                                }
                            }
                        });

                        return false;
                    });
                }).done(function () {
                    console.log("second success");
                })
                      .fail(function () {
                          console.log("error");
                      })
                      .always(function () {
                          console.log("complete");
                      });



            },
            error: function (error) {
                alert('Geolocalizzazione fallita: ' + error.message);
            },
            not_supported: function () {
                alert("Il tuo browser non supporta la geolocalizzazione");
            },
            always: function () {
                // Message when geolocation succeed
            }
        });





    };


    return {
        init: function () {
            
            initMapSearch();

            
        }
    };
}();

// Inizializzazione all'avvio 
jQuery(function () { MapsAlberghi.init(); });

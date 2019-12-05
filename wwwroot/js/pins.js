'use strict';

var Mustache = require('mustache');

function getPinTemplate(pin) {
    var template = $('#pin-template').html();
    return Mustache.render(template, {
        category: pin.pinCategory.toLowerCase(),
        type: pin.pinType.toLowerCase(),
        title: pin.title,
        description: pin.description
    });
}

app.getUserPins = function(email) {
    var $loader = $('.loader');
    return $.ajax({
        url: './NearMe/GetUserPins/',
        contentType: "application/json; charset=utf-8",
        data: { email : email },
        beforeSend: function () {
            $loader.removeClass('d-none');
        }
    }).done(function () {
        $loader.addClass('d-none');
    });
};

app.loadUserPins = function(marker, map, email) {
    marker.addListener('click', function() {
        var $pins = $('.pins').empty();
        app.getUserPins(email).always(function (pins) {
            var i;
            for (i = 0; i < pins.length; i++) {
                var pin = pins[i];
                $pins.append(getPinTemplate(pin));
            }
            require('./rating')();
        });
    });
};

var markers = [];
var bounds,
    map,
    zoomListener, 
    zoomScale;

function buildLegend() {
    var icons = {
        parking: {
            name: 'Offers',
            icon: '<span class="offers"></span>'
        },
        library: {
            name: 'Requests',
            icon: '<span class="requests"></span>'
        },
        info: {
            name: 'Both',
            icon: '<span class="both"></span>'
        }
    };
    var legend = document.getElementById('legend');

    for (var key in icons) {
        var type = icons[key];
        var name = type.name;
        var icon = type.icon;
        var div = document.createElement('div');
        div.innerHTML = icon + ' ' + name;
        legend.appendChild(div);
    }

    map.controls[google.maps.ControlPosition.RIGHT_BOTTOM]
        .push(document.getElementById('legend'));
}

function setMapZoom() {
    if (map.getZoom() >= zoomScale) {
        map.setZoom(zoomScale);
    }
}

function fitBounds() {
    if (window.outerWidth > 576) {
        var offsetLeft = document.querySelector('.pins-container').offsetWidth;
        map.fitBounds(bounds, {left: offsetLeft + 100});

        zoomScale = 12;
        zoomListener = google.maps.event.addListenerOnce(map, 'bounds_changed', setMapZoom);
    } else {
        zoomScale = 11;
        zoomListener = google.maps.event.addListenerOnce(map, 'bounds_changed', setMapZoom);

        map.fitBounds(bounds);
    }

    setTimeout(function() {
        google.maps.event.removeListener(zoomListener);
    }, 2000);
}

// Initialize and add the map
app.initMap = function(coords, locations) {
    var lat = Number(coords.lat);
    var lng = Number(coords.lng);
    map = new google.maps.Map(document.getElementById('map'), {
        center: {lat: lat, lng: lng},
        mapTypeId: 'terrain',
        mapTypeControl: false,
        streetViewControl: false,
        zoomControl: true,
        zoomControlOptions: {
            position: google.maps.ControlPosition.TOP_RIGHT
        }
     });

    buildLegend();

    bounds = new google.maps.LatLngBounds();

    for (var i = 0; i < locations.length; i++)
    {
        var marker = new google.maps.Marker({
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                fillOpacity: 0.5,
                fillColor: locations[i].positionColor,
                strokeOpacity: 0.75,
                strokeColor: locations[i].positionColor,
                strokeWeight: 2,
                scale: 35
            },
            map: map,
            position: locations[i].position
        });

        app.loadUserPins(marker, map, locations[i].email);

        marker.setMap(map);
        markers.push(marker);
    }

    for (var i = 0; i < markers.length; i++) {
        bounds.extend(markers[i].position);
    }

    fitBounds();
};

window.onresize = function() {
    fitBounds();
};

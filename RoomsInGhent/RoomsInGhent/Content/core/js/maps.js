
var map;
var pref = "";
var schoolMarker;

function initMaps() {

    var mapOptions = {
        center: new google.maps.LatLng(51.053468, 3.73038),
        zoom: 13,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map_canvas"),
        mapOptions);
    $.ajax({
        url: '/api/RoomsApi/GetAll',
        type: 'get',
        dataType: 'json',
        success: function (result) {

            for (var i = 0; i < result.length; i++) {
                var room = result[i];

                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(room.lat, room.lng),
                    map: map,
                    animation: google.maps.Animation.DROP
                });

                var wind = new google.maps.InfoWindow();
                bindWindow(marker, buildContent(room.mainImg, room.address, room.price, room.link), wind);
            }
        }
    });
}

function buildContent(img, address, price, link) {
    var str = //Image
'<figure><a href="' + pref + link + '"><img src="' + pref + img + '" alt="" style="max-height:100px;max-width:150px" /></a></figure>' +
'<div style="float:left">' +
    '<p>' + address + '</p>' +
    '<p>' + price + '&euro;</p>' +
    '<a href="' + pref + link + '">Lees meer</a>' +
'</div>'
    return str;
}

function bindWindow(marker, contentString, infowindow) {
    google.maps.event.addListener(marker, 'click', function () {
        infowindow.setContent(contentString);
        infowindow.open(map, marker);
    });
}



function getNear(address) {
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'address': address }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {

            var latlng = results[0].geometry.location;
            var lat = latlng.lat();
            var lng = latlng.lng();

            //var greenIcon = "http://www.google.com/intl/en_us/mapfiles/ms/micons/green-dot.png";
            //schoolMarker = new google.maps.Marker({
            //    position: new google.maps.LatLng(latlng.lat, latlng.lng),
            //    map: map,
            //    icon: greenIcon
            //});

            map.setZoom(14);
            map.setCenter(latlng);

            $("#recent_rooms").css("display", "none");
            $("#found_rooms").css("display", "block");

            $.ajax({
                url: '/api/RoomsApi/GetNear',
                method: 'get',
                data: { par1: lat, par2: lng, par3: 5 },
                dataType: 'json',
                success: function (results) {

                    var str = "";

                    for (var i = 0; i < results.length; i++) {
                        str += generateRoom(results[i]);
                    }

                    $("#fRooms").html(str);
                }
            });
        }
    });
}


function generateRoom(room) {
    var str = '<article class="room-small row">\n' +
        '\t<a href="' + pref + room.link + '">\n' +
        '\t\t<figure class="span1">\n' +
        '\t\t\t<img src="' + pref + room.mainImg + '" alt="kot foto" />\n' +
        '\t\t</figure>\n' +
        '\t</a>\n' +
        '\t<div class="room-info">\n' +
        '\t\t<p class="room-size pull-right"><a href="' + pref + '/Rooms/Index?minSize=' + (room.size - 2) + '&maxSize=' + (room.Size + 2) + '">' + room.size + 'm²</a></p>\n' +
        '\t\t<p class="room-address">' + room.address + '</p>\n' +
        '\t\t<p class="room-region">' + room.region + '</p>\n' +
        '\t\t<p class="room-price">\n';

    if (room.included) {
        str += '\t\t\t<span class="badge badge-success pull-right" title="kosten inbegrepen">' + room.price + '€</span>\n';
    } else {
        str += '\t\t\t<span class="badge badge-warning pull-right" title="kosten niet inbegrepen">' + room.price + '€</span>\n';
    }

    str += '\t\t</p>\n' +
        '\t</div>\n' +
        '\t<a href="' + pref + room.link + '">Lees meer</a>\n' +
        '</article>\n';

    return str;
}





$(function () {
    var script = document.createElement("script");
    script.src = "https://maps.googleapis.com/maps/api/js?key=AIzaSyDM7r3mQA7nMDhjxc1Azfa2J6ODTgTP_3I&sensor=false&callback=initMaps";
    document.body.appendChild(script);

    $("#schoolSel").change(function () {
        if ($("#schoolSel").val() != -1) {
            getNear($("#schoolSel").val());
        }
    });

    $("#addresInp").change(function () {
        if ($("#addresInp").val() != "") {
            getNear($("#addresInp").val());
        }
    })
});
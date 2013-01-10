var roomsPer = 5;
var map;
var schoolMarker;
var ownPref = "";

function initMaps() {

    var mapOptions = {
        center: new google.maps.LatLng(51.053468, 3.73038),
        zoom: 13,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
    map = new google.maps.Map(document.getElementById("map_canvas"),
        mapOptions);
    $.ajax({
        url: ownPref + '/api/RoomsApi/GetAll',
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
                bindWindow(marker, buildContent(ownPref, room.mainImg, room.address, room.price, room.link), wind);
            }
        }
    });
    $.ajax({
        url: 'http://nicolasquartier.ikdoeict.be/app2/api/roomsapi/getallrooms',
        type: 'get',
        datatype: 'json',
        success: function (result) {
            for (var i = 0; i < result.length; i++) {
                var room = result[i];

                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(room.latitude, room.longitude),
                    map: map,
                    animation: google.maps.Animation.DROP
                });

                var wind = new google.maps.InfoWindow();
                bindWindow(marker, buildContent("http://nicolasquartier.ikdoeict.be/app2/", "Content/images/" + room.images.split(',')[0], room.adres + " " + room.number, room.rent, "Rooms/Details/" + room.id), wind);
            }
        }
    });
    $.ajax({
        url: 'http://jenstrio.ikdoeict.be/app1/api/rooms/getall',
        type: 'get',
        datatype: 'json',
        success: function (result) {
            for (var i = 0; i < result.length; i++) {
                var room = result[i];

                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(room.latitute, room.longitude),
                    map: map,
                    animation: google.maps.Animation.DROP
                });

                var wind = new google.maps.InfoWindow();
                bindWindow(marker, buildContent("", room.image, room.address, room.price, "http://jenstrio.ikdoeict.be/app0/Rooms/Detail/" + room.id), wind);
            }
        }
    });
}

function buildContent(pref, img, address, price, link) {
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
            if (schoolMarker) schoolMarker.setMap(null);
            schoolMarker = new google.maps.Marker({
                position: latlng,
                map: map,
                icon: ownPref + '/Content/core/img/green-dot.png',
                title: address
            });

            // Array to contain all rooms
            var rooms = new Array();
            var counter = 0;
            var amount = 3;

            map.setZoom(14);
            map.setCenter(latlng);

            $("#recent_rooms").css("display", "none");
            $("#found_rooms").css("display", "block");

            // GetMy rooms
            $.ajax({
                url: ownPref + '/api/RoomsApi/GetNear',
                method: 'get',
                data: { par1: lat, par2: lng, par3: roomsPer },
                dataType: 'json',
                success: function (results) {
                    for (var i = 0; i < results.length; i++) {
                        var room = results[i];

                        rooms.push({
                            string: generateRoom(room,  calculateDistance(latlng, room.lat, room.lng)),
                            distance: calcDist(latlng, room.lat, room.lng)
                        });
                    }

                    counter++;

                    if (counter == amount) {
                        var str = "";
                        rooms.sort(function (a, b) {
                            return a.distance - b.distance;
                        });
                        for (var i = 0; i < rooms.length; i++) {
                            str += rooms[i].string;
                        }
                        $("#fRooms").html(str);
                    }
                }
            });

            $.ajax({
                url: 'http://nicolasquartier.ikdoeict.be/app2/api/roomsapi/getallrooms',
                type: 'get',
                datatype: 'json',
                success: function (results) {
                    results.sort(function (a, b) {
                        return Math.sqrt(Math.pow(lat - a.latitude, 2) + Math.pow(lng - a.longitude, 2)) - Math.sqrt(Math.pow(lat - b.latitude, 2) + Math.pow(lng - b.longitude, 2));
                    });

                    for (var i = 0; i < results.length && i < roomsPer; i++) {
                        var room = results[i];

                        rooms.push({
                            string: generateAnonRoom("http://nicolasquartier.ikdoeict.be/app2/",
                                "Rooms/Details/" + room.id,
                                "Content/images/" + room.images.split(',')[0],
                                room.size,
                                room.adres + " " + room.number,
                                '&nbsp;',
                                room.rent,
                                true,
                                calculateDistance(latlng, room.latitude, room.longitude)),
                            distance: calcDist(latlng, room.latitude, room.longitude)
                        });
                    }

                    counter++;

                    if (counter == amount) {
                        var str = "";
                        rooms.sort(function (a, b) {
                            return a.distance - b.distance;
                        });
                        for (var i = 0; i < rooms.length; i++) {
                            str += rooms[i].string;
                        }
                        $("#fRooms").html(str);
                    }
                }
            });

            $.ajax({
                url: 'http://jenstrio.ikdoeict.be/app1/api/rooms/getall',
                type: 'get',
                datatype: 'json',
                success: function (results) {
                    results.sort(function (a, b) {
                        return Math.sqrt(Math.pow(lat - a.latitute, 2) + Math.pow(lng - a.longitude, 2)) - Math.sqrt(Math.pow(lat - b.latitute, 2) + Math.pow(lng - b.longitude, 2));
                    });

                    for (var i = 0; i < results.length && i < roomsPer; i++) {
                        var room = results[i];

                        rooms.push({
                            string: generateAnonRoom("",
                                "http://jenstrio.ikdoeict.be/app0/Rooms/Detail/" + room.id,
                                room.image,
                                room.size,
                                room.address,
                                '&nbsp;',
                                room.price,
                                true,
                                calculateDistance(latlng, room.latitute, room.longitude)),
                            distance: calcDist(latlng, room.latitute, room.longitude)
                        });
                    }

                    counter++;

                    if (counter == amount) {
                        var str = "";
                        rooms.sort(function (a, b) {
                            return a.distance - b.distance;
                        });
                        for (var i = 0; i < rooms.length; i++) {
                            str += rooms[i].string;
                        }
                        $("#fRooms").html(str);
                    }
                }
            });
        }
    });
}


function generateRoom(room, distance) {
    var str = '<article class="room-small row">\n' +
        '\t<a href="' + ownPref + room.link + '">\n' +
        '\t\t<figure class="span1">\n' +
        '\t\t\t<img src="' + ownPref + room.mainImg + '" alt="kot foto" />\n' +
        '\t\t</figure>\n' +
        '\t</a>\n' +
        '\t<div class="room-info">\n' +
        '\t\t<p class="room-size pull-right"><a href="' + ownPref + '/Rooms/Index?minSize=' + (room.size - 2) + '&maxSize=' + (room.size + 2) + '">' + room.size + 'm²</a></p>\n' +
        '\t\t<p class="room-address">' + room.address + '</p>\n' +
        '\t\t<p class="room-distance pull-right">' + distance + '</p>\n' +
        '\t\t<p class="room-region">' + room.region + '</p>\n' +
        '\t\t<p class="room-price">\n';

    if (room.included) {
        str += '\t\t\t<span class="badge badge-success pull-right" title="kosten inbegrepen">' + room.price + '€</span>\n';
    } else {
        str += '\t\t\t<span class="badge badge-warning pull-right" title="kosten niet inbegrepen">' + room.price + '€</span>\n';
    }

    str += '\t\t</p>\n' +
        '\t</div>\n' +
        '\t<a href="' + ownPref + room.link + '">Lees meer</a>\n' +
        '</article>\n';

    return str;
}

function generateAnonRoom(pref, link, img, size, address, region, price, included, distance) {
    var str = '<article class="room-small row">\n' +
        '\t<a href="' + pref + link + '">\n' +
        '\t\t<figure class="span1">\n' +
        '\t\t\t<img src="' + pref + img + '" alt="kot foto" />\n' +
        '\t\t</figure>\n' +
        '\t</a>\n' +
        '\t<div class="room-info">\n' +
        '\t\t<p class="room-size pull-right"><a href="~/Rooms/Index?minSize=' + (size - 2) + '&maxSize=' + (size + 2) + '">' + size + 'm²</a></p>\n' +
        '\t\t<p class="room-address">' + address + '</p>\n' +
        '\t\t<p class="room-distance pull-right">' + distance + '</p>\n' +
        '\t\t<p class="room-region">' + region + '</p>\n' +
        '\t\t<p class="room-price">\n';

    if (included) {
        str += '\t\t\t<span class="badge badge-success pull-right" title="kosten inbegrepen">' + price + '€</span>\n';
    } else {
        str += '\t\t\t<span class="badge badge-warning pull-right" title="kosten niet inbegrepen">' + price + '€</span>\n';
    }

    str += '\t\t</p>\n' +
        '\t</div>\n' +
        '\t<a href="' + pref + link + '">Lees meer</a>\n' +
        '</article>\n';

    return str;
}

function calculateDistance(latlng, lat, lng) {
    var dist = calcDist(latlng, lat, lng);
    if (dist < 1000) return dist + "m";
    else {
        return Math.round(dist / 100) / 10 + "km";
    }
}

function calcDist(latlng, lat, lng) {
    return Math.round(google.maps.geometry.spherical.computeDistanceBetween(latlng, new google.maps.LatLng(lat, lng)));;
}




$(function () {
    var script = document.createElement("script");
    script.src = "https://maps.googleapis.com/maps/api/js?key=AIzaSyDM7r3mQA7nMDhjxc1Azfa2J6ODTgTP_3I&sensor=false&callback=initMaps&libraries=geometry";
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
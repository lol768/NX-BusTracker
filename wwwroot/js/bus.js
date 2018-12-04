(function () {
    var map;
    document.addEventListener("DOMContentLoaded", e => {
        map = L.map('map').setView([52.35, -1.5], 12);
        L.tileLayer('https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token={accessToken}', {
            attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
            maxZoom: 18,
            id: 'mapbox.streets',
            accessToken: 'pk.eyJ1IjoiZXNwZXJuZXQiLCJhIjoiY2psdHUyanIxMDh1ajNxcXZjcW52czkxdCJ9.-7mwU0MKux72kJoGcrVOfg'
        }).addTo(map);
        grabData();
    });
    var oldMarkers = [];
    var blueIcon = L.icon({
        iconUrl: '/images/bus_blue.svg',
        iconSize:     [32, 32], // size of the icon
        iconAnchor:   [16, 16], // point of the icon which will correspond to marker's location
        popupAnchor:  [0, 0] // point from which the popup should open relative to the iconAnchor
    });
    var greenIcon = L.icon({
        iconUrl: '/images/bus_green.svg',
        iconSize:     [32, 32], // size of the icon
        iconAnchor:   [16, 16], // point of the icon which will correspond to marker's location
        popupAnchor:  [0, 0] // point from which the popup should open relative to the iconAnchor
    });

    var purpleIcon = L.icon({
        iconUrl: '/images/bus_purple.svg',
        iconSize:     [32, 32], // size of the icon
        iconAnchor:   [16, 16], // point of the icon which will correspond to marker's location
        popupAnchor:  [0, 0] // point from which the popup should open relative to the iconAnchor
    });

    var blackIcon = L.icon({
        iconUrl: '/images/bus_black.svg',
        iconSize:     [32, 32], // size of the icon
        iconAnchor:   [16, 16], // point of the icon which will correspond to marker's location
        popupAnchor:  [0, 0] // point from which the popup should open relative to the iconAnchor
    });

    var stopIcon = L.icon({
        iconUrl: '/images/stop.svg',
        iconSize:     [8, 8], // size of the icon
        iconAnchor:   [4, 4], // point of the icon which will correspond to marker's location
        popupAnchor:  [0, 0] // point from which the popup should open relative to the iconAnchor
    });
    
    var plottedRoutes = {};
    
    setInterval(grabData, 10000);
    grabData();
    plotRoutes();
    
    $("input").on("click", function() {
       if (!$(this).prop("checked")) {
           plottedRoutes[$(this).attr("name")].remove();
       } else {
           plottedRoutes[$(this).attr("name")].addTo(map);
       }
    });
    
    var existingStops = {};

    function makeStopMarker(stop, text) {
        if (stop.atcoCode in existingStops) {
            var y = document.createElement("span");

            y.textContent = text;
            existingStops[stop.atcoCode].innerHTML = existingStops[stop.atcoCode].innerHTML + "<br />" + y.innerHTML;
            return;
        }
        var marker = L.marker([stop.latitude, stop.longitude], {"icon": stopIcon}).addTo(map);
        var popup = document.createElement("h4");

        popup.textContent = stop.name;
        var container = document.createElement("span");
        container.appendChild(popup);

        container.innerHTML = container.innerHTML + text;
        marker.bindPopup(container);
        existingStops[stop.atcoCode] = container;
    }

    function plotRoutes() {
        fetch("/js/11-inbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                makeStopMarker(stop, "11, towards Coventry");
            }
            var latlngs = [];
            for (var point of data.waypoints) {
                latlngs.push([point.latitude, point.longitude]);
            }
            var polyline = L.polyline(latlngs, {color: 'red'});
            plottedRoutes["11-inbound.json"] = polyline;
        });

        fetch("/js/11-outbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                makeStopMarker(stop, "11, towards Leamington");
            }
            var latlngs = [];
            for (var point of data.waypoints) {
                latlngs.push([point.latitude, point.longitude]);
            }
            var polyline = L.polyline(latlngs, {color: 'blue'});
            plottedRoutes["11-outbound.json"] = polyline;
        });

        fetch("/js/12X-outbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                makeStopMarker(stop, "12X, towards Warwick Uni");
            }
            var latlngs = [];
            for (var point of data.waypoints) {
                latlngs.push([point.latitude, point.longitude]);
            }
            var polyline = L.polyline(latlngs, {color: 'green'});
            plottedRoutes["12X-outbound.json"] = polyline;


        });

        fetch("/js/12X-inbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                makeStopMarker(stop, "12X, towards Coventry");
            }
            var latlngs = [];
            for (var point of data.waypoints) {
                latlngs.push([point.latitude, point.longitude]);
            }
            var polyline = L.polyline(latlngs, {color: 'purple'});
            plottedRoutes["12X-inbound.json"] = polyline;
        });
    }

    function grabData() {
        $(".glyphicon-refresh").fadeIn();
        fetch("/Home/Data").then(r => r.json()).then(data => {
            for (var m of oldMarkers) {
                map.removeLayer(m);
            }
            for (var key in data) {
                for (var busLoc of data[key]) {
                    let icon = key.includes("11") ? blueIcon : greenIcon;
                    if (key.includes("11U")) {
                        icon = purpleIcon;
                    }

                    if (key.includes("18")) {
                        icon = blackIcon;
                    }
                    var marker = L.marker([busLoc.latitude, busLoc.longitude], {"icon": icon}).addTo(map);
                    var popup = document.createElement("span");
                    
                    popup.innerHTML = mapName(key);
                    marker.bindPopup(popup);
                    oldMarkers.push(marker);
                }
            }
            $(".glyphicon-refresh").fadeOut();

        });
    }
    
    function mapName(name) {
        switch(name) {
            case "11-Inbound": return "<h4>11 to Coventry</h4>via Kenilworth";
            case "11-Outbound": return "<h4>11 to Leamington</h4>via Kenilworth";
            case "11U-Outbound": return "<h4>11 to Warwick Uni</h4>via Kenilworth";
            case "11U-Inbound": return "<h4>11U to Earlsdon</h4>via Kenilworth";
            case "12X-Outbound": return "<h4>12X to Warwick Uni</h4>Express Service";
            case "12X-Inbound": return "<h4>12X to Coventry</h4>Express Service";
            case "18-Inbound": return "<h4>18 to Coventry</h4>via Hearsall Common";
            case "18A-Outbound": return "<h4>18 to Tile Hill</h4>via Hearsall Common";
            case "18-Outbound": return "<h4>18A to Tile Hill</h4>via Cannon Hill Road";
            case "18A-Inbound": return "<h4>18A to Coventry</h4>via Cannon Hill Road";
        }
        return name;
    }
})();


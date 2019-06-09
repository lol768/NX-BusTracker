(function () {
    var map;
    var knownStops = {};
    var darkMode = document.body.classList.contains("dark");
    document.addEventListener("DOMContentLoaded", e => {
        $(".fa-plug").hide();
        $(".fa-sync").hide();
        $(".fa-times").hide();

        map = L.map('map').setView([52.35, -1.5], 12);
        window.TheMap = map;
        var style = darkMode ? "dark" : "light";
        L.tileLayer('https://api.mapbox.com/styles/v1/mapbox/' + style + '-v9/tiles/256/{z}/{x}/{y}@2x?access_token={accessToken}', {
            attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a>, Imagery © <a href="https://www.mapbox.com/">Mapbox</a>',
            maxZoom: 18,
            id: 'mapbox.streets',
            accessToken: 'pk.eyJ1IjoiZXNwZXJuZXQiLCJhIjoiY2psdHUyanIxMDh1ajNxcXZjcW52czkxdCJ9.-7mwU0MKux72kJoGcrVOfg'
        }).addTo(map);
        const lc = L.control.locate({
            icon: "glyphicon glyphicon-map-marker",
            iconLoading: "glyphicon glyphicon-option-horizontal",
        }).addTo(map);
        grabData();
        connectWebSocket();
        
        $(".controls").hide();
        
        $(".settings").click(function(e) {
           $("#map").toggle();
           $(".controls").toggle();
        });
    });
    
    /** @type Marker[] **/
    let oldMarkers = [];
    /** @type {Object.<string, Array.<Marker>>} **/
    let oldMarkersByRoute = {};
    
    const blueIcon = L.icon({
        iconUrl: '/images/bus_blue.svg',
        iconSize: [32, 32], 
        iconAnchor: [16, 16],
        popupAnchor: [0, 0]
    });
    const greenIcon = L.icon({
        iconUrl: '/images/bus_green.svg',
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, 0]
    });

    const purpleIcon = L.icon({
        iconUrl: '/images/bus_purple.svg',
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, 0]
    });

    const blackIcon = L.icon({
        iconUrl: '/images/bus_black.svg',
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, 0]
    });

    const lightIcon = L.icon({
        iconUrl: '/images/bus_light.svg',
        iconSize: [32, 32],
        iconAnchor: [16, 16],
        popupAnchor: [0, 0]
    });

    const stopIcon = L.icon({
        iconUrl: darkMode ? '/images/stop_light.svg' : '/images/stop.svg',
        iconSize: [8, 8],
        iconAnchor: [4, 4],
        popupAnchor: [0, 0]
    });

    const plottedRoutes = {};

    //setInterval(grabData, 10000);
    function connectWebSocket() {
        const endpoint = "wss://" + window.location.host + "/Home/Websocket";
        const ws = new WebSocket(endpoint);
        ws.onmessage = (d) => {
            if ("data" in d) {
                console.log(d.data);
                handleRetrievedData(JSON.parse(d.data));
            }
        };
        ws.onopen = () => {
            $(".fa-plug").show();
        };
        const error = () => {
            $(".fa-times").show();
            connectWebSocket();
        };
        ws.onerror = error;
        ws.onclose = error;
    }
    
    //grabData();
    connectWebSocket();
    plotRoutes();

    function updateRouteLayerFromCheckbox() {
        if (!$(this).prop("checked")) {
            plottedRoutes[$(this).attr("name")].remove();
        } else {
            plottedRoutes[$(this).attr("name")].addTo(map);
        }
    }
    $("input").on("click", updateRouteLayerFromCheckbox);

    const existingStops = {};

    function makeStopMarker(stop, text) {
        if (stop.atcoCode in existingStops) {
            const y = document.createElement("span");

            y.textContent = text;
            existingStops[stop.atcoCode].innerHTML = existingStops[stop.atcoCode].innerHTML + "<br />" + y.innerHTML;
            return;
        }
        const marker = L.marker([stop.latitude, stop.longitude], {"icon": stopIcon}).addTo(map);
        const popup = document.createElement("h5");

        popup.textContent = stop.name;
        const container = document.createElement("span");
        
        container.appendChild(popup);

        container.innerHTML = "<h4>Bus Stop</h4>" + container.innerHTML + "Served by:<br />" + text;
        marker.bindPopup(container);
        existingStops[stop.atcoCode] = container;
    }

    function featClick(feature, layer) {
        let name = feature.properties.junction ? feature.properties.junction : feature.properties.name;
        let maxspeed = feature.properties.maxspeed !== undefined ? feature.properties.maxspeed : "unknown speed";
        let popupText = "<b>" + name + "</b> - " + maxspeed;
        
        if (name === undefined) {
            return;
        }

        layer.bindPopup(popupText, {
            closeButton: true,
            offset: L.point(0, 0)
        });
        layer.on('click', function() {
            layer.openPopup();
            console.log(feature.properties);
        });
    }

    function plotRoutes() {
        let elevenInbound = fetch("/js/11-inbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                knownStops[stop.atcoCode] = stop;
                makeStopMarker(stop, "11, towards Coventry");
            }
        });

        let elevenInboundBetter = fetch("/js/11-inbound-better.json").then(r => r.json()).then(data => {
            var geo11I = L.geoJSON(data, {
                onEachFeature: featClick,
                filter: function(feature, layer) {
                    return feature.name !== "Stop";
                },
                style: {"color": "#4b77be"}
            });
            plottedRoutes["11-inbound.json"] = geo11I;
        });

        let elevenOutboundBetter = fetch("/js/11-outbound-better.json").then(r => r.json()).then(data => {
            var geo11O = L.geoJSON(data, {
                onEachFeature: featClick,
                filter: function(feature, layer) {
                    return feature.name !== "Stop";
                },
                style: {"color": "#4b77be"}
            });
            plottedRoutes["11-outbound.json"] = geo11O;
        });

        let elevenOutbound = fetch("/js/11-outbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                knownStops[stop.atcoCode] = stop;
                makeStopMarker(stop, "11, towards Leamington");
            }
            
        });

        let twelveXOutbound = fetch("/js/12X-outbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                knownStops[stop.atcoCode] = stop;
                makeStopMarker(stop, "12X, towards Warwick Uni");
            }
        });
        
        let twelveXOutboundRoute = fetch("/js/12X-outbound-better.json").then(r => r.json()).then(data => {
            var geo12xO = L.geoJSON(data, {
                onEachFeature: featClick,
                filter: function(feature, layer) {
                    return feature.name !== "Stop";
                },
                style: {"color": "green"}
            });
            plottedRoutes["12X-outbound.json"] = geo12xO;
        });

        let twelveXInbound = fetch("/js/12X-inbound.json").then(r => r.json()).then(data => {
            for (var stop of data.stops) {
                knownStops[stop.atcoCode] = stop;
                makeStopMarker(stop, "12X, towards Coventry");
            }
        });
        
        let twelveXInboundRoute = fetch("/js/12X-inbound-better.json").then(r => r.json()).then(data => {
            var geo12xI = L.geoJSON(data, {
                onEachFeature: featClick,
                filter: function(feature, layer) {
                    return feature.name !== "Stop";
                },
                style: {"color": "green"}
            });
            plottedRoutes["12X-inbound.json"] = geo12xI;
        });
        
        Promise.all([elevenInbound, elevenOutbound, elevenOutboundBetter, elevenInboundBetter, twelveXOutboundRoute, twelveXInbound, twelveXOutbound, twelveXInboundRoute]).then(() => {
            $("input").each(updateRouteLayerFromCheckbox);
        });

    }

    function getNewPopup(busLoc, key) {
        var popup = document.createElement("span");
        let ts = new Date(busLoc.collectionTimestamp);

        let speed = Math.round(busLoc.speedGuess);
        if (speed < 0) {
            speed = "???";
        }
        var nextStop = "";
        if (busLoc.atStop in knownStops) {
            nextStop = "<br/>Current stop: " + knownStops[busLoc.atStop].name;
        }
        if (busLoc.nextStop in knownStops) {
            nextStop = "<br />Next stop: " + knownStops[busLoc.nextStop].name;
        }
        popup.innerHTML = mapName(key) + "<br /><small>Collected at " + ("0" + ts.getUTCHours()).slice(-2) + ":" +
            ("0" + ts.getUTCMinutes()).slice(-2) + ":" +
            ("0" + ts.getUTCSeconds()).slice(-2) + "</small><br />" + speed + " mph<br />" + nextStop;
        return popup;
    }

    function updateAll(data) {
        for (var key of Object.keys(data)) {
            if (!oldMarkersByRoute.hasOwnProperty(key)) { oldMarkersByRoute[key] = []; }
            for (var busLoc of data[key]) {
                let icon = key.includes("11") ? blueIcon : greenIcon;
                if (key.includes("11U")) {
                    icon = purpleIcon;
                }

                if (key.includes("18")) {
                    icon = darkMode ? lightIcon : blackIcon;
                }
                var marker = L.marker([busLoc.latitude, busLoc.longitude], {"icon": icon}).addTo(map);
                var popup = getNewPopup(busLoc, key);
                var customOptions =
                    {
                        'className' : 'bus-popup'
                    };
                marker.bindPopup(popup, customOptions);
                oldMarkers.push(marker);
                oldMarkersByRoute[key].push(marker);
            }
        }
    }

    function updateInPlace(oldMarkers, data) {
        // n^2
        for (var route of Object.keys(data)) {
            for (let m1 of oldMarkersByRoute[route]) {
                var bestEuclideanDistance = -1;
                var bestNewData = null;
                var bestNewPopup = null;
                for (let m2 of data[route]) {
                    var dist = L.CRS.Earth.distance(m1.getLatLng(), [m2.latitude, m2.longitude]);
                    if (bestEuclideanDistance === -1 || dist < bestEuclideanDistance) {
                        bestEuclideanDistance = dist;
                        bestNewData = m2;
                        bestNewPopup = getNewPopup(m2, route);
                    }
                }
                m1.setLatLng([bestNewData.latitude, bestNewData.longitude]);
                console.log(bestNewPopup.innerHTML);
                m1.setPopupContent(bestNewPopup.innerHTML);
                data[route] = data[route].filter((v,i,a) => {
                    return v !== bestNewData;
                })
            }
        }
    }

    function handleRetrievedData(data) {
        console.log("Fetched!");
        var dataCount = Object.keys(data).map(k => data[k].length).reduce((a, b) => a + b);
        if (dataCount === oldMarkers.length) {
            console.log("In place update");
            updateInPlace(oldMarkers, data);
        } else {
            for (var m of oldMarkers) {
                map.removeLayer(m);
            }
            oldMarkers = [];
            oldMarkersByRoute = {};
            console.log("Full update,", dataCount, oldMarkers.length);
            updateAll(data);
        }
    }

    function grabData() {
        $(".fa-sync").fadeIn();
        fetch("/Home/Data").then(r => r.json()).then(data => {
            handleRetrievedData(data);
            $(".fa-sync").fadeOut();
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


function httpGetAsync(theUrl, callback) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    }
    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.send(null);
}

var drones = httpGetAsync("http://localhost:8680/api/drone", function (dronesAsJsonString) {
    console.log(dronesAsJsonString);

    var drones = JSON.parse(dronesAsJsonString);
    for(var i = 0; i < drones.length; i++)
    {
        console.log("Id: " + drones[i].Id);
        console.log("Drones: " + drones[i].State);
    }

});

setTimeout(window.location.reload(), 1000);
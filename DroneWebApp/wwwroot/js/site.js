var drones = [];

(function worker() {
    $.get("http://localhost:8680/api/drone", function (data) {
        for (var i = 0; i < data.length; i++)
        {
            var drone = data[i];
            var index = drones.indexOf(drone.Id);
            if(index === -1) 
            {
                console.log(drone.Id + ' is a new id');
                // Drone doesn't exist
                $('.DroneList').append('<li><div class=drone' + drone.Id + '>Id: ' + drone.Id + ', Altitude: ' + drone.State.Altitude + ', Longitude: ' + drone.State.Longitude + ', Latitude: ' + drone.State.Latitude + '</div></li>');
                // Store index
                drones.push(drone.Id);
            }
            else
            {
                console.log(index + ' is an existing id');
                // Drone exists
                $('.drone' + drone.Id).html('Id: ' + drone.Id + ', Altitude: ' + drone.State.Altitude + ', Longitude: ' + drone.State.Longitude + ', Latitude: ' + drone.State.Latitude);
            }
        }

        setTimeout(worker, 1000);
    });
})();

var drones = [];

(function worker() {
    $.get("http://localhost:8680/api/drone?t=" + Math.floor((Math.random() * 1000) + 1), function (data) {

        // Assume data is a JSON array
        for (var i = 0; i < data.length; i++)
        {
            var drone = data[i];
            var index = drones.indexOf(drone.Id);

            if (index === -1) // Drone doesn't exist
            {
                console.log('New drone id [' + drone.Id + ']');
                // Create element
                $('.DroneList').append('<li><div class=drone' + drone.Id + '>Id: ' + drone.Id + ', Altitude: ' + drone.State.Altitude + ', Longitude: ' + drone.State.Longitude + ', Latitude: ' + drone.State.Latitude + '</div></li>');
                // Store index
                drones.push(drone.Id);
            }
            else // Drone exists
            {
                console.log('Drone id [' + drone.Id + '] already exists');
                console.log(drone.State);
                var existingState = drones[index].State;
                
                if (drone.State == null)
                {
                    console.log("ERROR: State is null!"); // Pass through
                }
                else if (drone.State === existingState)
                {
                    console.log("State not changed"); // Pass through
                }
                else
                {
                    console.log("Updating state"); // Update state
                    $('.drone' + drone.Id).html('Id: ' + drone.Id + '</br>' +
                                                ', Alt: ' + drone.State.Altitude + '</br>' +
                                                ', Lon: ' + drone.State.Longitude + '</br>' +
                                                ', Lat: ' + drone.State.Latitude + '</br>' +
                                                ', Hea: ' + drone.State.Heading + '</br>' +
                                                ', Vel: ' + drone.State.Speed);
                }
            }
        }

        // Recursively invoke worker function with delay
        setTimeout(worker, 2000);
    });
})();

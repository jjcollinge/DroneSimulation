var drones = [];

(function worker() {
    $.get("http://localhost:8109/api/drones?t=" + Math.floor((Math.random() * 1000) + 1), function (data) {

        // Assume data is a JSON array
        for (var i = 0; i < data.length; i++) {
            console.log(data[i]);

        }

        // Recursively invoke worker function with delay
        setTimeout(worker, 2000);
    });
})();
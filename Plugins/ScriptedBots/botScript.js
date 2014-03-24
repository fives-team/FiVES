function vectorToString(vector) {
    return vector.x + " " + vector.y + " " + vector.z;
}

function isInsideBlock(pos) {
    return (pos.x > 0 ? pos.x % 75 > 15 : pos.x % 75 > -60) &&
        (pos.z > 0 ? pos.z % 75 > 15 : pos.z % 75 > -60);
}

function computeNextPosition() {
    var pos = entity["location"]["position"];
    var vel = entity["motion"]["velocity"];
    return new Vector(eventLoop.intervalMs * vel.x + pos.x,
                      eventLoop.intervalMs * vel.y + pos.y,
                      eventLoop.intervalMs * vel.z + pos.z);
}

function changeDirection() {
    var angle = Math.random() * 2 * Math.PI;
    var speed = 3 * eventLoop.intervalMs / 1000;
    entity["motion"]["velocity"] = new Vector(Math.sin(angle) * speed, 0, Math.cos(angle) * speed);
    //logger.debug('bot velocity: ' + vectorToString(entity['motion']['velocity']));
}

eventLoop.addTickFiredHandler(function (timespan) {
    // keep changing direction until the next position is not going to be inside of a block
    while (isInsideBlock(computeNextPosition()))
        changeDirection();

    // also change direction randomly on average every 10 seconds
    var numCallsPerSecond = 1000 / eventLoop.intervalMs;
    var prob = 1 / (numCallsPerSecond * 10);
    if (Math.random() < prob) {
        changeDirection();
    }

    //logger.debug('bot position: ' + vectorToString(entity['location']['position']));
});

// start movement
changeDirection();

//logger.debug('bot position: ' + vectorToString(entity['location']['position']));
//logger.debug('bot velocity: ' + vectorToString(entity['motion']['velocity']));
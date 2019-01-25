var sphero = require("sphero"),
    bb8 = sphero("fa8ea18a0b32469a85360f1ea47ae119"); // change BLE address accordingly

var currentSpeed, maxSpeed;

bb8.connect(function() {
  bb8.ping(function(err, data) {
    console.log(err || "data: " + data);
  });

  bb8.sleep(0, 0, 0, function(err, data) {
    console.log(err || "data: " + data);
  });

  // console.log("HelloWorld");
  //bb8.streamGyroscope();
  //
  maxSpeed = 120;
  currentSpeed = 0;
  var acceleration = 5;//The higher the less acceleration (!) represents duration it take to reach maxSpeed

  setInterval(function(){
    if(currentSpeed == 0){
      currentSpeed = maxSpeed/8;
    } else if(currentSpeed < maxSpeed){
      currentSpeed += maxSpeed/8;
    }
    moveForward(currentSpeed);
  }, 1000);
});

bb8.on("velocity", function(data) {
  console.log("velocity:");
  console.log("  sensor:", data.xVelocity.sensor);
  console.log("    range:", data.xVelocity.range);
  console.log("    units:", data.xVelocity.units);
  console.log("    value:", data.xVelocity.value[0]);

  console.log("  sensor:", data.yVelocity.sensor);
  console.log("    range:", data.yVelocity.range);
  console.log("    units:", data.yVelocity.units);
  console.log("    value:", data.yVelocity.value[0]);
});
bb8.on("gyroscope", function(data){
  console.log(data);
});

function startMovingForward(seconds){
  currentSpeed = maxSpeed/seconds;

  for(var v=0; v<seconds; v++){
    setTimeout(function(){
      console.log("Going forward, currentSpeed: "+currentSpeed);
      moveForward(currentSpeed += maxSpeed/seconds)
    }, v*1000);
  }
}
function startMovingBackward(seconds){
  currentSpeed = maxSpeed/seconds;

  for(var v=0; v<seconds; v++){
    setTimeout(function(){
      console.log("Going backward, currentSpeed: "+currentSpeed);
      moveBackward(currentSpeed += maxSpeed/seconds)
    }, v*1000);
  }
}
function moveForward(speed) {
  var opts = {
	   lmode: 0x01,
	   lpower: speed,
	   rmode: 0x01,
     rpower: speed
   }
   bb8.setRawMotors(opts);
}

function moveBackward(speed) {
  var opts = {
	   lmode: 0x02,
	   lpower: speed,
	   rmode: 0x02,
     rpower: speed
   }
   bb8.setRawMotors(opts);
}

function standardCallback(err) {
  console.log(err);
}

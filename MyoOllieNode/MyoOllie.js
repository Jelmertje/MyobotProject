var Myo = require('myo');
var WebSocket = require('ws');
var myMyo;

var sphero = require("sphero"),
    bb8 = sphero("fa8ea18a0b32469a85360f1ea47ae119"); // change BLE address accordingly

var currentSpeed, maxSpeed, bb8Connected = false, velocity, setToForward = true;

console.log("trying to connect to Ollie ...");
bb8.connect(function() {
  console.log("Connected");

  currentSpeed = 0;
  maxSpeed = 50;

  bb8.streamVelocity();

  bb8Connected = true;
  console.log('Press any key...');
});

Myo.connect('my.app.id', WebSocket);

Myo.onError = function () {
  console.log("Failed to connect");
}

Myo.on('connected', function(){
  console.log("Connected");
    myMyo = this;
    addEvents(myMyo);
});

var addEvents = function(myo){
    myo.on('fist', function(){
        console.log('fist for ', this.macAddress);
        if(bb8Connected) {
            startMovingForward();
        }
    });
    myo.on('fingers_spread', function(){
      console.log('fingers spread for ', this.macAddress);
      if(bb8Connected) {
          startMovingBackward();
      }
    });
}

function startMovingForward(){
  if(!setToForward) {
    bb8.stop(reallyStartMovingForward);
  } else {
    moveForward(maxSpeed);
  }
}
function reallyStartMovingForward() {
  setToForward = true;
  startMovingForward();
}

function startMovingBackward(){
  if(setToForward) {
    bb8.stop(reallyStartMovingBackward);
  } else {
    moveBackward(maxSpeed);
  }
}
function reallyStartMovingBackward(){
  setToForward = false;
  startMovingBackward();
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

bb8.on("velocity", function(data) {
  velocity = data.xVelocity.value[0] + data.yVelocity.value[0];
  console.log("velocity:" + velocity);
});

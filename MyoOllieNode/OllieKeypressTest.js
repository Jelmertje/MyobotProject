const readline = require('readline');
var sphero = require("sphero"),
    bb8 = sphero("fa8ea18a0b32469a85360f1ea47ae119"); // change BLE address accordingly

var currentSpeed, maxSpeed, bb8Connected = false, velocity, setToForward = true;

console.log("trying to connect...");
bb8.connect(function() {
  console.log("Connected");

  currentSpeed = 0;
  maxSpeed = 80;

  bb8.streamVelocity();

  bb8Connected = true;
  console.log('Press any key...');
});

readline.emitKeypressEvents(process.stdin);
process.stdin.setRawMode(true);
process.stdin.on('keypress', (str, key) => {
  if (key.ctrl && key.name === 'c') {
    process.exit();
  } else {
    console.log(`You pressed the "${str}" key`);
    console.log();
    console.log(key);
    console.log();

    if(bb8Connected) {
      if(key.name === 'w' || key.name === 'up'){
        startMovingForward();
      } else if (key.name === 's' || key.name === 'down'){
        startMovingBackward();
      } else {
        bb8.stop(function() {
          console.log("Ollie stopped...")
        });
      }
    }
  }
});

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

var Myo = require('myo');
var WebSocket = require('ws');
var myMyo;

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
    });
    myo.on('fingers_spread', function(){
      console.log('fingers spread for ', this.macAddress);
    });
}

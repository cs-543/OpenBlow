/// <reference path="node.d.ts"/>
var net = require('net');

if (process.argv.length < 4) {
    console.log('Usage: node controller.js CONTROLLER_COUNT CONTROLLER_PORT_BASE');
    return;
}

var CONTROLLER_COUNT = parseInt(process.argv[2]);
var CONTROLLER_PORT_BASE = parseInt(process.argv[3]);

/**
*
*/
var Packet = (function () {
    function Packet(from, to, data) {
        this.from = null;
        this.to = null;
        this.data = null;
        this.from = from;
        this.to = to;
        this.data = data;
    }
    return Packet;
})();

/**
*
*/
var Rule = (function () {
    function Rule(flowId, nextHop, ttl) {
        this.flow_id = -1;
        this.next_hop = null;
        this.ttl = -1;
        this.flow_id = flowId;
        this.next_hop = nextHop;
        this.ttl = ttl;
    }
    return Rule;
})();

/**
*
*/
var RuleNotFoundPacket = (function () {
    function RuleNotFoundPacket(flowId) {
        this.failed_flow_id = -1;
        this.failed_flow_id = flowId;
    }
    return RuleNotFoundPacket;
})();

/**
*
*/
var RuleRequest = (function () {
    function RuleRequest() {
    }
    return RuleRequest;
})();

var CONNECT = 0;
var DISCONNECT = 0;

/**
*
*/
var Controller = (function () {
    function Controller(id) {
        var _this = this;
        this.name = "<anonymous>";
        this.port = -1;
        this.rules = [];
        this.server = null;
        this.name = "Controller " + id;
        this.port = id + CONTROLLER_PORT_BASE;

        this.server = net.createServer(function (socket) {
            CONNECT++;
            var clientId = socket.remoteAddress + ':' + socket.remotePort;

            _this.log('Client ' + clientId + ' connected');

            socket.on('end', function () {
                _this.log('Client ' + clientId + ' disconnected');
                DISCONNECT++;

                _this.log("(" + CONNECT + " / " + DISCONNECT + ")");
            });

            // We're gonna read text.
            socket.setEncoding('utf8');
            0;

            // Data event
            socket.on('data', function (data) {
                try  {
                    _this.handleRequest(socket, JSON.parse(data));
                } catch (e) {
                    _this.log(e);
                    socket.end();
                }
            });
        });
    }
    Controller.prototype.addRule = function (rule) {
        this.rules[rule.flow_id] = rule;
    };

    Controller.prototype.listen = function () {
        var _this = this;
        this.server.listen(this.port, function () {
            _this.log('Listening on port ' + _this.port);
        });
    };

    Controller.prototype.handleRequest = function (socket, inPacket) {
        var request = inPacket.data;
        var payload = this.rules[request.flow_id] || null;

        if (payload === null) {
            payload = new RuleNotFoundPacket(request.flow_id);
        }

        this.log('Providing rule for flow ID ' + request.flow_id);

        socket.end(JSON.stringify(new Packet(socket.address().address + ':' + socket.address().port, socket.remoteAddress + ':' + socket.remotePort, payload)) + '\n');
    };

    Controller.prototype.log = function (message) {
        console.log('[' + this.name + '] ' + message);
    };
    return Controller;
})();

var controllers = new Array(CONTROLLER_COUNT);

for (var i = 0; i < controllers.length; ++i) {
    controllers[i] = new Controller(i);
}

// Install rules.
[
    [1, 'null:0', 10],
    [2, 'null:0', 10],
    [3, 'null:0', 10],
    [4, 'null:0', 10],
    [5, 'null:0', 10],
    [6, 'null:0', 10],
    [7, 'null:0', 10],
    [8, 'null:0', 10],
    [9, 'null:0', 10]
].forEach(function (v, k) {
    var c = controllers[k % controllers.length];
    c.addRule(new Rule(v[0], v[1], v[2]));
});

for (i = 0; i < controllers.length; ++i) {
    controllers[i].listen();
}

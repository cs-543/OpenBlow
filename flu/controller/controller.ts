/// <reference path="node.d.ts"/>

import net = require('net');

if (process.argv.length < 4) {
    console.log('Usage: node controller.js CONTROLLER_COUNT CONTROLLER_PORT_BASE');
}

var CONTROLLER_COUNT = parseInt(process.argv[2]);
var CONTROLLER_PORT_BASE = parseInt(process.argv[3]);

/**
 *
 */
class Packet {
    from: string = null;
    to: string = null;
    data: any = null;

    constructor(from: string, to: string, data: any) {
        this.from = from;
        this.to = to;
        this.data = data;
    }
}

/**
 *
 */
class Rule {
    flow_id: number = -1;
    next_hop: string = null;
    ttl: number = -1;

    constructor(flowId: number, nextHop: string, ttl: number) {
        this.flow_id = flowId;
        this.next_hop = nextHop;
        this.ttl = ttl;
    }
}

/**
 *
 */
class RuleNotFoundPacket {
    failed_flow_id: number = -1;

    constructor(flowId: number) {
        this.failed_flow_id = flowId;
    }
}

/**
 *
 */
class RuleRequest {
    flow_id: number;
}

var CONNECT = 0;
var DISCONNECT = 0;

/**
 *
 */
class Controller {
    private name: string = "<anonymous>";
    private port: number = -1;

    private rules: Rule[] = [];
    private server: net.Server = null;

    constructor(id: number) {
        this.name = "Controller " + id;
        this.port = id + CONTROLLER_PORT_BASE;

        this.server = net.createServer(socket => {
            CONNECT++;
            var clientId = socket.remoteAddress + ':' + socket.remotePort;

            this.log('Client ' + clientId + ' connected');

            socket.on('end', () => {
                this.log('Client ' + clientId + ' disconnected');
                DISCONNECT++;

                this.log("(" + CONNECT + " / " + DISCONNECT + ")");
            });

            // We're gonna read text.
            socket.setEncoding('utf8');0            
            // Data event
            socket.on('data', (data: string) => {
                try {
                    this.handleRequest(socket, JSON.parse(data));
                } catch (e) {
                    this.log(e);
                    socket.end(); 
                }
            });
        });
    }

    public addRule(rule: Rule) {
        this.rules[rule.flow_id] = rule;
    }

    public listen(): void {
        this.server.listen(this.port, () => {
            this.log('Listening on port ' + this.port);
        });
    }

    private handleRequest(socket: net.NodeSocket, inPacket: Packet): void {
        var request: RuleRequest = inPacket.data;
        var payload: any = this.rules[request.flow_id] || null;

        if (payload === null) {
            payload = new RuleNotFoundPacket(request.flow_id);
        }

        this.log('Providing rule for flow ID ' + request.flow_id);

        socket.end(JSON.stringify(new Packet(
            socket.address().address + ':' +  socket.address().port,
            socket.remoteAddress + ':' + socket.remotePort,
            payload
        )) + '\n');
    }

    private log(message: string): void {
        console.log('[' + this.name + '] ' + message);
    }
}

var controllers: Controller[] = new Array(CONTROLLER_COUNT);

// Create the controllers.
for (var i = 0; i < controllers.length; ++i) {
    controllers[i] = new Controller(i);
}

// Install rules.
[
    // Flow id, netx hop, ttl
    [ 1, 'null:0', 10 ],
    [ 2, 'null:0', 10 ],
    [ 3, 'null:0', 10 ],
    [ 4, 'null:0', 10 ],
    [ 5, 'null:0', 10 ],
    [ 6, 'null:0', 10 ],
    [ 7, 'null:0', 10 ],
    [ 8, 'null:0', 10 ],
    [ 9, 'null:0', 10 ],
].forEach((v: any[], k: number) => {
    var c = controllers[k % controllers.length];
    c.addRule(new Rule(v[0], v[1], v[2]));
});

// Start up.
for (i = 0; i < controllers.length; ++i) {
    controllers[i].listen();
}
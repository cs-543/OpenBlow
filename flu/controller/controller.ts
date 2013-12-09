/// <reference path="node.d.ts"/>

import net = require('net');

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
class RuleRequest {
    flow_id: number;
}

/**
 *
 */
class Controller {
    private static PORT_BASE = 8800;

    private name: string = "<anonymous>";
    private port: number = -1;

    private rules: Rule[] = [];
    private server: net.Server = null;

    constructor(id: number) {
        this.name = "Controller " + id;
        this.port = id + Controller.PORT_BASE;

        this.server = net.createServer(socket => {
            var clientId = socket.remoteAddress + ':' + socket.remotePort;

            this.log('Client ' + clientId + ' connected');

            socket.on('end', () => {
                this.log('Client ' + clientId + ' disconnected');
            });

            // We're gonna read text.
            socket.setEncoding('utf8');
            
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
        var rule = this.rules[request.flow_id] || null;

        if (rule === null) {
            throw 'No rule for flow ID ' + request.flow_id;
        }

        this.log('Providing rule for flow ID ' + request.flow_id);
        socket.end(JSON.stringify(new Packet(
            socket.address().address + ':' +  socket.address().port,
            socket.remoteAddress + ':' + socket.remotePort,
            rule
        )) + '\n');
    }

    private log(message: string): void {
        console.log('[' + this.name + '] ' + message);
    }
}

var controllers: Controller[] = new Array(4);

// Create the controllers.
for (var i = 0; i < controllers.length; ++i) {
    controllers[i] = new Controller(i);
}

// Install rules.
[
    [ 1, 'localhost', 11 ],
    [ 2, 'localhost', 22 ],
    [ 3, 'localhost', 33 ],
    [ 4, 'localhost', 44 ],
    [ 5, 'localhost', 55 ],
    [ 6, 'localhost', 66 ],
    [ 7, 'localhost', 77 ],
    [ 8, 'localhost', 88 ],
    [ 9, 'localhost', 99 ],
].forEach((v: any[], k: number) => {
    var c = controllers[k % controllers.length];
    c.addRule(new Rule(v[0], v[1], v[2]));
});

// Start up.
for (i = 0; i < controllers.length; ++i) {
    controllers[i].listen();
}
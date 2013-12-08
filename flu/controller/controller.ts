/// <reference path="node.d.ts"/>

import net = require('net');

/**
 * Just a function for printing messages with a time.
 *
 * @param message The string to display
 */
function log(message: string): void {
    var date = new Date();
    var dateStr = date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds();

    console.log('[' + dateStr + '] ' + message);
}

/**
 *
 */
class Rule {
    flow_id: number;
    next_hop: string;
    ttl: number;

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
    private static PORT = 8888;

    private rules: Rule[] = [];
    private server: net.Server = null;

    constructor(rules: Rule[] = []) {
        this.rules = rules;

        this.server = net.createServer(socket => {
            var clientId = socket.remoteAddress + ':' + socket.remotePort;

            log('Client ' + clientId + ' connected');

            socket.on('end', () => {
                log('Client ' + clientId + ' disconnected');
            });

            // We're gonna read text.
            socket.setEncoding('utf8');
            
            // Data event
            socket.on('data', (data: string) => {
                try {
                    this.handleRequest(socket, data);
                } catch (e) {
                    console.error(e);        
                }
            });
        });
    }

    public addRule(rule: Rule) {
        this.rules[rule.flow_id] = rule;
    }

    public listen(): void {
        this.server.listen(Controller.PORT, () => {
            log('Listening on port ' + Controller.PORT);
        });
    }

    private handleRequest(socket: net.NodeSocket, data: string): void {
        var request: RuleRequest = JSON.parse(data);
        var rule = this.rules[request.flow_id] || null;

        if (rule != null) {
            log('Providing rule for flow ID ' + request.flow_id);
            socket.end(JSON.stringify(rule));
        } else {
            throw 'No rule found for flow ID ' + request.flow_id;
        }
    }
}

// Create the controller
var controller = new Controller();

[
    [ 1, '192.168.0.1', 11 ],
    [ 2, '192.168.0.2', 22 ],
    [ 3, '192.168.0.3', 33 ],
    [ 4, '192.168.0.4', 44 ],
    [ 5, '192.168.0.5', 55 ],
    [ 6, '192.168.0.6', 66 ],
    [ 7, '192.168.0.7', 77 ],
    [ 8, '192.168.0.8', 88 ],
    [ 9, '192.168.0.9', 99 ],
].forEach((rule: any[]) => {
    controller.addRule(new Rule(rule[0], rule[1], rule[2]));
});

controller.listen();

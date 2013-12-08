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

    constructor(flowId: number, nextHop: string) {
        this.flow_id = flowId;
        this.next_hop = nextHop;
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
    private server: net.Server = null;

    constructor() {
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
                    this.handleRequest(data);
                } catch (e) {
                    console.error(e);        
                }
            });
        });
    }

    public listen(): void {
        this.server.listen(Controller.PORT, () => {
            log('Listening on port ' + Controller.PORT);
        });
    }

    private handleRequest(data: string): void {
        var request: RuleRequest = JSON.parse(data);

    }
}

var controller = new Controller();
controller.listen();

// Function to create a WebSocket logger
function createWebSocketLogger(ws) {
    const logEvent = (eventName, event) => {
        console.log(`WebSocket Event: ${eventName}`, event);
    };

    ws.addEventListener('open', (event) => {
        logEvent('open', event);
    });

    ws.addEventListener('message', (event) => {
        logEvent('message', event);
    });

    ws.addEventListener('error', (event) => {
        logEvent('error', event);
    });

    ws.addEventListener('close', (event) => {
        logEvent('close', event);
    });

    // Optionally, log the data sent through the WebSocket
    const originalSend = ws.send;
    ws.send = function (data) {
        console.log(`WebSocket Sending Data: ${data}`);
        originalSend.call(this, data);
    };

    return ws;
}

// Function to initialize WebSocket logger
window.initializeWebSocketLogger = (dotnetHelper) => {
    // Save the original WebSocket constructor
    const OriginalWebSocket = window.WebSocket;

    // Create a wrapper for the WebSocket constructor
    function WebSocketLogger(url, protocols) {
        // Call the original WebSocket constructor
        const ws = new OriginalWebSocket(url, protocols);

        // Return the enhanced WebSocket instance
        return createWebSocketLogger(ws);
    }

    // Replace the global WebSocket with the wrapper
    window.WebSocket = WebSocketLogger;
};

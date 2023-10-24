// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var dict = {};

var connection = new signalR.HubConnectionBuilder().withUrl("/hubs/updates").build();

// For Server calling
connection.on("logDebug", (txt) => { console.log(`logDebug: ${txt}`); });
connection.on("initilize", (txt) => {
    console.log("initilize()");
    const json = JSON.parse(txt);
    console.log(json);
    const items = json.connections;
    items.forEach((itew) => { insertButton(itew); });

});
connection.on("write-text", (txt) => {
    const el = document.getElementById('console-container')
    el.innerText += txt;
    console.log(`logDebug: ${txt}`);
});
connection.on("write-line", (txt) => {
    const el = document.querySelector('#console-container .console-channel');
    el.innerHTML += '<br>' + txt;
    //window.scrollTo({ left: 0, top: document.body.scrollHeight, behavior: "smooth" });
    //el.scrollTop = el.scrollHeight;
    //const br = document.createElement("br");
    //br.innerHTML =
    //el.appendChild (br);
    console.log(`logDebug: ${txt}`);
});
connection.on("create-tab", (txt) => {
    const json = JSON.parse(txt);
    const tab = { tab_id: json.id };
    dict[json.id] = tab;
    connection.invoke("ReportTabCreated", tab);
});

connection.on("connect-console", (txt) => {
    insertButton(txt);
});
connection.on("disconnect-console", (txt) => {

});

connection.on("fill-console", (json) => {
    const id = json.id;
    const el = document.getElementById(id);
    const logs = json.logs;
    logs.forEach((item) => {
        el.innerHTML += "<br>" + item;
    });
});


connection.onclose((er) => {
    console.log(`Connection closed. Er:${er}`);
    start();    // reconnect
});
start();
function start() {
    var promise = connection.start();
    promise.then((x) => {
        connection.invoke(`NotifyStart`, `this is string from client sended after begin`)
            .then((x) => {
                console.log("Calling RequestInitilization");
                connection.invoke(`RequestInitilization`, `this is string from client sended after begin`);
            }
            );
    });
}

function insertButton(item) {
    const id = item.id;
    console.log("insert buttob " + id);
    const container = document.querySelector('#console-list');
    const template = container.children[0];
    const clone = template.cloneNode(true);
    clone.removeAttribute("hidden");
    container.appendChild(clone);
    clone.innerText = id;

    clone.onclick = () => { SwitchButton(item); };
}

function SwitchButton(item) {
    const existed = document.getElementById(item.id);
    if (existed != null) {
        existed.remove();
    }
    else {
        const el = document.querySelector("#console-default");
        const container = el.parentElement;
        const clone = el.cloneNode();
        clone.id = item.id;
        container.appendChild(clone);
        requestLogs(item);
    }
}

function requestLogs(item) {
    connection.invoke("GetLogs", item);
}

function createCompareWindow(consoleLeft, consoleRight, container) {
    const c = new CompareWindow(consoleLeft, consoleRight, container);
}
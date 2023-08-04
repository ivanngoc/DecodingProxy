console.log(`site.js runned`);
var connection = new signalR.HubConnectionBuilder().withUrl("/hubs/info").build();
// когда сервер посылает команду на выполнение этого метода
connection.on("syncTextBox", (txt) => {
    throw new Error('not implemented');
});

connection.on("logDebug", (txt) => {
    console.log(`logDebug: ${txt}`);
});


connection.on("ctdStalled", (content) => {
    throw new Error('not implemented');
});

connection.on("ctdRevived", (content) => {
    throw new Error('not implemented');
});

connection.on("ctdCreated", (content) => {
    throw new Error('not implemented');
});
connection.on("ctdDisposed", (content) => {
    throw new Error('not implemented');
});

connection.on("connectionAdd", (content) => {
    const data = JSON.parse(content);
    console.log(data);
    const table = document.getElementById('table-connections');
    const template = document.getElementById('template-raw');
    const clone = template.cloneNode(true);
    clone.classList.remove('template');
    table.insertAdjacentElement('beforeend', clone);
    clone.id = data.Id;

    clone.querySelector('td.cell-num').innerHTML = Array.from(clone.parentNode.children).indexOf(clone);
    clone.querySelector('td.cell-id').innerHTML = data.Id;
    clone.querySelector('td.cell-host').innerHTML = data.Host;
    clone.querySelector('td.cell-port').innerHTML = data.Port;
    clone.querySelector('td.cell-status').innerHTML = data.Status;
    clone.querySelector('td.cell-version').innerHTML = data.Version;
});

connection.on("connectionRemove", (content) => {
    console.log(content);
    const data = JSON.parse(content);
    const raw = document.getElementById(data.Id);
    raw.remove();
    const table = document.getElementById('table-connections');

    for (const child of table.children) {
        child.querySelector('td.cell-num').innerHTML = Array.from(table.children).indexOf(child);
    }
});
connection.on("connectionUpdateAll", (content) => {
    console.log(content);
    const data = JSON.parse(content);
    const raw = document.getElementById(data.Id);

    raw.querySelector('td.cell-num').innerHTML = Array.from(raw.parentNode.children).indexOf(raw);
    raw.querySelector('td.cell-id').innerHTML = data.Id;
    raw.querySelector('td.cell-host').innerHTML = data.Host;
    raw.querySelector('td.cell-port').innerHTML = data.Port;
    raw.querySelector('td.cell-status').innerHTML = data.Status;
    raw.querySelector('td.cell-version').innerHTML = data.Version;
});

connection.on("connectionUpdateStatus", (content) => {
    throw new Error('not implemented');
});

connection.onclose((er) => {
    console.log(`Connection closed. Er:${er}`);
    start();    // reconnect
});

start();

function start() {
    var promise = connection.start();
    promise.then((x) => {
        connection.invoke(`NotifyStart`, `this is string from client sended after begin`);
    });
}
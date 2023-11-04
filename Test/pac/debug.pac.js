function FindProxyForURL(url, host) {

    if (isInNet(resolved_ip, "192.168.0.3", "255.255.255.0") || isInNet(resolved_ip, "127.0.0.1", "255.255.255.255")) return "DIRECT";

    if (url.startsWith("http://")) return "DIRECT";
    if (url.startsWith("https://")) return "DIRECT";

    const resolved_ip = dnsResolve(host);
    return "DIRECT";
}
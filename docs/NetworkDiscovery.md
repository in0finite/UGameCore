
### How to implement net discovery ?

1. use integrated NetworkDiscovery from unity

2. do it yourself with UDP clients

***

### NetworkDiscovery pros:

- house-keeping done for you (performance, multiple threads ?, errors when reading data ?, broadcast key, version, etc)

- works on all platforms ?


### NetworkDiscovery cons:

- networking is no longer maintained by unity

- needs a separate game object because it calls DontDestroyOnLoad()

- sometimes mixes broadcast hostId with the one from network server - when you try to start network server, it says it can not open port (even through server was stopped, and noone should be listening on that port)

- can't properly shutdown sockets, hosts or whatever - so when you start/stop broadcasting or receiving multiple times, it suddenly displays error that it can not open socket on specified port

- who knows what more bugs it has

- some of these bugs are inside NetworkTransport class, which is closed source, so they can't be fixed


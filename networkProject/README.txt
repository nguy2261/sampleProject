(Multi-threaded TCP server)
twitterTrend_server
twitterTrend_client

We are going to implement a multi-threaded twitterTrend server and twitterTrend client.

TwitterTrend server is a simple program that takes city names from client programs and serves top 3 trending keywords to client programs. 
The server is serving multiple concurrent clients in a pool of threads. 

One thread on the server program handles a client program. TwitterTrend client sends the request to get the 3 trending keywords from the server. 

This client program have a clientX.in file that contains one or multiple city names. 
In addition, the communication protocol (messages) is implemented to send and receive the data through the network. 

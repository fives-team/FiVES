# How to use this Dockerfile

You can build a docker image based on this Dockerfile. This image will contain a running instance of the FiVES Synchronization GEi. FiVES will expose ports 8181 for clients to retrieve the server configuration files, 8081 for the REST Scene API, and 34837 for WebSocket Realtime Synchronization. This requires that you have [docker](https://docs.docker.com/installation/) installed on your machine.

If you want to have a FiVES instance running as quickly as possible, please have a look at section _The Fastest Way_ .

## The Fastest Way

### Run The Container

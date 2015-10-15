# How to use this Dockerfile

You can build a docker image based on this Dockerfile. This image will contain a running instance of the FiVES Synchronization GEi. FiVES will expose ports 8181 for clients to retrieve the server configuration files, 8081 for the REST Scene API, and 34837 for WebSocket Realtime Synchronization. This requires that you have [docker](https://docs.docker.com/installation/) installed on your machine.

If you want to have a FiVES instance running as quickly as possible, please have a look at section _The Fastest Way_ .

## The Fastest Way

### Run The Container

`docker run -t -i tospie/fives`

You can define forwarded ports with the -p flag. By default, FiVES listens on ports 8181 for SINFONI services, 8081 for the Scene API REST interface, and 34837 for realtime synchronization:

`docker run -t -p 818:8181 -p 8081:8081 -p 34837:34837 tospie/fives`

The above commands pull the image from the Docker Registry instead of building your own. Keep in mind that the pulled image is run locally.

After the image is downloaded, you can expect the self-introducing server configuration file at `http://[HOSTADDRESS]:8181/fives/` , and access the Scene API via `http://[HOSTADDRESS]:8081/entites` .

# How to use this Dockerfile

You can build a docker image based on this Dockerfile. This image will contain a running instance of the FiVES Synchronization GEi. FiVES will expose ports 8181 for clients to retrieve the server configuration files, 8081 for the REST Scene API, and 34837 for WebSocket Realtime Synchronization. This requires that you have [docker](https://docs.docker.com/installation/) installed on your machine.

If you want to have a FiVES instance running as quickly as possible, please have a look at section _The Fastest Way_ .

## The Fastest Way

### Run The Container

`docker run -t -i tospie/fives`

You can define forwarded ports with the -p flag. By default, FiVES listens on ports 8181 for SINFONI services, 8081 for the Scene API REST interface, and 34837 for realtime synchronization:

`docker run -t -i -p 8181:8181 -p 8081:8081 -p 34837:34837 tospie/fives`

The above commands pull the image from the Docker Registry instead of building your own. Keep in mind that the pulled image is run locally.

After the image is downloaded, you can expect the self-introducing server configuration file at `http://[HOSTADDRESS]:8181/fives/` , and access the Scene API via `http://[HOSTADDRESS]:8081/entites` .

## Build the image

This is an alternative approach to the one presented in the previous section. You do not need to go through these steps if you have downloaded image from Dockerhub. The end result will be the same, but this way you have a bit more of control of what's happening.

You only need to do this once you have downloaded Dockerfile to your system:

`sudo docker build -t fives . `

The parameter `-t fives` gives the image a name. This name could be anything, or even include an organization like ``-t org/fives``. This name is later used to run the container based on the image.

If you want to know more about images and the building process you can find it in [Docker's documentation](https://docs.docker.com/userguide/dockerimages/).

### Run the Container

The following line will run the container exposing ports 8181, 8081 and 34837:

`sudo docker run -t -i -p 8181:8181 -p 8081:8081 -p 34837:34837 fives`

If you did not build the image yourself and want to use the one on Docker Hub use the following command:

`sudo docker run -t -i -p 818:8181 -p 8081:8081 -p 34837:34837 tospie/fives`

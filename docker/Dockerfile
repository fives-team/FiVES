FROM mono:latest

MAINTAINER Torsten Spieldenner <torsten.spieldenner@dfki.de>

RUN apt-get update \
	&& apt-get -y install git

RUN cd ~ \
	&& git clone https://github.com/fives-team/fives -b docker-linux-install-511

RUN cd ~/fives \
	&& nuget restore FIVES.sln \
	&& xbuild FIVES.sln

WORKDIR /root/fives/Binaries/Debug

EXPOSE 8181 8081 34837

CMD ["mono", "FIVES.exe"]

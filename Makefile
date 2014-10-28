SHELL := /bin/bash

build: chromedriver correctpaths mozcerts
	echo "Building"
	xbuild
	rm /tmp/nuget -rf

chromedriver:
	if [ ! -f chromedriver.exe ]; then                                               \
	  echo "Downloading Chrome Driver";                                              \
	  wget http://chromedriver.storage.googleapis.com/2.10/chromedriver_linux64.zip; \
	  unzip chromedriver_linux64.zip;                                                \
	  mv chromedriver chromedriver.exe;                                              \
	fi

correctpaths:
	echo "Replacing \\ with / in all *.csproj and *.sln files"
	sed -i 's/\\/\//g' $$(find . -name *.csproj -o -name *.sln)

mozcerts:
	echo "Installing Mozilla certificates"
	mozroots --import --sync

tests: build
	for PROJECT_FILE in $$(find . -name *.csproj); do                                                                     \
	  echo "Running tests from $$PROJECT_FILE";                                                                           \
	  PATH=$$PATH:$$PWD xvfb-run -a mono packages/NUnit.Runners.2.6.3/tools/nunit-console.exe $$PROJECT_FILE || exit $$?; \
	done

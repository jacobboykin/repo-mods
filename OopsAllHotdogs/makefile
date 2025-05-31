SHELL = /usr/bin/env bash -o pipefail
.SHELLFLAGS = -ec

# Build the project
build:
	msbuild

# Prepare an artifact to zip up and publish
prepare: build
	cp bin/Debug/OopsAllHotdogs.dll artifact/

# Zip the contents into artifact.zip for uploading to thunderstore
zip: prepare
	zip -r artifact.zip artifact/*
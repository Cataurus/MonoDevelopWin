#!/bin/bash

#. ./Install.DotNet.sh
. ./Install.Mono.sh

sudo apt-get install -y git make cmake gnupg ca-certificates build-essential intltool libssh2-1-dev zlib1g-dev

sudo apt-get install -y fsharp gtk-sharp2 software-properties-common

sudo apt-get update
sudo apt-get upgrade

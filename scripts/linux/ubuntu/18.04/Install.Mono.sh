#!/bin/bash

echo
echo ========================================
echo system update und notwendige pakete
echo ========================================
echo
sudo apt-get update
sudo apt-get upgrade

sudo apt-get install -y gnupg ca-certificates

echo
echo ========================================
echo Mono Verzeichnis wird hinzu gefügt
echo ========================================
echo

sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list

sudo apt-get update
sudo apt-get upgrade

echo
echo ========================================
echo Mono-devel wird installiert
echo ========================================
echo

sudo apt-get install -y mono-devel

echo 
mono --version

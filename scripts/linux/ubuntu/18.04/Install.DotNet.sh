#!/bin/bash

echo 
echo ========================================
echo installiert .Net auf ihrem system
echo ========================================
echo 

echo
echo ========================================
echo system update
echo ========================================
echo
sudo apt-get update
sudo apt-get upgrade

echo
echo ========================================
echo notwendige pakete werden installiert
echo ========================================
echo
sudo apt-get -y /
    wget /
    ca-certificates

echo
echo ========================================
echo .Net installationspaket wird geladen
echo ========================================
echo
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

echo
echo ========================================
echo .net installationspaket wird installiert
echo ========================================
echo
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get install -y apt-transport-https dirmngr

sudo apt-get update
sudo apt-get upgrade

echo
echo ========================================
echo .Net SDK LS wird installiert
echo ========================================
echo
sudo apt-get install -y dotnet-sdk-2.1

echo
echo ========================================
echo Prüfe Installation
echo ========================================
echo
dotnet --info

#!/bin/bash

inputDir=""
if [ "$1" == "" ]
    then
        inputDir="$HOME/isiSpamDetect/input/"
    else
        inputDir=$1
fi
inputDir=$(echo $inputDir | sed -r 's/(.*[^\/])$/\1\//g')
echo "Parujse do xml..."
./txt2xml.sh 'manual' "$inputDir"

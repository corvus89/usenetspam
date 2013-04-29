#!/bin/bash

mojeDir=$2
lista=$(ls $mojeDir | tr ' \n' ' ')
mojeDir=$(echo $mojeDir | sed  's/\//\\\//g')
wynik=$(echo $lista | sed -re "s/([^ ]+)/$mojeDir\1/g")

javac ParserXML.java
java ParserXML "$1" $wynik

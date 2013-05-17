#!/bin/bash
mojeDir=$(echo $1 | sed -r 's/(.+)\/$/\1/g')
rm $mojeDir/../content_short/*
rm -r $mojeDir/../content_short
lista=$(ls $mojeDir | tr ' \n' ' ')
mkdir $mojeDir/../content_short
for f in $lista
do
	tekst=$(cat $mojeDir/$f)
	if [ ${#tekst} -gt 150 ]
	then
		echo $tekst > $mojeDir/../content_short/$f.txt	
	fi
done

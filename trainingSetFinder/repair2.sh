#!/bin/bash
mojeDir=$(echo $1 | sed -r 's/(.+)\/$/\1/g')
rm $mojeDir/../content_short/*
rm -r $mojeDir/../content_short
lista=$(ls $mojeDir | tr ' \n' ' ')
mkdir $mojeDir/../content_short
for f in $lista
do
	cat $mojeDir/$f |  sed -r 's/[:alnum:]{100,} ?//g' > $mojeDir/../content_short/$f
done

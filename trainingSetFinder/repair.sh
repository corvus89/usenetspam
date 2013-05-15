#!/bin/bash
mojeDir=$(echo $1 | sed -r 's/(.+)\/$/\1/g')
rm $mojeDir/../content_popr/*
rm -r $mojeDir/../content_popr
lista=$(ls $mojeDir | tr ' \n' ' ')
mkdir $mojeDir/../content_popr
for f in $lista
do
	cat $mojeDir/$f |  sed -r 's/Ą/a/g' | sed -r 's/Ć/c/g' | sed -r 's/Ę/e/g' | sed -r 's/Ł/l/g' | sed -r 's/Ń/n/g' | sed -r 's/Ó/o/g' | sed -r 's/Ś/s/g' | sed -r 's/Ż/z/g' | sed -r 's/Ź/z/g' | tr -dc '[:print:]\n' > $mojeDir/../content_popr/$f
done

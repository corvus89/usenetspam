#!/bin/bash
oldDir=$(pwd)
mojeDir=$(echo $1 | sed -r 's/(.+)\/$/\1/g')

rm $mojeDir/../content_many/*
rm -r $mojeDir/../content_many
lista=$(ls $mojeDir | tr ' \n' ' ')
mkdir $mojeDir/../content_many
cd $mojeDir/../content_many/
for f in $lista
do
	echo $mojeDir/$f
	name=${f:0:${#f} - 4}
	csplit -sz --prefix='' --suffix-format=$name"_"%06d $mojeDir/$f /\n/ {*}
done

cd $oldDir

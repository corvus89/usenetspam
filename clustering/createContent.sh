#!/bin/bash
mojeDir=$(echo $1 | sed -r 's/(.+)\/$/\1/g')
rm $mojeDir/content/all
rm -r $mojeDir/content
mkdir $mojeDir/content
lista=$(ls $mojeDir/ | tr ' \n' ' ')
for f in $lista
do
    echo $mojeDir/$f
    cat $mojeDir/$f | tr -d '\t' | sed -r 's/.+<\/content>/\t/g'| sed -r '/(^<.+>(.+)(<\/.+>)$)|(<.+>)/d' | tr '[:upper:]' '[:lower:]' | tr -d '[:punct:]' | tr '\n' ' ' | sed -r 's/ \t /\n/g' | sed -r 's/[ ]+/ /g' | sed -r 's/(^ )|( $)//g' | sed -r 's/[0-9]+//g' | sed -r 's/(http)|(www)[a-z]+//g' | sed -r 's/ą/a/g' | sed -r 's/ć/c/g' | sed -r 's/ę/e/g' | sed -r 's/ł/l/g' | sed -r 's/ń/n/g' | sed -r 's/ó/o/g' | sed -r 's/ś/s/g' | sed -r 's/ż/z/g' | sed -r 's/ź/z/g' > $mojeDir/content/$f
done

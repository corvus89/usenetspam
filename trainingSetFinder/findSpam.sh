#!/bin/bash

lista=$(ls parsed_files | tr ' \n' ' ')
for f in $lista
do
 #egrep -Hn -C 10 "subject" $f
 wynik=$(perl getIDofRegular.pl parsed_files/$f)
 for id in $wynik
 do
  bash ./id2xml.sh $id
  echo "+++++++++++++++++++++++++++"
 done
 echo "============================="
done

#!/bin/bash

oldDir=$(pwd)
myDir=$(echo "$0" | sed -r 's/(.*)\/(.+)$/\1/g')
cd "$myDir"
linia=$(egrep "^$1;" "index.txt")
linia=$(echo "$linia" | sed -r 's/([0-9]+ [0-9]+ [0-9]+ )(.+( .+)*)/\1\"\2\"/g')
linia=$(echo "$linia" | tr ';' ' ')
array=($linia)
echo $linia
start="${array[1]}"
end="${array[2]}"
name="${array[3]}"
len=$((${#array[@]}))
if [ $len -ne 4 ]
then
for i in 4..$len
do
#echo ${array[$i]}
name=$name' '${array[$i]}
done
fi

end=$(($end - $start + 1))
echo '<?xml version="1.0" encoding="utf-8"?>'
cat "parsed_files/$name.xml" | tail -n +$start | head -n $end | sed -r 's/\[CDATA\[\[/\[CDATA\[/g'
cd $oldDir

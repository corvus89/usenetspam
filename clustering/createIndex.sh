ls | cut -d '_' -f1 | uniq -c | sed -r 's/^[  ]+//g' > ../indeksgrup

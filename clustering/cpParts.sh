#!/bin/bash
rm content10/*
rm -r content10
mkdir content10
while read linia
do
	nazwa=$(cut -d ' ' -f2)
	cp $nazwa"_part"{1..10} content10/
done

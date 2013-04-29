#!/usr/bin/perl

use feature "say";
use strict;
use utf8;
use Encode;
binmode(STDOUT, ":utf8");

my $lastId = "";
my @matchingId = ();

while(<>){
	$_ = decode('UTF-8', $_);
	chomp($_);
	if($_ =~ m/<sphinx:document id="([0-9]+)">/){
		$lastId = $1;
	} else {
		if($_ =~ m/<from><!\[CDATA\[\["3cat" <3cat\@polbox.com>\]\]><\/from>/ 
			or $_ =~ m/3cat/) {
			push(@matchingId, $lastId);
		}
	}
}
say join(" ", @matchingId);

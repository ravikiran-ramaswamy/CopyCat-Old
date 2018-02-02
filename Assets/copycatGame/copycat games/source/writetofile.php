<?php
// the name of the text file

// get the variables from flash
$disc= $_REQUEST['disc'];
$somecontent = $_REQUEST['somecontent'];

if($disc == 'bad_santa')
	$filename = 'copycat_badsanta_log.txt';
elseif($disc == 'sea_hunt')
	$filename = 'copycat_seahunt_log.txt';
elseif($disc == 'crystals')
	$filename = 'copycat_crystals_log.txt';
elseif($disc == 'dragon_cave')
	$filename = 'copycat_dragoncave_log.txt';
elseif($disc == 'ghost_house)
	$filename = 'copycat_ghosthouse_log.txt';
else
	$filename = 'copycat_alienabduction_log.txt';

// If the file doesn't exist, attempt to create it
// and open it for writing
if (!$handle = fopen($filename, 'a')) {
	 echo "&message=Cannot open file&";
	 exit;
}

// Write $somecontent to the opened file.
if (fwrite($handle, $somecontent) === FALSE) {
	echo "&message=Cannot write to file&";
	exit;
}
// close the file
fclose($handle);

// send the variable to flash
echo "&message=$somecontent&";

?> 

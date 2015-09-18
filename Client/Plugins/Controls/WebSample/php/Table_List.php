<html>
<head>

<title>MySQL Table List</title>

<style>

h3 {
	margin-top: 0px;
	margin-bottom: 20px;
}

.item {
	height: 30px;

	line-height: 30px;
	color: #333333;
}

.item:hover {
	color: #0080ff;
}

</style>

</head>
<body>
<?php
$servername = "localhost";
$username = "feenuxco_reader";
$password = "ethan123";
$database = "feenuxco_customerlocal_cnc_mazatrol_mazak_01";

if (!mysql_connect($servername, $username, $password))
    die("Can't connect to database");

if (!mysql_select_db($database))
    die("Can't select database");

// sending query
$sql = "SHOW TABLES";
$result = mysql_query($sql);


if ($result)
{
// printing table rows
while($row = mysql_fetch_row($result))
{
	if ($row[0] == $_POST["tablename"])
	{
		echo "<button type='submit' name='tablename' value='{$row[0]}' class='hvr-sweep-to-right-selected'>{$row[0]}</button><br>";
	}
	else
	{
		echo "<button type='submit' name='tablename' value='{$row[0]}' class='hvr-sweep-to-right'>{$row[0]}</button><br>";
	}
}

mysql_free_result($result);
}

?>
</body></html> 
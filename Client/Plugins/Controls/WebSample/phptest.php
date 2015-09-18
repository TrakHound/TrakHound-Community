<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>

    <title>TrakHound - Web Sample - Plugin</title>
	
	<style>
		
	#list-div {
		
		float: left;
		margin-right: 50px;
		margin-top: 20px;
		margin-bottom: 20px;
		min-width: 150px;
		
	}
	
	#demo-div {
		
		width: 75%;
		margin-left: auto;
		margin-right: auto;
		max-width: 1400px;
	}
	
	#table-div {
		
		width: 100%;
		margin-left: auto;
		margin-right: auto;
		margin-bottom: 50px;
		margin-top: 65px;
		float: left;
		max-width: 1200px;
		
		font-size: 10pt;
	}
	
	#th-div {
		
		width: 100%;
		height: 100%;
		
	}
	
	</style>

</head>

<body>

    <div id="main-wrap">

        <div id="trakhound-background">

			<div id="demo-div">
			
			<h1>Demo</h1>
			
			<p>Below is a demo of the tables available from the TrakHound Server application. Click on the table names on the left to view the data available:</p>
			
				<div id="list-div">
				
					<h3 style="text-align: center;">Tables</h3>
				
					<form action="" method="post">
						<?php include("php/Table_List.php"); ?>
					</form>
				
				</div>
				
				<div id="table-div">
				
					<?php include ("php/Table_Display.php"); ?>
				
				</div>
			
			</div>

        </div>

    </div>
	
</body>

</html>

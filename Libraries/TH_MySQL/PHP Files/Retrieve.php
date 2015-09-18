<?php
$servername = $_POST['server'];
$username = $_POST['user'];
$password = $_POST['password'];
$database = $_POST['db'];

// Create connection
$conn = new mysqli($servername, $username, $password, $database);

// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

$sql = $_POST["query"];
$result = $conn->query($sql);

$rows = array();

if ($result) {

if ($result->num_rows > 0) {
    // output data of each row
    while($row = $result->fetch_assoc()) {
        $rows[] = $row;
    }

print json_encode($rows);

}
}

$conn->close();
?>

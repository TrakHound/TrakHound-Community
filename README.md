![TrakHound Logo] (http://feenux.com/github/images/github_header_01.png)

<p align="center">
  <a href="http://www.trakhound.org/">www.TrakHound.org</a>
</p>

##About
TrakHound is an Open Source software package designed to retrieve and store data from CNC and other PLC driven equipment. Data is retrieved using <a href="http://mtconnect.org">MTConnect®</a> and data is stored in a database. 

##Client
TrakHound Client is a Windows application and is used to view the data that was processed and stored by the server. The client's core only manages the pages, updates, etc. and all of the data related pages are plugins. Preinstalled plugins include Dashboard, Device Compare, Table Manager, and Status Data. These provide the tools necessary to monitor current basic device / production data.

<div align="center">
  <div>
    <img width="400" src="http://feenux.com/github/images/client_dashboard_01.png"/>
    &nbsp;&nbsp;
    <img width="400" src="http://feenux.com/github/images/client_tablemanager_01.png"/>
  </div>
</div>

<br>

<div align="center">
  <div>
    <img width="400" src="http://feenux.com/github/images/client_devicemanager_01.png"/>
    &nbsp;&nbsp;
    <img width="400" src="http://feenux.com/github/images/client_plugins_01.png"/>
  </div>
</div>




##Server
TrakHound Server is used to collect, process, and store the data from each device. The data collection is built in but the processing of the data is left up to plugins. Server plugins receive the Probe, Current, and Sample data and can also send data between each other. Preinstalled plugins include Instance Table, Generated Data, Shifts, Cycles, and OEE. Data is stored in a database which is also controlled by plugins. Preinstalled is a MySQL database plugin but any other SQL type of database can be developed. Data can also be stored in multiple databases for redundancy or other uses.

The Server is ran as a Windows application and is controlled by a System Tray Icon menu. The menu contains options to Login / Logout, Start / Stop server, open the Device Manager for configuring the server, and a Console. 

<br>

<div align="center">
  <div>
    <img src="http://feenux.com/github/images/server_menu_01.png"/>
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
    <img src="http://feenux.com/github/images/server_menu_02.png"/>
  </div>
</div>

<br><br>

Important messages are displayed as Windows Notifications and others are written to the Console and stored in log files.

![TrakHound Server Notification] (http://feenux.com/github/images/server_notification_01.png)

![TrakHound Server Console] (http://feenux.com/github/images/server_console_01.png)



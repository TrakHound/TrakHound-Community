<!--
  Title: TrakHound
  Description: Open Source MDC Software Package using MTConnect
  Author: Feenux LLC
  -->
  
![TrakHound Logo] (http://feenux.com/github/v2/images/headers/header_01.png)

<p align="center">
  <a href="http://www.trakhound.org/">www.TrakHound.org</a>
</p>

##About
TrakHound is an Open Source MDC software package designed to retrieve and store data from CNC and other PLC driven industrial equipment. Data is retrieved using <a href="http://mtconnect.org">MTConnect®</a>, processed, and then stored in a database. 

Machine Data Collection (MDC) software is used to evaluate shop productivity and identify problem areas. TrakHound provides free dashboards to view current machine status and basic production data so shop supervisors can quickly see which machines are operating normally and which ones are in need of attention. This transparency allows for a smoother workflow through your shop with reduced downtime. 

Setup is easy using the built in Device Manager where a user can search their entire network for MTConnect compatible machines and then those machines get cross-referenced with our Device Catalog to find the best matched Device Configuration. Once matched, a list of machines on the network will be shown and the user can simply select which ones to monitor. Device Configurations are fully customizable by the user.

 <table style="width:100%">
 
  <tr>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/Dashboard_01.png"/></td>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/Dashboard_02.png"/></td>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/Dashboard_03.png"/></td>
  </tr>
  
  <tr>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/DeviceManager_01.png"/></td>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/DeviceManager_02.png"/></td>
    <td><img src="http://www.feenux.com/github/v2/images/screenshots/DeviceManager_05.png"/></td>
  </tr>
  
</table> 

##How it Works

TrakHound works by reading data using the MTConnect communications protocol. This data is then processed and stored in a database. The default database is SQLite where data is stored on the PC that TrakHound is installed on. Other databases can be used for remote storage and currently MySQL and SQL Server are supported.

####Basic Setup
Below is a diagram showing how a basic setup works. This shows two machines communicating directly with one PC and all data is stored on that PC. This is a good starting point for most users and can be used in scenarios where only one user needs to view data.
![Basic Communications Setup] (http://feenux.com/github/v2/images/diagrams/overview_basic_01.png)

####Standalone Server Setup
Below is a diagram showing how a standalone server setup works. This shows two machines communicating with a standalone server with TrakHound Server installed. The server then stores the data in a separate database that can be accessed by PC's on the network. Multiple PC's can then view data using TrakHound while reading directly from the database.
![Standalone Communications Setup] (http://feenux.com/github/v2/images/diagrams/overview_standalone_01.png)

##Licensing
TrakHound is licensed under the GPLv3 software license. For more information please contact us at info@TrakHound.org.

##Contributions
TrakHound welcomes any comments, reccomendations, pull requests, or any other type of contributions! Please fork and contribute back at any time as this project was created as a tool for the community. If you have any questions please contact us at info@TrakHound.org.

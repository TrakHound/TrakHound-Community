![TrakHound Logo] (http://feenux.com/github/images/github_header_01.png)

<p align="center">
  <a href="http://www.trakhound.org/">www.TrakHound.org</a>
</p>

##About
TrakHound is an Open Source software package designed to retrieve and store data from CNC and other PLC driven equipment. Data is retrieved using <a href="http://mtconnect.org">MTConnect®</a> and data is stored in a database. 

##Server
TrakHound Server is used to collect, process, and store the data from each device. The data collection is built in but the processing of the data is left up to plugins. Server plugins receive the Probe, Current, and Sample data and can also send data between each other. Preinstalled plugins include Instance Table, Generated Data, Shifts, Cycles, and OEE. Data is stored in a database which is also controlled by plugins. Preinstalled is a MySQL database plugin but any other SQL type of database can be developed. Data can also be stored in multiple databases for redundancy or other uses.

The Server is ran as a Windows application and is controlled by a System Tray Icon menu. The menu contains options to Login / Logout, Start / Stop server, open the Device Manager for configuring the server, and a Console. 

![TrakHound Server Menu (Logged In)] (http://feenux.com/github/images/server_menu_01.png)

![TrakHound Server Menu (Logged Out)] (http://feenux.com/github/images/server_menu_02.png)

Important messages are displayed as Windows Notifications.

![TrakHound Server Notification] (http://feenux.com/github/images/server_notification_01.png)

##Client
TrakHound Client 


TrakHound is comprised of Server and Client applications.


##What Makes TrakHound Different than the Rest?

TrakHound is not just another CNC Monitoring Software, it acts as a platform for developers to collaborate their ideas into one software package which in-turn will provide the CNC community with the best solution possible! There are no upgrades or premium versions of TrakHound to purchase. The Client and Server applications are both totally free of charge and we intend to keep it that way. We believe the best way to provide our end users with a solution tailored to them is to let them choose which pages, screens, data, etc. they want by way of installing plugins. These plugins can both be developed by TrakHound as well others in the community. Some may cost while others are free. This allows a company who has the capabilities to develop their own plugin to fit their own needs to also share that with others that can use it and improve upon it. This type of community involvement is what is necessary to move CNC technology out of the "stone age".

##Code
The source code is contained in one VS2013 Solution and is organized by Client, Server, and Library directories. The Client Plugins are built to the 'C:\TrakHound\Plugins\' directory and the Server Plugins are built to the 'C:\TrakHound\Server\Plugins\' directory. This is where the Client and Server look for plugins to load. The Configuration files can be anywhere and a sample configuration file is contained in the Solution's root folder. To use this configuration, you must edit the Configuration.Xml files for both the Client and Server applications to point to the location where you put the Solution files. See example below:

#Configuration.XML

    <Configuration>
      <Devices>
        <Device>
          <Settings_Path>C:\TrakHound\Configuration Files\MTConnect\Settings.xml</Settings_Path>
        </Device>
      </Devices>
    </Configuration>
  
Note: This configuration setup is not the intended final solutions and a better, easier way is currently being worked on. This was to avoid having to log into any servers or create any accounts in order to use the software.


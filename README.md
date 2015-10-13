![TrakHound Logo] (http://feenux.com/misc/TrakHound_Logo_Full_10.png)

[TrakHound.org] (http://www.trakhound.org/)

![ScreenShot] (http://feenux.com/trakhound/images/devicecompare_05_sm.png)

##About
TrakHound is an Open Source software package designed to use the MTConnect® communication protocol to retrieve and store data from CNC and other PLC driven equipment. Since TrakHound is based on a plugin architecture, you can think of it as more of a development platform than a stand alone monitoring application. Most of the work in developing a MTConnect® compatible application is already done and all that is needed is to develop a simple plugin for TrakHound!

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


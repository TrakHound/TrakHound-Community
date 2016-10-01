<!--
  Title: TrakHound
  Description: Open Source MDC Software Package using MTConnect
  Author: Feenux LLC
  -->
  
![TrakHound Logo] (http://trakhound.com/images/github-header.jpg)

<p align="center">
  <a href="http://www.trakhound.com/">www.TrakHound.com</a>
</p>

## News
**Update 9/8/16 - v1.3 Official Release** 

### User Interface

The User Interface has been updated and now features a cleaner interface that should be eaiser to read as status colors stand out better on the lighter background. We are also working on a Themes feature where user-defined themes can be used.

### New TrakHound API
- The Server is now broken down into two parts. The Data Processing Server, which collects data using MTConnect and then processes it using plugins, now sends that processed data to a Data Storage Server using HTTP and JSON. The benefit to seperating these is that the data storage server can be written in many different formats and languages and provides the ability to store data in many more ways. 
- The TrakHound Server application/service runs both the Data Processing Server and a local Data Storage Server. The local Data Storage Server recieves the JSON data and stores it in a local SQLite database. We also have an Apache/PHP/MySQL based Data Storage Server that receives the JSON data and stores it in a remote MySQL database. The Apache/PHP/MySQL based application is not open source as of yet but we plan to once we can implement a better authentication system. 
- The best advantage of this setup is that data can be retrieved using the TrakHound API HTTP functions which allows for clients to easily be developed on many different platforms. This also allows TrakHound to supplement other systems/software as it can be easily integrated. Previous to this change, data was read directly from the database which meant that it was up to the client to establish communication with the database server itself. This caused issues when trying to communicate with different database types (MySQL, SQLite, SQL Server, etc.). Now, the client just has to communicate using the standard API and basic HTTP functions.
- Using the new HTTP based API now makes TrakHound fully cloud compatible!

### IMTS 2016
- TrakHound is going to be featured in Okuma's booth at IMTS 2016 (booth S-8500) as TrakHound is now part of the Okuma App Store. Okuma customers can now go to the Okuma App Store to download their MTConnect Agent/Adapter and will now see TrakHound Community (listed as MTConnect Display) right beside it. Check it out at <a href="https://www.myokuma.com/mtconnect-display">Okuma App Store - MTConnect Display</a>.


## Help Needed!

- We are working on our documenation and if anyone has any good screenshots of the TrakHound dashboards or other screens and would like to contribute then please send those to info@trakhound.com.


##About
TrakHound is an Open Source MDC software package designed to retrieve and store data from CNC and other PLC driven industrial equipment. Data is retrieved using <a href="http://mtconnect.org">MTConnect®</a>, processed, and then stored in a database. 

Machine Data Collection (MDC) software is used to evaluate shop productivity and identify problem areas. TrakHound provides free dashboards to view current machine status and basic production data so shop supervisors can quickly see which machines are operating normally and which ones are in need of attention. This transparency allows for a smoother workflow through your shop with reduced downtime. 

Setup is easy using the built in Device Manager where a user can search their entire network for MTConnect compatible machines and then those machines get cross-referenced with our Device Catalog to find the best matched Device Configuration. Once matched, a list of machines on the network will be shown and the user can simply select which ones to monitor. Device Configurations are fully customizable by the user.

 <table style="width:100%">
 
  <tr>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/overview_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/controllerstatus_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/oeetimeline_01.png"/></td>
  </tr>
  
  <tr>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/oeestatus_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/devicestatustimes_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/productionstatustimes_01.png"/></td>
  </tr>
  
  <tr>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/devicemanager_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/autodetect_01.png"/></td>
    <td><img src="http://www.trakhound.com/images/download_screenshots/trakhound_community/v1.3/edit_description_01.png"/></td>
  </tr>
 
</table> 


####Basic Setup
Below is a diagram showing how a basic setup works. This shows two machines communicating directly with one PC and all data is stored on that PC. This is a good starting point for most users and can be used in scenarios where only one user needs to view data.
![Basic Communications Setup] (http://trakhound.com/images/overview_basic_04.png)

##Licensing & Dependencies
TrakHound is licensed under the [GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html) software license. For more information about open source licensing or purchasing a commercial license please contact us at info@TrakHound.com.

TrakHound uses the following external libraries:
- MTConnect.NET by TrakHound - [Source Code](https://github.com/TrakHound/MTConnect.NET) - [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0)
- [JSON.NET](http://www.newtonsoft.com/json) by Newtonsoft - [Source Code](https://github.com/JamesNK/Newtonsoft.Json) - [MIT](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)
- [SQLite](https://www.sqlite.org/index.html) - [Source Code](https://www.sqlite.org/download.html) - [Public Domain](https://www.sqlite.org/copyright.html)

##Contributions
TrakHound welcomes any comments, reccomendations, pull requests, or any other type of contributions! Please fork and contribute back at any time as this project was created as a tool for the community. If you have any questions please contact us at info@TrakHound.org.

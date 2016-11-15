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
### Update 11/15/16 

We have been hard at work getting TrakHound ready for 2017 and are excited to announce some upcoming changes! We have learned a lot since the beginning of this project and feel TrakHound will very quickly represent the next step in the IIoT for advanced manufacturing.

#### TrakHound API
Early 2017 we will be launching TrakHound v2 which features a brand new API that will follow a standard RESTful model, store more detailed device data, and be compatible with most programming languages and applications. Along with TrakHound Server, this API provides a fully featured open source data collection solution for any MTConnect application. Applications only have to interface with the API using HTTP where they can directly access fully processed manufacturing data which can then be easily displayed or graphed on any Web, Mobile, or PC application.

#### TrakHound Server
TrakHound Server is constantly being improved upon in order to provide more data and be more stable. Our goal with TrakHound Server is to be to MTConnect what Apache is to HTTP. Having a widely used and open source server application for MTConnect will drastically expand the accessibility and adoption of the protocol.


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
TrakHound welcomes any comments, reccomendations, pull requests, or any other type of contributions! Please fork and contribute back at any time as this project was created as a tool for the community. If you have any questions please contact us at info@TrakHound.com.

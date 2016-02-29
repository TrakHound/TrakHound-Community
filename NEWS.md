# TrakHound News

## Recent Updates
Below is a list of recent updates to TrakHound 

- [v1.6.2](../../../TrakHound/releases/tag/v1.6.2-beta)

## Upcoming Updates
Below is a list of upcoming updates to TrakHound (mainly to fix current issues)

- Add SQLite Database plugin to project
- Enable 'non user' based configurations. This was intially the only way to configure TrakHound and was partially kept while remote user accounts were added but just needs to be finished.
- Add ability to 'link' Device Catalog configurations to a Device Name that would be found in an MTConnect Probe request. This will allow for the user to simply enter an IP address and TrakHound will automatically select the best suitable configuration for their device.
- Redesign 'Add Device' page to start by listing reachable IP addresses on the network and then running a Probe request on each and apply references to Device Catalog. This will result in the user simply selecting which machine to add from the list of network nodes and TrakHound will automitically add and the appropriate configuration.
- Improve TH_StatusData client plugin to look at 'INFORMATION_SCHEMA' table to only retrieve tables that were changed. This will improve performance for TrakHound Client as it will not be retrieving tables that haven't even changed (this is especially true for remote databases).
- Completely catch any exceptions in plugins. This is especially relevant to TrakHound Client and it is partially implemented already but a Try Catch is going to be required in any plugin with a specific exception that Client or Server will handle.


## Upcoming Projects
Below is a list of upcoming projects to TrakHound that will add new Features and Functionality

- Finalize Plugin interfaces and API documentation. Add Plugin NuGet packages (Client, Server, and Database) to Nuget.org for ease of plugin development.
- Add Automatic Updates functionality. This was previously implemented but some issues were found so it was removed.
- Add Error Dialog to add Issues directly to Github from TrakHound. For example, if TrakHound Client throws an exception, a dialog will appear where the user can Add an issue or if the issue has already been added, the user can comment on the issue, and if the issue has been resolved, the user can click a link to either reference the Release that fixed the issue or offer to update to the latest release.
- Revert TrakHound Server back to being run as a Windows Service. It was changed to a standard Windows Application when user accounts were introduced as the 'Remember Me' settings were stored in the Registry and a Registry location wasn't found that could be accessed by both the Service and any Forms that were run under the currrent Windows User (user might have limited privileges). Forms would include the TrakHound Login Dialog along with Device Manager. A standard Windows Application was the temporary solution to this problem. A better solution is needed that would most likely require using WCF to communicate between any user dialogs and the sever's service.
- 
## Needed
Below is a list of items that we are looking for outside help for. If you are able to help in any way please contact us info@TrakHound.org.

- A Linux Mono implementation for TrakHound-Server-Core that is simply capable of running the server. We are looking to be able to distribute the Server application on Linux systems which would help broaden the ways TrakHound can be integrated into already existing systems.
- A web based Device Manager that is capable of at least enabling/disabling devices and ability to restart server.
- A web based Table Manager that is capable of viewing raw data for each device's database.


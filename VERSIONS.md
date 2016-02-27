# Versions
### List of what is contained in each version and what is planned for future versions.

## v1.6.2
- [x] Fix TH_DeviceCompare_OEE timeline reset to accomodate issue #2
- [x] Fix Database Configuration Page issues relating to password and username for UseTrakHoundCloud option
- [x] Fix Database Configuration Page issues relating to showing that a password has been entered
- [x] Improved retrieving the list of Devices in TrakHound Client by only retrieving the list for Device Manager and then sending that list to all of the plugins. Plugins using the IClientPlugin interface are now required to use an ObservableCollection instead of a List and must handle any changes to the collection (if needed).
- [x] Added Table.Replace to TH_Database.IDatabasePlugin to remove the need to connect to a database twice when needing to Drop then Create a table. This was initially implemented to fix the issue of GenEventValues table not getting populated due to a connection error.

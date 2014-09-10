newrelic_perfmon_plugin
=======================

_New Relic Windows Perfmon Plugin_

This is an executable/Windows service to push Windows Perfmon data to the [New Relic Platform](http://newrelic.com/platform "New Relic Platform"). 

### Requriements

* .NET >= 4.0
* New Relic account

### Configuration

Create two new text files in the .\config directory named `newrelic.json` and `plugin.json`. You can use the contents of the _template_ files as a starting point.

* **newrelic.json** - The only required field is your New Relic license key which can be found on the Account Summary page. See [github.com/newrelic-platform/newrelic_dotnet_sdk](https://github.com/newrelic-platform/newrelic_dotnet_sdk#configuration-options) for more options.
* **plugin.json** - This is the list of servers and counters you want to monitor. The `name` field needs to be a network address/hostname accessible from the system this service is running on. This `name` will show up as the Instance name in [https://rpm.newrelic.com](https://rpm.newrelic.com).

#### Counters
The CIM interface is uses to collect to Perfmon data. You can use the following directions to find and build new entries for the `counterList` array in **plugin.json**.

Get a list of all counter categories:

```powershell
# Using Powershell 4.0
PS C:\> Get-CimClass Win32_PerfFormattedData* | Select CimClassName
```

Let's take `root/cimv2:Win32_PerfFormattedData_MSSQLSQLEXPRESS_MSSQLSQLEXPRESSBufferManager` for example.

* provider = "MSSQLSQLEXPRESS"
* category = "MSSQLSQLEXPRESSBufferManager"

The format is `Win32_PerfFormattedData_{provider}_{category}`.

Get a list of all counters for that category:
 
```powershell
PS C:\> Get-CimInstance "Win32_PerfFormattedData_MSSQLSQLEXPRESS_MSSQLSQLEXPRESSBufferManager"


Caption               :
Description           :
Name                  :
Frequency_Object      :
Frequency_PerfTime    :
Frequency_Sys100NS    :
Timestamp_Object      :
Timestamp_PerfTime    :
Timestamp_Sys100NS    :
AWElookupmapsPersec   : 0
AWEstolenmapsPersec   : 0
AWEunmapcallsPersec   : 0
AWEunmappagesPersec   : 0
AWEwritemapsPersec    : 0
Buffercachehitratio   : 100
CheckpointpagesPersec : 0
Databasepages         : 247
FreeliststallsPersec  : 0
Freepages             : 396
LazywritesPersec      : 0
Pagelifeexpectancy    : 251325
PagelookupsPersec     : 56
PagereadsPersec       : 0
PagewritesPersec      : 0
ReadaheadpagesPersec  : 0
Reservedpages         : 0
Stolenpages           : 893
Targetpages           : 84612
Totalpages            : 1536
PSComputerName        :
```

* counter = "Buffercachehitratio"
* unit = "% Cache hits" _Check the [Metric Units Reference](https://docs.newrelic.com/docs/plugins/plugin-developer-resources/developer-reference/metric-units-reference "Metric Units Reference") when selecting units._

Putting that all together, you would add the following line under `counterList` in **plugin.json**.

```javascript
{"provider": "MSSQLSQLEXPRESS", "category":"MSSQLSQLEXPRESSBufferManager", "counter":"Buffercachehitratio", "unit": "% cache hits"}
```

Optionally, you can include an `instance` property. You can see the following in the template.

```javascript
{"provider": "PerfOS", "category":"Processor", "instance":"_Total", "counter":"PercentProcessorTime", "unit": "% time"}
```
There is an instance of the counter for each logical processor. The __total_ instance represents the sum of all of them. 

If you run this, you'll see all of the instances and the `Name` property is the identifier.
```powershell
Get-CimInstance "Win32_PerfFormattedData_PerfOS_Processor"
```
If the counter has multiple instances and the instance property is not included in **plugin.json** all instances will be polled automatically.

#### Examples

Here are some example counterLists for some different applications.

* [IIS](https://github.com/cdhunt/newrelic_perfmon_plugin/blob/master/newrelic_perfmon_plugin/config/plugin.iis.json "IIS")
* [SocketLabs Mail Transfer Agent](https://github.com/cdhunt/newrelic_perfmon_plugin/blob/master/newrelic_perfmon_plugin/config/plugin.hurricaneserver.json "SocketLabs Hurricane MTA")

### Service Installation

`newrelic_perfmon_plugin.exe install` 

You will be prompted for credentials. The service will need to run under an account that has user access to all hosts referenced in **plugin.json**.

This executable is built using the [Topshelf](http://topshelf-project.com/ "Topshelf") library. 

New Relic Windows Service Monitor Plugin 
========================================

This is an executable/Windows service to push Windows Services data to the [New Relic Platform](http://newrelic.com/platform "New Relic Platform"). 

### Table of Contents

 * [Requirements](#requirements)
 * [Configuration](#configuration)
  * [Custom Counters](#custom-counters)
 * [Service Installation](#service-installation)

### Requirements

* .NET >= 4.5
* New Relic account

### Configuration

Create two new text files in the .\config directory named `newrelic.json` and `plugin.json`. You can use the contents of the _template_ files as a starting point.

* **newrelic.json** - The only required field is your New Relic license key which can be found on the Account Summary page. See [github.com/newrelic-platform/newrelic_dotnet_sdk](https://github.com/newrelic-platform/newrelic_dotnet_sdk#configuration-options) for more options.
* **plugin.json** - This is the list of servers and services you want to make sure are running. The `name` field needs to be a network address/hostname accessible from the system this service is running on. This `name` will show up as the Instance name in [https://rpm.newrelic.com](https://rpm.newrelic.com).

### Service Installation

`newrelic_servicemon_plugin.exe install` 

You will be prompted for credentials. The service will need to run under an account that has user access to all hosts referenced in **plugin.json**.

This executable is built using the [Topshelf](http://topshelf-project.com/ "Topshelf") library.
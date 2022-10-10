# RemoteService
Utility for Windows service start/stop.

Requires .NET 4.8.

## Usage

RemoteService.exe &lt;HOST&gt; &lt;ACTION&gt; &lt;SERVICENAME&gt; [USERNAME] [PASSWORD]

Parameter | Description
--- | ---
`HOST` | Address of host
`ACTION` | Action to perform, `START`or `STOP`
`SERVICENAME` | Name of Windows service
`USERNAME` | Alternate user name, current user will be used if not given
`PASSWORD` | Password, mandatory if `USERNAME` is used



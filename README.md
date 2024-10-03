# RemoteService

Utility for starting/stopping remote Windows services.

Authenticates to the $ADMIN share in order to make sure authentication is done
prior to trying to access the service. 

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

## Troubleshooting
For non-domain users, if the above didn't work even though you supplied the correct credentials for remote machine.
please try to run the following powershell commands on the remote machine and try again:
<pre>Get-NetConnectionProfile | Set-NetConnectionProfile -NetworkCategory Private
Enable-PSRemoting -Force
winrm quickconfig
winrm set winrm/config/service/auth '@{Basic="true"}'
winrm set winrm/config/service '@{AllowUnencrypted="true"}'
winrm set winrm/config/service/auth '@{CbtHardeningLevel="relaxed"}'
gpupdate /force</pre>

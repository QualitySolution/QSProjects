using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QS.ErrorReporting
{
    public class JournaldLogService : IAsyncLogService
    {
	    private readonly string systemdServiceName;

	    public JournaldLogService(string systemdServiceName) {
		    this.systemdServiceName = systemdServiceName ?? throw new ArgumentNullException(nameof(systemdServiceName));
	    }

	    public async Task<string> GetLogAsync(uint? rowCount = null) {
		    rowCount ??= 300; //Потому что журнал Systemd точно большой. Не ограничивать количество строк нельзя.
		    var info = new ProcessStartInfo
		    {
			    RedirectStandardOutput = true,
			    UseShellExecute = false,
			    FileName = "journalctl",
			    Arguments = $" -u {systemdServiceName} -n {rowCount.Value}"
		    };
		    var process = new Process { StartInfo = info };
		    process.Start();
            
		    var result = await process.StandardOutput.ReadToEndAsync();
		    process.WaitForExit();
            
		    return result;
        }
    }
}

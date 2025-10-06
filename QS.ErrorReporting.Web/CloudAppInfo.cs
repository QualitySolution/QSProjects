using System;
using QS.Project.Versioning;

namespace QS.ErrorReporting.Web
{
    public class CloudAppInfo : IApplicationInfo
    {
        public byte ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductTitle { get; }
        public string Modification { get; set; }
        public string ModificationTitle { get; }
        public bool ModificationIsHidden { get; }
        public string[] CompatibleModifications { get; }
        public Version Version { get; set; }
        public bool IsBeta { get; }
        public DateTime? BuildDate { get; }
    }
}

using System;

namespace QSProjectsLib
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyLogoIcon : Attribute 
	{
		public string ResourceName;
		public AssemblyLogoIcon() : this(string.Empty) {}
		public AssemblyLogoIcon(string resourceName) { ResourceName = resourceName; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblySupport : Attribute 
	{
		public string SupportInfo;
		public bool ShowTechnologyUsed;
		public string TechnologyUsed;
		public AssemblySupport() : this(string.Empty, true, string.Empty) {}
		public AssemblySupport(string supportInfo) : this(supportInfo, true, string.Empty) {}
		public AssemblySupport(string supportInfo, bool showTechnologyUsed) : this(supportInfo, showTechnologyUsed, string.Empty) {}
		public AssemblySupport(string supportInfo, bool showTechnologyUsed, string technologyUsed) 
		{ 
			SupportInfo = supportInfo;
			ShowTechnologyUsed = showTechnologyUsed;
			TechnologyUsed = technologyUsed;
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AssemblyAuthor : Attribute 
	{
		public string Name;
		public AssemblyAuthor(string name) { Name = name; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyEditionAttribute : Attribute 
	{
		public string Edition;
		public string[] AllowEdition;
		public AssemblyEditionAttribute(string edition) { Edition = edition; }
		public AssemblyEditionAttribute() { }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyAppWebsite : Attribute 
	{
		public string Link;
		public AssemblyAppWebsite(string link) { Link = link; }
	}

}

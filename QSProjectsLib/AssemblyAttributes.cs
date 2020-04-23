using System;

namespace QSProjectsLib
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyLogoIconAttribute : Attribute 
	{
		public string ResourceName;
		public AssemblyLogoIconAttribute() : this(string.Empty) {}
		public AssemblyLogoIconAttribute(string resourceName) { ResourceName = resourceName; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblySupportAttribute : Attribute 
	{
		public string SupportInfo;
		public bool ShowTechnologyUsed;
		public string TechnologyUsed;
		public AssemblySupportAttribute() : this(string.Empty, true, string.Empty) {}
		public AssemblySupportAttribute(string supportInfo) : this(supportInfo, true, string.Empty) {}
		public AssemblySupportAttribute(string supportInfo, bool showTechnologyUsed) : this(supportInfo, showTechnologyUsed, string.Empty) {}
		public AssemblySupportAttribute(string supportInfo, bool showTechnologyUsed, string technologyUsed) 
		{ 
			SupportInfo = supportInfo;
			ShowTechnologyUsed = showTechnologyUsed;
			TechnologyUsed = technologyUsed;
		}
	}

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AssemblyAuthorAttribute : Attribute 
	{
		public string Name;
		public AssemblyAuthorAttribute(string name) { Name = name; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyEditionAttribute : Attribute 
	{
		public string Edition;
		public string Title;
		public string[] AllowEdition;
		public AssemblyEditionAttribute(string edition) { Edition = edition; }
		public AssemblyEditionAttribute() { }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyAppWebsiteAttribute : Attribute 
	{
		public string Link;
		public AssemblyAppWebsiteAttribute(string link) { Link = link; }
	}

	[AttributeUsage(AttributeTargets.Assembly)]
	public class AssemblyBetaBuildAttribute : Attribute 
	{
		
	}
}

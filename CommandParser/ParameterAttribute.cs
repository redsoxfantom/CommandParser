using System;

namespace CommandParser
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ParameterAttribute : System.Attribute
	{
		public String[] Names{get;set;}

		public bool IsUnnamedParameter{get;set;}

		public ParameterAttribute(bool IsUnnamedParameter = false, params string[] Names)
		{
			this.Names = Names;
			this.IsUnnamedParameter = IsUnnamedParameter;
		}
	}
}


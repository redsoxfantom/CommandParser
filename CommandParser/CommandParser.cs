using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace CommandParser
{
	public class Parser
	{
		private Dictionary<Regex,Tuple<Type,Action<object>>> commands;

		public Parser (Dictionary<Regex,Tuple<Type,Action<object>>> commands)
		{
			this.commands = commands;
		}

		public bool Parse(string commandString)
		{
			string command = commandString.Split (' ') [0];
			foreach(var commandKVp in commands)
			{
				if(commandKVp.Key.IsMatch(command))
				{
					ProcessCommand (commandString, commandKVp.Value);
					return true;
				}
			}
			return false;
		}

		public void AddCommand(Regex commandRegex, Tuple<Type,Action<object>> command)
		{
			commands.Add (commandRegex, command);
		}

		private void ProcessCommand(string commandString, Tuple<Type,Action<object>> paramActionTuple)
		{
			var paramsList = commandString.Split (' ');
			var type = paramActionTuple.Item1;
			var action = paramActionTuple.Item2;
			var parameter = Activator.CreateInstance (type);

			for(int i = 1; i < paramsList.Count(); i++)
			{
				var param = paramsList [i];

				// Firse, check to see if the given parameter name directly matches any of the property names
				var prop = parameter.GetType ().GetProperty (param);
				if(prop != null)
				{
					object paramValue;
					if(prop.PropertyType == typeof(Boolean))
					{
						paramValue = true;
					}
					else
					{
						i++;
						paramValue = DetermineParamValue(paramsList [i],prop.PropertyType);
					}
					prop.SetValue (parameter, paramValue);
				}
				else
				{
					// Next, iterate over all the Parameter attributes to see if they match the given parameter name
					foreach(var potentialProp in parameter.GetType().GetProperties())
					{
						foreach(var attr in potentialProp.GetCustomAttributes(true))
						{
							if(attr.GetType() == typeof(ParameterAttribute))
							{
								ParameterAttribute paramAttr = (ParameterAttribute)attr;
								if(paramAttr.Names.Contains(param))
								{
									object paramValue;
									if(potentialProp.PropertyType == typeof(Boolean))
									{
										paramValue = true;
									}
									else
									{
										i++;
										paramValue = DetermineParamValue(paramsList [i],potentialProp.PropertyType);
									}
									potentialProp.SetValue (parameter, paramValue);
								}
							}
						}
					}
				}
			}

			// Finally, check if any propery is an "Unnamed parameter". These are the last parameters in the list 
			// (therefore, there can only be one per parameter object)
			// For example, "cat /path/to/file". /path/to/file is an unnamed parameter. 
			foreach(var potentialProp in parameter.GetType().GetProperties())
			{
				foreach(var attr in potentialProp.GetCustomAttributes(true))
				{
					if(attr.GetType() == typeof(ParameterAttribute))
					{
						ParameterAttribute paramAttr = (ParameterAttribute)attr;
						if(paramAttr.IsUnnamedParameter)
						{
							object paramValue;
							if(potentialProp.PropertyType == typeof(Boolean))
							{
								paramValue = true;
							}
							else
							{
								paramValue = DetermineParamValue(paramsList[paramsList.Length-1],potentialProp.PropertyType);
							}
							potentialProp.SetValue (parameter, paramValue);
						}
					}
				}
			}

			action (parameter);
		}

		private object DetermineParamValue(string paramFromCommand, Type paramType)
		{
			if(paramType == typeof(string))
			{
				return paramFromCommand;
			}
			if(paramType == typeof(int))
			{
				return int.Parse (paramFromCommand);
			}
			if(paramType == typeof(float))
			{
				return float.Parse (paramFromCommand);
			}

			throw new ArgumentException (String.Format ("Parameter type {0} cannot be processed", paramType.Name));
		}
	}
}


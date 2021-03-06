# CommandParser
## What Is This?
This library is used to process simple text strings to execute commands and pass parameters, similiarly to how the shell behind a terminal functions. Games with a primarily text-based interface can use this to handle string parsing so you just have to worry about the logic behind the commands.

## How To Use It
This libraries interface is very minimalistic, you should be able to integrate it into your project with minimal changes. You can use the CommandParserTests.cs file as a guide, but here's an explanation:

Everything in CommandParser is based around the "Parser" class, which takes in the constructor a dictionary mapping a regular expression that matches the command the user will type in to a Tuple containing the Action that will be executed in response to that command and a C# object Type containing Properties that will be filled in by the parameters the user puts after the command. This dictionary can be modified after construction via the "AddCommand" method.

After creating and instance of the Parser class, call the "Parse" method passing the string that you want to check for commands. Under the hood, the parser strips off the first word of the string (in other words, performs a string.split(' ')[0]. Note this means that commands consisting of words with spaces between them are not possible). This method then checks to see if any regular expressions in its command dictionary match the stripped of word. As soon as it finds one, it creates and populates an instance of the C# parameter object associated with the matched Regex with any parameters passed in after the command word. It then calls the Action associated with the matched Regex, passing the parameter object. The method then returns "true" indicating a command was successfully matched. If no match was made, it returns "false".

I'll cover parameter parsing in more detail later, but for now lets have an
### Example

    using CommandParser
    
    // This is the Type that will be populated by the parameters passed in with the command.
    public class StringParam
    {
        public String Input{get;set;}
    }
    
    // Parameter types can also be empty (this could be used for commands that don't take any parameters)
    public class EmptyParam
    {
    
    }
    
    // Parameter Types can have multiple Properties. Currently, "int", "bool", "float", and "string" are supported
    public class MultiParam
    {
        public int IntVal {get;set;}
        public float FloatVal {get;set;}
    }
    
    // Boolean parameters are a bit special. See the rest of the example for the explanation
    public class BoolParam
    {
        public bool BoolParam {get;set;}
    }
    
    public class Program
    {
        public static void Main(String[] args)
        {
            var dict = new Dictionary<Regex,Tuple<Type,Action<Object>>>();
            dict.Add(new Regex("[Cc]ommand1"), // Will match either "Command1" or "command1" 
                     new Tuple<Type,Action<Object>>(typeof(StringParam),(param)=>
                     {
                         StringParam strParam = (StringParam)param;
                         Console.WriteLine(strParam.Input);
                     }));
            dict.Add(new Regex("[Cc]ommand2"), // Will match either "Command2" or "command2" 
                     new Tuple<Type,Action<Object>>(typeof(EmptyParam),(param)=>
                     {
                         // No parameters are passed to this Action (since we passed in an EmptyParam type)
                         Console.WriteLine("Command2 called");
                     }));
            dict.Add(new Regex("[Cc]ommand3"), // Will match either "Command3" or "command3" 
                     new Tuple<Type,Action<Object>>(typeof(MultiParam),(param)=>
                     {
                         MultiParam mParam = (MultiParam)param;
                         Console.WriteLine("Command3 called with IntParam "+mParam.IntVal+" and FloatVal "+mParam.FloatVal);
                     }));
            dict.Add(new Regex("[Cc]ommand34"), // Will match either "Command4" or "command4" 
                     new Tuple<Type,Action<Object>>(typeof(BoolParam),(param)=>
                     {
                         BoolParam mParam = (BoolParam)param;
                         Console.WriteLine(mParam.BoolParam);
                     }));
            
            Parser parser = new Parser(dict); // Create an instance of the Parser, passing in the dictionary containing the command Dictionary we made earlier
            
            parser.Parse("Command1 Input abcd"); // Will return "true" and print "abcd"
            
            parser.Parse("Command2 someParam someValue something"); // Will return "true" and print "Command2 called". 
                                                                    // Note that even though we passed other parameters in the string, 
                                                                    // since "Command2" uses an EmptyParam object defining no parameters the 
                                                                    // object passed to the Action will still be empty.
            
            parser.Parse("GarbledCommand"); // No action will be called and this will return "false".
            
            parser.Parse("Command3 FloatParam 15.4 IntParam 10"); // Will return "true" and print "Command3 called with IntParam 10 and FloatParam 15.4"
                                                                  // Note that this example shows that multiple parameters can be defined in any order.
            
            parser.Parse("command3 IntParam 15"); // Will return "true" and print "Command3 called with IntParam 15 and FloatParam 0"
                                                  // Note this example shows that parameters can be left out of a command. 
                                                  // If a parameter is not defined, the parameter object Property will take on it's default value
                                                  // Defaults: Int = 0, Float = 0, Bool = false, String = null
                                                  
            parser.Parse("command4"); // Will return "true" and print "false"
            parser.Parse("command4 BoolParam") // Will return "true" and print "true"
                                               // Boolean parameters don't read in the string after the parameter name
                                               // to decide what the value should be. If the parameter name is part of the string
                                               // passed in, then the parameter value will be "true". If not, then "false"
        }
    }
    
## Parameters
As shown in the example above, parameter objects consist of Properties with public Getters and Setters. In order to determine what the value of a particular Property in the parameter object should be, the Parser looks for words in the string matching the name of the Property. For example:

    public class ParamObj
    {
        public int Value {get;set;}
    }
    
    parser.Parse("command Value 10"); // Value will get matched
    parser.Parse("command value 10"); // Value will *not* get matched
    
It is also possible to use a Parameter attribute to define other "names" that a parameter can be matched to. For example:

    public class ParamObj
    {
        [Parameter(false,"-v","val","Val")] // Don't worry about the "false" in the beginning, I'll cover that later
        public int Value{get;set;}
    }
    
    parser.Parse("command -v 10"); // Value will get matched
    parser.Parse("command val 10"); // Value will get matched
    parser.Parse("command Val 10"): // Value will get matched
    parser.Parse("command Value 10"); // Value will get matched
    
Finally, a parameter can be defined as an "unnamed parameter". This means that the Parser will not try to look for a parameter name in the input string, it will simply take the last word in the string and assign that to the unnamed parameter. This is similiar to many common commands, such as "cat /path/to/file". For example:

    public class ParamObj
    {
        [Parameter(true)]
        public int Value{get;set;}
    }
    
    parser.Parse("command 10"); // Value will get matched 
    parser.Parse("command Value 10); // Value will still get matched
    
A few things to note:
- There can only be one unnamed parameter in a parameter object
- Only the last word in the input string will be assigned to the unnamed parameter

## Compiling
This was written using MonoDevelop 5.10 on Ubuntu 16.04. It will also compile under VisualStudio 2016, but will require the NUnit.Framework library to compile the unit tests.

## TODO
This is a *very* simple command parser implementation. As such, there are quite a few things that a real shell can do that this can't. Some of them could be implemented on the client side, but I'm working on adding the following:
- Parameters cannot be combined.
  - Many commands allow you to combine single-letter parameters. For example, instead of writing "tar -x -v -f" you can write "tar -xvf". This parser can't handle that currently, it must be handled by the client
- Parameter values cannot be multiple words long
  - Unlike a normal command shell, you can't just pass "command param 'some parameter value'" and get the value "some parameter value" for the "param" property. Instead, you will get "some". This cannot be handled by the client, it must be done by the Parser.
- There's no way to define parameters as "required". That is, if they are not supplied in the command string, the command should not be processed.
- See any other differences you want implemented? Let me know by adding an issue, or send a pull request!

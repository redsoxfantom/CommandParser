# CommandParser
## What Is This?
This library is used to process simple text strings to execute commands and pass parameters, similiarly to how the shell behind a terminal functions. Games with a primarily text-based interface can use this to handle string parsing so you just have to worry about the logic behind the commands.

## How To Use It
This libraries interface is very minimalistic, you should be able to integrate it into your project with minimal changes. You can use the CommandParserTests.cs file as a guide, but here's an explanation:

Everything in CommandParser is based around the "Parser" class, which takes in the construct a dictionary mapping a regular expression that matches the command the user will type in to a Tuple containing the Action that will be executed in response to that command and a C# object Type containing Properties that will be filled in by the parameters the user puts after the command. This dictionary can be modified after construction via the "AddCommand" method.

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
            
            Parser parser = new Parser(dict); // Create an instance of the Parser, passing in the dictionary containing the command Dictionary we made earlier
            
            parser.Parse("Command1 Input abcd"); // Will return "true" and print "abcd"
            
            parser.Parse("Command2 someParam someValue something"); // Will return "true" and print "Command2 called". 
                                                                    // Note that even though we passed other parameters in the string, 
                                                                    // since "Command2" uses an EmptyParam object defining no parameters the 
                                                                    // object passed to the Action will still be empty.
            
            parser.Parse("GarbledCommand"); // No action will be called and this will return "false".
            
            parser.Parse("Command3 IntParam 10 FloatParam 15.4"); // Will return "true" and print "Command3 called with IntParam 10 and FloatParam 15.4"
        }
    }

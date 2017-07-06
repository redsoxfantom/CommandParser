# CommandParser
## What Is This?
This library is used to process simple text strings to execute commands and pass parameters, similiarly to how the shell behind a terminal functions. Games with a primarily text-based interface can use this to handle string parsing so you just have to worry about the logic behind the commands.

## How To Use It
This libraries interface is very minimalistic, you should be able to integrate it into your project with minimal changes. You can use the CommandParserTests.cs file as a guide, but here's an explanation:

Everything in CommandParser is based around the "Parser" class, which takes in the construct a dictionary mapping a regular expression that matches the command the user will type in to a Tuple containing the Action that will be executed in response to that command and a C# object Type containing Properties that will be filled in by the parameters the user puts after the command.

# VSIX.TidyCSharp

A Visual Studio extension that adds a few enhancements to improve C# code development. It provides a list of automatic refactorig tools for cleaner C# code.

## How it works

1. Close all open windows
2. Right click on a C# file, folder, project or even the whole solution.
3. Select "Clean up C#..."
4. In the pop-up select the clean up actions you are interested in.
5. Click Run

### Supported auto-change rules

You can select any of the following supported rules to apply to all of your code in one go.
Some of the titles below are controversial and subjective. Feel free to exclude them if you don't like them. They are included in this tool because they reduce the code size, making it quicker to see the main elements of the code, which is why we like them.

- Remove and sort Usings
- Normalize white spaces 
  -No { and } for short blocks (single statement and < 70 chars)
  - Blank line between } and the next statement. 
  - Blank line between methods. 
  - Trim the file Space before comment text: //TODO: => // TODO: 
  - No blank lines immediately after { and immediately before } 
  - No duplicate blank lines between "Usings .." 
  - No duplicate blank lines between comments 
  - No duplicate blank lines between namespace members 
  - No duplicate blank lines between class members 
  - No duplicate blank lines between method statement 
- Remove unnecessary explicit 'private' where its the default 
  - Remove 'private' from properties 
  - Remove 'private' from methods 
  - Remove 'private' from fields 
  - Remove 'private' from nested classes 
- Small methods properties -> Expression bodied 
  - Convert ReadOnly Property => ReadOnly Property with only a single return statement and length less than 100 chars
  - Convert Methods => Method with only a single return statement and length less than 100 chars
- Simplify async calls
  - Remove unnecessary async / await pair (simply return the task).
- Convert traditional properties to auto-properties 
- Use 'var' for variable declarations
- Compact multiple class field declarations into one line 
  - Declare multiple class fields [with the same type] on the same line (if total size < 80 chars) 
  - Remove unnecessary explicit "=null" from class fields. 
  - Remove unnecessary explicit "=0" or "=false" from class fields.
- Remove unnecessary "this." 
  - Remove from property calls 
  - Remove from field calls 
  - Remove from method calls 
- Use camelCase for... 
  - Local variable declarations 
  - Method parameters 
- Class field and const casing... 
  - Const fields: USE_THIS_FORMAT 
  - Class fields: Change _something -> Something or something 
- Move constructors before methods 
- Remove unnecessary 'Attribute (e.g. [SomethingAttribute] -> [Something]) 
- Compact small if/else blocks 
- Use C# alias type names (e.g. System.Int32 -> int) 

It will then open each file, apply the clean up actions, save and close. It can take a more time for a larger code base. You can check out all of its changes in your source control tool diff tool.

## Build and running the code

### Prerequisites

1. Visual Studio with Visual Studio extensible tools and features installed.
2. GIT for Windows ([install from here](http://gitforwindows.org/))

## Contributing

As this solution is an opensource project, so any kind of contributions are welcome! Just fork the repo, do your changes then make a merge request. 
We'll review your code ASAP and we will do the merge if everything was just fine. If there was any kind of issues, post it at [issues](https://github.com/Geeksltd/VSIX.TidyCSharp/issues) section.

## Authors

This project is maintained and supported by the GeeksLTD.

See also the list of [contributors](https://github.com/Geeksltd/VSIX.TidyCSharp/contributors) who participated in this project.

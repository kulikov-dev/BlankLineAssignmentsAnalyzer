### BlankLineAssignmentsAnalizer
I ran into a problem that the organization introduced a rule to separate variable assignment blocks with blank lines. It's a nightmare to fix it by hand, so after short analysis I found out:
* ReSharper does not contain such kind of a rule and it is not possible to create it;
* StyleCop allows to write your own analyzer, but it is not supported in VisualStudio 2022.

As a solution, I wrote a small plug-in for Visual Studio 2022 that implements the check of this rule via built-in Roslyn analyzer (.NET Compiler Platofrm). The plugin highlights sections of code that don't match the rule and allows to auto-fix them (CodeFix).

Example:
  ``` csharp
//// Wrong
CallFirstFunc();
var a = "a";
var b = 2;
CallSomeFunc();
  ``` 
  
  ``` csharp
//// Correct
CallFirstFunc();

var a = "a";
var b = 2;
 
CallSomeFunc();
  ``` 

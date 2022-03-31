using HelloWorld.Core;
using Spectre.Console;

var greeter = new Greeter(
    args.FirstOrDefault() ?? AnsiConsole.Ask<string>("What's your [green]name[/]?")
);

AnsiConsole.Write(
    new FigletText(greeter.Greet())
        .Centered()
        .Color(Color.Red));
namespace HelloWorld.Core;
public record Greeter(string Name)
{
    public string Greet() => string.Concat("Hello ", Name);
}

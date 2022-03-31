using HelloWorld.Core;
using NUnit.Framework;

namespace HelloWorld.Tests;

public class GreeterTests
{
    [Test]
    public void ShouldGreet()
    {
        //Given
        const string expect = "Hello World";

        var greeter = new Greeter("World");

        //When
        var result = greeter.Greet();

        //Then
        Assert.AreEqual(expect, greeter.Greet());
    }
}
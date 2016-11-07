using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloRepo;
using Xunit;


public class HelloRepoTests
{
    [Fact]
    public void HelloRepoDoesSomething()
    {
        var c1 = new Class1();
        c1.DoSomething();

    }
}
﻿using NUnit.Framework;
using StyletIoC;
using System;

namespace StyletUnitTests.StyletIoC.StyletIoC;

[TestFixture]
public class StyletIoCInstanceBindingTests
{
    private interface I1 { }

    private class C1 : I1 { }

    private class C2 : IDisposable
    {
        public bool Disposed;
        public void Dispose()
        {
            this.Disposed = true;
        }
    }

    [Test]
    public void InstanceBindingUsesInstanceToResolve()
    {
        var c1 = new C1();

        var builder = new StyletIoCBuilder();
        builder.Bind<I1>().ToInstance(c1);
        IContainer ioc = builder.BuildContainer();

        Assert.AreEqual(c1, ioc.Get<I1>());
        Assert.AreEqual(c1, ioc.Get<I1>());
    }

    [Test]
    public void ContainerDisposesOfInstanceBindingIfRequested()
    {
        var c2 = new C2();

        var builder = new StyletIoCBuilder();
        builder.Bind<C2>().ToInstance(c2);
        IContainer ioc = builder.BuildContainer();

        ioc.Dispose();
        Assert.True(c2.Disposed);
    }

    [Test]
    public void ContainerDoesNotDisposeOfInstanceBindingIfNotRequested()
    {
        var c2 = new C2();

        var builder = new StyletIoCBuilder();
        builder.Bind<C2>().ToInstance(c2).DisposeWithContainer(false);
        IContainer ioc = builder.BuildContainer();

        ioc.Dispose();
        Assert.False(c2.Disposed);
    }
}

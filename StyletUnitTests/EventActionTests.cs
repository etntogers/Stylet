﻿using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StyletUnitTests
{
    [TestFixture]
    public class EventActionTests
    {
        private class Subject : DependencyObject
        {
            public event EventHandler SimpleEventHandler;
            public event Action BadEventHandler;
        }

        private class Target
        {
            public bool DoSomethingCalled;
            public void DoSomething()
            {
                this.DoSomethingCalled = true;
            }

            public void DoSomethingWithBadArgument(string arg)
            {
            }

            public void DoSomethingWithSenderAndBadArgument(object sender, object e)
            {
            }

            public void DoSomethingWithTooManyArguments(object sender, EventArgs e, object another)
            {
            }

            public EventArgs EventArgs;
            public void DoSomethingWithEventArgs(EventArgs ea)
            {
                this.EventArgs = ea;
            }

            public object Sender;
            public void DoSomethingWithObjectAndEventArgs(object sender, EventArgs e)
            {
                this.Sender = sender;
                this.EventArgs = e;
            }

            public void DoSomethingUnsuccessfully()
            {
                throw new InvalidOperationException("foo");
            }
        }

        private class Target2
        {
        }

        private DependencyObject subject;
        private Target target;
        private EventInfo eventInfo;

        [SetUp]
        public void SetUp()
        {
            this.target = new Target();
            this.subject = new Subject();
            this.eventInfo = typeof(Subject).GetEvent("SimpleEventHandler");
            View.SetActionTarget(this.subject, this.target);
        }

        [Test]
        public void ThrowsIfNullTargetBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfNonExistentActionBehaviourIsDisable()
        {
            Assert.Throws<ArgumentException>(() => new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Disable));
        }

        [Test]
        public void ThrowsIfTargetNullBehaviourIsThrowAndTargetBecomesNull()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Enable);
            Assert.Throws<ActionTargetNullException>(() => View.SetActionTarget(this.subject, null));
        }

        [Test]
        public void ThrowsIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Throw);
            Assert.Throws<ActionNotFoundException>(() => View.SetActionTarget(this.subject, new Target2()));
        }

        [Test]
        public void ThrowsIfMethodHasTooManyArguments()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithTooManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasBadParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithBadArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasBadEventArgsParameter()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithSenderAndBadArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void ThrowsIfMethodHasTooManyParameters()
        {
            Assert.Throws<ActionSignatureInvalidException>(() => new EventAction(this.subject, this.eventInfo, "DoSomethingWithTooManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }

        [Test]
        public void InvokingCommandDoesNothingIfTargetIsNull()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandDoesNothingIfActionIsNonExistent()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, new Target2());
            cmd.GetDelegate().DynamicInvoke(null, null);
        }

        [Test]
        public void InvokingCommandCallsMethod()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            cmd.GetDelegate().DynamicInvoke(null, null);
            Assert.True(this.target.DoSomethingCalled);
        }

        [Test]
        public void InvokingCommandCallsMethodWithEventArgs()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomethingWithEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = new RoutedEventArgs();
            cmd.GetDelegate().DynamicInvoke(null, arg);
            Assert.AreEqual(arg, this.target.EventArgs);
        }

        [Test]
        public void InvokingCommandCallsMethodWithSenderAndEventArgs()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomethingWithObjectAndEventArgs", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var sender = new object();
            var arg = new RoutedEventArgs();
            cmd.GetDelegate().DynamicInvoke(sender, arg);

            Assert.AreEqual(sender, this.target.Sender);
            Assert.AreEqual(arg, this.target.EventArgs);
        }

        [Test]
        public void BadEventHandlerSignatureThrows()
        {
            var cmd = new EventAction(this.subject, typeof(Subject).GetEvent("BadEventHandler"), "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            Assert.Throws<ActionEventSignatureInvalidException>(() => cmd.GetDelegate());
        }

        [Test]
        public void PropagatesActionException()
        {
            var cmd = new EventAction(this.subject, this.eventInfo, "DoSomethingUnsuccessfully", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var e = Assert.Throws<TargetInvocationException>(() => cmd.GetDelegate().DynamicInvoke(null, null));
            Assert.IsInstanceOf<InvalidOperationException>(e.InnerException);
            Assert.AreEqual("foo", e.InnerException.Message);
        }
    }
}
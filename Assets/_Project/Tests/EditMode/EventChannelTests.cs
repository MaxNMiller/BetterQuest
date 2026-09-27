using NUnit.Framework;
using UnityEngine;
using Spaa.Events;

namespace Spaa.Tests
{
    public class EventChannelTests
    {
        [Test]
        public void IntChannel_Raise_InvokesRegisteredListener()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();
            int received = -1;
            channel.RegisterListener(value => received = value);

            channel.Raise(42);

            Assert.AreEqual(42, received);
        }

        [Test]
        public void IntChannel_UnregisterListener_StopsReceivingEvents()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();
            int callCount = 0;
            void Listener(int value) => callCount++;

            channel.RegisterListener(Listener);
            channel.UnregisterListener(Listener);
            channel.Raise(1);

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void IntChannel_Raise_WithNoListeners_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<IntEventChannelSO>();

            Assert.DoesNotThrow(() => channel.Raise(1));
        }

        [Test]
        public void VoidChannel_Raise_InvokesRegisteredListener()
        {
            var channel = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            bool raised = false;
            channel.RegisterListener(() => raised = true);

            channel.Raise();

            Assert.IsTrue(raised);
        }

        [Test]
        public void VoidChannel_UnregisterListener_StopsReceivingEvents()
        {
            var channel = ScriptableObject.CreateInstance<VoidEventChannelSO>();
            int callCount = 0;
            void Listener() => callCount++;

            channel.RegisterListener(Listener);
            channel.UnregisterListener(Listener);
            channel.Raise();

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void VoidChannel_Raise_WithNoListeners_DoesNotThrow()
        {
            var channel = ScriptableObject.CreateInstance<VoidEventChannelSO>();

            Assert.DoesNotThrow(() => channel.Raise());
        }
    }
}

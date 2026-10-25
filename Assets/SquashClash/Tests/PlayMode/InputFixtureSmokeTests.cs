using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SquashClash.Runtime.Tests
{
    public sealed class InputFixtureSmokeTests : InputTestFixture
    {
        [Test]
        public void VirtualTouchscreen_DeliversEventWithTimestamp()
        {
            var screen = InputSystem.AddDevice<Touchscreen>();
            double observed = double.NaN;
            void Observe(InputEventPtr e, InputDevice device)
            {
                if (device == screen && e.IsA<StateEvent>()) observed = e.time;
            }
            InputSystem.onEvent += Observe;
            try
            {
                InputSystem.QueueStateEvent(screen, new TouchState
                {
                    touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began,
                    position = new Vector2(123, 234)
                }, 123.456);
                InputSystem.Update();
                Assert.That(observed, Is.EqualTo(123.456).Within(1e-9));
                Assert.That(screen.touches[0].touchId.ReadValue(), Is.EqualTo(1));
            }
            finally { InputSystem.onEvent -= Observe; }
        }
    }
}

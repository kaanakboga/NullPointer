using System;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Tests.PlayMode
{
    internal sealed class FakeGameplayInputSource : IGameplayInputSource
    {
        public event Action InteractPressed;
        public event Action PausePressed;

        public Vector2 Move { get; set; }

        public void PressInteract()
        {
            InteractPressed?.Invoke();
        }

        public void PressPause()
        {
            PausePressed?.Invoke();
        }
    }
}

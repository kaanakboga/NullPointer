using System;
using UnityEngine;

namespace NullPointer.Input
{
    public interface IGameplayInputSource
    {
        event Action InteractPressed;
        event Action PausePressed;

        Vector2 Move { get; }
    }
}

using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace Message.Avalonia.Helpers;

/// <summary>
///     Provides helper methods for animating controls.
/// </summary>
internal static class ControlAnimationHelper
{
    /// <summary>
    ///     Animates a property of a control from one value to another over a specified duration.
    /// </summary>
    /// <typeparam name="T">The type of the property to animate.</typeparam>
    /// <param name="control">The control to animate.</param>
    /// <param name="property">The property to animate.</param>
    /// <param name="from">The starting value of the property.</param>
    /// <param name="to">The ending value of the property.</param>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="count">The number of times to repeat the animation. Default is 1.</param>
    public static void Animate<T>(
        this Animatable control,
        AvaloniaProperty property,
        T from,
        T to,
        TimeSpan duration,
        ulong count = 1
    )
    {
        new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(count),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter { Property = property, Value = from },
                    },
                    KeyTime = TimeSpan.FromSeconds(0),
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter { Property = property, Value = to },
                    },
                    KeyTime = duration,
                },
            },
        }.RunAsync(control);
    }
}

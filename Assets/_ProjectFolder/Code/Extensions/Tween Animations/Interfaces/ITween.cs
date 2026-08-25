using PrimeTween;

namespace UnityEngine.Animations
{
    public interface ITween
    {
        TweenSettings Settings { get; }
        bool IsEnabled { get; }

        void Play(bool value);
        void ForcePlay(bool value);
    }
}
using System;
namespace CarliSenseXR.Core
{
    public sealed class CarliHoldGate
    {
        public readonly float RequiredSeconds;
        public float Progress { get; private set; }
        public bool Confirmed { get; private set; }
        public CarliHoldGate(float requiredSeconds)
        {
            if (float.IsNaN(requiredSeconds) || float.IsInfinity(requiredSeconds) || requiredSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(requiredSeconds));
            RequiredSeconds = requiredSeconds;
        }
        public bool Tick(float delta, bool targetHovered, bool pinchHeld, bool trackingValid)
        {
            if (Confirmed) return false;
            if (!trackingValid || !targetHovered || !pinchHeld || float.IsNaN(delta) || float.IsInfinity(delta) || delta <= 0 || delta > 0.25f)
            { Reset(); return false; }
            Progress = Math.Min(1f, Progress + delta / RequiredSeconds);
            if (Progress < 1f) return false;
            Confirmed = true; return true;
        }
        public void Reset() { Progress = 0f; Confirmed = false; }
    }
}

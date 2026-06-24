using System;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Controls Dragon NaturallySpeaking's microphone via COM automation.
    /// Uses late-binding (dynamic) so no Dragon SDK reference is required at build time.
    /// Fails silently when Dragon is not installed or the COM object is unavailable.
    /// </summary>
    public sealed class DragonMicService
    {
        private object? _dragon;

        /// <summary>True when Dragon's COM object was found and is responding.</summary>
        public bool IsDragonAvailable { get; private set; }

        /// <summary>
        /// Attempts to connect to the Dragon COM automation object.
        /// Call once during app startup; safe to call even when Dragon is not installed.
        /// </summary>
        public void Initialize()
        {
            try
            {
                var t = Type.GetTypeFromProgID("NaturallySpeaking.Application");
                if (t == null) return;
                _dragon = Activator.CreateInstance(t);
                IsDragonAvailable = _dragon != null;
            }
            catch
            {
                // Dragon not installed, COM object not registered, or Dragon not running.
                IsDragonAvailable = false;
            }
        }

        /// <summary>
        /// Turns Dragon's microphone on or off.
        /// No-op when Dragon is unavailable.
        /// </summary>
        public void SetMicrophoneOn(bool on)
        {
            if (_dragon == null) return;
            try
            {
                // Dragon 15+ Professional Individual exposes a bool Microphone property.
                ((dynamic)_dragon).Microphone = on;
            }
            catch
            {
                // Property name mismatch or Dragon COM call failed — ignore.
            }
        }
    }
}

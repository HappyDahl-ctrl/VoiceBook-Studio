using System;

namespace VoiceBookStudio.Services
{
    /// <summary>
    /// Controls Dragon NaturallySpeaking's microphone via COM automation.
    /// Uses late-binding (dynamic) so no Dragon SDK reference is required at build time.
    /// Fails silently when Dragon is not installed or the COM object is unavailable.
    ///
    /// Dragon's automation surface does not expose a boolean "Microphone" property —
    /// mic state is a MicState property (IDgnMicBtn / IDgnMicrophone) taking one of the
    /// DgnMicStateConstants values below. A prior version of this class targeted a
    /// "NaturallySpeaking.Application.Microphone" property that doesn't exist on real
    /// Dragon installs, so SetMicrophoneOn silently no-opped on every real machine it
    /// ran on. Initialize() now tries the known ProgIDs for the mic-control object in
    /// order of likelihood; if Dragon's actual registered ProgID differs from all of
    /// these, it still degrades to the same safe no-op as before.
    /// </summary>
    public sealed class DragonMicService
    {
        // DgnMicStateConstants, from Dragon's COM type library.
        private const int DgnMicOff = 1;
        private const int DgnMicOn  = 2;

        // Tried in order; the first ProgID that resolves is used.
        private static readonly string[] MicObjectProgIds =
        {
            "DgnMicBtn.DgnMicBtn",
            "DgnMicBtn",
            "Dragon.DgnMicBtn",
        };

        private object? _dragon;

        /// <summary>True when Dragon's COM object was found and is responding.</summary>
        public bool IsDragonAvailable { get; private set; }

        /// <summary>
        /// Attempts to connect to the Dragon COM automation object.
        /// Call once during app startup; safe to call even when Dragon is not installed.
        /// </summary>
        public void Initialize()
        {
            foreach (string progId in MicObjectProgIds)
            {
                try
                {
                    var t = Type.GetTypeFromProgID(progId);
                    if (t == null) continue;
                    _dragon = Activator.CreateInstance(t);
                    if (_dragon != null)
                    {
                        IsDragonAvailable = true;
                        return;
                    }
                }
                catch
                {
                    // This ProgID isn't registered on this machine — try the next one.
                }
            }
            IsDragonAvailable = false;
        }

        /// <summary>
        /// Turns Dragon's microphone on or off.
        /// Returns true only if the COM call completed without throwing — callers must
        /// not tell the user Dragon's mic state changed unless this returns true, since
        /// a caught exception here means Dragon's mic is still in whatever state it was
        /// in before the call (e.g. still listening while the app claims it was muted).
        /// Returns false without attempting anything when Dragon is unavailable.
        /// </summary>
        public bool SetMicrophoneOn(bool on)
        {
            if (_dragon == null) return false;
            try
            {
                ((dynamic)_dragon).MicState = on ? DgnMicOn : DgnMicOff;
                return true;
            }
            catch
            {
                // Property name mismatch or Dragon COM call failed — report failure so
                // the caller doesn't announce a mic state change that didn't happen.
                return false;
            }
        }
    }
}

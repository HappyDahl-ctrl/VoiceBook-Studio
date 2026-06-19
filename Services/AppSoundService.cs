using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace VoiceBookStudio.Services
{
    // ----------------------------------------------------------------
    // Sound events — one entry per distinct notification type
    // ----------------------------------------------------------------

    public enum AppSound
    {
        // Lifecycle
        AppReady,            // warm rising chime — app loaded and waiting
        AppClosing,          // gentle falling chime — goodbye

        // Project
        ProjectOpened,       // ascending 3-note — book opened
        ProjectSaved,        // short decisive ping — saved
        AutoSaved,           // barely-there tick — silent background save

        // Chapters
        ChapterAdded,        // bright ding — something new created
        ChapterDeleted,      // low descending — cautionary
        ChapterMoved,        // short upward sweep — repositioned

        // AI
        AiResponded,         // two-note chime — response arrived
        AiError,             // soft low buzz — something went wrong

        // Actions
        TextInserted,        // brief soft pop — text placed
        ExportSuccess,       // 4-note ascending fanfare — done
        ExportError,         // descending buzz — failed

        // Voice commands
        CommandRecognized,   // near-silent tick — heard and handled
        CommandNotRecognized,// short low note — not understood

        // Tutorial
        TutorialStep,        // neutral ping — page turn
        TutorialComplete,    // celebratory 4-note — finished

        // Generic
        Error,               // soft error tone
        Success,             // two-note positive
    }

    /// <summary>
    /// Plays short synthesised audio cues for app events.
    /// All sounds are generated in memory from pure sine waves — no
    /// external audio files are required. Sounds play asynchronously
    /// and never block the UI thread.
    ///
    /// Sound complements JAWS: JAWS reads what happened in speech;
    /// sounds confirm the moment it happened with a distinct tone.
    /// Both play simultaneously through the Windows audio mixer.
    /// </summary>
    public sealed class AppSoundService : IDisposable
    {
        // Per-sound WAV cache — regenerated when Volume changes
        private readonly ConcurrentDictionary<AppSound, byte[]> _cache = new();
        private bool _disposed;
        private double _volume = 0.65;

        public bool IsEnabled { get; set; } = true;

        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0.0, 1.0);
                _cache.Clear(); // next Play() will regenerate at new volume
            }
        }

        public void Play(AppSound sound)
        {
            if (!IsEnabled || _disposed) return;
            byte[] wav = _cache.GetOrAdd(sound, s => GenerateWav(s, _volume));
            // Queue on the thread pool — PlaySound blocks its thread until done
            // but never blocks the UI or audio-feedback SAPI thread.
            ThreadPool.QueueUserWorkItem(_ => PlayWav(wav));
        }

        public void Dispose() => _disposed = true;

        // ----------------------------------------------------------------
        // Win32 PlaySound — plays a WAV buffer in memory, no file needed
        // ----------------------------------------------------------------

        [DllImport("winmm.dll")]
        private static extern bool PlaySound([In] byte[] pszSound, IntPtr hmod, uint fdwSound);

        private const uint SND_SYNC      = 0x0000;
        private const uint SND_MEMORY    = 0x0004;
        private const uint SND_NODEFAULT = 0x0002;

        private static void PlayWav(byte[] wav)
        {
            try
            {
                // SND_SYNC: blocks the calling (thread-pool) thread until done,
                // so the byte array is guaranteed alive for the full playback.
                PlaySound(wav, IntPtr.Zero, SND_MEMORY | SND_SYNC | SND_NODEFAULT);
            }
            catch { /* audio device unavailable — fail silently */ }
        }

        // ----------------------------------------------------------------
        // Sound definitions — sequences of (freqStart, freqEnd, duration, amp)
        // freqStart == freqEnd → pure tone; otherwise → frequency sweep
        // ----------------------------------------------------------------

        private readonly record struct Seg(double F0, double F1, double Dur, double Amp);

        private static Seg T(double freq, double dur, double amp = 0.28)
            => new(freq, freq, dur, amp);

        private static Seg S(double f0, double f1, double dur, double amp = 0.22)
            => new(f0, f1, dur, amp);

        private static Seg[] GetSegments(AppSound sound) => sound switch
        {
            // ── Lifecycle ──────────────────────────────────────────
            AppSound.AppReady =>
            [
                T(880,  0.11),       // A5 — first note
                T(1047, 0.22)        // C6 — warm landing
            ],
            AppSound.AppClosing =>
            [
                T(1047, 0.11),       // C6
                T(880,  0.22)        // A5 — fade out
            ],

            // ── Project ────────────────────────────────────────────
            AppSound.ProjectOpened =>
            [
                T(523, 0.07),        // C5
                T(659, 0.07),        // E5
                T(784, 0.22)         // G5 — book-open chord
            ],
            AppSound.ProjectSaved =>
            [
                T(880, 0.055, 0.38)  // A5 — short decisive snap
            ],
            AppSound.AutoSaved =>
            [
                T(880, 0.035, 0.06)  // A5 — barely audible tick
            ],

            // ── Chapters ───────────────────────────────────────────
            AppSound.ChapterAdded =>
            [
                T(1047, 0.19, 0.32)  // C6 — bright positive ding
            ],
            AppSound.ChapterDeleted =>
            [
                T(330, 0.10, 0.26),  // E4
                T(262, 0.19, 0.26)   // C4 — descending, cautionary
            ],
            AppSound.ChapterMoved =>
            [
                S(380, 700, 0.09, 0.24)  // upward whoosh
            ],

            // ── AI ─────────────────────────────────────────────────
            AppSound.AiResponded =>
            [
                T(659, 0.08),        // E5
                T(880, 0.22)         // A5 — "response ready" chime
            ],
            AppSound.AiError =>
            [
                T(150, 0.28, 0.30)   // sub-bass soft buzz
            ],

            // ── Actions ────────────────────────────────────────────
            AppSound.TextInserted =>
            [
                T(660, 0.030, 0.22)  // soft pop
            ],
            AppSound.ExportSuccess =>
            [
                T(523,  0.065, 0.26),  // C5
                T(659,  0.065, 0.26),  // E5
                T(784,  0.065, 0.26),  // G5
                T(1047, 0.22,  0.34)   // C6 — fanfare landing
            ],
            AppSound.ExportError =>
            [
                T(300, 0.12, 0.28),
                T(150, 0.18, 0.28)
            ],

            // ── Voice commands ─────────────────────────────────────
            AppSound.CommandRecognized =>
            [
                T(1200, 0.020, 0.16)  // very short high tick
            ],
            AppSound.CommandNotRecognized =>
            [
                T(220, 0.08, 0.22)    // short low note
            ],

            // ── Tutorial ───────────────────────────────────────────
            AppSound.TutorialStep =>
            [
                T(392, 0.07, 0.22)    // G4 — neutral page turn
            ],
            AppSound.TutorialComplete =>
            [
                T(523,  0.060, 0.28),  // C5
                T(659,  0.060, 0.28),  // E5
                T(784,  0.060, 0.28),  // G5
                T(1047, 0.22,  0.34)   // C6 — celebratory
            ],

            // ── Generic ────────────────────────────────────────────
            AppSound.Error =>
            [
                T(180, 0.20, 0.28)
            ],
            AppSound.Success =>
            [
                T(659, 0.08),
                T(880, 0.18)
            ],

            _ => [T(660, 0.08)]
        };

        // ----------------------------------------------------------------
        // PCM WAV synthesis
        // ----------------------------------------------------------------

        private const int SampleRate = 22050;

        private static byte[] GenerateWav(AppSound sound, double volume)
        {
            var segments = GetSegments(sound);

            using var ms     = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Reserve the 44-byte header; fill it in after we know the data size
            writer.Write(new byte[44]);

            int sampleCount = 0;

            foreach (var seg in segments)
            {
                int    n          = (int)(SampleRate * seg.Dur);
                double scaledAmp  = seg.Amp * volume;
                double phase      = 0.0;

                for (int i = 0; i < n; i++)
                {
                    // Linear interpolation of frequency (sweep support)
                    double t    = (double)i / n;
                    double freq = seg.F0 + (seg.F1 - seg.F0) * t;

                    // Envelope: 5ms linear attack then exponential decay
                    double attack  = Math.Min(1.0, i / (SampleRate * 0.005));
                    double decay   = Math.Exp(-5.0 * i / n);
                    double env     = attack * decay;

                    double sample = scaledAmp * env * Math.Sin(phase);
                    writer.Write((short)(Math.Clamp(sample, -1.0, 1.0) * 32767));
                    sampleCount++;

                    // Advance phase; wrap to prevent floating-point drift
                    phase += 2.0 * Math.PI * freq / SampleRate;
                    if (phase >= 2.0 * Math.PI) phase -= 2.0 * Math.PI;
                }
            }

            int dataBytes = sampleCount * 2; // 16-bit mono = 2 bytes/sample

            // Write RIFF/WAVE header at offset 0
            ms.Position = 0;
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataBytes);           // file size - 8
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);                       // fmt chunk size
            writer.Write((short)1);                 // PCM
            writer.Write((short)1);                 // mono
            writer.Write(SampleRate);
            writer.Write(SampleRate * 2);           // byte rate (1 ch × 2 B/sample)
            writer.Write((short)2);                 // block align
            writer.Write((short)16);                // bits per sample
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);

            return ms.ToArray();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace VoiceBookStudio.Helpers
{
    /// <summary>
    /// Raises UIA notification events so JAWS and other screen readers receive
    /// live announcements from VoiceBook even when they occur outside of a focus change.
    ///
    /// RaiseNotificationEvent requires Windows 10 version 1703 or later.
    /// All errors are swallowed silently — UIA failures must never surface to the user.
    /// </summary>
    public static class UiaAnnouncer
    {
        /// <summary>
        /// Raises a UIA notification event on <paramref name="element"/>'s automation peer.
        /// JAWS will announce <paramref name="message"/> according to the urgency setting.
        /// Safe to call on the UI thread at any time; does nothing if a peer cannot be obtained.
        /// </summary>
        /// <param name="element">Any visible WPF element owned by the current window.</param>
        /// <param name="message">Plain-English text for JAWS to read aloud.</param>
        /// <param name="isUrgent">
        /// When true, uses ImportantMostRecent processing — JAWS interrupts whatever it is
        /// currently saying. When false, uses CurrentThenMostRecent — JAWS queues the
        /// announcement after its current utterance.
        /// </param>
        public static void Announce(UIElement element, string message, bool isUrgent = false)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            try
            {
                AutomationPeer? peer = UIElementAutomationPeer.FromElement(element)
                                   ?? new UIElementAutomationPeer(element);
                if (peer == null) return;

                peer.RaiseNotificationEvent(
                    isUrgent
                        ? AutomationNotificationKind.Other
                        : AutomationNotificationKind.ActionCompleted,
                    isUrgent
                        ? AutomationNotificationProcessing.ImportantMostRecent
                        : AutomationNotificationProcessing.CurrentThenMostRecent,
                    message,
                    Guid.NewGuid().ToString());
            }
            catch { }
        }
    }
}

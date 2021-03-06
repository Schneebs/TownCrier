﻿using System;
using System.Text;

namespace TownCrier
{
    class TimedTrigger
    {
        public int Minute;
        public Webhook Webhook;
        public string Message;
        public bool Enabled;

        // Timer-specific stuff
        System.Windows.Forms.Timer Timer;
        ulong LastFrameNum;
        ulong CurrentFrameNum;

        public TimedTrigger(int evt, Webhook webhook, string message, bool enabled)
        {
            try
            {
                Minute = evt;
                Webhook = webhook;
                Message = message;
                Enabled = enabled;

                // Create a new timer but don't set it up just yet
                // because timers can be created disabled when saved to settings
                Timer = new System.Windows.Forms.Timer
                {
                    Interval = Minute * 1000 * 60 // Interval is milliseconds
                };
                Timer.Tick += Timer_Tick;

                if (enabled)
                {
                    Enable();
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        public void Enable()
        {
            try
            {
                Enabled = true;
                Timer.Start();
                LastFrameNum = 0;
                CurrentFrameNum = 0;
                Globals.Host.Underlying.Hooks.RenderPreUI += new Decal.Interop.Core.IACHooksEvents_RenderPreUIEventHandler(hooks_RenderPreUI);
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        public void Disable()
        {
            try
            {
                Enabled = false;
                Timer.Stop();
                Globals.Host.Underlying.Hooks.RenderPreUI -= hooks_RenderPreUI;
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        public void Dispose()
        {
            try
            {
                Disable();
                Timer.Dispose();

            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        private void hooks_RenderPreUI()
        {
            CurrentFrameNum++;
        }

        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("TimedTrigger: Every ");
                sb.Append(Minute.ToString());
                sb.Append(" minute(s), the '");
                sb.Append(Webhook.Name);
                sb.Append("' webhook will trigger with format string '");
                sb.Append(Message);
                sb.Append("'. Currently ");
                sb.Append(Enabled ? "Enabled" : "Disabled");
                sb.Append(".");

                return sb.ToString();

            }
            catch (Exception ex)
            {
                Util.LogError(ex);

                return "Failed to print Timer";
            }
        }

        public string ToSetting()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("timedtrigger\t");
                sb.Append(Minute.ToString());
                sb.Append("\t");
                sb.Append(Webhook.Name);
                sb.Append("\t");
                sb.Append(Message);
                sb.Append("\t");
                sb.Append(Enabled);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Util.LogError(ex);

                return "Failed to print TimedTrigger.";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CurrentFrameNum == LastFrameNum)
                {
                    return;
                }

                Webhook.Send(new WebhookMessage(Message, ""));

                // Update frame counter so we'll know if we're behind next time
                LastFrameNum = CurrentFrameNum;
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }
    }
}

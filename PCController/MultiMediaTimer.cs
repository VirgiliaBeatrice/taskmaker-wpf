using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace PCController
{
    public partial class MMTimer : Component
    {
        [DllImport("winmm.dll", SetLastError = true)]
        static extern UInt32 timeSetEvent(UInt32 msDelay, UInt32 msResolution,
          TimerEventHandler handler, UIntPtr userCtx, UInt32 eventType);

        [DllImport("winmm.dll", SetLastError = true)]
        static extern UInt32 timeKillEvent(UInt32 timerEventId);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern UInt32 timeBeginPeriod(UInt32 uMilliseconds);
        //public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint timeEndPeriod(UInt32 uMilliseconds);

        const int TIMERR_NOERROR = 0;
        //---
        [DllImport("winmm.dll", SetLastError = true)]
        static extern UInt32 timeGetDevCaps(ref TimeCaps timeCaps, UInt32 sizeTimeCaps);

        [StructLayout(LayoutKind.Sequential)]
        public struct TimeCaps
        {
            public UInt32 wPeriodMin;
            public UInt32 wPeriodMax;
        };

        private delegate void TimerEventHandler(UInt32 id, UInt32 msg, UIntPtr userCtx,
          UIntPtr rsv1, UIntPtr rsv2);

        private UInt32 uTimerID;
        private bool iEnabled;
        private uint iInterval;
        private uint iResolution;
        TimerEventHandler TimerHandler;

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
            set
            {
                //値が異なる場合のみ設定
                if (iEnabled != value)
                {
                    iEnabled = value;
                    UpdateTimeEvent();
                }
            }
        }

        public uint Interval
        {
            get
            {
                return iInterval;
            }
            set
            {
                if (iResolution > value)
                {
                    iResolution = value;
                }

                //値が異なる場合のみ設定
                if (iInterval != value)
                {
                    iInterval = value;
                    UpdateTimeEvent();
                }
            }
        }

        public uint Resolution
        {
            get
            {
                return iResolution;
            }
            set
            {
                TimeCaps timeCaps = new TimeCaps();
                UInt32 DevCaps = timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps));
                if (value > timeCaps.wPeriodMax)
                {
                    iResolution = timeCaps.wPeriodMax;
                }
                if (value < timeCaps.wPeriodMin)
                {
                    iResolution = timeCaps.wPeriodMin;
                }
                if (value > iInterval)
                {
                    iResolution = iInterval;
                }

                //値が異なる場合のみ設定
                if (iResolution != value)
                {
                    iResolution = value;
                    UpdateTimeEvent();
                }
            }
        }
        public delegate void TimerDelegate(object sender);
        private TimerDelegate onTimer;
        public event TimerDelegate OnTimer
        {
            add
            {
                onTimer += value;
            }
            remove
            {
                onTimer -= value;
            }
        }

        public MMTimer()
        {
            InitializeComponent();
            initProc();
        }

        public MMTimer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            initProc();
        }

        private void initProc()
        {
            iInterval = 1000;
            iResolution = 1000;
            iEnabled = true;
            uTimerID = 0;

            TimerHandler = TimerProc;
            UpdateTimeEvent();
        }

        public void UpdateTimeEvent()
        {
            UInt32 msDelay;
            UInt32 msResolution;
            uint uKillTimer;
            uint uEndPeriod;
            UIntPtr UserCtx = UIntPtr.Zero;

            if (iEnabled == true && iInterval > 0)
            {
                if (uTimerID != 0)
                {
                    uKillTimer = timeKillEvent(uTimerID);
                }
                msDelay = (uint)iInterval;
                msResolution = (uint)iResolution;

                if (timeBeginPeriod(msResolution) == TIMERR_NOERROR)
                {
                    uTimerID = timeSetEvent(msDelay, msResolution, TimerHandler, UserCtx, 1);
                }
                else
                {
                    //
                }
            }
            else
            {
                if (uTimerID != 0)
                {
                    uKillTimer = timeKillEvent(uTimerID);

                    msResolution = (uint)iResolution;
                    uEndPeriod = timeEndPeriod(msResolution);
                }
            }
        }

        private void TimerProc(UInt32 id, UInt32 msg, UIntPtr userCtx, UIntPtr rsv1, UIntPtr rsv2)
        {
            onTimer?.Invoke(this);
        }
    }
}
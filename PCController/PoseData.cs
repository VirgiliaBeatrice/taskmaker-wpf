using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace PCController
{
    public class PoseData : IComparable
    {
        public int nMotor;
        public int[] values;
        protected int time_ = 0;
        public PoseData(int nM) {
            nMotor = nM;
            values = new int[nMotor];
        }
        public int Time
        {
            set { time_ = value; }
            get { return time_; }
        }
        public int CompareTo(object o)
        {
            return time_ - ((PoseData)o).time_;
        }
        public static PoseData operator +(PoseData a, PoseData b)
        {
            if (a == null || b == null) return null;
            PoseData rv = new PoseData(a.nMotor);
            for (int i = 0; i < rv.nMotor; i++)
            {
                rv.values[i] = a.values[i] + b.values[i];
                rv.time_ = a.time_;
            }
            return rv;
        }
        public static PoseData operator +(PoseData a, int [] b)
        {
            PoseData rv = new PoseData(a.nMotor);
            for (int i = 0; i < rv.nMotor; i++)
            {
                rv.values[i] = a.values[i] + b[i];
                rv.time_ = a.time_;
            }
            return rv;
        }
        public static implicit operator short[] (PoseData p)
        {
            short[] rv = new short[p.values.Length];
            for (int i = 0; i < rv.Length; ++i) {
                rv[i] = (short)p.values[i];
            }
            return rv;
        }
    }
}

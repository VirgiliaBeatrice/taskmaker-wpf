using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace PCController
{
    public class SDEC
    {
        public const int NBITS = 10;
        public const int ONE = 1 << NBITS;
        public const int MASK = ONE-1;
        public static double toDouble(int s)
        {
            double d = s;
            d /= (1 << NBITS);
            return d;
        }
        public static int ToLDEC(short s) { return s << (LDEC.NBITS - SDEC.NBITS);  }
    }
    public class LDEC
    {
        public const int NBITS = 16;
        public const int ONE = 1 << NBITS;
        public const int MASK = ONE - 1;
        public static double toDouble(int s)
        {
            double d = s;
            d /= (1 << NBITS);
            return d;
        }
        public static short ToSDEC(int l) { return (short)(l >> (LDEC.NBITS - SDEC.NBITS)); }
    }
    public class MotorLimit
    {
        public NumericUpDown udCalib, udMin, udMax;
        public Panel panel;
        public int Minimum
        {
            set { udCalib.Minimum = udMin.Value = value; }
            get { return (int)udMin.Value; }
        }
        public int Maximum
        {
            set { udCalib.Maximum = udMax.Value = value; }
            get { return (int)udMax.Value; }
        }
        public int Offset
        {
            set { udCalib.Value = value; }
            get { return (int)udCalib.Value; }
        }
        public MotorLimit() {
            Init();
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init(); }
        private void Init()
        {
            int width = 100;
            udMin = new NumericUpDown();
            udMax = new NumericUpDown();
            udCalib = new NumericUpDown();
            udMin.Width = width;
            udMax.Width = width;
            udCalib.Width = width;
            panel = new Panel();
            udMax.Minimum = udMin.Minimum = -300000;
            udMax.Maximum = udMin.Maximum = 300000;

            udCalib.Top = 0;
            udMax.Top = udCalib.Top + udCalib.Height;
            udMin.Top = udMax.Top + udMax.Height;
            udMax.TabStop = false;
            udMin.TabStop = false;

            Label la = new Label();
            la.Width = 30;
            la.Text = "Min:";
            la.Top = udMin.Top;
            panel.Controls.Add(la);
            udMin.Left = la.Width;
            panel.Controls.Add(udMin);

            la = new Label();
            la.Width = 30;
            la.Text = "Max:";
            la.Top = udMax.Top;
            panel.Controls.Add(la);
            udMax.Left = la.Width;
            panel.Controls.Add(udMax);

            la = new Label();
            la.Text = "Off:";
            la.Width = 30;
            la.Top = udCalib.Top;
            panel.Controls.Add(la);

            udCalib.Left = la.Width;
            panel.Controls.Add(udCalib);
            panel.Height = udMin.Height + udMax.Height + udCalib.Height;
            panel.Width = udMin.Width + la.Width;
        }
    }

    public class MotorPosition
    {
        public Panel panel;
        public NumericUpDown upDown;
        public TrackBar bar;
        public event EventHandler ValueChanged;
        const int WIDTH = 120;
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init(WIDTH); }
        public MotorPosition(int w = WIDTH) { Init(w);  }
        private void Init(int w)
        {
            upDown = new NumericUpDown();
            upDown.Width = w;
            bar = new TrackBar();
            bar.Top = upDown.Height;
            bar.Width = upDown.Width;
            panel = new Panel();
            panel.Width = upDown.Width;
            panel.Height = upDown.Height + bar.Height - 10;
            panel.Controls.Add(upDown);
            panel.Controls.Add(bar);
            upDown.ValueChanged += UpdateBar;
            bar.ValueChanged += UpdateUpDown;
            bar.Minimum = (int)upDown.Minimum;
            bar.Maximum = (int)upDown.Maximum;
            bar.TabStop = false;
            bar.LargeChange = 1000;
            upDown.Increment = 1000;
        }
        private void UpdateBar(object sender, EventArgs e)
        {
            bar.Value = (int)((NumericUpDown)sender).Value;
        }
        private void UpdateUpDown(object sender, EventArgs e)
        {
            upDown.Value = ((TrackBar)sender).Value;
            if (ValueChanged != null) ValueChanged(sender, e);
        }
        public int Maximum
        {
            set { upDown.Maximum = bar.Maximum = value; }
            get { return (int)upDown.Maximum; }
        }
        public int Minimum
        {
            set { upDown.Minimum = bar.Minimum = value;
                if (Maximum - Minimum > 100000)
                {
                    upDown.Increment = 1000;
                }
                else {
                    upDown.Increment = 100;
                }
            }
            get { return (int)upDown.Minimum; }
        }
        public int Value
        {
            set { upDown.Value = value; }
            get { return (int)upDown.Value; }
        }
        public int Width
        {
            set { upDown.Width = bar.Width = panel.Width = value;  }
            get { return panel.Width; }
        }
    }
    [DataContract]
    public class MotorTorque {
        public Panel panel;
        public NumericUpDown udMin;
        public NumericUpDown udMax;
        [DataMember]
        public int Maximum
        {
            set { udMax.Value = value; }
            get { return (int)udMax.Value; }
        }
        [DataMember]
        public int Minimum
        {
            set { udMin.Value = value; }
            get { return (int)udMin.Value; }
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init(); }
        public MotorTorque() { Init(); }
        private void Init(){
            int width = 100;
            udMin = new NumericUpDown();
            udMax = new NumericUpDown();
            udMin.Width = width;
            udMax.Width = width;
            panel = new Panel();
            udMax.Minimum = udMin.Minimum = -SDEC.ONE;
            udMax.Maximum = udMin.Maximum = SDEC.ONE;
            Minimum = -SDEC.ONE;
            Maximum = SDEC.ONE;

            udMin.Top = 0;
            Label la = new Label();
            la.Width = 30;
            la.Text = "Min:";
            la.Top = udMin.Top;
            udMin.Left = la.Width;
            panel.Controls.Add(udMin);
            panel.Controls.Add(la);

            udMax.Top = udMin.Top + udMin.Height;
            la = new Label();
            la.Width = 30;
            la.Text = "Max:";
            la.Top = udMax.Top;
            udMax.Left = la.Width;
            panel.Controls.Add(udMax);
            panel.Controls.Add(la);
            panel.Height = udMin.Height + udMax.Height;
            panel.Width = udMin.Width + la.Width;
        }
    }
    [DataContract]
    public class MotorPd
    {
        public Panel panel;
        public NumericUpDown udK;
        public NumericUpDown udB;
        public NumericUpDown udA;
        [DataMember]
        public int K
        {
            set { udK.Value = value; }
            get { return (int)udK.Value; }
        }
        [DataMember]
        public int B
        {
            set { udB.Value = value; }
            get { return (int)udB.Value; }
        }
        [DataMember]
        public int A
        {
            set { udA.Value = value; }
            get { return (int)udA.Value; }
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init(); }
        public MotorPd() { Init();  }
        public void SetNuibotDefault()
        {
            K = 1024;
            B = 1024 * 3 / 2;
            A = 1024 / 2;
        }
        public void SetSpidarDefault()
        {
            K = 100;
            B = 50;
            A = 1024 / 2;
        }
        private void Init(){
            int width = 100;
            udK = new NumericUpDown();
            udB = new NumericUpDown();
            udA = new NumericUpDown();
            udK.Width = width;
            udB.Width = width;
            udA.Width = width;
            panel = new Panel();
            udK.Minimum = udB.Minimum = -32000;
            udK.Maximum = udB.Maximum = 32000;
            udA.Minimum = -1024 * 8;
            udA.Maximum = 1024 * 8;
            SetNuibotDefault();
            udK.Top = 0;
            Label lK = new Label();
            lK.Margin = new Padding(0, 0, 0, 0);
            lK.Padding = new Padding(0, 0, 0, 0);
            lK.Width = 20;
            lK.Text = "K:";
            lK.Top = udK.Top;
            udK.Left = lK.Width;
            udB.Top = udK.Top + udK.Height;
            Label lB = new Label();
            lB.Margin = new Padding(0, 0, 0, 0);
            lB.Padding = new Padding(0, 0, 0, 0);
            lB.Width = 20;
            lB.Text = "B:";
            lB.Top = udB.Top;
            udB.Left = lB.Width;
            udA.Top = udB.Top + udB.Height;
            Label lA = new Label();
            lA.Margin = new Padding(0, 0, 0, 0);
            lA.Padding = new Padding(0, 0, 0, 0);
            lA.Width = 20;
            lA.Text = "A:";
            lA.Top = udA.Top;
            udA.Left = lA.Width;

            panel.Controls.Add(udA);
            panel.Controls.Add(lA);
            panel.Controls.Add(udB);
            panel.Controls.Add(lB);
            panel.Controls.Add(udK);
            panel.Controls.Add(lK);

            panel.Height = udK.Height + udB.Height + udA.Height;
            panel.Width = udK.Width + lA.Width;
        }
    }

    [DataContract]
    public class MotorHeat
    {
        public Panel panel;
        public NumericUpDown udHeatLimit;
        public NumericUpDown udHeatRelease;
        [DataMember]
        public long HeatLimit
        {
            set { udHeatLimit.Value = value; }
            get { return (long)udHeatLimit.Value; }
        }
        public short HeatRelease
        {
            set { udHeatRelease.Value = value; }
            get { return (short)udHeatRelease.Value; }
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init(); }
        public MotorHeat() { Init(); }
        private void Init()
        {
            int width = 100;
            udHeatLimit = new NumericUpDown();
            udHeatRelease = new NumericUpDown();
            udHeatRelease.Width = width;
            udHeatLimit.Width = width;
            panel = new Panel();
            udHeatLimit.Minimum = 0;
            udHeatLimit.Maximum = 30000 * LDEC.ONE;
            udHeatRelease.Minimum = 0;
            udHeatRelease.Maximum = 32000;
            HeatRelease = (int)(0.5 * SDEC.ONE);
            HeatLimit = 20 * 10 * SDEC.ToLDEC(HeatRelease);  //   (20 sec * 10Hz)

            udHeatRelease.Top = udHeatLimit.Top + udHeatLimit.Height;
            Label lHeatRelease = new Label();
            lHeatRelease.Margin = new Padding(0, 0, 0, 0);
            lHeatRelease.Padding = new Padding(0, 0, 0, 0);
            lHeatRelease.Width = 40;
            lHeatRelease.Text = "Release:";
            lHeatRelease.Top = udHeatRelease.Top;
            udHeatRelease.Left = lHeatRelease.Width;
            panel.Controls.Add(udHeatRelease);
            panel.Controls.Add(lHeatRelease);

            Label lHeatLimit = new Label();
            lHeatLimit.Margin = new Padding(0, 0, 0, 0);
            lHeatLimit.Padding = new Padding(0, 0, 0, 0);
            lHeatLimit.Width = 40;
            lHeatLimit.Text = "Limit:";
            lHeatLimit.Top = udHeatLimit.Top;
            udHeatLimit.Left = lHeatLimit.Width;
            panel.Controls.Add(udHeatLimit);
            panel.Controls.Add(lHeatLimit);
            udHeatRelease.Top = udHeatLimit.Top + udHeatLimit.Height;
            panel.Height = udHeatLimit.Height + udHeatRelease.Height;
            panel.Width = udHeatRelease.Width + lHeatRelease.Width;
        }
        public void SetNuibotDefault()
        {
            HeatLimit = 102400;
            HeatRelease = 512;
        }
        public void SetSpidarDefault()
        {
            HeatLimit = 102400;
            HeatRelease = 4096;
        }
    }

    [DataContract]
    public partial class Motor {
        public MotorPosition position;
        public MotorLimit limit;
        [DataMember]
        public MotorTorque torque;
        [DataMember]
        public MotorPd pd;
        [DataMember]
        public MotorHeat heat;
        public Motor()
        {
            Init();
        }
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context) { Init();  }
        private void Init() {
            position = new MotorPosition();
            limit = new MotorLimit();
            torque = new MotorTorque();
            pd = new MotorPd();
            heat = new MotorHeat();
            Minimum = -10000;
            Maximum = 30000;
            limit.udMax.ValueChanged += MaximumChanged;
            limit.udMin.ValueChanged += MinimumChanged;
        }
        void MaximumChanged(Object sender, EventArgs e)
        {
            Maximum = (int)((NumericUpDown)sender).Value;
        }
        void MinimumChanged(Object sender, EventArgs e)
        {
            Minimum = (int)((NumericUpDown)sender).Value;
        }

        [DataMember]
        public int Offset
        {
            set { limit.Offset = value; }
            get { return limit.Offset; }
        }
        [DataMember]
        public int Minimum
        {
            set { limit.Minimum = position.Minimum = value; }
            get { return limit.Minimum; }
        }
        [DataMember]
        public int Maximum
        {
            set { limit.Maximum = position.Maximum = value; }
            get { return limit.Maximum; }
        }
        public int Value
        {
            set { position.Value = value; }
            get { return position.Value; }
        }
    }
    public class Touch
    {
        public Int32 Value;

        public Int16 Threshold;

        public bool Flag;

        public Touch()
        {
            Value = 0;
            Threshold = 0;
            Flag = false;
        }
    }
    public partial class Motors : List<Motor> {
        public PoseData Offset() {
            PoseData rv = new PoseData(Count);
            for (int i = 0; i < rv.nMotor; ++i) {
                rv.values[i] = this.ElementAt(i).Offset;
            }
            return rv;
        }
    }
}

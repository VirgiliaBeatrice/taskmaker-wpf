using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace PCController
{
    public static class ControlExtensions
    {
        public static T Clone<T>(this T controlToClone)
            where T : Control
        {
            PropertyInfo[] controlProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            foreach (PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    if (propInfo.Name != "WindowTarget")
                        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
                }
            }
            return instance;
        }
    }

    public class Haptic
    {
        NumericUpDown udK, udB, udT, udM;
        public Label laCurrent;
        public short K
        {
            get { return (short)udK.Value; }
            set { udK.Value = value; }
        }
        public short B
        {
            get { return (short)udB.Value; }
            set { udB.Value = value; }
        }
        public short T
        {
            get { return (short)udT.Value; }
            set { udT.Value = value; }
        }
        public short M
        {
            get { return (short)udM.Value; }
            set { udM.Value = value; }
        }
        public Panel panel;
        public Haptic() {
            int width = 100;
            laCurrent = new Label();
            udK = new NumericUpDown();
            udB = new NumericUpDown();
            udT = new NumericUpDown();  //  threshold
            udM = new NumericUpDown();  //  minmumCurrent
            udK.Width = udB.Width = udT.Width = udM.Width = width;
            panel = new Panel();
            udK.Minimum = udB.Minimum = -30000;
            udK.Maximum = udB.Maximum = 30000;
            udT.Minimum = -300000;
            udT.Maximum = 300000;
            udK.Value = 300;
            udT.Value = -3000;
            udM.Value = 8;

            laCurrent.Top = 0;
            udK.Top = laCurrent.Height;
            udB.Top = udK.Top + udK.Height;
            udT.Top = udB.Top + udK.Height;
            udM.Top = udT.Top + udK.Height;

            Label la = new Label();
            la.BackColor = System.Drawing.Color.White;
            la.Margin = new Padding(0, 0, 0, 0);
            la.Padding = new Padding(0, 0, 0, 0);
            la.Width = 40;

            la.Text = "MinT:";
            la.Top = udM.Top;
            panel.Controls.Add(la.Clone());
            udM.Left = la.Width;
            panel.Controls.Add(udM);

            la.Text = "Len:";
            la.Top = udT.Top;
            panel.Controls.Add(la.Clone());
            udT.Left = la.Width;
            panel.Controls.Add(udT);

            la.Text = "B:";
            la.Top = udB.Top;
            panel.Controls.Add(la.Clone());
            udB.Left = la.Width;
            panel.Controls.Add(udB);

            la.Text = "K:";
            la.Top = udK.Top;
            panel.Controls.Add(la.Clone());
            udK.Left = la.Width;
            panel.Controls.Add(udK);

            panel.Controls.Add(laCurrent);

            panel.Height = laCurrent.Height + udK.Height + udB.Height + udT.Height + udM.Height;
            panel.Width = udK.Width + la.Width;
        }
    }
    public class Haptics : List<Haptic> {

        public short [] currents=null;
        new public void Add(Haptic h) {
            base.Add(h);
            currents = new short[base.Count];
        }
        public void Update()
        {
            for(int i=0; i<Count; ++i)
            {
                this[i].laCurrent.Text = "" + currents[i];
            }
        }
    }
}

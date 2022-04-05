using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace PCController
{
    public class Pose : PoseData
    {
        public new int Time
        {
            set
            {
                time_ = value;
                if (button.Parent != null)
                {
                    Control f = button.Parent;
                    TrackBar track = (TrackBar)f.Controls.Find("track", true)[0];
                    button.Left = (int)(Time * Scale + Offset) + 1;
                    button.Top = track.Top + (int)(track.Height * 0.8) - 5;
                }
            }
            get
            {
                return time_;
            }
        }

        //  時間とトラックバーの座標変換
        public double TrackScale()
        {
            if (button.Parent == null) return 1;
            Control f = button.Parent;
            TrackBar track = (TrackBar)f.Controls.Find("track", true)[0];
            return (double)(track.Width - 27) / (double)track.Maximum;
        }
        //  時間とトラックバーの座標変換
        public double TrackOffset()
        {
            if (button.Parent == null) return 13;
            Control f = button.Parent;
            TrackBar track = (TrackBar)f.Controls.Find("track", true)[0];
            return track.Left + 13;
        }
        public double Scale
        {
            get
            {
                return TrackScale();
            }
        }
        public double Offset
        {
            get
            {
                return TrackOffset() - button.Size.Width / 2.0;
            }
        }
        public Button button;
        public Pose(int nMotor) : base(nMotor)
        {
            button = new Button();
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 10;
            button.FlatAppearance.BorderColor = Color.Blue;
            button.Size = new Size(5, 8);
        }
    }
}

//#define RUNTICK_DEBUG

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;

namespace PCController
{
    public partial class PCMainForm : Form
    {
        List<Pose> poses = new List<Pose>();
        int curTime = 0;
        //  新しいPoseを作る。青いインジケータ(ボタン)も作る
        Pose NewPose()
        {
            Pose pose = new Pose(boards.NMotor);
            tpPos.Controls.Add(pose.button);
            pose.Time = track.Value;
            pose.button.BringToFront();
            pose.button.Click += pose_Click;
            pose.button.MouseDown += pose_MouseDown;
            pose.button.MouseMove += track_MouseMove;
            pose.button.MouseUp += track_MouseUp;
            return pose;
        }
        private void btCopy_Click(object sender, EventArgs e)
        {
            String str = "";
            for (int i = 0; i < boards.NMotor; ++i)
            {
                str += ((Motor)motors[i]).Value;
                if (i < boards.NMotor - 1) str += "\t";
            }
            Clipboard.SetDataObject(str);
        }
        private void add_Click(object sender, EventArgs e)
        {
            Pose find = null;
            foreach (Pose pose in poses)
            {
                if (pose.Time == track.Value) find = pose;
            }
            if (find == null)
            {
                int time = track.Value;
                Pose pose = NewPose();
                SaveFromEditor(pose);
                if (time != track.Value)
                {
                    pose.Time = time;
                    track.Value = time;
                }
                poses.Add(pose);
            }
            poses.Sort();
        }

        private void del_Click(object sender, EventArgs e)
        {
            Pose find = null;
            foreach (Pose pose in poses)
            {
                if (pose.Time == track.Value) find = pose;
            }
            if (find != null)
            {
                tpPos.Controls.Remove(find.button);
                poses.Remove(find);
                PoseData p = Interpolate(track.Value);
                if (p != null) LoadToEditor(p);
            }

        }
        private void pose_Click(object sender, EventArgs e)
        {
            Pose find = null;
            foreach (Pose pose in poses)
            {
                if (pose.button == sender) find = pose;
            }
            if (find != null) track.Value = find.Time;
        }
        int dragX, dragTime;
        Pose dragPose;
        private void pose_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Pose find = null;
            foreach (Pose pose in poses)
            {
                if (pose.button == sender) find = pose;
            }
            if (find != null)
            {
                track.Value = find.Time;
                dragPose = find;
                dragX = Cursor.Position.X;
                dragTime = find.Time;
            }
        }
        private void track_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) dragPose = null;
            if (dragPose == null) return;
            int time = dragTime + (int)((Cursor.Position.X - dragX) / dragPose.Scale);
            if (time < 0) time = 0; if (time > track.Maximum) time = track.Maximum;
            dragPose.Time = time;
        }
        private void track_MouseUp(object sender, MouseEventArgs e)
        {
            if (dragPose == null) return;
            int time = dragTime + (int)((Cursor.Position.X - dragX) / dragPose.Scale);
            if (time < 0) time = 0; if (time > track.Maximum) time = track.Maximum;
            dragPose.Time = time;
            dragPose = null;
            poses.Sort();
        }

        private void track_ValueChanged(object sender, EventArgs e)
        {
            Pose pose = null;
            foreach (Pose p in poses)
            {
                if (p.Time == track.Value)
                {
                    pose = p;
                }
            }
            if (pose != null)
            {
                LoadToEditor(pose);
                boards.SendPosDirect(pose + motors.Offset());
            }
            else
            {
                PoseData p = Interpolate(track.Value);
                if (p != null)
                {
                    LoadToEditor(p);
                    boards.SendPosDirect(p + motors.Offset());
                }
            }
        }

        private void btLoad_Click(object sender, EventArgs e)
        {
            openPose.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            LoadMotion(openPose.FileName);
        }
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(savePose.FileName);
            file.Write(poses.Count); file.Write("\t");
            file.Write(boards.NMotor); file.Write("\t");
            file.Write(udLoopTime.Value); file.Write("\n");
            int lastTime = 0;
            for (int h = 0; h < poses.Count; ++h)
            {
                Pose pose = (Pose)poses[h];
                file.Write(pose.Time - lastTime);
                lastTime = pose.Time;
                for (int i = 0; i < boards.NMotor; ++i)
                {
                    file.Write("\t");
                    file.Write(pose.values[i]);
                }
                file.Write("\n");
            }
            file.Close();
        }
        public bool LoadMotion(String Filename)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(Filename);
            String[] cells;
            do
            {
                cells = file.ReadLine().Split('\t');
            } while (cells[0].IndexOf('#') != -1);

            int nPose = int.Parse(cells[0]);
            if (boards.Count > 0 && boards.NMotor != int.Parse(cells[1]))
            {
                SetTextMessage("Dofs of file data and connected system do not match.");
                return false;
            }
            else
            {
                boards.SetNMotor(int.Parse(cells[1]));
                ResetMotor();
            }
            udLoopTime.Value = int.Parse(cells[2]);
            foreach (Pose pose in poses)
            {
                Controls.Remove(pose.button);
            }
            poses.Clear();
            int lastTime = 0;
            while (true)
            {
                string line = file.ReadLine();
                if (line == null) break;
                if (line.IndexOf('#') != -1) continue;
                string[] cells2 = line.Split('\t');
                if (cells2.Length < boards.NMotor + 1) break;
                Pose pose = NewPose();
                lastTime += int.Parse(cells2[0]);
                pose.Time = lastTime;
                for (int i = 0; i < boards.NMotor; ++i)
                {
                    pose.values[i] = int.Parse(cells2[i + 1]);
                }
                poses.Add(pose);
            }
            file.Close();
            if (motors.Count != poses.Count) {
                
            }
            PoseData p = Interpolate(track.Value) + motors.Offset();
            if (p != null) LoadToEditor(p);

            return (true);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            savePose.ShowDialog();
        }

        private void udLoopTime_ValueChanged(object sender, EventArgs e)
        {
            track.Maximum = (int)udLoopTime.Value;
            udkeyTime.Maximum = udLoopTime.Value;
            foreach (Pose pose in poses)
            {
                pose.Time = pose.Time < track.Maximum ? pose.Time : track.Maximum;
            }
        }

        private void UpdateCurTime(int time, bool bNoSend = false)
        {
            curTime = time;
            if (curTime > udLoopTime.Value)
            {
                curTime = 0;
                if (ckRunOnce.Checked == true) //モーションを一度だけ実行する場合
                {
                    ckRun.Checked = false;
                    boards.SendPosDirect(Interpolate(curTime) + motors.Offset());
                }
            }
            if (poses.Count > 0) {
                Pose pose = poses[0];
                laCurTime.Left = (int)(curTime * pose.TrackScale() + pose.TrackOffset() + laCurTime.Width / 2);
            }
            lbCurTime.Text = curTime.ToString();
        }
        //  motorsにposeの値をロード
        bool SaveFromEditorGuard;
        void LoadToEditor(PoseData pose)
        {
            SaveFromEditorGuard = true;
            udkeyTime.Value = pose.Time;
            for (int i = 0; i < boards.NMotor; ++i)
            {
                Motor m = (Motor)motors[i];
                int val = (int)pose.values[i];
                if (m.Maximum < val) val = m.Maximum;
                if (m.Minimum > val) val = m.Minimum;
                m.Value = val;
            }
            SaveFromEditorGuard = false;
        }
        void SaveFromEditor(Pose pose)
        {
            if (SaveFromEditorGuard) return;
            bool changeTime = pose.Time == track.Value;
            pose.Time = (int)udkeyTime.Value;
            for (int i = 0; i < boards.NMotor; ++i)
            {
                pose.values[i] = ((Motor)motors[i]).Value;
            }
            if (changeTime) track.Value = pose.Time;
        }
        private void GetEditedValue(object sender, EventArgs e)
        {
            Pose pose = null;
            foreach (Pose p in poses)
            {
                if (p.Time == track.Value) pose = p;
            }
            if (pose == null)
            {
                if (SaveFromEditorGuard) return;
                SaveFromEditorGuard = true;
                pose = NewPose();
                udkeyTime.Value = track.Value;
                SaveFromEditorGuard = false;
                SaveFromEditor(pose);
                poses.Add(pose);
            }
            else
            {
                SaveFromEditor(pose);
            }
            poses.Sort();
            if (!ckRun.Checked)
            {
                boards.SendPosDirect(pose + motors.Offset());
            }
        }

        //  Return interpolated pose of poses in the timeline + Offset.
        PoseData Interpolate(double time)
        {
            if (poses.Count < 2) return null;
            int i;
            for (i = 0; i < poses.Count; ++i)
            {
                Pose pose = (Pose)poses[i];
                if (pose.Time > time) break;
            }
            if (i == 0) i = poses.Count;
            Pose pose0 = (Pose)poses[i - 1];
            Pose pose1 = (Pose)poses[i % poses.Count];
            double dt = pose1.Time - pose0.Time;
            if (dt < 0)
            {
                dt += track.Maximum + 1;
                if (time < pose1.Time) time += track.Maximum + 1;
            }
            double rate = (time - pose0.Time) / dt;
            PoseData rv = new PoseData(boards.NMotor);
            for (int j = 0; j < boards.NMotor; ++j)
            {
                double val = (1 - rate) * (int)pose0.values[j] + rate * (int)pose1.values[j];
                rv.values[j] = (int)val;// + motors[j].Offset;
            }
            rv.Time = (int)time % track.Maximum;
            return rv;
        }
        private void UpdatePos()
        {
            if (ckRun.Checked)
            {
                UpdatePosInterpolate();
            }
            else {
                UpdatePosDirect();
            }
        }
        const int NINTERPOLATEFILL = 6;
        private void UpdatePosInterpolate() {
#if true   //  interpolate on motor drivers
            boards.SetControlMode(Boards.ControlMode.CM_INTERPOLATE);
            int remain = (int)(byte)((int)boards.InterpolateTargetCountWrite - (int)boards.InterpolateTargetCountRead);
            int vacancy = boards.NTarget - remain;
            int diff = NINTERPOLATEFILL - remain;
#if RUNTICK_DEBUG
            System.Diagnostics.Debug.Write("RunTimer: Remain=");
            System.Diagnostics.Debug.Write(remain);
            System.Diagnostics.Debug.Write("(");
            System.Diagnostics.Debug.Write(remain);
            System.Diagnostics.Debug.Write(") Cw=");
            System.Diagnostics.Debug.Write(boards.InterpolateTargetCountWrite);
            System.Diagnostics.Debug.Write(" Cr=");
            System.Diagnostics.Debug.Write(boards.InterpolateTargetCountRead);
            System.Diagnostics.Debug.Write(" tMin=");
            System.Diagnostics.Debug.Write(boards.TickMin);
            System.Diagnostics.Debug.Write(" tMax=");
            System.Diagnostics.Debug.Write(boards.TickMax);
            System.Diagnostics.Debug.Write(" vac=");
            System.Diagnostics.Debug.Write(vacancy);
            System.Diagnostics.Debug.Write("(");
            System.Diagnostics.Debug.Write(vacancy);
            System.Diagnostics.Debug.Write(") diff=");
            System.Diagnostics.Debug.WriteLine(diff);
#endif
            if (diff < 0)
            {
                System.Diagnostics.Debug.WriteLine("Interpolation targets error diff = " + diff);
                diff = 0;
            }
            if (vacancy < 2)
            {
                diff = 0;
            }
            //  send targets
            if (diff > 0)
            {
                //  Send targets
                for (int i = 0; i < diff; ++i)
                {
#if false              //  test for update
                    if (runTimer.Enabled)
                    {
                        int len = (int)udpComm.interpolateTargetCountWrite - (int)udpComm.interpolateTargetCountRead;
                        if (len > 3)
                        {
                            udpComm.interpolateTargetCountWrite--;
                            udpComm.SendPoseInterpolate(Interpolate(curTime), (ushort)runTimer.Interval);
                        }
                    }
#endif
                    curTime += timer.Interval * (int)udStep.Value;
                    UpdateCurTime(curTime, true);
                    if (ckRun.Checked)  //  onceの場合、UpdateCurTimeでckRunが切れる。
                    {
                        boards.SendPosInterpolate(Interpolate(curTime) + motors.Offset(), (ushort)(timer.Interval*3));
#if RUNTICK_DEBUG
                        System.Diagnostics.Debug.Write("cor:" + boards.InterpolateTargetCountRead);
                        System.Diagnostics.Debug.Write(" cow:" + boards.InterpolateTargetCountWrite);
                        System.Diagnostics.Debug.Write(" diff:" + diff);
                        System.Diagnostics.Debug.Write(" period:");
                        System.Diagnostics.Debug.Write((ushort)timer.Interval*3);
                        System.Diagnostics.Debug.Write(" tg:");
                        if (Interpolate(curTime) != null)
                        {
                            System.Diagnostics.Debug.Write(Interpolate(curTime).values[0]);
                        }
                        System.Diagnostics.Debug.WriteLine("");
#endif
                    }
                }   //  for
            }
            else
            {  
                //  Get current status of interpolation
                boards.SendPosInterpolate(Interpolate(curTime) + motors.Offset(), 0);
            }
#else   //  use direct
            UpdateCurTime(curTime += tmRun.Interval * (int)udStep.Value);
            udpComm.SendPoseDirect(Interpolate(curTime) + motors.Offset());
            udpComm.SendPackets();
#endif
        }
        private void UpdatePosDirect() {
            {
                short[] targets = new short[boards.NMotor];
                for (int i = 0; i < motors.Count; ++i)
                {
                    targets[i] = (short)motors[i].position.Value;
                }
                boards.SendPosDirect(targets);
            }
        }
        private void openPose_FileOk(object sender, CancelEventArgs e)
        {
            LoadMotion(openPose.FileName);
        }
        private void savePose_FileOk(object sender, CancelEventArgs e)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(savePose.FileName);
            file.Write(poses.Count); file.Write("\t");
            file.Write(boards.NMotor); file.Write("\t");
            file.Write(udLoopTime.Value); file.Write("\n");
            int lastTime = 0;
            for (int h = 0; h < poses.Count; ++h)
            {
                Pose pose = (Pose)poses[h];
                file.Write(pose.Time - lastTime);
                lastTime = pose.Time;
                for (int i = 0; i < boards.NMotor; ++i)
                {
                    file.Write("\t");
                    file.Write(pose.values[i]);
                }
                file.Write("\n");
            }
            file.Close();
        }
    }
}
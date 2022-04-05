//#define USE_THREAD
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Linq;


namespace PCController
{
    public delegate void SendMessageHandler(float[] config);

    public partial class PCMainForm : Form
    {
        Boards boards;
        Motors motors;
        Haptics haptics;
        private MMTimer mmTimer;
        public SendMessageHandler handler;


        bool bHaptic = false;
        int[] times = null;
        int count = 0;
        int count1000 = 0;
        void mmTimer_Tick(Object sender)   //  Haptic制御
        {
            count ++;
            if (count >= 1000) {
                count = 0;
                count1000++;
                System.Diagnostics.Debug.WriteLine("Tick called " + count1000 * 1000 + " times.");
            }
            short vib=0;
            short [] wave = { 1, 0, -1, 0 };
            if (bHaptic) {
                for (int i = 0; i < boards.NMotor; ++i) {
                    int diff = haptics[i].T - boards.GetPos(i);
                    if (diff > SDEC.ONE) diff = SDEC.ONE;
                    if (diff < 0)
                    {
                        diff = 0;
                        times[i] = 0;
                    }
                    else {
                        times[i] ++;
                        vib = (short)((short)udAmp.Value * wave[times[i] % 4]);
                        vib = (short)(vib * Math.Exp(-times[i] * (double)udDamp.Value));
                    }
                    short c = (short)(diff * haptics[i].K / SDEC.ONE);
                    if (c < haptics[i].M) c = haptics[i].M;
                    c += vib;
                    haptics.currents[i] = c;
                }
                boards.SendCurrent(haptics.currents);
            }   
        }
        public PCMainForm()
        {
            mmTimer = new MMTimer();
            mmTimer.Interval = 1;
            mmTimer.Resolution = 1;
            mmTimer.Enabled = false;
            mmTimer.OnTimer += mmTimer_Tick;

            System.Diagnostics.Debug.Assert(CommandId.CI_NCOMMAND <= CommandId.CI_NCOMMAND_MAX);
            InitializeComponent();
            boards = new Boards();
            boards.Serial = uartBin;
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            cmbPortBin.Items.AddRange(ports);
            if (cmbPortBin.Items.Count > 0)
            {
                cmbPortBin.Text = cmbPortBin.Items[0].ToString();
            }
            motors = new Motors();
            haptics = new Haptics();
            udLoopTime_ValueChanged(udLoopTime, null);
            ResetMagnet();
        }

        public void SetMotor(float[] motorParams) {
            for (int i = 0; i < motors.Count; ++i) {
                motors[i].position.Value = (int) motorParams[i];
            }
        }

        void SetTextMessage(string msg)
        {
            txMsg.Text = msg;
        }
        private void ResetPanels() {
            ResetHapticTab();
            ResetCurrentTab();
            ResetMotor();
            ResetMagnet();
        }
        private void ResetHapticTab() {
            flHaptic.Controls.Clear();
            haptics.Clear();
            for (int i = 0; i < boards.NMotor; ++i)
            {
                Haptic h = new Haptic();
                flHaptic.Controls.Add(h.panel);
                haptics.Add(h);
            }
        }
        List<CurrentControl> currentControls = new List<CurrentControl>();
        private void ResetCurrentTab()
        {
            flCurrent.Controls.Clear();
            currentControls.Clear();
            for (int i = 0; i < boards.NMotor; ++i) {
                CurrentControl cc = new CurrentControl();
                cc.Init();
                flCurrent.Controls.Add(cc.panel);
                currentControls.Add(cc);
            }
        }
         public void ResetMotor() {
            motors.Clear();
            flParam.Controls.Clear();
            flPos.Controls.Clear();
            for (int i = 0; i < boards.NMotor; ++i) {
                Motor m = new Motor();
                motors.Add(m);
                flPos.Controls.Add(m.position.panel);
                flParam.Controls.Add(m.pd.panel);
                flHeat.Controls.Add(m.heat.panel);
                m.position.ValueChanged += GetEditedValue;
            }
            short[] k = new short[boards.NMotor];
            short[] b = new short[boards.NMotor];
            short[] a = new short[boards.NMotor];
            short[] limit = new short[boards.NMotor];
            short[] release = new short[boards.NMotor];
            short[] torqueMin = new short[boards.NMotor];
            short[] torqueMax = new short[boards.NMotor];
            boards.RecvParamPd(ref k, ref b);
            boards.RecvParamCurrent(ref a);
            boards.RecvParamTorque(ref torqueMin, ref torqueMax);
            boards.RecvParamHeat(ref limit, ref release);
            for (int i = 0; i < boards.NMotor; ++i)
            {
                motors[i].pd.K = k[i];
                motors[i].pd.B = b[i];
                motors[i].pd.A = a[i];
                if (limit[i] > 32000) limit[i] = 32000;
                if (limit[i] < 0) limit[i] = 0;
                motors[i].heat.HeatLimit = limit[i] * release[i];
                motors[i].heat.HeatRelease = release[i];
                motors[i].torque.Minimum = torqueMin[i];
                motors[i].torque.Maximum = torqueMax[i];
            }
        }
        private void btListBoards_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            if (uartBin.IsOpen) uartBin.Close();
            if (cmbPortBin.Text.Length == 0) return;
            uartBin.PortName = cmbPortBin.Text;
            uartBin.BaudRate = 2000000;
            try
            {
                uartBin.Open();
            }
            catch
            {
                return;
            }
            if (uartBin.IsOpen)
            {
                trBoards.Nodes.Clear();
                boards.Clear();
                boards.EnumerateBoard();
                foreach (Board b in boards)
                {
                    TreeNode nb = trBoards.Nodes.Add("#" + b.boardId
                        + "M" + b.nMotor + "C" + b.nCurrent + "F" + b.nForce
                        );
                    nb.Nodes.Add("ID " + b.boardId);
                    nb.Nodes.Add("model " + b.modelNumber);
                    nb.Nodes.Add("nTarget " + b.nTarget);
                    nb.Nodes.Add("nMotor " + b.nMotor);
                    nb.Nodes.Add("nCurrent " + b.nCurrent);
                    nb.Nodes.Add("nForce " + b.nForce);
                }
                times = Enumerable.Repeat(0, boards.NMotor).ToArray();
                ResetPanels();
            }
        }
        private void UpdateCurrent()
        {
            short[] currents = new short[boards.NMotor];
            for (int i = 0; i < currentControls.Count; ++i)
            {
                currents[i] = (short)currentControls[i].udTargetCurrent.Value;
            }
            short[] curCurrents = boards.SendCurrent(currents);
            for (int i = 0; i < currentControls.Count; ++i)
            {
                currentControls[i].lbCurrent.Text = "" + curCurrents[i];
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
	        if (bHaptic) {
	            haptics.Update();
            }
            else if (tbControl.SelectedTab == tpPos)
			{
                UpdatePos();
            }
			else if (tbControl.SelectedTab == tpCurrent)
            {
                UpdateCurrent();
            }
			else if (tbControl.SelectedTab == tpMagnet) {
                UpdateMagnet();
            }
            txMsg.Text = "";
            for (int i=0; i< boards.NMotor; ++i)
            {
                txMsg.Text += boards.GetPos(i);
                txMsg.Text += " ";
            }
        }

        private void trBoards_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                int id;
                if (!int.TryParse(e.Label, out id)) {
                    if (!int.TryParse(e.Label.Substring(2), out id))
                    {
                        e.CancelEdit = true;
                    }
                }
                if (!e.CancelEdit)
                {
                    foreach (Board b in boards)
                    {
                        if (b.boardId == id)
                        {
                            e.CancelEdit = true;
                        }
                    }
                }
                if (!e.CancelEdit) {
                    int oid;
                    int.TryParse(e.Node.Text.Substring(2), out oid);
                    byte[] ids = new byte[boards.Count];
                    for (int i = 0; i < boards.Count; ++i) {
                        ids[i] = (byte)boards[i].boardId;
                        if (ids[i] == oid)
                        {
                            ids[i] = (byte)id;
                        }
                    }
                    boards.SendParamBoardId(ids);
                }
            }
        }

        private void trBoards_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node != null)
            {
                if (!e.Node.Text.Contains("ID"))
                {
                    e.CancelEdit = true;
                }
            }
        }

        private void udTick_ValueChanged(object sender, EventArgs e)
        {
            int tick = (int)udTick.Value;
            if (tick < 1) tick = 1;
            timer.Interval = tick;
        }

        private void btSendPd_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            short[] k = new short[boards.NMotor];
            short[] b = new short[boards.NMotor];
            short[] a = new short[boards.NMotor];
            for (int i = 0; i < motors.Count; ++i)
            {
                k[i] = (short)motors[i].pd.K;
                b[i] = (short)motors[i].pd.B;
                a[i] = (short)motors[i].pd.A;
            }
            boards.SendParamPd(k, b);
            boards.SendParamCurrent(a);
        }
        private void btRecvPd_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            short[] k = new short[boards.NMotor];
            short[] b = new short[boards.NMotor];
            short[] a = new short[boards.NMotor];
            boards.RecvParamPd(ref k, ref b);
            boards.RecvParamCurrent(ref a);
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].pd.K = k[i];
                motors[i].pd.B = b[i];
                motors[i].pd.A = a[i];
            }
        }
        private void btLoadNuibot_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].pd.SetNuibotDefault();
            }
        }
        private void btLoadSpidar_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].pd.SetSpidarDefault();
            }
        }
        private void btLoadNubotiHeat_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].heat.SetNuibotDefault();
            }
        }
        private void btLoadSpidarHeat_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].heat.SetSpidarDefault();
            }
        }

        private void btSendHeat_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            short[] heatLimit = new short[boards.NMotor];
            short[] heatRelease = new short[boards.NMotor];
            for (int i = 0; i < motors.Count; ++i)
            {
                heatRelease[i] = motors[i].heat.HeatRelease;
                heatLimit[i] = (short)(motors[i].heat.HeatLimit / heatRelease[i]);
            }
            boards.SendParamHeat(heatLimit, heatRelease);
        }

        private void btRecvHeat_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            short[] heatLimit = new short[boards.NMotor];
            short[] heatRelease = new short[boards.NMotor];
            boards.RecvParamHeat(ref heatLimit, ref heatRelease);
            for (int i = 0; i < motors.Count; ++i)
            {
                motors[i].heat.HeatRelease = heatRelease[i];
                motors[i].heat.HeatLimit = heatLimit[i] * heatRelease[i];
            }
        }

        private void HapticControl() {
            while (bHaptic)
            {
                mmTimer_Tick(this);
            }
        }

#if USE_THREAD
        Thread hapticThread;
#endif
        private void btHapticStart_Click(object sender, EventArgs e)
        {
            if (haptics == null || haptics.Count == 0) return;

            if (!bHaptic)
            {
                btHapticStart.Text = "Stop";
                bHaptic = true;
#if USE_THREAD
                mmTimer.Enabled = false;
                hapticThread = new Thread(new ThreadStart(HapticControl));
                hapticThread.Priority = ThreadPriority.Highest;
                hapticThread.Start();
#else //  MM timer
                mmTimer.Enabled = true;
#endif
            }
            else {
                btHapticStart.Text = "Start";
                bHaptic = false;
                mmTimer.Enabled = false;
            }
        }

        private void btReset_Click(object sender, EventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
            boards.SendResetMotor();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bHaptic) btHapticStart_Click(sender, e);
        }

        private void bt_SendTo_Click(object sender, EventArgs e) {
            float[] targets = new float[boards.NMotor];
            for (int i = 0; i < motors.Count; ++i) {
                targets[i] = motors[i].position.Value;
            }

            this.handler(targets);
        }
    }
    public class CurrentControl
    {
        public Panel panel;
        public NumericUpDown udTargetCurrent;
        public Label lbCurrent;
        public void Init()
        {
            panel = new Panel();
            lbCurrent = new Label();
            udTargetCurrent = new NumericUpDown();
            lbCurrent.Width = 80;
            lbCurrent.Height = 16;
            udTargetCurrent.Maximum = 1024 * 2;
            udTargetCurrent.Minimum = -1024 * 2;
            udTargetCurrent.Width = lbCurrent.Width;
            udTargetCurrent.Top = lbCurrent.Height;
            panel.Width = udTargetCurrent.Width;
            panel.Height = udTargetCurrent.Height + lbCurrent.Height;
            panel.Controls.Add(lbCurrent);
            panel.Controls.Add(udTargetCurrent);
        }
    }
}

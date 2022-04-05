namespace PCController
{
    partial class PCMainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.uartBin = new System.IO.Ports.SerialPort(this.components);
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.openPose = new System.Windows.Forms.OpenFileDialog();
            this.savePose = new System.Windows.Forms.SaveFileDialog();
            this.splitContainerHoriz = new System.Windows.Forms.SplitContainer();
            this.tbControl = new System.Windows.Forms.TabControl();
            this.tpMagnet = new System.Windows.Forms.TabPage();
            this.pnMagnet = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btMeasure = new System.Windows.Forms.Button();
            this.tpHaptic = new System.Windows.Forms.TabPage();
            this.flHaptic = new System.Windows.Forms.FlowLayoutPanel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btHapticStart = new System.Windows.Forms.Button();
            this.udAmp = new System.Windows.Forms.NumericUpDown();
            this.udDamp = new System.Windows.Forms.NumericUpDown();
            this.tpPos = new System.Windows.Forms.TabPage();
            this.flPos = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCopy = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btSave = new System.Windows.Forms.Button();
            this.btLoad = new System.Windows.Forms.Button();
            this.udkeyTime = new System.Windows.Forms.NumericUpDown();
            this.del = new System.Windows.Forms.Button();
            this.add = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.udTick = new System.Windows.Forms.NumericUpDown();
            this.udLoopTime = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.udStep = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.lbCurTime = new System.Windows.Forms.Label();
            this.ckRun = new System.Windows.Forms.CheckBox();
            this.ckRunOnce = new System.Windows.Forms.CheckBox();
            this.laCurTime = new System.Windows.Forms.Label();
            this.track = new System.Windows.Forms.TrackBar();
            this.tpCurrent = new System.Windows.Forms.TabPage();
            this.flCurrent = new System.Windows.Forms.FlowLayoutPanel();
            this.tpParam = new System.Windows.Forms.TabPage();
            this.flParam = new System.Windows.Forms.FlowLayoutPanel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btLoadSpidar = new System.Windows.Forms.Button();
            this.btLoadNuibot = new System.Windows.Forms.Button();
            this.btRecvPd = new System.Windows.Forms.Button();
            this.btSendPd = new System.Windows.Forms.Button();
            this.tpHeat = new System.Windows.Forms.TabPage();
            this.flHeat = new System.Windows.Forms.FlowLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btLoadSpidarHeat = new System.Windows.Forms.Button();
            this.btLoadNubotiHeat = new System.Windows.Forms.Button();
            this.btRecvHeat = new System.Windows.Forms.Button();
            this.btSendHeat = new System.Windows.Forms.Button();
            this.splitContainerTop = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btReset = new System.Windows.Forms.Button();
            this.cmbPortBin = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btListBoards = new System.Windows.Forms.Button();
            this.splitContainerBottom = new System.Windows.Forms.SplitContainer();
            this.trBoards = new System.Windows.Forms.TreeView();
            this.txMsg = new System.Windows.Forms.TextBox();
            this.bt_SendTo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerHoriz)).BeginInit();
            this.splitContainerHoriz.Panel1.SuspendLayout();
            this.splitContainerHoriz.Panel2.SuspendLayout();
            this.splitContainerHoriz.SuspendLayout();
            this.tbControl.SuspendLayout();
            this.tpMagnet.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tpHaptic.SuspendLayout();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAmp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDamp)).BeginInit();
            this.tpPos.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udkeyTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTick)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udLoopTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udStep)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.track)).BeginInit();
            this.tpCurrent.SuspendLayout();
            this.tpParam.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tpHeat.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).BeginInit();
            this.splitContainerTop.Panel1.SuspendLayout();
            this.splitContainerTop.Panel2.SuspendLayout();
            this.splitContainerTop.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).BeginInit();
            this.splitContainerBottom.Panel1.SuspendLayout();
            this.splitContainerBottom.Panel2.SuspendLayout();
            this.splitContainerBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // uartBin
            // 
            this.uartBin.BaudRate = 2000000;
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // openPose
            // 
            this.openPose.FileName = "pose.txt";
            this.openPose.Filter = "姿勢ファイル|*.txt|姿勢CSV|*.csv|すべてのファイル|*.*";
            this.openPose.FileOk += new System.ComponentModel.CancelEventHandler(this.openPose_FileOk);
            // 
            // savePose
            // 
            this.savePose.DefaultExt = "txt";
            this.savePose.FileName = "pose.txt";
            this.savePose.Filter = "姿勢ファイル|*.txt|姿勢ファイル(csv)|*.csv|すべてのファイル|*.*";
            this.savePose.FileOk += new System.ComponentModel.CancelEventHandler(this.savePose_FileOk);
            // 
            // splitContainerHoriz
            // 
            this.splitContainerHoriz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerHoriz.Location = new System.Drawing.Point(0, 0);
            this.splitContainerHoriz.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainerHoriz.Name = "splitContainerHoriz";
            // 
            // splitContainerHoriz.Panel1
            // 
            this.splitContainerHoriz.Panel1.Controls.Add(this.tbControl);
            // 
            // splitContainerHoriz.Panel2
            // 
            this.splitContainerHoriz.Panel2.Controls.Add(this.splitContainerTop);
            this.splitContainerHoriz.Size = new System.Drawing.Size(738, 514);
            this.splitContainerHoriz.SplitterDistance = 563;
            this.splitContainerHoriz.SplitterWidth = 3;
            this.splitContainerHoriz.TabIndex = 4;
            // 
            // tbControl
            // 
            this.tbControl.Controls.Add(this.tpMagnet);
            this.tbControl.Controls.Add(this.tpHaptic);
            this.tbControl.Controls.Add(this.tpPos);
            this.tbControl.Controls.Add(this.tpCurrent);
            this.tbControl.Controls.Add(this.tpParam);
            this.tbControl.Controls.Add(this.tpHeat);
            this.tbControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbControl.Location = new System.Drawing.Point(0, 0);
            this.tbControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tbControl.Name = "tbControl";
            this.tbControl.SelectedIndex = 0;
            this.tbControl.Size = new System.Drawing.Size(563, 514);
            this.tbControl.TabIndex = 5;
            // 
            // tpMagnet
            // 
            this.tpMagnet.BackColor = System.Drawing.SystemColors.Control;
            this.tpMagnet.Controls.Add(this.pnMagnet);
            this.tpMagnet.Controls.Add(this.panel3);
            this.tpMagnet.Location = new System.Drawing.Point(4, 22);
            this.tpMagnet.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpMagnet.Name = "tpMagnet";
            this.tpMagnet.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpMagnet.Size = new System.Drawing.Size(555, 488);
            this.tpMagnet.TabIndex = 5;
            this.tpMagnet.Text = "Magnet";
            // 
            // pnMagnet
            // 
            this.pnMagnet.AutoScroll = true;
            this.pnMagnet.AutoSize = true;
            this.pnMagnet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnMagnet.Location = new System.Drawing.Point(2, 31);
            this.pnMagnet.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.pnMagnet.Name = "pnMagnet";
            this.pnMagnet.Size = new System.Drawing.Size(551, 454);
            this.pnMagnet.TabIndex = 3;
            this.pnMagnet.Resize += new System.EventHandler(this.pnMagnet_Resize);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btMeasure);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(2, 3);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(551, 28);
            this.panel3.TabIndex = 2;
            // 
            // btMeasure
            // 
            this.btMeasure.Location = new System.Drawing.Point(2, 3);
            this.btMeasure.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btMeasure.Name = "btMeasure";
            this.btMeasure.Size = new System.Drawing.Size(56, 20);
            this.btMeasure.TabIndex = 0;
            this.btMeasure.Text = "Measure";
            this.btMeasure.UseVisualStyleBackColor = true;
            this.btMeasure.Click += new System.EventHandler(this.btMeasure_Click);
            // 
            // tpHaptic
            // 
            this.tpHaptic.Controls.Add(this.flHaptic);
            this.tpHaptic.Controls.Add(this.panel6);
            this.tpHaptic.Location = new System.Drawing.Point(4, 22);
            this.tpHaptic.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpHaptic.Name = "tpHaptic";
            this.tpHaptic.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpHaptic.Size = new System.Drawing.Size(555, 488);
            this.tpHaptic.TabIndex = 5;
            this.tpHaptic.Text = "Haptic";
            this.tpHaptic.UseVisualStyleBackColor = true;
            // 
            // flHaptic
            // 
            this.flHaptic.AutoScroll = true;
            this.flHaptic.BackColor = System.Drawing.SystemColors.Window;
            this.flHaptic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flHaptic.Location = new System.Drawing.Point(2, 36);
            this.flHaptic.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flHaptic.Name = "flHaptic";
            this.flHaptic.Size = new System.Drawing.Size(551, 449);
            this.flHaptic.TabIndex = 1;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label5);
            this.panel6.Controls.Add(this.label6);
            this.panel6.Controls.Add(this.btHapticStart);
            this.panel6.Controls.Add(this.udAmp);
            this.panel6.Controls.Add(this.udDamp);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(2, 3);
            this.panel6.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(551, 33);
            this.panel6.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 9);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Vibration Amp:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(151, 9);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Damp:";
            // 
            // btHapticStart
            // 
            this.btHapticStart.Location = new System.Drawing.Point(333, 2);
            this.btHapticStart.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btHapticStart.Name = "btHapticStart";
            this.btHapticStart.Size = new System.Drawing.Size(75, 29);
            this.btHapticStart.TabIndex = 2;
            this.btHapticStart.Text = "Start";
            this.btHapticStart.UseVisualStyleBackColor = true;
            this.btHapticStart.Click += new System.EventHandler(this.btHapticStart_Click);
            // 
            // udAmp
            // 
            this.udAmp.Location = new System.Drawing.Point(84, 7);
            this.udAmp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.udAmp.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.udAmp.Name = "udAmp";
            this.udAmp.Size = new System.Drawing.Size(58, 20);
            this.udAmp.TabIndex = 3;
            this.udAmp.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // udDamp
            // 
            this.udDamp.DecimalPlaces = 3;
            this.udDamp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.udDamp.Location = new System.Drawing.Point(190, 7);
            this.udDamp.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.udDamp.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udDamp.Name = "udDamp";
            this.udDamp.Size = new System.Drawing.Size(56, 20);
            this.udDamp.TabIndex = 4;
            this.udDamp.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // tpPos
            // 
            this.tpPos.Controls.Add(this.flPos);
            this.tpPos.Controls.Add(this.panel2);
            this.tpPos.Controls.Add(this.laCurTime);
            this.tpPos.Controls.Add(this.track);
            this.tpPos.Location = new System.Drawing.Point(4, 22);
            this.tpPos.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpPos.Name = "tpPos";
            this.tpPos.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpPos.Size = new System.Drawing.Size(555, 488);
            this.tpPos.TabIndex = 3;
            this.tpPos.Text = "Pos";
            this.tpPos.UseVisualStyleBackColor = true;
            // 
            // flPos
            // 
            this.flPos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flPos.Location = new System.Drawing.Point(2, 86);
            this.flPos.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flPos.Name = "flPos";
            this.flPos.Size = new System.Drawing.Size(551, 399);
            this.flPos.TabIndex = 98;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.bt_SendTo);
            this.panel2.Controls.Add(this.btCopy);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.btSave);
            this.panel2.Controls.Add(this.btLoad);
            this.panel2.Controls.Add(this.udkeyTime);
            this.panel2.Controls.Add(this.del);
            this.panel2.Controls.Add(this.add);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.udTick);
            this.panel2.Controls.Add(this.udLoopTime);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.udStep);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.lbCurTime);
            this.panel2.Controls.Add(this.ckRun);
            this.panel2.Controls.Add(this.ckRunOnce);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(2, 37);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(551, 49);
            this.panel2.TabIndex = 99;
            // 
            // btCopy
            // 
            this.btCopy.AutoSize = true;
            this.btCopy.Location = new System.Drawing.Point(206, 22);
            this.btCopy.Name = "btCopy";
            this.btCopy.Size = new System.Drawing.Size(42, 23);
            this.btCopy.TabIndex = 113;
            this.btCopy.Text = "&Clip";
            this.btCopy.UseVisualStyleBackColor = true;
            this.btCopy.Click += new System.EventHandler(this.btCopy_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 112;
            this.label4.Text = "Time";
            // 
            // btSave
            // 
            this.btSave.Location = new System.Drawing.Point(158, 22);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(42, 23);
            this.btSave.TabIndex = 110;
            this.btSave.Text = "&Save";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // btLoad
            // 
            this.btLoad.Location = new System.Drawing.Point(113, 22);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(42, 23);
            this.btLoad.TabIndex = 109;
            this.btLoad.Text = "&Load";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // udkeyTime
            // 
            this.udkeyTime.Location = new System.Drawing.Point(30, 23);
            this.udkeyTime.Name = "udkeyTime";
            this.udkeyTime.Size = new System.Drawing.Size(77, 20);
            this.udkeyTime.TabIndex = 111;
            // 
            // del
            // 
            this.del.Location = new System.Drawing.Point(293, 22);
            this.del.Name = "del";
            this.del.Size = new System.Drawing.Size(42, 23);
            this.del.TabIndex = 99;
            this.del.Text = "&Del";
            this.del.UseVisualStyleBackColor = true;
            this.del.Click += new System.EventHandler(this.del_Click);
            // 
            // add
            // 
            this.add.Location = new System.Drawing.Point(250, 22);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(42, 23);
            this.add.TabIndex = 98;
            this.add.Text = "&Add";
            this.add.UseVisualStyleBackColor = true;
            this.add.Click += new System.EventHandler(this.add_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 3);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 108;
            this.label3.Text = "ms";
            // 
            // udTick
            // 
            this.udTick.Location = new System.Drawing.Point(92, 0);
            this.udTick.Margin = new System.Windows.Forms.Padding(2);
            this.udTick.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.udTick.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udTick.Name = "udTick";
            this.udTick.Size = new System.Drawing.Size(44, 20);
            this.udTick.TabIndex = 107;
            this.udTick.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udTick.ValueChanged += new System.EventHandler(this.udTick_ValueChanged);
            // 
            // udLoopTime
            // 
            this.udLoopTime.Location = new System.Drawing.Point(274, 0);
            this.udLoopTime.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.udLoopTime.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.udLoopTime.Name = "udLoopTime";
            this.udLoopTime.Size = new System.Drawing.Size(60, 20);
            this.udLoopTime.TabIndex = 100;
            this.udLoopTime.Value = new decimal(new int[] {
            4000,
            0,
            0,
            0});
            this.udLoopTime.ValueChanged += new System.EventHandler(this.udLoopTime_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(266, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 106;
            this.label2.Text = "/";
            // 
            // udStep
            // 
            this.udStep.Location = new System.Drawing.Point(191, 0);
            this.udStep.Name = "udStep";
            this.udStep.Size = new System.Drawing.Size(30, 20);
            this.udStep.TabIndex = 104;
            this.udStep.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(156, 3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(38, 13);
            this.label10.TabIndex = 103;
            this.label10.Text = "Speed";
            // 
            // lbCurTime
            // 
            this.lbCurTime.Location = new System.Drawing.Point(232, 2);
            this.lbCurTime.Name = "lbCurTime";
            this.lbCurTime.Size = new System.Drawing.Size(34, 14);
            this.lbCurTime.TabIndex = 102;
            this.lbCurTime.Text = "0";
            this.lbCurTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ckRun
            // 
            this.ckRun.AutoSize = true;
            this.ckRun.Location = new System.Drawing.Point(50, 3);
            this.ckRun.Name = "ckRun";
            this.ckRun.Size = new System.Drawing.Size(46, 17);
            this.ckRun.TabIndex = 101;
            this.ckRun.Text = "R&un";
            this.ckRun.UseVisualStyleBackColor = true;
            // 
            // ckRunOnce
            // 
            this.ckRunOnce.AutoSize = true;
            this.ckRunOnce.Location = new System.Drawing.Point(3, 3);
            this.ckRunOnce.Name = "ckRunOnce";
            this.ckRunOnce.Size = new System.Drawing.Size(52, 17);
            this.ckRunOnce.TabIndex = 105;
            this.ckRunOnce.Text = "Once";
            this.ckRunOnce.UseVisualStyleBackColor = true;
            // 
            // laCurTime
            // 
            this.laCurTime.BackColor = System.Drawing.Color.Red;
            this.laCurTime.Location = new System.Drawing.Point(147, 30);
            this.laCurTime.Name = "laCurTime";
            this.laCurTime.Size = new System.Drawing.Size(3, 6);
            this.laCurTime.TabIndex = 13;
            // 
            // track
            // 
            this.track.AutoSize = false;
            this.track.Dock = System.Windows.Forms.DockStyle.Top;
            this.track.LargeChange = 20;
            this.track.Location = new System.Drawing.Point(2, 3);
            this.track.Margin = new System.Windows.Forms.Padding(0);
            this.track.Maximum = 10000;
            this.track.Name = "track";
            this.track.Size = new System.Drawing.Size(551, 34);
            this.track.TabIndex = 1;
            this.track.TickStyle = System.Windows.Forms.TickStyle.None;
            this.track.ValueChanged += new System.EventHandler(this.track_ValueChanged);
            // 
            // tpCurrent
            // 
            this.tpCurrent.Controls.Add(this.flCurrent);
            this.tpCurrent.Location = new System.Drawing.Point(4, 22);
            this.tpCurrent.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpCurrent.Name = "tpCurrent";
            this.tpCurrent.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpCurrent.Size = new System.Drawing.Size(555, 488);
            this.tpCurrent.TabIndex = 1;
            this.tpCurrent.Text = "Current";
            this.tpCurrent.UseVisualStyleBackColor = true;
            // 
            // flCurrent
            // 
            this.flCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flCurrent.Location = new System.Drawing.Point(2, 3);
            this.flCurrent.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flCurrent.Name = "flCurrent";
            this.flCurrent.Size = new System.Drawing.Size(551, 482);
            this.flCurrent.TabIndex = 0;
            // 
            // tpParam
            // 
            this.tpParam.Controls.Add(this.flParam);
            this.tpParam.Controls.Add(this.panel4);
            this.tpParam.Location = new System.Drawing.Point(4, 22);
            this.tpParam.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpParam.Name = "tpParam";
            this.tpParam.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpParam.Size = new System.Drawing.Size(555, 488);
            this.tpParam.TabIndex = 2;
            this.tpParam.Text = "Param";
            this.tpParam.UseVisualStyleBackColor = true;
            // 
            // flParam
            // 
            this.flParam.AutoScroll = true;
            this.flParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flParam.ForeColor = System.Drawing.Color.CadetBlue;
            this.flParam.Location = new System.Drawing.Point(2, 28);
            this.flParam.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flParam.Name = "flParam";
            this.flParam.Size = new System.Drawing.Size(551, 457);
            this.flParam.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btLoadSpidar);
            this.panel4.Controls.Add(this.btLoadNuibot);
            this.panel4.Controls.Add(this.btRecvPd);
            this.panel4.Controls.Add(this.btSendPd);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(2, 3);
            this.panel4.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(551, 25);
            this.panel4.TabIndex = 3;
            // 
            // btLoadSpidar
            // 
            this.btLoadSpidar.Location = new System.Drawing.Point(461, 3);
            this.btLoadSpidar.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btLoadSpidar.Name = "btLoadSpidar";
            this.btLoadSpidar.Size = new System.Drawing.Size(89, 20);
            this.btLoadSpidar.TabIndex = 4;
            this.btLoadSpidar.Text = "Load SPIDAR";
            this.btLoadSpidar.UseVisualStyleBackColor = true;
            this.btLoadSpidar.Click += new System.EventHandler(this.btLoadSpidar_Click);
            // 
            // btLoadNuibot
            // 
            this.btLoadNuibot.Location = new System.Drawing.Point(373, 3);
            this.btLoadNuibot.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btLoadNuibot.Name = "btLoadNuibot";
            this.btLoadNuibot.Size = new System.Drawing.Size(84, 20);
            this.btLoadNuibot.TabIndex = 3;
            this.btLoadNuibot.Text = "Load Nuibot";
            this.btLoadNuibot.UseVisualStyleBackColor = true;
            this.btLoadNuibot.Click += new System.EventHandler(this.btLoadNuibot_Click);
            // 
            // btRecvPd
            // 
            this.btRecvPd.Location = new System.Drawing.Point(64, 3);
            this.btRecvPd.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btRecvPd.Name = "btRecvPd";
            this.btRecvPd.Size = new System.Drawing.Size(56, 20);
            this.btRecvPd.TabIndex = 2;
            this.btRecvPd.Text = "Receive";
            this.btRecvPd.UseVisualStyleBackColor = true;
            this.btRecvPd.Click += new System.EventHandler(this.btRecvPd_Click);
            // 
            // btSendPd
            // 
            this.btSendPd.Location = new System.Drawing.Point(4, 3);
            this.btSendPd.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btSendPd.Name = "btSendPd";
            this.btSendPd.Size = new System.Drawing.Size(56, 20);
            this.btSendPd.TabIndex = 1;
            this.btSendPd.Text = "Send";
            this.btSendPd.UseVisualStyleBackColor = true;
            this.btSendPd.Click += new System.EventHandler(this.btSendPd_Click);
            // 
            // tpHeat
            // 
            this.tpHeat.Controls.Add(this.flHeat);
            this.tpHeat.Controls.Add(this.panel5);
            this.tpHeat.Location = new System.Drawing.Point(4, 22);
            this.tpHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpHeat.Name = "tpHeat";
            this.tpHeat.Padding = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tpHeat.Size = new System.Drawing.Size(555, 488);
            this.tpHeat.TabIndex = 4;
            this.tpHeat.Text = "Heat";
            this.tpHeat.UseVisualStyleBackColor = true;
            // 
            // flHeat
            // 
            this.flHeat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flHeat.Location = new System.Drawing.Point(2, 31);
            this.flHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flHeat.Name = "flHeat";
            this.flHeat.Size = new System.Drawing.Size(551, 454);
            this.flHeat.TabIndex = 3;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.btLoadSpidarHeat);
            this.panel5.Controls.Add(this.btLoadNubotiHeat);
            this.panel5.Controls.Add(this.btRecvHeat);
            this.panel5.Controls.Add(this.btSendHeat);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(2, 3);
            this.panel5.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(551, 28);
            this.panel5.TabIndex = 5;
            // 
            // btLoadSpidarHeat
            // 
            this.btLoadSpidarHeat.Location = new System.Drawing.Point(461, 5);
            this.btLoadSpidarHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btLoadSpidarHeat.Name = "btLoadSpidarHeat";
            this.btLoadSpidarHeat.Size = new System.Drawing.Size(89, 20);
            this.btLoadSpidarHeat.TabIndex = 6;
            this.btLoadSpidarHeat.Text = "Load SPIDAR";
            this.btLoadSpidarHeat.UseVisualStyleBackColor = true;
            this.btLoadSpidarHeat.Click += new System.EventHandler(this.btLoadSpidarHeat_Click);
            // 
            // btLoadNubotiHeat
            // 
            this.btLoadNubotiHeat.Location = new System.Drawing.Point(373, 5);
            this.btLoadNubotiHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btLoadNubotiHeat.Name = "btLoadNubotiHeat";
            this.btLoadNubotiHeat.Size = new System.Drawing.Size(84, 20);
            this.btLoadNubotiHeat.TabIndex = 5;
            this.btLoadNubotiHeat.Text = "Load Nuibot";
            this.btLoadNubotiHeat.UseVisualStyleBackColor = true;
            this.btLoadNubotiHeat.Click += new System.EventHandler(this.btLoadNubotiHeat_Click);
            // 
            // btRecvHeat
            // 
            this.btRecvHeat.Location = new System.Drawing.Point(65, 3);
            this.btRecvHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btRecvHeat.Name = "btRecvHeat";
            this.btRecvHeat.Size = new System.Drawing.Size(56, 20);
            this.btRecvHeat.TabIndex = 4;
            this.btRecvHeat.Text = "Receive";
            this.btRecvHeat.UseVisualStyleBackColor = true;
            this.btRecvHeat.Click += new System.EventHandler(this.btRecvHeat_Click);
            // 
            // btSendHeat
            // 
            this.btSendHeat.Location = new System.Drawing.Point(4, 3);
            this.btSendHeat.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btSendHeat.Name = "btSendHeat";
            this.btSendHeat.Size = new System.Drawing.Size(56, 20);
            this.btSendHeat.TabIndex = 2;
            this.btSendHeat.Text = "Send";
            this.btSendHeat.UseVisualStyleBackColor = true;
            this.btSendHeat.Click += new System.EventHandler(this.btSendHeat_Click);
            // 
            // splitContainerTop
            // 
            this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTop.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTop.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainerTop.Name = "splitContainerTop";
            this.splitContainerTop.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerTop.Panel1
            // 
            this.splitContainerTop.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainerTop.Panel2
            // 
            this.splitContainerTop.Panel2.Controls.Add(this.splitContainerBottom);
            this.splitContainerTop.Size = new System.Drawing.Size(172, 514);
            this.splitContainerTop.SplitterDistance = 117;
            this.splitContainerTop.SplitterWidth = 3;
            this.splitContainerTop.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btReset);
            this.panel1.Controls.Add(this.cmbPortBin);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btListBoards);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(172, 117);
            this.panel1.TabIndex = 6;
            // 
            // btReset
            // 
            this.btReset.Location = new System.Drawing.Point(4, 70);
            this.btReset.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btReset.Name = "btReset";
            this.btReset.Size = new System.Drawing.Size(91, 20);
            this.btReset.TabIndex = 3;
            this.btReset.Text = "Reset Motor";
            this.btReset.UseVisualStyleBackColor = true;
            this.btReset.Click += new System.EventHandler(this.btReset_Click);
            // 
            // cmbPortBin
            // 
            this.cmbPortBin.FormattingEnabled = true;
            this.cmbPortBin.Location = new System.Drawing.Point(4, 20);
            this.cmbPortBin.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.cmbPortBin.Name = "cmbPortBin";
            this.cmbPortBin.Size = new System.Drawing.Size(92, 21);
            this.cmbPortBin.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "UART port#";
            // 
            // btListBoards
            // 
            this.btListBoards.Location = new System.Drawing.Point(4, 45);
            this.btListBoards.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btListBoards.Name = "btListBoards";
            this.btListBoards.Size = new System.Drawing.Size(91, 20);
            this.btListBoards.TabIndex = 2;
            this.btListBoards.Text = "List boards";
            this.btListBoards.UseVisualStyleBackColor = true;
            this.btListBoards.Click += new System.EventHandler(this.btListBoards_Click);
            // 
            // splitContainerBottom
            // 
            this.splitContainerBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBottom.Location = new System.Drawing.Point(0, 0);
            this.splitContainerBottom.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainerBottom.Name = "splitContainerBottom";
            this.splitContainerBottom.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerBottom.Panel1
            // 
            this.splitContainerBottom.Panel1.Controls.Add(this.trBoards);
            // 
            // splitContainerBottom.Panel2
            // 
            this.splitContainerBottom.Panel2.Controls.Add(this.txMsg);
            this.splitContainerBottom.Size = new System.Drawing.Size(172, 394);
            this.splitContainerBottom.SplitterDistance = 219;
            this.splitContainerBottom.SplitterWidth = 3;
            this.splitContainerBottom.TabIndex = 5;
            // 
            // trBoards
            // 
            this.trBoards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trBoards.LabelEdit = true;
            this.trBoards.Location = new System.Drawing.Point(0, 0);
            this.trBoards.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.trBoards.Name = "trBoards";
            this.trBoards.Size = new System.Drawing.Size(172, 219);
            this.trBoards.TabIndex = 4;
            this.trBoards.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.trBoards_BeforeLabelEdit);
            this.trBoards.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.trBoards_AfterLabelEdit);
            // 
            // txMsg
            // 
            this.txMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txMsg.Location = new System.Drawing.Point(0, 0);
            this.txMsg.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txMsg.Multiline = true;
            this.txMsg.Name = "txMsg";
            this.txMsg.ReadOnly = true;
            this.txMsg.Size = new System.Drawing.Size(172, 172);
            this.txMsg.TabIndex = 0;
            // 
            // bt_SendTo
            // 
            this.bt_SendTo.Location = new System.Drawing.Point(446, 11);
            this.bt_SendTo.Name = "bt_SendTo";
            this.bt_SendTo.Size = new System.Drawing.Size(75, 23);
            this.bt_SendTo.TabIndex = 114;
            this.bt_SendTo.Text = "SendTo";
            this.bt_SendTo.UseVisualStyleBackColor = true;
            this.bt_SendTo.Click += new System.EventHandler(this.bt_SendTo_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 514);
            this.Controls.Add(this.splitContainerHoriz);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "MainForm";
            this.Text = "PCController";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.splitContainerHoriz.Panel1.ResumeLayout(false);
            this.splitContainerHoriz.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerHoriz)).EndInit();
            this.splitContainerHoriz.ResumeLayout(false);
            this.tbControl.ResumeLayout(false);
            this.tpMagnet.ResumeLayout(false);
            this.tpMagnet.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tpHaptic.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udAmp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udDamp)).EndInit();
            this.tpPos.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udkeyTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udTick)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udLoopTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udStep)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.track)).EndInit();
            this.tpCurrent.ResumeLayout(false);
            this.tpParam.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tpHeat.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.splitContainerTop.Panel1.ResumeLayout(false);
            this.splitContainerTop.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTop)).EndInit();
            this.splitContainerTop.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainerBottom.Panel1.ResumeLayout(false);
            this.splitContainerBottom.Panel2.ResumeLayout(false);
            this.splitContainerBottom.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBottom)).EndInit();
            this.splitContainerBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.IO.Ports.SerialPort uartBin;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.OpenFileDialog openPose;
        private System.Windows.Forms.SaveFileDialog savePose;
        private System.Windows.Forms.SplitContainer splitContainerHoriz;
        private System.Windows.Forms.SplitContainer splitContainerTop;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cmbPortBin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btListBoards;
        private System.Windows.Forms.SplitContainer splitContainerBottom;
        private System.Windows.Forms.TreeView trBoards;
        private System.Windows.Forms.TextBox txMsg;
        private System.Windows.Forms.TabControl tbControl;
        private System.Windows.Forms.TabPage tpPos;
        private System.Windows.Forms.FlowLayoutPanel flPos;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCopy;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.NumericUpDown udkeyTime;
        private System.Windows.Forms.Button del;
        private System.Windows.Forms.Button add;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown udTick;
        private System.Windows.Forms.NumericUpDown udLoopTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udStep;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lbCurTime;
        private System.Windows.Forms.CheckBox ckRun;
        private System.Windows.Forms.CheckBox ckRunOnce;
        private System.Windows.Forms.Label laCurTime;
        private System.Windows.Forms.TrackBar track;
        private System.Windows.Forms.TabPage tpCurrent;
        private System.Windows.Forms.FlowLayoutPanel flCurrent;
        private System.Windows.Forms.TabPage tpParam;
        private System.Windows.Forms.Button btRecvPd;
        private System.Windows.Forms.Button btSendPd;
        private System.Windows.Forms.FlowLayoutPanel flParam;
        private System.Windows.Forms.TabPage tpHeat;
        private System.Windows.Forms.Button btRecvHeat;
        private System.Windows.Forms.Button btSendHeat;
        private System.Windows.Forms.FlowLayoutPanel flHeat;
        private System.Windows.Forms.TabPage tpMagnet;
        private System.Windows.Forms.Button btMeasure;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btLoadSpidar;
        private System.Windows.Forms.Button btLoadNuibot;
        private System.Windows.Forms.Panel pnMagnet;
        private System.Windows.Forms.TabPage tpHaptic;
        private System.Windows.Forms.FlowLayoutPanel flHaptic;
        private System.Windows.Forms.Button btHapticStart;
        private System.Windows.Forms.Button btReset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown udDamp;
        private System.Windows.Forms.NumericUpDown udAmp;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button btLoadSpidarHeat;
        private System.Windows.Forms.Button btLoadNubotiHeat;
        private System.Windows.Forms.Button bt_SendTo;
    }
}


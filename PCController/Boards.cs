using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace PCController
{
    public class Boards : List<Board>
    {
        System.IO.Ports.SerialPort serial = null;
        public System.IO.Ports.SerialPort Serial
        {
            get { return serial; }
            set { serial = value; }
        }
        public enum ControlMode
        {
            CM_DIRECT,
            CM_CURRENT,
            CM_INTERPOLATE
        };
        ControlMode controlMode = ControlMode.CM_DIRECT;
        byte interpolateTargetCountWrite;
        public byte InterpolateTargetCountWrite { get { return interpolateTargetCountWrite; } }
        public byte InterpolateTargetCountRead
        {
            get
            {
                if (Count == 0) return 0;
                byte rv = this[0].interpolateTargetCountRead;
                foreach (Board b in this)
                {
                    int diff = b.interpolateTargetCountRead - rv;
                    if (diff < 0) rv = (byte)(rv + diff);
                }
                return rv;
            }
        }
        public ushort TickMin
        {
            get
            {
                if (Count == 0) return 0;
                ushort rv = this[0].tickRead;
                foreach (Board b in this)
                {
                    int diff = b.tickRead - rv;
                    if (diff < 0) rv = (ushort)(rv + diff);
                }
                return rv;
            }
        }
        public ushort TickMax
        {
            get
            {
                if (this.Count == 0) return 0;
                ushort rv = this[0].tickRead;
                foreach (Board b in this)
                {
                    int diff = b.tickRead - rv;
                    if (diff > 0) rv = (ushort)(rv + diff);
                }
                return rv;
            }
        }
        int nMotor;
        int nCurrent;
        int nForce;
        int nTouch;
        int nTarget;
        public int NMotor { get { return nMotor; } }
        public int NCurrent { get { return nCurrent; } }
        public int NForce { get { return nForce; } }
        public int NTarget { get { return nTarget; } }
        public int NTouch { get { return nTouch; } }
        public void SetNMotor(int n) { nMotor = n; }
        public short GetPos(int i) { return pos[i]; }
        public short GetVel(int i) { return vel[i]; }
        public short GetCurrent(int i) { return current[i]; }
        public short GetForce(int i) { return force[i]; }
        short[] pos;
        short[] vel;
        short[] current;
        short[] force;
        short[] touch;
        public static void WriteUShort(byte[] buf, ref int cur, ushort v)
        {
            buf[cur++] = (byte)(v & 0xFF);
            buf[cur++] = (byte)(v >> 8);
        }
        public static void WriteShort(byte[] buf, ref int cur, short v)
        {
            buf[cur++] = (byte)(v & 0xFF);
            buf[cur++] = (byte)(v >> 8);
        }
        public static void WriteByte(byte[] buf, ref int cur, byte v)
        {
            buf[cur++] = v;
        }
        public static short ReadShort(byte[] buf, ref int cur)
        {
            short v;
            v = buf[cur++];
            v = (short)(v | (short)(buf[cur++] << 8));
            return v;
        }
        public static void PrepareBuffers(ref byte[] send, ref byte[] recv, CommandId id, Board board)
        {
            int wait = board.GetWaitLen(id);
            int bufLen = wait + board.CommandLen(id);
            int retLen = board.ReturnLen(id);
            send = Enumerable.Repeat((byte)0, bufLen).ToArray();
            recv = new byte[retLen];
            send[0] = Board.MakeHeader(id, board.boardId);
        }
        public void ReadSerial(ref byte[] recvBuf)
        {
            int nRead = 0;
            while (nRead < recvBuf.Length)
            {
                nRead += Serial.Read(recvBuf, nRead, recvBuf.Length - nRead);
            }
        }

        public void SendPosDirect(short[] targets)
        {
            SetControlMode(ControlMode.CM_DIRECT);
            int mi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_DIRECT, board);
                int cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, targets[mi++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    pos[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    vel[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void SetControlMode(ControlMode m)
        {
            if (m != controlMode)
            {
                if (m == ControlMode.CM_INTERPOLATE)
                {
                    interpolateTargetCountWrite = 0;
                    foreach (Board b in this)
                    {
                        b.interpolateTargetCountRead = 0x100 - 2;
                    }
                }
                controlMode = m;
            }
        }
        public void SendPosInterpolate(short[] targets, ushort period)
        {
            int mi = 0;
            SetControlMode(ControlMode.CM_INTERPOLATE);
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_INTERPOLATE, board);
                int cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, targets[mi++]);
                }
                WriteShort(sendBuf, ref cur, (short)period);
                WriteByte(sendBuf, ref cur, interpolateTargetCountWrite);
                if (period > 0) //  period == 0 means no target is sent but get the state.
                {
                    interpolateTargetCountWrite++;
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    pos[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
                board.tickRead = (ushort)ReadShort(recvBuf, ref cur);
                board.interpolateTargetCountRead = recvBuf[cur];
            }
        }

        //  return received currents;
        public short[] SendCurrent(short[] currents, bool bWriteOnly=false)
        {
            SetControlMode(ControlMode.CM_CURRENT);
            int mi = 0;
            short[] rv = new short[currents.Length];
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_CURRENT, board);
                int cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, currents[mi++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                if (!bWriteOnly)
                {
                    ReadSerial(ref recvBuf);
                }
                cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    pos[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    vel[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    rv[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
            }
            return rv;
        }
        public void SendResetMotor() {
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_RESET_SENSOR, board);
                int cur = 1;
                WriteShort(sendBuf, ref cur, (short)ResetSensorFlags.RSF_MOTOR);
                Serial.Write(sendBuf, 0, sendBuf.Length);
            }
        }        public void SendSense()
        {
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SENSOR, board);
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 1;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    pos[board.motorMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nCurrent; ++i)
                {
                    current[board.currentMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nForce; ++i)
                {
                    force[board.forceMap[i]] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nTouch; ++i)
                {
                    touch[board.touchMap[i]] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void SendParamPd(short[] k, short[] b)
        {
            int ki = 0;
            int bi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_PD;
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, k[ki++]);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, b[bi++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                Thread.Sleep(100);
            }
        }
        public void SendParamHeat(short[] limit, short[] release)
        {
            int ki = 0;
            int bi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_MOTOR_HEAT;
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, limit[ki++]);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, release[bi++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                Thread.Sleep(100);
            }
        }
        public void SendParamCurrent(short[] a)
        {
            int ai = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_CURRENT;
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, a[ai++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                Thread.Sleep(100);
            }
        }
        public void SendParamTorqueLimit(short[] tmin, short[] tmax)
        {
            int i1 = 0;
            int i2 = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_TORQUE_LIMIT;
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, tmin[i1++]);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    WriteShort(sendBuf, ref cur, tmax[i2++]);
                }
                Serial.Write(sendBuf, 0, sendBuf.Length);
                Thread.Sleep(100);
            }
        }
        public void SendParamBoardId(byte[] ids)
        {
            int i = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_SET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_BOARD_ID;
                int cur = 2;
                board.boardId = ids[i];
                sendBuf[cur++] = ids[i++];
                Serial.Write(sendBuf, 0, sendBuf.Length);
                Thread.Sleep(100);
            }
        }
        public void EnumerateBoard()
        {
            if (serial == null) return;
            byte[] tempInit = { 0, 0, 0, 0, 0, 0, 0 };
            Board boardTemp = new Board(tempInit, null);
            //  clear serial receive buffer
            while (Serial.BytesToRead > 0) Serial.DiscardInBuffer();
            //  compute length
            int wait = boardTemp.GetWaitLen(CommandId.CI_BOARD_INFO);
            int bufLen = wait + boardTemp.CommandLen(CommandId.CI_BOARD_INFO);
            int retLen = boardTemp.ReturnLen(CommandId.CI_BOARD_INFO);
            byte[] sendBuf = Enumerable.Repeat((byte)0, bufLen).ToArray();
            byte[] recvBuf = new byte[retLen];
            //  get information of all boards.
            nMotor = 0; nCurrent = 0; nForce = 0; nTouch = 0;
            for (int bi = 0; bi < 8; ++bi)
            {
                sendBuf[0] = Board.MakeHeader(CommandId.CI_BOARD_INFO, bi);
                Serial.Write(sendBuf, 0, bufLen);
                for (int t = 0; t < 10; ++t)
                {
                    if (Serial.BytesToRead == retLen)
                    {
                        Serial.Read(recvBuf, 0, retLen);
                        Board board = new Board(recvBuf, this);
                        nMotor += board.nMotor;
                        nCurrent += board.nCurrent;
                        nForce += board.nForce;
                        nTouch += board.nTouch;
                        Add(board);
                    }
                    Thread.Sleep(2);
                }
            }
            //  set command length of all boards.
            foreach (Board board in this)
            {
                sendBuf[0] = Board.MakeHeader(CommandId.CI_SET_CMDLEN, board.boardId);
                for (int i = 0; i < (int)CommandId.CI_NCOMMAND; ++i)
                {
                    sendBuf[i + 1] = (byte)board.CommandLen((CommandId)i);
                }
                Serial.Write(sendBuf, 0, bufLen);
            }
            //  update nTarget
            nTarget = int.MaxValue;
            foreach (Board board in this)
            {
                nTarget = board.nTarget < nTarget ? board.nTarget : nTarget;
            }
            pos = Enumerable.Repeat((short)0, NMotor).ToArray();
            vel = Enumerable.Repeat((short)0, NMotor).ToArray();
            current = Enumerable.Repeat((short)0, NCurrent).ToArray();
            force = Enumerable.Repeat((short)0, NForce).ToArray();
            touch = Enumerable.Repeat((short)0, NTouch).ToArray();
        }

        public void RecvParamPd(ref short[] k, ref short[] b)
        {
            int ki = 0;
            int bi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_GET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_PD;
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    k[ki++] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    b[bi++] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void RecvParamHeat(ref short[] limit, ref short[] release)
        {
            int ki = 0;
            int bi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_GET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_MOTOR_HEAT;
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    limit[ki++] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    release[bi++] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void RecvParamTorque(ref short[] min, ref short[] max)
        {
            int ki = 0;
            int bi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_GET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_TORQUE_LIMIT;
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    min[ki++] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    max[bi++] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void RecvParamCurrent(ref short[] a)
        {
            int ai = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_GET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_CURRENT;
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    a[ai++] = ReadShort(recvBuf, ref cur);
                }
            }
        }
        public void RecvParamMagnetSensor(ref short[] m)
        {
            int mi = 0;
            foreach (Board board in this)
            {
                byte[] sendBuf = null, recvBuf = null;
                PrepareBuffers(ref sendBuf, ref recvBuf, CommandId.CI_GET_PARAM, board);
                sendBuf[1] = (byte)SetParamType.PT_MAGNET;
                Serial.Write(sendBuf, 0, sendBuf.Length);
                ReadSerial(ref recvBuf);
                int cur = 2;
                for (int i = 0; i < board.nMotor; ++i)
                {
                    m[mi + i*2] = ReadShort(recvBuf, ref cur);
                }
                for (int i = 0; i < board.nMotor; ++i)
                {
                    m[mi + i*2+1] = ReadShort(recvBuf, ref cur);
                }
                mi += board.nMotor * 2;
            }
        }
    }
}
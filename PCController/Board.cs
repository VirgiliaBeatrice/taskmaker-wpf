using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace PCController
{
    public class Board
    {
        public int boardId;
        public int modelNumber;
        public int nTarget;
        public int nMotor;
        public int nCurrent;
        public int nForce;
        public int nTouch;
        public ushort tickRead;    //  tick read from board
        public byte interpolateTargetCountRead;
        public List<int> motorMap = new List<int>();
        public List<int> currentMap = new List<int>();
        public List<int> forceMap = new List<int>();
        public List<int> touchMap = new List<int>();
        public Board(byte[] info, Boards boards) {
            int cur = 0;
            boardId = GetBoardId(info[cur++]);
            modelNumber = info[cur++];
            nTarget = info[cur++];
            nMotor = info[cur++];
            nCurrent = info[cur++];
            nForce = info[cur++];
            nTouch = info[cur++];
            int c = 0;
            
            if (boards != null) c = boards.NMotor;
            for (int i = 0; i < nMotor; ++i)
            {
                motorMap.Add(c++);
            }
            c = 0;
            if (boards != null) c = boards.NCurrent;
            for (int i = 0; i < nCurrent; ++i)
            {
                currentMap.Add(c++);
            }
            c = 0;
            if (boards != null) c = boards.NForce;
            for (int i = 0; i < nForce; ++i)
            {
                forceMap.Add(c++);
            }
            c = 0;
            if (boards != null) c = boards.NTouch;
            for (int i = 0; i < nTouch; ++i)
            {
                touchMap.Add(c++);
            }
        }
        public int CommandLen(CommandId c) {
            switch (c)
            {
                case CommandId.CI_NONE:
                    return 1;
                case CommandId.CI_BOARD_INFO:
                    return 1;
                case CommandId.CI_SET_CMDLEN:
                    return 1 + (int)CommandId.CI_NCOMMAND;
                case CommandId.CI_ALL:
                    return 1 + 4 + nMotor * 2 * (1 + (nForce==0?1:nForce));    //  nMotor * (pos + (vel or jacob))
                case CommandId.CI_SENSOR:
                    return 1;
                case CommandId.CI_DIRECT:
                    return 1 + nMotor*2*2;
                case CommandId.CI_CURRENT:
                    return 1 + nMotor*2;
                case CommandId.CI_INTERPOLATE:
                    return 1 + nMotor*2 + 2 + 1;
                case CommandId.CI_FORCE_CONTROL:
                    return 1 + nMotor*(2 + 2 * nForce) + 2 + 1;    //  nMotor * pos jacob
                case CommandId.CI_SET_PARAM:
                    return 1 + 1 + nMotor * 2 * 2;
                case CommandId.CI_RESET_SENSOR:
                    return 1 + 2;
                case CommandId.CI_GET_PARAM:
                    return 1 + 1;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            Debug.Assert(false);
            return 0;
        }
        public int ReturnLen(CommandId r)
        {
            switch (r)
            {
                case CommandId.CI_BOARD_INFO: return 1 + 6;
                case CommandId.CI_DIRECT: return 1 + nMotor * 2 * 2;
                case CommandId.CI_INTERPOLATE: return 1 + nMotor * 2 + 2 + 1;
                case CommandId.CI_CURRENT: return 1 + nMotor * 3 * 2;
                case CommandId.CI_GET_PARAM: return 1 + 1 + nMotor * 2 * 2;
                case CommandId.CI_SENSOR: return 1 + (nMotor + nCurrent + nForce + nTouch) * 2;
            }
            return 0;
        }
        public int GetWaitLen(CommandId c) {
            int wait = ReturnLen(c) - CommandLen(c) + 20;
            if (wait < 5) wait = 5;
            return wait;
        }
        public static byte MakeHeader(CommandId c, int boardId) {
            Debug.Assert(boardId < 8);
            return (byte)(((int)c << 3) | boardId);
        }
        public static int GetBoardId(byte header)
        {
            return header & 0x7;
        }
        public static int GetCommand(byte header)
        {
            return header >> 3;
        }
    }
}

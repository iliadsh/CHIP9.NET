using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace CHIP9.NET
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(0)]
        public byte A;

        [FieldOffset(1)]
        public byte C;
        [FieldOffset(2)]
        public byte B;
        [FieldOffset(1)]
        public ushort BC;

        [FieldOffset(3)]
        public byte E;
        [FieldOffset(4)]
        public byte D;
        [FieldOffset(3)]
        public ushort DE;

        [FieldOffset(5)]
        public byte L;
        [FieldOffset(6)]
        public byte H;
        [FieldOffset(5)]
        public ushort HL;

        [FieldOffset(7)]
        public ushort SP;
        [FieldOffset(8)]
        public ushort PC;
    }

    public struct Flags
    {
        public bool Z;
        public bool N;
        public bool H;
        public bool C;
    }

    public class CPU
    {
        public byte[] memory = new byte[ushort.MaxValue];
        public bool[,] screen_buffer = new bool[128, 64];
        public Registers registers = new Registers();
        public Flags flags = new Flags();
        public delegate void Operate(out ushort moveAmount);
        public Operate[] opcodes = new Operate[0xFF];

        public void Run()
        {
            InitOpcodes();
            registers.PC = 0;
            byte[] bootromBytes = File.ReadAllBytes("bootrom");
            byte[] romBytes = File.ReadAllBytes("rom");
            Array.Copy(bootromBytes, 0, memory, 0, bootromBytes.Length);
            Array.Copy(romBytes, 0, memory, 0x597, romBytes.Length);
            new Thread(new ThreadStart(FetchExecute));
        }

        public void FetchExecute()
        {
            while (true)
            {
                byte opcode = memory[registers.PC];
                var operation = opcodes[opcode];
                if (operation == null)
                {
                    Console.WriteLine(string.Format("SYSTEM FATAL ERROR: Unknown Opcode {0}", opcode));
                    break;
                }
                operation(out ushort moveAmount);
                registers.PC += moveAmount;
            }
        }

        public void InitOpcodes()
        {
            //LDI B, xx
            opcodes[0x20] = (out ushort moveAmount) =>
            {
                registers.B = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI C, xx
            opcodes[0x30] = (out ushort moveAmount) =>
            {
                registers.C = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI D, xx
            opcodes[0x40] = (out ushort moveAmount) =>
            {
                registers.D = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI E, xx
            opcodes[0x50] = (out ushort moveAmount) =>
            {
                registers.E = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI H, xx
            opcodes[0x60] = (out ushort moveAmount) =>
            {
                registers.H = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI L, xx
            opcodes[0x70] = (out ushort moveAmount) =>
            {
                registers.L = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI (HL), xx
            opcodes[0x80] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDI A, xx
            opcodes[0x90] = (out ushort moveAmount) =>
            {
                registers.A = memory[registers.PC + 1];
                moveAmount = 2;
            };
            //LDX BC, xxyy
            opcodes[0x21] = (out ushort moveAmount) =>
            {
                registers.C = memory[registers.PC + 1];
                registers.B = memory[registers.PC + 2];
                moveAmount = 3;
            };
            //LDX DE, xxyy
            opcodes[0x31] = (out ushort moveAmount) =>
            {
                registers.E = memory[registers.PC + 1];
                registers.D = memory[registers.PC + 2];
                moveAmount = 3;
            };
            //LDX HL, xxyy
            opcodes[0x41] = (out ushort moveAmount) =>
            {
                registers.L = memory[registers.PC + 1];
                registers.H = memory[registers.PC + 2];
                moveAmount = 3;
            };
            //LDX SP, xxyy
            opcodes[0x22] = (out ushort moveAmount) =>
            {
                registers.SP = 0;
                registers.SP |= memory[registers.PC + 1];
                registers.SP |= (ushort)(memory[registers.PC + 2] << 8);
                moveAmount = 3;
            };
            //PUSH B
            opcodes[0x81] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.B;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH C
            opcodes[0x91] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.C;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH D
            opcodes[0xA1] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.D;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH E
            opcodes[0xB1] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.E;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH H
            opcodes[0xC1] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.H;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH L
            opcodes[0xD1] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.L;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH (HL)
            opcodes[0xC0] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = memory[registers.HL];
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH A
            opcodes[0xD0] = (out ushort moveAmount) =>
            {
                memory[registers.SP] = registers.A;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH BC
            opcodes[0x51] = (out ushort moveAmount) =>
            {
                memory[registers.SP - 1] = registers.B;
                memory[registers.SP] = registers.C;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH DE
            opcodes[0x61] = (out ushort moveAmount) =>
            {
                memory[registers.SP - 1] = registers.D;
                memory[registers.SP] = registers.E;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH HL
            opcodes[0x71] = (out ushort moveAmount) =>
            {
                memory[registers.SP - 1] = registers.H;
                memory[registers.SP] = registers.L;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //POP B
            opcodes[0x82] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.B = memory[registers.SP];
                moveAmount = 1;
            };
            //POP C
            opcodes[0x92] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.C = memory[registers.SP];
                moveAmount = 1;
            };
            //POP D
            opcodes[0xA2] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.D = memory[registers.SP];
                moveAmount = 1;
            };
            //POP E
            opcodes[0xB2] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.E = memory[registers.SP];
                moveAmount = 1;
            };
            //POP H
            opcodes[0xC2] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.H = memory[registers.SP];
                moveAmount = 1;
            };
            //POP L
            opcodes[0xD2] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.L = memory[registers.SP];
                moveAmount = 1;
            };
            //POP (HL)
            opcodes[0xC3] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                memory[registers.HL] = memory[registers.SP];
                moveAmount = 1;
            };
            //POP A
            opcodes[0xD3] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.A = memory[registers.SP];
                moveAmount = 1;
            };
            //POP BC
            opcodes[0x52] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.C = memory[registers.SP];
                registers.B = memory[registers.SP + 1];
                moveAmount = 1;
            };
            //POP DE
            opcodes[0x62] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.E = memory[registers.SP];
                registers.D = memory[registers.SP + 1];
                moveAmount = 1;
            };
            //POP HL
            opcodes[0x72] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                registers.L = memory[registers.SP];
                registers.H = memory[registers.SP + 1];
                moveAmount = 1;
            };
            //kms
            //MOVE B, B
            opcodes[0x09] = (out ushort moveAmount) =>
            {
                registers.B = registers.B;
                moveAmount = 1;
            };
            //MOVE B, C
            opcodes[0x19] = (out ushort moveAmount) =>
            {
                registers.B = registers.C;
                moveAmount = 1;
            };
            //MOVE B, D
            opcodes[0x29] = (out ushort moveAmount) =>
            {
                registers.B = registers.D;
                moveAmount = 1;
            };
            //MOVE B, E
            opcodes[0x39] = (out ushort moveAmount) =>
            {
                registers.B = registers.E;
                moveAmount = 1;
            };
            //MOVE B, H
            opcodes[0x49] = (out ushort moveAmount) =>
            {
                registers.B = registers.H;
                moveAmount = 1;
            };
            //MOVE B, L
            opcodes[0x59] = (out ushort moveAmount) =>
            {
                registers.B = registers.L;
                moveAmount = 1;
            };
            //MOVE B, (HL)
            opcodes[0x69] = (out ushort moveAmount) =>
            {
                registers.B = memory[registers.HL];
                moveAmount = 1;
            };
            //MOVE B, A
            opcodes[0x79] = (out ushort moveAmount) =>
            {
                registers.B = registers.A;
                moveAmount = 1;
            };
            //MOVE C, B
            opcodes[0x89] = (out ushort moveAmount) =>
            {
                registers.C = registers.B;
                moveAmount = 1;
            };
            //MOVE C, C
            opcodes[0x99] = (out ushort moveAmount) =>
            {
                registers.C = registers.C;
                moveAmount = 1;
            };
            //MOVE C, D
            opcodes[0xA9] = (out ushort moveAmount) =>
            {
                registers.C = registers.D;
                moveAmount = 1;
            };
            //MOVE C, E
            opcodes[0xB9] = (out ushort moveAmount) =>
            {
                registers.C = registers.E;
                moveAmount = 1;
            };
            //MOVE C, H
            opcodes[0xC9] = (out ushort moveAmount) =>
            {
                registers.C = registers.H;
                moveAmount = 1;
            };
            //MOVE C, L
            opcodes[0xD9] = (out ushort moveAmount) =>
            {
                registers.C = registers.L;
                moveAmount = 1;
            };
            //MOVE C, (HL)
            opcodes[0xE9] = (out ushort moveAmount) =>
            {
                registers.C = memory[registers.HL];
                moveAmount = 1;
            };
            //MOVE C, A
            opcodes[0xF9] = (out ushort moveAmount) =>
            {
                registers.C = registers.A;
                moveAmount = 1;
            };
        }
    }
}

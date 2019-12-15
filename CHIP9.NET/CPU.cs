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
        public Thread execThread;

        public void Run()
        {
            InitOpcodes();
            registers.PC = 0;
            byte[] bootromBytes = File.ReadAllBytes("bootrom");
            byte[] romBytes = File.ReadAllBytes("rom");
            Array.Copy(bootromBytes, 0, memory, 0, bootromBytes.Length);
            Array.Copy(romBytes, 0, memory, 0x597, romBytes.Length);
            execThread = new Thread(new ThreadStart(FetchExecute));
            execThread.Start();
        }

        public void FetchExecute()
        {
            while (true)
            {
                byte opcode = memory[registers.PC];
                var operation = opcodes[opcode];
                if (operation == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("SYSTEM FATAL ERROR: Unknown Opcode {0}", opcode));
                    Console.ResetColor();
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
            //MOV B, B
            opcodes[0x09] = (out ushort moveAmount) =>
            {
                registers.B = registers.B;
                moveAmount = 1;
            };
            //MOV B, C
            opcodes[0x19] = (out ushort moveAmount) =>
            {
                registers.B = registers.C;
                moveAmount = 1;
            };
            //MOV B, D
            opcodes[0x29] = (out ushort moveAmount) =>
            {
                registers.B = registers.D;
                moveAmount = 1;
            };
            //MOV B, E
            opcodes[0x39] = (out ushort moveAmount) =>
            {
                registers.B = registers.E;
                moveAmount = 1;
            };
            //MOV B, H
            opcodes[0x49] = (out ushort moveAmount) =>
            {
                registers.B = registers.H;
                moveAmount = 1;
            };
            //MOV B, L
            opcodes[0x59] = (out ushort moveAmount) =>
            {
                registers.B = registers.L;
                moveAmount = 1;
            };
            //MOV B, (HL)
            opcodes[0x69] = (out ushort moveAmount) =>
            {
                registers.B = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV B, A
            opcodes[0x79] = (out ushort moveAmount) =>
            {
                registers.B = registers.A;
                moveAmount = 1;
            };
            //MOV C, B
            opcodes[0x89] = (out ushort moveAmount) =>
            {
                registers.C = registers.B;
                moveAmount = 1;
            };
            //MOV C, C
            opcodes[0x99] = (out ushort moveAmount) =>
            {
                registers.C = registers.C;
                moveAmount = 1;
            };
            //MOV C, D
            opcodes[0xA9] = (out ushort moveAmount) =>
            {
                registers.C = registers.D;
                moveAmount = 1;
            };
            //MOV C, E
            opcodes[0xB9] = (out ushort moveAmount) =>
            {
                registers.C = registers.E;
                moveAmount = 1;
            };
            //MOV C, H
            opcodes[0xC9] = (out ushort moveAmount) =>
            {
                registers.C = registers.H;
                moveAmount = 1;
            };
            //MOV C, L
            opcodes[0xD9] = (out ushort moveAmount) =>
            {
                registers.C = registers.L;
                moveAmount = 1;
            };
            //MOV C, (HL)
            opcodes[0xE9] = (out ushort moveAmount) =>
            {
                registers.C = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV C, A
            opcodes[0xF9] = (out ushort moveAmount) =>
            {
                registers.C = registers.A;
                moveAmount = 1;
            };
            //MOV D, B
            opcodes[0x0A] = (out ushort moveAmount) =>
            {
                registers.D = registers.B;
                moveAmount = 1;
            };
            //MOV D, C
            opcodes[0x1A] = (out ushort moveAmount) =>
            {
                registers.D = registers.C;
                moveAmount = 1;
            };
            //MOV D, D
            opcodes[0x2A] = (out ushort moveAmount) =>
            {
                registers.D = registers.D;
                moveAmount = 1;
            };
            //MOV D, E
            opcodes[0x3A] = (out ushort moveAmount) =>
            {
                registers.D = registers.E;
                moveAmount = 1;
            };
            //MOV D, H
            opcodes[0x4A] = (out ushort moveAmount) =>
            {
                registers.D = registers.H;
                moveAmount = 1;
            };
            //MOV D, L
            opcodes[0x5A] = (out ushort moveAmount) =>
            {
                registers.D = registers.L;
                moveAmount = 1;
            };
            //MOV D, (HL)
            opcodes[0x6A] = (out ushort moveAmount) =>
            {
                registers.D = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV D, A
            opcodes[0x7A] = (out ushort moveAmount) =>
            {
                registers.D = registers.A;
                moveAmount = 1;
            };
            //MOV E, B
            opcodes[0x8A] = (out ushort moveAmount) =>
            {
                registers.E = registers.B;
                moveAmount = 1;
            };
            //MOV E, C
            opcodes[0x9A] = (out ushort moveAmount) =>
            {
                registers.E = registers.C;
                moveAmount = 1;
            };
            //MOV E, D
            opcodes[0xAA] = (out ushort moveAmount) =>
            {
                registers.E = registers.D;
                moveAmount = 1;
            };
            //MOV E, E
            opcodes[0xBA] = (out ushort moveAmount) =>
            {
                registers.E = registers.E;
                moveAmount = 1;
            };
            //MOV E, H
            opcodes[0xCA] = (out ushort moveAmount) =>
            {
                registers.E = registers.H;
                moveAmount = 1;
            };
            //MOV E, L
            opcodes[0xDA] = (out ushort moveAmount) =>
            {
                registers.E = registers.L;
                moveAmount = 1;
            };
            //MOV E, (HL)
            opcodes[0xEA] = (out ushort moveAmount) =>
            {
                registers.E = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV E, A
            opcodes[0xFA] = (out ushort moveAmount) =>
            {
                registers.E = registers.A;
                moveAmount = 1;
            };
            //MOV H, B
            opcodes[0x0B] = (out ushort moveAmount) =>
            {
                registers.H = registers.B;
                moveAmount = 1;
            };
            //MOV H, C
            opcodes[0x1B] = (out ushort moveAmount) =>
            {
                registers.H = registers.C;
                moveAmount = 1;
            };
            //MOV H, D
            opcodes[0x2B] = (out ushort moveAmount) =>
            {
                registers.H = registers.D;
                moveAmount = 1;
            };
            //MOV H, E
            opcodes[0x3B] = (out ushort moveAmount) =>
            {
                registers.H = registers.E;
                moveAmount = 1;
            };
            //MOV H, H
            opcodes[0x4B] = (out ushort moveAmount) =>
            {
                registers.H = registers.H;
                moveAmount = 1;
            };
            //MOV H, L
            opcodes[0x5B] = (out ushort moveAmount) =>
            {
                registers.H = registers.L;
                moveAmount = 1;
            };
            //MOV H, (HL)
            opcodes[0x6B] = (out ushort moveAmount) =>
            {
                registers.H = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV H, A
            opcodes[0x7B] = (out ushort moveAmount) =>
            {
                registers.H = registers.A;
                moveAmount = 1;
            };
            //MOV L, B
            opcodes[0x8B] = (out ushort moveAmount) =>
            {
                registers.L = registers.B;
                moveAmount = 1;
            };
            //MOV L, C
            opcodes[0x9B] = (out ushort moveAmount) =>
            {
                registers.L = registers.C;
                moveAmount = 1;
            };
            //MOV L, D
            opcodes[0xAB] = (out ushort moveAmount) =>
            {
                registers.L = registers.D;
                moveAmount = 1;
            };
            //MOV L, E
            opcodes[0xBB] = (out ushort moveAmount) =>
            {
                registers.L = registers.E;
                moveAmount = 1;
            };
            //MOV L, H
            opcodes[0xCB] = (out ushort moveAmount) =>
            {
                registers.L = registers.H;
                moveAmount = 1;
            };
            //MOV L, L
            opcodes[0xDB] = (out ushort moveAmount) =>
            {
                registers.L = registers.L;
                moveAmount = 1;
            };
            //MOV L, (HL)
            opcodes[0xEB] = (out ushort moveAmount) =>
            {
                registers.L = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV L, A
            opcodes[0xFB] = (out ushort moveAmount) =>
            {
                registers.L = registers.A;
                moveAmount = 1;
            };
            //MOV (HL), B
            opcodes[0x0C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.B;
                moveAmount = 1;
            };
            //MOV (HL), C
            opcodes[0x1C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.C;
                moveAmount = 1;
            };
            //MOV (HL), D
            opcodes[0x2C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.D;
                moveAmount = 1;
            };
            //MOV (HL), E
            opcodes[0x3C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.E;
                moveAmount = 1;
            };
            //MOV (HL), H
            opcodes[0x4C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.H;
                moveAmount = 1;
            };
            //MOV (HL), L
            opcodes[0x5C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.L;
                moveAmount = 1;
            };
            //HCF
            opcodes[0x6C] = (out ushort moveAmount) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("SYSTEM: HALT & CATCH FIRE CALLED");
                Console.ResetColor();
                moveAmount = 1;
            };
            //MOV (HL), A
            opcodes[0x7C] = (out ushort moveAmount) =>
            {
                memory[registers.HL] = registers.A;
                moveAmount = 1;
            };
            //MOV A, B
            opcodes[0x8C] = (out ushort moveAmount) =>
            {
                registers.A = registers.B;
                moveAmount = 1;
            };
            //MOV A, C
            opcodes[0x9C] = (out ushort moveAmount) =>
            {
                registers.A = registers.C;
                moveAmount = 1;
            };
            //MOV A, D
            opcodes[0xAC] = (out ushort moveAmount) =>
            {
                registers.A = registers.D;
                moveAmount = 1;
            };
            //MOV A, E
            opcodes[0xBC] = (out ushort moveAmount) =>
            {
                registers.A = registers.E;
                moveAmount = 1;
            };
            //MOV A, H
            opcodes[0xCC] = (out ushort moveAmount) =>
            {
                registers.A = registers.H;
                moveAmount = 1;
            };
            //MOV A, L
            opcodes[0xDC] = (out ushort moveAmount) =>
            {
                registers.A = registers.L;
                moveAmount = 1;
            };
            //MOV A, (HL)
            opcodes[0xEC] = (out ushort moveAmount) =>
            {
                registers.A = memory[registers.HL];
                moveAmount = 1;
            };
            //MOV A, A
            opcodes[0xFC] = (out ushort moveAmount) =>
            {
                registers.A = registers.A;
                moveAmount = 1;
            };
            //MOV HL, BC
            opcodes[0xED] = (out ushort moveAmount) =>
            {
                registers.HL = registers.BC;
                moveAmount = 1;
            };
            //MOV HL, DE
            opcodes[0xFD] = (out ushort moveAmount) =>
            {
                registers.HL = registers.DE;
                moveAmount = 1;
            };
            //CLRFLAG
            opcodes[0x08] = (out ushort moveAmount) =>
            {
                flags.Z = false;
                flags.N = false;
                flags.H = false;
                flags.C = false;
                moveAmount = 1;
            };
            //SETFLAG Z, 1
            opcodes[0x18] = (out ushort moveAmount) =>
            {
                flags.Z = true;
                moveAmount = 1;
            };
            //SETFLAG Z, 0
            opcodes[0x28] = (out ushort moveAmount) =>
            {
                flags.Z = false;
                moveAmount = 1;
            };
            //SETFLAG N, 1
            opcodes[0x38] = (out ushort moveAmount) =>
            {
                flags.N = true;
                moveAmount = 1;
            };
            //SETFLAG N, 0
            opcodes[0x48] = (out ushort moveAmount) =>
            {
                flags.N = false;
                moveAmount = 1;
            };
            //SETFLAG H, 1
            opcodes[0x58] = (out ushort moveAmount) =>
            {
                flags.H = true;
                moveAmount = 1;
            };
            //SETFLAG H, 0
            opcodes[0x68] = (out ushort moveAmount) =>
            {
                flags.H = false;
                moveAmount = 1;
            };
            //SETFLAG C, 1
            opcodes[0x78] = (out ushort moveAmount) =>
            {
                flags.C = true;
                moveAmount = 1;
            };
            //SETFLAG C, 0
            opcodes[0x88] = (out ushort moveAmount) =>
            {
                flags.C = false;
                moveAmount = 1;
            };
            //ADD B
            opcodes[0x04] = (out ushort moveAmount) =>
            {
                registers.B += registers.A;
                moveAmount = 1;
            };
            //ADD C
            opcodes[0x14] = (out ushort moveAmount) =>
            {
                registers.C += registers.A;
                moveAmount = 1;
            };
            //ADD D
            opcodes[0x24] = (out ushort moveAmount) =>
            {
                registers.D += registers.A;
                moveAmount = 1;
            };
            //ADD E
            opcodes[0x34] = (out ushort moveAmount) =>
            {
                registers.E += registers.A;
                moveAmount = 1;
            };
            //ADD H
            opcodes[0x44] = (out ushort moveAmount) =>
            {
                registers.H += registers.A;
                moveAmount = 1;
            };
            //ADD L
            opcodes[0x54] = (out ushort moveAmount) =>
            {
                registers.L += registers.A;
                moveAmount = 1;
            };
            //ADD (HL)
            opcodes[0x64] = (out ushort moveAmount) =>
            {
                memory[registers.HL] += registers.A;
                moveAmount = 1;
            };
            //ADD A
            opcodes[0x74] = (out ushort moveAmount) =>
            {
                registers.A += registers.A;
                moveAmount = 1;
            };
        }
    }
}

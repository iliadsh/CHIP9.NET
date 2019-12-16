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
        [FieldOffset(9)]
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
        public byte[] memory = new byte[ushort.MaxValue + 1];
        public bool[,] screen_buffer = new bool[128, 64];
        public Registers registers = new Registers();
        public Flags flags = new Flags();
        public delegate void Operate(out ushort moveAmount);
        public Operate[] opcodes = new Operate[0xFF + 1];
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
                if (registers.PC == 0x326) 
                {
                    flags.H = true;
                }
                var operation = opcodes[opcode];
                if (operation == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("SYSTEM FATAL ERROR: Unknown Opcode {0} at {1}", opcode.ToString("X2"), registers.PC.ToString("X4")));
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
                memory[registers.SP + 1] = registers.B;
                memory[registers.SP] = registers.C;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH DE
            opcodes[0x61] = (out ushort moveAmount) =>
            {
                memory[registers.SP + 1] = registers.D;
                memory[registers.SP] = registers.E;
                registers.SP -= 2;
                moveAmount = 1;
            };
            //PUSH HL
            opcodes[0x71] = (out ushort moveAmount) =>
            {
                memory[registers.SP + 1] = registers.H;
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
                flags.Z = (byte)(registers.B + registers.A) == 0;
                flags.N = ((byte)(registers.B + registers.A) & 0b10000000) != 0;
                flags.H = (registers.B & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.B > (0xFF - registers.A);

                registers.B += registers.A;
                moveAmount = 1;
            };
            //ADD C
            opcodes[0x14] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.C + registers.A) == 0;
                flags.N = ((byte)(registers.C + registers.A) & 0b10000000) != 0;
                flags.H = (registers.C & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.C > (0xFF - registers.A);

                registers.C += registers.A;
                moveAmount = 1;
            };
            //ADD D
            opcodes[0x24] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.D + registers.A) == 0;
                flags.N = ((byte)(registers.D + registers.A) & 0b10000000) != 0;
                flags.H = (registers.D & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.D > (0xFF - registers.A);

                registers.D += registers.A;
                moveAmount = 1;
            };
            //ADD E
            opcodes[0x34] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.E + registers.A) == 0;
                flags.N = ((byte)(registers.E + registers.A) & 0b10000000) != 0;
                flags.H = (registers.E & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.E > (0xFF - registers.A);

                registers.E += registers.A;
                moveAmount = 1;
            };
            //ADD H
            opcodes[0x44] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.H + registers.A) == 0;
                flags.N = ((byte)(registers.H + registers.A) & 0b10000000) != 0;
                flags.H = (registers.H & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.H > (0xFF - registers.A);

                registers.H += registers.A;
                moveAmount = 1;
            };
            //ADD L
            opcodes[0x54] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.L + registers.A) == 0;
                flags.N = ((byte)(registers.L + registers.A) & 0b10000000) != 0;
                flags.H = (registers.L & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.L > (0xFF - registers.A);

                registers.L += registers.A;
                moveAmount = 1;
            };
            //ADD (HL)
            opcodes[0x64] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(memory[registers.HL] + registers.A) == 0;
                flags.N = ((byte)(memory[registers.HL] + registers.A) & 0b10000000) != 0;
                flags.H = (memory[registers.HL] & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = memory[registers.HL] > (0xFF - registers.A);

                memory[registers.HL] += registers.A;
                moveAmount = 1;
            };
            //ADD A
            opcodes[0x74] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.A + registers.A) == 0;
                flags.N = ((byte)(registers.A + registers.A) & 0b10000000) != 0;
                flags.H = (registers.A & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.A > (0xFF - registers.A);

                registers.A += registers.A;
                moveAmount = 1;
            };
            //ADDI xx
            opcodes[0xA7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];

                flags.Z = (byte)(xx + registers.A) == 0;
                flags.N = ((byte)(xx + registers.A) & 0b10000000) != 0;
                flags.H = (xx & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = xx > (0xFF - registers.A);

                registers.A += xx;
                moveAmount = 2;
            };
            //ADDX BC
            opcodes[0x83] = (out ushort moveAmount) =>
            {
                flags.Z = (ushort)(registers.BC + registers.A) == 0;
                flags.N = ((ushort)(registers.BC + registers.A) & 0b1000000000000000) != 0;
                flags.H = (registers.BC & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.BC > (0xFFFF - registers.A);

                registers.BC += registers.A;
                moveAmount = 1;
            };
            //ADDX DE
            opcodes[0x93] = (out ushort moveAmount) =>
            {
                flags.Z = (ushort)(registers.DE + registers.A) == 0;
                flags.N = ((ushort)(registers.DE + registers.A) & 0b1000000000000000) != 0;
                flags.H = (registers.DE & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.DE > (0xFFFF - registers.A);

                registers.DE += registers.A;
                moveAmount = 1;
            };
            //ADDX HL
            opcodes[0xA3] = (out ushort moveAmount) =>
            {
                flags.Z = (ushort)(registers.HL + registers.A) == 0;
                flags.N = ((ushort)(registers.HL + registers.A) & 0b1000000000000000) != 0;
                flags.H = (registers.HL & 0xF) > (0xF - (registers.A & 0xF));
                flags.C = registers.HL > (0xFFFF - registers.A);

                registers.HL += registers.A;
                moveAmount = 1;
            };
            //SUB B
            opcodes[0x84] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.B - registers.A) == 0;
                flags.N = ((byte)(registers.B - registers.A) & 0b10000000) != 0;
                flags.H = (registers.B & 0xF) < (registers.A & 0xF);
                flags.C = registers.B < registers.A;

                registers.B -= registers.A;
                moveAmount = 1;
            };
            //SUB C
            opcodes[0x94] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.C - registers.A) == 0;
                flags.N = ((byte)(registers.C - registers.A) & 0b10000000) != 0;
                flags.H = (registers.C & 0xF) < (registers.A & 0xF);
                flags.C = registers.C < registers.A;

                registers.C -= registers.A;
                moveAmount = 1;
            };
            //SUB D
            opcodes[0xA4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.D - registers.A) == 0;
                flags.N = ((byte)(registers.D - registers.A) & 0b10000000) != 0;
                flags.H = (registers.D & 0xF) < (registers.A & 0xF);
                flags.C = registers.D < registers.A;

                registers.D -= registers.A;
                moveAmount = 1;
            };
            //SUB E
            opcodes[0xB4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.E - registers.A) == 0;
                flags.N = ((byte)(registers.E - registers.A) & 0b10000000) != 0;
                flags.H = (registers.E & 0xF) < (registers.A & 0xF);
                flags.C = registers.E < registers.A;

                registers.E -= registers.A;
                moveAmount = 1;
            };
            //SUB H
            opcodes[0xC4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.H - registers.A) == 0;
                flags.N = ((byte)(registers.H - registers.A) & 0b10000000) != 0;
                flags.H = (registers.H & 0xF) < (registers.A & 0xF);
                flags.C = registers.H < registers.A;

                registers.H -= registers.A;
                moveAmount = 1;
            };
            //SUB L
            opcodes[0xD4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.L - registers.A) == 0;
                flags.N = ((byte)(registers.L - registers.A) & 0b10000000) != 0;
                flags.H = (registers.L & 0xF) < (registers.A & 0xF);
                flags.C = registers.L < registers.A;

                registers.L -= registers.A;
                moveAmount = 1;
            };
            //SUB (HL)
            opcodes[0xE4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(memory[registers.HL] - registers.A) == 0;
                flags.N = ((byte)(memory[registers.HL] - registers.A) & 0b10000000) != 0;
                flags.H = (memory[registers.HL] & 0xF) < (registers.A & 0xF);
                flags.C = memory[registers.HL] < registers.A;

                memory[registers.HL] -= registers.A;
                moveAmount = 1;
            };
            //SUB A
            opcodes[0xF4] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.A - registers.A) == 0;
                flags.N = ((byte)(registers.A - registers.A) & 0b10000000) != 0;
                flags.H = (registers.A & 0xF) < (registers.A & 0xF);
                flags.C = registers.A < registers.A;

                registers.A -= registers.A;
                moveAmount = 1;
            };
            //SUBI xx
            opcodes[0xB7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];

                flags.Z = (byte)(registers.A - xx) == 0;
                flags.N = ((byte)(registers.A - xx) & 0b10000000) != 0;
                flags.H = (registers.A & 0xF) < (xx & 0xF);
                flags.C = registers.A < xx;

                registers.A -= xx;
                moveAmount = 2;
            };
            //INC B
            opcodes[0x03] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.B + 1) == 0;
                flags.N = ((byte)(registers.B + 1) & 0b10000000) != 0;
                flags.H = (registers.B & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.B > (0xFF - 1);

                registers.B += 1;
                moveAmount = 1;
            };
            //INC C
            opcodes[0x13] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.C + 1) == 0;
                flags.N = ((byte)(registers.C + 1) & 0b10000000) != 0;
                flags.H = (registers.C & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.C > (0xFF - 1);

                registers.C += 1;
                moveAmount = 1;
            };
            //INC D
            opcodes[0x23] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.D + 1) == 0;
                flags.N = ((byte)(registers.D + 1) & 0b10000000) != 0;
                flags.H = (registers.D & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.D > (0xFF - 1);

                registers.D += 1;
                moveAmount = 1;
            };
            //INC E
            opcodes[0x33] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.E + 1) == 0;
                flags.N = ((byte)(registers.E + 1) & 0b10000000) != 0;
                flags.H = (registers.E & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.E > (0xFF - 1);

                registers.E += 1;
                moveAmount = 1;
            };
            //INC H
            opcodes[0x43] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.H + 1) == 0;
                flags.N = ((byte)(registers.H + 1) & 0b10000000) != 0;
                flags.H = (registers.H & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.H > (0xFF - 1);

                registers.H += 1;
                moveAmount = 1;
            };
            //INC L
            opcodes[0x53] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.L + 1) == 0;
                flags.N = ((byte)(registers.L + 1) & 0b10000000) != 0;
                flags.H = (registers.L & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.L > (0xFF - 1);

                registers.L += 1;
                moveAmount = 1;
            };
            //INC (HL)
            opcodes[0x63] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(memory[registers.HL] + 1) == 0;
                flags.N = ((byte)(memory[registers.HL] + 1) & 0b10000000) != 0;
                flags.H = (memory[registers.HL] & 0xF) > (0xF - (1 & 0xF));
                flags.C = memory[registers.HL] > (0xFF - 1);

                memory[registers.HL] += 1;
                moveAmount = 1;
            };
            //INC A
            opcodes[0x73] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.A + 1) == 0;
                flags.N = ((byte)(registers.A + 1) & 0b10000000) != 0;
                flags.H = (registers.A & 0xF) > (0xF - (1 & 0xF));
                flags.C = registers.A > (0xFF - 1);

                registers.A += 1;
                moveAmount = 1;
            };
            //INX BC
            opcodes[0xA8] = (out ushort moveAmount) =>
            {
                registers.BC++;
                moveAmount = 1;
            };
            //INX DE
            opcodes[0xB8] = (out ushort moveAmount) =>
            {
                registers.DE++;
                moveAmount = 1;
            };
            //INX HL
            opcodes[0xC8] = (out ushort moveAmount) =>
            {
                registers.HL++;
                moveAmount = 1;
            };
            //DEC B
            opcodes[0x07] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.B - 1) == 0;
                flags.N = ((byte)(registers.B - 1) & 0b10000000) != 0;
                flags.H = (registers.B & 0xF) < (1 & 0xF);
                flags.C = registers.B < 1;

                registers.B -= 1;
                moveAmount = 1;
            };
            //DEC C
            opcodes[0x17] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.C - 1) == 0;
                flags.N = ((byte)(registers.C - 1) & 0b10000000) != 0;
                flags.H = (registers.C & 0xF) < (1 & 0xF);
                flags.C = registers.C < 1;

                registers.C -= 1;
                moveAmount = 1;
            };
            //DEC D
            opcodes[0x27] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.D - 1) == 0;
                flags.N = ((byte)(registers.D - 1) & 0b10000000) != 0;
                flags.H = (registers.D & 0xF) < (1 & 0xF);
                flags.C = registers.D < 1;

                registers.D -= 1;
                moveAmount = 1;
            };
            //DEC E
            opcodes[0x37] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.E - 1) == 0;
                flags.N = ((byte)(registers.E - 1) & 0b10000000) != 0;
                flags.H = (registers.E & 0xF) < (1 & 0xF);
                flags.C = registers.E < 1;

                registers.E -= 1;
                moveAmount = 1;
            };
            //DEC H
            opcodes[0x47] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.H - 1) == 0;
                flags.N = ((byte)(registers.H - 1) & 0b10000000) != 0;
                flags.H = (registers.H & 0xF) < (1 & 0xF);
                flags.C = registers.H < 1;

                registers.H -= 1;
                moveAmount = 1;
            };
            //DEC L
            opcodes[0x57] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.L - 1) == 0;
                flags.N = ((byte)(registers.L - 1) & 0b10000000) != 0;
                flags.H = (registers.L & 0xF) < (1 & 0xF);
                flags.C = registers.L < 1;

                registers.L -= 1;
                moveAmount = 1;
            };
            //DEC (HL)
            opcodes[0x67] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(memory[registers.HL] - 1) == 0;
                flags.N = ((byte)(memory[registers.HL] - 1) & 0b10000000) != 0;
                flags.H = (memory[registers.HL] & 0xF) < (1 & 0xF);
                flags.C = memory[registers.HL] < 1;

                memory[registers.HL] -= 1;
                moveAmount = 1;
            };
            //DEC A
            opcodes[0x77] = (out ushort moveAmount) =>
            {
                flags.Z = (byte)(registers.A - 1) == 0;
                flags.N = ((byte)(registers.A - 1) & 0b10000000) != 0;
                flags.H = (registers.A & 0xF) < (1 & 0xF);
                flags.C = registers.A < 1;

                registers.A -= 1;
                moveAmount = 1;
            };
            //AND B
            opcodes[0x05] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.B & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.B = result;
                moveAmount = 1;
            };
            //AND C
            opcodes[0x15] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.C & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.C = result;
                moveAmount = 1;
            };
            //AND D
            opcodes[0x25] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.D & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.D = result;
                moveAmount = 1;
            };
            //AND E
            opcodes[0x35] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.E & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.E = result;
                moveAmount = 1;
            };
            //AND H
            opcodes[0x45] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.H & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.H = result;
                moveAmount = 1;
            };
            //AND L
            opcodes[0x55] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.L & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.L = result;
                moveAmount = 1;
            };
            //AND (HL)
            opcodes[0x65] = (out ushort moveAmount) =>
            {
                byte result = (byte)(memory[registers.HL] & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                memory[registers.HL] = result;
                moveAmount = 1;
            };
            //AND A
            opcodes[0x75] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.A & registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 1;
            };
            //ANDI xx
            opcodes[0xC7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];
                byte result = (byte)(registers.A & xx);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 2;
            };
            //OR B
            opcodes[0x05] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.B | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.B = result;
                moveAmount = 1;
            };
            //OR C
            opcodes[0x15] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.C | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.C = result;
                moveAmount = 1;
            };
            //OR D
            opcodes[0x25] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.D | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.D = result;
                moveAmount = 1;
            };
            //OR E
            opcodes[0x35] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.E | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.E = result;
                moveAmount = 1;
            };
            //OR H
            opcodes[0x45] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.H | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.H = result;
                moveAmount = 1;
            };
            //OR L
            opcodes[0x55] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.L | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.L = result;
                moveAmount = 1;
            };
            //OR (HL)
            opcodes[0x65] = (out ushort moveAmount) =>
            {
                byte result = (byte)(memory[registers.HL] | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                memory[registers.HL] = result;
                moveAmount = 1;
            };
            //OR A
            opcodes[0x75] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.A | registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 1;
            };
            //ORI xx
            opcodes[0xD7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];
                byte result = (byte)(registers.A | xx);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 2;
            };
            //XOR B
            opcodes[0x06] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.B ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.B = result;
                moveAmount = 1;
            };
            //XOR C
            opcodes[0x16] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.C ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.C = result;
                moveAmount = 1;
            };
            //XOR D
            opcodes[0x26] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.D ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.D = result;
                moveAmount = 1;
            };
            //XOR E
            opcodes[0x36] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.E ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.E = result;
                moveAmount = 1;
            };
            //XOR H
            opcodes[0x46] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.H ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.H = result;
                moveAmount = 1;
            };
            //XOR L
            opcodes[0x56] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.L ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.L = result;
                moveAmount = 1;
            };
            //XOR (HL)
            opcodes[0x66] = (out ushort moveAmount) =>
            {
                byte result = (byte)(memory[registers.HL] ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                memory[registers.HL] = result;
                moveAmount = 1;
            };
            //XOR A
            opcodes[0x76] = (out ushort moveAmount) =>
            {
                byte result = (byte)(registers.A ^ registers.A);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 1;
            };
            //XORI xx
            opcodes[0xE7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];
                byte result = (byte)(registers.A ^ xx);

                flags.Z = result == 0;
                flags.N = (result & 0b10000000) != 0;
                flags.H = false;
                flags.C = false;

                registers.A = result;
                moveAmount = 2;
            };
            //CMP B
            opcodes[0x86] = (out ushort moveAmount) =>
            {
                flags.Z = registers.B == registers.A;
                flags.N = registers.B < registers.A;
                moveAmount = 1;
            };
            //CMP C
            opcodes[0x96] = (out ushort moveAmount) =>
            {
                flags.Z = registers.C == registers.A;
                flags.N = registers.C < registers.A;
                moveAmount = 1;
            };
            //CMP D
            opcodes[0xA6] = (out ushort moveAmount) =>
            {
                flags.Z = registers.D == registers.A;
                flags.N = registers.D < registers.A;
                moveAmount = 1;
            };
            //CMP E
            opcodes[0xB6] = (out ushort moveAmount) =>
            {
                flags.Z = registers.E == registers.A;
                flags.N = registers.E < registers.A;
                moveAmount = 1;
            };
            //CMP H
            opcodes[0xC6] = (out ushort moveAmount) =>
            {
                flags.Z = registers.H == registers.A;
                flags.N = registers.H < registers.A;
                moveAmount = 1;
            };
            //CMP L
            opcodes[0xD6] = (out ushort moveAmount) =>
            {
                flags.Z = registers.L == registers.A;
                flags.N = registers.L < registers.A;
                moveAmount = 1;
            };
            //CMP (HL)
            opcodes[0xE6] = (out ushort moveAmount) =>
            {
                flags.Z = memory[registers.HL] == registers.A;
                flags.N = memory[registers.HL] < registers.A;
                moveAmount = 1;
            };
            //CMP A
            opcodes[0xF6] = (out ushort moveAmount) =>
            {
                flags.Z = registers.A == registers.A;
                flags.N = registers.A < registers.A;
                moveAmount = 1;
            };
            //CMPI A xx
            opcodes[0xF7] = (out ushort moveAmount) =>
            {
                byte xx = memory[registers.PC + 1];
                flags.Z = registers.A == xx;
                flags.N = registers.A < xx;
                moveAmount = 2;
            };
            //CMPS B
            opcodes[0x0D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.B == registers.A;
                flags.N = registers.B < registers.A;
                moveAmount = 1;
            };
            //CMPS C
            opcodes[0x1D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.C == registers.A;
                flags.N = registers.C < registers.A;
                moveAmount = 1;
            };
            //CMPS D
            opcodes[0x2D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.D == registers.A;
                flags.N = (sbyte)registers.D < (sbyte)registers.A;
                moveAmount = 1;
            };
            //CMPS E
            opcodes[0x3D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.E == registers.A;
                flags.N = (sbyte)registers.E < (sbyte)registers.A;
                moveAmount = 1;
            };
            //CMPS H
            opcodes[0x4D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.H == registers.A;
                flags.N = (sbyte)registers.H < (sbyte)registers.A;
                moveAmount = 1;
            };
            //CMPS L
            opcodes[0x5D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.L == registers.A;
                flags.N = (sbyte)registers.L < (sbyte)registers.A;
                moveAmount = 1;
            };
            //CMPS (HL)
            opcodes[0x6D] = (out ushort moveAmount) =>
            {
                flags.Z = memory[registers.HL] == registers.A;
                flags.N = (sbyte)memory[registers.HL] < (sbyte)registers.A;
                moveAmount = 1;
            };
            //CMPS A
            opcodes[0x7D] = (out ushort moveAmount) =>
            {
                flags.Z = registers.A == registers.A;
                flags.N = (sbyte)registers.A < (sbyte)registers.A;
                moveAmount = 1;
            };
            //SIN
            opcodes[0xE0] = (out ushort moveAmount) =>
            {
                registers.A = (byte)Console.Read();
                moveAmount = 1;
            };
            //SOUT
            opcodes[0xE1] = (out ushort moveAmount) =>
            {
                Console.Write((char)registers.A);
                moveAmount = 1;
            };
            //CLRSCR
            opcodes[0xF0] = (out ushort moveAmount) =>
            {
                Thread.Sleep(100);
                Array.Clear(screen_buffer, 0, screen_buffer.Length);
                moveAmount = 1;
            };
            //DRAW
            opcodes[0xF1] = (out ushort moveAmount) =>
            {
                byte line = registers.A;
                sbyte x = (sbyte)registers.C;
                sbyte y = (sbyte)registers.B;
                for (int i = 7; i >= 0; i--)
                {
                    byte lx = (byte)(7 - i);
                    if (x + lx < 0 || x + lx > 127) continue;
                    bool state = (line & (1 << i)) >> i == 1;
                    screen_buffer[x + lx, y] = state;
                }
                moveAmount = 1;
            };
            //JMP xxyy
            opcodes[0x0F] = (out ushort moveAmount) =>
            {
                ushort val = 0;
                val |= memory[registers.PC + 1];
                val |= (ushort)(memory[registers.PC + 2] << 8);
                registers.PC = val;
                moveAmount = 0;
            };
            //JMPZ xxyy
            opcodes[0x1F] = (out ushort moveAmount) =>
            {
                if (flags.Z)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPNZ xxyy
            opcodes[0x2F] = (out ushort moveAmount) =>
            {
                if (!flags.Z)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPN xxyy
            opcodes[0x3F] = (out ushort moveAmount) =>
            {
                if (flags.N)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPNN xxyy
            opcodes[0x4F] = (out ushort moveAmount) =>
            {
                if (!flags.N)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPH xxyy
            opcodes[0x5F] = (out ushort moveAmount) =>
            {
                if (flags.H)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPNH xxyy
            opcodes[0x6F] = (out ushort moveAmount) =>
            {
                if (!flags.H)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPC xxyy
            opcodes[0x7F] = (out ushort moveAmount) =>
            {
                if (flags.C)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMPNC xxyy
            opcodes[0x8F] = (out ushort moveAmount) =>
            {
                if (!flags.C)
                {
                    ushort val = 0;
                    val |= memory[registers.PC + 1];
                    val |= (ushort)(memory[registers.PC + 2] << 8);
                    registers.PC = val;
                    moveAmount = 0;
                }
                else
                {
                    moveAmount = 3;
                }
            };
            //JMP xx
            opcodes[0x9F] = (out ushort moveAmount) =>
            {
                sbyte xx = (sbyte)memory[registers.PC + 1];
                registers.PC = (ushort)(registers.PC + xx);
                moveAmount = 2;
            };
            //JMPZ xx
            opcodes[0xAF] = (out ushort moveAmount) =>
            {
                if (flags.Z)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPNZ xx
            opcodes[0xBF] = (out ushort moveAmount) =>
            {
                if (!flags.Z)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPN xx
            opcodes[0xCF] = (out ushort moveAmount) =>
            {
                if (flags.N)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPNN xx
            opcodes[0xDF] = (out ushort moveAmount) =>
            {
                if (!flags.N)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPH xx
            opcodes[0xEF] = (out ushort moveAmount) =>
            {
                if (flags.H)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPNH xx
            opcodes[0xFF] = (out ushort moveAmount) =>
            {
                if (!flags.H)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPC xx
            opcodes[0xEE] = (out ushort moveAmount) =>
            {
                if (flags.C)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //JMPNC xx
            opcodes[0xFE] = (out ushort moveAmount) =>
            {
                if (!flags.C)
                {
                    sbyte xx = (sbyte)memory[registers.PC + 1];
                    registers.PC = (ushort)(registers.PC + xx);
                }
                moveAmount = 2;
            };
            //CALL xxyy
            opcodes[0x1E] = (out ushort moveAmount) =>
            {
                ushort newPC = (ushort)(registers.PC + 3);
                memory[registers.SP + 1] = (byte)((newPC & 0xFF00) >> 8);
                memory[registers.SP] = (byte)(newPC & 0xFF);
                registers.SP -= 2;

                ushort val = 0;
                val |= memory[registers.PC + 1];
                val |= (ushort)(memory[registers.PC + 2] << 8);
                registers.PC = val;
                moveAmount = 0;
            };
            //RET
            opcodes[0x0E] = (out ushort moveAmount) =>
            {
                registers.SP += 2;
                ushort val = 0;
                val |= (ushort)(memory[registers.SP + 1] << 8);
                val |= memory[registers.SP];
                registers.PC = val;
                moveAmount = 0;
            };
            //NOP
            opcodes[0x00] = (out ushort moveAmount) =>
            {
                moveAmount = 1;
            };
        }
    }
}

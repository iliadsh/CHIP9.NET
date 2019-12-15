using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHIP9.NET
{
    public partial class Screen : Form
    {
        CPU cpu;
        public Screen()
        {
            InitializeComponent();
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            KeyDown += Screen_KeyDown;
            KeyUp += Screen_KeyUp;
            Paint += Screen_Paint;
            cpu = new CPU();
            cpu.Run();
        }

        private void Screen_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            SolidBrush brush = new SolidBrush(Color.White);
            int width = Width / 128;
            int height = Height / 64;
            for(int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    if (cpu.screen_buffer[x, y])
                    {
                        Rectangle rect = new Rectangle(x * width, y * height, width, height);
                        e.Graphics.FillRectangle(brush, rect);
                    }
                }
            }
            brush.Dispose();
            Refresh();
        }

        private void Screen_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    cpu.memory[0xF000] &= 0b01111111;
                    break;
                case Keys.A:
                    cpu.memory[0xF000] &= 0b10111111;
                    break;
                case Keys.S:
                    cpu.memory[0xF000] &= 0b11011111;
                    break;
                case Keys.D:
                    cpu.memory[0xF000] &= 0b11101111;
                    break;
                case Keys.K:
                    cpu.memory[0xF000] &= 0b11110111;
                    break;
                case Keys.L:
                    cpu.memory[0xF000] &= 0b11111011;
                    break;
                case Keys.LShiftKey:
                    cpu.memory[0xF000] &= 0b11111101;
                    break;
                case Keys.LControlKey:
                    cpu.memory[0xF000] &= 0b11111110;
                    break;
                default:
                    break;
            }
        }

        private void Screen_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    cpu.memory[0xF000] |= 0b10000000;
                    break;
                case Keys.A:
                    cpu.memory[0xF000] |= 0b01000000;
                    break;
                case Keys.S:
                    cpu.memory[0xF000] |= 0b00100000;
                    break;
                case Keys.D:
                    cpu.memory[0xF000] |= 0b00010000;
                    break;
                case Keys.K:
                    cpu.memory[0xF000] |= 0b00001000;
                    break;
                case Keys.L:
                    cpu.memory[0xF000] |= 0b00000100;
                    break;
                case Keys.LShiftKey:
                    cpu.memory[0xF000] |= 0b00000010;
                    break;
                case Keys.LControlKey:
                    cpu.memory[0xF000] |= 0b00000010;
                    break;
                default:
                    break;
            }
        }
    }
}

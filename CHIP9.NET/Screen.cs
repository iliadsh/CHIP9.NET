using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Threading;

namespace CHIP9.NET
{
    public sealed class Screen : GameWindow
    {
        private CPU cpu;
        private int fpsCounter = 0;
        private int fps = 0;
        private Thread fpsThread;

        public Screen()
            :base(1000,
                 500,
                 GraphicsMode.Default,
                 "CHIP-9",
                 GameWindowFlags.Default,
                 DisplayDevice.Default,
                 2,
                 0,
                 GraphicsContextFlags.ForwardCompatible)
        {
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }
        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            VSync = VSyncMode.Off;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            fpsThread = new Thread(new ThreadStart(() => {
                while (true)
                {
                    fps = fpsCounter;
                    fpsCounter = 0;
                    Thread.Sleep(1000);
                }
            }));
            fpsThread.Start();
            cpu = new CPU();
            cpu.Run();
        }
        protected override void OnClosed(EventArgs e)
        {
            cpu.execThread.Abort();
            fpsThread.Abort();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HandleKeyboard();
            fpsCounter++;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"CHIP-9 | FPS: {fps}";
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(1f, 1f, 1f, 1f);

            float width = (float)2 / 128;
            float height = (float)2 / 64;
            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    if (cpu.screen_buffer[x, y])
                    {
                        GL.Vertex2(x * width - 1, 1 - y * height);
                        GL.Vertex2(x * width - 1 + width, 1 - y * height);
                        GL.Vertex2(x * width - 1 + width, 1 - y * height - height);
                        GL.Vertex2(x * width - 1, 1 - y * height - height);
                    }
                }
            }

            GL.End();
            SwapBuffers();
        }
        private void HandleKeyboard()
        {
            var keystate = Keyboard.GetState();

            if(keystate.IsKeyDown(Key.W))
            {
                cpu.memory[0xF000] |= 0b10000000;
            } 
            else
            {
                cpu.memory[0xF000] &= 0b01111111;
            }
            if(keystate.IsKeyDown(Key.A))
            {
                cpu.memory[0xF000] |= 0b01000000;
            }
            else
            {
                cpu.memory[0xF000] &= 0b10111111;
            }
            if(keystate.IsKeyDown(Key.S))
            {
                cpu.memory[0xF000] |= 0b00100000;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11011111;
            }
            if(keystate.IsKeyDown(Key.D))
            {
                cpu.memory[0xF000] |= 0b00010000;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11101111;
            }
            if(keystate.IsKeyDown(Key.K))
            {
                cpu.memory[0xF000] |= 0b00001000;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11110111;
            }
            if(keystate.IsKeyDown(Key.L))
            {
                cpu.memory[0xF000] |= 0b00000100;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11111011;
            }
            if(keystate.IsKeyDown(Key.ShiftLeft))
            {
                cpu.memory[0xF000] |= 0b00000010;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11111101;
            }
            if(keystate.IsKeyDown(Key.ControlLeft))
            {
                cpu.memory[0xF000] |= 0b00000001;
            }
            else
            {
                cpu.memory[0xF000] &= 0b11111110;
            }
        }
    }
}

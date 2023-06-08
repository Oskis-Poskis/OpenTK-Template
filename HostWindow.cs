using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Window.Common;
using Window.Rendering;
using Window.Helper;

using System.Runtime.InteropServices;

namespace Window
{
    class Window : GameWindow
    {
        unsafe public Window(NativeWindowSettings settings) : base(GameWindowSettings.Default, settings)
        {
            CenterWindow();
            
            state.LoadState(WindowPtr);
            size = new(state.properties.width, state.properties.height);
            Title = state.properties.title;
            WindowShader = new Shader(base_path + "Shaders/window.vert", base_path + "Shaders/window.frag", base_path + "Shaders/window.geom");
            TextShader = new Shader(base_path + "Shaders/text.vert", base_path + "Shaders/text.frag");

            GUIWindow window1 = new GUIWindow($"Window {1}", new(size.X / 2, size.Y / 2), new(size.X / 4, size.Y / 4));
            GUIWindow window2 = new GUIWindow($"Window {2}", new(size.X / 2, size.Y / 2), new(size.X / 4, size.Y / 4));
            GUIWindow window3 = new GUIWindow($"Window {3}", new(size.X / 2, size.Y / 2), new(size.X / 4, size.Y / 4));
            GUIWindow window4 = new GUIWindow($"Window {4}", new(size.X / 2, size.Y / 2), new(size.X / 4, size.Y / 4));

            windows = new List<GUIWindow> { window1, window2, window3, window4 };
        }

        StatCounter stats = new();
        public static Vector2 mouse_pos;
        public static Vector2i size;

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        public static Shader WindowShader, TextShader;
        public static List<GUIWindow> windows;
        int activeIndex = 0;
        WindowSaveState state = new WindowSaveState(new WindowProperties());

        Text text;

        private static void OnDebugMessage(
            DebugSource source,
            DebugType type,
            int id,
            DebugSeverity severity,
            int length,
            IntPtr pMessage,
            IntPtr pUserParam)
        {
            string message = Marshal.PtrToStringAnsi(pMessage, length);
            Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
            if (type == DebugType.DebugTypeError)
            {
                throw new Exception(message);
            }
        }

        private static DebugProc DebugMessageDelegate = OnDebugMessage;
        unsafe protected override void OnLoad()
        {
            base.OnLoad();

            MakeCurrent();
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            VSync = VSyncMode.Off;
            text = new Text("testttt", new(-1, 1), 30);
            
            IsVisible = true;
        }

        unsafe protected override void OnUnload()
        {
            base.OnUnload();
            
            state.SaveState(WindowPtr);
        }

        unsafe protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            mouse_pos.X = MouseState.Position.X;
            mouse_pos.Y = MouseState.Position.Y;

            bool leftDown = IsMouseButtonDown(MouseButton.Button1);
            bool leftPress = IsMouseButtonPressed(MouseButton.Button1);
            bool leftReleased = IsMouseButtonReleased(MouseButton.Button1);
            bool altDown = IsKeyDown(Keys.LeftAlt);

            for (int i = 0; i < windows.Count; i++)
            {
                if (leftPress &&
                    windows[i].IsWindowHovered() &&
                    !windows[activeIndex].IsResizing &&
                    !windows[activeIndex].IsMoving &&
                    !windows[activeIndex].IsWindowHovered())
                {
                    activeIndex = i;
                    Console.WriteLine("Selected: " + windows[i].Title);
                }

                if (activeIndex == i)
                {
                    windows[i].TransformWindow(leftDown, leftPress, leftReleased, altDown);
                    Cursor = windows[i].cursor;
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            Render();
            stats.Count(args);
            Title = stats.fps.ToString("0.0") + " | " + stats.ms.ToString("0.00");
        }

        unsafe protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            state.Resize(e.Width, e.Height);
            size = new(e.Width, e.Height);

            if (WindowShader != null && windows != null)
            {
                Render();
                foreach(GUIWindow window in windows)
                {
                    window.UpdateVertices();
                }

                if (text != null) text.UpdateVertices(new(-1, 1), 30);
            }
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.75f, 0.75f, 0.75f, 1);

            WindowShader.Use();
            for (int i = 0; i < windows.Count; i++)
            {
                if (i == activeIndex)
                {
                    WindowShader.SetVector3("shade", new(0.75f));
                    windows[i].z_index = 1;
                }
                else
                {
                    WindowShader.SetVector3("shade", new(0.5f));
                    windows[i].z_index = 0;
                }
                windows[i].Render(MouseState, i == activeIndex);
            }
            text.Render(-1, 1, 0.3f);

            SwapBuffers();
        }

        bool enter_fullscreen = false;
        unsafe protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Keys.F11)
            {
                enter_fullscreen = HelperClass.ToggleBool(enter_fullscreen);
                if (enter_fullscreen) GLFW.MaximizeWindow(WindowPtr);
                else GLFW.RestoreWindow(WindowPtr);
            }

            else if (e.Key == Keys.KeyPad1) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            else if (e.Key == Keys.KeyPad2) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }
    }
}

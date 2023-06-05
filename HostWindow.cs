using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Window.Common;
using Window.Rendering;
using Window.Helper;

using System.Runtime.InteropServices;

namespace Window
{
    class Window : GameWindow
    {
        unsafe public Window(NativeWindowSettings win) : base(GameWindowSettings.Default, win)
        {
            CenterWindow();
            
            state.LoadState(WindowPtr);
            size = new(state.properties.width, state.properties.height);
            Title = state.properties.title;

            GUIWindow_S = new Shader(base_path + "Shaders/window.vert", base_path + "Shaders/window.frag", base_path + "Shaders/window.geom");

            windows = new List<GUIWindow>();
            for (int i = 0; i < 4; i++)
            {
                GUIWindow window = new GUIWindow($"Window {i}", new(size.X / 2, size.Y / 2), new(size.X / 4, size.Y / 4));
                if (i == 1) window.settings.collapsable = false;
                windows.Add(window);
            }
        }

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        State state = new State(new WindowSaveState());

        Helper.FPScounter stats = new();
        public static Vector2i size;
        public static Vector2 mouse_pos;

        public static Shader GUIWindow_S;
        public static List<GUIWindow> windows;
        int activeIndex = 0;

        private static void OnDebugMessage(
            DebugSource source,     // Source of the debugging message.
            DebugType type,         // Type of the debugging message.
            int id,                 // ID associated with the message.
            DebugSeverity severity, // Severity of the message.
            int length,             // Length of the string in pMessage.
            IntPtr pMessage,        // Pointer to message string.
            IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
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
            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            VSync = VSyncMode.Off;
            
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

            if (GUIWindow_S != null && windows != null)
            {
                Render();
                foreach(GUIWindow window in windows) window.UpdateVertices();
            }
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.75f, 0.75f, 0.75f, 1);

            GUIWindow_S.Use();
            for (int i = 0; i < windows.Count; i++)
            {
                if (i == activeIndex)
                {
                    GUIWindow_S.SetVector3("shade", new(0.75f));
                    windows[i].z_index = 1;
                }
                else
                {
                    GUIWindow_S.SetVector3("shade", new(0.5f));
                    windows[i].z_index = 0;
                }
                windows[i].Render(MouseState, i == activeIndex);
            }

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

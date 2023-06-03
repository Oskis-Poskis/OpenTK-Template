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
        public Window(NativeWindowSettings win) : base(GameWindowSettings.Default, win)
        {
            CenterWindow();
            GUIWindow_S = new Shader(base_path + "Shaders/window.vert", base_path + "Shaders/window.frag", base_path + "Shaders/window.geom");
            size = win.Size;

            GUIWindow window1 = new GUIWindow("Window1", 500, 500, Vector2.Zero);
            GUIWindow window2 = new GUIWindow("Window2", 500, 500, Vector2.Zero);
            GUIWindow window3 = new GUIWindow("Window3", 500, 500, Vector2.Zero);

            windows = new List<GUIWindow>();
            windows.Add(window1);
            windows.Add(window2);
            windows.Add(window3);
        }

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        State state = new State(new WindowSaveState());

        Helper.FPScounter stats = new();
        public static Vector2i size;
        public static Vector2 mouse_pos;

        public static Shader GUIWindow_S;
        List<GUIWindow> windows;
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
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.
            Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);

            // Potentially, you may want to throw from the function for certain severity
            // messages.
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

            Title = state.properties.title;

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

            mouse_pos.X = Helper.HelperClass.MapRange(MouseState.Position.X, 0, state.properties.width, -1.0f, 1.0f);
            mouse_pos.Y = Helper.HelperClass.MapRange(MouseState.Position.Y, 0, state.properties.height, 1.0f, -1.0f);

            bool leftDown = IsMouseButtonDown(MouseButton.Button1);
            bool altDown = IsKeyDown(Keys.LeftAlt);

            for (int i = 0; i < windows.Count; i++)
            {
                if (IsMouseButtonPressed(MouseButton.Button1) &&
                    windows[i].IsWindowHovered() &&
                    !windows[activeIndex].IsResizing &&
                    !windows[activeIndex].IsMoving &&
                    !windows[activeIndex].IsWindowHovered())
                {
                    activeIndex = i;
                }

                if (i == activeIndex)
                {
                    windows[i].TransformWindow(leftDown, altDown);
                    Cursor = windows[i].cursor;
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            Render();
            stats.Count(args);
            Title = stats.fps.ToString("0.0");
        }

        unsafe protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            state.Resize(e.Width, e.Height);
            size = new(e.Width, e.Height);

            Render();
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
                    GUIWindow_S.SetFloat("index", 1.0f);
                }
                else
                {
                    GUIWindow_S.SetVector3("shade", new(0.5f));
                    GUIWindow_S.SetFloat("index", 0.0f);
                }
                windows[i].Render(MouseState);
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
        }
    }
}

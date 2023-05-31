using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

using Window.Rendering;
using Window.Common;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;

namespace Window
{
    class Window : GameWindow
    {
        public Window(NativeWindowSettings win) : base(GameWindowSettings.Default, win)
        {
            CenterWindow();
            GUIWindow_S = new Shader(base_path + "Shaders/window.vert", base_path + "Shaders/window.frag");
        }

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        State state = new State(new WindowSaveState());
        public static Vector2 mpos;

        public static GUIWindow window;
        Shader GUIWindow_S;

        Helper.FPScounter stats = new();

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
            GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
            // VSync = VSyncMode.On;

            state.LoadState(WindowPtr);
            Title = state.properties.title;
            window = new GUIWindow();

            IsVisible = true;

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        unsafe protected override void OnUnload()
        {
            base.OnUnload();
            
            state.SaveState(WindowPtr);
        }

        unsafe protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            mpos.X = Helper.HelperClass.MapRange(MouseState.Position.X, 0, state.properties.width, -1.0f, 1.0f);
            mpos.Y = Helper.HelperClass.MapRange(MouseState.Position.Y, 0, state.properties.height, 1.0f, -1.0f);

            if (window.IsWindowHovered() | window.isResizing)
            {
                window.TransformWindow(IsMouseButtonDown(MouseButton.Button1));
                Cursor = window.cursor;
            }
            else Cursor = MouseCursor.Default;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            Render();
            stats.Count(args);
            Title = stats.fps.ToString("0.0");
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            state.Resize(e.Width, e.Height);

            if (window != null) Render();
        }

        public void Render()
        {
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GUIWindow_S.Use();
            window.Render(MouseState, state.properties.width, state.properties.height);

            SwapBuffers();
        }

        bool enter_fullscreen = false;
        unsafe protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Keys.F11)
            {
                enter_fullscreen = Helper.HelperClass.ToggleBool(enter_fullscreen);
                if (enter_fullscreen) GLFW.MaximizeWindow(WindowPtr);
                else GLFW.RestoreWindow(WindowPtr);
            }
        }
    }
}

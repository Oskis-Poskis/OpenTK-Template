using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace Window
{
    class Window : GameWindow
    {
        public Window(NativeWindowSettings win) : base(GameWindowSettings.Default, win)
        {
            CenterWindow();
        }

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        State state = new State(new WindowSaveState());
        
        unsafe protected override void OnLoad()
        {
            base.OnLoad();

            MakeCurrent();
            state.LoadState(WindowPtr);
            Title = state.properties.title;

            IsVisible = true;
        }

        unsafe protected override void OnUnload()
        {
            base.OnUnload();
            
            state.SaveState(WindowPtr);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            Render();
        }

        public void Render()
        {
            GL.ClearColor(1, 1, 1, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            Render();
            GL.Viewport(0, 0, e.Width, e.Height);
            state.Resize(e.Width, e.Height);
        }
    }
}

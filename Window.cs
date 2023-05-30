using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Window
{
    class Window : GameWindow
    {
        public Window(NativeWindowSettings win) : base(GameWindowSettings.Default, win)
        {
            CenterWindow();
        }

        public static string base_path = AppDomain.CurrentDomain.BaseDirectory;
        WinState win_props;
        
        protected override void OnLoad()
        {
            base.OnLoad();

            IsVisible = true;
            win_props = new WinState(new WindowProperties(Title, Size.X, Size.Y));
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            win_props.SaveState();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }
    }
}

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Modine
{
    class Window : GameWindow
    {
        public Window(NativeWindowSettings window_settings) : base(GameWindowSettings.Default, window_settings)
        {
            CenterWindow();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            IsVisible = true;
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

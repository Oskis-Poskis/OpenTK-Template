using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

using WindowTemplate;
using WindowTemplate.Rendering;

namespace TestingProject
{
    public class TestingClass : HostWindow
    {
        public TestingClass(NativeWindowSettings settings) : base(settings)
        {
            GUIWindow window1 = new GUIWindow($"Window {1}", new(HostWindow.size.X / 2 - 100, (int)(HostWindow.size.Y * 0.65f)), new(0));
            GUIWindow window2 = new GUIWindow($"Window {2}", new(HostWindow.size.X / 2 - 100, (int)(HostWindow.size.Y * 0.65f)), new(HostWindow.size.X / 2, 0));

            windows = new List<GUIWindow> { window2, window1 };
        }

        public static List<GUIWindow> windows;

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            UpdateUI(windows);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            RenderUI(windows);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            ResizeUI(windows);
        }
    }
}
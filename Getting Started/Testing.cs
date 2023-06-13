using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using WindowTemplate;

namespace TestingProject
{
    public class TestingClass : HostWindow
    {
        public TestingClass(NativeWindowSettings settings) : base(settings)
        {
            
        }

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
        }
    }
}
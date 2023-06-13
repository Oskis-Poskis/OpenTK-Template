using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TestingProject
{
    class Program
    {
        static void Main()
        {
            NativeWindowSettings window_settings = new()
            {
                Vsync = VSyncMode.On,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.Debug
            };

            TestingClass window = new TestingClass(window_settings);
            window.Run();
        }
    }
}
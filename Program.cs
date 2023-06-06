using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Window
{
    class Program
    {
        static void Main()
        {
            NativeWindowSettings window_settings = new()
            {
                WindowBorder = WindowBorder.Resizable,
                StartVisible = false,
                StartFocused = true,
                WindowState = WindowState.Normal,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.Debug
            };

            Window window = new Window(window_settings);
            window.Run();
        }
    }
}
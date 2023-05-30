using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Modine
{
    class Program
    {
        static void Main()
        {
            NativeWindowSettings window_settings = new()
            {
                Title = "Template",
                Size = new Vector2i(1200, 700),
                WindowBorder = WindowBorder.Resizable,
                StartVisible = false,
                StartFocused = true,
                WindowState = WindowState.Normal,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.Debug
            };

            using Window window = new Window(window_settings);
            window.Run();
        }
    }
}
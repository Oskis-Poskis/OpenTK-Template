using System.Text.Json;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Window
{
    [Serializable]
    public struct WindowSaveState
    {
        public string title { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int positionx { get; set; }
        public int positiony { get; set; }
        public bool maximized { get; set; }

        public WindowSaveState()
        {
            this.title = "Template";
            this.width = 1200;
            this.height = 800;
            this.positionx = 0;
            this.positiony = 0;
            this.maximized = false;
        }
    }

    public class State
    {
        public WindowSaveState properties = new();
        public State(WindowSaveState Properties)
        {
            this.properties = Properties;
        }

        JsonSerializerOptions settings = new JsonSerializerOptions{ WriteIndented = true };
        string save_path = Window.base_path + "Save/windowstate.txt";

        unsafe public void SaveState(OpenTK.Windowing.GraphicsLibraryFramework.Window* WindowPtr)
        {
            if (Path.Exists(Window.base_path + "Save/windowstate.txt"))
            {
                GLFW.GetWindowSize(WindowPtr, out int width, out int height);
                properties.width = width;
                properties.height = height;

                GLFW.GetWindowPos(WindowPtr, out int x, out int y);
                properties.positionx = x;
                properties.positiony = y;

                properties.maximized = GLFW.GetWindowAttrib(WindowPtr, WindowAttributeGetBool.Maximized);

                string save_file = JsonSerializer.Serialize(properties, settings);
                using (StreamWriter writer = new StreamWriter(save_path))
                {
                    writer.Write(save_file);
                }
            }

            else Console.WriteLine("Path does not exist for Window State save.\n" + save_path);
        }

        unsafe public void LoadState(OpenTK.Windowing.GraphicsLibraryFramework.Window* WindowPtr)
        {
            if (Path.Exists(save_path))
            {
                string json = File.ReadAllText(save_path);
                WindowSaveState loaded_state = JsonSerializer.Deserialize<WindowSaveState>(json);
                if (loaded_state.width != 0 && loaded_state.height != 0)
                {
                    properties = loaded_state;
                }

                else
                {
                    properties = loaded_state;
                    properties.width = 500;
                    properties.height = 500;
                }
            }

            else
            {
                properties.positionx = (int)(properties.width / 2);
                properties.positiony = (int)(properties.height / 2);
            }

            GLFW.SetWindowSize(WindowPtr, properties.width, properties.height);
            GLFW.SetWindowPos(WindowPtr, properties.positionx, properties.positiony);
            if (properties.maximized) GLFW.MaximizeWindow(WindowPtr);
        }

        public void Resize(int Width, int Height)
        {
            this.properties.width = Width;
            this.properties.height = Height;
        }
    }
}
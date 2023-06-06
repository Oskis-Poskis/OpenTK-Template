using System.Text.Json;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Window
{
    [Serializable]
    public struct WindowProperties
    {
        public string title { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int positionx { get; set; }
        public int positiony { get; set; }
        public bool maximized { get; set; }

        public WindowProperties()
        {
            this.title = "Template";
            this.width = 1200;
            this.height = 800;
            this.positionx = 0;
            this.positiony = 0;
            this.maximized = false;
        }
    }

    public class WindowSaveState
    {
        public WindowProperties properties = new();
        public WindowSaveState(WindowProperties Properties)
        {
            this.properties = Properties;
        }

        JsonSerializerOptions settings = new JsonSerializerOptions{ WriteIndented = true };
        string save_path = Window.base_path + "Save/windowstate.txt";

        unsafe public void SaveState(OpenTK.Windowing.GraphicsLibraryFramework.Window* WindowPtr)
        {
            if (Path.Exists(Window.base_path + "Save/windowstate.txt"))
            {
                Save();
            }

            else
            {
                Directory.CreateDirectory(Window.base_path + "Save");
                Save();
            }

            void Save()
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
        }

        unsafe public void LoadState(OpenTK.Windowing.GraphicsLibraryFramework.Window* WindowPtr)
        {
            if (Path.Exists(save_path))
            {
                string json = File.ReadAllText(save_path);
                WindowProperties loaded_state = JsonSerializer.Deserialize<WindowProperties>(json);
                if (loaded_state.width != 0 && loaded_state.height != 0)
                {
                    properties = loaded_state;
                    Console.WriteLine("Succesfully loaded saved window state");
                    Console.WriteLine(json);
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
                properties.title = "Template";
                properties.positionx = (int)(properties.width / 2);
                properties.positiony = (int)(properties.height / 2);
                Console.WriteLine("Window state file path does not exist:\n" + save_path);
            }

            GLFW.SetWindowTitle(WindowPtr, properties.title);
            GLFW.SetWindowSize(WindowPtr, properties.width, properties.height);
            GLFW.SetWindowPos(WindowPtr, properties.positionx, properties.positiony);
            if (properties.maximized)
            {
                GLFW.MaximizeWindow(WindowPtr);
                Console.WriteLine("Maximized Window");
            }
            
        }

        public void Resize(int Width, int Height)
        {
            this.properties.width = Width;
            this.properties.height = Height;
        }
    }
}
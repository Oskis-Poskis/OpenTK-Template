using System.Text.Json;

namespace Window
{
    [Serializable]
    public struct WindowProperties
    {
        public string name { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public WindowProperties(string Name, int Width, int Height)
        {
            this.name = Name;
            this.width = Width;
            this.height = Height;
        }
    }

    public class WinState
    {
        public WindowProperties properties = new();
        public WinState(WindowProperties Properties)
        {
            this.properties = Properties;
        }

        JsonSerializerOptions settings = new JsonSerializerOptions{ WriteIndented = true };
        string save_path = Window.base_path + "Save/window_properties.test";

        public void SaveState()
        {
            if (Path.Exists(Window.base_path + "Save/window_properties.test"))
            {
                string save_file = JsonSerializer.Serialize(properties, settings);
                using (StreamWriter writer = new StreamWriter(save_path))
                {
                    writer.Write(save_file);
                }

                Console.WriteLine(save_file);
            }

            else Console.WriteLine("Path does not exist for window properties save.\n" + save_path);
        }

        public void LoadState()
        {

        }
    }
}
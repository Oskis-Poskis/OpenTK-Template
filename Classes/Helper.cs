

using OpenTK.Windowing.Common;

namespace WindowTemplate.Helper
{
    public class HelperClass
    {
        public static bool ToggleBool(bool toggleBool)
        {
            bool _bool = false;

            if (toggleBool == true) _bool = false;
            if (toggleBool == false) _bool = true;

            return _bool;
        }

        public static float MapRange(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return ((value - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;
        }

        public static float RandFloat()
        {
            Random rand = new Random();
            return (float)rand.NextDouble() * 2 - 1;
        }
    }

    public class StatCounter
    {
        public int frameCount = 0;
        public double elapsedTime = 0.0, fps = 0.0, ms;

        public void Count(FrameEventArgs args)
        {
            frameCount++;
            elapsedTime += args.Time;
            if (elapsedTime >= 1f)
            {
                fps = frameCount / elapsedTime;
                ms = 1000 * elapsedTime / frameCount;
                frameCount = 0;
                elapsedTime = 0.0;
            }
        }
    }
}
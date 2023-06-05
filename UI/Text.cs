using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

namespace Window.Rendering
{
    public class Text
    {
        string text;
        private int vaoHandle, vboHandle;
        private float[] text_vertices;
        public Vector2 start_pos;
        public float textHeight;

        public Text(string text, Vector2 StartPosition, float height)
        {
            this.text = text;
            textHeight = height;
            start_pos = StartPosition;
            int character_count = text.Length;

            text_vertices = new float[]
            {
                start_pos.X, start_pos.Y - height,
                start_pos.X, start_pos.Y,
                start_pos.X + height, start_pos.Y,

                start_pos.X, start_pos.Y - height,
                start_pos.X + height, start_pos.Y,
                start_pos.X + height, start_pos.Y - height
            };

            for (int i = 0; i < character_count; i += 6)
            {

            }

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, text_vertices.Length * sizeof(float), text_vertices, BufferUsageHint.DynamicDraw);
        
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Render()
        {
            GL.BindVertexArray(vaoHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void UpdateVertices(Vector2 StartPosition, float height)
        {
            start_pos = StartPosition;
            textHeight = height;

            text_vertices = new float[]
            {
                start_pos.X, start_pos.Y - height,
                start_pos.X, start_pos.Y,
                start_pos.X + height, start_pos.Y,

                start_pos.X, start_pos.Y - height,
                start_pos.X + height, start_pos.Y,
                start_pos.X + height, start_pos.Y - height
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, text_vertices.Length * sizeof(float), text_vertices);
        }
    }
}
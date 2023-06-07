using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

using Window.Helper;

namespace Window.Rendering
{
    public class Text
    {
        string text;
        private int vaoHandle, vboHandle, eboHandle;
        private float[] text_vertices;
        private uint[] text_indices;
        public Vector2 startposition;
        
        public float width, height;
        int character_count;

        public Text(string Text, Vector2 StartPosition, float height)
        {
            text = Text;
            startposition = StartPosition;
            character_count = Text.Length;

            width = HelperClass.MapRange(height, 0, Window.size.X, 0, 2);
            height = HelperClass.MapRange(height, 0, Window.size.Y, 0, 2);

            text_vertices = new float[8 * character_count];
            text_indices = new uint[character_count * 6];

            for (int i = 0; i < character_count; i++)
            {
                text_indices[0 + i * 6] = (uint)(i * 4);
                text_indices[1 + i * 6] = (uint)(i * 4 + 1);
                text_indices[2 + i * 6] = (uint)(i * 4 + 2);
                text_indices[3 + i * 6] = (uint)(i * 4);
                text_indices[4 + i * 6] = (uint)(i * 4 + 2);
                text_indices[5 + i * 6] = (uint)(i * 4 + 3);
            }

            for (int i = 0; i < character_count; i++)
            {
                float xOffset = i * width * 1.5f;

                text_vertices[0 + i * 8] = startposition.X + xOffset;
                text_vertices[1 + i * 8] = startposition.Y - height;
                text_vertices[2 + i * 8] = startposition.X + xOffset;
                text_vertices[3 + i * 8] = startposition.Y;

                text_vertices[4 + i * 8] = startposition.X + width + xOffset;
                text_vertices[5 + i * 8] = startposition.Y;
                text_vertices[6 + i * 8] = startposition.X + width + xOffset;
                text_vertices[7 + i * 8] = startposition.Y - height;
            }

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, text_vertices.Length * sizeof(float), text_vertices, BufferUsageHint.DynamicDraw);

            eboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, text_indices.Length * sizeof(uint), text_indices, BufferUsageHint.DynamicDraw);
        
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Render()
        {
            Window.TextShader.Use();
            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(PrimitiveType.Triangles, 6 * character_count, DrawElementsType.UnsignedInt, 0);
        }

        public void UpdateVertices(Vector2 StartPosition, float pixelheight)
        {
            startposition = StartPosition;
            width = HelperClass.MapRange(pixelheight, 0, Window.size.X, 0, 2);
            height = HelperClass.MapRange(pixelheight, 0, Window.size.Y, 0, 2);

            for (int i = 0; i < character_count; i++)
            {
                text_indices[0 + i * 6] = (uint)(i * 4);
                text_indices[1 + i * 6] = (uint)(i * 4 + 1);
                text_indices[2 + i * 6] = (uint)(i * 4 + 2);
                text_indices[3 + i * 6] = (uint)(i * 4);
                text_indices[4 + i * 6] = (uint)(i * 4 + 2);
                text_indices[5 + i * 6] = (uint)(i * 4 + 3);
            }

            for (int i = 0; i < character_count; i++)
            {
                float xOffset = i * width * 1.5f;

                text_vertices[0 + i * 8] = startposition.X + xOffset;
                text_vertices[1 + i * 8] = startposition.Y - height;
                text_vertices[2 + i * 8] = startposition.X + xOffset;
                text_vertices[3 + i * 8] = startposition.Y;

                text_vertices[4 + i * 8] = startposition.X + width + xOffset;
                text_vertices[5 + i * 8] = startposition.Y;
                text_vertices[6 + i * 8] = startposition.X + width + xOffset;
                text_vertices[7 + i * 8] = startposition.Y - height;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, text_vertices.Length * sizeof(float), text_vertices);
        }
    }
}
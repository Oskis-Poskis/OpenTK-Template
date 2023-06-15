using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

using WindowTemplate.Helper;
using System.Drawing;
using static System.Formats.Asn1.AsnWriter;

namespace WindowTemplate.UI
{
    struct Character
    {
        public char Char;
        public int TextureID;  // ID handle of the glyph texture
        public Vector2 Size;       // Size of glyph
        public Vector2 Bearing;    // Offset from baseline to left/top of glyph
        public int Advance;    // Offset to advance to next glyph

        public Character(char char_, int textureid, Vector2 size, Vector2 bearing, int advance)
        {
            Char = char_;
            int TextureID = textureid;
            Vector2 Size = size;
            Vector2 Bearing = bearing;
            int Advance = advance;
        }
    };

    public class Text
    {
        private int vaoHandle, vboHandle;
        List<Character> characters = new List<Character>();

        unsafe public Text()
        {
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            for (uint c = 0; c < 128; c++)
            {
                // error = FT_Load_Char(face, (char)c, FT_LOAD_RENDER);
                // if (error != FT_Error.FT_Err_Ok) Console.WriteLine(error);

                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                //GL.TexImage2D()
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);

                Character character = new Character();
                
                Console.WriteLine($"Char: {character.Char} | TextureID: {character.TextureID} | BitmapSize: {character.Size} | Left&Top {character.Bearing} | Advance {character.Advance}");

                characters.Insert((int)c, character);
            }

            //FT_Done_Face(face);
            //FT_Done_FreeType(library);

            float xpos = 0;
            float ypos = 0;
            float h = 0.1f;
            float w = 0.1f;
            float[] vertices = new float[]
            {
                xpos,     ypos + h,   0.0f, 0.0f,
                xpos,     ypos,       0.0f, 1.0f,
                xpos + w, ypos,       1.0f, 1.0f,
                xpos,     ypos + h,   0.0f, 0.0f,
                xpos + w, ypos,       1.0f, 1.0f,
                xpos + w, ypos + h,   1.0f, 0.0f
            };

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        }

        public void Render(string text, float x, float y, float scale)
        {
            HostWindow.TextShader.Use();
            HostWindow.TextShader.SetVector3("textColor", new Vector3(1, 0, 0));
            HostWindow.TextShader.SetMatrix4("projection", Matrix4.CreateOrthographicOffCenter(0, HostWindow.size.X, 0, HostWindow.size.Y, -1, 1));
            
            GL.ActiveTexture(TextureUnit.Texture0);
            HostWindow.TextShader.SetInt("text", 0);
            GL.BindTexture(TextureTarget.Texture2D, characters[0x41].TextureID);
            Console.WriteLine($"Index: {0x41} | {(char)0x41}");

            GL.BindVertexArray(vaoHandle);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            /*
            for (int c = 0; c < text.Length; c++)
            {
                Character ch = characters[c];

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;
                
                float[] vertices = new float[]
                {
                    xpos,     ypos + h,   0.0f, 0.0f,
                    xpos,     ypos,       0.0f, 1.0f,
                    xpos + w, ypos,       1.0f, 1.0f,
                    xpos,     ypos + h,   0.0f, 0.0f,
                    xpos + w, ypos,       1.0f, 1.0f,
                    xpos + w, ypos + h,   1.0f, 0.0f
                };

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                x += (ch.Advance >> 6) * scale;
            }
            */
        }

        public void UpdateVertices(Vector2 StartPosition, float pixelheight)
        {
            
        }
    }
}
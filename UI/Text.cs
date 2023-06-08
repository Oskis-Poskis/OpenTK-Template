using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

using FreeTypeSharp;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

using Window.Helper;
using System.Drawing;
using static System.Formats.Asn1.AsnWriter;

namespace Window.Rendering
{
    struct Character
    {
        public int TextureID;  // ID handle of the glyph texture
        public Vector2 Size;       // Size of glyph
        public Vector2 Bearing;    // Offset from baseline to left/top of glyph
        public int Advance;    // Offset to advance to next glyph

        public Character(int textureid, Vector2 size, Vector2 bearing, int advance)
        {
            int TextureID = textureid;
            Vector2 Size = size;
            Vector2 Bearing = bearing;
            int Advance = advance;
        }
    };

    public class Text
    {
        string text;
        private int vaoHandle, vboHandle, eboHandle;
        public Vector2 startposition;
        
        public float width, height;
        int character_count;

        List<Character> characters = new List<Character>();

        unsafe public Text(string Text, Vector2 StartPosition, float height)
        {
            IntPtr library, face;
            FT_Error error;
            FT_FaceRec* face_ptr;

            error = FT_Init_FreeType(out library);
            if (error != FT_Error.FT_Err_Ok) Console.WriteLine(error);

            error = FT_New_Face(library, $"{Window.base_path}Assets/Fonts/Segoe UI.ttf", 0, out face);
            face_ptr = (FT_FaceRec*)face;
            if (error != FT_Error.FT_Err_Ok) Console.WriteLine(error + "\n" + $"{Window.base_path}Assets/Fonts/Segoe UI.ttf");

            FT_Set_Pixel_Sizes(face, 0, 48);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            for (uint c = 0; c < 128; c++)
            {
                error = FT_Load_Char(face, c, FT_LOAD_RENDER);
                if (error != FT_Error.FT_Err_Ok) Console.WriteLine(error);

                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (PixelInternalFormat)All.Red,
                    (int)face_ptr->glyph->bitmap.width,
                    (int)face_ptr->glyph->bitmap.rows,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedByte,
                    face_ptr->glyph->bitmap.buffer
                    );

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, texture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, texture);

                Character character = new Character(
                    texture,
                    new(face_ptr->glyph->bitmap.width, face_ptr->glyph->bitmap.rows),
                    new(face_ptr->glyph->bitmap_left, face_ptr->glyph->bitmap_top),
                    (int)face_ptr->glyph->advance.x);

                characters.Insert((int)c, character);
            }

            FT_Done_Face(face);
            FT_Done_FreeType(library);


            text = Text;
            startposition = StartPosition;
            character_count = Text.Length;

            width = HelperClass.MapRange(height, 0, Window.size.X, 0, 2);
            height = HelperClass.MapRange(height, 0, Window.size.Y, 0, 2);

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, 0, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        }

        public void Render(float x, float y, float scale)
        {
            Window.TextShader.Use();

            Window.TextShader.SetVector3("textColor", new Vector3(1.0f));
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(vaoHandle);

            for (int c = 0; c < text.Count(); c++)
            {
                Character ch = characters[c];

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.X * scale;
                // update VBO for each character
                float[][] vertices = new float[6][]
                {
                    new float[] { xpos,     ypos + h,   0.0f, 0.0f },
                    new float[] { xpos,     ypos,       0.0f, 1.0f },
                    new float[] { xpos + w, ypos,       1.0f, 1.0f },
                    new float[] { xpos,     ypos + h,   0.0f, 0.0f },
                    new float[] { xpos + w, ypos,       1.0f, 1.0f },
                    new float[] { xpos + w, ypos + h,   1.0f, 0.0f }
                };

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length, vertices[0]);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                x += (ch.Advance >> 6) * scale;
            }
        }

        public void UpdateVertices(Vector2 StartPosition, float pixelheight)
        {
            startposition = StartPosition;
            width = HelperClass.MapRange(pixelheight, 0, Window.size.X, 0, 2);
            height = HelperClass.MapRange(pixelheight, 0, Window.size.Y, 0, 2);
        }
    }
}
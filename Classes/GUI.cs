using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

namespace Window.Rendering
{
    public class GUIWindow
    {
        string Title = "";
        Vector2i position;

        float TopEdge = 0.5f;
        float BottomEdge = -0.5f;
        float RightEdge = 0.5f;
        float LeftEdge = -0.5f;

        public bool IsHoveringAnyEdge;
        public bool[] EdgesHover = new bool[4];

        public bool isResizing = false;
        float edgeThreshold = 0.01f;

        private int vaoHandle;
        private int vboHandle;

        public float[] window_vertices = new float[12];
        public MouseCursor cursor = MouseCursor.Default;

        public GUIWindow()
        {
            window_vertices = new[]
            {
                LeftEdge, BottomEdge,
                LeftEdge, TopEdge,
                RightEdge, TopEdge,

                LeftEdge, BottomEdge,
                RightEdge, TopEdge,
                RightEdge, BottomEdge,
            };

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, window_vertices.Length * sizeof(float), window_vertices, BufferUsageHint.DynamicDraw);
        
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Update(MouseState mouseState, int windowWidth, int windowHeight)
        {
            float posx = Window.mpos.X;
            float posy = Window.mpos.Y;

            EdgesHover[0] = (posx >= LeftEdge - edgeThreshold && posx <= LeftEdge + edgeThreshold) && (posy < TopEdge) && (posy > BottomEdge);
            EdgesHover[1] = (posx >= RightEdge - edgeThreshold && posx <= RightEdge + edgeThreshold) && (posy < TopEdge) && (posy > BottomEdge);
            EdgesHover[2] = (posy >= BottomEdge - edgeThreshold && posy <= BottomEdge + edgeThreshold) && (posx > LeftEdge) && (posx < RightEdge);
            EdgesHover[3] = (posy >= TopEdge - edgeThreshold && posy <= TopEdge + edgeThreshold) && (posx > LeftEdge) && (posx < RightEdge);
            IsHoveringAnyEdge = (EdgesHover[0] | EdgesHover[1] | EdgesHover[2] | EdgesHover[3]);

            int hoveredEdgesCount = 0;
            for (int i = 0; i < EdgesHover.Length; i++)
            {
                if (EdgesHover[i])
                {
                    hoveredEdgesCount++;
                }
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        public void ResizeWindow(bool leftClick)
        {
            float xpos = Window.mpos.X;
            float ypos = Window.mpos.Y;

            if ((EdgesHover[3] | isResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[2])
            {
                cursor = MouseCursor.VResize;
                if (leftClick)
                {
                    isResizing = true;
                    TopEdge = ypos;
                    
                    window_vertices[3] = TopEdge;
                    window_vertices[5] = TopEdge;
                    window_vertices[9] = TopEdge;
                }
                else isResizing = false;
            }

            else if ((EdgesHover[1] | isResizing) && !EdgesHover[0] && !EdgesHover[2] && !EdgesHover[3])
            {
                cursor = MouseCursor.HResize;
                if (leftClick)
                {
                    isResizing = true;
                    RightEdge = Window.mpos.X;

                    window_vertices[4] = RightEdge;
                    window_vertices[8] = RightEdge;
                    window_vertices[10] = RightEdge;
                }
                else isResizing = false;
            }

            else if ((EdgesHover[2] | isResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[3])
            {
                cursor = MouseCursor.VResize;
                if (leftClick)
                {
                    isResizing = true;
                    BottomEdge = Window.mpos.Y;

                    window_vertices[1] = BottomEdge;
                    window_vertices[7] = BottomEdge;
                    window_vertices[11] = BottomEdge;
                }
                else isResizing = false;
            }

            else if ((EdgesHover[0] | isResizing) && !EdgesHover[1] && !EdgesHover[2] && !EdgesHover[3])
            {
                cursor = MouseCursor.HResize;
                if (leftClick)
                {
                    isResizing = true;
                    LeftEdge = Window.mpos.X;

                    window_vertices[0] = LeftEdge;
                    window_vertices[2] = LeftEdge;
                    window_vertices[6] = LeftEdge;
                }
                else isResizing = false;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);
        }
    }
}
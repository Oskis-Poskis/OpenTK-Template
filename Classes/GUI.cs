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

        float edgeThreshold = 0.01f;
        float topBarThickness = 0.2f;
        float min_windowSize = 0.05f;

        public bool[] EdgesHover = new bool[4];
        public bool IsHoveringAnyEdge = false;
        public bool IsActive = false;
        public bool isResizing = false;
        public bool isMoving = false;

        private int vaoHandle, vboHandle;

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

                LeftEdge, TopEdge,
                LeftEdge, TopEdge + topBarThickness,
                RightEdge, TopEdge + topBarThickness,

                LeftEdge, TopEdge,
                RightEdge, TopEdge + topBarThickness,
                RightEdge, TopEdge
            };

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, window_vertices.Length * sizeof(float), window_vertices, BufferUsageHint.DynamicDraw);
        
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Render(MouseState mouseState, int windowWidth, int windowHeight)
        {
            float posx = Window.mpos.X;
            float posy = Window.mpos.Y;

            EdgesHover[0] = (posx >= LeftEdge - edgeThreshold && posx <= LeftEdge + edgeThreshold) && (posy < TopEdge + topBarThickness) && (posy > BottomEdge);
            EdgesHover[1] = (posx >= RightEdge - edgeThreshold && posx <= RightEdge + edgeThreshold) && (posy < TopEdge + topBarThickness) && (posy > BottomEdge);
            EdgesHover[2] = (posy >= BottomEdge - edgeThreshold && posy <= BottomEdge + edgeThreshold) && (posx > LeftEdge) && (posx < RightEdge);
            EdgesHover[3] = (posy >= TopEdge - edgeThreshold + topBarThickness && posy <= TopEdge + edgeThreshold + topBarThickness) && (posx > LeftEdge) && (posx < RightEdge);
            IsHoveringAnyEdge = (EdgesHover[0] | EdgesHover[1] | EdgesHover[2] | EdgesHover[3]);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 12);
        }

        float prevXpos = 0;
        float prevYpos = 0;
        public void TransformWindow(bool leftClick)
        {
            float xpos = Window.mpos.X;
            float ypos = Window.mpos.Y;

            if (IsHoveringAnyEdge)
            {
                // Check TopEdge for hover
                if ((EdgesHover[3] | isResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[2])
                {
                    cursor = MouseCursor.VResize;
                    if (leftClick)
                    {
                        if (MathHelper.Abs(ypos - topBarThickness - BottomEdge) > min_windowSize)
                        {
                            isResizing = true;
                            TopEdge = ypos - topBarThickness;
                            
                            window_vertices[3] = TopEdge;
                            window_vertices[5] = TopEdge;
                            window_vertices[9] = TopEdge;

                            window_vertices[13] = TopEdge;
                            window_vertices[15] = TopEdge + topBarThickness;
                            window_vertices[17] = TopEdge + topBarThickness;

                            window_vertices[19] = TopEdge;
                            window_vertices[21] = TopEdge + topBarThickness;
                            window_vertices[23] = TopEdge;
                        }
                    }
                    else isResizing = false;
                }

                // Check RightEdge for hover
                else if ((EdgesHover[1] | isResizing) && !EdgesHover[0] && !EdgesHover[2] && !EdgesHover[3])
                {
                    cursor = MouseCursor.HResize;
                    if (leftClick)
                    {
                        if (MathHelper.Abs(xpos - LeftEdge) > min_windowSize)
                        {
                            isResizing = true;
                            RightEdge = Window.mpos.X;

                            window_vertices[4] = RightEdge;
                            window_vertices[8] = RightEdge;
                            window_vertices[10] = RightEdge;

                            window_vertices[16] = RightEdge;
                            window_vertices[20] = RightEdge;
                            window_vertices[22] = RightEdge;
                        }
                    }
                    else isResizing = false;
                }

                // Check BottomEdge for hover
                else if ((EdgesHover[2] | isResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[3])
                {
                    cursor = MouseCursor.VResize;
                    if (leftClick)
                    {
                        if (MathHelper.Abs(ypos - TopEdge) > min_windowSize)
                        {
                            isResizing = true;
                            BottomEdge = Window.mpos.Y;

                            window_vertices[1] = BottomEdge;
                            window_vertices[7] = BottomEdge;
                            window_vertices[11] = BottomEdge;
                        }
                    }
                    else isResizing = false;
                }

                // Check LeftEdge for hover
                else if ((EdgesHover[0] | isResizing) && !EdgesHover[1] && !EdgesHover[2] && !EdgesHover[3])
                {
                    cursor = MouseCursor.HResize;
                    if (leftClick)
                    {
                        if (MathHelper.Abs(xpos - RightEdge) > min_windowSize)
                        {
                            isResizing = true;
                            LeftEdge = Window.mpos.X;

                            window_vertices[0] = LeftEdge;
                            window_vertices[2] = LeftEdge;
                            window_vertices[6] = LeftEdge;

                            window_vertices[12] = LeftEdge;
                            window_vertices[14] = LeftEdge;
                            window_vertices[18] = LeftEdge;
                        }
                    }
                    else isResizing = false;
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);
            }

            else
            {
                cursor = MouseCursor.Default;
            }

            /*if (((xpos < RightEdge) && (xpos > LeftEdge) && (ypos < TopEdge + topBarThickness) && (ypos > TopEdge) && !isResizing) | isMoving)
            {
                if (leftClick)
                {
                    isMoving = true;

                    float dx = xpos - prevXpos;
                    float dy = ypos - prevYpos;

                    for (int i = 0; i < window_vertices.Length; i += 2)
                    {
                        window_vertices[i] += dx;
                        window_vertices[i + 1] += dy;
                    }

                    prevXpos = xpos;
                    prevYpos = ypos;
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);
            }

            else
            {
                isMoving = false;
            }*/
        }

        public bool IsWindowHovered()
        {
            if ((Window.mpos.X > LeftEdge - edgeThreshold) &&
                (Window.mpos.X < RightEdge + edgeThreshold) &&
                (Window.mpos.Y < TopEdge + edgeThreshold + topBarThickness) &&
                (Window.mpos.Y > BottomEdge - edgeThreshold)) return true;
            else return false;
        }
    }
}
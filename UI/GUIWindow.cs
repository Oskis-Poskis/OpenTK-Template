using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

namespace Window.Rendering
{
    public class GUIWindow
    {
        public string Title;

        private float TopEdge = 0.5f;
        private float BottomEdge = -0.5f;
        private float RightEdge = 0.5f;
        private float LeftEdge = -0.5f;

        private float edgeThreshold = 0.01f;
        private float topBarThickness = 0.05f;
        private float min_windowSize = 0.05f;
        private float borderThickness = 0.1f;

        public bool[] EdgesHover = new bool[4];
        public bool IsHoveringAnyEdge = false;
        public bool IsHoveringTopBar = false;
        public bool IsResizing = false;
        public bool IsMoving = false;

        private int vaoHandle, vboHandle, eboHandle;
        private float[] window_vertices = new float[32];
        private uint[] window_indices = new uint[18];
        public MouseCursor cursor = MouseCursor.Default;

        private Vector2 zero = Vector2.Zero;

        public GUIWindow(string title, Vector2 position)
        {
            this.Title = title;

            window_vertices = new float[]
            {
                // Inner Bottom Left 0
                LeftEdge, BottomEdge,
                // Inner Top Left 1
                LeftEdge, TopEdge,
                // Inner Top Right 2
                RightEdge, TopEdge,
                // Inner Bottom Right 3
                RightEdge, BottomEdge,

                // Topbar Left 4
                LeftEdge, TopEdge + topBarThickness,
                // Topbar Right 5
                RightEdge, TopEdge + topBarThickness,
                
                // Outer Top Left 6
                LeftEdge - borderThickness, TopEdge + topBarThickness + borderThickness,
                // Outer Top Right 7
                RightEdge + borderThickness, TopEdge + topBarThickness + borderThickness,
                // Outer Bottom Right 8
                RightEdge + borderThickness, BottomEdge - borderThickness,
                // Outer Bottom Left 9
                LeftEdge - borderThickness, BottomEdge - borderThickness
            };

            window_indices = new uint[]
            {
                // Main Window
                0, 1, 2,
                0, 2, 3,

                // Top Bar
                1, 4, 5,
                1, 5, 2,

                // Top Border
                4, 6, 7,
                4, 7, 5,

                // Right Border
                5, 7, 8,
                5, 8, 3,

                // Bottom Border
                3, 8, 9,
                3, 9, 0,

                // Left Border
                0, 9, 6,
                0, 6, 4
            };

            for (int i = 0; i < window_vertices.Length; i += 2)
            {
                window_vertices[i] += position.X;
                window_vertices[i + 1] += position.Y;
            }

            LeftEdge += position.X;
            RightEdge += position.X;
            TopEdge += position.Y;
            BottomEdge += position.Y;

            vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(vaoHandle);

            vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, window_vertices.Length * sizeof(float), window_vertices, BufferUsageHint.DynamicDraw);
        
            eboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, window_indices.Length * sizeof(uint), window_indices, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            CalculateWindowScaling();
        }

        public void Render(MouseState mouseState)
        {
            float posx = Window.mouse_pos.X;
            float posy = Window.mouse_pos.Y;

            EdgesHover[0] = (posx >= LeftEdge - edgeThreshold && posx <= LeftEdge + edgeThreshold) && (posy < TopEdge + topBarThickness) && (posy > BottomEdge);
            EdgesHover[1] = (posx >= RightEdge - edgeThreshold && posx <= RightEdge + edgeThreshold) && (posy < TopEdge + topBarThickness) && (posy > BottomEdge);
            EdgesHover[2] = (posy >= BottomEdge - edgeThreshold && posy <= BottomEdge + edgeThreshold) && (posx > LeftEdge) && (posx < RightEdge);
            EdgesHover[3] = (posy >= TopEdge + topBarThickness - edgeThreshold && posy <= TopEdge + edgeThreshold + topBarThickness) && (posx > LeftEdge) && (posx < RightEdge);
            
            IsHoveringTopBar = (posx >= LeftEdge + edgeThreshold && posx <= RightEdge - edgeThreshold) && (posy >= TopEdge && posy <= TopEdge - edgeThreshold + topBarThickness);
            IsHoveringAnyEdge = (EdgesHover[0] | EdgesHover[1] | EdgesHover[2] | EdgesHover[3]);
            
            GL.BindVertexArray(vaoHandle);
            GL.DrawElements(PrimitiveType.Triangles, window_indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        float initialXPos = 0;
        float initialYPos = 0;

        public void TransformWindow(bool leftClick, bool altDown)
        {
            float xpos = Window.mouse_pos.X;
            float ypos = Window.mouse_pos.Y;
            bool windowHovered = IsWindowHovered();

            if (windowHovered | IsResizing | IsMoving)
            {
                if (IsHoveringTopBar | IsMoving | (windowHovered && altDown))
                {
                    cursor = MouseCursor.Default;
                    if (leftClick)
                    {
                        if (!IsMoving)
                        {
                            initialXPos = xpos;
                            initialYPos = ypos;
                        }

                        IsMoving = true;

                        float xOffset = xpos - initialXPos;
                        float yOffset = ypos - initialYPos;

                        TopEdge += yOffset;
                        BottomEdge += yOffset;
                        LeftEdge += xOffset;
                        RightEdge += xOffset;

                        for (int i = 0; i < window_vertices.Length; i += 2)
                        {
                            window_vertices[i] += xOffset;
                            window_vertices[i + 1] += yOffset;
                        }

                        initialXPos = xpos;
                        initialYPos = ypos;

                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);
                    }
                    
                    else
                    {
                        IsMoving = false;
                    }
                }

                else if (windowHovered && !IsHoveringTopBar && !IsHoveringAnyEdge)
                {
                    cursor = MouseCursor.Default;
                }

                else if (IsHoveringAnyEdge)
                {
                    // Check TopEdge for hover
                    if ((EdgesHover[3] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[2])
                    {
                        cursor = MouseCursor.VResize;
                        if (leftClick)
                        {
                            if (ypos > BottomEdge + topBarThickness + min_windowSize)
                            {
                                IsResizing = true;
                                TopEdge = ypos - topBarThickness;

                                window_vertices[3] = TopEdge;
                                window_vertices[5] = TopEdge;
                                window_vertices[9] = TopEdge + topBarThickness;
                                window_vertices[11] = TopEdge + topBarThickness;
                                window_vertices[13] = TopEdge + topBarThickness + borderThickness;
                                window_vertices[15] = TopEdge + topBarThickness + borderThickness;
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check RightEdge for hover
                    else if ((EdgesHover[1] | IsResizing) && !EdgesHover[0] && !EdgesHover[2] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.HResize;
                        if (leftClick)
                        {
                            if (xpos > LeftEdge + min_windowSize)
                            {
                                IsResizing = true;
                                RightEdge = Window.mouse_pos.X;
                                
                                window_vertices[4] = RightEdge;
                                window_vertices[6] = RightEdge;
                                window_vertices[10] = RightEdge;
                                window_vertices[14] = RightEdge + borderThickness - topBarThickness;
                                window_vertices[16] = RightEdge + borderThickness - topBarThickness;
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check BottomEdge for hover
                    else if ((EdgesHover[2] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.VResize;
                        if (leftClick)
                        {
                            if (ypos < TopEdge - min_windowSize)
                            {
                                IsResizing = true;
                                BottomEdge = Window.mouse_pos.Y;
                                
                                window_vertices[1] = BottomEdge;
                                window_vertices[7] = BottomEdge;
                                window_vertices[17] = BottomEdge - borderThickness;
                                window_vertices[19] = BottomEdge - borderThickness;
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check LeftEdge for hover
                    else if ((EdgesHover[0] | IsResizing) && !EdgesHover[1] && !EdgesHover[2] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.HResize;
                        if (leftClick)
                        {
                            if (xpos < RightEdge - min_windowSize)
                            {
                                IsResizing = true;
                                LeftEdge = Window.mouse_pos.X;

                                window_vertices[0] = LeftEdge;
                                window_vertices[2] = LeftEdge;
                                window_vertices[8] = LeftEdge;
                                window_vertices[12] = LeftEdge - borderThickness + topBarThickness;
                                window_vertices[18] = LeftEdge - borderThickness + topBarThickness;
                            }
                        }
                        else IsResizing = false;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);
                }

                else cursor = MouseCursor.Default; 
            }

            else cursor = MouseCursor.Default;
        }

        float storedAspect = 1;
        public void CalculateWindowScaling()
        {
            float aspect = (float)Window.size.X / Window.size.Y;

            if (storedAspect != aspect)
            {
                float scaleFactor = storedAspect / aspect;

                LeftEdge *= scaleFactor;
                RightEdge *= scaleFactor;

                for (int i = 0; i < window_vertices.Length; i += 2)
                {
                    window_vertices[i] *= scaleFactor;
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, window_vertices.Length * sizeof(float), window_vertices);

                storedAspect = aspect;
            }
        }

        public bool IsWindowHovered()
        {
            if ((Window.mouse_pos.X > LeftEdge - edgeThreshold) &&
                (Window.mouse_pos.X < RightEdge + edgeThreshold) &&
                (Window.mouse_pos.Y < TopEdge + edgeThreshold + topBarThickness) &&
                (Window.mouse_pos.Y > BottomEdge - edgeThreshold)) return true;
            else return false;
        }
    }
}
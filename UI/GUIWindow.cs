using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;

using Window.Helper;

namespace Window.Rendering
{
    public class GUIWindow
    {
        public string Title;
        public int z_index;
        float xpos, ypos;
        public MouseCursor cursor = MouseCursor.Default;

        private float TopEdge =     0.5f;
        private float BottomEdge = -0.5f;
        private float RightEdge =   0.5f;
        private float LeftEdge =    -0.5f;

        private float edgeThreshold =   0.01f;
        private float min_windowSize =  0.05f;
        private float topbar_thickness, topBar_reference =  20f;
        private float border_y, border_x, border_thickness = 3f;

        public bool[] EdgesHover = new bool[4];
        public bool IsHoveringAnyEdge =  false;
        public bool IsHoveringTitleBar = false;
        public bool IsResizing =         false;
        public bool IsMoving =           false;

        private int mainVAO, mainVBO, mainEBO;
        private float[] main_vertices;
        private uint[] main_indices;

        private int interactionVAO, interactionVBO, interactionEBO;
        private float[] interaction_vertices;
        private uint[] interaction_indices;

        /// <summary>
        /// Create a new window instance, generates vertices and indices
        /// </summary>
        /// <param name="title">An awesome window title</param>
        /// <param name="width">Window width in pixels</param>
        /// <param name="height">Window height in pixels</param>
        public GUIWindow(string title, int width, int height, Vector2 position)
        {
            Title = title;
            float x = HelperClass.MapRange(width, 0, Window.size.X, 0, 2);
            float y = HelperClass.MapRange(height, 0, Window.size.Y, 0, 2);

            TopEdge = y / 2;
            BottomEdge = -y / 2;
            RightEdge = x / 2;
            LeftEdge = -x / 2;

            UpdateVertices();
            main_indices = new uint[]
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
                0, 6, 4,
            };

            interaction_indices = new uint[]
            {
                // Collapse Button
                0, 1, 2,
                0, 2, 3
            };

            for (int i = 0; i < main_vertices.Length; i += 2)
            {
                main_vertices[i] += position.X;
                main_vertices[i + 1] += position.Y;
            }

            LeftEdge += position.X;
            RightEdge += position.X;
            TopEdge += position.Y;
            BottomEdge += position.Y;

            mainVAO = GL.GenVertexArray();
            GL.BindVertexArray(mainVAO);

            mainVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, main_vertices.Length * sizeof(float), main_vertices, BufferUsageHint.DynamicDraw);
        
            mainEBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mainEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, main_indices.Length * sizeof(uint), main_indices, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            interactionVAO = GL.GenVertexArray();
            GL.BindVertexArray(interactionVAO);

            interactionVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, interactionVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, interaction_vertices.Length * sizeof(float), interaction_vertices, BufferUsageHint.DynamicDraw);

            interactionEBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, interactionEBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, interaction_indices.Length * sizeof(uint), interaction_indices, BufferUsageHint.DynamicDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public void Render(MouseState mouseState, bool isActive)
        {
            xpos = HelperClass.MapRange(Window.mouse_pos.X, 0, Window.size.X, -1.0f, 1.0f);
            ypos = HelperClass.MapRange(Window.mouse_pos.Y, 0, Window.size.Y, 1.0f, -1.0f);

            if (IsWindowHovered())
            {
                EdgesHover[0] = (xpos >= LeftEdge - edgeThreshold && xpos <= LeftEdge + edgeThreshold) && (ypos < TopEdge + topbar_thickness) && (ypos > BottomEdge);
                EdgesHover[1] = (xpos >= RightEdge - edgeThreshold && xpos <= RightEdge + edgeThreshold) && (ypos < TopEdge + topbar_thickness) && (ypos > BottomEdge);
                EdgesHover[2] = (ypos >= BottomEdge - edgeThreshold && ypos <= BottomEdge + edgeThreshold) && (xpos > LeftEdge) && (xpos < RightEdge);
                EdgesHover[3] = (ypos >= TopEdge + topbar_thickness - edgeThreshold && ypos <= TopEdge + edgeThreshold + topbar_thickness) && (xpos > LeftEdge) && (xpos < RightEdge);
                
                IsHoveringTitleBar = (xpos >= LeftEdge + edgeThreshold && xpos <= RightEdge - edgeThreshold) && (ypos >= TopEdge && ypos <= TopEdge - edgeThreshold + topbar_thickness);
                IsHoveringAnyEdge = (EdgesHover[0] | EdgesHover[1] | EdgesHover[2] | EdgesHover[3]);
            }
            
            Window.GUIWindow_S.SetFloat("index", z_index);
            Window.GUIWindow_S.SetInt("interaction", 0);
            GL.BindVertexArray(mainVAO);
            GL.DrawElements(PrimitiveType.Triangles, main_indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.DepthMask(false);
            // Window.GUIWindow_S.SetFloat("index", z_index + 0.1f / 100);
            Window.GUIWindow_S.SetInt("interaction", 1);
            GL.BindVertexArray(interactionVAO);
            GL.DrawElements(PrimitiveType.Triangles, interaction_indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DepthMask(true);
        }

        float initialXPos = 0;
        float initialYPos = 0;

        public void TransformWindow(bool leftClick, bool altDown)
        {
            bool windowHovered = IsWindowHovered();
            xpos = HelperClass.MapRange(Window.mouse_pos.X, 0, Window.size.X, -1.0f, 1.0f);
            ypos = HelperClass.MapRange(Window.mouse_pos.Y, 0, Window.size.Y, 1.0f, -1.0f);
            border_y = HelperClass.MapRange(border_thickness, 0, Window.size.Y, 0, 2);
            border_x = HelperClass.MapRange(border_thickness, 0, Window.size.X, 0, 2);

            if (windowHovered | IsResizing | IsMoving)
            {
                // Move Window
                if (IsHoveringTitleBar | IsMoving | (windowHovered && altDown))
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

                        for (int i = 0; i < main_vertices.Length; i += 2)
                        {
                            main_vertices[i] += xOffset;
                            main_vertices[i + 1] += yOffset;
                        }

                        initialXPos = xpos;
                        initialYPos = ypos;

                        GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
                        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, main_vertices.Length * sizeof(float), main_vertices);

                        UpdateSecondaryVertices();
                    }
                    
                    else IsMoving = false;
                }

                else if (windowHovered && !IsHoveringTitleBar && !IsHoveringAnyEdge)
                {
                    cursor = MouseCursor.Default;
                }

                else if (IsHoveringAnyEdge)
                {
                    // Check TopEdge for hover
                    if ((EdgesHover[3] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[2])
                    {
                        cursor = MouseCursor.VResize;
                        if (leftClick && Window.mouse_pos.Y > border_thickness)
                        {
                            if (ypos > BottomEdge + topbar_thickness + min_windowSize)
                            {
                                IsResizing = true;
                                TopEdge = ypos - topbar_thickness;

                                main_vertices[3] = TopEdge;
                                main_vertices[5] = TopEdge;
                                main_vertices[9] = TopEdge + topbar_thickness;
                                main_vertices[11] = TopEdge + topbar_thickness;
                                main_vertices[13] = TopEdge + topbar_thickness + border_y;
                                main_vertices[15] = TopEdge + topbar_thickness + border_y;

                                UpdateSecondaryVertices();
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check RightEdge for hover
                    else if ((EdgesHover[1] | IsResizing) && !EdgesHover[0] && !EdgesHover[2] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.HResize;
                        if (leftClick && Window.mouse_pos.X < Window.size.X - border_thickness)
                        {
                            if (xpos > LeftEdge + min_windowSize)
                            {
                                IsResizing = true;
                                RightEdge = xpos;
                                
                                main_vertices[4] = RightEdge;
                                main_vertices[6] = RightEdge;
                                main_vertices[10] = RightEdge;
                                main_vertices[14] = RightEdge + border_x;
                                main_vertices[16] = RightEdge + border_x;

                                UpdateSecondaryVertices();
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check BottomEdge for hover
                    else if ((EdgesHover[2] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.VResize;
                        if (leftClick && Window.mouse_pos.Y < Window.size.Y - border_thickness)
                        {
                            if (ypos < TopEdge - min_windowSize)
                            {
                                IsResizing = true;
                                BottomEdge = ypos;
                                
                                main_vertices[1] = BottomEdge;
                                main_vertices[7] = BottomEdge;
                                main_vertices[17] = BottomEdge - border_y;
                                main_vertices[19] = BottomEdge - border_y;
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check LeftEdge for hover
                    else if ((EdgesHover[0] | IsResizing) && !EdgesHover[1] && !EdgesHover[2] && !EdgesHover[3])
                    {
                        cursor = MouseCursor.HResize;
                        if (leftClick && Window.mouse_pos.X > border_thickness)
                        {
                            if (xpos < RightEdge - min_windowSize)
                            {
                                IsResizing = true;
                                LeftEdge = xpos;

                                main_vertices[0] = LeftEdge;
                                main_vertices[2] = LeftEdge;
                                main_vertices[8] = LeftEdge;
                                main_vertices[12] = LeftEdge - border_x;
                                main_vertices[18] = LeftEdge - border_x;
                            }
                        }
                        else IsResizing = false;
                    }

                    GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, main_vertices.Length * sizeof(float), main_vertices);
                }

                else cursor = MouseCursor.Default;
            }

            else cursor = MouseCursor.Default;
        }

        public void UpdateVertices()
        {
            border_y = HelperClass.MapRange(border_thickness, 0, Window.size.Y, 0, 2);
            border_x = HelperClass.MapRange(border_thickness, 0, Window.size.X, 0, 2);
            topbar_thickness = HelperClass.MapRange(topBar_reference, 0, Window.size.Y, 0, 2);

            main_vertices = new float[]
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
                LeftEdge, TopEdge + topbar_thickness,
                // Topbar Right 5
                RightEdge, TopEdge + topbar_thickness,
                
                // Outer Top Left 6
                LeftEdge - border_x, TopEdge + topbar_thickness + border_y,
                // Outer Top Right 7
                RightEdge + border_x, TopEdge + topbar_thickness + border_y,
                // Outer Bottom Right 8
                RightEdge + border_x, BottomEdge - border_y,
                // Outer Bottom Left 9
                LeftEdge - border_x, BottomEdge - border_y,
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, main_vertices.Length * sizeof(float), main_vertices);

            UpdateSecondaryVertices();
        }

        public void UpdateSecondaryVertices()
        {
            float sizex = HelperClass.MapRange(topBar_reference, 0, Window.size.X, 0, 2);

            interaction_vertices = new float[]
            {
                // Collapse Button
                RightEdge - sizex, TopEdge,
                RightEdge - sizex, TopEdge + topbar_thickness,
                RightEdge, TopEdge + topbar_thickness,
                RightEdge, TopEdge
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, interactionVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, interaction_vertices.Length * sizeof(float), interaction_vertices);
        }

        /// <summary>
        /// Returns true if the window is active and cursor is within it's boundary
        /// </summary>
        public bool IsWindowHovered()
        {
            if ((xpos > LeftEdge - edgeThreshold) &&
                (xpos < RightEdge + edgeThreshold) &&
                (ypos < TopEdge + edgeThreshold + topbar_thickness) &&
                (ypos > BottomEdge - edgeThreshold)) return true;
            else return false;
        }
    }
}
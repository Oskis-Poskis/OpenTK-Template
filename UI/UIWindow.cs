using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common.Input;
using WindowTemplate.Helper;

namespace WindowTemplate.UI
{
    public struct UISettings
    {
        public bool fullscreen;
        public bool collapsable;
        public bool moveable;
        public bool resizeable_l, resizeable_t, resizeable_r, resizeable_b;

        public UISettings()
        {
            fullscreen = false;
            collapsable = true;
            moveable = true;
            resizeable_l = true;
            resizeable_t = true;
            resizeable_r = true;
            resizeable_b = true;
        }
    }

    public class UIWindow
    {
        public string Title;
        public int z_index;
        float xpos, ypos;
        public MouseCursor cursor = MouseCursor.Default;
        public UISettings settings = new UISettings();

        private float TopEdge =     0.5f;
        private float BottomEdge = -0.5f;
        private float RightEdge =   0.5f;
        private float LeftEdge =   -0.5f;

        private Vector3 hover_tint = Vector3.One;

        private float edgeThreshold =   0.01f;
        private float min_windowSize =  0.05f;
        private float topbar_thickness, topBar_reference =  20;
        private float border_y, border_x, border_reference = 2;
        
        private float collapse_size;
        private float bottom;

        public float width, height;

        public bool[] EdgesHover = new bool[4];
        public bool IsHoveringAnyEdge =  false;
        public bool IsHoveringTitleBar = false;
        public bool IsResizing =         false;
        public bool IsMoving =           false;
        public bool IsCollapsed =        false;

        private int mainVAO, mainVBO, mainEBO;
        private float[] main_vertices;
        private uint[] main_indices = new uint[]
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

        private int interactionVAO, interactionVBO, interactionEBO;
        private float[] interaction_vertices;
        private uint[] interaction_indices = new uint[]
        {
            // Collapse Button
            0, 1, 2,
            0, 2, 3
        };

        /// <summary>
        /// Create a new window instance, generates vertices and indices
        /// </summary>
        /// <param name="title">An awesome window title</param>
        /// <param name="size">Window size in pixels, including titlebar</param>
        /// <param name="position">Position of the top left corner relative to the host windows top left corner, in pixels</param>
        public UIWindow(string title, Vector2i size, Vector2 position)
        {
            Title = title;
            float x = HelperClass.MapRange(size.X, 0, HostWindow.size.X, 0, 2);
            float y = HelperClass.MapRange(size.Y, 0, HostWindow.size.Y, 0, 2);

            TopEdge = y / 2;
            BottomEdge = -y / 2;
            RightEdge = x / 2;
            LeftEdge = -x / 2;

            border_y = HelperClass.MapRange(border_reference, 0, HostWindow.size.Y, 0, 2);
            border_x = HelperClass.MapRange(border_reference, 0, HostWindow.size.X, 0, 2);
            topbar_thickness = HelperClass.MapRange(topBar_reference, 0, HostWindow.size.Y, 0, 2);

            float xoff = HelperClass.MapRange(position.X, 0, HostWindow.size.X, -1, 1) + x / 2;
            float yoff = HelperClass.MapRange(position.Y, 0, HostWindow.size.Y, -1, 1) + y / 2;

            LeftEdge += xoff;
            RightEdge += xoff;
            TopEdge += -yoff - topbar_thickness;
            BottomEdge += -yoff;

            UpdateVertices();

            // Setup main window
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

            // Setup interaction widgets etc
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

            width = Vector2.Distance(new Vector2(LeftEdge + border_x, 0),
                                     new Vector2(RightEdge - border_x, 0));

            width = Vector2.Distance(new Vector2(0, BottomEdge + border_y),
                                     new Vector2(0, TopEdge));

            width = MathHelper.Clamp(HelperClass.MapRange(width, 0, 2, 0, HostWindow.size.X), 0, HostWindow.size.X);
            height = MathHelper.Clamp(HelperClass.MapRange(height, 0, 2, 0, HostWindow.size.Y), 0, HostWindow.size.Y);
        }

        public void Render(MouseState mouseState, bool isActive)
        {
            xpos = HelperClass.MapRange(HostWindow.mouse_pos.X, 0, HostWindow.size.X, -1.0f, 1.0f);
            ypos = HelperClass.MapRange(HostWindow.mouse_pos.Y, 0, HostWindow.size.Y, 1.0f, -1.0f);

            if (IsWindowHovered())
            {
                EdgesHover[0] = (xpos >= LeftEdge - edgeThreshold && xpos <= LeftEdge + edgeThreshold) && (ypos < TopEdge + topbar_thickness) && (ypos > BottomEdge);
                EdgesHover[1] = (xpos >= RightEdge - edgeThreshold && xpos <= RightEdge + edgeThreshold) && (ypos < TopEdge + topbar_thickness) && (ypos > BottomEdge);
                EdgesHover[2] = (ypos >= BottomEdge - edgeThreshold && ypos <= BottomEdge + edgeThreshold) && (xpos > LeftEdge) && (xpos < RightEdge);
                EdgesHover[3] = (ypos >= TopEdge + topbar_thickness - edgeThreshold && ypos <= TopEdge + edgeThreshold + topbar_thickness) && (xpos > LeftEdge) && (xpos < RightEdge);
                
                IsHoveringTitleBar = (xpos >= LeftEdge + edgeThreshold && xpos <= RightEdge - edgeThreshold) && (ypos >= TopEdge - border_y && ypos <= TopEdge - edgeThreshold + topbar_thickness);
                IsHoveringAnyEdge = (EdgesHover[0] | EdgesHover[1] | EdgesHover[2] | EdgesHover[3]);
            }

            HostWindow.WindowShader.SetFloat("index", settings.fullscreen ? -1 : z_index);
            if (settings.collapsable && !settings.fullscreen)
            {
                HostWindow.WindowShader.SetInt("interaction", 1);
                HostWindow.WindowShader.SetVector3("tint", hover_tint);
                GL.BindVertexArray(interactionVAO);
                GL.DrawElements(PrimitiveType.Triangles, interaction_indices.Length, DrawElementsType.UnsignedInt, 0);
            }

            HostWindow.WindowShader.SetInt("interaction", 0);
            GL.BindVertexArray(mainVAO);
            GL.DrawElements(PrimitiveType.Triangles, main_indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        float initialXPos = 0;
        float initialYPos = 0;

        public void TransformWindow(bool leftDown, bool leftPress, bool leftReleased, bool altDown)
        {
            bool windowHovered = IsWindowHovered();
            xpos = HelperClass.MapRange(HostWindow.mouse_pos.X, 0, HostWindow.size.X, -1.0f, 1.0f);
            ypos = HelperClass.MapRange(HostWindow.mouse_pos.Y, 0, HostWindow.size.Y, 1.0f, -1.0f);
            border_y = HelperClass.MapRange(border_reference, 0, HostWindow.size.Y, 0, 2);
            border_x = HelperClass.MapRange(border_reference, 0, HostWindow.size.X, 0, 2);

            collapse_size = HelperClass.MapRange(topBar_reference, 0, HostWindow.size.X, 0, 2);

            if (windowHovered | IsResizing | IsMoving)
            {
                // Move Window
                if (IsHoveringTitleBar | IsMoving | (windowHovered && altDown) && !settings.fullscreen)
                {
                    cursor = MouseCursor.Default;
                    if (leftDown && settings.moveable)
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
                        bottom += yOffset;
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

                    if (IsHoveringTitleBar && settings.collapsable && !settings.fullscreen)
                    {
                        if (IsRectangleHovered(
                            new(RightEdge - collapse_size - border_x, TopEdge + topbar_thickness - border_y),
                            new(RightEdge - border_x, TopEdge - border_y)))
                        {
                            if (leftPress)
                            {
                                IsCollapsed = HelperClass.ToggleBool(IsCollapsed);
                                if (IsCollapsed)
                                {
                                    hover_tint = new(0.4f);
                                }
                                UpdateVertices();
                                Console.WriteLine("Collapsed: " + Title);
                            }
                        }
                    }

                    if (leftReleased && !IsCollapsed)
                    {
                        hover_tint = new(1);
                    }
                }

                else if (windowHovered && !IsHoveringTitleBar && !IsHoveringAnyEdge)
                {
                    cursor = MouseCursor.Default;
                }

                else if (IsHoveringAnyEdge && !settings.fullscreen)
                {
                    // Check TopEdge for hover
                    if ((EdgesHover[3] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[2] && !IsCollapsed && settings.resizeable_t)
                    {
                        cursor = MouseCursor.VResize;
                        if (leftDown && HostWindow.mouse_pos.Y > border_reference)
                        {
                            if (ypos > BottomEdge + topbar_thickness + min_windowSize)
                            {
                                IsResizing = true;
                                TopEdge = ypos - topbar_thickness;

                                main_vertices[3] = TopEdge - border_y;
                                main_vertices[5] = TopEdge - border_y;
                                main_vertices[9] = TopEdge + topbar_thickness - border_y;
                                main_vertices[11] = TopEdge + topbar_thickness - border_y;
                                main_vertices[13] = TopEdge + topbar_thickness;
                                main_vertices[15] = TopEdge + topbar_thickness;

                                UpdateSecondaryVertices();
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check RightEdge for hover
                    else if ((EdgesHover[1] | IsResizing) && !EdgesHover[0] && !EdgesHover[2] && !EdgesHover[3]  && settings.resizeable_r)
                    {
                        cursor = MouseCursor.HResize;
                        if (leftDown && HostWindow.mouse_pos.X < HostWindow.size.X - border_reference)
                        {
                            if (xpos > LeftEdge + collapse_size + border_x * 2)
                            {
                                IsResizing = true;
                                RightEdge = xpos;
                                
                                main_vertices[4] = RightEdge - border_x;
                                main_vertices[6] = RightEdge - border_x;
                                main_vertices[10] = RightEdge - border_x;
                                main_vertices[14] = RightEdge;
                                main_vertices[16] = RightEdge;

                                UpdateSecondaryVertices();
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check BottomEdge for hover
                    else if ((EdgesHover[2] | IsResizing) && !EdgesHover[0] && !EdgesHover[1] && !EdgesHover[3] && !IsCollapsed  && settings.resizeable_b)
                    {
                        cursor = MouseCursor.VResize;
                        if (leftDown && HostWindow.mouse_pos.Y < HostWindow.size.Y - border_reference)
                        {
                            if (ypos < TopEdge - min_windowSize)
                            {
                                IsResizing = true;
                                BottomEdge = ypos;
                                
                                main_vertices[1] = BottomEdge + border_y;
                                main_vertices[7] = BottomEdge + border_y;
                                main_vertices[17] = BottomEdge;
                                main_vertices[19] = BottomEdge;
                            }
                        }
                        else IsResizing = false;
                    }

                    // Check LeftEdge for hover
                    else if ((EdgesHover[0] | IsResizing) && !EdgesHover[1] && !EdgesHover[2] && !EdgesHover[3] && settings.resizeable_l)
                    {
                        cursor = MouseCursor.HResize;
                        if (leftDown && HostWindow.mouse_pos.X > border_reference)
                        {
                            if (xpos < RightEdge - collapse_size - border_x * 2)
                            {
                                IsResizing = true;
                                LeftEdge = xpos;

                                main_vertices[0] = LeftEdge + border_x;
                                main_vertices[2] = LeftEdge + border_x;
                                main_vertices[8] = LeftEdge + border_x;
                                main_vertices[12] = LeftEdge;
                                main_vertices[18] = LeftEdge;
                            }
                        }
                        else IsResizing = false;
                    }

                    width = Vector2.Distance(new Vector2(LeftEdge + border_x, 0),
                                             new Vector2(RightEdge - border_x, 0));

                    width = Vector2.Distance(new Vector2(0, BottomEdge + border_y),
                                             new Vector2(0, TopEdge));

                    width = MathHelper.Clamp(HelperClass.MapRange(width, -1, 1, 0, HostWindow.size.X), 0, HostWindow.size.X);
                    height = MathHelper.Clamp(HelperClass.MapRange(height, -1, 1, 0, HostWindow.size.Y), 0, HostWindow.size.Y);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, main_vertices.Length * sizeof(float), main_vertices);
                }
                else cursor = MouseCursor.Default;
            }
            else cursor = MouseCursor.Default;
        }

        public void UpdateVertices()
        {
            border_y = HelperClass.MapRange(border_reference, 0, HostWindow.size.Y, 0, 2);
            border_x = HelperClass.MapRange(border_reference, 0, HostWindow.size.X, 0, 2);
            topbar_thickness = HelperClass.MapRange(topBar_reference, 0, HostWindow.size.Y, 0, 2);

            bottom = IsCollapsed ? TopEdge - border_y * 2 : BottomEdge;

            if (settings.fullscreen)
            {
                LeftEdge = -1;
                RightEdge = 1;
                TopEdge = 1 - topbar_thickness;
                BottomEdge = -1;
            }

            main_vertices = new float[]
            {
                // Inner Bottom Left 0
                LeftEdge + border_x, bottom + border_y,
                // Inner Top Left 1
                LeftEdge + border_x, TopEdge - border_y,
                // Inner Top Right 2
                RightEdge - border_x, TopEdge - border_y,
                // Inner Bottom Right 3
                RightEdge - border_x, bottom + border_y,

                // Topbar Left 4
                LeftEdge + border_x, TopEdge + topbar_thickness - border_y,
                // Topbar Right 5
                RightEdge - border_x, TopEdge + topbar_thickness - border_y,
                
                // Outer Top Left 6
                LeftEdge, TopEdge + topbar_thickness,
                // Outer Top Right 7
                RightEdge, TopEdge + topbar_thickness,
                // Outer Bottom Right 8
                RightEdge, bottom,
                // Outer Bottom Left 9
                LeftEdge, bottom,
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, mainVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, main_vertices.Length * sizeof(float), main_vertices);

            UpdateSecondaryVertices();
        }

        public void UpdateSecondaryVertices()
        {
            collapse_size = HelperClass.MapRange(topBar_reference, 0, HostWindow.size.X, 0, 2);

            interaction_vertices = new float[]
            {
                // Collapse Button
                RightEdge - collapse_size - border_x, TopEdge - border_y,
                RightEdge - collapse_size - border_x, TopEdge + topbar_thickness - border_y,
                RightEdge - border_x, TopEdge + topbar_thickness - border_y,
                RightEdge - border_x, TopEdge - border_y
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, interactionVBO);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, interaction_vertices.Length * sizeof(float), interaction_vertices);
        }

        public bool IsWindowHovered()
        {
            return (IsRectangleHovered(
                new(LeftEdge - edgeThreshold, TopEdge + edgeThreshold + topbar_thickness),
                new(RightEdge + edgeThreshold, (IsCollapsed ? bottom : BottomEdge) - edgeThreshold)));
        }

        public bool IsRectangleHovered(Vector2 TopLeft, Vector2 BottomRight)
        {
            if ((xpos < BottomRight.X) &&
                (xpos > TopLeft.X) &&
                (ypos < TopLeft.Y) &&
                (ypos > BottomRight.Y))
            {
                return true;
            }

            return false;
        }
    }
}
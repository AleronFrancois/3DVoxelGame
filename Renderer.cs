using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;


/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public class Renderer : GameWindow
{   
    // Block position, color and mesh data object
    private struct BlockInstance
    {
        public Matrix4 ModelMatrix;
        public Vector3 Color;
        public int Vao;
        public int Vbo;
        public int Ebo;
        public int Texture;
    }

    private List<BlockInstance> blocks; // List of blocks
    private int shaderProgram; // Shader program for rendering
    private Matrix4 projection, view; // Projection and view matrices for camera setup

    // Camera position, orientation and movement settings structure 
    private struct Camera
    {
        public Vector3 Position; // Camera position
        public Vector3 Front; // Camera forward direction
        public Vector3 Up; // Camera upward direction
        public float Speed; // Camera movement sensitivity
        public float Yaw; // Vertical axis
        public float Pitch; // Horizontal axis
        public float Sensitivity; // Camera mouse sensitivity 
    }

    private Camera camera; // Camera instance to handle movement and view
    private float mouseX, mouseY; // Mouse position from previous frame
    private bool firstMouse = true; // Detects the first mouse movement

    private bool wireframe = false; // Wireframe mode for debugging
        


    public Renderer(GameWindowSettings windowSettings, NativeWindowSettings 
    nativeWindowSettings) : base(windowSettings, nativeWindowSettings)
    {   
        // Initialise camera settings
        camera = new Camera
        {
            Position = new Vector3(0, 0, 5),
            Front = new Vector3(0, 0, -1),
            Up = new Vector3(0, 1, 0),
            Speed = 0.001f,
            Yaw = -90.0f,
            Pitch = 0.0f,
            Sensitivity = 0.05f
        };

        blocks = new List<BlockInstance>(); // Initialise list of blocks
    }



    protected override void OnLoad()
    {
        base.OnLoad();

        // Set window to grab mouse cursor
        this.CursorState = CursorState.Hidden;
        this.CursorState = CursorState.Grabbed;

        //this.VSync = VSyncMode.On;

        // Set clear-color and enable depth testing
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        // Set up projection matrix for perspective view
        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);

        InitialiseShaders();// Initialises shaders for rendering

        // Set wireframe mode as default for debuging
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        GL.Enable(EnableCap.CullFace); // Enable face culling

        // Add block to scene
        AddBlock(new Vector3(0, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
    }



    private void InitialiseShaders()
    {   
        // Initialise shader program
        Shader shader = new Shader();
        shaderProgram = shader.CreateShaderProgram();
        GL.UseProgram(shaderProgram);
    }



    private void AddBlock(Vector3 position, Vector3 scale, Vector3 color, string? texturePath = null)
    {
        // Ensure the texture path is correct, using escaped backslashes or forward slashes
        string correctedTexturePath = @"C:\Users\Aleron\Documents\3DVoxelEngine\Textures\Grass.jpg";

        // Create the block mesh (VAO, VBO, EBO, Texture)
        var (vao, vbo, ebo, texture) = BlockMesh.CreateBlock(correctedTexturePath);

        // Create the model matrix (position and scale)
        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);

        // If a texture path is provided, you don't need to manually load the texture since it's handled by CreateBlock
        // The texture is already loaded as part of CreateBlock

        // Add the block to the scene with the texture
        blocks.Add(new BlockInstance
        {
            ModelMatrix = model,
            Color = color,
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Texture = texture  // Store the texture handle
        });
    }



    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return; // Handle users input if window is focused

        HandleKeyboardInput(); // Handle users keyboard input
        HandleMouseInput(); // Handle users mouse input

        // Update view matrix based on camera position
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);
    }



    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        // Clear screen
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        RenderScene(); // Render the scene
        SwapBuffers(); // Swap buffers to display the rendered frame
    }



    private void RenderBlock(BlockInstance block)
    {
        // Use the shader program for rendering
        GL.UseProgram(shaderProgram);

        // Set projection, view, and model matrices as uniforms
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref block.ModelMatrix);

        // Bind the VAO and draw the block
        GL.BindVertexArray(block.Vao);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);
        GL.BindVertexArray(0); // Unbind the VAO

        // Ensure texture state is consistent for subsequent blocks
        GL.Enable(EnableCap.Texture2D);
    }





    private void RenderScene()
    {
        // Use shader program for rendering
        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);

        // Render each block in the scene
        for (int i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref block.ModelMatrix);
            GL.Uniform3(GL.GetUniformLocation(shaderProgram, "objectColor"), block.Color);
            GL.BindVertexArray(block.Vao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);
            RenderBlock(block);
        }
    }


    private void HandleKeyboardInput()
    {
        var keyboard = KeyboardState;

        // WASDQE movement
        if (keyboard.IsKeyDown(Keys.W)) camera.Position += camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.S)) camera.Position -= camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.A)) camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.D)) camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.Q)) camera.Position += camera.Speed * camera.Up;
        if (keyboard.IsKeyDown(Keys.E)) camera.Position -= camera.Speed * camera.Up;

        // Shift to increase speed
        if (keyboard.IsKeyDown(Keys.LeftShift)) camera.Speed = 0.007f;
        else camera.Speed = 0.001f;

        // Close window
        if (keyboard.IsKeyDown(Keys.Escape)) Close();

        // Toggle cursor visibility
        if (keyboard.IsKeyDown(Keys.D1)) 
        {
            if (CursorState == CursorState.Grabbed) CursorState = CursorState.Normal;
            else CursorState = CursorState.Hidden; CursorState = CursorState.Grabbed;
        }
        
        // Toggle wireframe mode
        if (KeyboardState.IsKeyPressed(Keys.D2))
        {   
            wireframe = !wireframe;
            GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
        } 

        // Reset camera position
        if (keyboard.IsKeyPressed(Keys.D3)) 
        {
            camera.Position = new Vector3(0, 0, 5); 
            camera.Yaw = -90.0f;
            camera.Pitch = 0.0f;
        }
    }



    private void HandleMouseInput()
    {
        var mouse = MouseState; // Get current mouse state
        
        // Check for first mouse movement then initialise last mouse movement
        if (firstMouse)
        {
            mouseX = mouse.X;
            mouseY = mouse.Y;
            firstMouse = false;
        }

        float offsetX = mouse.X - mouseX; // Calculate horizontal mouse movement
        float offsetY = mouseY - mouse.Y; // Calculate vertical mouse movement
        mouseX = mouse.X; // Store current mouse X position
        mouseY = mouse.Y; // Store current mouse Y position

        // Apply sensitivity value to mouse input
        offsetX *= camera.Sensitivity;
        offsetY *= camera.Sensitivity;
        camera.Yaw += offsetX;
        camera.Pitch += offsetY;

        // Prevent camera flipping through pitch rotation
        if (camera.Pitch > 89.0f) camera.Pitch = 89.0f;
        if (camera.Pitch < -89.0f) camera.Pitch = -89.0f;

        // Calculator new vector based on yaw and pitch
        Vector3 front;
        front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
        front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(camera.Pitch));
        front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
        camera.Front = Vector3.Normalize(front);
    }
}
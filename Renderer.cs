using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
    private int shaderProgram; // Shader program for rendering
    private Matrix4 projection, view; // Projection and view matrices for camera setup

    // Camera position, orientation and movement settings structure 
    public struct Camera
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

    //private float mouseX, mouseY; // Mouse position from previous frame
    //private bool firstMouse = true; // Detects the first mouse movement

    private bool wireframe = false; // Wireframe mode for debugging
        
    private Blocks blocks;

    private HandleUserInput handleUserInput;

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

        blocks = new Blocks(); // Initialise list of blocks
        handleUserInput = new HandleUserInput(camera, this);
    }



    protected override void OnLoad()
    {
        base.OnLoad();
        
        // Set up projection matrix and initialise shaders for rendering
        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);
        InitialiseShaders(); 

        // Set window to grab mouse cursor
        //this.CursorState = CursorState.Hidden;
        this.CursorState = CursorState.Grabbed;

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Set clear color
        GL.Enable(EnableCap.DepthTest); // Enable depth test
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); // Enable wireframe mode
        GL.Enable(EnableCap.CullFace); // Enable face culling
        //this.VSync = VSyncMode.On; // Enable VSync
        
        // Add blocks to scene
        blocks.AddGrassBlock(new Vector3(0, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
        blocks.AddGrassBlock(new Vector3(1, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
        blocks.AddStoneBlock(new Vector3(2, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
    }



    private void InitialiseShaders()
    {   
        // Initialise shader program
        Shader shader = new Shader();
        shaderProgram = shader.CreateShaderProgram();
        GL.UseProgram(shaderProgram);
    }



    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return; // Handle users input if window is focused
        
        // Handle keyboard and mouse input
        KeyboardState keyboard = KeyboardState;
        MouseState mouse = MouseState;
        HandleKeyboardInput(keyboard);
        HandleMouseInput(ref camera, mouse);

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



    private void RenderScene()
    {
        // Use shader program for rendering
        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);

        // Get blocks from the Blocks class instance
        List<BlockInstance> blocksList = blocks.GetBlocks();

        // Render each block in the scene
        for (int i = 0; i < blocksList.Count; i++)
        {
            var block = blocksList[i];
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, block.Texture);
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref block.ModelMatrix);
            GL.Uniform3(GL.GetUniformLocation(shaderProgram, "objectColor"), block.Color);
            GL.BindVertexArray(block.Vao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);
            RenderBlock(block);
        }
    }



    private void RenderBlock(BlockInstance block)
    {
        // Bind the VAO and draw the block
        GL.BindVertexArray(block.Vao);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);
        GL.BindVertexArray(0);

        // Ensure texture state is consistent for subsequent blocks
        GL.Enable(EnableCap.Texture2D);
    }



    private void HandleKeyboardInput(KeyboardState keyboard)
    {   
        // Keyboard input for camera movement
        handleUserInput.Movement(ref camera, keyboard);

        // Game window keybinds 
        handleUserInput.WindowKeys(keyboard); 
        
        // Toggle wireframe mode
        if (keyboard.IsKeyPressed(Keys.D2))
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



    private void HandleMouseInput(ref Renderer.Camera camera, MouseState mouseState)
    {
        // Mouse input for camera movement
        handleUserInput.MouseInput(ref camera, mouseState.X, mouseState.Y);
    }
}
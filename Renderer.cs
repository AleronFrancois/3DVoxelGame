using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public class Renderer : GameWindow
{   
    // Camera position, orientation and movement settings structure 
    public struct Camera
    {
        public Vector3 Position; 
        public Vector3 Front;
        public Vector3 Up; 
        public float Speed;
        public float Yaw; 
        public float Pitch; 
        public float Sensitivity;
    }

    private HandleUserInput handleUserInput; // Handle user input for camera movement
    private Matrix4 projection, view; // Projection and view matrices for camera setup
    private Camera camera; // Camera instance to handle movement and view
    private Blocks blocks; // Blocks instance to handle block rendering
    private bool wireframe = false; // Wireframe mode for debugging
    private int shaderProgram; // Shader program for rendering

    // Combined object data to render in one draw call
    private int combinedVbo;
    private int combinedVao;
    private int combinedEbo;

    private Vector3 lastChunkPosition = Vector3.Zero; // Last chunk position for chunk generation
    private bool initialChunkGenerated = false; // Initial chunk generation flag
    private HashSet<Vector3> generatedChunks = new HashSet<Vector3>();

    

    public Renderer(GameWindowSettings windowSettings, NativeWindowSettings 
    nativeWindowSettings) : base(windowSettings, nativeWindowSettings)
    {   
        // Initialise camera settings
        camera = new Camera
        {
            Position = new Vector3(2, 5, 2), // Adjust initial position to be above the center of the chunk
            Front = new Vector3(0, 0, -1),
            Up = new Vector3(0, 1, 0),
            Speed = 0.001f,
            Yaw = -90.0f,
            Pitch = 0.0f,
            Sensitivity = 0.05f
        };

        blocks = new Blocks(); // Initialise list of blocks
        handleUserInput = new HandleUserInput(camera, this); // Initialise user input handler
    }



    protected override void OnLoad()
    {
        base.OnLoad();

        CombineBlockData(); // Combine block data for rendering
        InitialiseShaders(); // Initialise shaders for rendering
        
        // Set up projection matrix and initialise shaders for rendering
        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);
        
        this.CursorState = CursorState.Grabbed; // Hide and lock cursor
        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Set background color
        GL.Enable(EnableCap.DepthTest); // Enable depth testing
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); // Enable wireframe mode
        GL.Enable(EnableCap.CullFace); // Enable face culling
        //this.VSync = VSyncMode.On; // Enable VSync
        
        //blocks.AddGrassBlock(new Vector3(1, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
        //blocks.AddStoneBlock(new Vector3(2, 0, -5), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
    }



    

    private void ChunkSystem(Vector3 cameraPosition)
    {
        int chunkSizeX = 4; // Define the chunk size for X
        int chunkSizeZ = 4; // Define the chunk size for Z

        // Calculate the chunk position based on the camera's X and Z coordinates, lock Y coordinate to 0
        int chunkX = (int)Math.Floor(cameraPosition.X / chunkSizeX) * chunkSizeX;
        int chunkY = 0; // Lock Y coordinate to 0 for level field
        int chunkZ = (int)Math.Floor(cameraPosition.Z / chunkSizeZ) * chunkSizeZ;

        Vector3 currentChunkPosition = new Vector3(chunkX, chunkY, chunkZ);

        // Only generate a new chunk if the camera has moved to a new chunk grid position
        if (!generatedChunks.Contains(currentChunkPosition))
        {
            GenerateChunk(currentChunkPosition);
            generatedChunks.Add(currentChunkPosition);
            lastChunkPosition = currentChunkPosition;
            initialChunkGenerated = true;
            Console.WriteLine($"Chunk generated at: ({chunkX}, {chunkY}, {chunkZ})");
        }
    }



    private void GenerateChunk(Vector3 chunkPosition)
    {
        int chunkSizeX = 4; // Define the chunk dimensions for X
        int chunkSizeY = 1; // Define the chunk dimensions for Y
        int chunkSizeZ = 4; // Define the chunk dimensions for Z

        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int y = 0; y < chunkSizeY; y++)
            {
                for (int z = 0; z < chunkSizeZ; z++)
                {
                    blocks.AddGrassBlock(chunkPosition + new Vector3(x, y - 5, z), new Vector3(1, 1, 1), new Vector3(0.6f, 0f, 0.6f));
                }
            }
        }
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

        ChunkSystem(camera.Position);
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
            Matrix4 modelMatrix = block.ModelMatrix;
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref modelMatrix);
            GL.Uniform3(GL.GetUniformLocation(shaderProgram, "objectColor"), block.Color);
            GL.BindVertexArray(combinedVao);
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
        handleUserInput.WindowKeys(ref camera, keyboard); 
        
        // Toggle wireframe mode
        if (keyboard.IsKeyPressed(Keys.D2))
        {   
            wireframe = !wireframe;
            GL.PolygonMode(MaterialFace.FrontAndBack, wireframe ? PolygonMode.Line : PolygonMode.Fill);
        } 
    }



    private void HandleMouseInput(ref Renderer.Camera camera, MouseState mouseState)
    {
        // Mouse input for camera movement
        handleUserInput.MouseInput(ref camera, mouseState.X, mouseState.Y);
    }



    private void CombineBlockData()
    {   
        // Combine block data for rendering
        List<BlockInstance> blocksList = blocks.GetBlocks();
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        uint indexOffset = 0;

        // Combine vertices and indices
        foreach (var block in blocksList)
        {
            vertices.AddRange(block.Vertices);
            foreach (var index in block.Indices)
            {
                indices.Add(index + indexOffset);
            }
            indexOffset += (uint)(block.Vertices.Length / 8);
        }
        
        // Generate combined VAO, VBO and EBO
        combinedVao = GL.GenVertexArray();
        GL.BindVertexArray(combinedVao);
        combinedVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, combinedVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.StaticDraw);
        combinedEbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, combinedEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        // Set up vertex attributes
        int stride = 8 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.BindVertexArray(0);
    }



    protected override void OnUnload()
    {
        base.OnUnload();

        // Clean up shader program
        GL.DeleteProgram(shaderProgram);

        // Clean up combined buffers
        GL.DeleteBuffer(combinedVbo);
        GL.DeleteBuffer(combinedEbo);
        GL.DeleteVertexArray(combinedVao);
    }
}
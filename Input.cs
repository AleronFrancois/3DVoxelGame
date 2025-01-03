using OpenTK.Graphics.ES11;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public class HandleUserInput
{
    private Renderer.Camera camera; // Get camera from renderer class
    private Renderer renderer; // Get renderer from renderer class

    private float mouseX, mouseY; // Mouse position from previous frame
    private bool firstMouse = true; // Detects the first mouse movement



    // Constructor to initialise with Camera and Renderer
    public HandleUserInput(Renderer.Camera camera, Renderer renderer)
    {
        this.camera = camera;
        this.renderer = renderer;
    }



    public void Movement(ref Renderer.Camera camera, KeyboardState keyboard)
    {
        // Handle WASDQE movement
        if (keyboard.IsKeyDown(Keys.W)) camera.Position += camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.S)) camera.Position -= camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.A)) camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.D)) camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.Q)) camera.Position += camera.Speed * camera.Up;
        if (keyboard.IsKeyDown(Keys.E)) camera.Position -= camera.Speed * camera.Up;

        // Shift to increase speed
        if (keyboard.IsKeyDown(Keys.LeftShift)) camera.Speed = 0.007f;
        else camera.Speed = 0.001f;
    }



    public void WindowKeys(KeyboardState keyboard)
    {
        // Close window
        if (keyboard.IsKeyDown(Keys.Escape)) renderer.Close();

        // Toggle cursor visibility
        if (keyboard.IsKeyPressed(Keys.D1))
        {
            if (renderer.CursorState == CursorState.Grabbed) renderer.CursorState = CursorState.Normal;
            else renderer.CursorState = CursorState.Grabbed;
            
        }
    }



    public void MouseInput(ref Renderer.Camera camera, float mouseX, float mouseY)
    {
        // Check for first mouse movement then initialise last mouse movement
        if (firstMouse)
        {
            this.mouseX = mouseX;
            this.mouseY = mouseY;
            firstMouse = false;
        }

        // Calculate offsets
        float offsetX = mouseX - this.mouseX; // Horizontal movement
        float offsetY = this.mouseY - mouseY; // Vertical movement
        this.mouseX = mouseX; // Store current mouse X position
        this.mouseY = mouseY; // Store current mouse Y position

        // Apply sensitivity value to mouse input
        offsetX *= camera.Sensitivity;
        offsetY *= camera.Sensitivity;

        // Update camera yaw and pitch
        camera.Yaw += offsetX;
        camera.Pitch += offsetY;

        // Prevent camera flipping through pitch rotation
        if (camera.Pitch > 89.0f) camera.Pitch = 89.0f;
        if (camera.Pitch < -89.0f) camera.Pitch = -89.0f;

        // Calculate new front vector based on yaw and pitch
        Vector3 front;
        front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
        front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(camera.Pitch));
        front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
        camera.Front = Vector3.Normalize(front);
    }
}

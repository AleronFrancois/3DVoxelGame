using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;


/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public class Program
{
    public static void Main()
    {
        var windowSettings = new GameWindowSettings();
        var nativeWindowSettings = new NativeWindowSettings { Size = new Vector2i(1200, 700), Title = "3D Engine" };

        using (var renderer = new Renderer(windowSettings, nativeWindowSettings))
        {
            renderer.Run();
        }
    }
}
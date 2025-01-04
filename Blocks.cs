using System.Drawing.Drawing2D;
using OpenTK.Mathematics;


/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public class BlockInstance
{
    public float[] Vertices { get; set; } = Array.Empty<float>();
    public uint[] Indices { get; set; } = Array.Empty<uint>();
    public int Vao { get; set; }
    public int Vbo { get; set; }
    public int Ebo { get; set; }
    public int Texture { get; set; }
    public Matrix4 ModelMatrix { get; set; }
    public Vector3 Color { get; set; }

    // Constructor and other methods...
}



public class Blocks
{
    // List of blocks
    private List<BlockInstance> blocks; 
    public List<BlockInstance> GetBlocks() => blocks;



    // Constructor to initialise list of blocks
    public Blocks()
    {
        blocks = new List<BlockInstance>();
    }



    public void AddGrassBlock(Vector3 position, Vector3 scale, Vector3 color, string? texturePath = null)
    {
        // Texture path
        string TexturePath = @"C:\Users\Aleron\Documents\3DVoxelEngine\Textures\Sand.jpg";

        // Create the block mesh
        var (vao, vbo, ebo, texture) = BlockMesh.CreateBlock(TexturePath);

        // Create the model matrix
        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);

        // Add the block to scene
        blocks.Add(new BlockInstance
        {
            ModelMatrix = model,
            Color = color,
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Texture = texture
        });
    }

    

    public void AddStoneBlock(Vector3 position, Vector3 scale, Vector3 color, string? texturePath = null)
    {
        // Texture path
        string TexturePath = @"C:\Users\Aleron\Documents\3DVoxelEngine\Textures\Stone.png";

        // Create the block mesh
        var (vao, vbo, ebo, texture) = BlockMesh.CreateBlock(TexturePath);

        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);

        // Add the block to scene
        blocks.Add(new BlockInstance
        {
            ModelMatrix = model,
            Color = color,
            Vao = vao,
            Vbo = vbo,
            Ebo = ebo,
            Texture = texture
        });
    }
}

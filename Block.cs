using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging; 
using System;


/// ---------------3D Game Engine---------------
/// 
/// [Author]          > Aleron Francois
/// [Date Started]    > 23/12/2024
/// [Current state]   > Currently in development
/// 
/// --------------------------------------------


public static class BlockMesh
{
    private static float[] vertices = new float[]
    {
        // Back face
        1f, 1f, 0f,  1f, 1f,
        1f, 0f, 0f,  1f, 0f,
        0f, 0f, 0f,  0f, 0f,
        0f, 1f, 0f,  0f, 1f, 
        
        // Front face
        0f, 1f, 1f,  0f, 1f,
        0f, 0f, 1f,  0f, 0f,
        1f, 0f, 1f,  1f, 0f,
        1f, 1f, 1f,  1f, 1f,

        // Right face
        1f, 1f, 1f,  1f, 1f,
        1f, 0f, 1f,  1f, 0f,
        1f, 0f, 0f,  0f, 0f,
        1f, 1f, 0f,  0f, 1f,

        // Left face
        0f, 1f, 0f,  0f, 1f,
        0f, 0f, 0f,  0f, 0f,
        0f, 0f, 1f,  1f, 0f,
        0f, 1f, 1f,  1f, 1f,

        // Top face
        0f, 1f, 1f,  0f, 1f,
        1f, 1f, 1f,  1f, 1f,
        1f, 1f, 0f,  1f, 0f,
        0f, 1f, 0f,  0f, 0f,

        // Bottom face
        0f, 0f, 1f,  0f, 1f,
        0f, 0f, 0f,  0f, 0f,
        1f, 0f, 0f,  1f, 0f,
        1f, 0f, 1f,  1f, 1f
    };

    private static ushort[] indices = new ushort[]
    {
        0, 1, 3, 3, 1, 2, // Back face
        4, 5, 7, 7, 5, 6, // Front face
        8, 9, 11, 11, 9, 10, // Right face
        12, 13, 15, 15, 13, 14, // Left face
        16, 17, 19, 19, 17, 18, // Top face
        20, 21, 23, 23, 21, 22  // Bottom face
    };



    public static (int VAO, int VBO, int EBO, int texture) CreateBlock(string textureFilePath)
    {   
        // Generate OpenGL objects
        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        int ebo = GL.GenBuffer();
        int texture = LoadTexture(textureFilePath);

        // Set up vao, vbo and ebo
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.BindVertexArray(0);

        // Return OpenGL object
        return (vao, vbo, ebo, texture);
    }



    private static int LoadTexture(string filePath)
    {
        int texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texture);

        using (var bitmap = new Bitmap(filePath))
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                        ImageLockMode.ReadOnly,
                                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                          OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return texture;
    }
}



public class BlockFaceMesh
{
    public int VAO { get; private set; }
    public int VBO { get; private set; }
    public int EBO { get; private set; }

    public BlockFaceMesh(float[] vertices, ushort[] indices)
    {
        // Generate OpenGL objects
        VAO = GL.GenVertexArray();
        VBO = GL.GenBuffer();
        EBO = GL.GenBuffer();

        // Bind VAO
        GL.BindVertexArray(VAO);

        // Bind and buffer vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Bind and buffer index data
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);

        // Set up vertex attribute pointers (positions only)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Unbind VAO
        GL.BindVertexArray(0);
    }

    public void Render()
    {
        // Bind VAO and render elements
        GL.BindVertexArray(VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, 0);
        GL.BindVertexArray(0);
    }
}
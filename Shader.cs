using OpenTK.Graphics.OpenGL4;
using System;

class Shader
{
    public int CreateShaderProgram() 
    {
        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec2 aTexCoord;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec2 TexCoord;
            out vec3 fragPos;

            void main()
            {
                fragPos = aPos;
                gl_Position = projection * view * model * vec4(aPos, 1.0);
                TexCoord = aTexCoord;
            }";

        string fragmentShaderSource = @"
            #version 330 core
            in vec2 TexCoord;
            in vec3 fragPos;
            out vec4 FragColor;

            uniform sampler2D texture1;

            void main()
            {
                vec3 color = abs(fragPos);
                FragColor = texture(texture1, TexCoord);
            }";

        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

        int shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);

        return shaderProgram;
    }

    private int CompileShader(ShaderType type, string source) 
    {
        int shader = GL.CreateShader(type);
        string infoLog = GL.GetShaderInfoLog(shader);

        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        if (!string.IsNullOrEmpty(infoLog)) 
        {
            Console.WriteLine($"Error compiling shader: {infoLog}");
        }

        return shader;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.opentk_control.shader_compiler
{
    public static class ShaderLibrary
    {

        public enum ShaderType
        {
            MeshShader,
            TextShader,
            SelectionShader
        }


        #region "Vertex Shaders"

        private static string mesh_vert_shader()
        {
            return @"

#version 330 core

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

uniform float vertexTransparency; // Transparency of the mesh

layout(location = 0) in vec2 node_position;
layout(location = 1) in vec3 vertexColor;
layout(location = 2) in float is_dynamic;
layout(location = 3) in float deflscale; // Deflection scale value = normalized_deflscale (varies 0 to 1) * max deformation

out vec3 v_Color;
out float v_is_dynamic;
out float v_deflscale;
out float v_Transparency;

void main()
{
    // To handle dynamic drawing
    v_is_dynamic = is_dynamic;
    v_deflscale = deflscale;

    // Set the point color and transparency
    v_Color = vertexColor;
    v_Transparency = vertexTransparency;

    // Final position with projection matrix (fixes clipping issues)
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(node_position, 0.0, 1.0);
}

                    ";

        }


        private static string selrect_vert_shader()
        {
            return @"

#version 330 core

layout(location = 0) in vec2 node_position;

out vec4 v_Color;

void main()
{
	v_Color = vec4(0.8039f,0.3608f,0.3608f,0.5f);

	// Final position passed to fragment shader
	gl_Position = vec4(node_position,0.0f,1.0f);
}

                    ";

        }


        public static string text_vert_shader()
        {
            return @"

#version 330 core

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

uniform float vertexTransparency; // Transparency of the mesh

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 origin;
layout(location = 2) in vec2 textureCoord;
layout(location = 3) in vec3 textColor;

out vec4 v_textureColor;
out vec2 v_textureCoord;

void main()
{

	// apply Translation to the final position 
	vec4 finalPosition =  projectionMatrix * viewMatrix * modelMatrix * vec4(position,0.0f,1.0f);

	// apply Translation to the text origin
	vec4 finalTextorigin =  projectionMatrix * viewMatrix * modelMatrix * vec4(origin,0.0f,1.0f);
    
    float zoomscale = viewMatrix[0][0];
	// Remove the zoom scale
	vec2 scaled_pt = vec2(finalPosition.x - finalTextorigin.x,finalPosition.y - finalTextorigin.y) / zoomscale;
		
	// Set the final position of the vertex
	gl_Position = vec4(scaled_pt.x + finalTextorigin.x, scaled_pt.y + finalTextorigin.y, 0.0f, 1.0f);

	// Calculate texture coordinates for the glyph
	v_textureCoord = textureCoord;
	
	// Pass the texture color to the fragment shader
	v_textureColor = vec4(textColor,vertexTransparency);
}

                    ";

        }



        #endregion

        #region "Fragment shaders"

        private static string mesh_frag_shader()
        {

            return @"

#version 330 core

in vec3 v_Color;
in float v_is_dynamic;
in float v_deflscale;
in float v_Transparency;

out vec4 f_Color; // fragment's final color (out to the fragment shader)


vec3 jetHeatmap(float value) 
{

    return clamp(vec3(1.5) - abs(4.0 * vec3(value) + vec3(-3, -2, -1)), vec3(0), vec3(1));
}


void main() 
{

    vec3 vertexColor = v_Color;
    
    if (v_is_dynamic == 1.0f)
    {
        vertexColor = jetHeatmap(v_deflscale);
    }

    f_Color = vec4(vertexColor, v_Transparency); // Set the final color
}

                    ";

        }


        private static string selrect_frag_shader()
        {
            return @"

#version 330 core

in vec4 v_Color;

out vec4 f_Color; // fragment's final color (out to the fragment shader)

void main()
{
	f_Color = v_Color;
}

                    ";

        }


        public static string text_frag_shader()
        {
            return @"

#version 330 core
uniform sampler2D u_Texture;

in vec4 v_textureColor;
in vec2 v_textureCoord;

out vec4 f_Color; // fragment's final color (out to the fragment shader)

void main()
{
	vec4 texColor = vec4(1.0, 1.0, 1.0, texture(u_Texture, v_textureCoord).r);
	f_Color = v_textureColor * texColor;
}

                    ";

        }



        #endregion

        public static string get_vertex_shader(ShaderType type)
        {
            // Returns the vertex shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_vert_shader();
                case ShaderType.SelectionShader:
                    return selrect_vert_shader();
                case ShaderType.TextShader:
                    return text_vert_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }

        public static string get_fragment_shader(ShaderType type)
        {
            // Returns the fragment shader
            switch (type)
            {
                case ShaderType.MeshShader:
                    return mesh_frag_shader();
                case ShaderType.SelectionShader:
                    return selrect_frag_shader();
                case ShaderType.TextShader:
                    return text_frag_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }
    }
}

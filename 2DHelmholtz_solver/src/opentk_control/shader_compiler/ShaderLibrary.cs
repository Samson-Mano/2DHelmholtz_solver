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
            RsltMeshShader,
            ModalRsltMeshShader,

            ChladniRsltMeshShader,
            TextShader,
            SelectionShader,

            DrawingAxisShader,
            RsltTextShader,

            ContourBarShader,
        }


        #region "Mesh Shaders"

        private static string mesh_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP; 
            uniform float vertexTransparency; // Transparency of the mesh

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in vec3 vertexColor;

            out vec4 v_Color;

            void main()
            {
                
                // Set the point color and transparency
                v_Color = vec4(vertexColor, vertexTransparency);

                // Final position with projection matrix
                gl_Position = uMVP * vec4(node_position, 0.0, 1.0);
            }


                    ";

        }




        private static string mesh_frag_shader()
        {

            return @"

            #version 330 core

            in vec4 v_Color;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)


            void main() 
            {

                f_Color = v_Color; // Set the final color
            }


                    ";

        }

        #endregion




        #region "Result Mesh Shaders"

        private static string rsltmesh_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP; 

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in float deflscale;

            out float v_deflscale;

            void main()
            {
                
                v_deflscale = deflscale;

                // Final position with projection matrix
                gl_Position = uMVP * vec4(node_position, 0.0, 1.0);
            }


                    ";

        }




        private static string rsltmesh_frag_shader()
        {

            return @"

            #version 330 core

            uniform float vertexTransparency; // Transparency of the mesh

            in float v_deflscale;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)


            vec3 jetHeatmap(float value) 
            {
                float t = value;

                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            void main() 
            {

                vec3 vertexColor = jetHeatmap(v_deflscale);

                f_Color = vec4(vertexColor, vertexTransparency); // Set the final color
            }


                    ";

        }

        #endregion



        #region "Modal Result Mesh Shaders"

        private static string modalrsltmesh_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP; 

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in float deflscale;

            out float v_deflscale;

            void main()
            {
                
                v_deflscale = deflscale;
      
                // Final position with projection matrix
                gl_Position = uMVP * vec4(node_position, 0.0, 1.0);
            }

                    ";

        }




        private static string modalrsltmesh_frag_shader()
        {

            return @"


            #version 330 core

            uniform float vertexTransparency; // Transparency of the mesh
            uniform float sinevalue = 1.0;


            uniform float uNodeWidth = 0.08;    // how thick the node lines are (try 0.02 – 0.08)
            uniform float uColormapMode = 2.0; // 0 = plasma, 1 = inferno, 2 = magma, 3 = greys
            uniform float uDisplacementScale = 1.0; // max |deflscale| for normalization


            in float v_deflscale;


            out vec4 f_Color; // fragment's final color (out to the fragment shader)



            vec3 jetHeatmap(float value) 
            {
                float t  = (value + 1.0) * 0.5; // Normalize to [0, 1] for oscillation
      
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            // ---------------------------------------------------------------
            // Colormaps (approximated, good enough for visualization)
            // Each takes t in [0,1] and returns an RGB color.
            // ---------------------------------------------------------------

            vec3 plasma(float t)
            {
                // P. Bourke's polynomial fit of the plasma colormap
                const vec3 c0 = vec3(0.050383, 0.029803, 0.527975);
                const vec3 c1 = vec3(0.063536, 0.028426, 0.533124);
                const vec3 c2 = vec3(0.075353, 0.027206, 0.538007);
                const vec3 c3 = vec3(0.086222, 0.026125, 0.542658);
                const vec3 c4 = vec3(0.096379, 0.025165, 0.547103);
                const vec3 c5 = vec3(0.105980, 0.024309, 0.551368);
                const vec3 c6 = vec3(0.115124, 0.023556, 0.555468);
                // Simpler 3-stop approximation:
                vec3 a = vec3(0.050, 0.030, 0.528);
                vec3 b = vec3(0.798, 0.280, 0.470);
                vec3 c = vec3(0.940, 0.975, 0.131);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }

            vec3 inferno(float t)
            {
                vec3 a = vec3(0.001, 0.000, 0.014);
                vec3 b = vec3(0.735, 0.215, 0.330);
                vec3 c = vec3(0.988, 0.998, 0.645);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }

            vec3 magma(float t)
            {
                vec3 a = vec3(0.001, 0.000, 0.014);
                vec3 b = vec3(0.716, 0.215, 0.475);
                vec3 c = vec3(0.987, 0.991, 0.750);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }


            vec3 greys(float t)
            {
                return vec3(1.0 - t);
            }


            vec3 viridis(float t)
            {
                vec3 a = vec3(0.267, 0.005, 0.329);
                vec3 b = vec3(0.188, 0.407, 0.554);
                vec3 c = vec3(0.208, 0.718, 0.472);
                vec3 d = vec3(0.993, 0.906, 0.144);
                if (t < 0.333) return mix(a, b, t * 3.0);
                if (t < 0.666) return mix(b, c, (t - 0.333) * 3.0);
                return mix(c, d, (t - 0.666) * 3.0);
            }


            vec3 cividis(float t)
            {
                vec3 a = vec3(0.000, 0.135, 0.305);
                vec3 b = vec3(0.359, 0.377, 0.443);
                vec3 c = vec3(0.739, 0.719, 0.521);
                vec3 d = vec3(1.000, 0.988, 0.269);
                if (t < 0.333) return mix(a, b, t * 3.0);
                if (t < 0.666) return mix(b, c, (t - 0.333) * 3.0);
                return mix(c, d, (t - 0.666) * 3.0);
            }


            vec3 turbo(float t)
            {
                vec3 a = vec3(0.190, 0.072, 0.232);
                vec3 b = vec3(0.271, 0.528, 0.979);
                vec3 c = vec3(0.180, 0.849, 0.521);
                vec3 d = vec3(0.973, 0.769, 0.188);
                vec3 e = vec3(0.921, 0.137, 0.094);
                if (t < 0.25) return mix(a, b, t * 4.0);
                if (t < 0.50) return mix(b, c, (t - 0.25) * 4.0);
                if (t < 0.75) return mix(c, d, (t - 0.50) * 4.0);
                return mix(d, e, (t - 0.75) * 4.0);
            }


            vec3 aurora(float t)
            {
                vec3 a = vec3(0.02, 0.02, 0.08);
                vec3 b = vec3(0.10, 0.55, 0.45);
                vec3 c = vec3(0.35, 0.95, 0.65);
                vec3 d = vec3(0.85, 0.60, 0.95);
                if (t < 0.33) return mix(a, b, t * 3.0);
                if (t < 0.66) return mix(b, c, (t - 0.33) * 3.0);
                return mix(c, d, (t - 0.66) * 3.0);
            }


            vec3 ember(float t)
            {
                vec3 a = vec3(0.05, 0.00, 0.00);
                vec3 b = vec3(0.55, 0.05, 0.02);
                vec3 c = vec3(0.95, 0.45, 0.05);
                vec3 d = vec3(1.00, 0.95, 0.65);
                if (t < 0.33) return mix(a, b, t * 3.0);
                if (t < 0.66) return mix(b, c, (t - 0.33) * 3.0);
                return mix(c, d, (t - 0.66) * 3.0);
            }


            vec3 ocean(float t)
            {
                vec3 a = vec3(0.00, 0.02, 0.10);
                vec3 b = vec3(0.00, 0.20, 0.45);
                vec3 c = vec3(0.05, 0.55, 0.70);
                vec3 d = vec3(0.75, 0.95, 0.95);
                if (t < 0.33) return mix(a, b, t * 3.0);
                if (t < 0.66) return mix(b, c, (t - 0.33) * 3.0);
                return mix(c, d, (t - 0.66) * 3.0);
            }


            vec3 sunset(float t)
            {
                vec3 a = vec3(0.15, 0.02, 0.30);
                vec3 b = vec3(0.75, 0.15, 0.45);
                vec3 c = vec3(0.98, 0.55, 0.20);
                vec3 d = vec3(1.00, 0.95, 0.70);
                if (t < 0.33) return mix(a, b, t * 3.0);
                if (t < 0.66) return mix(b, c, (t - 0.33) * 3.0);
                return mix(c, d, (t - 0.66) * 3.0);
            }


            vec3 neon(float t)
            {
                vec3 a = vec3(0.00, 0.95, 1.00);
                vec3 b = vec3(1.00, 0.00, 0.85);
                vec3 c = vec3(1.00, 0.95, 0.00);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }


            vec3 forest(float t)
            {
                vec3 a = vec3(0.05, 0.10, 0.05);
                vec3 b = vec3(0.25, 0.45, 0.15);
                vec3 c = vec3(0.65, 0.70, 0.25);
                vec3 d = vec3(0.95, 0.85, 0.45);
                if (t < 0.33) return mix(a, b, t * 3.0);
                if (t < 0.66) return mix(b, c, (t - 0.33) * 3.0);
                return mix(c, d, (t - 0.66) * 3.0);
            }


            vec3 ridged(float t)
            {
                float bands = 8.0;
                float r = abs(fract(t * bands) - 0.5) * 2.0; // triangle wave 0..1
                vec3 base = plasma(t);
                // Modulate brightness by the ridge
                return base * (0.55 + 0.45 * r);
            }


            vec3 iridescent(float t)
            {
                // Sine-based palette (Inigo Quilez style)
                vec3 a = vec3(0.5, 0.5, 0.5);
                vec3 b = vec3(0.5, 0.5, 0.5);
                vec3 c = vec3(1.0, 1.0, 1.0);
                vec3 d = vec3(0.00, 0.33, 0.67);
                return a + b * cos(6.28318 * (c * t + d));
            }


            vec3 topographic(float t)
            {
                float bands = 12.0;
                float band  = floor(t * bands) / bands;
                return plasma(band);
            }


            vec3 firestorm(float t)
            {
                vec3 base = ember(t);
                float flicker = 0.9 + 0.1 * sin(t * 60.0);
                return base * flicker;
            }


            vec3 chrome(float t)
            {
                vec3 a = vec3(0.1, 0.1, 0.12);
                vec3 b = vec3(0.9, 0.9, 0.95);
                vec3 c = vec3(0.3, 0.3, 0.35);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }


            vec3 applyColormap(float t)
            {
                 int mode = int(uColormapMode + 0.5);
                if (mode == 0) return plasma(t);
                if (mode == 1) return inferno(t);
                if (mode == 2) return magma(t);
                if (mode == 3) return greys(t);
                if (mode == 4) return viridis(t);
                if (mode == 5) return cividis(t);
                if (mode == 6) return turbo(t);
                if (mode == 7) return aurora(t);
                if (mode == 8) return ember(t);
                if (mode == 9) return ocean(t);
                if (mode == 10) return sunset(t);
                if (mode == 11) return neon(t);
                if (mode == 12) return forest(t);
                if (mode == 13) return ridged(t);
                if (mode == 14) return iridescent(t);
                if (mode == 15) return topographic(t);
                if (mode == 16) return firestorm(t);
                if (mode == 17) return chrome(t);
                return plasma(t);
            }

            void main() 
            {

                vec3 vertexColor = vec3(0.0, 0.0, 0.0); // Default color

                if(sinevalue < 1.1)
                {
                    // Animation mode: Use sinevalue to modulate the color
                    vertexColor = jetHeatmap(v_deflscale * sinevalue);
                }
                else
                {
                    // Static mode: Plot the chladni pattern using the deflection scale
                    // 1) Normalize the signed deflection to [0, 1]
                    float scaled = v_deflscale / max(uDisplacementScale, 1e-6);

                    // 2) Distance from the node line (|d| = 0 means on a node)
                    float nodeDist = abs(scaled);

                    // 3) Base color from colormap of |deflection|
                    //    Node regions (small |d|) map to low t, antinodes to high t.
                    vec3 base = applyColormap(nodeDist);

                    // 4) Chladni node-line highlight:
                    //    Use a sharp falloff so only the very-near-zero region lights up.
                    //    smoothstep(edge0, edge1, x) — we invert it so nodeDist=0 -> 1.
                    float nodeLine = 1.0 - smoothstep(0.0, uNodeWidth, nodeDist);

                    // Boost the node line to white; keep antinodes at base color.
                    vec3 nodeColor = vec3(1.0); // pure white node lines

                    // Blend: where nodeLine = 1, we get white; where 0, we get base.
                    vertexColor = mix(base, nodeColor, nodeLine);

                    // 5) Optional: darken the antinodes slightly so nodes pop more.
                    //    Comment out if you want the colormap to dominate.
                    vertexColor *= mix(0.35, 1.0, nodeDist);
                }   

                f_Color = vec4(vertexColor, vertexTransparency); // Set the final color
            }


                    ";

        }





        private static string modalrsltmesh_frag_shader11()
        {

            return @"


            #version 330 core

            in float v_deflscale;
            in float v_Transparency;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)



            vec3 jetHeatmap(float value) 
            {
                float t  = (value + 1.0) * 0.5; // Normalize to [0, 1] for oscillation
      
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            void main() 
            {

                vec3 vertexColor = jetHeatmap(v_deflscale);

                f_Color = vec4(vertexColor, v_Transparency); // Set the final color
            }


                    ";

        }

        #endregion




        #region "Chladni Result Mesh Shaders"

        private static string chladnirsltmesh_vert_shader()
        {
            return @"

                #version 330 core

                uniform mat4 uMVP;
                uniform float vertexTransparency;

                layout(location = 0) in vec2 node_position;
                layout(location = 1) in float deflscale;

                out float v_deflscale;
                out float v_Transparency;

                void main()
                {
                    v_deflscale   = deflscale;
                    v_Transparency = vertexTransparency;
                    gl_Position   = uMVP * vec4(node_position, 0.0, 1.0);
                }


                    ";

        }




        private static string chladnirsltmesh_frag_shader()
        {

            return @"


            #version 330 core

            in float v_deflscale;
            in float v_Transparency;

            out vec4 f_Color;

            uniform float uNodeWidth = 0.08;    // how thick the node lines are (try 0.02 – 0.08)
            uniform float uColormapMode = 1.0; // 0 = plasma, 1 = inferno, 2 = magma, 3 = greys
            uniform float uDisplacementScale = 1.0; // max |deflscale| for normalization

            // ---------------------------------------------------------------
            // Colormaps (approximated, good enough for visualization)
            // Each takes t in [0,1] and returns an RGB color.
            // ---------------------------------------------------------------

            vec3 plasma(float t)
            {
                // P. Bourke's polynomial fit of the plasma colormap
                const vec3 c0 = vec3(0.050383, 0.029803, 0.527975);
                const vec3 c1 = vec3(0.063536, 0.028426, 0.533124);
                const vec3 c2 = vec3(0.075353, 0.027206, 0.538007);
                const vec3 c3 = vec3(0.086222, 0.026125, 0.542658);
                const vec3 c4 = vec3(0.096379, 0.025165, 0.547103);
                const vec3 c5 = vec3(0.105980, 0.024309, 0.551368);
                const vec3 c6 = vec3(0.115124, 0.023556, 0.555468);
                // Simpler 3-stop approximation:
                vec3 a = vec3(0.050, 0.030, 0.528);
                vec3 b = vec3(0.798, 0.280, 0.470);
                vec3 c = vec3(0.940, 0.975, 0.131);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }

            vec3 inferno(float t)
            {
                vec3 a = vec3(0.001, 0.000, 0.014);
                vec3 b = vec3(0.735, 0.215, 0.330);
                vec3 c = vec3(0.988, 0.998, 0.645);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }

            vec3 magma(float t)
            {
                vec3 a = vec3(0.001, 0.000, 0.014);
                vec3 b = vec3(0.716, 0.215, 0.475);
                vec3 c = vec3(0.987, 0.991, 0.750);
                if (t < 0.5) return mix(a, b, t * 2.0);
                return mix(b, c, (t - 0.5) * 2.0);
            }

            vec3 greys(float t)
            {
                return vec3(t);
            }

            vec3 applyColormap(float t)
            {
                if (uColormapMode < 0.5)      return plasma(t);
                else if (uColormapMode < 1.5) return inferno(t);
                else if (uColormapMode < 2.5) return magma(t);
                else                          return greys(t);
            }

            void main()
            {
                // 1) Normalize the signed deflection to [0, 1]
                float scaled = v_deflscale / max(uDisplacementScale, 1e-6);

                // 2) Distance from the node line (|d| = 0 means on a node)
                float nodeDist = abs(scaled);

                // 3) Base color from colormap of |deflection|
                //    Node regions (small |d|) map to low t, antinodes to high t.
                vec3 base = applyColormap(nodeDist);

                // 4) Chladni node-line highlight:
                //    Use a sharp falloff so only the very-near-zero region lights up.
                //    smoothstep(edge0, edge1, x) — we invert it so nodeDist=0 -> 1.
                float nodeLine = 1.0 - smoothstep(0.0, uNodeWidth, nodeDist);

                // Boost the node line to white; keep antinodes at base color.
                vec3 nodeColor = vec3(1.0); // pure white node lines

                // Blend: where nodeLine = 1, we get white; where 0, we get base.
                vec3 color = mix(base, nodeColor, nodeLine);

                // 5) Optional: darken the antinodes slightly so nodes pop more.
                //    Comment out if you want the colormap to dominate.
                color *= mix(0.35, 1.0, nodeDist);

                f_Color = vec4(color, v_Transparency);
            }


                    ";

        }

        #endregion








        #region "Text shaders"

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
    
    float zoomscale = 1.0f; //viewMatrix[0][0];
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





        #region "Result Text shaders"

        public static string rslttext_vert_shader()
        {
            return @"

            #version 330 core

            uniform mat4 uMVP;           // Model-View-Projection matrix
            uniform float zoomscale = 1.0f;

            uniform float vertexTransparency = 1.0f; // Transparency of the mesh

            layout(location = 0) in vec2 position;
            layout(location = 1) in vec2 origin;
            layout(location = 2) in vec2 textureCoord;
            layout(location = 3) in vec3 textColor;

            out vec4 v_textureColor;
            out vec2 v_textureCoord;

            void main()
            {

	            // apply Translation to the final position 
	            vec4 finalPosition =  uMVP * vec4(position,0.0f,1.0f);

	            // apply Translation to the text origin
	            vec4 finalTextorigin =  uMVP * vec4(origin,0.0f,1.0f);
    

	            // Remove the zoom scale
	            vec2 scaled_pt = vec2(finalPosition.x - finalTextorigin.x,finalPosition.y - finalTextorigin.y) / zoomscale;
		
	            // Set the final position of the vertex
	            gl_Position = vec4(scaled_pt.x + finalTextorigin.x, scaled_pt.y + finalTextorigin.y, 0.0f, 1.0f);


	            // Calculate texture coordinates for the glyph
	            v_textureCoord = textureCoord;
	
	            // Pass the texture color to the fragment shader
	            v_textureColor = vec4(textColor, vertexTransparency);
            }

                    ";

        }


        public static string rslttext_frag_shader()
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



        #region "Selection Shader"



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



        #endregion



        #region "Drawing Axis Shader"

        private static string drawingaxis_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in vec3 node_color;

            out vec4 v_Color;

            void main()
            {
	            v_Color = vec4(node_color, 1.0f);

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string drawingaxis_frag_shader()
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


        #endregion


        #region "Contour Bar Shader"

        private static string contourbar_vert_shader()
        {
            return @"

            #version 330 core

            layout(location = 0) in vec2 node_position;
            layout(location = 1) in float node_value; // Value for the contour level between 0.0 and 1.0

            out float v_node_value;

            void main()
            {
	            // Map the node_value to a color (e.g., from blue to red)
	            v_node_value = node_value;

	            // Final position passed to fragment shader
	            gl_Position = vec4(node_position,0.0f,1.0f);
            }

                    ";

        }



        private static string contourbar_frag_shader()
        {
            return @"

            #version 330 core

            in float v_node_value;

            out vec4 f_Color; // fragment's final color (out to the fragment shader)
            
             vec3 jetHeatmap(float value) 
            {
                float t = value; // (value + 1.0) * 0.5;
                return clamp(vec3(1.5) - abs(4.0 * vec3(t) + vec3(-3, -2, -1)), vec3(0), vec3(1));
            }


            void main()
            {
                vec3 contourColor = vec3(0.0); 
                
                if(v_node_value < 0.0f)
                    contourColor = vec3(0.4f, 0.4f, 0.4f); // Dark gray for negative values
                else if(v_node_value > 1.0f)
                    contourColor = vec3(0.8f, 0.8f, 0.8f); // Light gray for values greater than 1
                else
                    contourColor = jetHeatmap(v_node_value); // Use the heatmap for values between 0 and 1


	            f_Color = vec4(contourColor, 1.0f);
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
                case ShaderType.RsltMeshShader:
                    return rsltmesh_vert_shader();
                case ShaderType.ModalRsltMeshShader:
                    return modalrsltmesh_vert_shader();
                case ShaderType.ChladniRsltMeshShader:
                    return chladnirsltmesh_vert_shader();
                case ShaderType.SelectionShader:
                    return selrect_vert_shader();
                case ShaderType.TextShader:
                    return text_vert_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_vert_shader();
                case ShaderType.RsltTextShader:
                    return rslttext_vert_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_vert_shader();
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
                case ShaderType.RsltMeshShader:
                    return rsltmesh_frag_shader();
                case ShaderType.ModalRsltMeshShader:
                    return modalrsltmesh_frag_shader();
                case ShaderType.ChladniRsltMeshShader:
                    return chladnirsltmesh_frag_shader();
                case ShaderType.SelectionShader:
                    return selrect_frag_shader();
                case ShaderType.TextShader:
                    return text_frag_shader();
                case ShaderType.DrawingAxisShader:
                    return drawingaxis_frag_shader();
                case ShaderType.RsltTextShader:
                    return rslttext_frag_shader();
                case ShaderType.ContourBarShader:
                    return contourbar_frag_shader();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown shader type");

            }
        }
    }
}

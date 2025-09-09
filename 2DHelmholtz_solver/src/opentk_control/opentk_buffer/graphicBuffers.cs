using _2DHelmholtz_solver.opentk_control.opentk_buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.src.opentk_control.opentk_buffer
{
    public class graphicBuffers
    {
        VertexBuffer vbo;
        VertexArray vao;
        IndexBuffer ibo;

        public graphicBuffers(float[] vertexbuffer_data, int vertexbuffer_size,
            int[] indexbuffer_indices, int indexbuffer_count,
            VertexBufferLayout vertexbuffer_layout, bool is_DynamicDraw)
        {
            
            // Initialize the vertex array
            vao = new VertexArray();

            // Vertex buffer (vertices and number of vertices * sizeof(float))
            vbo = new VertexBuffer(vertexbuffer_data, vertexbuffer_size, is_DynamicDraw);

            // Index buffer (indices and number of indices)
            ibo = new IndexBuffer(indexbuffer_indices, indexbuffer_count);

            // Vertex Array (vertex buffer and vertex buffer layout) 
            vao.Add_vertexBuffer(vbo, vertexbuffer_layout);

        }


        public void UpdateDynamicVertexBuffer(float[] vertexbuffer_data, int vertexbuffer_size)
        {
            // Dynamically update the vertex data to the Vertex Buffer
            vbo.updateVertexBuffer(vertexbuffer_data, vertexbuffer_size);

        }


        public void Bind()
        {
            // Bind the buffers
            vao.Bind();
            ibo.Bind();

        }

        public void UnBind()
        {
            // Un Bind the buffers
            vao.UnBind();
            ibo.UnBind();   

        }

    }
}

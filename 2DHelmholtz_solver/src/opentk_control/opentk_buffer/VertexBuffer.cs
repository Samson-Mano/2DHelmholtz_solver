// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DHelmholtz_solver.opentk_control.opentk_buffer
{
  public  class VertexBuffer
    {
        private int _m_renderer_id;

       // public int m_rendered_id { get { return this._m_renderer_id; } }

        // size is byte

        public VertexBuffer(float[] vertexbuffer_data, int vertexbuffer_size, bool is_DynamicDraw)
        {
            // Main Constructor
            // Set up vertex buffer
            this._m_renderer_id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._m_renderer_id);

            if (is_DynamicDraw == false)
            {
                // Setup static buffer
                GL.BufferData(BufferTarget.ArrayBuffer, vertexbuffer_size, vertexbuffer_data, BufferUsageHint.StaticDraw);
            }
            else
            {
                // Setup dynamic buffer
                GL.BufferData(BufferTarget.ArrayBuffer, vertexbuffer_size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }

        }


        public void updateVertexBuffer(float[] vertexbuffer_data, int vertexbuffer_size)
        {
            // Important!! Call only in Dynamic Buffer case
            // Update the vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._m_renderer_id);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexbuffer_size, vertexbuffer_data);

        }


        public void Bind()
        {
            // Bind buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._m_renderer_id);
        }

        public void UnBind()
        {
            // Unbind with 0
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete_VertexBuffer()
        {
            // Delete this buffer (acts like a  destructor)
            GL.DeleteBuffer(this._m_renderer_id);
        }
    }
}

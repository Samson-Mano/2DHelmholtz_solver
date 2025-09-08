using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace _2DHelmholtz_solver.opentk_control.opentk_buffer
{
   public class IndexBuffer
    {
        private int _m_renderer_id;
        private int _m_count;

        public int GetCount { get { return this._m_count; } }

        // count is element count
        public IndexBuffer(int[] indexbuffer_indices, int indexbuffer_count)
        {
            // Main Constructor
            // Set up Index buffer
            this._m_count = indexbuffer_count;

            // Generate a new buffer ID for the index buffer
            this._m_renderer_id = GL.GenBuffer();

            // Bind the buffer to the GL_ELEMENT_ARRAY_BUFFER target
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._m_renderer_id);

            // Copy the index data to the buffer
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexbuffer_count * sizeof(uint), indexbuffer_indices, BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            // Bind the buffer to the GL_ELEMENT_ARRAY_BUFFER target
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._m_renderer_id);
        }

        public void UnBind()
        {
            // Unbind the buffer from the GL_ELEMENT_ARRAY_BUFFER target
            // Unbind with 0
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Delete_IndexBuffer()
        {
            // Delete this buffer (acts like a  destructor)
            GL.DeleteBuffer(this._m_renderer_id);
        }
    }
}

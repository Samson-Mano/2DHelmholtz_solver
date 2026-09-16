using _2DHelmholtz_solver.opentk_control.opentk_buffer;
// OpenTK library
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace _2DHelmholtz_solver.src.opentk_control.opentk_buffer
{

    public class VertexBufferR1 : IDisposable
    {
        private int _rendererId;
        private int _capacity;  // Current capacity in bytes
        private int _size;      // Current used size in bytes
        private bool _disposed = false;
        private List<float> _localBuffer = new List<float>();  // Local copy of all vertex data

        public int Size => _size;
        public int Capacity => _capacity;


        public VertexBufferR1(int vertexbuffer_count = 10)  // Note: Data count is the number of float count
        {
            // Main Constructor
            _rendererId = GL.GenBuffer();
            _capacity = vertexbuffer_count * sizeof(float);
            _size = 0;

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, _capacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            UnBind();
        }

        public void AppendVertexBuffer(float[] vertexbuffer_data)
        {
            if (vertexbuffer_data == null || vertexbuffer_data.Length == 0)
                return;

            // Add to local buffer
            _localBuffer.AddRange(vertexbuffer_data);

            int vertexbuffer_size = _localBuffer.Count * sizeof(float);

            Bind();

            // Grow buffer if needed
            if (vertexbuffer_size > _capacity)
            {
                // Grow the GPU buffer to accommodate new data
                int newCapacity = Math.Max(_capacity * 2, vertexbuffer_size);

                // Reallocate GPU buffer with new size
                GL.BufferData(BufferTarget.ArrayBuffer, newCapacity, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                _capacity = newCapacity;
            }

            // Upload ALL data to GPU (this is the key - upload everything, not just new data)
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexbuffer_size, _localBuffer.ToArray());
            _size = _localBuffer.Count;

            UnBind();
        }


        public void updateVertexBuffer(float[] vertexbuffer_data)
        {
            if (vertexbuffer_data == null || vertexbuffer_data.Length == 0)
                return;

            int vertexbuffer_size = vertexbuffer_data.Length * sizeof(float);

            // Replace entire local buffer
            _localBuffer.Clear();
            _localBuffer.AddRange(vertexbuffer_data);

            // Important!! Call only in Dynamic Buffer case
            // Update the vertex data
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._rendererId);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexbuffer_size, vertexbuffer_data);

        }


        public void ClearVertexBuffer()
        {
            _localBuffer.Clear();
            _size = 0;

            //// Optional: Clear GPU memory (not strictly necessary since we'll overwrite)
            //// But if you want to be thorough:
            //Bind();
            //byte[] zeros = new byte[_capacity];
            //GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, zeros.Length, zeros);
            //UnBind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, _rendererId);
        public void UnBind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteBuffer(_rendererId);
                _disposed = true;
            }
        }

    }


    public class VertexArrayR1 : IDisposable
    {
        private int _m_renderer_id;
        private bool _disposed = false;

        public VertexArrayR1()
        {
            // Main Constructor: generates a unique vertex array object ID.
            this._m_renderer_id = GL.GenVertexArray();

        }

        public void Add_vertexBuffer(VertexBufferR1 vb, VertexBufferLayout layout)
        {
            // Add and Bind a vertex buffer to an appropriate layout
            // Vertex Buffer layout  contains the information about co-ordinates, normals, color etc

            // Bind the vertex array object.
            Bind();

            // Bind the vertex buffer object.
            vb.Bind();

            // Set up the layout here
            IReadOnlyList<VertexBufferElement> elements = layout.GetElements;
            int offset = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                VertexBufferElement element = elements[i];

                // Enable the vertex attribute array for the specified buffer index.
                GL.EnableVertexAttribArray(i);

                // Set up the vertex attribute pointer for the specified buffer index.
                // This specifies how to interpret the vertex data in the vertex buffer object.
                GL.VertexAttribPointer(i, element.count, element.type, element.normalized, layout.GetStride, offset);


                // Offset is the previous layout count (most likely the stride will remain the same)
                offset = offset + (element.count * VertexBufferElement.GetSizeOfType(element.type));
            }

            // Unbind the vertex buffer object.
            vb.UnBind();

            // Unbind the vertex array object.
            UnBind();

        }

        public void Bind()
        {
            // Binds the vertex array object for use with subsequent OpenGL calls.
            GL.BindVertexArray(this._m_renderer_id);

        }

        public void UnBind()
        {
            // Unbinds the currently bound vertex array object.
            GL.BindVertexArray(0);

        }


        public void Dispose()
        {
            if (!_disposed)
            {
                // Delete this buffer (acts like a  destructor)
                GL.DeleteVertexArray(this._m_renderer_id);
                _disposed = true;
            }
        }


    }




    public class IndexBufferR1 : IDisposable
    {

        private int _rendererId;
        private int _capacity;  // Capacity in indices
        private int _size;     // Current size of indices in bytes
        private bool _disposed;
        private int _bufferCount;
        private List<int> _localBuffer = new List<int>();  // Local copy of all index data

        public int Size => _size;
        public int Capacity => _capacity;

        public int BufferCount => _bufferCount;


        public IndexBufferR1(int indexbuffer_count = 10)
        {
            _rendererId = GL.GenBuffer();
            _capacity = indexbuffer_count * sizeof(uint);
            _size = 0;
            _bufferCount = 0;

            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, _capacity,
                         IntPtr.Zero, BufferUsageHint.DynamicDraw);
            UnBind();
        }


        public void AppendIndexBuffer(int[] indexbuffer_indices)
        {
            int indexbuffer_size = indexbuffer_indices.Length * sizeof(uint);

            if (_size + indexbuffer_size > _capacity)
            {
                Grow(Math.Max(_capacity * 2, _size + indexbuffer_size));
            }

            Bind();


            // Append at current size position
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)_size,
                            indexbuffer_size, indexbuffer_indices);

            _bufferCount += indexbuffer_indices.Length;
            _size += indexbuffer_size;
            UnBind();
        }


        public void UpdateIndexBuffer(int[] indexbuffer_data)
        {
            int indexbuffer_size = indexbuffer_data.Length * sizeof(uint);

            // Important!! Call only in Dynamic Buffer case
            // Update the index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._rendererId);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, indexbuffer_size, indexbuffer_data);

        }



        private void Grow(int newCapacity)
        {
            int newBufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, newBufferId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, newCapacity,
                         IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Copy existing data
            GL.BindBuffer(BufferTarget.CopyReadBuffer, _rendererId);
            GL.BindBuffer(BufferTarget.CopyWriteBuffer, newBufferId);
            GL.CopyBufferSubData(BufferTarget.CopyReadBuffer, BufferTarget.CopyWriteBuffer,
                                IntPtr.Zero, IntPtr.Zero, _size);

            GL.DeleteBuffer(_rendererId);
            _rendererId = newBufferId;
            _capacity = newCapacity;
        }

        public void ClearIndexBuffer()
        {
            _size = 0;
            _bufferCount = 0;
            Bind();
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero,
                            _capacity, IntPtr.Zero);
            UnBind();
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rendererId);
        public void UnBind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        public void Dispose()
        {
            if (!_disposed)
            {
                GL.DeleteBuffer(_rendererId);
                _disposed = true;
            }
        }
    }


}

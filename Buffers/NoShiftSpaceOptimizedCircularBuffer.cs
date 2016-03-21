using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buffers
{
    public class NoShiftSpaceOptimizedCircularBuffer<T> : SpaceOptimizedCircularBuffer<T> where T : struct
    {
        public NoShiftSpaceOptimizedCircularBuffer(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
            : base(blockSize, maximumBufferSize, invalidValuePlaceHolder)
        {
        }
        
        /// <summary>
        /// Add block when no empty block is available.
        /// </summary>
        protected override void AddBlock()
        {
            if ((m_dataSize == 0) && (m_bufferSize == 0))
            {
                m_blockListReadHead = m_blockListWriteHead = m_blockList.AddFirst(CreateBlock());
                m_bufferSize += m_blockSize;
            }
            else if ((m_blockWriteHead == m_blockSize - 1) && (m_bufferSize != m_maximumBufferSize))
            {
                var nextHead = m_blockListWriteHead.Next ?? m_blockList.First;
                if (m_blockListReadHead == nextHead)
                {
                    m_blockList.AddAfter(m_blockListWriteHead, CreateBlock());
                    m_bufferSize += m_blockSize;
                }
            }
        }

        /// <summary>
        /// Remove as soon as an empty block is available.
        /// </summary>
        protected override void RemoveBlock()
        {
            if (m_dataSize == 0)
            {
                m_blockList.RemoveFirst();
                m_bufferSize -= m_blockSize;
            }
            else if (m_blockReadHead == 0)
            {
                var prevHead = m_blockListReadHead.Previous ?? m_blockList.Last;
                if (m_blockListWriteHead != prevHead)
                {
                    m_blockList.Remove(prevHead);
                    m_bufferSize -= m_blockSize;
                }
            }
        }

        protected override void CheckParameters(int blockSize, int maximumBufferSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentException("Block size cannot be zero or less.", nameof(blockSize));
            }
            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentException("Maximum buffer size cannot be less than block size.", nameof(maximumBufferSize));
            }
        }
    }
}

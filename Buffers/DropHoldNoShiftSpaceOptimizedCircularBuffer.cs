using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buffers
{
    public class DropHoldNoShiftSpaceOptimizedCircularBuffer<T> : NoShiftSpaceOptimizedCircularBuffer<T> where T : struct
    {
        int m_requiredBlocks;
        int m_holdValue;
        int m_holdCounter;
        int m_holdCount;

        public DropHoldNoShiftSpaceOptimizedCircularBuffer(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
            : base(blockSize, maximumBufferSize, invalidValuePlaceHolder)
        {
            m_requiredBlocks = 0;
            m_holdValue = 0;
            m_holdCounter = 0;
            m_holdCount = m_blockSize * 128;
        }

        public override bool Write(T data)
        {
            bool status = base.Write(data);
            CalculateRequiredBlocks();
            return status;
        }

        public override bool Read(ref T data)
        {
            bool status = base.Read(ref data);
            CalculateRequiredBlocks();
            return status;
        }

        protected override void AddBlock()
        {
            base.AddBlock();
        }

        protected override void RemoveBlock()
        {
            
        }

        private void CalculateRequiredBlocks()
        {
            m_requiredBlocks = (int)Math.Ceiling((double)m_dataSize / (double)m_blockSize);
            m_requiredBlocks++;

            if (m_requiredBlocks < m_holdValue)
            {
                if (m_holdCounter < m_holdCount)
                {
                    m_holdCounter++;
                }
                else
                {
                    m_holdCounter = 0;
                    m_holdValue = m_requiredBlocks;
                    RemoveBlocks();
                }
            }
            else
            {
                m_holdCounter = 0;
                m_holdValue = m_requiredBlocks;
            }
        }

        private void RemoveBlocks()
        {
            while (m_bufferSize - m_dataSize >= m_blockSize * 2)
            {
                base.RemoveBlock();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buffers
{
    public class SpaceOptimizedCircularBuffer<T> where T : struct
    {
        protected int m_blockSize;                              // Size of a block.
        protected int m_maximumBufferSize;                      // Maximum of size of buffer. (No of blocks) x (block size).
        protected int m_bufferSize;                             // Size of buffer at present. (No of blocks) x (block size).
        protected int m_dataSize;                               // Size of the data in the buffer.
        protected LinkedList<T[]> m_blockList;                  // List of blocks.
        protected LinkedListNode<T[]> m_blockListReadHead;      // Read-head position in the block-list.
        protected int m_blockReadHead;                          // Read-head position in the block. [To be read]
        protected LinkedListNode<T[]> m_blockListWriteHead;     // Write-head position in the block-list.
        protected int m_blockWriteHead;                         // Write-head position in the block. [Written]
        protected T m_invalidValuePlaceholder;                  // Value for no-data.


        public SpaceOptimizedCircularBuffer(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
        {
            CheckParameters(blockSize, maximumBufferSize);


            m_blockSize = blockSize;
            m_maximumBufferSize = (((maximumBufferSize - 1) / blockSize) + 1) * blockSize;
            m_bufferSize = 0;
            m_dataSize = 0;
            m_blockList = new LinkedList<T[]>();
            m_blockListReadHead = null;
            m_blockReadHead = 0;
            m_blockListWriteHead = null;
            m_blockWriteHead = blockSize - 1;
            m_invalidValuePlaceholder = invalidValuePlaceHolder;
        }

        public virtual Boolean Write(T data)
        {
            if (m_dataSize == m_maximumBufferSize)
            {
                return false;
            }
            else
            {
                // Add block if required.
                AddBlock();

                // Move ahead
                MoveHead(ref m_blockWriteHead, ref m_blockListWriteHead);

                // Write data
                m_blockListWriteHead.Value[m_blockWriteHead] = data;
                m_dataSize++;


                return true;
            }
        }

        public virtual Boolean Read(ref T data)
        {
            if (m_dataSize == 0)
            {
                return false;
            }
            else
            {
                // Read data
                data = m_blockListReadHead.Value[m_blockReadHead];
                m_blockListReadHead.Value[m_blockReadHead] = m_invalidValuePlaceholder;
                m_dataSize--;

                // Move ahead
                MoveHead(ref m_blockReadHead, ref m_blockListReadHead);

                // Remove block if required
                RemoveBlock();

                return true;
            }
        }

        protected virtual void CheckParameters(int blockSize, int maximumBufferSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentException("Block size cannot be zero or less.", nameof(blockSize));
            }
            if (maximumBufferSize <= 0)
            {
                throw new ArgumentException("Maximum buffer size cannot be zero or less.", nameof(maximumBufferSize));
            }
        }

        protected virtual void AddBlock()
        {
            if (m_dataSize == m_bufferSize) // buffer is full
            {
                // Add block
                LinkedListNode<T[]> addedBlock;
                T[] newBlock = CreateBlock();

                if (m_blockList.Count == 0)
                {
                    addedBlock = m_blockList.AddFirst(newBlock);
                    m_blockListWriteHead = addedBlock;
                    m_blockListReadHead = addedBlock;
                }
                else if (m_blockWriteHead < m_blockSize - 1)
                {
                    addedBlock = m_blockList.AddAfter(m_blockListWriteHead, newBlock);
                    MoveBlockListHead(ref m_blockListReadHead);

                    // Shift data
                    for (int i = m_blockWriteHead + 1; i < m_blockSize; i++)
                    {
                        addedBlock.Value[i] = m_blockListWriteHead.Value[i];
                        m_blockListWriteHead.Value[i] = m_invalidValuePlaceholder;
                    }
                }
                else
                {
                    m_blockList.AddAfter(m_blockListWriteHead, newBlock);
                }

                m_bufferSize += m_blockSize;
            }
        }

        protected virtual void RemoveBlock()
        {
            if (m_bufferSize - m_dataSize == m_blockSize) // buffer is empty of (blocksize) length.
            {
                LinkedListNode<T[]> removedBlock;

                if (m_dataSize == 0) // buffer is empty
                {
                    removedBlock = m_blockListReadHead;
                    m_blockList.Remove(removedBlock);
                    m_blockListReadHead = null;
                    m_blockListWriteHead = null;
                }
                else if (m_blockReadHead > 0)
                {
                    removedBlock = m_blockListReadHead.Previous ?? m_blockList.Last;
                    MoveBlockListHead(ref m_blockListWriteHead);
                    m_blockList.Remove(removedBlock);

                    // Shift data
                    for (int i = 0; i < m_blockReadHead; i++)
                    {
                        m_blockListReadHead.Value[i] = removedBlock.Value[i];
                    }
                }
                else
                {
                    m_blockList.Remove(m_blockListReadHead.Previous ?? m_blockList.Last);
                }

                m_bufferSize -= m_blockSize;
            }
        }

        protected void MoveHead(ref int blockHead, ref LinkedListNode<T[]> blockListHead)
        {
            if (blockHead == m_blockSize - 1)
            {
                blockHead = 0;
                MoveBlockListHead(ref blockListHead);
            }
            else
            {
                blockHead++;
            }
        }

        protected void MoveBlockListHead(ref LinkedListNode<T[]> blockListHead)
        {
            if (blockListHead.Next == null)
            {
                blockListHead = m_blockList.First;
            }
            else
            {
                blockListHead = blockListHead.Next;
            }
        }

        protected T[] CreateBlock()
        {
            return Enumerable.Repeat<T>(m_invalidValuePlaceholder, m_blockSize).ToArray();
        }

        public int DataSize
        {
            get { return m_dataSize; }
        }

        public virtual int BufferSize
        {
            get { return m_bufferSize; }
        }

        public int BlockCount
        {
            get { return m_blockList.Count; }
        }

    }
}

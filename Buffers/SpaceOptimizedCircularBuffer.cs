using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buffers
{
    /// <summary>
    /// Circular buffer whose size adjust to the requirement.
    /// However, there is an upper limit to which the size can increase.
    /// </summary>
    /// <typeparam name="T">Any value type can be stored in the buffer.</typeparam>
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

        /// <summary>
        /// Constructor to initialize the buffer.
        /// </summary>
        /// <param name="blockSize">Size of a block.</param>
        /// <param name="maximumBufferSize">
        /// Maximum size of the buffer is always integral multiple of the blockSize.
        /// The integral multiple is a minimum value required to accommodate maximumBufferSize.
        /// </param>
        /// <param name="invalidValuePlaceHolder">
        /// This value in the buffer represents non-written data unit.
        /// Useful for debugging.
        /// </param>
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

        /// <summary>
        /// Writes a data unit to the buffer.
        /// </summary>
        /// <param name="data">Data unit to be written.</param>
        /// <returns>
        /// Returns true only if data is successfully written.
        /// Returns false if buffer has reached maximum size and there's no empty unit.
        /// </returns>
        public virtual bool Write(T data)
        {
            // Data cannot be written if buffer has reached maximum size and there's no empty unit.
            if (m_dataSize == m_maximumBufferSize) // Buffer is full and reached its limit.
            {
                return false;
            }
            else // Buffer size is not at max.
            {
                /* WriteHead is always at the previously written data.
                 * Hence, head is moved forward first to write data.
                 * 
                 * A new block is added if buffer is full.
                 * This behavior can be overridden by derived classes.
                 */

                // Add a block if required.
                AddBlock();

                // Move write head forward.
                MoveHead(ref m_blockWriteHead, ref m_blockListWriteHead);

                // Write data at the write head.
                m_blockListWriteHead.Value[m_blockWriteHead] = data;

                // Increase the data size.
                m_dataSize++;


                return true;
            }
        }

        /// <summary>
        /// Reads a data unit from the buffer.
        /// </summary>
        /// <param name="data">Read data unit.</param>
        /// <returns>
        /// Returns true only if data is read successfully.
        /// Returns false if buffer is empty.
        /// </returns>
        public virtual bool Read(ref T data)
        {
            // Data cannot be read if buffer is empty.
            if (m_dataSize == 0) // Buffer is empty.
            {
                return false;
            }
            else // Buffer is not empty.
            {
                /* Read head is always at where the next data unit would be read from.
                 * Hence, data is first written then the head is moved.
                 * 
                 * A block is removed as soon as buffer has empty units equal to block size.
                 * This behavior can be overridden by the derived classes.
                 */

                // Read data at the read head.
                data = m_blockListReadHead.Value[m_blockReadHead];
                m_blockListReadHead.Value[m_blockReadHead] = m_invalidValuePlaceholder;

                // Decrease the data size.
                m_dataSize--;

                // Move read head forward.
                MoveHead(ref m_blockReadHead, ref m_blockListReadHead);

                // Remove block if required.
                RemoveBlock();

                return true;
            }
        }

        /// <summary>
        /// Checks if a block should be added. Adds if needed.
        /// </summary>
        protected virtual void AddBlock()
        {
            // Add a block only if buffer is full.
            if (m_dataSize == m_bufferSize)
            {
                // Add block
                LinkedListNode<T[]> addedBlock;
                T[] newBlock = CreateBlock();

                
                if (m_blockList.Count == 0) // Empty buffer / No blocks
                {
                    addedBlock = m_blockList.AddFirst(newBlock);
                    m_blockListWriteHead = addedBlock;
                    m_blockListReadHead = addedBlock;
                }
                else if (m_blockWriteHead < m_blockSize - 1) // Last written unit is not at the end of the block
                {
                    /*  v^
                     * DDDDD DDDDD
                     *  v      ^
                     * DDOOO OODDD DDDDD
                     */

                    // A new block is added after the current read-block.
                    // The data unit present after the last written unit are shifted to newly added block.
                    addedBlock = m_blockList.AddAfter(m_blockListWriteHead, newBlock);

                    // Since, write head was not at the end of the block and buffer is full,
                    // the read head has to be in the same block.
                    // Now that a new block is added and data is shifted,
                    // the read head should also be shifted.
                    MoveBlockListHead(ref m_blockListReadHead);

                    // Shift data
                    for (int i = m_blockWriteHead + 1; i < m_blockSize; i++)
                    {
                        addedBlock.Value[i] = m_blockListWriteHead.Value[i];
                        m_blockListWriteHead.Value[i] = m_invalidValuePlaceholder;
                    }
                }
                else // Last written unit is at the end of the block.
                {
                    /*     v ^
                     * DDDDD DDDDD
                     *     v       ^
                     * DDDDD OOOOO DDDDD
                     */

                    m_blockList.AddAfter(m_blockListWriteHead, newBlock);
                }

                // Increase the buffer size by block size.
                m_bufferSize += m_blockSize;
            }
        }

        /// <summary>
        /// Check's if a block should be removed. Removes if needed.
        /// </summary>
        protected virtual void RemoveBlock()
        {
            if (m_bufferSize - m_dataSize == m_blockSize) // buffer is empty of (block size) length.
            {
                LinkedListNode<T[]> removedBlock;

                if (m_dataSize == 0) // buffer is empty
                {
                    removedBlock = m_blockListReadHead;
                    m_blockList.Remove(removedBlock);
                    m_blockListReadHead = null;
                    m_blockListWriteHead = null;
                }
                else if (m_blockReadHead > 0) // Portion of block has data
                {
                    /*    v      ^
                     * DDDDO OOOOD DDDDD
                     *          v^
                     *       DDDDD DDDDD
                     */

                    // Previous block of the read-block is removed
                    // and the data in that block is shifted to the read-block
                    removedBlock = m_blockListReadHead.Previous ?? m_blockList.Last;
                    
                    // In this case, the previous block has to be the current write-block.
                    // Write head is advanced as this block will be removed.
                    MoveBlockListHead(ref m_blockListWriteHead);

                    // Remove the block
                    m_blockList.Remove(removedBlock);

                    // Shift data
                    for (int i = 0; i < m_blockReadHead; i++)
                    {
                        m_blockListReadHead.Value[i] = removedBlock.Value[i];
                    }
                }
                else // Complete block is empty
                {
                    /*     v       ^
                     * DDDDD OOOOO DDDDD
                     *     v ^
                     * DDDDD DDDDD
                     */

                    m_blockList.Remove(m_blockListReadHead.Previous ?? m_blockList.Last);
                }

                // Decrease the buffer size by block size.
                m_bufferSize -= m_blockSize;
            }
        }

        /// <summary>
        /// Moves the head forward.
        /// </summary>
        /// <param name="blockHead">Reference to block head.</param>
        /// <param name="blockListHead">Reference to block-list head.</param>
        protected void MoveHead(ref int blockHead, ref LinkedListNode<T[]> blockListHead)
        {
            if (blockHead == m_blockSize - 1) // Block head is at the end of the block.
            {
                // If block head is at the end of the block,
                // move to next block.

                blockHead = 0;
                MoveBlockListHead(ref blockListHead);
            }
            else // Block head is not at the end of the block.
            {
                // If block head is not at the end of the block,
                // simply move the block head forward.
                blockHead++;
            }
        }

        /// <summary>
        /// Moves the block-list head forward.
        /// </summary>
        /// <param name="blockListHead">Reference to block-list head.</param>
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

        /// <summary>
        /// Creates an empty block.
        /// The data units are initialized with invalid-value-placeholder.
        /// </summary>
        /// <returns>An empty block.</returns>
        protected T[] CreateBlock()
        {
            return Enumerable.Repeat<T>(m_invalidValuePlaceholder, m_blockSize).ToArray();
        }

        /// <summary>
        /// Returns the data size.
        /// </summary>
        public int DataSize
        {
            get { return m_dataSize; }
        }

        /// <summary>
        /// Returns the buffer size.
        /// </summary>
        public virtual int BufferSize
        {
            get { return m_bufferSize; }
        }

        /// <summary>
        /// Returns the block-count.
        /// </summary>
        public int BlockCount
        {
            get { return m_blockList.Count; }
        }

    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buffers;

namespace Buffers.Tests
{
    [TestClass]
    public class SpaceOptimizedCircularBufferTests
    {
        virtual protected SpaceOptimizedCircularBuffer<T>
        InitBuffer<T>(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
            where T : struct
        {
            return new SpaceOptimizedCircularBuffer<T>(blockSize, maximumBufferSize, invalidValuePlaceHolder);
        }

        [TestMethod]
        public void TestCtor()
        {
            try
            {
                var buffer = InitBuffer<int>(0, 1, -1);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("blockSize", ex.ParamName);
            }

            try
            {
                var buffer = InitBuffer<int>(1, 0, -1);
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("maximumBufferSize", ex.ParamName);
            }
        }

        [TestMethod]
        public void TestSimpleReadWrite()
        {
            var buffer = InitBuffer<int>(2, 2, -1);
            int temp = 0;
            bool result = false;

            result = buffer.Read(ref temp);
            Assert.AreEqual(false, result);

            result = buffer.Write(1);
            Assert.AreEqual(true, result);

            result = buffer.Write(2);
            Assert.AreEqual(true, result);

            result = buffer.Write(3);
            Assert.AreEqual(false, result);

            temp = 0;

            result = buffer.Read(ref temp);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1, temp);

            result = buffer.Read(ref temp);
            Assert.AreEqual(true, result);
            Assert.AreEqual(2, temp);

            result = buffer.Read(ref temp);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestResize()
        {
            var buffer = InitBuffer<int>(2, 3, -1);
            int temp = 0;
            bool result = true;

            buffer.Write(1);
            buffer.Write(2);
            buffer.Read(ref temp);
            buffer.Read(ref temp);

            buffer.Write(1);
            buffer.Write(2);
            buffer.Write(3);
            buffer.Write(4);
            result = buffer.Write(5);
            Assert.AreEqual(false, result);


            buffer.Read(ref temp);
            Assert.AreEqual(1, temp);

            buffer.Read(ref temp);
            Assert.AreEqual(2, temp);

            buffer.Read(ref temp);
            Assert.AreEqual(3, temp);

            buffer.Read(ref temp);
            Assert.AreEqual(4, temp);

            result = true;
            result = buffer.Read(ref temp);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestingMethod_Write_BufferFull()
        {
            int blockSize = 2;
            int maxBufferSize = blockSize * 2;

            var buffer = InitBuffer<int>(blockSize, maxBufferSize, -1); // block-size, max-buffer-size, invalid value placeholder
            
            bool result = true;

            for(int i=1; i<=maxBufferSize; i++)
            {
                result = buffer.Write(i);
                Assert.AreEqual(true, result);
            }

            result = buffer.Write(maxBufferSize + 1);
            Assert.AreEqual(false, result);

            int temp = 0;
            for (int i = 1; i <= maxBufferSize; i++)
            {
                result = buffer.Read(ref temp);
                Assert.AreEqual(true, result);
                Assert.AreEqual(i, temp);
            }
        }

        [TestMethod]
        public void TestingMethod_Read_BufferEmpty()
        {
            int blockSize = 2;
            int maxBufferSize = blockSize * 4; //8
            int writeReadSize = 8;

            var buffer = InitBuffer<int>(blockSize, maxBufferSize, -1); // block-size, max-buffer-size, invalid value placeholder

            bool result = true;
            
            for (int i = 1; i <= writeReadSize; i++)
            {
                result = buffer.Write(i);
                Assert.AreEqual(true, result);
            }

            int temp = 0;
            for (int i = 1; i <= writeReadSize; i++)
            {
                result = buffer.Read(ref temp);
                Assert.AreEqual(true, result);
                Assert.AreEqual(i, temp);
            }

            result = buffer.Read(ref temp);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void TestingMethod_Write_Read_BufferNotFull()
        {
            int blockSize = 2;
            int maxBufferSize = blockSize * 4; //8
            int writeReadSize = 7;
            int writeReadCycles = 3;

            var buffer = InitBuffer<int>(blockSize, maxBufferSize, -1); // block-size, max-buffer-size, invalid value placeholder

            bool result = true;

            for (int c = 0; c < writeReadCycles; c++)
            {
                for (int i = 1; i <= writeReadSize; i++)
                {
                    result = buffer.Write(i);
                    Assert.AreEqual(true, result);
                }

                int temp = 0;
                for (int i = 1; i <= writeReadSize; i++)
                {
                    result = buffer.Read(ref temp);
                    Assert.AreEqual(true, result);
                    Assert.AreEqual(i, temp);
                }
            }
        }
        
        [TestMethod]
        public void TestShift()
        {
            int blockSize = 3;
            int maxBufferSize = 6;
            var buffer = InitBuffer<int>(blockSize, maxBufferSize, -1);
            int temp = -1;
            bool result = true;

            // move 1 unit
            buffer.Write(1);
            buffer.Read(ref temp);

            // fill the buffer
            for (int i = 0; i < maxBufferSize; i++)
            {
                buffer.Write(i);
            }

            result = buffer.Write(6);
            Assert.AreEqual(false, result);

            // read the buffer
            for (int i = 0; i < maxBufferSize; i++)
            {
                result = false;
                result = buffer.Read(ref temp);
                Assert.AreEqual(true, result);
                Assert.AreEqual(i, temp);
            }

            result = true;
            result = buffer.Read(ref temp);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RandomReadWrite()
        {
            int blockSize = 7;
            int maxBufferSize = 70;
            var buffer = InitBuffer<int>(blockSize, maxBufferSize, -1);
            int temp = -1;
            bool result = true;

            int writtenCount = 0;
            int readCount = 0;
            int localWriteCount = 0;
            int localReadCount = 0;
            Random randomizer = new Random();
            for (int rCount = 0; rCount < 10000; rCount++)
            {
                localWriteCount = randomizer.Next(1, 75);
                localReadCount = randomizer.Next(1, 70);

                for (int write = 0; write < localWriteCount; write++)
                {
                    if (buffer.Write(writtenCount))
                    {
                        writtenCount++;
                    }
                    else
                    {
                        Assert.AreEqual(maxBufferSize, writtenCount - readCount);
                    }
                }

                for (int read = 0; read < localWriteCount; read++)
                {
                    result = buffer.Read(ref temp);
                    if (result)
                    {
                        Assert.AreEqual(readCount, temp);
                        readCount++;
                    }
                    else
                    {
                        Assert.AreEqual(true, readCount == writtenCount);
                    }
                }
            }
        }

        [TestMethod]
        public void TestBufferSize()
        {
            var buffer = InitBuffer<int>(2, 2, -1);

            int size = buffer.BufferSize;
            Assert.AreEqual(0, size);
        }

        [TestMethod]
        public void TestGetDataSize()
        {
            var buffer = InitBuffer<int>(2, 2, -1);

            int size = buffer.DataSize;
            Assert.AreEqual(0, size);
        }

        [TestMethod]
        public void TestBlockCount()
        {
            var buffer = InitBuffer<int>(2, 2, -1);

            int size = buffer.BlockCount;
            Assert.AreEqual(0, size);
        }
    }
}

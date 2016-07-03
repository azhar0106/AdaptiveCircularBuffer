using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buffers;

namespace Buffers.Tests
{
    [TestClass]
    public class NoShiftSpaceOptimizedCircularBufferTests : SpaceOptimizedCircularBufferTests
    {
        protected override SpaceOptimizedCircularBuffer<T> InitBuffer<T>(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
        {
            return new NoShiftSpaceOptimizedCircularBuffer<T>(blockSize, maximumBufferSize, invalidValuePlaceHolder);
        }

        protected override void CheckBlockSize(int bufferUnits, int blockSize, int actualBlockCount)
        {
            
        }


        [TestMethod]
        public override void TestBufferSize()
        {
            var buffer = InitBuffer<int>(2, 2, -1);

            int size = buffer.BufferSize;
            Assert.AreEqual(2, size);
        }

        [TestMethod]
        public override void TestBlockCount()
        {
            var buffer = InitBuffer<int>(2, 2, -1);

            int size = buffer.BlockCount;
            Assert.AreEqual(1, size);
        }
    }
}

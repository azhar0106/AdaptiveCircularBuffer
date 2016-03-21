using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buffers;

namespace Buffers.Tests
{
    [TestClass]
    public class NoShiftSpaceOptimizedCircularBuffer : SpaceOptimizedCircularBufferTests
    {
        protected override SpaceOptimizedCircularBuffer<T> InitBuffer<T>(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
        {
            return new NoShiftSpaceOptimizedCircularBuffer<T>(blockSize, maximumBufferSize, invalidValuePlaceHolder);
        }
    }
}

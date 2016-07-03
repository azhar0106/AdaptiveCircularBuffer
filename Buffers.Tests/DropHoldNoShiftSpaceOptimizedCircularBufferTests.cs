using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Buffers;

namespace Buffers.Tests
{
    [TestClass]
    public class DropHoldNoShiftSpaceOptimizedCircularBufferTests : NoShiftSpaceOptimizedCircularBufferTests
    {
        protected override SpaceOptimizedCircularBuffer<T> InitBuffer<T>(int blockSize, int maximumBufferSize, T invalidValuePlaceHolder)
        {
            return new DropHoldNoShiftSpaceOptimizedCircularBuffer<T>(blockSize, maximumBufferSize, invalidValuePlaceHolder);
        }
    }
}

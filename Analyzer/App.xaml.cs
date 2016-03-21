using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Buffers;

namespace Analyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const int MAX_BUFFER_SIZE = 1024;
        public static Controller Controller1 { get; set; }
        public static Controller Controller2 { get; set; }

        static App()
        {
            Controller1 = new Controller(.01, MAX_BUFFER_SIZE, new SpaceOptimizedCircularBuffer<sbyte>
                (32, MAX_BUFFER_SIZE, -1));
            Controller2 = new Controller(.01, MAX_BUFFER_SIZE, new DropHoldNoShiftSpaceOptimizedCircularBuffer<sbyte>
                (32, MAX_BUFFER_SIZE, -1));
        }
    }
}

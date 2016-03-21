using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffers;
using System.ComponentModel;
using System.Windows.Threading;

namespace Analyzer
{
    public class Controller : INotifyPropertyChanged
    {
        SpaceOptimizedCircularBuffer<sbyte> m_buffer;
        DispatcherTimer m_timer;
        Double m_refreshInterval;               // Refresh rate in seconds. Should not be more than 1.
        Double m_writeRate;                     // Write rate in bps
        Double m_readRate;                      // Read rate in bps
        int m_bufferSize;                       // Current buffer size
        int m_maxBufferSize;                    // Maximum buffer size
        int[] m_bufferDiffs;                    // Buffer sizes
        int m_bufferDiffsCurrent;               // Index of current buffer size
        Double m_allocationRate;                // Allocation/Deallocation in bytes per sec.
        String m_state;                         // Started, Paused, Stopped
        const String STATE_STARTED = "Started";
        const String STATE_PAUSED = "Paused";
        const String STATE_STOPPED = "Stopped";

        public event PropertyChangedEventHandler PropertyChanged;

        public String State
        {
            get { return m_state; }
        }

        public int MaxBufferSize
        {
            get { return m_maxBufferSize; }
        }

        public int BufferSize
        {
            get { return m_bufferSize; }
        }

        public Double WriteRate
        {
            get { return m_writeRate; }
            set
            {
                if (value == m_writeRate)
                    return;

                m_writeRate = Math.Max(0, value);
                NotifyPropertyChanged(nameof(WriteData));
            }
        }

        public Double ReadRate
        {
            get { return m_readRate; }
            set
            {
                if (value == m_readRate)
                    return;

                m_readRate = Math.Max(0, value);
                NotifyPropertyChanged(nameof(ReadRate));
            }
        }

        public Double AllocationRate
        {
            get { return m_allocationRate; }
        }


        public Controller(Double refreshInterval, int maxBufferSize, SpaceOptimizedCircularBuffer<sbyte> buffer)
        {
            if (refreshInterval > 1 || refreshInterval <= 0)
            {
                throw new ArgumentException("Refresh interval should be greater than 0 but not greater than 1.", nameof(refreshInterval));
            }

            m_state = STATE_STOPPED;
            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromSeconds(m_refreshInterval);
            m_timer.Tick += M_timer_Tick;
            m_refreshInterval = refreshInterval;
            m_writeRate = 0.0;
            m_readRate = 0.0;

            m_maxBufferSize = maxBufferSize;
            m_buffer = buffer;
            m_bufferDiffs = new int[(int)(1.0 / refreshInterval)];
        }

        public void Start()
        {
            m_timer.Start();
            m_state = STATE_STARTED;
        }

        public void Stop()
        {
            m_timer.Stop();
            m_state = STATE_STOPPED;
        }

        public void Pause()
        {
            m_timer.Stop();
            m_state = STATE_PAUSED;
        }


        private void M_timer_Tick(object sender, EventArgs e)
        {
            WriteData();
            CalculateAllocationRate();

            ReadData();
            CalculateAllocationRate();
        }

        private void WriteData()
        {
            int bytesToWrite = Convert.ToInt32(m_writeRate * m_refreshInterval);
            for (int i = 0; i < bytesToWrite; i++)
            {
                m_buffer.Write(Convert.ToSByte(i % 128));
            }

            NotifyPropertyChanged(nameof(BufferSize));
        }

        private void ReadData()
        {
            int bytesToRead = Convert.ToInt32(m_readRate * m_refreshInterval);
            sbyte data = -1;
            for (int i = 0; i < bytesToRead; i++)
            {
                m_buffer.Read(ref data);
            }

            NotifyPropertyChanged(nameof(BufferSize));
        }

        private void CalculateAllocationRate()
        {
            int diff = m_buffer.BufferSize - m_bufferSize;
            m_bufferSize = m_buffer.BufferSize;

            //m_bufferDiffsCurrent++;
            //m_bufferDiffsCurrent %= m_bufferDiffs.Length;
            //m_bufferDiffs[m_bufferDiffsCurrent] = diff;

            //m_allocationRate = m_bufferDiffs.Select(d => Math.Abs(d)).Sum();

            diff = Math.Abs(diff);

            m_allocationRate = (double)diff / m_refreshInterval;
            
            NotifyPropertyChanged(nameof(AllocationRate));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

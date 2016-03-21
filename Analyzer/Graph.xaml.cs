using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Analyzer
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : UserControl
    {
        Controller m_controller;
        public Graph()
        {
            InitializeComponent();
        }

        public void Init(Controller controller, int lineCount, int pointsToDraw)
        {
            m_controller = controller;
            m_controller.PropertyChanged += M_controller_PropertyChanged;
            Plot.AddLines(lineCount, pointsToDraw);
        }

        private void M_controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Double[] values = { -1.0, -1.0, -1.0 };

            values[0] = (double)m_controller.BufferSize / (double)m_controller.MaxBufferSize;
            values[1] = (double)m_controller.AllocationRate / (1024.0 * 16);
            values[2] = (double)m_controller.ReadRate / (1024.0);

            Plot.Update(values);
        }
    }
}

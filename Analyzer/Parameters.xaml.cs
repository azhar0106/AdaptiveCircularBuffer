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
    /// Interaction logic for Parameters.xaml
    /// </summary>
    public partial class Parameters : UserControl
    {
        Controller m_controller;

        public Parameters()
        {
            InitializeComponent();
        }

        public void Init(Controller controller)
        {
            m_controller = controller;
            m_controller.PropertyChanged += M_controller_PropertyChanged;
        }

        private void M_controller_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            WriteRateLabel.Content = m_controller.WriteRate.ToString();
            ReadRateLabel.Content = m_controller.ReadRate.ToString();
            BufferSizeLabel.Content = m_controller.BufferSize.ToString();
            AllocationRateLabel.Content = m_controller.AllocationRate.ToString();
        }
    }
}

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
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : UserControl
    {
        Controller m_controller1;
        Controller m_controller2;

        public ControlPanel()
        {
            InitializeComponent();
            m_controller1 = App.Controller1;
            m_controller2 = App.Controller2;
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            m_controller1.Start();
            m_controller2.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            m_controller1.Pause();
            m_controller2.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            m_controller1.Stop();
            m_controller2.Stop();
        }

        private void WriteRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_controller1.WriteRate = e.NewValue;
            m_controller2.WriteRate = e.NewValue;
        }

        private void ReadRateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_controller1.ReadRate = e.NewValue;
            m_controller2.ReadRate = e.NewValue;
        }
    }
}

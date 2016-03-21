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
    /// Interaction logic for MultiLinePlot.xaml
    /// </summary>
    public partial class MultiLinePlot : UserControl
    {
        bool m_isInit;
        int m_lineCount, m_pointsToPlot;
        Polyline[] m_lines;
        PointCollection[] m_points;
        
        public MultiLinePlot()
        {
            InitializeComponent();
            this.SizeChanged += MultiLinePlot_SizeChanged;
            this.Loaded += MultiLinePlot_Loaded;
        }

        private void MultiLinePlot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MultiLinePlot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!m_isInit && (e.NewSize.Width != 0.0 && e.NewSize.Height != 0.0))
            {
                InitPoints();
                m_isInit = true;
            }
            else
            {
                UpdateXY(e.PreviousSize, e.NewSize);
            }
        }

        public void AddLines(int lineCount, int pointsToPlot)
        {
            m_lineCount = lineCount;
            m_pointsToPlot = pointsToPlot;

            m_lines = new Polyline[m_lineCount];
            m_points = new PointCollection[lineCount];
            for (int i = 0; i < m_lineCount; i++)
            {
                byte r = Convert.ToByte(Math.Floor((Convert.ToDouble((i + 0) % 3)) / 2.0) * 255);
                byte g = Convert.ToByte(Math.Floor((Convert.ToDouble((i + 1) % 3)) / 2.0) * 255);
                byte b = Convert.ToByte(Math.Floor((Convert.ToDouble((i + 2) % 3)) / 2.0) * 255);
                m_lines[i] = new Polyline();
                m_lines[i].Stroke = new SolidColorBrush(Color.FromArgb(127, r, g, b));
                m_lines[i].StrokeThickness = 3.0;
                m_points[i] = new PointCollection();
                m_lines[i].Points = m_points[i];
                Canvas.Children.Add(m_lines[i]);
            }
            m_isInit = false;

        }

        private void InitPoints()
        {
            Double x, y;
            for (int i = 0; i < m_lineCount; i++)
            {
                for (int px = 0; px < m_pointsToPlot; px++)
                {
                    x = Canvas.ActualWidth * ((Double)px / (Double)m_pointsToPlot);
                    y = Canvas.ActualHeight;// * ((Double)px / (Double)m_pointsToPlot);
                    m_points[i].Add(new Point(x, y));
                }
            }
        }

        private void UpdateXY(Size prevSize, Size newSize)
        {
            Double x, y;
            Double xScale = newSize.Width / prevSize.Width;
            Double yScale = newSize.Height / prevSize.Height;

            for (int i = 0; i < m_lineCount; i++)
            {
                for (int px = 0; px < m_pointsToPlot; px++)
                {
                    x = m_points[i][px].X * xScale;
                    y = m_points[i][px].Y * yScale;
                    m_points[i][px] = new Point(x, y);
                }
            }
        }

        public void Update(Double[] y)
        {
            for (int i = 0; i < m_lineCount; i++)
            {
                Double yValue;
                for (int x = 0; x < m_pointsToPlot - 1; x++)
                {
                    m_points[i][x] = new Point(m_points[i][x].X, m_points[i][x + 1].Y);
                }
                yValue = Canvas.ActualHeight - (y[i] * Canvas.ActualHeight);
                m_points[i][m_pointsToPlot - 1] = new Point(Canvas.ActualWidth, yValue);
            }
        }
    }
}

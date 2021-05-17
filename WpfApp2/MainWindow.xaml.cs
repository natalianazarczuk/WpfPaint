using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        Random rnd = new Random();

        List<Shape> shapes = new List<Shape>();
        List<Shape> selected = new List<Shape>();

        bool drawing_rect;
        bool drawing_elips;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int x = rnd.Next(0, 4);

            for (int i = 0; i < x; i++)
            {
                Rectangle rect = new Rectangle();
                rect.Width = rnd.Next(50, 300);
                rect.Height = rnd.Next(50, 300);
                rect.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));
                Canvas.SetLeft(rect, rnd.Next(0, (int)canvas.ActualWidth - (int)rect.Width));
                Canvas.SetTop(rect, rnd.Next(0, (int)canvas.ActualHeight - (int)rect.Height));
                canvas.Children.Add(rect);
                shapes.Add(rect);

                rect.Cursor = Cursors.Hand;
            }

            for (int i = 0; i < (6 - x); i++)
            {
                Ellipse elips = new Ellipse();
                elips.Width = rnd.Next(50, 300);
                elips.Height = rnd.Next(50, 300);
                elips.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));
                Canvas.SetLeft(elips, rnd.Next(0, (int)canvas.ActualWidth - (int)elips.Width));
                Canvas.SetTop(elips, rnd.Next(0, (int)canvas.ActualHeight - (int)elips.Height));
                canvas.Children.Add(elips);
                shapes.Add(elips);

                elips.Cursor = Cursors.Hand;
            }
        }

        private void select(Shape shape)
        {
            DropShadowEffect glow = new DropShadowEffect();
            glow.Color = Colors.White;
            glow.BlurRadius = 50;
            glow.Direction = 270;

            selected.Add(shape);
            Panel.SetZIndex(shape, 1);
            shape.Effect = glow;

            Delete.IsEnabled = true;
            RandomColor.IsEnabled = true;
        }

        private void deselect(Shape shape)
        {
            selected.Remove(shape);
            Panel.SetZIndex(shape, 0);
            shape.Effect = null;
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Shape)
            {
                var shape = (Shape)e.OriginalSource;

                if (selected.Contains(shape)) //if this object is already selected then deselect it
                {
                    deselect(shape);
                    Delete.IsEnabled = false;
                    RandomColor.IsEnabled = false;
                }
                else //if it wasnt selected then select it                
                    select(shape);
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Shape)
            {
                var shape = (Shape)e.OriginalSource;

                if (selected.Contains(shape))
                {
                    //something about moving shapes
                }
                else //if it wasnt selected - deselect all and select only this 
                {
                    foreach (var s in selected)
                        deselect(s);

                    select(shape);
                }
            }
            else //all the selected shapes are deselected when click on canvas 
            {
                foreach (var shape in selected)
                {
                    Panel.SetZIndex(shape, 0);
                    shape.Effect = null;
                }
                selected.Clear();

                Delete.IsEnabled = false;
                RandomColor.IsEnabled = false;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            foreach (Shape shape in selected)
            {
                canvas.Children.Remove(shape);
            }
        }

        private void RandomColor_Click(object sender, RoutedEventArgs e)
        {
            foreach (Shape shape in selected)
            {
                shape.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));
            }
        }

        private Point anchorPoint;
        Rectangle rect;
        Ellipse elips;

        private void DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            drawing_rect = false;
            drawing_elips = true;
            canvas.Cursor = Cursors.Cross;
            elips = new Ellipse();
        }

        private void DrawRectangle_Click(object sender, RoutedEventArgs e)
        {
            drawing_elips = false;
            drawing_rect = true;
            canvas.Cursor = Cursors.Cross;
            rect = new Rectangle();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (drawing_rect)
            {
                canvas.CaptureMouse();
                anchorPoint = e.MouseDevice.GetPosition(canvas);
                rect.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));
                canvas.Children.Add(rect);
                shapes.Add(rect);
            }
            else if (drawing_elips)
            {
                canvas.CaptureMouse();
                anchorPoint = e.MouseDevice.GetPosition(canvas);
                elips.Fill = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 233)));
                canvas.Children.Add(elips);
                shapes.Add(elips);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!canvas.IsMouseCaptured)
                return;

            Point location = e.MouseDevice.GetPosition(canvas);

            double minX = Math.Min(location.X, anchorPoint.X);
            double minY = Math.Min(location.Y, anchorPoint.Y);
            double maxX = Math.Max(location.X, anchorPoint.X);
            double maxY = Math.Max(location.Y, anchorPoint.Y);

            if (drawing_rect)
            {
                Canvas.SetTop(rect, minY);
                Canvas.SetLeft(rect, minX);

                double height = maxY - minY;
                double width = maxX - minX;

                rect.Height = Math.Abs(height);
                rect.Width = Math.Abs(width);
            }
            else if (drawing_elips)
            {
                Canvas.SetTop(elips, minY);
                Canvas.SetLeft(elips, minX);

                double height = maxY - minY;
                double width = maxX - minX;

                elips.Height = Math.Abs(height);
                elips.Width = Math.Abs(width);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            canvas.ReleaseMouseCapture();
            canvas.Cursor = Cursors.Arrow;
            drawing_elips = false;
            drawing_rect = false;            
        }

        public static void SaveCanvasToFile(Window window, Canvas canvas, int dpi, string filename)
        {
            Size size = new Size(window.Width, window.Height);
            canvas.Measure(size);

            var rtb = new RenderTargetBitmap((int)window.Width, (int)window.Height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(canvas);

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));

            using (var stm = System.IO.File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void ExportPNG_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Image";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG File (.png)|*.png";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                SaveCanvasToFile(this, canvas, 96, filename);
            }
        }
    }
}

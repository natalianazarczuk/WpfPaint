using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
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

            for (int i = 0; i < (4 - x); i++)
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

            var props = typeof(Colors).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var colorInfos = props.Select(prop =>
            {
                var color = (Color)prop.GetValue(null, null);
                return new ColorInfo()
                {
                    Name = prop.Name,
                    Rgb = color,
                    RgbInfo = $"R:{color.R}, G:{color.G}, B:{color.B}"
                };
            });

            DataContext = colorInfos;
        }

        ////this for binding maybe
        //private void show_last()
        //{

        //}

        private void select(Shape shape)
        {
            if (!drawing_rect && !drawing_elips)
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

                //when selected binding on the last ;(((
                var last = selected.Last();
                WidthText.Text = $"{last.Width}";
                HeightText.Text = $"{last.Height}";
            }   
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Shape)
            {
                var shape = (Shape)e.OriginalSource;

                if (selected.Contains(shape)) //if this object is already selected then deselect it
                {
                    Panel.SetZIndex(shape, 0);
                    shape.Effect = null;
                    selected.Remove(shape);

                    //properties of the current last one 
                    if (selected.Count != 0)
                    {
                        var last = selected.Last();
                        WidthText.Text = $"{last.Width}";
                        HeightText.Text = $"{last.Height}";
                    }
                    else
                    {
                        Delete.IsEnabled = false;
                        RandomColor.IsEnabled = false;
                    }

                }
                else //if it wasnt selected then select it 
                {
                    select(shape);
                }                
            }
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Shape)
            {
                var shape = (Shape)e.OriginalSource;             
                if (!selected.Contains(shape)) //if it wasnt selected - deselect all and select only this 
                {
                    foreach (var s in selected)
                    {
                        Panel.SetZIndex(s, 0);
                        s.Effect = null;
                    }
                    selected.Clear();

                    select(shape);
                }
            }
            else //all the selected shapes are deselected when click on canvas 
            {
                foreach (var s in selected)
                {
                    Panel.SetZIndex(s, 0);
                    s.Effect = null;
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

        bool captured = false;
        double x_shape, x_canvas, y_shape, y_canvas;
        Shape moving = null;

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
            else if(e.OriginalSource is Shape) //moving all selected shapes
            {
                moving = (Shape)e.OriginalSource;
                moving.Cursor = Cursors.ScrollAll;
                Mouse.Capture(moving);
                captured = true;
                x_shape = Canvas.GetLeft(moving);
                x_canvas = e.GetPosition(canvas).X;
                y_shape = Canvas.GetTop(moving);
                y_canvas = e.GetPosition(canvas).Y;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                moving.Cursor = Cursors.ScrollAll;
                double x = e.GetPosition(canvas).X;
                double y = e.GetPosition(canvas).Y;
                x_shape += x - x_canvas;
                Canvas.SetLeft(moving, x_shape);
                x_canvas = x;
                y_shape += y - y_canvas;
                Canvas.SetTop(moving, y_shape);
                y_canvas = y;
            }

            if (!canvas.IsMouseCaptured)
                return;

            Point location = e.MouseDevice.GetPosition(canvas);

            double minX = Math.Min(location.X, anchorPoint.X);
            double minY = Math.Min(location.Y, anchorPoint.Y);
            double maxX = Math.Max(location.X, anchorPoint.X);
            double maxY = Math.Max(location.Y, anchorPoint.Y);

            double height = maxY - minY;
            double width = maxX - minX;

            if (drawing_rect)
            {
                Canvas.SetTop(rect, minY);
                Canvas.SetLeft(rect, minX);

                rect.Height = Math.Abs(height);
                rect.Width = Math.Abs(width);
            }
            else if (drawing_elips)
            {
                Canvas.SetTop(elips, minY);
                Canvas.SetLeft(elips, minX);

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

            if (captured)
            {
                Mouse.Capture(null);
                captured = false;
                moving.Cursor = Cursors.Hand;
                moving = null;
            }          
        }

        public static void SaveCanvasToFile(Window window, Canvas canvas, int dpi, string filename)
        {
            Size size = new Size(window.Width, window.Height);
            canvas.Measure(size);

            var rtb = new RenderTargetBitmap((int)window.Width, (int)window.Height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(canvas);

            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(rtb));

            using (var stm = File.Create(filename))
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

        //only the last one selected has to rotate from -180 to 180
        private void AngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (selected.Count != 0)
            {
                var tr_rotate = new RotateTransform(0);
                var tr_group = new TransformGroup();
                tr_group.Children.Add(tr_rotate);

                var last = selected.Last();

                last.RenderTransform = tr_group;
                tr_rotate.Angle = AngleSlider.Value;
                tr_rotate.CenterX = last.Width / 2;
                tr_rotate.CenterY = last.Height / 2;
            }      
        }

        private void ColorsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selected.Count != 0)
            {
                var last = selected.Last();
                var comboItem = (ColorInfo)ColorsBox.SelectedItem;
                last.Fill = new SolidColorBrush(Color.FromRgb(comboItem.Rgb.R, comboItem.Rgb.G, comboItem.Rgb.B));
            }
        }




    }
}

#region #Disclaimer

// Author: Adalberto L. Simeone (Taranto, Italy)
// E-Mail: avengerdragon@gmail.com
// Website: http://www.avengersutd.com/blog
//
// This source code is Intellectual property of the Author
// and is released under the Creative Commons Attribution
// NonCommercial License, available at:
// http://creativecommons.org/licenses/by-nc/3.0/
// You can alter and use this source code as you wish,
// provided that you do not use the results in commercial
// projects, without the express and written consent of
// the Author.

#endregion #Disclaimer

#region Using directives

using Nikse.SubtitleEdit.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

#endregion Using directives

namespace Nikse.SubtitleEdit.Logic.ColorChooser
{
    public class ColorWheel : IDisposable
    {
        // These resources should be disposed
        // of when you're done with them.

        #region Delegates

        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);

        #endregion Delegates

        // Keep track of the current mouse state.

        #region MouseState enum

        public enum MouseState
        {
            MouseUp,
            ClickOnColor,
            DragInColor,
            ClickOnBrightness,
            DragInBrightness,
            ClickOutsideRegion,
            DragOutsideRegion,
        }

        #endregion MouseState enum

        // The code needs to convert back and forth between
        // degrees and radians. There are 2*PI radians in a
        // full circle, and 360 degrees. This constant allows
        // you to convert back and forth.
        private const double DegreesPerRadian = 180.0 / Math.PI;

        // COLOR_COUNT represents the number of distinct colors
        // used to create the circular gradient. Its value
        // is somewhat arbitrary -- change this to 6, for
        // example, to see what happens. 1536 (6 * 256) seems
        // a good compromise -- it's enough to get a full
        // range of colors, but it doesn't overwhelm the processor
        // attempting to generate the image. The color wheel
        // contains 6 sections, and each section displays
        // 256 colors. Seems like a reasonable compromise.
        private const int ColorCount = 6 * 256;
        private readonly int _brightnessMax;
        private readonly int _brightnessMin;

        private readonly Rectangle _brightnessRectangle;
        private readonly Region _brightnessRegion;
        private readonly double _brightnessScaling;
        private readonly int _brightnessX;
        private readonly Region _colorRegion;
        private readonly int _radius;
        private readonly Rectangle _selectedColorRectangle;
        public ColorChangedEventHandler ColorChanged;

        // selectedColor is the actual value selected
        // by the user. fullColor is the same color,
        // with its brightness set to 255.
        private ColorHandler.HSV _hsv;
        private ColorHandler.ARGB _argb;

        // Locations for the two "pointers" on the form.

        private int _brightness = byte.MaxValue;
        private Point _brightnessPoint;
        private readonly Point _centerPoint;
        private Bitmap _colorImage;
        private Point _colorPoint;
        private readonly Rectangle _colorRectangle;
        private MouseState _currentState = MouseState.MouseUp;
        private Color _fullColor;
        private Graphics _g;
        private Color _selectedColor = Color.White;

        public ColorWheel(Rectangle colorRectangle, Rectangle brightnessRectangle, Rectangle selectedColorRectangle)
        {
            // Caller must provide locations for color wheel
            // (colorRectangle), brightness "strip" (brightnessRectangle)
            // and location to display selected color (selectedColorRectangle).

            using (var path = new GraphicsPath())
            {
                // Store away locations for later use.
                _colorRectangle = colorRectangle;
                _brightnessRectangle = brightnessRectangle;
                _selectedColorRectangle = selectedColorRectangle;

                // Calculate the center of the circle.
                // Start with the location, then offset
                // the point by the radius.
                // Use the smaller of the width and height of
                // the colorRectangle value.
                _radius = Math.Min(colorRectangle.Width, colorRectangle.Height) / 2;
                _centerPoint = colorRectangle.Location;
                _centerPoint.Offset(_radius, _radius);

                // Start the pointer in the center.
                _colorPoint = _centerPoint;

                // Create a region corresponding to the color circle.
                // Code uses this later to determine if a specified
                // point is within the region, using the IsVisible
                // method.
                path.AddEllipse(colorRectangle);
                _colorRegion = new Region(path);

                // set { the range for the brightness selector.
                _brightnessMin = _brightnessRectangle.Top;
                _brightnessMax = _brightnessRectangle.Bottom;

                // Create a region corresponding to the
                // brightness rectangle, with a little extra
                // "breathing room".

                path.AddRectangle(new Rectangle(brightnessRectangle.Left, brightnessRectangle.Top - 10,
                    brightnessRectangle.Width + 10, brightnessRectangle.Height + 20));
                // Create region corresponding to brightness
                // rectangle. Later code uses this to
                // determine if a specified point is within
                // the region, using the IsVisible method.
                _brightnessRegion = new Region(path);

                // Set the location for the brightness indicator "marker".
                // Also calculate the scaling factor, scaling the height
                // to be between 0 and 255.
                _brightnessX = brightnessRectangle.Left + brightnessRectangle.Width;
                _brightnessScaling = (double)255 / (_brightnessMax - _brightnessMin);

                // Calculate the location of the brightness
                // pointer. Assume it's at the highest position.
                _brightnessPoint = new Point(_brightnessX, _brightnessMax);

                // Create the bitmap that contains the circular gradient.
                CreateGradient();
            }
        }

        public Color Color => _selectedColor;

        protected void OnColorChanged(ColorHandler.ARGB argb, ColorHandler.HSV hsv)
        {
            var e = new ColorChangedEventArgs(argb, hsv);
            ColorChanged(this, e);
        }

        public void SetMouseUp()
        {
            // Indicate that the user has
            // released the mouse.
            _currentState = MouseState.MouseUp;
        }

        public void Draw(Graphics g, ColorHandler.HSV hsv)
        {
            // Given HSV values, update the screen.
            _g = g;
            _hsv = hsv;
            CalcCoordsAndUpdate(_hsv);
            UpdateDisplay();
        }

        public void Draw(Graphics g, ColorHandler.ARGB argb)
        {
            // Given RGB values, calculate HSV and then update the screen.
            _g = g;
            _hsv = ColorHandler.RGBtoHSV(argb);
            CalcCoordsAndUpdate(_hsv);
            UpdateDisplay();
        }

        public void Draw(Graphics g, Point mousePoint)
        {
            try
            {
                // You've moved the mouse.
                // Now update the screen to match.

                // Keep track of the previous color pointer point,
                // so you can put the mouse there in case the
                // user has clicked outside the circle.
                Point newColorPoint = _colorPoint;
                Point newBrightnessPoint = _brightnessPoint;

                // Store this away for later use.
                _g = g;

                if (_currentState == MouseState.MouseUp)
                {
                    if (!mousePoint.IsEmpty)
                    {
                        if (_colorRegion.IsVisible(mousePoint))
                        {
                            // Is the mouse point within the color circle?
                            // If so, you just clicked on the color wheel.
                            _currentState = MouseState.ClickOnColor;
                        }
                        else if (_brightnessRegion.IsVisible(mousePoint))
                        {
                            // Is the mouse point within the brightness area?
                            // You clicked on the brightness area.
                            _currentState = MouseState.ClickOnBrightness;
                        }
                        else
                        {
                            // Clicked outside the color and the brightness
                            // regions. In that case, just put the
                            // pointers back where they were.
                            _currentState = MouseState.ClickOutsideRegion;
                        }
                    }
                }

                switch (_currentState)
                {
                    case MouseState.ClickOnBrightness:
                    case MouseState.DragInBrightness:
                        // Calculate new color information
                        // based on the brightness, which may have changed.
                        Point newPoint = mousePoint;
                        if (newPoint.Y < _brightnessMin)
                        {
                            newPoint.Y = _brightnessMin;
                        }
                        else if (newPoint.Y > _brightnessMax)
                        {
                            newPoint.Y = _brightnessMax;
                        }
                        newBrightnessPoint = new Point(_brightnessX, newPoint.Y);
                        _brightness = (int)((_brightnessMax - newPoint.Y) * _brightnessScaling);
                        _hsv.Value = _brightness;
                        _brightness = byte.MaxValue;
                        _argb = ColorHandler.HSVtoRGB(_hsv);
                        _brightness = (_argb.Red + _argb.Green + _argb.Blue) / 3;
                        break;

                    case MouseState.ClickOnColor:
                    case MouseState.DragInColor:
                        // Calculate new color information
                        // based on selected color, which may have changed.
                        newColorPoint = mousePoint;

                        // Calculate x and y distance from the center,
                        // and then calculate the angle corresponding to the
                        // new location.
                        Point delta = new Point(mousePoint.X - _centerPoint.X, mousePoint.Y - _centerPoint.Y);
                        int degrees = CalcDegrees(delta);

                        // Calculate distance from the center to the new point
                        // as a fraction of the radius. Use your old friend,
                        // the Pythagorean theorem, to calculate this value.
                        double distance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y) / _radius;

                        if (_currentState == MouseState.DragInColor)
                        {
                            if (distance > 1)
                            {
                                // Mouse is down, and outside the circle, but you
                                // were previously dragging in the color circle.
                                // What to do?
                                // In that case, move the point to the edge of the
                                // circle at the correct angle.
                                distance = 1;
                                newColorPoint = GetPoint(degrees, _radius, _centerPoint);
                            }
                        }

                        // Calculate the new HSV and RGB values.
                        _hsv.Hue = (degrees * 255 / 360);
                        _hsv.Saturation = (int)(distance * 255);
                        _brightness = byte.MaxValue;
                        _hsv.Value = _brightness;
                        _argb = ColorHandler.HSVtoRGB(_hsv);
                        if (_argb.Red < 0 || _argb.Red > byte.MaxValue || _argb.Green < 0 || _argb.Green > byte.MaxValue || _argb.Blue < 0 || _argb.Blue > byte.MaxValue)
                        {
                            UpdateDisplay();
                            return;
                        }
                        _brightness = (_argb.Red + _argb.Green + _argb.Blue) / 3;
                        _fullColor = ColorHandler.HSVtoColor(_hsv.Alpha, _hsv.Hue, _hsv.Saturation, 255);
                        break;
                }
                _selectedColor = ColorHandler.HSVtoColor(_hsv);

                // Raise an event back to the parent form,
                // so the form can update any UI it's using
                // to display selected color values.
                OnColorChanged(_argb, _hsv);

                // On the way out, set the new state.
                switch (_currentState)
                {
                    case MouseState.ClickOnBrightness:
                        _currentState = MouseState.DragInBrightness;
                        break;
                    case MouseState.ClickOnColor:
                        _currentState = MouseState.DragInColor;
                        break;
                    case MouseState.ClickOutsideRegion:
                        _currentState = MouseState.DragOutsideRegion;
                        break;
                }

                // Store away the current points for next time.
                _colorPoint = newColorPoint;
                _brightnessPoint = newBrightnessPoint;

                // Draw the gradients and points.
                UpdateDisplay();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        private Point CalcBrightnessPoint(int brightness)
        {
            // Take the value for brightness (0 to 255), scale to the
            // scaling used in the brightness bar, then add the value
            // to the bottom of the bar. return the correct point at which
            // to display the brightness pointer.
            return new Point(_brightnessX, (int)(_brightnessMax - brightness / _brightnessScaling));
        }

        private void UpdateDisplay()
        {
            // Update the gradients, and place the
            // pointers correctly based on colors and
            // brightness.

            using (Brush selectedBrush = new SolidBrush(_selectedColor))
            {
                // Draw the saved color wheel image.
                _g.DrawImage(_colorImage, _colorRectangle);

                // Draw the "selected color" rectangle.
                using (TextureBrush textureBrush = new TextureBrush(Resources.TransparentBackground))
                {
                    _g.FillRectangle(textureBrush, _selectedColorRectangle);
                }

                _g.FillRectangle(selectedBrush, _selectedColorRectangle);
                _g.DrawRectangle(Pens.Black, _selectedColorRectangle);
                // Draw the "brightness" rectangle.
                DrawLinearGradient(_fullColor);
                // Draw the two pointers.
                DrawColorPointer(_colorPoint);
                DrawBrightnessPointer(_brightnessPoint);
            }
        }

        private void CalcCoordsAndUpdate(ColorHandler.HSV hsv)
        {
            // Convert color to real-world coordinates and then calculate
            // the various points. HSV.Hue represents the degrees (0 to 360),
            // HSV.Saturation represents the radius.
            // This procedure doesn't draw anything--it simply
            // updates class-level variables. The UpdateDisplay
            // procedure uses these values to update the screen.

            // Given the angle (HSV.Hue), and distance from
            // the center (HSV.Saturation), and the center,
            // calculate the point corresponding to
            // the selected color, on the color wheel.
            _colorPoint = GetPoint((double)hsv.Hue / 255 * 360, (double)hsv.Saturation / 255 * _radius, _centerPoint);

            // Given the brightness (HSV.value), calculate the
            // point corresponding to the brightness indicator.
            _brightnessPoint = CalcBrightnessPoint(hsv.Value);

            // Store information about the selected color.
            _brightness = hsv.Value;
            _selectedColor = ColorHandler.HSVtoColor(hsv);
            _argb = ColorHandler.HSVtoRGB(hsv);

            // The full color is the same as HSV, except that the
            // brightness is set to full (255). This is the top-most
            // color in the brightness gradient.
            _fullColor = ColorHandler.HSVtoColor(hsv.Alpha, hsv.Hue, hsv.Saturation, 255);
        }

        private void DrawLinearGradient(Color topColor)
        {
            // Given the top color, draw a linear gradient
            // ranging from black to the top color. Use the
            // brightness rectangle as the area to fill.
            using (var lgb = new LinearGradientBrush(_brightnessRectangle, topColor, Color.Black, LinearGradientMode.Vertical))
            {
                _g.FillRectangle(lgb, _brightnessRectangle);
            }
        }

        private static int CalcDegrees(Point pt)
        {
            int degrees;

            if (pt.X == 0)
            {
                // The point is on the y-axis. Determine whether
                // it's above or below the x-axis, and return the
                // corresponding angle. Note that the orientation of the
                // y-coordinate is backwards. That is, A positive Y value
                // indicates a point BELOW the x-axis.
                degrees = pt.Y > 0 ? 270 : 90;
            }
            else
            {
                // This value needs to be multiplied
                // by -1 because the y-coordinate
                // is opposite from the normal direction here.
                // That is, a y-coordinate that's "higher" on
                // the form has a lower y-value, in this coordinate
                // system. So everything's off by a factor of -1 when
                // performing the ratio calculations.
                degrees = (int)(-Math.Atan((double)pt.Y / pt.X) * DegreesPerRadian);

                // If the x-coordinate of the selected point
                // is to the left of the center of the circle, you
                // need to add 180 degrees to the angle. ArcTan only
                // gives you a value on the right-hand side of the circle.
                if (pt.X < 0)
                {
                    degrees += 180;
                }

                // Ensure that the return value is
                // between 0 and 360.
                degrees = (degrees + 360) % 360;
            }
            return degrees;
        }

        private void CreateGradient()
        {
            // Create a new PathGradientBrush, supplying
            // an array of points created by calling
            // the GetPoints method.
            using (var pgb = new PathGradientBrush(GetPoints(_radius, new Point(_radius, _radius))))
            {
                // Set the various properties. Note the SurroundColors
                // property, which contains an array of points,
                // in a one-to-one relationship with the points
                // that created the gradient.
                pgb.CenterColor = Color.White;
                pgb.CenterPoint = new PointF(_radius, _radius);
                pgb.SurroundColors = GetColors();

                // Create a new bitmap containing
                // the color wheel gradient, so the
                // code only needs to do all this
                // work once. Later code uses the bitmap
                // rather than recreating the gradient.
                _colorImage = new Bitmap(_colorRectangle.Width, _colorRectangle.Height, PixelFormat.Format32bppArgb);
                using (Graphics newGraphics = Graphics.FromImage(_colorImage))
                {
                    newGraphics.FillEllipse(pgb, 0, 0, _colorRectangle.Width, _colorRectangle.Height);
                }
            }
        }

        private static Color[] GetColors()
        {
            // Create an array of COLOR_COUNT
            // colors, looping through all the
            // hues between 0 and 255, broken
            // into COLOR_COUNT intervals. HSV is
            // particularly well-suited for this,
            // because the only value that changes
            // as you create colors is the Hue.
            var colors = new Color[ColorCount];
            for (int i = 0; i <= ColorCount - 1; i++)
            {
                colors[i] = ColorHandler.HSVtoColor(255, (int)((double)(i * 255) / ColorCount), 255, 255);
            }
            return colors;
        }

        private static Point[] GetPoints(double radius, Point centerPoint)
        {
            // Generate the array of points that describe
            // the locations of the COLOR_COUNT colors to be
            // displayed on the color wheel.
            var points = new Point[ColorCount];
            for (int i = 0; i <= ColorCount - 1; i++)
            {
                points[i] = GetPoint((double)(i * 360) / ColorCount, radius, centerPoint);
            }
            return points;
        }

        private static Point GetPoint(double degrees, double radius, Point centerPoint)
        {
            // Given the center of a circle and its radius, along
            // with the angle corresponding to the point, find the coordinates.
            // In other words, convert from polar to rectangular coordinates.
            double radians = degrees / DegreesPerRadian;
            return new Point((int)(centerPoint.X + Math.Floor(radius * Math.Cos(radians))),
                (int)(centerPoint.Y - Math.Floor(radius * Math.Sin(radians))));
        }

        private void DrawColorPointer(Point pt)
        {
            // Given a point, draw the color selector.
            // The constant SIZE represents half
            // the width -- the square will be twice
            // this value in width and height.
            const int size = 3;
            _g.DrawRectangle(Pens.Black, pt.X - size, pt.Y - size, size * 2, size * 2);
        }

        private void DrawBrightnessPointer(Point pt)
        {
            // Draw a triangle for the
            // brightness indicator that "points"
            // at the provided point.
            const int height = 10;
            const int width = 7;

            var points = new Point[3];
            points[0] = pt;
            points[1] = new Point(pt.X + width, pt.Y + height / 2);
            points[2] = new Point(pt.X + width, pt.Y - height / 2);
            _g.FillPolygon(Brushes.Black, points);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of graphic resources
                _colorImage?.Dispose();
                _colorRegion?.Dispose();
                _brightnessRegion?.Dispose();
                _g?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}

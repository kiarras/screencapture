using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace screencapture
{
    /// <summary>
    /// Lógica de interacción para Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {


        #region Imports DLL
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);
        #endregion

        enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }


        private Rect r;
        private bool capture;
        private Image imagePreview;
        public Window2()
        {
            InitializeComponent();
        }
        public Window2(Image cuadrado)
        {
            imagePreview = cuadrado;
            InitializeComponent();
        }

        private void transWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!capture)
            {
                r.Location = e.GetPosition(this);
            }
            else
            {
                imagePreview.Source = CaptureRegion((int)r.Width, (int)r.Height);
                Close();
                
            }
            
        }

        private void transWindow_MouseMove(object sender, MouseEventArgs e)
        {
            r.Width = Math.Abs(r.Location.X - e.GetPosition(this).X);
            r.Height = Math.Abs(r.Location.Y - e.GetPosition(this).Y);
            Rectangle rect = new Rectangle();
            rect.Width = r.Width;
            rect.Height = r.Height;
            rect.Stroke = Brushes.Red;

        }

        private void transWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            capture = true;
            Cursor = Cursors.Hand;
        }
        public static BitmapSource CaptureRegion(int width, int height)
        {
            IntPtr sourceDC = IntPtr.Zero;
            IntPtr targetDC = IntPtr.Zero;
            IntPtr compatibleBitmapHandle = IntPtr.Zero;

            //int width = PrimaryScreen.Bounds.Width;
            //int height = Screen.PrimaryScreen.Bounds.Height;

            const int y = 100;
            const int x = 100;
            BitmapSource bitmapSource = null;
            try
            {
                // gets the main desktop and all open windows
                sourceDC = GetDC(GetDesktopWindow());

                targetDC = CreateCompatibleDC(sourceDC);

                // create a bitmap compatible with our target DC
                compatibleBitmapHandle = CreateCompatibleBitmap(sourceDC, width, height);

                // gets the bitmap into the target device context
                SelectObject(targetDC, compatibleBitmapHandle);

                // copy from source to destination
                BitBlt(targetDC, 0, 0, width, height, sourceDC, x, y, TernaryRasterOperations.SRCCOPY);

                // Here's the WPF glue to make it all work. It converts from an
                // hBitmap to a BitmapSource. Love the WPF interop functions
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    compatibleBitmapHandle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error capturing region {0},{1},{2},{3}", x, y, width, height), ex);
            }
            finally
            {
                DeleteObject(compatibleBitmapHandle);

                ReleaseDC(IntPtr.Zero, sourceDC);
                ReleaseDC(IntPtr.Zero, targetDC);
            }
            return bitmapSource;
        }
    }
}

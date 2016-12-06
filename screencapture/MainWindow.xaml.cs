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
using System.Windows.Media.Imaging; //Screenshot 
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Interop; //transparent Child-Window
using System.Runtime.InteropServices; //Import User32 DLL

namespace screencapture
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CONSTANTES Y VARIABLES
        Rect rectCapture;
        rectWindow rW;
        Window2 tW; 
        #endregion

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



        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                captureALL();

            }
            catch (Exception objError)
            {
                MessageBox.Show(objError.ToString(), "Error");
            }
            finally
            {
                Show();
            }
        }

        private void captureALL()
        {
            //ocultamos la ventana de la aplicación para que 
            //no aparezca en la captura de pantalla
            Hide();

            //esperamos unos milisegundos para asegurarnos que 
            //se ha ocultado la ventana
            System.Threading.Thread.Sleep(250);

            //obtenemos la resolución de la campura
            rectCapture.Location = new Point(0, 0);
            rectCapture.Width = SystemParameters.FullPrimaryScreenWidth;
            rectCapture.Height = SystemParameters.FullPrimaryScreenHeight;



            //creamos un Bitmap del tamaño de nuestra pantalla


            imagePreview.Source = CaptureRegion((int)rectCapture.Width, (int)rectCapture.Height);
        }


        // capture a region of a the screen, defined by the hWnd
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

        private void buttonRect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ocultamos la ventana de la aplicación para que 
                //no aparezca en la captura de pantalla
                Hide();

                //esperamos unos milisegundos para asegurarnos que 
                //se ha ocultado la ventana
                System.Threading.Thread.Sleep(250);

                rW = new rectWindow();
                rW.Show();
            }
            catch (Exception objError)
            {
                MessageBox.Show(objError.ToString(), "Error");
            }
            finally
            {
                //if(!rW.IsActive)
                //Show();
            }
        }

        private void rectboton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //captureALL();
                //ocultamos la ventana de la aplicación para que 
                //no aparezca en la captura de pantalla
                Hide();

                tW = new Window2(imagePreview);
                tW.ShowDialog();
                
            }
            catch (Exception objError)
            {
                MessageBox.Show(objError.ToString(), "Error");
            }
            finally
            {
                //while(tW.IsActive)
                //{
                //    Hide();
                //}
                Show();
                
                
            }
        }

        private void imagePreview_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Show();
        }
    }
}

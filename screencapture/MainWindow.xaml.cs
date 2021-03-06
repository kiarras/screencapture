﻿using System;
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
using Microsoft.Win32;
using System.IO;


namespace screencapture
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Importacion DLL 
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

        #region CONSTANTES Y VARIABLES
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
        public static Rect rectCapture;
        Window2 tW; 
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }
        // Boton de captura de toda la pantalla
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

        //Captura de toda la pantalla
        private void captureALL()
        {
            // Se oculta la ventana de la aplicación para que no aparezca en la captura de pantalla
            Hide();

            //esperamos unos milisegundos para asegurarnos que se ha ocultado la ventana
            System.Threading.Thread.Sleep(250);

            //Resolución de la captura
            rectCapture.Location = new Point(0, 0);
            rectCapture.Width = SystemParameters.FullPrimaryScreenWidth;
            rectCapture.Height = SystemParameters.FullPrimaryScreenHeight;

            //creamos un Bitmap del tamaño de nuestra pantalla
            imagePreview.Source = CaptureRegion((int)rectCapture.Width, (int)rectCapture.Height);
        }

        //Capturar una region de la pantalla
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

        //Capturar recuadro de pantalla
        private void rectboton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ocultar la ventana de la aplicación para que no aparezca en la captura de pantalla
                Hide();

                //Crear y abrir ventana de seleccion de recuadro
                tW = new Window2(imagePreview, rectCapture);
                tW.ShowDialog();
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

        //Copiar imagen al portapapeles
        private void button_copy_Click(object sender, RoutedEventArgs e)
        {
            //Crear bitmap para el portapapeles
            BitmapSource bmpSource = (BitmapSource)imagePreview.Source; 

            //Copiar imagen en el portapapeles 
            Clipboard.SetImage(bmpSource);
        }

        //Guardar imagen en disco
        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            // Abrir cuadro de dialogo para saval imagen
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Guardar imagen como...";
            dlg.DefaultExt = "jpg";
            dlg.Filter = "Images jpg|*.jpg| tif|*.tif| png|*.png| bmp|*.bmp";
            dlg.AddExtension = true;
            dlg.FilterIndex = 1;

            if (dlg.ShowDialog() == true)
            {
                //Extraer extensión del archivo a guardar
                string ext = System.IO.Path.GetExtension(dlg.FileName);
                //Crear archivo stream para guardar
                FileStream stream = new FileStream(dlg.FileName, FileMode.Create);
                //Crear bitmap para el stream
                BitmapSource bmpSource = (BitmapSource)imagePreview.Source;

                //Segun la extensión codificar el bitmap y guardar
                switch (ext)
                {
                    case ".jpg":
                        JpegBitmapEncoder encoderJpeg = new JpegBitmapEncoder();
                        encoderJpeg.Frames.Add(BitmapFrame.Create(bmpSource));
                        encoderJpeg.Save(stream);
                        break;
                    case ".tif":
                        TiffBitmapEncoder encoderTiff = new TiffBitmapEncoder();
                        encoderTiff.Frames.Add(BitmapFrame.Create(bmpSource));
                        encoderTiff.Save(stream);
                        break;
                    case ".png":
                        PngBitmapEncoder encoderPng = new PngBitmapEncoder();
                        encoderPng.Frames.Add(BitmapFrame.Create(bmpSource));
                        encoderPng.Save(stream);
                        break;
                    case ".bmp":
                        BmpBitmapEncoder encoderBmp = new BmpBitmapEncoder();
                        encoderBmp.Frames.Add(BitmapFrame.Create(bmpSource));
                        encoderBmp.Save(stream);
                        break;
                }
                        
            }
        }
    }
}

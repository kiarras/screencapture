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
using System.Globalization; //Culture-Info 
using System.Windows.Interop; //transparent Child-Window 
using System.Runtime.InteropServices; //Import User32 DLL 


namespace screencapture
{
    /// <summary> 
    /// Interaktionslogik für MainWindow.xaml 
    /// </summary> 
    public partial class rectWindow : Window
    {
        #region Formular 
        //==========================< Region Formular >========================== 
        public rectWindow()
        {
            //-------------< MainWindow() >------------- 
            //*1.Event: Initialisierung 
            InitializeComponent();
            //-------------</ MainWindow() >------------- 
        }

        private void wndMain_Loaded(object sender, RoutedEventArgs e)
        {
            //-------------< wndMain_Loaded() >------------- 
            //*2.Event: 
            fp_Init_FactorDPI();
            //-------------</ wndMain_Loaded() >------------- 
        }
        //==========================< Region Formular >========================== 
        #endregion

        #region Buttons 
        //==========================< Region Buttons >========================== 
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //-------------< BtnClose_Click() >------------- 
            this.Close();
            //-------------</ BtnClose_Click() >------------- 
        }
        private void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            //-------------< BtnCapture_Click() >------------- 
            //----< Screen Breite Hoehe ermitteln >---- 

            //< Bitmap Screen Desktop erstellen > 
            ctlImage.Source = fp_Capture_Desktop_byScreenDC();
            //</ Bitmap Screen Desktop erstellen > 

            ctlSelection.Margin = new Thickness(0, 0, 0, 0);
            ctlSelection.Width = ctlBorder_Image.ActualWidth;
            ctlSelection.Height = ctlBorder_Image.ActualHeight;
            //-------------</ BtnCapture_Click() >------------- 
        }

        private void BtnToClipboard_Click(object sender, RoutedEventArgs e)
        {
            //-------------< BtnToClipboard_Click() >------------- 
            //*speichert das Image (Bitmap,BitmapSource, ImageSource) in die Clipboard 
            //namespace System.Windows.Clipboard 

            //< BitmapSource aus Control erstellen > 
            BitmapSource bmpSource = (BitmapSource)ctlImage.Source;
            //</ BitmapSource aus Control erstellen > 

            //< in Clipboard speichern > 
            Clipboard.SetImage(bmpSource);
            //</ in Clipboard speichern > 
            //-------------</ BtnToClipboard_Click() >------------- 
        }
        //==========================</ Region Buttons >========================== 
        #endregion


        #region Funktionen 
        //==========================< Region Funktionen >========================== 
        public double factor_dpi_Width = 1;
        public double factor_dpi_Height = 1;

        /// <summary> 
        /// Initialisiere Faktor DPI zu Pixel 
        /// </summary> 
        public void fp_Init_FactorDPI()
        {
            Window wndMain = Application.Current.MainWindow;
            PresentationSource srcDevice = PresentationSource.FromVisual(wndMain);
            Matrix m = srcDevice.CompositionTarget.TransformToDevice;
            factor_dpi_Width = m.M11;
            factor_dpi_Height = m.M22;
        }



        /// <summary> 
        /// Capture the screenshot by using screenDC user32 
        /// source http://stackoverflow.com/questions/1736287/capturing-a-window-with-wpf 
        /// </summary> 
        public BitmapSource fp_Capture_Desktop_byScreenDC()
        {
            //-------------< fp_Capture_Desktop_byScreenDC() >------------- 
            //--< Umrechnung WPF Screen-Auflösung in DPI >-- 

            double Screen_Width_inDPI = SystemParameters.PrimaryScreenWidth * factor_dpi_Width;
            double Screen_Height_inDPI = SystemParameters.PrimaryScreenHeight * factor_dpi_Height;
            //--</ Umrechnung WPF Screen-Auflösung in DPI >-- 
            //----</ Screen Breite Hoehe ermitteln >---- 

            IntPtr screenDC = GetDC(IntPtr.Zero);
            IntPtr memDC = CreateCompatibleDC(screenDC);
            IntPtr hBitmap = CreateCompatibleBitmap(screenDC, (int)Screen_Width_inDPI, (int)Screen_Height_inDPI);
            SelectObject(memDC, hBitmap); // Select bitmap from compatible bitmap to memDC 

            // TODO: BitBlt may fail horribly 
            BitBlt(memDC, 0, 0, (int)Screen_Width_inDPI, (int)Screen_Height_inDPI, screenDC, 0, 0, TernaryRasterOperations.SRCCOPY);
            BitmapSource bsource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            ReleaseDC(IntPtr.Zero, screenDC);
            ReleaseDC(IntPtr.Zero, memDC);
            return bsource;
            //-------------</ fp_Capture_Desktop_byScreenDC() >------------- 
        }
        #region WINAPI DLL Imports 

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private enum TernaryRasterOperations : uint
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
            WHITENESS = 0x00FF0062
        }

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        #endregion

        //==========================</ Region Funktionen >========================== 
        #endregion


        #region Mouse-Events 
        //==========================< Region: Mouse_Events >========================== 
        Boolean isDragging = false;
        private void ctlImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //-------------< ctlImage_MouseDown() >------------- 
            if (isDragging == false)
            {
                isDragging = true;
                fp_set_Cropper(sender, e);
            }
            //-------------</ ctlImage_MouseDown() >------------- 
        }

        private void ctlImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //-------------< ctlImage_MouseUp() >------------- 
            if (isDragging)
            {
                isDragging = false;
                CropType = 0;
            }
            //-------------</ ctlImage_MouseUp() >------------- 
        }



        private void ctlImage_MouseMove(object sender, MouseEventArgs e)
        {
            //-------------< ctlImage_MouseMove() >------------- 
            fp_set_Cropper(sender, e);
            //-------------</ ctlImage_MouseMove() >------------- 
        }

        private void fp_set_Cropper(object sender, MouseEventArgs e)
        {
            //-------------< fp_set_Cropper() >------------- 
            if (isDragging)
            {
                if (e.LeftButton != MouseButtonState.Pressed)
                {
                    //isDragging = false; 
                }
                else
                {
                    //< Positionen ermitteln > 
                    double x_PosMouse = e.GetPosition(ctlBorder_Image).X;
                    double y_PosMouse = e.GetPosition(ctlBorder_Image).Y;
                    double posLeft_SelectFrame = ctlSelection.Margin.Left;
                    double posTop_SelectFrame = ctlSelection.Margin.Top;
                    double width_SelectFrame = ctlSelection.ActualWidth;
                    double heigth_SelectFrame = ctlSelection.ActualHeight;
                    //</ Positionen ermitteln > 
                    //double newWidth = width_SelectFrame ; 
                    //double newHeight = heigth_SelectFrame; 
                    //double newLeft = posLeft_SelectFrame; 
                    //double newTop = posTop_SelectFrame; 

                    if (CropType == 0)
                    {
                        //*Direct Init Crop 
                        //*wenn nichts markiert, dann fahre direkt LinksOben-RechtsUnten 
                        posLeft_SelectFrame = x_PosMouse;
                        posTop_SelectFrame = y_PosMouse;
                        width_SelectFrame = 1;
                        heigth_SelectFrame = 1;
                        ctlSelection.Margin = new Thickness(x_PosMouse, y_PosMouse, 0, 0); //fix leftTop 
                        CropType = (int)CropTypes.BottomRight;
                    }


                    //------</ Edges bearbeiten >------ 
                    //< Top_Left > 
                    if (CropType == (int)CropTypes.TopLeft)
                    {
                        heigth_SelectFrame = (posTop_SelectFrame + heigth_SelectFrame) - y_PosMouse; //neue hoehe 
                        width_SelectFrame = (posLeft_SelectFrame + width_SelectFrame) - x_PosMouse;//neue breite 
                        posLeft_SelectFrame = x_PosMouse;
                        posTop_SelectFrame = y_PosMouse;
                    }
                    //</ Top_Left > 

                    //< Top_Right > 
                    if (CropType == (int)CropTypes.TopRight)
                    {
                        heigth_SelectFrame = (posTop_SelectFrame + heigth_SelectFrame) - y_PosMouse; //neue hoehe 
                        width_SelectFrame = x_PosMouse - posLeft_SelectFrame;//neue breite 
                                                                             //posLeft_SelectFrame = x_PosMouse; 
                        posTop_SelectFrame = y_PosMouse;
                    }
                    //</ Top_Right > 

                    //< Bottom_Left > 
                    if (CropType == (int)CropTypes.BottomLeft)
                    {
                        heigth_SelectFrame = y_PosMouse - posTop_SelectFrame; //neue hoehe 
                        width_SelectFrame = (posLeft_SelectFrame + width_SelectFrame) - x_PosMouse;//neue breite 
                        posLeft_SelectFrame = x_PosMouse;
                    }
                    //</ Bottom_Left > 

                    //< Bottom_Right > 
                    if (CropType == (int)CropTypes.BottomRight)
                    {
                        width_SelectFrame = x_PosMouse - posLeft_SelectFrame;
                        heigth_SelectFrame = y_PosMouse - posTop_SelectFrame;
                    }
                    //</ Bottom_Right > 
                    //------< Edges bearbeiten >------ 


                    //-< korrekturen > 
                    if (width_SelectFrame < 0) { width_SelectFrame = 0; }
                    if (heigth_SelectFrame < 0) { heigth_SelectFrame = 0; }
                    //-</ korrekturen > 


                    //--< SET new values >-- 
                    ctlSelection.Margin = new Thickness(posLeft_SelectFrame, posTop_SelectFrame, 0, 0);
                    ctlSelection.Width = width_SelectFrame;
                    ctlSelection.Height = heigth_SelectFrame;
                    //--</ SET new values >-- 

                    //< anzeigen > 
                    if (ctlSelection.Visibility != Visibility.Visible)
                    { ctlSelection.Visibility = Visibility.Visible; }
                    //</ anzeigen > 
                }
            }
            //-------------</ fp_set_Cropper() >------------- 
        }
        //==========================</ Region: Mouse_Events >========================== 
        #endregion

        #region CropEdges 
        //==========================< Region: CropEdges >========================== 
        public int CropType = 0;

        enum CropTypes
        {
            TopLeft = 1,
            TopRight = 2,
            BottomLeft = 3,
            BottomRight = 4
        }

        private void edge_TopLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CropType = (int)CropTypes.TopLeft;
        }

        private void edge_TopRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CropType = (int)CropTypes.TopRight;
        }

        private void edge_BottomLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CropType = (int)CropTypes.BottomLeft;
        }

        private void edge_BottomRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CropType = (int)CropTypes.BottomRight;
        }
        //==========================< Region: CropEdges >========================== 
        #endregion
    }





}

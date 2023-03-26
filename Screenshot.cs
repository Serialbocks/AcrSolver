using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AcrSolver
{
    public static class Screenshot
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        const int SRCCOPY = 0xcc0020;

        [DllImportAttribute("gdi32.dll")]
        private static extern int BitBlt(
          IntPtr hdcDest,     // handle to destination DC (device context)
          int nXDest,         // x-coord of destination upper-left corner
          int nYDest,         // y-coord of destination upper-left corner
          int nWidth,         // width of destination rectangle
          int nHeight,        // height of destination rectangle
          IntPtr hdcSrc,      // handle to source DC
          int nXSrc,          // x-coordinate of source upper-left corner
          int nYSrc,          // y-coordinate of source upper-left corner
          Int32 dwRop         // raster operation code
          );
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);
        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);


        public static Bitmap PrintWindow()
        {
            var tableHandleNullable = GetTableHandle();
            if(tableHandleNullable == null)
            {
                return null;
            }
            var handle = (IntPtr)tableHandleNullable;
            var windowRect = new RECT();

            GetWindowRect(handle, out windowRect);
            int width = windowRect.Width;
            int height = windowRect.Height;

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(windowRect.left, windowRect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }
            return bmp;

            //// get te hDC of the target window
            //IntPtr hdcSrc = GetWindowDC(handle);
            //
            //// create a device context we can copy to
            //IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            //
            //// create a bitmap we can copy it to,
            //IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            //// select the bitmap object
            //IntPtr hOld = SelectObject(hdcDest, hBitmap);
            //
            //// bitblt over
            //BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0,
            //    (int)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
            //// restore selection
            //SelectObject(hdcDest, hOld);
            //// clean up
            //DeleteDC(hdcDest);
            //ReleaseDC(handle, hdcSrc);
            //
            //Image img = Image.FromHbitmap(hBitmap);
            //// free up the Bitmap object
            //DeleteObject(hBitmap);
            //
            //return img;

            //RECT rc;
            //GetWindowRect(tableHandle, out rc);
            //
            //WINDOWINFO info = new WINDOWINFO();
            //info.cbSize = (uint)Marshal.SizeOf(info);
            //GetWindowInfo(tableHandle, ref info);
            //
            //var destHandle = CreateCompatibleDC(tableHandle);
            //
            //BitBlt(destHandle, 0, 0, rc.Width, rc.Height, tableHandle, 0, 0,
            //    (int)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
            //
            //Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            //Graphics gfxBmp = Graphics.FromImage(bmp);
            //IntPtr hdcBitmap = gfxBmp.GetHdc();
            //
            //PrintWindow(destHandle, hdcBitmap, 0);
            //
            //gfxBmp.ReleaseHdc(hdcBitmap);
            //gfxBmp.Dispose();
            //
            //return bmp;
        }

        private static IntPtr? GetTableHandle()
        {
            var process = GetAcrProcess();
            if(process == null)
            {
                return null;
            }

            IntPtr? tablehandle = null;
            foreach(var windowHandle in EnumerateProcessWindowHandles(process.Id))
            {
                string windowTitle = GetWindowTitle(windowHandle);
                if(windowTitle.ToUpper().Contains("NO LIMIT"))
                {
                    tablehandle = windowHandle;
                }
            }
            
            return tablehandle;
        }

        private static Process GetAcrProcess()
        {
            var processes = Process.GetProcesses();
            Process acrProcess = null;
            foreach(var process in processes)
            {
                if(process.ProcessName.ToUpper().Contains("AMERICAS"))
                {
                    acrProcess = process;
                    break;
                }
            }
            return acrProcess;
        }
        private static string GetWindowTitle(IntPtr windowHandle)
        {
            uint SMTO_ABORTIFHUNG = 0x0002;
            uint WM_GETTEXT = 0xD;
            int MAX_STRING_SIZE = 32768;
            IntPtr result;
            string title = string.Empty;
            IntPtr memoryHandle = Marshal.AllocCoTaskMem(MAX_STRING_SIZE);
            Marshal.Copy(title.ToCharArray(), 0, memoryHandle, title.Length);
            SendMessageTimeout(windowHandle, WM_GETTEXT, (IntPtr)MAX_STRING_SIZE, memoryHandle, SMTO_ABORTIFHUNG, (uint)1000, out result);
            title = Marshal.PtrToStringAuto(memoryHandle);
            Marshal.FreeCoTaskMem(memoryHandle);
            return title;
        }

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);
        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        private static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
        {
        }
        public RECT(int Left, int Top, int Right, int Bottom)
        {
            left = Left;
            top = Top;
            right = Right;
            bottom = Bottom;
        }

        public int X
        {
            get { return left; }
            set { left = value; }
        }
        public int Y
        {
            get { return top; }
            set { top = value; }
        }
        public int Left
        {
            get { return left; }
            set { left = value; }
        }
        public int Top
        {
            get { return top; }
            set { top = value; }
        }
        public int Right
        {
            get { return right; }
            set { right = value; }
        }
        public int Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }
        public int Height
        {
            get { return bottom - top; }
            set { bottom = value + top; }
        }
        public int Width
        {
            get { return right - left; }
            set { right = value + left; }
        }
        public Point Location
        {
            get { return new Point(Left, Top); }
            set
            {
                left = value.X;
                top = value.Y;
            }
        }
        public Size Size
        {
            get { return new Size(Width, Height); }
            set
            {
                right = value.Width + left;
                bottom = value.Height + top;
            }
        }

        public static implicit operator Rectangle(RECT Rectangle)
        {
            return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
        }
        public static implicit operator RECT(Rectangle Rectangle)
        {
            return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
        }
        public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
        {
            return Rectangle1.Equals(Rectangle2);
        }
        public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
        {
            return !Rectangle1.Equals(Rectangle2);
        }

        public override string ToString()
        {
            return "{Left: " + left + "; " + "Top: " + top + "; Right: " + right + "; Bottom: " + bottom + "}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool Equals(RECT Rectangle)
        {
            return Rectangle.Left == left && Rectangle.Top == top && Rectangle.Right == right && Rectangle.Bottom == bottom;
        }

        public override bool Equals(object Object)
        {
            if (Object is RECT)
            {
                return Equals((RECT)Object);
            }
            else if (Object is Rectangle)
            {
                return Equals(new RECT((Rectangle)Object));
            }

            return false;
        }
    }
}

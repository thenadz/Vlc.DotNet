using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops.Signatures;

namespace Vlc.DotNet.Wpf
{
    public partial class VlcControl : HwndHost, ISupportInitialize
    {
        private IntPtr hwndControl;

        private IntPtr hwndHost;

        private int hostHeight;

        private int hostWidth;

        private VlcMediaPlayer myVlcMediaPlayer;

        private bool disposed = false;

        public event EventHandler<VlcLibDirectoryNeededEventArgs> VlcLibDirectoryNeeded;

        #region VlcControl Init

        //static VlcControl()
        //{
        //    WNDCLASSEX wnd = default(WNDCLASSEX);
        //    wnd.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
        //    wnd.style = (int)(CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS); //Doubleclicks are active
        //    wnd.hbrBackground = (IntPtr)COLOR_BACKGROUND + 1; //Black background, +1 is necessary
        //    wnd.cbClsExtra = 0;
        //    wnd.cbWndExtra = 0;
        //    wnd.hInstance = Marshal.GetHINSTANCE(typeof(VlcControl).Module); // alternative: Process.GetCurrentProcess().Handle;
        //    wnd.hIcon = IntPtr.Zero;
        //    wnd.hCursor = LoadCursor(IntPtr.Zero, (int)IDC_CROSS); // Crosshair cursor;
        //    wnd.lpszMenuName = null;
        //    wnd.lpszClassName = nameof(VlcControl);
        //    wnd.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegWndProc);
        //    wnd.hIconSm = IntPtr.Zero;
        //    registerResult = RegisterClassEx(ref wnd);
        //}

        public VlcControl()
        {
        }

        /// <summary>When overridden in a derived class, creates the window to be hosted. </summary>
        /// <returns>The handle to the child Win32 window to create.</returns>
        /// <param name="hwndParent">The window handle of the parent window.</param>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            hwndControl = IntPtr.Zero;

            int width = (!double.IsNaN(Width) && !double.IsInfinity(Width)) ? (int)Math.Round(Width) : 0;
            int height = (!double.IsNaN(Height) && !double.IsInfinity(Height)) ? (int)Math.Round(Height) : 0;
            hwndControl = CreateWindowEx(0, "static", string.Empty, WS_CHILD | WS_VISIBLE,
                                          0, 0,
                                          width, height,
                                          hwndParent.Handle,
                                          (IntPtr)HOST_ID,
                                          IntPtr.Zero,
                                          0);

            if (hwndControl == IntPtr.Zero)
            {
                uint error = GetLastError();
                //return false;
            }

            return new HandleRef(this, hwndControl);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        /// <summary>When overridden in a derived class, destroys the hosted window. </summary>
        /// <param name="hwnd">A structure that contains the window handle.</param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        [Category("Media Player")]
        public string[] VlcMediaplayerOptions { get; set; }

        [Category("Media Player")]
        public DirectoryInfo VlcLibDirectory { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsPlaying
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.IsPlaying();
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Starts the initialization process for this element.
        /// </summary>
        public void BeginInit()
        {
            // not used yet
        }

        /// <summary>
        /// Indicates that the initialization process for the element is complete.
        /// </summary>
        /// <exception cref="System.Exception">'VlcLibDirectory' must be set.</exception>
        public void EndInit()
        {
            if (IsInDesignMode || myVlcMediaPlayer != null)
                return;
            if (VlcLibDirectory == null && (VlcLibDirectory = OnVlcLibDirectoryNeeded()) == null)
            {
                throw new Exception("'VlcLibDirectory' must be set.");
            }

            if (VlcMediaplayerOptions == null)
            {
                myVlcMediaPlayer = new VlcMediaPlayer(VlcLibDirectory);
            }
            else
            {
                myVlcMediaPlayer = new VlcMediaPlayer(VlcLibDirectory, VlcMediaplayerOptions);
            }
            myVlcMediaPlayer.VideoHostControlHandle = Handle;
            RegisterEvents();
        }

        private bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(this);

        protected void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (myVlcMediaPlayer != null)
                    {
                        UnregisterEvents();
                        if (IsPlaying)
                            Stop();
                        myVlcMediaPlayer.Dispose();
                    }
                    myVlcMediaPlayer = null;
                    base.Dispose(disposing);
                }
                disposed = true;
            }
        }

        public DirectoryInfo OnVlcLibDirectoryNeeded()
        {
            EventHandler<VlcLibDirectoryNeededEventArgs> del = VlcLibDirectoryNeeded;
            if (del != null)
            {
                VlcLibDirectoryNeededEventArgs args = new VlcLibDirectoryNeededEventArgs();
                del(this, args);
                return args.VlcLibDirectory;
            }
            return null;
        }
        #endregion

        #region VlcControl Functions & Properties

        public void Play()
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.Play();
            }
        }

        public void Play(FileInfo file, params string[] options)
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.SetMedia(file, options);
                Play();
            }
        }

        public void Play(Uri uri, params string[] options)
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.SetMedia(uri, options);
                Play();
            }
        }

        public void Play(string mrl, params string[] options)
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.SetMedia(mrl, options);
                Play();
            }
        }

        public void Pause()
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.Pause();
            }
        }

        public void Stop()
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                myVlcMediaPlayer.Stop();
            }

        }

        public VlcMedia GetCurrentMedia()
        {
            //EndInit();
            if (myVlcMediaPlayer != null)
            {
                return myVlcMediaPlayer.GetMedia();
            }
            else
            {
                return null;
            }
        }

        public void TakeSnapshot(string fileName)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            myVlcMediaPlayer.TakeSnapshot(fileInfo);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public float Position
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Position;
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (myVlcMediaPlayer != null)
                {
                    myVlcMediaPlayer.Position = value;
                }

            }
        }

        [Browsable(false)]
        public IChapterManagement Chapter
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Chapters;
                }
                else
                {
                    return null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public float Rate
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Rate;
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (myVlcMediaPlayer != null)
                {
                    myVlcMediaPlayer.Rate = value;
                }
            }
        }

        [Browsable(false)]
        public MediaStates State
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.State;
                }
                else
                {
                    return MediaStates.NothingSpecial;
                }
            }
        }

        [Browsable(false)]
        public ISubTitlesManagement SubTitles
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.SubTitles;
                }
                else
                {
                    return null;
                }

            }
        }

        [Browsable(false)]
        public IVideoManagement Video
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Video;
                }
                else
                {
                    return null;
                }
            }
        }

        [Browsable(false)]
        public IAudioManagement Audio
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Audio;
                }
                else
                {
                    return null;
                }
            }
        }

        [Browsable(false)]
        public long Length
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Length;
                }
                else
                {
                    return -1;
                }

            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public long Time
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Time;
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                if (myVlcMediaPlayer != null)
                {
                    myVlcMediaPlayer.Time = value;
                }
            }
        }

        [Browsable(false)]
        public int Spu
        {
            get
            {
                if (myVlcMediaPlayer != null)
                {
                    return myVlcMediaPlayer.Spu;
                }
                return -1;
            }

            set
            {
                if (myVlcMediaPlayer != null)
                {
                    myVlcMediaPlayer.Spu = value;
                }
            }
        }

        private static ushort? registerResult;

        private static ushort RegisterResult { get;  }

        public void SetMedia(FileInfo file, params string[] options)
        {
            //EndInit();
            myVlcMediaPlayer.SetMedia(file, options);
        }

        public void SetMedia(Uri file, params string[] options)
        {
            //EndInit();
            myVlcMediaPlayer.SetMedia(file, options);
        }

        public void SetMedia(string mrl, params string[] options)
        {
            //EndInit();
            myVlcMediaPlayer.SetMedia(mrl, options);
        }

        #endregion

        #region PInvoke Declarations

        private const int WS_CHILD = 0x40000000;

        private const int WS_VISIBLE = 0x10000000;

        private const int LBS_NOTIFY = 0x00000001;

        private const int HOST_ID = 0x00000002;

        private const int LISTBOX_ID = 0x00000001;

        private const int WS_VSCROLL = 0x00200000;

        private const int WS_BORDER = 0x00800000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassEx")]
        internal static extern ushort RegisterClassEx([In] ref WNDCLASSEX lpWndClass);

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                        string lpszClassName,
                                                        string lpszWindowName,
                                                        int style,
                                                        int x, int y,
                                                        int width, int height,
                                                        IntPtr hwndParent,
                                                        IntPtr hMenu,
                                                        IntPtr hInst,
                                                        [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        #endregion PInvoke Declarations
    }
}
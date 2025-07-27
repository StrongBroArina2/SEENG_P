using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using VRage.Utils;

namespace SEENG_Core
{
    public class TextureOverlayRenderer : Form
    {
        private static TextureOverlayRenderer instance;
        private PictureBox pictureBox;
        private string currentModPath;

     
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const float OverlayWidthPercent = 0.25f;  
        private const float CenterXPercent = 0.0f;       
        private const float CenterYPercent = 0.0f;      
        private const float AspectRatio = 16f / 9f;      

        private TextureOverlayRenderer()
        {
            SetProcessDPIAware();
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            Enabled = false;
            SetStyle(ControlStyles.UserMouse, false);
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            MyLog.Default.WriteLine($"screen: {screenWidth}x{screenHeight}");
            float relativeWidth = OverlayWidthPercent; 
            float relativeHeight = relativeWidth / AspectRatio;
            int windowWidth = (int)(screenWidth * relativeWidth);
            int windowHeight = (int)(screenHeight * relativeHeight);

            // yeah
            float relativeX = CenterXPercent - (relativeWidth / 2f);
            float relativeY = CenterYPercent - (relativeHeight / 2f);
            int x = (int)(screenWidth * relativeX);
            int y = (int)(screenHeight * relativeY);

            Location = new Point(x, y);
            Size = new Size(windowWidth, windowHeight);

            // dat cool picture
            pictureBox = new PictureBox
            {
                Size = Size,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Dock = DockStyle.Fill
            };
            Controls.Add(pictureBox);

            Load += (s, e) => SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE );

            // bad moder moment
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 10000;
            timer.Tick += (s, e) =>
            {
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE );
                if (!this.Focused && Visible) Hide();
            };
            timer.Start();

        }

        // fuck this shit man
        public static void StartOverlay(string modPath)
        {
            if (instance == null || instance.IsDisposed)
            {
                Thread overlayThread = new Thread(() =>
                {
                    Application.Run(instance = new TextureOverlayRenderer());
                })
                {
                    IsBackground = true
                };
                overlayThread.Start();
                while (instance == null || !instance.IsHandleCreated) Thread.Sleep(10);
            }
            instance.LoadTexture(modPath);
            instance.Show();
        }

        public void LoadTexture(string modPath)
        {
            if (modPath == null)
            {
                pictureBox.Image = null;
                currentModPath = null;
                return;
            }

            if (currentModPath != modPath)
            {
                currentModPath = modPath;
                string texturePath = Path.Combine(modPath, "Data", "seengm_enginepriview.png");
                MyLog.Default.WriteLine($"Attempting to load texture from: {texturePath}");
                if (File.Exists(texturePath))
                {
                    pictureBox.Image?.Dispose();
                    pictureBox.Image = new Bitmap(texturePath);
                    MyLog.Default.WriteLine($"Texture loaded successfully: {texturePath}");
                }
                else
                {
                    MyLog.Default.WriteLine($"Texture not found: {texturePath}");
                    pictureBox.Image = null;
                }
            }
        }

        public static void CloseOverlay()
        {
            if (instance != null && !instance.IsDisposed)
            {
                instance.Invoke((MethodInvoker)delegate
                {
                    instance.pictureBox.Image?.Dispose();
                    instance.Hide();
                });
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            pictureBox.Image?.Dispose();
        }
    }
}
﻿using AForge.Video;
using Newtonsoft.Json.Linq;
using PuzzleBot.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PuzzleBotUi
{
    public sealed class KeyWatcher
    {
        private readonly Key _key;
        private readonly bool _alt;
        private readonly Action _onDown;
        private readonly Action _onUp;

        private bool _state = false;

        public KeyWatcher(UIElement target, Key key, bool alt, Action onDown, Action onUp)
        {
            Contract.Assert(target != null);
            _key = key;
            _alt = alt;
            _onDown = onDown;
            _onUp = onUp;
            target.KeyDown += OnKeyDown;
            target.KeyUp += OnKeyUp;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_state == false) return;
            if (e.Key == _key) {
                _state = false;
                _onUp?.Invoke();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_state == true) return;
            if (e.Key == _key && (!_alt || Keyboard.IsKeyDown(Key.LeftAlt))) {
                _state = true;
                _onDown?.Invoke();
            }
        }
    }

    public sealed class Host
        : IHost
    {
        private readonly string _paramFile;
        private readonly Action<string, string> _writer;

        private JObject _settings;

        public Host(string paramFile, Action<string, string> writer)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(paramFile));
            Contract.Assert(writer != null);
            _paramFile = paramFile;
            _writer = writer;

            if (File.Exists(_paramFile))
                _settings = JObject.Parse(File.ReadAllText(_paramFile));
            else
                _settings = new JObject();
        }

        public T GetParam<T>(string name)
        {
            if (_settings[name] != null)
                return _settings[name].Value<T>();
            else
                return default(T);
        }

        public void SaveParam<T>(string name, T val)
        {
        }

        public void WriteLogMessage(string component, string msg)
        {
            _writer(component, msg);
        }
    }

    public partial class MainWindow : Window
    {
        private readonly Host _host;
        private readonly CncMachine _machine;
        
        public MainWindow()
        {
            InitializeComponent();


            MJPEGStream stream = new MJPEGStream("http://192.168.1.74:8081/?action=stream");
            stream.NewFrame += Stream_NewFrame;
            stream.Start();

            new KeyWatcher(this, Key.Up, false, () => _machine.StartJogTop(), () => _machine.StopJog());
            new KeyWatcher(this, Key.Down, false, () => _machine.StartJogDown(), () => _machine.StopJog());
            new KeyWatcher(this, Key.Left, false, () => _machine.StartJogLeft(), () => _machine.StopJog());
            new KeyWatcher(this, Key.Right, false, () => _machine.StartJogRight(), () => _machine.StopJog());
            new KeyWatcher(this, Key.U, false, () => _machine.StartJogUp(), () => _machine.StopJog());
            new KeyWatcher(this, Key.D, false, () => _machine.StartJogDown(), () => _machine.StopJog());
            new KeyWatcher(this, Key.L, false, () => _machine.StartJogCcw(), () => _machine.StopJog());
            new KeyWatcher(this, Key.R, false, () => _machine.StartJogCw(), () => _machine.StopJog());

            //new KeyWatcher(this, Key.Up, true, () => _machine.StartJogTop(), () => _machine.StopJog());
        }

        private void WriteLogMessage(string source, string msg)
        {
            consoleOutput.Items.Add($"[{DateTime.Now:HH:mm:ss.fff}][{source}]: {msg}");
            if (consoleOutput.Items.Count > 10000) consoleOutput.Items.RemoveAt(0);
            consoleOutput.ScrollIntoView(consoleOutput.Items.GetItemAt(consoleOutput.Items.Count - 1));
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            WriteLogMessage("KeyUp", "A key was released!");
            Debug.WriteLine("Up");
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("Down");
        }

        private void Stream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Dispatcher.Invoke(() => {
                cameraImage.Source = ToBitmapSource(eventArgs.Frame);
            });
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into a WPF <see cref="BitmapSource"/>.
        /// </summary>
        /// <remarks>Uses GDI to do the conversion. Hence the call to the marshalled DeleteObject.
        /// </remarks>
        /// <param name="source">The source bitmap.</param>
        /// <returns>A BitmapSource</returns>
        public static BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception) {
                bitSrc = null;
            }
            finally {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}
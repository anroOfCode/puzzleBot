using PuzzleBot.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PuzzleBot.Console
{
    using Newtonsoft.Json.Linq;
    using System.Diagnostics.Contracts;
    using System.IO;
    using Console = System.Console;

    internal sealed class Host
        : IHost
    {
        private readonly string _paramFile;
        private JObject _settings;
        private Dictionary<string, Action> _keyDelegates = new Dictionary<string, Action>();
        private Dictionary<char, bool> _keyDown = new Dictionary<char, bool>();

        public Host(string paramFile)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(paramFile));
            _paramFile = paramFile;

            if (File.Exists(_paramFile))
                _settings = JObject.Parse(File.ReadAllText(_paramFile));
            else
                _settings = new JObject();
        }

        public T GetParam<T>(string name)
        {
            if (_settings[name] != null) {
                if (typeof(T).IsEquivalentTo(typeof(JObject)))
                    return (T)(object)_settings[name];
                else
                    return _settings[name].ToObject<T>();
            }
            else {
                switch (name) {
                    case "MachineHostName": return (T)(object)"192.168.1.74";
                    case "MachinePort": return (T)(object)2000;
                    case "DownCameraPort": return (T)(object)8080;
                    case "UpCameraPort": return (T)(object)8081;

                    case "MaxX": return (T)(object)595.0;
                    case "MaxY": return (T)(object)360.0;
                    case "MaxZ": return (T)(object)30.0;
                    // 365 degrees max =)
                    case "MaxA": return (T)(object)365.0;
                    // 0.2mm nudge
                    case "NudgeX": return (T)(object)0.2;
                    case "NudgeY": return (T)(object)0.2;
                    case "NudgeZ": return (T)(object)0.2;
                    // 1 degree nudge
                    case "NudgeA": return (T)(object)1.0;
                    default: return default(T);
                }
            }
        }

        public void SaveParam<T>(string name, T val)
        {
            _settings[name] = JToken.FromObject(val);
            File.WriteAllText(_paramFile, _settings.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public void WriteLogMessage(string component, string msg)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}][{component}] - {msg}");
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public ICameraView CreateCameraView(string title)
        {
            var cv = new CameraView(title);
            cv.Form.KeyDown += CameraViewKeyDown;
            cv.Form.KeyUp += CameraViewKeyUp;
            return cv;
        }

        private void CameraViewKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            var str = GetKeyDelegateString((char)e.KeyCode, true, e.Alt);
            if (_keyDelegates.ContainsKey(str)) {
                _keyDelegates[str]();
            }

            if (_keyDown.ContainsKey((char)e.KeyCode)) {
                _keyDown[(char)e.KeyCode] = false;
            }
        }

        private void CameraViewKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            var str = GetKeyDelegateString((char)e.KeyCode, false, e.Alt);
            if (_keyDelegates.ContainsKey(str) && (!_keyDown.ContainsKey((char)e.KeyCode) || !_keyDown[(char)e.KeyCode])) {
                _keyDelegates[str]();
                _keyDown[(char)e.KeyCode] = true;
            }
        }

        public void SetKeyDelegate(char keyChar, bool up, bool alt, Action method)
        {
            _keyDelegates[GetKeyDelegateString(keyChar, up, alt)] = method;
        }

        private string GetKeyDelegateString(char keyChar, bool up, bool alt)
        {
            return $"{keyChar}_{up}_{alt}";
        }
    }
}

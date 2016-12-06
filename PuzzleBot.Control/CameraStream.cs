using AForge.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuzzleBotSerial
{
    public sealed class CameraStream
        : IDisposable
    {
        public enum StreamState
        {
            Starting,
            Streaming,
            Error,
            Stopped
        }

        public Bitmap LastImage;
        public double AverageFps;
        public StreamState State;

        private readonly MJPEGStream _stream;
        private const double _smoothingFactor = 0.9;

        private DateTime _lastFrame;

        public CameraStream(string server, int port)
        {
            // Assumes we're using MJPEG_streamer on the raspberry pi.
            _stream = new MJPEGStream($"http://{server}:{port}/?action=stream");
            _lastFrame = DateTime.Now;
            _stream.Start();

            _stream.VideoSourceError += VideoSourceError;
            _stream.NewFrame += NewFrame;
            _stream.Start();
            State = StreamState.Starting;
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            LastImage = eventArgs.Frame;
            State = StreamState.Streaming;
            var newFps = 1 / (DateTime.Now - _lastFrame).TotalSeconds;
            AverageFps = _smoothingFactor * AverageFps + (1 - _smoothingFactor) * newFps;
            _lastFrame = DateTime.Now;
        }

        private void VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            State = StreamState.Error;
        }

        public void Dispose()
        {
            _stream.Stop();
            _stream.WaitForStop();
            State = StreamState.Stopped;
            LastImage = null;
        }
    }
}

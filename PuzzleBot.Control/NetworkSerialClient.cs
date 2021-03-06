﻿// Copyright (c) 2016 Andrew Robinson. All rights reserved.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PuzzleBot.Control
{
    internal sealed class NetworkSerialClient 
        : IDisposable
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly Action<string> _onLine;
        private readonly Thread _readerThread;

        public NetworkSerialClient(string host, int port, Action<string> onLine)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(host));
            Contract.Assert(port > 0);
            Contract.Assert(onLine != null);
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8);
            _onLine = onLine;
            _readerThread = new Thread(ReadLoop);
            _readerThread.Start();
        }

        public void WriteLine(string msg)
        {
            Contract.Assert(!string.IsNullOrWhiteSpace(msg));
            _writer.Write(msg);
            _writer.Write("\n");
            _writer.Flush();
        }

        public void Dispose()
        {
            _client.Close();
        }

        private void ReadLoop()
        {
            while (true) {
                var line = _reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                _onLine(line);
            }
        }
    }
}

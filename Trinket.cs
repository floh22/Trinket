using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using floh22.Trinket.Windows;

namespace floh22.Trinket
{
    public class Trinket
    {
        private bool _run = true;
        private bool _forceExit = false;
        private Socket soc;

        private const string TargetProcessName = "League of Legends";
        private ProcessEventWatcher ProcessEventWatcher;
        private Process? _targetProcess;

        public bool UseProcessDetection = true;
        public bool Connected { get { return IsAlive(); } }
        public EventHandler<LiveEvent> OnLiveEvent;
        public EventHandler OnConnect, OnDisconnect, OnConnectionError;

        public Trinket(bool AutoConnect, bool UseProcessDetection)
        {
            this.UseProcessDetection = UseProcessDetection;
            if(UseProcessDetection)
                ProcessEventWatcher = new ProcessEventWatcher();

            OnConnectionError += (s, e) => soc.Dispose();

            if(AutoConnect)
            {
                Connect();
            }
        }

        public Trinket() : this(true, true) { }

        public void Connect()
        {
            if (UseProcessDetection)
            {
                StartWaitingForTargetProcess();
                return;
            }


            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                soc.Connect("127.0.0.1", 34243);
            }
            catch (SocketException)
            {
                OnConnectionError?.Invoke(this, EventArgs.Empty);
            }

            RunSocketConnection();
        }

        private void RunSocketConnection()
        {
            OnConnect?.Invoke(this, EventArgs.Empty);

            while (true)
            {
                if (!_run)
                {
                    if(!_forceExit)
                        _run = true;
                    else
                        _forceExit = false;

                    soc.Dispose();

                    OnDisconnect.Invoke(this, EventArgs.Empty); 
                    break;
                }
                if (soc.Available > 0)
                {
                    int size = soc.Available;
                    byte[] bytes = new byte[size];
                    soc.Receive(bytes, 0, size, SocketFlags.None);
                    string responseContent = Encoding.UTF8.GetString(bytes);
                    LiveEvent response = JsonSerializer.Deserialize<LiveEvent>(responseContent);
                    OnLiveEvent?.Invoke(this, response);
                }
            }
        }

        public void Disconnect()
        {
            _forceExit = true;
            _run = false;
        }

        private bool IsAlive()
        {
            return !((soc.Poll(1000, SelectMode.SelectRead) && (soc.Available == 0)) || !soc.Connected);
        }

        //Process Handling adapted from
        //https://github.com/Johannes-Schneider/GoldDiff/blob/master/GoldDiff/App.xaml.cs

        private void StartWaitingForTargetProcess()
        {
            ProcessEventWatcher.ProcessStarted += ProcessEventWatcher_OnProcessStarted;
            ProcessEventWatcher.ProcessStopped += ProcessEventWatcher_OnProcessStopped;

            var processes = Process.GetProcessesByName(TargetProcessName);
            if (processes.Length > 0)
            {
                _targetProcess = processes[0];
                TargetProcessStarted();
            }
        }

        private void ProcessEventWatcher_OnProcessStarted(object sender, ProcessEventEventArguments e)
        {
            if (_targetProcess != null)
            {
                return;
            }

            try
            {
                var newProcess = Process.GetProcessById(e.ProcessId);
                if (!newProcess.ProcessName.Equals(TargetProcessName))
                {
                    return;
                }

                _targetProcess = newProcess;
                TargetProcessStarted();
            }
            catch
            {
                // ignored
            }
        }

        private void ProcessEventWatcher_OnProcessStopped(object sender, ProcessEventEventArguments e)
        {
            if (_targetProcess == null || _targetProcess.Id != e.ProcessId)
            {
                return;
            }

            _targetProcess = null;
            TargetProcessStopped();
        }

        private void TargetProcessStarted()
        {
            soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _ = Task.Run(() => {
                while (!IsAlive())
                {
                    try
                    {
                        soc.Connect("127.0.0.1", 34243);
                    }
                    catch (SocketException)
                    {
                        if (!_run) {
                            OnConnectionError?.Invoke(this, EventArgs.Empty);
                            return;
                        }
                    }
                }
            });

            RunSocketConnection();
        }

        private void TargetProcessStopped()
        {
            _run = false;

        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Trinket()
        {
            Dispose(false);
        }

        private bool _isDisposed;

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (disposing)
            {
                ProcessEventWatcher.Dispose();
                soc.Dispose();
            }
        }

        #endregion
    }
}

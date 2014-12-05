﻿using Newtonsoft.Json;
using SensusService.Exceptions;
using SensusService.Probes.Location;
using SensusUI.UiProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Geolocation;

namespace SensusService
{
    /// <summary>
    /// Provides platform-independent service functionality.
    /// </summary>
    public abstract class SensusServiceHelper : INotifyPropertyChanged
    {
        #region static members
        private static SensusServiceHelper _singleton;

        private static string _protocolsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "protocols.json");
        private static string _previouslyRunningProtocolsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "previously_running_protocols.json");

        private static string _logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "sensus_log.txt");
        private static string _logTag = "SERVICE-HELPER";
        private static LoggingLevel _loggingLevel = LoggingLevel.Off;  // no logging allowed until the service helper has been set

        private static object _staticLockObject = new object();

        /// <summary>
        /// This is a shortcut accessor for the logging level of the service. It gets set when the singleton helper is set.
        /// </summary>
        public static LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
        }

        public static SensusServiceHelper Get()
        {
            // service helper be null for a brief period between the time when the app starts and when the service constructs the helper object.
            int triesLeft = 5;
            while (triesLeft-- > 0)
            {
                lock (_staticLockObject)
                    if (_singleton == null)
                        Thread.Sleep(1000);
                    else
                        break;
            }

            if (_singleton == null)
                throw new SensusException("Sensus failed to start service helper.");

            return _singleton;
        }

        public static void Set(SensusServiceHelper singleton)
        {
            lock (_staticLockObject)
            {
                _singleton = singleton;
                _loggingLevel = _singleton._logger.Level;
            }
        }
        #endregion

        /// <summary>
        /// Raised when a UI-relevant property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised when the service helper has stopped.
        /// </summary>
        public event EventHandler Stopped;

        private bool _stopped;
        private Logger _logger;
        private List<Protocol> _registeredProtocols;

        public List<Protocol> RegisteredProtocols
        {
            get
            {
                // registered protocols get deserialized on service startup. wait for them.
                int triesLeft = 5;
                while (triesLeft-- > 0)
                {
                    lock (this)
                        if (_registeredProtocols == null)
                        {
                            if (_loggingLevel >= LoggingLevel.Normal)
                                Log("Waiting for registered protocols to be deserialized.", _logTag);

                            Thread.Sleep(1000);
                        }
                        else
                            return _registeredProtocols;
                }

                throw new SensusException("Failed to get registered protocols.");
            }
        }

        [DisplayYesNoUiProperty("Charging:")]
        public abstract bool IsCharging { get; }

        [DisplayYesNoUiProperty("WiFi Connected:")]
        public abstract bool WiFiConnected { get; }

        public abstract string DeviceId { get; }

        protected SensusServiceHelper(Geolocator geolocator)
        {
            GpsReceiver.Get().Initialize(geolocator);

            _stopped = true;

#if DEBUG
            _logger = new Logger(_logPath, true, true, LoggingLevel.Debug, Console.Error);
#else
            _logger = new Logger(_logPath, true, true, LoggingLevel.Normal, Console.Error);
#endif

            if (_loggingLevel >= LoggingLevel.Normal)
                _logger.WriteLine("Log file started at \"" + _logPath + "\".", _logTag);
        }

        /// <summary>
        /// Starts platform-independent service functionality. Okay to call multiple times, even if the service is already running.
        /// </summary>
        public void Start()
        {
            lock (this)
                if (_stopped)
                    _stopped = false;
                else
                    return;

            _registeredProtocols = new List<Protocol>();

            try
            {
                using (StreamReader protocolsFile = new StreamReader(_protocolsPath))
                {
                    _registeredProtocols = JsonConvert.DeserializeObject<List<Protocol>>(protocolsFile.ReadToEnd(), new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        TypeNameHandling = TypeNameHandling.All,
                        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                    });

                    protocolsFile.Close();
                }
            }
            catch (Exception ex) { if (_loggingLevel >= LoggingLevel.Normal) Log("Failed to deserialize protocols:  " + ex.Message); }

            if (_loggingLevel >= LoggingLevel.Normal)
                Log("Deserialized " + _registeredProtocols.Count + " protocols.", _logTag);

            try
            {
                List<string> previouslyRunningProtocols = new List<string>();

                using (StreamReader previouslyRunningProtocolsFile = new StreamReader(_previouslyRunningProtocolsPath))
                {
                    previouslyRunningProtocols = JsonConvert.DeserializeObject<List<string>>(previouslyRunningProtocolsFile.ReadToEnd());
                    previouslyRunningProtocolsFile.Close();
                }

                foreach (Protocol protocol in _registeredProtocols)
                    if (!protocol.Running && previouslyRunningProtocols.Contains(protocol.Id))
                    {
                        if (_loggingLevel >= LoggingLevel.Normal)
                            Log("Starting previously running protocol:  " + protocol.Name, _logTag);

                        StartProtocolAsync(protocol);
                    }
            }
            catch (Exception ex) { if (_loggingLevel >= LoggingLevel.Normal) Log("Failed to deserialize ids for previously running protocols:  " + ex.Message, _logTag); }
        }

        public void RegisterProtocol(Protocol protocol)
        {
            lock (this)
                if (!_stopped)
                    if (!_registeredProtocols.Contains(protocol))
                        _registeredProtocols.Add(protocol);
        }

        public Task StartProtocolAsync(Protocol protocol)
        {
            lock (this)
                if (_stopped)
                    return null;
                else
                {
                    if (!_registeredProtocols.Contains(protocol))  // can't call RegisterProtocol here due to locking -- just repeat the code
                        _registeredProtocols.Add(protocol);

                    return protocol.StartAsync();
                }
        }

        public Task StopProtocolAsync(Protocol protocol, bool unregister)
        {
            lock (this)
                if (_stopped)
                    return null;
                else
                {
                    if (unregister)
                        _registeredProtocols.Remove(protocol);

                    return protocol.StopAsync();
                }
        }

        public Task StopAsync()
        {
            return Task.Run(async () =>
                {
                    // prevent any future interactions with the SensusServiceHelper
                    lock (this)
                        if (_stopped)
                            return;
                        else
                            _stopped = true;

                    if (_loggingLevel >= LoggingLevel.Normal)
                        Log("Stopping Sensus service.", _logTag);

                    List<string> runningProtocolIds = new List<string>();

                    foreach (Protocol protocol in _registeredProtocols)
                        if (protocol.Running)
                        {
                            runningProtocolIds.Add(protocol.Id);
                            await protocol.StopAsync();
                        }

                    try
                    {
                        using (StreamWriter protocolsFile = new StreamWriter(_protocolsPath))
                        {
                            protocolsFile.Write(JsonConvert.SerializeObject(_registeredProtocols, Formatting.Indented, new JsonSerializerSettings
                            {
                                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                                TypeNameHandling = TypeNameHandling.All,
                                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                            }));

                            protocolsFile.Close();
                        }
                    }
                    catch (Exception ex) { if (_loggingLevel >= LoggingLevel.Normal) Log("Failed to serialize protocols:  " + ex.Message, _logTag); }

                    _registeredProtocols = null;

                    try
                    {
                        using (StreamWriter previouslyRunningProtocolsFile = new StreamWriter(_previouslyRunningProtocolsPath))
                        {
                            previouslyRunningProtocolsFile.Write(JsonConvert.SerializeObject(runningProtocolIds, Formatting.Indented));
                            previouslyRunningProtocolsFile.Close();
                        }
                    }
                    catch (Exception ex) { if (_loggingLevel >= LoggingLevel.Normal) Log("Failed to serialize running protocol ID list:  " + ex.Message, _logTag); }

                    if (Stopped != null)
                        Stopped(null, null);
                });
        }

        public async void Destroy()
        {
            await StopAsync();

            _loggingLevel = LoggingLevel.Off;
            _logger.Close();
            _logger = null;
        }

        public void Log(string message, params string[] tags)
        {
            StringBuilder tagString = null;
            if (tags != null && tags.Length > 0)
            {
                tagString = new StringBuilder();
                foreach (string tag in tags)
                    if (!string.IsNullOrWhiteSpace(tag))
                        tagString.Append("[" + tag.ToUpper() + "]");
            }

            _logger.WriteLine((tagString == null || tagString.Length == 0 ? "" : tagString.ToString() + ":") + message);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
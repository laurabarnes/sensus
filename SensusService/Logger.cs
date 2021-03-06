﻿#region copyright
// Copyright 2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SensusService
{
    /// <summary>
    /// Logger for Sensus.
    /// </summary>
    public class Logger
    {
        public event EventHandler<string> MessageLogged;

        private string _path;
        private LoggingLevel _level;
        private TextWriter[] _otherOutputs;
        private StreamWriter _file;

        public LoggingLevel Level
        {
            get { return _level; }
        }

        public Logger(string path, LoggingLevel level, params TextWriter[] otherOutputs)
        {
            _path = path;
            _level = level;
            _otherOutputs = otherOutputs;

            InitializeFile(_path, true);
        }

        private void InitializeFile(string path, bool append)
        {
            _file = new StreamWriter(path, append);
            _file.AutoFlush = true;
        }

        public void Log(string message, LoggingLevel level, params string[] tags)
        {
            lock (this)
            {
                if (level > _level)
                    return;

                message = new Regex(@"\s\s+").Replace(message.Replace('\r', ' ').Replace('\n', ' ').Trim(), " ");
                if (string.IsNullOrWhiteSpace(message))
                    return;

                StringBuilder tagString = null;
                if (tags != null && tags.Length > 0)
                {
                    tagString = new StringBuilder();
                    foreach (string tag in tags)
                        if (!string.IsNullOrWhiteSpace(tag))
                            tagString.Append((tagString.Length == 0 ? "" : ",") + tag.ToUpper());
                }

                message = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " " + (tagString == null || tagString.Length == 0 ? "" : "[" + tagString + "]:  ") + message;

                try { _file.WriteLine(message); }
                catch (Exception) { }

                if (_otherOutputs != null)
                    foreach (TextWriter otherOutput in _otherOutputs)
                        otherOutput.WriteLine(message);

                if (MessageLogged != null)
                    MessageLogged(this, message);
            }
        }

        public List<string> Read(int mostRecentLines)
        {
            lock (this)
            {
                _file.Close();

                List<string> lines = File.ReadAllLines(_path).Reverse().ToList();

                if (mostRecentLines > lines.Count)
                    mostRecentLines = lines.Count;

                lines = lines.GetRange(0, mostRecentLines);

                InitializeFile(_path, true);

                return lines;
            }
        }

        public virtual void Clear()
        {
            lock (this)
            {
                _file.Close();
                InitializeFile(_path, false);
            }
        }

        public void Close()
        {
            lock (this)
                _file.Close();
        }
    }
}

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
 
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace SensusService.Probes.Communication
{
    public class SmsDatum : Datum
    {
        private string _fromNumber;
        private string _toNumber;
        private string _message;

        public string FromNumber
        {
            get { return _fromNumber; }
            set { _fromNumber = value == null ? "" : new Regex(@"[^0-9]").Replace(value, ""); }
        }

        public string ToNumber
        {
            get { return _toNumber; }
            set { _toNumber = value == null ? "" : new Regex(@"[^0-9]").Replace(value, ""); }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        [JsonIgnore]
        public override string DisplayDetail
        {
            get { return _message; }
        }

        public SmsDatum(Probe probe, DateTimeOffset timestamp, string fromNumber, string toNumber, string message)
            : base(probe, timestamp)
        {
            FromNumber = fromNumber;
            ToNumber = toNumber;
            _message = message;
        }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine +
                   "From:  " + _fromNumber + Environment.NewLine +
                   "To:  " + _toNumber + Environment.NewLine +
                   "Message:  " + _message;
        }
    }
}

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

namespace SensusService.Probes.Apps
{
    public class RunningAppsDatum : Datum
    {
        private string _name;
        private string _description;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [JsonIgnore]
        public override string DisplayDetail
        {
            get { return "Name:  " + _name + " (" + _description + ")"; }
        }

        public RunningAppsDatum(Probe probe, DateTimeOffset timestamp, string name, string description)
            : base(probe, timestamp)
        {
            _name = name;
            _description = description;
        }

        public override string ToString()
        {
            return base.ToString() + Environment.NewLine +
                   "Name:  " + _name + Environment.NewLine +
                   "Description:  " + _description;
        }
    }
}

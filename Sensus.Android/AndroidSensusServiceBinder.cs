#region copyright
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
 
using Android.OS;
using SensusService;

namespace Sensus.Android
{
    public class AndroidSensusServiceBinder : Binder
    {
        private AndroidSensusServiceHelper _sensusServiceHelper;

        public AndroidSensusServiceHelper SensusServiceHelper
        {
            get { return _sensusServiceHelper; }
            set { _sensusServiceHelper = value; }
        }

        public bool IsBound
        {
            get { return _sensusServiceHelper != null; }
        }

        public AndroidSensusServiceBinder(AndroidSensusServiceHelper sensusServiceHelper)
        {
            _sensusServiceHelper = sensusServiceHelper;
        }
    }
}
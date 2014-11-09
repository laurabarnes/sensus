﻿using Android.Content;
using Sensus.Probes;
using Sensus.Probes.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Geolocation;

namespace Sensus.Android.Probes
{
    /// <summary>
    /// Initializes protocols and their probes with platform-specific bindings.
    /// </summary>
    public class AndroidProbeInitializer : ProbeInitializer
    {
        private Context _context;

        public AndroidProbeInitializer(Context context)
        {
            _context = context;
        }

        protected override ProbeState Initialize(Probe probe)
        {
            if(base.Initialize(probe) != ProbeState.Initialized)
            {
                if(probe is GpsLocationProbe)
                {
                    Geolocator locator = new Geolocator(_context);
                    if (locator.IsGeolocationEnabled)
                    {
                        GpsLocationProbe gpsProbe = probe as GpsLocationProbe;
                        gpsProbe.Initialize(locator);
                    }
                }
            }

            return probe.State;
        }
    }
}
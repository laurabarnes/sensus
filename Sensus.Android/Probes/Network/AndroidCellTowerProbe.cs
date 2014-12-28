using Android.App;
using Android.Telephony;
using SensusService;
using SensusService.Probes.Network;
using System;

namespace Sensus.Android.Probes.Network
{
    public class AndroidCellTowerProbe : CellTowerProbe
    {
        private TelephonyManager _telephonyManager;
        private AndroidCellTowerChangeListener _cellTowerChangeListener;

        public AndroidCellTowerProbe()
        {
            _cellTowerChangeListener = new AndroidCellTowerChangeListener();
            _cellTowerChangeListener.CellTowerChanged += (o, cellTowerLocation) =>
                {
                    StoreDatum(new CellTowerDatum(this, DateTimeOffset.UtcNow, cellTowerLocation));
                };
        }

        protected override bool Initialize()
        {
            try
            {
                _telephonyManager = Application.Context.GetSystemService(global::Android.Content.Context.TelephonyService) as TelephonyManager;
                if (_telephonyManager == null)
                    throw new Exception("No telephony present.");

                return base.Initialize();
            }
            catch (Exception ex)
            {
                SensusServiceHelper.Get().Logger.Log("Failed to initialize " + GetType().FullName + ":  " + ex.Message, LoggingLevel.Normal);
                return false;
            }
        }

        public override void StartListening()
        {
            _telephonyManager.Listen(_cellTowerChangeListener, PhoneStateListenerFlags.CellLocation);
        }

        public override void StopListening()
        {
            _telephonyManager.Listen(_cellTowerChangeListener, PhoneStateListenerFlags.None);
        }
    }
}
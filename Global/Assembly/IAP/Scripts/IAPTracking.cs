
using System;

namespace BEH.IAP
{
	public class IAPTracking
	{
		public static Action<Data> onTrackIAP;

		public static void TrackIAP(Data data)
		{
			onTrackIAP?.Invoke(data);
        }

		public struct Data
		{
            public string ItemID;
            public string ItemType;
			public int Amount;
			public string Currency;
			public string CartType;
        }
    }

}

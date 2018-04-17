using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eleon.Modding;

namespace Tracker
{

    public class Request
    {
        public ushort SeqNrCounter = 1500;
        public Dictionary<Int32, Request> TrackRequest = new Dictionary<Int32, Request> { };

        public struct TrackData
        {
            //public Int32 trackingID;
            public ushort seqnr;
            public object anything;
            public Eleon.Modding.CmdId requestID;
        }
        public class snTracker
        {
            public static TrackData SeqNrTracker(Eleon.Modding.CmdId RequestID, ushort seqNr, object Anything)
            {
                TrackData NewData = new TrackData();
                //NewData.trackingID = TrackingID;
                NewData.seqnr = seqNr;
                NewData.anything = Anything;
                NewData.requestID = RequestID;
                return NewData;
            }
        }
    }
}

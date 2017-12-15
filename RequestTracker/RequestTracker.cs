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
            public Int32 empyrionID;
            public Eleon.Modding.CmdId requestID;
        }
        public static TrackData SeqNrTracker(Eleon.Modding.CmdId RequestID, ushort seqNr, Int32 EmpyrionID)
        {
            TrackData NewData = new TrackData();
            //NewData.trackingID = TrackingID;
            NewData.seqnr = seqNr;
            NewData.empyrionID = EmpyrionID;
            NewData.requestID = RequestID;
            return NewData;
        }
    }
}

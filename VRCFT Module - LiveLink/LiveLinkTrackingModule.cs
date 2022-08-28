using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ViveSR.anipal.Lip;
using VRCFaceTracking;
using VRCFaceTracking.Params;

namespace VRCFT_Module___LiveLink
{
    // This class contains the overrides for any VRCFT Tracking Data struct functions
    public static class TrackingData
    {
        // Map the EyeBlink and EyeSquint LiveLink blendshapes to the openness SRanipal blendshape
        private static float eyeCalc(float eyeBlink, float eyeSquint) => (float)Math.Pow(0.05 + eyeBlink, 6) + eyeSquint;

        // Map the JawOpen and MouthClose LiveLink blendshapes to the apeShape SRanipal blendshape
        private static float apeCalc(float jawOpen, float mouthClose) => (0.05f + jawOpen) * (float)Math.Pow(0.05 + mouthClose, 2);

        // Map the LiveLink module's single-eye data to the SRanipal API
        private static void Update(ref Eye data, LiveLinkTrackingDataEye external)
        {
            data.Look = new Vector2(external.EyeYaw, -1 * external.EyePitch);
            data.Openness = 1 - eyeCalc(external.EyeBlink, external.EyeSquint);
            data.Widen = external.EyeWide;
        }

        // Map the LiveLink module's lip tracking data to the SRanipal API
        private static void Update(ref LipTrackingData data,  LiveLinkTrackingDataLips external)
        {
            //if (!UnifiedLibManager.LipEnabled) return;

            Dictionary<LipShape_v2, float> lipShapes = new Dictionary<LipShape_v2, float>{
                    { LipShape_v2.JawRight, external.JawRight }, // +JawX
                    { LipShape_v2.JawLeft, external.JawLeft }, // -JawX
                    { LipShape_v2.JawForward, external.JawForward },
                    { LipShape_v2.JawOpen, external.JawOpen },
                    { LipShape_v2.MouthApeShape, apeCalc(external.JawOpen, external.MouthClose) },
                    { LipShape_v2.MouthUpperRight, external.MouthRight }, // +MouthUpper
                    { LipShape_v2.MouthUpperLeft, external.MouthLeft }, // -MouthUpper
                    { LipShape_v2.MouthLowerRight, external.MouthRight }, // +MouthLower
                    { LipShape_v2.MouthLowerLeft, external.MouthLeft }, // -MouthLower
                    { LipShape_v2.MouthUpperOverturn, external.MouthShrugUpper },
                    { LipShape_v2.MouthLowerOverturn, external.MouthShrugLower },
                    { LipShape_v2.MouthPout, (external.MouthFunnel + external.MouthPucker) / 2 },
                    { LipShape_v2.MouthSmileRight, external.MouthSmileRight }, // +SmileSadRight
                    { LipShape_v2.MouthSmileLeft, external.MouthSmileLeft }, // +SmileSadLeft
                    { LipShape_v2.MouthSadRight, external.MouthFrownRight }, // -SmileSadRight
                    { LipShape_v2.MouthSadLeft, external.MouthFrownLeft }, // -SmileSadLeft
                    { LipShape_v2.CheekPuffRight, external.CheekPuff },
                    { LipShape_v2.CheekPuffLeft, external.CheekPuff },
                    { LipShape_v2.CheekSuck, 0 },
                    { LipShape_v2.MouthUpperUpRight, external.MouthUpperUpRight },
                    { LipShape_v2.MouthUpperUpLeft, external.MouthUpperUpLeft },
                    { LipShape_v2.MouthLowerDownRight, external.MouthLowerDownRight },
                    { LipShape_v2.MouthLowerDownLeft, external.MouthLowerDownLeft },
                    { LipShape_v2.MouthUpperInside, external.MouthRollUpper },
                    { LipShape_v2.MouthLowerInside, external.MouthRollLower },
                    { LipShape_v2.MouthLowerOverlay, 0 },
                    { LipShape_v2.TongueLongStep1, external.TongueOut },
                    { LipShape_v2.TongueLeft, 0 }, // -TongueX
                    { LipShape_v2.TongueRight, 0 }, // +TongueX
                    { LipShape_v2.TongueUp, 0 }, // +TongueY
                    { LipShape_v2.TongueDown, 0 }, // -TongueY
                    { LipShape_v2.TongueRoll, 0 },
                    { LipShape_v2.TongueLongStep2, external.TongueOut },
                    { LipShape_v2.TongueUpRightMorph, 0 },
                    { LipShape_v2.TongueUpLeftMorph, 0 },
                    { LipShape_v2.TongueDownRightMorph, 0 },
                    { LipShape_v2.TongueDownLeftMorph, 0 },
                };

            for (int i = 0; i < SRanipal_Lip_v2.WeightingCount; i++)
            {
                data.LatestShapes[i] = lipShapes.Values.ElementAt(i);
            }
        }

        // Map the LiveLink module's eye data to the SRanipal API
        private static void Update(ref EyeTrackingData data, LiveLinkTrackingDataStruct external)
        {
            //if (!UnifiedLibManager.EyeEnabled) return;

            Update(ref data.Right, external.right_eye);
            Update(ref data.Left, external.left_eye);
            Update(ref data.Combined, external.getCombined());
        }

        // Map the LiveLink module's full data to the SRanipal API
        public static void Update(LiveLinkTrackingDataStruct external)
        {
            Update(ref UnifiedTrackingData.LatestEyeData, external);
            Update(ref UnifiedTrackingData.LatestLipData, external.lips);
        }
    }

    public class LiveLinkTrackingModule : ExtTrackingModule
    {
        public override (bool SupportsEye, bool SupportsLip) Supported => (true, true);

        private static CancellationTokenSource _cancellationToken;

        private UdpClient _liveLinkConnection;
        private IPEndPoint _liveLinkRemoteEndpoint;
        private LiveLinkTrackingDataStruct _latestData;

        // Starts listening and waits for the first packet to come in to initialize
        public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            Logger.Msg("Initializing Live Link Tracking module");
            
            _cancellationToken?.Cancel();
            UnifiedTrackingData.LatestEyeData.SupportsImage = false;
            UnifiedTrackingData.LatestLipData.SupportsImage = false;
            
            _liveLinkConnection = new UdpClient(Constants.Port);
            _liveLinkRemoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            _latestData = new LiveLinkTrackingDataStruct();
            
            ReadData(_liveLinkConnection, _liveLinkRemoteEndpoint, ref _latestData);
            return (true, true);
        }

        // Update the face pose every 10ms, this is the same frequency that Pimax and SRanipal use
        public override Action GetUpdateThreadFunc()
        {
            _cancellationToken = new CancellationTokenSource();
            return () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    Update();
                    Thread.Sleep(10);
                }
            };
        }

        // The update function needs to be defined separately in case the user is running with the --vrcft-nothread launch parameter
        // Currently doing all data processing in this Update function, should probably move into TrackingData
        public void Update()
        {
            ReadData(_liveLinkConnection, _liveLinkRemoteEndpoint, ref _latestData);
            TrackingData.Update(_latestData);   // Worse case we have bad data from LiveLink, we just update with the same values
        }

        // A chance to de-initialize everything. This runs synchronously inside main game thread. Do not touch any Unity objects here.
        public override void Teardown()
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            _liveLinkConnection.Close();
            Logger.Msg("LiveLink Teardown");
        }

        // Read the data from the LiveLink UDP stream and place it into a LiveLinkTrackingDataStruct
        private void ReadData(UdpClient liveLinkConnection, IPEndPoint liveLinkRemoteEndpoint, ref LiveLinkTrackingDataStruct trackingData)
        {
            Dictionary<string, float> values = new Dictionary<string, float>();

            try
            {
                // Grab the packet
                // TODO: This just blocks and waits to receive, are we sure this is the freshest packet?
                Byte[] recieveBytes = liveLinkConnection.Receive(ref liveLinkRemoteEndpoint);

                // There is a bunch of static data at the beginning of the packet, it may be variable length because it includes phone name
                // So grab the last 244 bytes of the packet sent using some Linq magic, since that's where our blendshapes live
                IEnumerable<Byte> trimmedBytes = recieveBytes.Skip(Math.Max(0, recieveBytes.Count() - 244));

                // More Linq magic, this splits our 244 bytes into 61, 4-byte chunks which we can then turn into floats
                List<List<Byte>> chunkedBytes = trimmedBytes
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / 4)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                // Process each float in out chunked out list
                foreach (var item in chunkedBytes.Select((value, i) => new { i, value }))
                {
                    // First, reverse the list because the data will be in big endian, then convert it to a float
                    item.value.Reverse();
                    values.Add(Constants.LiveLinkNames[item.i], BitConverter.ToSingle(item.value.ToArray(), 0));
                }
            }
            catch (Exception e)
            {
                Logger.Msg(e.ToString());
            }

            // Check that we got all 61 values before we go processing things
            if (values.Count() != 61)
            {
                return;
            }
            
            trackingData.ProcessData(values);
        }
    }
}

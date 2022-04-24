using System;
using System.Threading;
using VRCFaceTracking;
using VRCFaceTracking.Params;

namespace VRCFT_Module___Pimax_Eye_Tracking
{ 
    public class PimaxTrackingInterface : ITrackingModule
    {
        private static readonly Ai1EyeData PimaxEyeData = new Ai1EyeData();
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;

        public bool SupportsEye => true;
        public bool SupportsLip => false;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            return (success, false);
        }

        public Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    if (_needsUpdate)
                        Update();

                    Thread.Sleep(10);
                }
            };
        }

        public void Teardown()
        {
            CancellationToken.Cancel();
            PimaxTracker.Stop();
        }

        public void Update()
        {
            if (!UnifiedLibManager.EyeEnabled) return;
            
            PimaxEyeData.Update();

            UnifiedTrackingData.LatestEyeData.Left.Look = PimaxEyeData.Left.PupilCenter;
            UnifiedTrackingData.LatestEyeData.Right.Look = PimaxEyeData.Right.PupilCenter;
            UnifiedTrackingData.LatestEyeData.Combined.Look = PimaxEyeData.Recommended.PupilCenter;
            UnifiedTrackingData.LatestEyeData.Left.Openness = PimaxEyeData.Left.Openness;
            UnifiedTrackingData.LatestEyeData.Right.Openness = PimaxEyeData.Right.Openness;
            UnifiedTrackingData.LatestEyeData.Combined.Openness = PimaxEyeData.Recommended.Openness;
        }
    }
}
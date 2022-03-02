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
            //UnifiedTrackingData.LatestEyeData.UpdateData(PimaxEyeData);
            UnifiedTrackingData.LatestEyeData.Left.Look = new Vector2((PimaxEyeData.Left.PupilCenterX * 2 - 1)*2, (PimaxEyeData.Left.PupilCenterY * 2 - 1)*2);
            UnifiedTrackingData.LatestEyeData.Right.Look = new Vector2((PimaxEyeData.Right.PupilCenterX * 2 - 1)*2, (PimaxEyeData.Right.PupilCenterY * 2 - 1)*2);
            UnifiedTrackingData.LatestEyeData.Combined.Look = new Vector2((PimaxEyeData.Recommended.PupilCenterX * 2 - 1)*2, (PimaxEyeData.Recommended.PupilCenterY * 2 - 1)*2);
            UnifiedTrackingData.LatestEyeData.Left.Openness = 1f; //PimaxEyeData.Left.Openness * 0.01f;
            UnifiedTrackingData.LatestEyeData.Right.Openness = 1f; //PimaxEyeData.Right.Openness * 0.01f;
            UnifiedTrackingData.LatestEyeData.Combined.Openness = 1f; // PimaxEyeData.Recommended.Openness * 0.01f;
        }
    }
}
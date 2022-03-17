using System.Collections.Generic;
using System.Reflection;

namespace VRCFT_Module___LiveLink
{
    // Live Link Single-Eye tracking data
    public class LiveLinkTrackingDataEye
    {
        public float EyeBlink;
        public float EyeLookDown;
        public float EyeLookIn;
        public float EyeLookOut;
        public float EyeLookUp;
        public float EyeSquint;
        public float EyeWide;
        public float EyePitch;
        public float EyeYaw;
        public float EyeRoll;
    }

    // Live Link lip tracking data
    public class LiveLinkTrackingDataLips
    {
        public float JawForward;
        public float JawLeft;
        public float JawRight;
        public float JawOpen;
        public float MouthClose;
        public float MouthFunnel;
        public float MouthPucker;
        public float MouthLeft;
        public float MouthRight;
        public float MouthSmileLeft;
        public float MouthSmileRight;
        public float MouthFrownLeft;
        public float MouthFrownRight;
        public float MouthDimpleLeft;
        public float MouthDimpleRight;
        public float MouthStretchLeft;
        public float MouthStretchRight;
        public float MouthRollLower;
        public float MouthRollUpper;
        public float MouthShrugLower;
        public float MouthShrugUpper;
        public float MouthPressLeft;
        public float MouthPressRight;
        public float MouthLowerDownLeft;
        public float MouthLowerDownRight;
        public float MouthUpperUpLeft;
        public float MouthUpperUpRight;
        public float CheekPuff;
        public float CheekSquintLeft;
        public float CheekSquintRight;
        public float NoseSneerLeft;
        public float NoseSneerRight;
        public float TongueOut;
    }

    // Live Link brow tracking data
    public class LiveLinkTrackingDataBrow
    {
        public float BrowDownLeft;
        public float BrowDownRight;
        public float BrowInnerUp;
        public float BrowOuterUpLeft;
        public float BrowOuterUpRight;
    }

    // All Live Link tracking data
    public class LiveLinkTrackingDataStruct
    {
        public LiveLinkTrackingDataEye left_eye = new LiveLinkTrackingDataEye();
        public LiveLinkTrackingDataEye right_eye = new LiveLinkTrackingDataEye();
        public LiveLinkTrackingDataLips lips = new LiveLinkTrackingDataLips();
        public LiveLinkTrackingDataBrow brow = new LiveLinkTrackingDataBrow();

        public LiveLinkTrackingDataEye getCombined()
        {
            LiveLinkTrackingDataEye combined = new LiveLinkTrackingDataEye();
            foreach (var field in typeof(LiveLinkTrackingDataEye).GetFields(BindingFlags.Instance |
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.Public))
            {
                object temp = combined;
                field.SetValue(temp, ((float)field.GetValue(left_eye) + (float)field.GetValue(right_eye)) / 2);
                combined = (LiveLinkTrackingDataEye)temp;
            }
            return combined;
        }
        
        public void ProcessData(Dictionary<string, float> values)
        {
            // For each of the eye tracking blendshapes
            foreach (var field in typeof(LiveLinkTrackingDataEye).GetFields())
            {
                string leftName = field.Name + "Left";
                string rightName = field.Name + "Right";

                field.SetValue(left_eye, values[leftName]);
                field.SetValue(right_eye, values[rightName]);
            }

            // For each of the lip tracking blendshapes
            foreach (var field in typeof(LiveLinkTrackingDataLips).GetFields())
            {
                field.SetValue(lips, values[field.Name]);
            }

            // For each of the brow tracking blendshapes
            foreach (var field in typeof(LiveLinkTrackingDataBrow).GetFields())
            {
                field.SetValue(brow, values[field.Name]);
            }
        }
    }

    public static class Constants
    {
        // The proper names of each ARKit blendshape
        public static readonly string[] LiveLinkNames = {
            "EyeBlinkLeft",
            "EyeLookDownLeft",
            "EyeLookInLeft",
            "EyeLookOutLeft",
            "EyeLookUpLeft",
            "EyeSquintLeft",
            "EyeWideLeft",
            "EyeBlinkRight",
            "EyeLookDownRight",
            "EyeLookInRight",
            "EyeLookOutRight",
            "EyeLookUpRight",
            "EyeSquintRight",
            "EyeWideRight",
            "JawForward",
            "JawLeft",
            "JawRight",
            "JawOpen",
            "MouthClose",
            "MouthFunnel",
            "MouthPucker",
            "MouthLeft",
            "MouthRight",
            "MouthSmileLeft",
            "MouthSmileRight",
            "MouthFrownLeft",
            "MouthFrownRight",
            "MouthDimpleLeft",
            "MouthDimpleRight",
            "MouthStretchLeft",
            "MouthStretchRight",
            "MouthRollLower",
            "MouthRollUpper",
            "MouthShrugLower",
            "MouthShrugUpper",
            "MouthPressLeft",
            "MouthPressRight",
            "MouthLowerDownLeft",
            "MouthLowerDownRight",
            "MouthUpperUpLeft",
            "MouthUpperUpRight",
            "BrowDownLeft",
            "BrowDownRight",
            "BrowInnerUp",
            "BrowOuterUpLeft",
            "BrowOuterUpRight",
            "CheekPuff",
            "CheekSquintLeft",
            "CheekSquintRight",
            "NoseSneerLeft",
            "NoseSneerRight",
            "TongueOut",
            "HeadYaw",
            "HeadPitch",
            "HeadRoll",
            "EyeYawLeft", // LeftEyeYaw
            "EyePitchLeft", // LeftEyePitch
            "EyeRollLeft", // LeftEyeRoll
            "EyeYawRight", // RightEyeYaw
            "EyePitchRight", // RightEyePitch
            "EyeRollRight"}; // RightEyeRoll

        public static int Port = 11111;
    }
}

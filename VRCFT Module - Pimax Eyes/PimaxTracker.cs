using System.Runtime.InteropServices;
using System;
using VRCFaceTracking.Params;

namespace VRCFT_Module___Pimax_Eye_Tracking
{
	public class Ai1EyeData
	{
		public static float pupilLerp = 0.5f;
		public static float opennessLerp = 0.8f;
		public static float atanFactor = 0.72811958756f; // Magic number, 0.72811958756 * atan(5 * 2) ~= 1
		public static float opennessFactor = 5f;

		public EyeExpressionState Left;
		public EyeExpressionState Right;
		public EyeExpressionState Recommended;

		EyeExpressionState PrevLeft;
		EyeExpressionState PrevRight;
		EyeExpressionState PrevRecommended;

		public void Update()
		{
			EyeExpressionState TempLeft = GetEyeExpressionState(Eye.Left);
			EyeExpressionState TempRight = GetEyeExpressionState(Eye.Right);
			EyeExpressionState TempRecommended = GetEyeExpressionState(PimaxTracker.GetRecommendedEye());

			/* Credit to azmidi for Linear Interpolation */
			PrevLeft.PupilCenter = Lerp(PrevLeft.PupilCenter, TempLeft.PupilCenter, pupilLerp);
			PrevLeft.Openness = Lerp(PrevLeft.Openness, TempLeft.Openness, opennessLerp);
			
			PrevRight.PupilCenter = Lerp(PrevRight.PupilCenter, TempRight.PupilCenter, pupilLerp);
			PrevRight.Openness = Lerp(PrevLeft.Openness, TempRight.Openness, opennessLerp);

			PrevRecommended.PupilCenter = Lerp(PrevRecommended.PupilCenter, TempRecommended.PupilCenter, pupilLerp);
			PrevRecommended.Openness = Lerp(PrevRecommended.Openness, TempRecommended.Openness, opennessLerp);

			Left = FilterEye(PrevLeft);
			Right = FilterEye(PrevRight);
			Recommended = FilterEye(PrevRecommended);

            //Console.WriteLine("X: " + TempLeft.PupilCenter.x + " -> " + PrevLeft.PupilCenter.x + " -> " + Left.PupilCenter.x + " Y: " + TempLeft.PupilCenter.y + " -> " + PrevLeft.PupilCenter.y + " -> " + Left.PupilCenter.y);
		}

		// Test Filtering Helper Method
		/* Credit to azmidi for arctangent filtering */
		private static EyeExpressionState FilterEye(EyeExpressionState state)
		{
			// I don't using arctan on the pupil center makes much sense
			//state.PupilCenterX = (float)(atanFactor * Math.Atan(state.PupilCenterX));
			//state.PupilCenterY = (float)(atanFactor * Math.Atan(state.PupilCenterY));
			//state.PupilCenter = new Vector2((float) (atanFactor * Math.Atan(state.PupilCenter.x)),
			//								(float) (atanFactor * Math.Atan(state.PupilCenter.y)));
			state.Openness = (float)(atanFactor * Math.Atan(opennessFactor * state.Openness));

			return state;
		}

		private static Vector2 Lerp(Vector2 start, Vector2 end, float lerpPercent)
        {
			return new Vector2(start.x + (end.x - start.x) * lerpPercent,
								start.y + (end.y - start.y) * lerpPercent);
        }

		private static float Lerp(float start, float end, float lerpPercent)
		{
			return start + (end - start) * lerpPercent;
		}

		private EyeExpressionState GetEyeExpressionState(Eye eye)
		{
			EyeExpressionState state;
			state.PupilCenter = new Vector2(PimaxTracker.GetEyeExpression(eye, EyeExpression.PupilCenterX),
											PimaxTracker.GetEyeExpression(eye, EyeExpression.PupilCenterY));
			state.Openness = PimaxTracker.GetEyeExpression(eye, EyeExpression.Openness);

			return state;
		}
	}
	
	
	// Mad props to NGenesis for these bindings <3

	public enum Eye {
		Any,
		Left,
		Right
	}

	public enum CallbackType {
		Start,
		Stop,
		Update
	}

	public enum EyeExpression
	{
		PupilCenterX, // Pupil center on the X axis, smoothed and normalized between -1 (looking left) ... 0 (looking forward) ... 1 (looking right)
		PupilCenterY, // Pupil center on the Y axis, smoothed and normalized between -1 (looking up) ... 0 (looking forward) ... 1 (looking down)
		Openness, // How open the eye is, smoothed and normalized between 0 (fully closed) ... 1 (fully open)
		Blink // Blink, 0 (not blinking) or 1 (blinking)
	}

	public struct EyeExpressionState
	{
		public Vector2 PupilCenter;
		public float Openness;
	};

	public delegate void EyeTrackerEventHandler();
	
    public static class PimaxTracker
    {
	    /// <summary>
	    /// Registers callbacks for the tracker to notify when it's finished initializing, when it has new data available and when the module is stopped.
	    /// </summary>
        [DllImport("PimaxEyeTracker", EntryPoint = "RegisterCallback")] public static extern void RegisterCallback(CallbackType type, EyeTrackerEventHandler callback);
	    
	    /// <summary>
	    /// Initializes the module.
	    /// </summary>
	    /// <returns>Initialization Successful</returns>
		[DllImport("PimaxEyeTracker", EntryPoint = "Start")] public static extern bool Start();
	    
	    /// <summary>
	    /// Stops the eye tracking module and disconnects the server
	    /// </summary>
		[DllImport("PimaxEyeTracker", EntryPoint = "Stop")] public static extern void Stop();
	    //[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeData")] public static extern EyeExpressionState GetEyeData(Eye eye);
		[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeExpression")] public static extern float GetEyeExpression(Eye eye, EyeExpression expression);
		[DllImport("PimaxEyeTracker", EntryPoint = "GetRecommendedEye")] public static extern Eye GetRecommendedEye();

	}
}
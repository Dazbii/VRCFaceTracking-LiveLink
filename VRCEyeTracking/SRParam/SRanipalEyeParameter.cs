﻿using System;
using UnityEngine;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCEyeTracking.ParamLib;

namespace VRCEyeTracking.SRParam
{
    public class SRanipalGeneralEyeParameter : FloatBaseParam, ISRanipalParam
    {
        private readonly Func<EyeData_v2, float> _getSRanipalParam;
        
        public SRanipalGeneralEyeParameter(Func<EyeData_v2, float> getValueFunc, string paramName, bool prioritised = false) 
            : base(paramName, prioritised) => _getSRanipalParam = getValueFunc;
        
        public void RefreshParam(EyeData_v2? eyeData, LipData_v2? lipData) => ParamValue = _getSRanipalParam.Invoke(eyeData.Value);
        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ParamIndex = null;
    }

    public class SRanipalXYEyeParameter : XYParam, ISRanipalParam
    {
        private readonly Func<EyeData_v2, Vector2> _getSRanipalParam;

        public SRanipalXYEyeParameter(Func<EyeData_v2, Vector2> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
            => _getSRanipalParam = getValueFunc;

        public void RefreshParam(EyeData_v2? eyeData, LipData_v2? lipData) => ParamValue = _getSRanipalParam.Invoke(eyeData.Value);
        void ISRanipalParam.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
    }
}
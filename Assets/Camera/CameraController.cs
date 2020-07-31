using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform startTarget;
    public Transform Target { get; private set; }

    [ReadOnly]
    [SerializeField]
    private CameraSettings currentSettings;
    public CameraSettings CurrentSettings
    {
        get { return currentSettings; }
        private set { currentSettings = value; }
    }


    public CameraSettings DefaultSettings;
    [SerializeField]
    private CameraSettings _startSettings;

    private float positionSmoothness;
    public float PositionSmoothness
    {
        get
        {
            if (isAnimatingPositionSmoothness)
                return positionSmoothness;
            else return CurrentSettings.PositionSmoothness;
        }
    }

    private float orientationSmoothness;
    public float OrientationSmoothness
    {
        get
        {
            if (isAnimatingOrientationSmoothness)
                return orientationSmoothness;
            else return CurrentSettings.OrientationSmoothness;
        }
    }


    private Vector3 _initPosition;
    private Vector3 _startOffset;
    private Vector3 _desiredPosition;
    private Quaternion _desiredRotation;
    private Vector3 _previousDesiredPosition;
    private Vector3 screenRatioDeltaOffset;

    private Vector3 _velocity;

    private bool isAnimatingPositionSmoothness;
    private bool isAnimatingOrientationSmoothness;


    private void Start()
    {
        ChangeSettings(_startSettings);
        _initPosition = transform.position;


        if (startTarget)
        {
            SetTarget(startTarget);

            CalculatePosition(CurrentSettings, Target);
            ForceToDesiredPosition();

            CalculateOrientation(CurrentSettings, Target);
            ForceToDesiredOrientation();
        }

    }
    
    private void Update()
    {
        if (CurrentSettings.UpdateMode == CameraSettings.CameraUpdateMode.Update)
        {
            if (Target)
            {
                FollowTarget(Target, Time.deltaTime);
            }
        }
    }
    private void FixedUpdate()
    {
        if (CurrentSettings.UpdateMode == CameraSettings.CameraUpdateMode.FixedUpdate)
        {
            if (Target)
            {
                FollowTarget(Target, Time.fixedDeltaTime);
            }
        }
    }
    private void LateUpdate()
    {
        if (CurrentSettings.UpdateMode == CameraSettings.CameraUpdateMode.LateUpdate)
        {
            if (Target)
            {
                FollowTarget(Target, Time.deltaTime);
            }
        }

    }

    private void FollowTarget(Transform target, float deltaTime)
    {
        CalculatePosition(CurrentSettings, target);

        if (CurrentSettings.UpdateOrientation)
            CalculateOrientation(CurrentSettings, target);

        // Apply
        transform.position = Vector3.SmoothDamp(transform.position, _desiredPosition, ref _velocity, PositionSmoothness);
        _previousDesiredPosition = _desiredPosition;

        if(CurrentSettings.UseOrientation && CurrentSettings.UpdateOrientation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _desiredRotation, OrientationSmoothness);
        }

    }

    #region CONTROLLER

    public void ChangeSettings(CameraSettings settings)
    {
        //Debug.Log("Change settings to " + settings);
        CurrentSettings = settings;

        screenRatioDeltaOffset = Vector3.zero;
        if (CurrentSettings.AddCorrectionFromScreenRatioDelta)
            screenRatioDeltaOffset = GetDeltaFromScreenRatio();

        if (CurrentSettings.UseOrientation && !CurrentSettings.UpdateOrientation)
            ForceToDesiredOrientation();

    }
    public void ResetToDefaultSettings()
    {
        ChangeSettings(DefaultSettings);
    }
    public void FreezeCamera()
    {
        Target = null;
    }

    public void SetTarget(Transform target)
    {
        //Debug.Log("Change camera target to " + target);
        Target = target;
    }

    #endregion


    #region POSITION

    private void CalculatePosition(CameraSettings settings, Transform target)
    {
        if (settings.UseLocalTargetMode)
        {
            if(settings.CoordSystem == CameraSettings.CoordinatesSystem.Spheric)
            {
                Vector3 localOffset = -(settings.sphericOffset.x * target.right + settings.sphericOffset.y * target.up + settings.sphericOffset.z * target.forward);
                localOffset += Quaternion.AngleAxis(settings.azimut, target.right) * Quaternion.AngleAxis(settings.polar, target.up) * target.forward * settings.radius;
                localOffset += screenRatioDeltaOffset;
                _desiredPosition = target.position - localOffset;
            }
            else
            {
                Vector3 localOffset = settings.Offset.x * target.right + settings.Offset.y * target.up + settings.Offset.z * target.forward;
                localOffset += screenRatioDeltaOffset;
                _desiredPosition = target.position + localOffset;
            }
            
        }
        else
        {
            if (settings.CoordSystem == CameraSettings.CoordinatesSystem.Spheric)
            {
                Vector3 offset = -(settings.sphericOffset.x * settings.RightAxis + settings.sphericOffset.y * settings.UpAxis + settings.sphericOffset.z * settings.FwdAxis);
                offset += Quaternion.AngleAxis(settings.azimut, settings.RightAxis) * Quaternion.AngleAxis(settings.polar, settings.UpAxis) * settings.FwdAxis * settings.radius;
                offset += screenRatioDeltaOffset;
                _desiredPosition = target.position - offset;
            }
            else
            {
                Vector3 offset = settings.Offset.x * settings.RightAxis + settings.Offset.y * settings.UpAxis + settings.Offset.z * settings.FwdAxis;
                offset += screenRatioDeltaOffset;
                
                _desiredPosition = target.position + offset;
            }

            // options
            if (settings.LockXAxis)
            {
                _desiredPosition.x = _initPosition.x;
            }
            if (settings.LockYAxis)
            {
                _desiredPosition.y = _initPosition.y;
            }
            if (settings.LockZAxis)
            {
                _desiredPosition.z = _initPosition.z;
            }

            ClampPosition(settings);



        }
    }

    private void ClampPosition(CameraSettings settings)
    {
        if (settings.useXLimits)
            _desiredPosition.x = Mathf.Clamp(_desiredPosition.x, settings.XLimits.x, settings.XLimits.y);
        if (settings.useYLimits)
            _desiredPosition.y = Mathf.Clamp(_desiredPosition.y, settings.YLimits.x, settings.YLimits.y);
        if (settings.useZLimits)
            _desiredPosition.z = Mathf.Clamp(_desiredPosition.z, settings.ZLimits.x, settings.ZLimits.y);

    }

    public void ForceToDesiredPosition()
    {
        CalculatePosition(CurrentSettings, Target);
        transform.position = _desiredPosition;
    }
    public void ForceToDesiredPosition(Vector3 pos)
    {
        transform.position = _desiredPosition = pos;
    }

    #endregion

    #region ORIENTATION

    private void CalculateOrientation(CameraSettings settings, Transform target)
    {
        if (CurrentSettings.UseLocalTargetMode)
        {
            Vector3 fw = Quaternion.AngleAxis(settings.Azimut, target.right) * Quaternion.AngleAxis(settings.Polar, target.up) * target.forward;
            _desiredRotation = Quaternion.LookRotation(fw, settings.UpAxis);
        }
        else
        {
            Vector3 fw = Quaternion.AngleAxis(settings.Azimut, settings.RightAxis) * Quaternion.AngleAxis(settings.Polar, settings.UpAxis) * settings.FwdAxis;
            _desiredRotation = Quaternion.LookRotation(fw, settings.UpAxis);
        }
        
    }

    public void ForceToDesiredOrientation()
    {
        CalculateOrientation(CurrentSettings, Target);
        transform.rotation = _desiredRotation;
    }

    #endregion


    #region SCREEN RATIO ADAPTER

    private void FitScreenRatio()
    {
        //Debug.Log("hdFOV = " + HDHorizontalFOV + ", fov = " + hFOV);

        CurrentSettings.Offset = CurrentSettings.Offset + GetDeltaFromScreenRatio();
        //CurrentSettings.YLimits = HDbaseSettings.YLimits + deltaDistance * Mathf.Tan(cam.fieldOfView/2 * Mathf.Deg2Rad) * new Vector2(1,-1);
        //CurrentSettings.XLimits = HDbaseSettings.XLimits + deltaDistance * Mathf.Tan(hFOV / 2 * Mathf.Deg2Rad) * new Vector2(1, -1);

    }

    private Vector3 GetDeltaFromScreenRatio()
    {
        Camera cam = GetComponent<Camera>();

        // fov is vertical angle
        float vFOV = cam.fieldOfView;
        float hFOV = GetHorizontalFOV(cam.fieldOfView, cam.aspect);

        Vector3 targetToCam = CurrentSettings.Offset;
        float d = targetToCam.magnitude;
        float deltaDistance = d * (Mathf.Tan(HDHorizontalFOV / 2 * Mathf.Deg2Rad) / Mathf.Tan(hFOV / 2 * Mathf.Deg2Rad) - 1);

        return targetToCam.normalized * deltaDistance;
    }

    private float GetHorizontalFOV(float camfov, float camAspect)
    {
        // because hfov is not directly availble
        float radAngle = camfov * Mathf.Deg2Rad;
        float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * camAspect);
        return  Mathf.Rad2Deg * radHFOV;
    }

    private float HDHorizontalFOV
    {
        get
        {
            return GetHorizontalFOV(GetComponent<Camera>().fieldOfView, 9f / 16f);
        }
    }

    #endregion


    #region UTILITY

    public IEnumerator AnimPositionSmoothing(float start, float end, float duration)
    {
        isAnimatingPositionSmoothness = true;
        for (float f = 0; f < duration; f += Time.deltaTime)
        {
            positionSmoothness = Mathf.Lerp(start, end, f / duration);
            yield return null;
        }
        positionSmoothness = end;
        isAnimatingPositionSmoothness = false;
    }
    public IEnumerator AnimPositionSmoothing(float end, float duration)
    {
        yield return AnimPositionSmoothing(CurrentSettings.PositionSmoothness, end, duration);
    }


    public IEnumerator AnimOrientationSmoothing(float start, float end, float duration)
    {
        isAnimatingOrientationSmoothness = true;
        for (float f = 0; f < duration; f += Time.deltaTime)
        {
            orientationSmoothness = Mathf.Lerp(start, end, f / duration);
            yield return null;
        }
        orientationSmoothness = end;
        isAnimatingOrientationSmoothness = false;
    }
    public IEnumerator AnimOrientationSmoothing(float end, float duration)
    {
        yield return AnimPositionSmoothing(CurrentSettings.OrientationSmoothness, end, duration);
    }



    public bool enableCamRotationAround { get; set; }
    public IEnumerator CloseUpAndRotateCameraAround(CameraSettings settings, float closeupDuration, float angularSpeed, AnimationCurve distanceCurve)
    {
        enableCamRotationAround = true;
        Vector3 center = Target.position;
        Vector3 toward = transform.position - center;
        float startDistance = toward.magnitude;
        float distance = startDistance;
        float timer = 0;

        FreezeCamera();


        while (enableCamRotationAround)
        {
            if (timer <= closeupDuration)
            {
                distance = Mathf.Lerp(startDistance, settings.radius, distanceCurve.Evaluate(timer / closeupDuration));
            }

            toward = Quaternion.AngleAxis(Time.deltaTime * angularSpeed, Vector3.up) * toward.normalized * distance;
            transform.position = center + toward;

            Quaternion rot = Quaternion.LookRotation(-toward, Vector3.up);
            transform.rotation = rot;

            timer += Time.deltaTime;
            yield return null;
        }
    }





    #endregion
}

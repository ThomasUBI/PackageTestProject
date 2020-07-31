using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "camSettings", menuName = "Camera/Settings")]
public class CameraSettings : ScriptableObject
{
    public enum CameraUpdateMode { Update, FixedUpdate, LateUpdate }
    public CameraUpdateMode UpdateMode;

    [SerializeField]
    private bool autoCalculateRightAxis;

    [Tooltip("Because of the screen ratio difference, the offset/distance of the camera may be adjusted")]
    public bool AddCorrectionFromScreenRatioDelta;

    [Tooltip("If true all parameters will be calculated in the local space of the target (for axis)")]
    public bool UseLocalTargetMode;

    #region AXIS

    [Title("Axis")]
    [ToggleGroup("UseCustomAxis")]
    public bool UseCustomAxis;
    
    [ToggleGroup("UseCustomAxis")]
    [SerializeField]
    private Vector3 right;
    [ToggleGroup("UseCustomAxis")]
    [SerializeField]
    private Vector3 up;
    [ToggleGroup("UseCustomAxis")]
    [SerializeField]
    private Vector3 fwd;

    public void SetCustomAxis(Vector3 right, Vector3 up, Vector3 fwd)
    {
        this.right =right;
        this.up = up;
        this.fwd = fwd;
    }

    public Vector3 UpAxis
    {
        get
        {
            if (UseCustomAxis)
                return up;
            else return Vector3.up;
        }
    }
    public Vector3 RightAxis
    {
        get
        {
            if (autoCalculateRightAxis)
                return Vector3.Cross(UpAxis, FwdAxis);
            else if (UseCustomAxis)
                return right;
            else return Vector3.right;
        }
    }
    public Vector3 FwdAxis
    {
        get
        {
            if (UseCustomAxis)
                return fwd;
            else return Vector3.forward;
        }
    }

    #endregion

    [Title("Smoothing")]
    public float PositionSmoothness = 0.3f;
    public float OrientationSmoothness = 0.1f;

    #region POSITION

    [Title("Position")]
    public enum CoordinatesSystem { Cartesian, Spheric }
    public CoordinatesSystem CoordSystem;
    private bool isCartesian { get { return CoordSystem == CoordinatesSystem.Cartesian; } }

    [ShowIf("isCartesian")]
    [SerializeField]
    private Vector3 offset = new Vector3(0,0,-10);

    public Vector3 Offset
    {
        get
        {
            return offset;
        }
        set
        {
            offset = value;
        }
    }

    [CreateSlider("position distance", 1, 50)]
    [HideIf("isCartesian")]
    public float radius = 10;
    [CreateSlider("position azimut", 0, 90)]
    [HideIf("isCartesian")]
    public float azimut = 45;
    [CreateSlider("position polar", -90, 90)]
    [HideIf("isCartesian")]
    public float polar;
    [HideIf("isCartesian")]
    public Vector3 sphericOffset;

    #endregion

    #region ORIENTATION

    [Title("Orientation")]
    public bool UseOrientation;
    public bool UpdateOrientation;
    [HideIf("isCartesian")]
    public bool UsePositionAngles = true;

    private bool usePositionAngle { get { return UsePositionAngles && !isCartesian; } }

    [DisableIf("usePositionAngle")]
    [CreateSlider("orientation azimut", 0, 90)]
    [SerializeField]
    private float orientationAzimut = 45;

    [CreateSlider("orientation polar", -90, 90)]
    [DisableIf("usePositionAngle")]
    [SerializeField]
    private float orientationPolar;

    public float Azimut
    {
        get
        {
            if (usePositionAngle)
                return azimut;
            else return orientationAzimut;
        }
    }
    public float Polar
    {
        get
        {
            if (usePositionAngle)
                return polar;
            else return orientationPolar;
        }
    }
    

    #endregion

    [Header("Limits")]
    public bool LockXAxis;
    public bool useXLimits;
    public Vector2 XLimits;
    public bool LockYAxis;
    public bool useYLimits;
    public Vector2 YLimits;
    public bool LockZAxis;
    public bool useZLimits;
    public Vector2 ZLimits;

}

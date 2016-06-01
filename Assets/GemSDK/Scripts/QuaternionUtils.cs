using UnityEngine;
using System;

namespace GemSDK.QuaternionUtils
{
    #region enum

    /// <summary>
    /// Represents coordinate space
    /// </summary>
    public enum CoordSpace { Body, World };

    #endregion

    #region converters
    /// <summary>
    /// Utility class to convert rotation quaternion to azimuth-elevation representation
    /// </summary>
    public static class AzimuthElevationConverter
    {  
        /// <summary>
        /// Converts the rotation quaternion to azimuth-elevation representation 
        /// </summary>
        /// <returns>The azimuth-elevation representation of the quaternion, {azimuth, elevation} in degrees</returns>
        /// <param name="quat">The quaternion to convert</param>
        /// <param name="coordSpace">Use <c>CoordSpace.Body</c> to convert regarding to Device axes or 
        /// <c>CoordSpace.World</c> to convert regarding to world axes</param>
        public static Vector2 Convert(Quaternion quat, CoordSpace coordspace = CoordSpace.Body)
        {
            float[] ae = Convert(new float[] {quat.w, quat.x, quat.y, quat.z}, coordspace);
            return new Vector2(ae[0], ae[1]);
        }

        private static float[] Convert(float[] quat, CoordSpace coordSpace)
        {
            float[] res = new float[2];

            if (coordSpace == CoordSpace.World)
                quat = QuatUtils.Inverse(quat);

            float[] forward = QuatUtils.TransformVecLH(quat, new float[] { 0f, 0f, 1f });

            float lXZ = (float)Math.Sqrt(forward[0] * forward[0] + forward[2] * forward[2]);

            //Azimuth
            if (lXZ < 0.00001f)
                res[0] = (float)Math.Sign(forward[0]) * (float)Math.PI;
            else
                res[0] = (float)Math.Asin((double)forward[0] / (double)lXZ);

            if (forward[2] < 0)
                res[0] = (float)Math.Sign(res[0]) * (float)Math.PI - res[0];

            //Elevation
            if (lXZ < 0.00001f)
                res[0] = (float)(Math.PI * Math.Sign(forward[1]));
            else
                res[1] = (float)Math.Atan(forward[1] / lXZ);

            if (coordSpace == CoordSpace.World)
            {
                res[0] = -res[0];
                res[0] = -res[1];
            }

            //Convert radians to degrees
            res[0] *= Mathf.Rad2Deg;
            res[1] *= Mathf.Rad2Deg;

            return res;
        }
    }

    /// <summary>
    /// Utility class to convert quaternion to tilt-elevation representation
    /// </summary>
    public static class TiltElevationConverter
    {
        /// <summary>
        /// Converts the rotation quaternion to tilt-elevation representation
        /// </summary>
        /// <returns>The tilt-elevation representation of the quaternion, {tilt, elevation}</returns>
        /// <param name="quat">The quaternion to convert</param>
        /// <param name="coordSpace">Use <c>CoordSpace.Body</c> to convert regarding to Device axes or 
        /// <c>CoordSpace.World</c> to convert regarding to world axes</param> 
        public static Vector2 Convert(Quaternion quat, CoordSpace coordspace = CoordSpace.Body)
        {
            float[] te = Convert(new float[] { quat.w, quat.x, quat.y, quat.z }, coordspace);
            return new Vector2(te[0], te[1]);
        }

        private static float[] Convert(float[] quat, CoordSpace coordSpace)
        {
            float[] res = new float[2];

            if (coordSpace == CoordSpace.World)
                quat = QuatUtils.Inverse(quat);

            float[] forward = QuatUtils.TransformVecLH(quat, new float[] { 0f, 0f, 1f });
            float[] right = QuatUtils.TransformVecLH(quat, new float[] { 1f, 0f, 0f });

            float lf_XZ = (float)Math.Sqrt(forward[0] * forward[0] + forward[2] * forward[2]);
            float lr_XZ = (float)Math.Sqrt(right[0] * right[0] + right[2] * right[2]);

            //Tilt
            if (lr_XZ < 0.00001f)
                res[0] = -(float)(Math.PI * Math.Sign(right[1]));
            else
                res[0] = -(float)Math.Atan(right[1] / lr_XZ);

            //Elevation
            if (lf_XZ < 0.00001f)
                res[0] = (float)(Math.PI * Math.Sign(forward[1]));
            else
                res[1] = (float)Math.Atan(forward[1] / lf_XZ);

            if (coordSpace == CoordSpace.World)
            {
                res[0] = -res[0];
                res[0] = -res[1];
            }

            //Convert radians to degrees
            res[0] *= Mathf.Rad2Deg;
            res[1] *= Mathf.Rad2Deg;

            return res;
        }
    }

    #endregion

    #region controllers

    /// <summary>
    /// Axis settings.
    /// </summary>
    public class AxisSettings
    {
        /// <summary>
        /// Axis sensitivity
        /// </summary>
        public float mult = 1f;
        /// <summary>
        ///	Minimum angle limit 
        /// </summary>
        public float min = 0f;
        /// <summary>
        /// Maximum angle limit 
        /// </summary>
        public float max = 0f;

        /// <summary>
        /// Sets all axis settings
        /// </summary>
        /// <param name="sensitivity">Axis sensitivity</param>
        /// <param name="min">Minimum angle limit</param>
        /// <param name="max">Maximum angle limit.</param>
        public void Configure(float sensitivity, float min, float max)
        {
            if (max < min)
                throw new ArgumentException("Max should be more than min");

            mult = sensitivity;
            this.min = min;
            this.max = max;
        }
    }

    /// <summary>
    /// Two-axis controller
    /// </summary>
    public abstract class TwoAxisController
    {
        /// <summary>
        /// Default constructor, By default coordinate system is CoordSystemType.Body
        /// </summary>
        /// 
        protected AxisSettings axis1 = new AxisSettings();
        protected AxisSettings axis2 = new AxisSettings();

        /// <summary>
        /// Returns corrected quaternion with applied sensitivity, angles limits and removes rotation around 3rd axis
        /// </summary>
        /// <returns>Corrected quaternion</returns>
        /// <param name="quat">The quaternion to correct</param>
        public abstract Quaternion UpdateQuat(Quaternion quat);

        protected static float clamp(float val, float min, float max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        protected static float clamp(float val, AxisSettings axis)
        {
            if (axis.min == axis.max)
                return val * axis.mult;

            return Math.Min(Math.Max(val * axis.mult, axis.min), axis.max);
        }
    }

    /// <summary>
    /// Controller that allows rotation only around device Y-axis (Azimuth) and X-axis (Elevation)
    /// </summary>
    public class AzimuthElevationController : TwoAxisController
    {
        /// <summary>
        /// Gets the azimuth config
        /// </summary>
        /// <value>The azimuth axis settings</value>
        public AxisSettings Azimuth { get { return axis1; } }
        /// <summary>
        /// Gets the elevation config
        /// </summary>
        /// <value>The elevation axis settings</value>
        public AxisSettings Elevation { get { return axis2; } }

        /// <summary>
        /// Returns corrected quaternion with applied sensitivity, angles limits and removes rotation around Z-axis
        /// </summary>
        /// <returns>Corrected quaternion</returns>
        /// <param name="quat">The quaternion to correct</param>
        public override Quaternion UpdateQuat(Quaternion quat)
        {
            Vector2 az_el = AzimuthElevationConverter.Convert(quat) * Mathf.Deg2Rad;

            float[] res = QuatUtils.FromYawPitchRoll(
                -clamp(az_el.y, axis2),
                clamp(az_el.x, axis1),
                0f);

            return new Quaternion(res[1], res[2], res[3], res[0]);
        }
    }

    /// <summary>
    /// Controller that allows rotation only around device Z-axis (Tilt) and X-axis (Elevation)
    /// </summary>
    public class TiltElevationController : TwoAxisController
    {
        /// <summary>
        /// Gets the tilt config
        /// </summary>
        /// <value>The tilt axis settings</value>
        public AxisSettings Tilt { get { return axis1; } }
        /// <summary>
        /// Gets the elevation config
        /// </summary>
        /// <value>The elevation axis settings</value>
        public AxisSettings Elevation { get { return axis2; } }

        /// <summary>
        /// Returns corrected quaternion with applied sensitivity, angles limits and removes rotation around Y-axis
        /// </summary>
        /// <returns>Corrected quaternion</returns>
        /// <param name="quat">The quaternion to correct</param>
        public override Quaternion UpdateQuat(Quaternion quat)
        {
            Vector2 tilt_el = TiltElevationConverter.Convert(quat) * Mathf.Deg2Rad;

            float[] res = QuatUtils.FromYawPitchRoll(
                -clamp(tilt_el.y, axis2),
                0f,
                -clamp(tilt_el.x, axis1));

            return new Quaternion(res[1], res[2], res[3], res[0]);
        }
    }

    #endregion

    #region quatutils

    /// <summary>
    /// Represents quaternion math utility functions for left-handed and right-hadned coordinate systems
    /// Based on XNA math library (handedless functions nad RH functions) and SlimDX math library (LH functions)
    /// </summary>
    internal static class QuatUtils
    {
        /// <summary>
        /// Identity rotation quaternion {1, 0, 0, 0}
        /// </summary>
        public static float[] Identity
        {
            get
            {
                return new float[] { 1, 0, 0, 0 };
            }
        }

        /// <summary>
        /// Modulates a quaternion by another
        /// </summary>
        /// <param name="q1">The first quaternion to modulate, {w, x, y, z}</param>
        /// <param name="q2">The second quaternion to modulate, {w, x, y, z}</param>
        /// <returns>Modulated quaternion, {w, x, y, z}</returns>
        public static float[] Mult(float[] q1, float[] q2)
        {
            float[] res = new float[4];

            float x = q1[1];
            float y = q1[2];
            float z = q1[3];
            float w = q1[0];
            float num4 = q2[1];
            float num3 = q2[2];
            float num2 = q2[3];
            float t = q2[0];
            float num12 = (y * num2) - (z * num3);
            float num11 = (z * num4) - (x * num2);
            float num10 = (x * num3) - (y * num4);
            float num9 = ((x * num4) + (y * num3)) + (z * num2);
            res[1] = ((x * t) + (num4 * w)) + num12;
            res[2] = ((y * t) + (num3 * w)) + num11;
            res[3] = ((z * t) + (num2 * w)) + num10;
            res[0] = (w * t) - num9;
            return res;
        }

        /// <summary>
        /// Conjugates a given quaternion
        /// </summary>
        /// <param name="q">The quatenion to conjugate, {w, x, y, z}</param>
        /// <returns>The conjugated quaternion, {w, x, y, z}</returns>
        public static float[] Conjugate(float[] q)
        {
            return new float[] { q[0], -q[1], -q[2], -q[3] };
        }

        /// <summary>
        /// Conjugates and renormalizes the quaternion
        /// </summary>
        /// <param name="q">The quaternion to conjugate and renormalize, {w, x, y, z}</param>
        /// <returns>The conjugated and renormalized quatenion, {w, x, y, z}</returns>
        public static float[] Inverse(float[] q)
        {
            float[] res = new float[] { q[0], -q[1], -q[2], -q[3] };
            float m = QuatUtils.Magnitude(q);
            return new float[] { res[0] / m, res[1] / m, res[2] / m, res[3] / m };
        }

        /// <summary>
        /// Converts a given quaternion into a unit quaternion
        /// </summary>
        /// <param name="q">The quaternion to convert, {w, x, y, z}</param>
        /// <returns>The converted quaternion, {w, x, y, z}</returns>
        public static float[] Normalize(float[] q)
        {
            float m = QuatUtils.MagnitudeSquared(q);
            return new float[] { q[0] / m, q[1] / m, q[2] / m, q[3] / m };
        }

        /// <summary>
        /// Calculates the magnitude of the quaternion
        /// </summary>
        /// <param name="q">The quaternion to calculate magnitude, {w, x, y, z}</param>
        /// <returns>Magnitude of the quaternion</returns>
        public static float Magnitude(float[] q)
        {
            return (float)Math.Sqrt(q[0] * q[0] + q[1] * q[1] + q[2] * q[2]
                                    + q[3] * q[3]);
        }

        /// <summary>
        /// Calculates the squared magnitude of the quaternion
        /// </summary>
        /// <param name="q">The quaternion to calculate magnitude, {w, x, y, z}</param>
        /// <returns>Squared magnitude of the quaternion</returns>
        public static float MagnitudeSquared(float[] q)
        {
            return q[0] * q[0] + q[1] * q[1] + q[2] * q[2]
                                    + q[3] * q[3];
        }

        /// <summary>
        /// Performs a linear interpolation between two quaternions
        /// </summary>
        /// <param name="q1">Start quaternion</param>
        /// <param name="q2">End quaternion</param>
        /// <param name="t">Value between 0 and 1 indicating the weight of the second quaternion</param>
        /// <returns>Result of the interpolation, {w, x, y, z}</returns>
        public static float[] Lerp(float[] q1, float[] q2, float t)
        {
            float num2 = 1f - t;
            float[] res = new float[4];

            float num5 = (((q1[1] * q2[1]) + (q1[2] * q2[2])) + (q1[3] * q2[3])) + (q1[0] * q2[0]);
            if (num5 >= 0f)
            {
                res[1] = (num2 * q1[1]) + (t * q2[1]);
                res[2] = (num2 * q1[2]) + (t * q2[2]);
                res[3] = (num2 * q1[3]) + (t * q2[3]);
                res[0] = (num2 * q1[0]) + (t * q2[0]);
            }
            else
            {
                res[1] = (num2 * q1[1]) - (t * q2[1]);
                res[2] = (num2 * q1[2]) - (t * q2[2]);
                res[3] = (num2 * q1[3]) - (t * q2[3]);
                res[0] = (num2 * q1[0]) - (t * q2[0]);
            }

            return QuatUtils.Normalize(res);
        }

        /// <summary>
        /// Interpolates between two quaternions, using spherical linear interpolation
        /// </summary>
        /// <param name="q1">Start quaternion</param>
        /// <param name="q2">End quaternion</param>
        /// <param name="t">Value between 0 and 1 indicating the weight of the second quaternion</param>
        /// <returns>Result of the interpolation, {w, x, y, z}</returns>
        public static float[] Slerp(float[] q1, float[] q2, float t)
        {
            float[] res = new float[4];
            float num2;
            float num3;

            float num4 = (((q1[1] * q2[1]) + (q1[2] * q2[2])) + (q1[3] * q2[3])) + (q1[0] * q2[0]);
            bool flag = false;
            if (num4 < 0f)
            {
                flag = true;
                num4 = -num4;
            }
            if (num4 > 0.999999f)
            {
                num3 = 1f - t;
                num2 = flag ? -t : t;
            }
            else
            {
                float num5 = (float)Math.Acos(num4);
                float num6 = (float)(1.0 / Math.Sin(num5));
                num3 = (float)Math.Sin((1f - t) * num5) * num6;
                num2 = flag ? ((float)-Math.Sin(t * num5) * num6) : ((float)Math.Sin(t * num5) * num6);
            }
            res[1] = (num3 * q1[1]) + (num2 * q2[1]);
            res[2] = (num3 * q1[2]) + (num2 * q2[2]);
            res[3] = (num3 * q1[3]) + (num2 * q2[3]);
            res[0] = (num3 * q1[0]) + (num2 * q2[0]);

            return res;
        }

        /// <summary>
        /// Converts rotation represented by the Euler angles to the quaternion rotation
        /// </summary>
        /// <param name="yaw">The yaw angle</param>
        /// <param name="pitch">The pitch angle</param>
        /// <param name="roll">The roll angle</param>
        /// <returns>The converted quaternion, {w, x, y, z}</returns>
        public static float[] FromYawPitchRoll(float yaw, float pitch, float roll)
        {
            float[] res = new float[4];
            float num9 = roll * 0.5f;
            float num6 = (float)Math.Sin(num9);
            float num5 = (float)Math.Cos(num9);
            float num8 = yaw * 0.5f;
            float num4 = (float)Math.Sin(num8);
            float num3 = (float)Math.Cos(num8);
            float num7 = pitch * 0.5f;
            float num2 = (float)Math.Sin(num7);
            float num = (float)Math.Cos(num7);
            res[1] = ((num * num4) * num5) + ((num2 * num3) * num6);
            res[2] = ((num2 * num3) * num5) - ((num * num4) * num6);
            res[3] = ((num * num3) * num6) - ((num2 * num4) * num5);
            res[0] = ((num * num3) * num5) + ((num2 * num4) * num6);
            return res;
        }

        /// <summary>
        /// Calculates the Euler angles of the quaternion rotation (Yaw, Pitch, Rall) 
        /// </summary>
        /// <param name="q">The Quaternion to calculate Euler angles, {w, x, y, z}</param>
        /// <returns>The calculated Euler angles, {yaw, pitch, roll}</returns>
        public static float[] ToYawPitchRoll(float[] q)
        {
            //Based on http://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine
            float[] res = new float[3];

            float sqw = q[0] * q[0];
            float sqx = q[1] * q[1];
            float sqy = q[2] * q[2];
            float sqz = q[3] * q[3];
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q[1] * q[0] - q[2] * q[3];

            if (test > 0.4995f * unit)
            { // singularity at north pole
                res[1] = NormalizeAngle(2f * (float)Math.Atan2(q[2], q[1]));
                res[0] = (float)Math.PI / 2;
                res[2] = 0;
                return res;
            }

            if (test < -0.4995f * unit)
            { // singularity at south pole
                res[1] = NormalizeAngle(-2f * (float)Math.Atan2(q[2], q[1]));
                res[0] = (float)-Math.PI / 2;
                res[2] = 0;
                return res;
            }

            q = new float[4] { q[2], q[0], q[3], q[1] };
            res[1] = (float)Math.Atan2(2f * q[1] * q[0] + 2f * q[2] * q[3], 1 - 2f * (q[3] * q[3] + q[0] * q[0]));     // Yaw
            res[0] = (float)Math.Asin(2f * (q[1] * q[3] - q[0] * q[2]));                             // Pitch
            res[2] = (float)Math.Atan2(2f * q[1] * q[2] + 2f * q[3] * q[0], 1 - 2f * (q[2] * q[2] + q[3] * q[3]));      // Roll

            res[0] = NormalizeAngle(res[0]);
            res[1] = NormalizeAngle(res[1]);
            res[2] = NormalizeAngle(res[2]);

            return res;
        }

        /// <summary>
        /// Normalizes angle to [0, 2*PI] bounds 
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Normalized angle in radians</returns>
        private static float NormalizeAngle(float angle)
        {
            float pi2 = (float)Math.PI * 2f;

            while (angle > pi2)
                angle -= pi2;
            while (angle < 0f)
                angle += pi2;
            return angle;
        }

        /// <summary>
        /// Converts quaternion to rotation 4x4 matrix in left-handed coordinate system
        /// </summary>
        /// <param name="q">Quaternion to convert, {w, x, y, z}</param>
        /// <returns>Rotation matrix, {M11, M12, M13, M14, M21, ... , M43, M44} </returns>
        public static float[] ToMatrixLH(float[] q)
        {
            float xx = q[1] * q[1];
            float yy = q[2] * q[2];
            float zz = q[3] * q[3];
            float xy = q[1] * q[2];
            float zw = q[3] * q[0];
            float zx = q[3] * q[1];
            float yw = q[2] * q[0];
            float yz = q[2] * q[3];
            float xw = q[1] * q[0];

            float[] mat = new float[] { 1f, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 1f };

            mat[0] = 1.0f - (2.0f * (yy + zz));
            mat[1] = 2.0f * (xy + zw);
            mat[2] = 2.0f * (zx - yw);
            mat[4] = 2.0f * (xy - zw);
            mat[5] = 1.0f - (2.0f * (zz + xx));
            mat[6] = 2.0f * (yz + xw);
            mat[8] = 2.0f * (zx + yw);
            mat[9] = 2.0f * (yz - xw);
            mat[10] = 1.0f - (2.0f * (yy + xx));

            return mat;
        }

        /// <summary>
        /// Transforms a vector by a quaternion rotation in left-handed coordinate system
        /// </summary>
        /// <param name="q">Quaternion rotation, {w, x, y, z}</param>
        /// <param name="vec">Vector to transform, {x, y, z}</param>
        /// <returns>Transformed vector, {x, y, z}</returns>
        public static float[] TransformVecLH(float[] q, float[] vec)
        {
            float x = q[1] + q[1];
            float y = q[2] + q[2];
            float z = q[3] + q[3];
            float wx = q[0] * x;
            float wy = q[0] * y;
            float wz = q[0] * z;
            float xx = q[1] * x;
            float xy = q[1] * y;
            float xz = q[1] * z;
            float yy = q[2] * y;
            float yz = q[2] * z;
            float zz = q[3] * z;

            float num1 = ((1.0f - yy) - zz);
            float num2 = (xy - wz);
            float num3 = (xz + wy);
            float num4 = (xy + wz);
            float num5 = ((1.0f - xx) - zz);
            float num6 = (yz - wx);
            float num7 = (xz - wy);
            float num8 = (yz + wx);
            float num9 = ((1.0f - xx) - yy);

            float[] res = new float[3] {
			((vec[0] * num1) + (vec[1] * num2)) + (vec[2] * num3),
			((vec[0] * num4) + (vec[1] * num5)) + (vec[2] * num6),
			((vec[0] * num7) + (vec[1] * num8)) + (vec[2] * num9) };

            return res;
        }

        /// <summary>
        /// Converts quaternion to rotation 4x4 matrix in right-handed coordinate system
        /// </summary>
        /// <param name="q">Quaternion to convert, {w, x, y, z}</param>
        /// <returns>Rotation matrix, {M11, M12, M13, M14, M21, ... , M43, M44} </returns>
        public static float[] ToMatrixRH(float[] q)
        {
            float x2 = q[1] * q[1];
            float y2 = q[2] * q[2];
            float z2 = q[3] * q[3];
            float xy = q[1] * q[2];
            float xz = q[1] * q[3];
            float yz = q[2] * q[3];
            float wx = q[0] * q[1];
            float wy = q[0] * q[2];
            float wz = q[0] * q[3];


            return new float[] { 1.0f - 2.0f * (y2 + z2), 2.0f * (xy - wz), 2.0f * (xz + wy), 0.0f,
                2.0f * (xy + wz), 1.0f - 2.0f * (x2 + z2), 2.0f * (yz - wx), 0.0f,
                2.0f * (xz - wy), 2.0f * (yz + wx), 1.0f - 2.0f * (x2 + y2), 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f };
            //float[] res = new float[16];

            //res[0] = 1f - 2f * (q[2] * q[2] + q[3] * q[3]);
            //res[1] = 2f * (q[1] * q[2] + q[0] * q[3]);
            //res[2] = 2f * (q[1] * q[3] - q[0] * q[2]);
            //res[4] = 2f * (q[1] * q[2] - q[0] * q[3]);
            //res[5] = 1f - 2f * (q[1] * q[1] + q[3] * q[3]);
            //res[6] = 2f * (q[2] * q[3] + q[0] * q[1]);
            //res[8] = 2f * (q[1] * q[3] + q[0] * q[2]);
            //res[9] = 2f * (q[2] * q[3] - q[0] * q[1]);
            //res[10] = 1f - 2f * (q[1] * q[1] + q[2] * q[2]);
            //res[15] = 1f;

            //return res;
        }


        /// <summary>
        /// Transforms a vector by a quaternion rotation in right-handed coordinate system
        /// </summary>
        /// <param name="q">Quaternion rotation, {w, x, y, z}</param>
        /// <param name="vec">Vector to transform, {x, y, z}</param>
        /// <returns>Transformed vector, {x, y, z}</returns>
        public static float[] TransformVecRH(float[] q, float[] vec)
        {
            float[] mat = ToMatrixRH(q);

            return new float[] {(vec[0] * mat[0]) + (vec[1] * mat[4]) + (vec[2] * mat[8]) + mat[12],
                                (vec[0] * mat[1]) + (vec[1] * mat[5]) + (vec[2] * mat[9]) + mat[13],
                                (vec[0] * mat[2]) + (vec[1] * mat[6]) + (vec[2] * mat[10]) + mat[14] };
        }
    }

    #endregion
}
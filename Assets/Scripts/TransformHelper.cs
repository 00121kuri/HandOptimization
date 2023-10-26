using UnityEngine;

public static class TransformHelper
{
    /// <summary>
    /// World座標系のQuaternionを対象のTransformのlocalRotationに変換する
    /// </summary>
    /// <param name="target">対象のTransform</param>
    /// <param name="worldRotation">World座標系のQuaternion</param>
    /// <returns>localRotation</returns>
    public static Quaternion InverseTransformRotation(this Transform target, Quaternion worldRotation)
    {
        return Quaternion.Inverse(target.transform.rotation) * worldRotation;
    }

    /// <summary>
    /// World座標系のEulerAnglesを対象のTransformのlocalEulerAnglesに変換する
    /// </summary>
    /// <param name="target">対象のTransform</param>
    /// <param name="worldEuler">World座標系のEulerAngles<</param>
    /// <returns>localEulerAngles</returns>
    public static Vector3 InverseTransformEulerAngles(this Transform target, Vector3 worldEuler)
    {
        return target.InverseTransformRotation(Quaternion.Euler(worldEuler)).eulerAngles;
    }
}
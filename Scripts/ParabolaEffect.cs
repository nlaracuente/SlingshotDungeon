using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Handles generating vector3 with a parabola lerp effect
/// 
/// Formula derived from the example provided in
/// <link>https://gist.github.com/ditzel/68be36987d8e7c83d48f497294c66e08</link>
/// </summary>
public class ParabolaEffect : MonoBehaviour
{
    /// <summary>
    /// Returns a Lerped vector3 based on the from/to positions creating an arc
    /// based on the arc heights and time
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="arc"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 Parabola(Vector3 from, Vector3 to, float arc, float time)
    {
        Func<float, float> f = x => -4 * arc * x * x + 4 * arc * x;

        Vector3 mid = Vector3.Lerp(from, to, time);

        return new Vector3(mid.x, f(time) + Mathf.Lerp(from.y, to.y, time), mid.z);
    }
}

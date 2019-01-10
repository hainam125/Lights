using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension {
    public static Vector2 ToVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }
}

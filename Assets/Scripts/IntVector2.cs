using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector2 {
    private int _x;
    private int _y;

    public IntVector2(int x, int y) {
        this._x = x;
        this._y = y;
    }

    public IntVector2(float x, float y) {
        this._x = (int)x;
        this._y = (int)y;
    }

    public int x {
        get { return _x; }
        set { _x = value; }
    }

    public int y {
        get { return _y; }
        set { _y = value; }
    }

    //Addition operator
    public static IntVector2 operator +(IntVector2 a, IntVector2 b) {
        return new IntVector2(a.x + b.x, a.y + b.y);
    }

    //Subtraction operator
    public static IntVector2 operator -(IntVector2 a, IntVector2 b) {
        return new IntVector2(a.x - b.x, a.y - b.y);
    }

    public override string ToString() {
        return "(" + _x + ", " + _y + ")";
    }

    /// <summary>
    /// Returns the distance (magnitude) between two IntVector2s.
    /// </summary>
    public static float Distance(IntVector2 a, IntVector2 b) {
        return Mathf.Sqrt(Mathf.Pow((a.x - b.x), 2) + Mathf.Pow((a.y - b.y), 2));
    }
}

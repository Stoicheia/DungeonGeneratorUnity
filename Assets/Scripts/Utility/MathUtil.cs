using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class MathUtil
{
    public static int UpperNetherPortalNumber(int x)
    {
        var sideLength = (x - 1) / 4 + 1;
        return sideLength * sideLength;
    }
}

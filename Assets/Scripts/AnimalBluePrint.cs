using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AnimalBluePrint {
    IntVector2 targetPos;
    AnimalType type;

    public AnimalBluePrint(IntVector2 target, AnimalType aType) {
        targetPos = target;
        type = aType;
    }

    public AnimalBluePrint(IntVector2 target) {
        targetPos = target;
        type = (AnimalType)Random.Range(0, 5);
    }

    public IntVector2 TargetPos { get { return targetPos; } }
    public AnimalType Type { get { return type; } }
}

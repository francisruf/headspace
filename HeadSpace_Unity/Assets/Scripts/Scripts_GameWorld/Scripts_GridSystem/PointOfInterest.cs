using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PointOfInterest
{
    void RevealPoint(MapPointOfInterest point);
    void SetStartingState(MapPointOfInterest point, bool isVisible);
}

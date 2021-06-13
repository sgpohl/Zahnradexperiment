using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput
{
    private static bool IsInReplayMode = false;
    public static Vector2 mousePosition
    {
        get
        {
            if (!IsInReplayMode)
                return Input.mousePosition;
            return Input.mousePosition;
        }
    }

    public static void Update()
    {
        //EventSystem.current.RaycastAll(myCustomPointerData, resultList);
    }
}

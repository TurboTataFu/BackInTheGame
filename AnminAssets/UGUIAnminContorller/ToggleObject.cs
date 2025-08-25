using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCloseUI : MonoBehaviour
{
    public GameObject targetObject;
    public void Toggle()
    {
        if(targetObject !=null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysLastSibling : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.SetAsLastSibling();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.GetSiblingIndex()!=transform.parent.childCount - 1)
        {
            transform.SetAsLastSibling();
        }
    }
}

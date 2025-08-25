using UnityEngine;

public class ObjectStateCursorControl : MonoBehaviour
{
    [SerializeField] private GameObject[] targetObjects;

    private void Update()
    {
        bool anyObjectActive = CheckAnyObjectActive();
        UpdateCursorState(anyObjectActive);
    }

    private bool CheckAnyObjectActive()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null && obj.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateCursorState(bool unlockCursor)
    {
        if (unlockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
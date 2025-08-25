using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor);
    void OnStartHover();
    void OnEndHover();
}
using UnityEngine;

public abstract class Motor : MonoBehaviour
{
    public abstract void Move(float x, float y);

    public abstract void MoveCamera(float x, float y);
}

using UnityEngine;

public class LookAtMouse : MonoBehaviour
{
    [SerializeField] private Camera _cam;

    private void Update()
    {
        var mouse = Input.mousePosition;
        var mouseRay = _cam.ScreenPointToRay(mouse);

        var middlePoint = (transform.position - _cam.transform.position).magnitude * 0.5f;
        transform.LookAt(mouseRay.origin + mouseRay.direction * middlePoint);
    }
}

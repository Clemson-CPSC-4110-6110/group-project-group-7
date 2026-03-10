using UnityEngine;

public class CartRiderController : MonoBehaviour
{
    public Transform xrOrigin;
    public Transform rideAnchor;

    public bool playerIsRiding = false;

    private Vector3 lastAnchorPosition;

    private void Start()
    {
        if (rideAnchor != null)
        {
            lastAnchorPosition = rideAnchor.position;
        }
    }

    private void LateUpdate()
    {
        if (!playerIsRiding || xrOrigin == null || rideAnchor == null)
            return;

        Vector3 delta = rideAnchor.position - lastAnchorPosition;
        xrOrigin.position += delta;
        lastAnchorPosition = rideAnchor.position;
    }

    public void StartRiding()
    {
        playerIsRiding = true;
        lastAnchorPosition = rideAnchor.position;
    }

    public void StopRiding()
    {
        playerIsRiding = false;
    }
}
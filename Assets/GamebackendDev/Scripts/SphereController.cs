using UnityEngine;
using Mirror;

public class SphereController : NetworkBehaviour
{
    private float speed = 0.05f;

    void Update()
    {
        if (isLocalPlayer)
        {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal") * speed, 0, Input.GetAxis("Vertical") * speed);
            transform.position = transform.position + movement;
        }

    }
}

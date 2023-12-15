using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _cam;

    public float moveSpeed,
        scrollSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * moveSpeed;

        var zoom = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

        _cam.transform.position += new Vector3(input.x, zoom, input.y) * Time.deltaTime;
    }
}

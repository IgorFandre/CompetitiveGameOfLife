using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoving : MonoBehaviour
{
    public Camera camera;
    void Start() {}
    void Update() {
        // Проверяем нажатия клавиш и двигаем камеру
        if (Input.GetKey(KeyCode.RightArrow)) {
            camera.transform.position += new Vector3(0.3f, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            camera.transform.position -= new Vector3(0.3f, 0, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            camera.transform.position += new Vector3(0, 0.3f, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            camera.transform.position -= new Vector3(0, 0.3f, 0);
        }
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals)) {
            camera.orthographicSize += 0.3f;
        } else if (Input.GetKey(KeyCode.Minus)) {
            camera.orthographicSize -= 0.3f;
        }
    }
}

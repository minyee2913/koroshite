using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Loading_spin : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, -100f * Time.deltaTime));
    }
}

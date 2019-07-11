using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLookat : MonoBehaviour
{
    public List<Transform> RotateAts;
    public float scale;
    Vector3 pos;

    // Start is called before the first frame update
    void Start()
    {
        pos = Vector3.zero;
        foreach (var e in RotateAts)
        {
            pos += e.position;
        }
        pos = pos / RotateAts.Count;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(pos, Vector3.up, Time.deltaTime * scale);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{
    // Start is called before the first frame update
    public float rate = 1f;

    Renderer render;
    float addTime = 0f;
    void Start()
    {
        render = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (render == null)
            return;
        addTime += Time.deltaTime * rate;
        float process = Mathf.Sin(addTime);
        render.material.SetFloat("_DisProcess", process * 100);
    }
}

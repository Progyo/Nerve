using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
        Debug.Log(GPT3.Request("Once upon a time", "davinci", 5, true));

        /*foreach(string engine in GPT3.engineIds) 
        {
            Debug.Log(engine);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cineTest : MonoBehaviour
{
    public GameObject follow, god;
  

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            follow.gameObject.SetActive(false);
            god.SetActive(true);
        }
        else if(Input.GetKeyDown(KeyCode.S))
        {

            follow.gameObject.SetActive(true);
            god.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class healthPointUIController : MonoBehaviour
{
    TextMeshProUGUI tmp;
    public void Awake()
    {
        tmp=GetComponent<TextMeshProUGUI>();
    }
    public void damage(int hp)
    {
        tmp.text = hp.ToString();
    }
}

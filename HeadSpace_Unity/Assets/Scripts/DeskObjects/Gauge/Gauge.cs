using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gauge : MonoBehaviour
{
    private SpriteRenderer gaugeFill;
    public float addPercent;
    private float fillAmount;

    void Start()
    {
        gaugeFill = GetComponent<SpriteRenderer>();
    }

    
    void Update()
    {                  //quand on pese sur space change le fillAmount de la gauge
        if (Input.GetKeyDown("space"))
        {
            AddToGauge(addPercent);
            Debug.Log("gauge");
        }
    }

                      //fonction change le height du sprite gaugeFill
    void AddToGauge(float amounttoAdd)
    {
        fillAmount = fillAmount + amounttoAdd;

                      //set un maximum de la gauge
        fillAmount = Mathf.Clamp(fillAmount, 0f, 100f);

        if (gaugeFill != null)
        {
            gaugeFill.size = new Vector2(gaugeFill.size.x, fillAmount/100);
        }


    }

}

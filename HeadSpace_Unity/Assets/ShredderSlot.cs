using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShredderSlot : MonoBehaviour
{   
    public Transform EndPosition;
    public SpriteRenderer GreenButton;
    public SpriteRenderer RedButton;
    private SlidableShredder parent = null;
    private bool shredding = false;

    private void Start()
    {
        GreenButton.enabled = false;
        RedButton.enabled = true;

        if (parent == null)
        {
            parent = transform.parent.GetComponent<SlidableShredder>();
        }
    }
    private void Update()
    {
        if (!CanShred() || shredding)
        {
            GreenButton.enabled = false;
            RedButton.enabled = true;
        }
        else
        {
            GreenButton.enabled = true;
            RedButton.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Destruc")

        {
            if(CanShred() && !shredding)
            { 
                collision.GetComponent<MovableObject>().Deselect();
                collision.transform.position = new Vector3(collision.transform.position.x, transform.position.y, collision.transform.position.z);
                GreenButton.enabled = false;
                RedButton.enabled = true;
                StartCoroutine(Shred(collision.gameObject));
                FindObjectOfType<AudioManager>().PlaySound("Shredder");
            }
        }

        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Indestruc")
        {
            GreenButton.enabled = false;
            RedButton.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Destruc")
        {
            GreenButton.enabled = true;
            RedButton.enabled = false;
        }
        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Indestruc")
        {
            GreenButton.enabled = true;
            RedButton.enabled = false;
        }
    }

    private IEnumerator Shred(GameObject doc)
    {
        shredding = true;
        yield return StartCoroutine(LerpToPosition(doc));
        shredding = false;
        doc.GetComponent<MovableObject>().DisableObject();
    }

    private IEnumerator LerpToPosition(GameObject doc)
    {
        Vector3 startingPosition = doc.transform.position;
        Vector3 endingPosition = EndPosition.position; 
        float timeElaps = 0f;
        float duration = 3f;
        while(timeElaps < duration)
        {
            doc.transform.position = Vector3.Lerp(startingPosition, endingPosition, timeElaps / duration);
            timeElaps += Time.deltaTime;
            yield return null;
        }
    }
    private bool CanShred()
    {   //Determine if we can shred based on the position of the shredder compared to the total width of the camera
        float neededRatioValue = 0.15f;
        float ratio = 1 - (Mathf.Abs(parent.transform.position.x) / (Camera.main.orthographicSize * 2));
        return ratio > neededRatioValue;
    }
}

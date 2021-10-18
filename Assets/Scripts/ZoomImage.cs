using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomImage : MonoBehaviour
{
    private float _initialFingersDistance;
    private Vector3 _initialScale;
    private Vector3 _initialPosition;
    private float _minScaleFactor = 0.5f;
    private float _maxScaleFactor = 2.0f;

    private void Start()
    {
        _initialPosition = transform.position;
    }
 
    private void Update()
    {
        if(Input.touches.Length == 2)
        {
            Touch t1 = Input.touches[0];
            Touch t2 = Input.touches[1];
       
            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                _initialFingersDistance = Vector2.Distance(t1.position, t2.position);
                _initialScale = transform.localScale;
            }
            else if(t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                float currentFingersDistance = Vector2.Distance(t1.position, t2.position);
                float scaleFactor = currentFingersDistance / _initialFingersDistance;
                if(_minScaleFactor < (_initialScale * scaleFactor).x && (_initialScale * scaleFactor).x < _maxScaleFactor) {
                    transform.localScale = _initialScale * scaleFactor;
                }
            }
            
        }
        else if(Input.touchCount==1 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).tapCount==2)
        {
            transform.localScale = Vector3.one;
            transform.position = _initialPosition;
        }
    }
}

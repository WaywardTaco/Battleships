using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMover : MonoBehaviour
{
    [SerializeField] private float CAMERA_MOVE_RATE;
    [SerializeField] private float CAMERA_ROTATE_RATE;
    private bool _isMovingCamera = false;
    
    public void MoveCamTo(Transform target, bool priority = true, float duration = -1){
        if(_isMovingCamera && !priority){
            return;
        }
        if(_isMovingCamera && priority){
            StopAllCoroutines();
        }

        _isMovingCamera = true;

        if(duration < 0){
            float distance = Vector3.Distance(target.position, transform.position);
            float rotDiff = Quaternion.Angle(target.rotation, transform.rotation);
            float moveDuration = distance / CAMERA_MOVE_RATE;
            float rotDuration = rotDiff / CAMERA_ROTATE_RATE;
            duration = moveDuration;
            if(moveDuration < rotDuration)
                duration = rotDuration;
        }

        StartCoroutine(MoveCamToPos(target, duration));
    }

    private IEnumerator MoveCamToPos(Transform target, float duration){
        transform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        float progress = 0.0f;
        while(progress < duration){
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime;

            if(progress < duration){
                transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, target.position, progress/duration), 
                    Quaternion.Lerp(startRotation, target.rotation, progress/duration)
                );
                continue;
            }
        }

        transform.SetPositionAndRotation(target.position, target.rotation);
        _isMovingCamera = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioVizECSJob
{

public class RootTarget : MonoBehaviour 
{
    public Transform cameraTransform;
    public int rotationSpeed;

	void Update() 
    {
        cameraTransform.LookAt(gameObject.transform);
        cameraTransform.Translate(Vector3.right*Time.deltaTime*rotationSpeed);
	}
}

}

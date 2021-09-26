using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract class for interactable objects
public class Interactable : MonoBehaviour
{
    #region Variables

    // Interaction radius
    [SerializeField] protected float _interactionRadius;

    #endregion

    #region Methods

    // Interact
    public void TryInteract(Transform playerTransform)
    {
        // Get distance to the object
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        // Check, if we are withing the radius of the object
        if (distance <= _interactionRadius)
        {
            Interact();
        }
    }

    // Interaction with the object
    protected virtual void Interact()
    {
        Debug.Log("Interaction successful");
        Destroy(gameObject);
    }

    #endregion
}

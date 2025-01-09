using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacerColliderScript : MonoBehaviour {
    public bool _isThisObjectColliding = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.gameObject.tag == "Board") { //if a collider has collided with a wall
            _isThisObjectColliding = true; //mark as colliding
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.transform.gameObject.tag == "Board") { //if a collider has collided with a wall
            _isThisObjectColliding = true; //mark as colliding
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.transform.gameObject.tag == "Board") //if a collider has collided with a wall
            _isThisObjectColliding = false; //mark as not colliding
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleScript : MonoBehaviour
{
    public Color32 _myColor; 

    private void OnTriggerStay2D(Collider2D other) {
        //if collision is made with piece
        if (other.CompareTag("piece") && other.GetComponent<PuzzlePieceScript>()._myColor.Equals(_myColor)) { //if the object is the piece that corresponds with the hole
            var _pieceScript = other.GetComponent<PuzzlePieceScript>(); //save reference to the piece's script
            if (!_pieceScript._isPieceMoving) _pieceScript._isPieceInCorrectHole = true; //if the piece isn't moving, change its boolean to true
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        //if collision is left with piece
        if (other.CompareTag("piece") && other.GetComponent<PuzzlePieceScript>()._myColor.Equals(_myColor)) { //if the object is the piece that corresponds with the hole
            var _pieceScript = other.GetComponent<PuzzlePieceScript>(); //save reference to the piece's script
            _pieceScript._isPieceInCorrectHole = false; //set boolean to false
        }
    }

    public void SaveMyColor() {
        _myColor = GetComponent<SpriteRenderer>().color; //save the color of the material
    }
}

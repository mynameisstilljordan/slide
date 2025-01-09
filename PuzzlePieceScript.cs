using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class PuzzlePieceScript : MonoBehaviour {
    public GameObject _gCS;
    public bool _isPieceMoving = false; //boolean that determines if piece is moving
    public bool _isPieceInCorrectHole = false; //this bool checks if the piece is in its correct hole
    public Vector3 _targetPosition; //the target position of the piece
    public Vector3 _myStartLocation; //the starting location of the piece
    float _movementSpeed; //the speed of the piece
    float _scale; //the scale of the piece
    public Color32 _myColor; //the color of the piece

    private void Start() {
        _gCS = GameObject.FindGameObjectWithTag("gameController"); //find the instance of the gamecontroller
        _movementSpeed = 20f; //set the movement speed
        _targetPosition = new Vector3(0, 0); //the target position for piece movement
        _scale = transform.localScale.x; //the scale of the piece
    }
    private void Update() {
        //if the tile is allowed to move, start moving to the target location
        if (_isPieceMoving) transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _movementSpeed * Time.deltaTime);
        //set boolean to true to indicate to PlayerControlScript that the piece is not moving
        if (transform.position == _targetPosition && _isPieceMoving) {
            _isPieceMoving = false;
            SoundManager.PlaySound("tap");
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play light impact vibration
        }
    }

    public void SaveMyColor() {
        _myColor = GetComponent<SpriteRenderer>().material.color; //save the color of the material
    }

    //this method checks if the piece is moving and returns the boolean method 
    public bool IsPieceMoving() {
        return _isPieceMoving;
    }

    //this method checks to see if the piece can be moved by raycasting
    public void CanThisPieceBeMoved(char _directionChar) {
        float _xDisplacement = 0;
        float _yDisplacement = 0;
        Vector2 _rayDirection = new Vector2(0, 0);

        switch (_directionChar) {
            //if the direction is up
            case 'u':
                _yDisplacement = _scale / 2;
                _rayDirection = Vector2.up;
                break;
            //if the direction is down
            case 'd':
                _yDisplacement = -_scale / 2;
                _rayDirection = Vector2.down;
                break;
            //if the direction is left
            case 'l':
                _xDisplacement = -_scale / 2;
                _rayDirection = Vector2.left;
                break;
            //if the direction is right
            case 'r':
                _xDisplacement = _scale / 2;
                _rayDirection = Vector2.right;
                break;
            //default case
            default:
                _xDisplacement = 0; _yDisplacement = 0;
                break;
        }

        float _distance = 0f; 
        int _numberOfPieces = 0; //this variable holds the amount of pieces that made contact with the ray
        int _numberOfPiecesInTheWay = 0; //this variable is for checking how many pieces are in between the current piece and the nearest immovable object 
        float _closestImmovableObstacleDistance = 100f;
        float _spaces; //this variable is for determining how many spaces the piece can move

        //new raycast hit
        RaycastHit2D[] _hits = Physics2D.RaycastAll(new Vector2(transform.position.x + _xDisplacement, transform.position.y + _yDisplacement), _rayDirection, 100f);

        //if the ray only hit 1 collider
        if (_hits.Length == 1) {
            //if the collider is the board or the wall
            if (_hits[0].collider.gameObject.CompareTag("wall") || _hits[0].collider.gameObject.CompareTag("Board")) {
                if (_directionChar == 'r' || _directionChar == 'l') _distance = Mathf.Abs(_hits[0].collider.gameObject.transform.position.x - transform.position.x); //save the distance (x)
                else if (_directionChar == 'u' || _directionChar == 'd') _distance = Mathf.Abs(_hits[0].collider.gameObject.transform.position.y - transform.position.y); //save the distance (y)
                if (_distance > _scale) { //if the distance is greater than 0.5f (meaning distance must be atleast 1 in order for piece to move)
                }
            }
        }

        //if the ray hit more than 1 collider
        else {
            //if the ray hits something
            foreach (RaycastHit2D hit in _hits) {
                //if the collider is the board or the wall
                if (hit.collider.gameObject.CompareTag("wall") || hit.collider.gameObject.CompareTag("Board")) {
                    if (_directionChar == 'r' || _directionChar == 'l') {
                        //if the current object (wall/board) in the "for" loop is closer than the previous closest wall/board 
                        if (_closestImmovableObstacleDistance > Mathf.Abs(hit.collider.gameObject.transform.position.x - transform.position.x))
                            _closestImmovableObstacleDistance = Mathf.Abs(hit.collider.gameObject.transform.position.x - transform.position.x); //update the variable   
                    }
                    else {
                        //if the current object (wall/board) in the "for" loop is closer than the previous closest wall/board 
                        if (_closestImmovableObstacleDistance > Mathf.Abs(hit.collider.gameObject.transform.position.y - transform.position.y))
                            _closestImmovableObstacleDistance = Mathf.Abs(hit.collider.gameObject.transform.position.y - transform.position.y); //update the variable   
                    }
                }
                //if the collider is another piece
                else if (hit.collider.gameObject.CompareTag("piece")) _numberOfPieces++; //increment the amount of obstacles
            }

            //run the loop 1 more time (now that the closestImmovableObstacleDistance is accurate)
            foreach (RaycastHit2D hit in _hits) {

                if (_directionChar == 'r' || _directionChar == 'l') { //if movement was horizontal
                    //if the piece hit is closer than the closest immovable object, increment numberofpiecesintheway
                    var _distanceToClosestPiece = Mathf.Abs(hit.collider.gameObject.transform.position.x - transform.position.x); //set the closest piece to the absolute difference of the distances
                    if (hit.collider.gameObject.CompareTag("piece") && _closestImmovableObstacleDistance > _distanceToClosestPiece) _numberOfPiecesInTheWay++; //increment if piece is closer than closest immovable object on axis
                }
                else { //if the movement was vertical
                    var _distanceToClosestPiece = Mathf.Abs(hit.collider.gameObject.transform.position.y - transform.position.y); //set the closest piece to the absolute difference of the distances
                    if (hit.collider.gameObject.CompareTag("piece") && _closestImmovableObstacleDistance > _distanceToClosestPiece) _numberOfPiecesInTheWay++; //increment if piece is closer than closest immovable object on axis
                }

            }
            //if there is enough space for the piece to move (with all other pieces accounted for)
            _distance = _closestImmovableObstacleDistance - (_numberOfPiecesInTheWay * _scale);
        }
        _distance = Mathf.Round((_distance / _scale)) * _scale; //round distance to the nearest .5 
        _spaces = (_distance - _scale) / _scale; //calculate the amount of spaces the piece can move
        if (_spaces < 1) { //if the value of spaces ever dips below 0, set it to 0
            _spaces = 0;
            _isPieceMoving = false; //update the boolean to show if the piece is moving
        }
        else _isPieceMoving = true; //update the boolean to show if the piece is moving

        //if the piece can be moved
        _targetPosition = new Vector3(transform.position.x + (2 * _xDisplacement * (_spaces)), transform.position.y + (2 * _yDisplacement) * (_spaces), -1); //set the new target position
    }
}

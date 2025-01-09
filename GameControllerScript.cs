using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;
using Lofelt.NiceVibrations;
//using LionStudios.Suite.Analytics;
//using LionStudios.Suite.Debugging;

public class GameControllerScript : MonoBehaviour {

    //schematic class
    internal class Schematic {
        public string Size { set; get; }
        public int NumberOfPieces { set; get; }
        public int NumberOfWalls { get; set; }

        public Schematic(string size, int numberOfPieces, int numberOfWalls) {
            this.Size = size;
            this.NumberOfPieces = numberOfPieces;
            this.NumberOfWalls = numberOfWalls;
        }
    }

    internal class Counter {
        int _easyCounterDefault = 15;
        int _hardCounterDefualt = 25;
        int _veryHardCounterDefault = 100;
        int _veryEasyCounterDefault = 45;

        public int VeryEasyCounter { set; get; }
        public int EasyCounter { set; get; }
        public int NormalCounter { set; get; }
        public int HardCounter { set; get; }
        public int VeryHardCounter { set; get; }

        public Counter() {
            VeryEasyCounter = PlayerPrefs.GetInt("VeryEasyCounter", _veryEasyCounterDefault-1);
            EasyCounter = PlayerPrefs.GetInt("EasyCounter", _easyCounterDefault-1);
            HardCounter = PlayerPrefs.GetInt("HardCounter", _hardCounterDefualt-1);
            VeryHardCounter = PlayerPrefs.GetInt("VeryHardCounter", _veryHardCounterDefault-1);
        }

        public void DecrementAllCounters() {
            if (VeryEasyCounter == 0) VeryEasyCounter = _veryEasyCounterDefault;
            if (EasyCounter == 0) EasyCounter = _easyCounterDefault;
            if (HardCounter == 0) HardCounter = _hardCounterDefualt;
            if (VeryHardCounter == 0) VeryHardCounter = _veryHardCounterDefault;
            VeryEasyCounter--;
            EasyCounter--;
            HardCounter--;
            VeryHardCounter--;
            PlayerPrefs.SetInt("VeryEasyCounter", VeryEasyCounter);
            PlayerPrefs.SetInt("EasyCounter", EasyCounter);
            PlayerPrefs.SetInt("HardCounter", HardCounter);
            PlayerPrefs.SetInt("VeryHardCounter", VeryHardCounter);
        }
    }

    enum GameState {
        Ingame, Paused, LevelComplete
    };

    [SerializeField] ParticleSystem _ps; //the confetti particlesystem
    GameState _currentGameState;
    Schematic _veryEasyLevel, _easyLevel, _normalLevel, _hardLevel, _veryHardLevel;
    Counter _counter;
    GameHandlerScript _gHS; //new gamehandlerscript reference
    public GameObject[] _pieces; //the array of pieces
    public Sprite[] _gameBoardSprites; //the board sprites (to change per board level)
    [SerializeField] GameObject _objectPlacer; //the objectPlacer GameObject
    [SerializeField] GameObject _wall; //the wall GameObject
    [SerializeField] GameObject _piece; //the piece GameObject
    [SerializeField] GameObject _hole; //the objective hole
    [SerializeField] GameObject _gameBoard; //the gameboard gameobject
    [SerializeField] GameObject _generatingGameBoardCover; //the cover for when the board is generating
    SpriteRenderer _generatingGameBoardCoverSpriteRenderer; //the renderer for the generating gameboardcover
    SpriteRenderer _gSR; //the spriterenderer of the gameboard
    [SerializeField] Canvas _inGameCanvas; //the ingame canvas
    [SerializeField] Canvas _settingsCanvas; //the settings canvas 
    [SerializeField] Canvas _endGameCanvas; //the endgame canvas 
    [SerializeField] TMP_Text _levelText; //the level text
    [SerializeField] TMP_Text _difficultyText; //the text difficulty
    GameObject _objectPlacerInstance = null; //the instance of the object placer
    Vector2 _startTouchPos, _endTouchPos; //for the start and end touch position
    Vector2 _transformScale = Vector2.zero; //the scale to resize objects
    int _minimumSwipeDistance = Screen.height * 5 / 100; //the distance required to swipe in order for an input to be registered (15% of screen height)
    public int _seed; //the seed of the board 
    float _incrementInterval; //the increment angle for the speciifc scale
    float _positionClamp; //this float holds the positive value of the furthest x or y a piece or wall can spawn at
    bool _areAllPiecesInCorrectHole = false; //a boolean for toggling
    public TMP_Text _scoreText; //the score text
    List<char[,]> _schematicsQueue = new List<char[,]>(); //the schematic queue 
    List<Vector2[]> _solutionKey = new List<Vector2[]>();
    //.3 = 15x15
    //.5 = 9x9
    //.9 = 5x5

    private void Start() {
        //LionDebugger.Hide();
        GlobalGameHandlerScript.Instance.IncrementLevelAttempt();
        //LionAnalytics.LevelStart(GlobalGameHandlerScript.Instance.Level, GlobalGameHandlerScript.Instance.LevelAttempt); //lion for level start

        //resize board to 80% of the screen width
        //float width = GetScreenToWorldWidth();
        //_gameBoard.transform.localScale = Vector3.one * (width * 0.8f); //set the scale of the board to 80% of the screen width

        _currentGameState = GameState.Ingame; //set current gamestate to ingame
        _counter = new Counter(); //the counter of the levels
        _levelText.text = "LEVEL: "+ GlobalGameHandlerScript.Instance.Level.ToString();

        //all the schematic presets
        _veryEasyLevel = new Schematic("large", 1, 3);
        _easyLevel = new Schematic("large", 2, 2);
        _normalLevel = new Schematic("medium", 2, 10);
        _hardLevel = new Schematic("medium", 3, 7);
        _veryHardLevel = new Schematic("small", 3, 15);

        GlobalGameHandlerScript.Instance.ShowBanner(); //show the banner
        _gHS = GameObject.FindGameObjectWithTag("gameHandler").GetComponent<GameHandlerScript>(); //find the gameobject with the tag and save the script reference
        _generatingGameBoardCoverSpriteRenderer = _generatingGameBoardCover.GetComponent<SpriteRenderer>(); //get the instance of the spriterenderer
        var panelColor = new Color32((byte)GlobalGameHandlerScript.Instance.BackgroundColor.PrimaryColor.r, (byte)GlobalGameHandlerScript.Instance.BackgroundColor.PrimaryColor.g, (byte)GlobalGameHandlerScript.Instance.BackgroundColor.PrimaryColor.b, 220);
        _generatingGameBoardCoverSpriteRenderer.color = panelColor;
        //_generatingGameBoardCoverSpriteRenderer.color = Camera.main.backgroundColor; //set the generating board cover color to the background color
        _settingsCanvas.transform.GetChild(0).GetComponent<Image>().color = panelColor; //set the pause panel to the background color
        _endGameCanvas.transform.GetChild(0).GetComponent<Image>().color = panelColor; //set the end game panel to the background color
        _gSR = _gameBoard.GetComponent<SpriteRenderer>(); //get the spriterenderer of the gameboard
        _pieces = new GameObject[9];

        LoadSeed(GlobalGameHandlerScript.Instance.Level); //load a new seed

        GenerateLevelSchematic(GetNextLevelType());
    }

    float GetScreenToWorldWidth() {
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        var width = edgeVector.x * 2;
        return width;
    }

    private void Update() {
        //if there's no pieces active and there's atleast one schematic in queue, build the level
        if (HowManyPiecesAreInArray() < 1 && _schematicsQueue.Count > 0)
            BuildLevel(NextSchematic(), NextSolutionKey());

        if (Input.GetKeyDown(KeyCode.L)) Debug.Log(_schematicsQueue.Count);
        if (Input.GetKeyDown(KeyCode.B)) BuildLevel(NextSchematic(), NextSolutionKey());

        //if all pieces are in correct hole
        if (AreAllPiecesInCorrectHole() && !_areAllPiecesInCorrectHole) {
            _areAllPiecesInCorrectHole = true; //set to true so this code is only reached once per level completion
            CheckLevelCompletion();
        }

        if (!AreAnyPiecesMoving()) {
            //save the number of touches
            var _count = Input.touchCount;

            //if there is only 1 touch on the screen
            if (_count == 1 && _currentGameState == GameState.Ingame) {
                var _touch = Input.GetTouch(0);

                //if the touch phase has began
                if (_touch.phase == TouchPhase.Began) {
                    _startTouchPos = _touch.position; //save the starting position
                }

                //if the touch phase has ended
                if (_touch.phase == TouchPhase.Ended) {
                    _endTouchPos = _touch.position; //save the ending position 
                    RegisterTouch(_startTouchPos, _endTouchPos); //register the touch 
                }
            }
#if UNITY_EDITOR
            //FOR TESTING ON PC
            if (Input.GetKeyDown(KeyCode.A)) Move('l');
            if (Input.GetKeyDown(KeyCode.S)) Move('d');
            if (Input.GetKeyDown(KeyCode.D)) Move('r');
            if (Input.GetKeyDown(KeyCode.W)) Move('u');
#endif
        }
    }

    private void BuildLevel(char[,] _schematic, Vector2[] _holeLocations) {
        int _boardSize = _schematic.GetLength(0); //save the board size to an int
        RemoveSchematicFromQueue();
        _objectPlacerInstance.transform.position = new Vector3(-_positionClamp, _positionClamp, -1);
        
        for (int i = 1; i < _boardSize-1; i++) { //for all rows on the board
            for (int j = 1; j < _boardSize-1; j++) { //for all columns on the board

                //place wall
                if (_schematic[i, j] == 'W') {
                    var _wallInstance = Instantiate(_wall, _objectPlacerInstance.transform.position, Quaternion.identity); //spawn a wall at the location
                    _wallInstance.transform.localScale = _transformScale; //set the scale to the transform scale
                    if (_wallInstance.transform.localScale.x == 1.5f) _wallInstance.transform.localScale = new Vector3(1.6f, 1.6f, 0f); //hardcode for large levels (because of poor masking)
                    //_wallInstance.GetComponent<SpriteRenderer>().enabled = false; //disable the sprite renderer
                }
                
                //place piece
                else if (_schematic[i,j] != 'W' && _schematic[i,j] != 'O') {
                    var _pieceInstance = Instantiate(_piece, (_objectPlacerInstance.transform.position), Quaternion.identity); //spawn a wall at the location //spawn a wall at the location
                    _pieceInstance.transform.localScale = _transformScale; //set the scale to the transform scale
                    _pieceInstance.GetComponent<SpriteRenderer>().material.color = _gHS._colors[((int)_schematic[i,j]) - 97]; //return the next color
                    _pieceInstance.GetComponent<PuzzlePieceScript>().SaveMyColor(); //save the material color
                    AddPieceToArray(_pieceInstance); //add the piece to the array
                }

                //place hole
                if (_holeLocations.Contains(new Vector2(i,j))) {
                    int index = 0; //a variable for determining which index the current hole is at

                    for (int k = 0; k < _holeLocations.Length; k++) { //for all the hole locations
                        if (_holeLocations[k].Equals(new Vector2(i, j))) { //if the current index is the right hole location
                            index = k; //save the value
                            break; //break;
                        }
                    }

                    var _holeInstance = Instantiate(_hole, new Vector3(_objectPlacerInstance.transform.position.x, _objectPlacerInstance.transform.position.y, -1.1f), Quaternion.identity); //spawn the piece at the given location
                    _holeInstance.transform.localScale = _transformScale; //apply the scale to the gameobject
                    _holeInstance.GetComponent<SpriteRenderer>().color = _gHS._colors[index]; //set the color of the hole
                    _holeInstance.GetComponent<HoleScript>().SaveMyColor();
                }
                _objectPlacerInstance.transform.position = new Vector3(_objectPlacerInstance.transform.position.x+_incrementInterval, _objectPlacerInstance.transform.position.y, -1); //move the objectPlacer [incrementInterval] spaces to the right
            }
            _objectPlacerInstance.transform.position = new Vector3(-_positionClamp, _objectPlacerInstance.transform.position.y - _incrementInterval, -1); //move the objectPlacer [incrementInterval] spaces down
        }
        _generatingGameBoardCover.SetActive(false); //hide the generating board cover
    }

    #region pre-generation setup
    //this method loads the given seed (0 for a random seed)
    private void LoadSeed(int s) {
        if (s == 0) _seed = (int)System.DateTime.Now.Ticks; //if no seed is passed, generate one
        else _seed = s; //if a seed is passed, load it
        UnityEngine.Random.InitState(_seed); //set the seed
    }

    //this method applies the scale so that each piece is placed correctly depending on the board size
    private void ApplyScale(string _size) {
        float _scaleSize = 0f; //the vairable used for resizing the gameobjects' scale

        switch (_size) { //switch case to cast the string paramater to a float
            case "tiny": _scaleSize = .5f; break; //in the case of a 15x15 board
            case "small": _scaleSize = .64285714285f; break; //in the case of a 9x9 board
            case "medium": _scaleSize = .9f; break; //in the case of a 5x5 board
            case "large": _scaleSize = 1.5f; break; //in the case of a 3x3 board
        }

        if (GameObject.FindGameObjectWithTag("objectPlacer") == null) { //if the objectPlacer doesn't exist already
            _objectPlacerInstance = Instantiate(_objectPlacer, Vector2.zero, Quaternion.identity); //spawn the object placer
            _objectPlacerInstance.transform.localScale = new Vector2(_scaleSize, _scaleSize); //set the scale of the objectplacer
            _objectPlacerInstance.GetComponent<SpriteRenderer>().enabled = false; //make the objecplacer invisible
        }

        _transformScale = _objectPlacerInstance.transform.localScale; //save the scale 
        _incrementInterval = _transformScale.x; //set the grid interval to the size of the pieces

        var _multiplier = Mathf.Round(2 / _incrementInterval); //get the multiplier from using the given equation
        _positionClamp = _incrementInterval * _multiplier; //multiply the incrementInterval by the found multiplier
    }
    #endregion

    #region schematic generation
    //this method generates a level schematic
    private void GenerateLevelSchematic(Schematic schematic) {
        _generatingGameBoardCoverSpriteRenderer.enabled = true; //enable the board cover
        ApplyScale(schematic.Size); //apply the scale to the board objects

        int _schematicSize = 0;
        switch (schematic.Size) {
            case "tiny":
                _schematicSize = 17;
                _gSR.sprite = _gameBoardSprites[0]; //tiny board
                break;
            case "small":
                _schematicSize = 11;
                _gSR.sprite = _gameBoardSprites[1]; //small board
                break;
            case "medium":
                _schematicSize = 7;
                _gSR.sprite = _gameBoardSprites[2]; //medium board
                break;
            case "large":
                _schematicSize = 5;
                _gSR.sprite = _gameBoardSprites[3]; //large board
                break;
        }

        char[,] _schematic = new char[_schematicSize, _schematicSize]; //allocate new schematic size for the board schematic
        for (int i = 0; i < _schematicSize; i++) { //for all rows on the board
            for (int j = 0; j < _schematicSize; j++) { //for all columns on the board
                //if the current index is one of the edge walls, set it to '#'
                if (i == 0 || j == 0 || i == _schematicSize - 1 || j == _schematicSize - 1) _schematic[i, j] = '#'; //if current index is on the border of the schematic, place a #
                else _schematic[i, j] = 'O'; //if the index is within the schematic, place a O
            }
        }

        int _schematicPositionClampMin = 1; //for left and upper clamp
        int _schematicPositionClampMax = _schematicSize - 1; //for right and lower clamp
        int _xPos = 0, _yPos = 0; //the random x and y position where the object will be placed


        //place all the wall placeholders on the board
        for (int i = 0; i < schematic.NumberOfWalls; i++) { //for the amount of walls to be placed
            while (_schematic[_xPos, _yPos] != 'O') {
                _xPos = UnityEngine.Random.Range(_schematicPositionClampMin, _schematicPositionClampMax); //select a random x position
                _yPos = UnityEngine.Random.Range(_schematicPositionClampMin, _schematicPositionClampMax); //select a random y position
            }
            _schematic[_xPos, _yPos] = 'W'; //if the piece is free, add a wall placeholder
        }

        //place all the piece placeholders on the board
        bool _isPositionBlockedIn = true; //this boolean checks if the current location of where an object is going to be placed is blocked in

        for (int i = 0; i < schematic.NumberOfPieces; i++) { //for the amount of walls to be placed
            while (_isPositionBlockedIn) {
                _xPos = UnityEngine.Random.Range(_schematicPositionClampMin, _schematicPositionClampMax); //select a random x position
                _yPos = UnityEngine.Random.Range(_schematicPositionClampMin, _schematicPositionClampMax); //select a random y position

                if ((_schematic[_xPos - 1, _yPos] == 'O' || _schematic[_xPos + 1, _yPos] == 'O' || _schematic[_xPos, _yPos - 1] == 'O' || _schematic[_xPos, _yPos + 1] == 'O') && (_schematic[_xPos, _yPos] == 'O')) //if there's one free space
                    _isPositionBlockedIn = false; //mark blocked in as false
            }

            //placing the pieces
            _schematic[_xPos, _yPos] = Convert.ToChar(i + 97); //place the piece at the location (97 = a, 98 = b etc...)
            _isPositionBlockedIn = true; //reset the positionblockedin variable
        }
        ShuffleLevelSchematic(_schematic);
    }

    //this method shuffles the level schematic [parameter] number of times
    private void ShuffleLevelSchematic(char[,] _schematic) {
        int _randomSelection; //the int for selecting the movement direction
        int _shuffleAmount = 100; //the amount of times the board needs to be shuffled
        Vector2[] _holeLocations = new Vector2[0]; //create empty vector 2 array
        char _slideDirection = ' ';
        for (int i = 0; i < _shuffleAmount; i++) { //for the amount of times the board needs to be shuffled

            //if halfway through shuffling, place holes
            if (i == (_shuffleAmount / 2)) {
                _holeLocations = CreateHolesArray(_schematic);
            }
            _randomSelection = UnityEngine.Random.Range(0, 4); //pick a random number
            switch (_randomSelection) {
                case 0: _slideDirection = 'u'; break;
                case 1: _slideDirection = 'd'; break;
                case 2: _slideDirection = 'l'; break;
                case 3: _slideDirection = 'r'; break;
            }
            if (Slide(_schematic, _slideDirection)) continue; //if a valid direction is given, go on
            else _shuffleAmount++; //if an invalid direction is given, try again
        }


        //if the board is already solved
        if (AreAllSchematicPiecesInAnyHole(_schematic, _holeLocations)) { //if the board has solved itself
            bool _wasValidDirectionChosen = false; //set the valid direction bool to false

            while (!_wasValidDirectionChosen) { //while a valid direction hasnt been chosen, choose one'
                _randomSelection = UnityEngine.Random.Range(0, 4); //pick a random number
                switch (_randomSelection) {
                    case 0: _slideDirection = 'u'; break;
                    case 1: _slideDirection = 'd'; break;
                    case 2: _slideDirection = 'l'; break;
                    case 3: _slideDirection = 'r'; break;
                }
                _wasValidDirectionChosen = Slide(_schematic, _slideDirection); //if a valid direction was chosen, move in it
            }
        }

        ValidateBoardSchematic(_schematic, _holeLocations); //send the schematic off to be validated
    }

    private Vector2[] CreateHolesArray(char[,] _schematic) {
        Vector2[] _holesArray = new Vector2[HowManyPiecesAreInSchematic(_schematic)]; //new array for holes
        int _incrementer = 0; //the incrementer for the hole array

        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) { //for all columns in the schematic
                //if the current index is on a piece
                if (_schematic[i, j] != 'O' && _schematic[i, j] != '#' && _schematic[i, j] != 'W') {
                    _holesArray[(int)_schematic[i, j] - 97] = new Vector2(i, j); //set the current location to a hole location (the position in array depends on the ascii value)
                    _incrementer++; //increment to move to next array index
                }
            }
        }

        return _holesArray; //return the holes array
    }

    //this method slides every piece on the board in a direction
    private bool Slide(char[,] _schematic, char _direction) {
        int _horizontalDisplacement = 0;
        int _verticalDisplacement = 0;
        int _pieceNumber = HowManyPiecesAreInSchematic(_schematic);

        switch (_direction) {
            case 'u': _verticalDisplacement = -1; break;
            case 'd': _verticalDisplacement = 1; break;
            case 'r': _horizontalDisplacement = 1; break;
            case 'l': _horizontalDisplacement = -1; break;
        }

        Vector2[] _startPostitions = new Vector2[_pieceNumber];
        char[] _startPositionChar = new char[_pieceNumber];
        int[] _movementDistances = new int[_pieceNumber];

        for (int i = 0; i < _startPostitions.Length; i++) {
            _startPostitions[i] = new Vector2(999, 999);
        }

        for (int i = 0; i < _movementDistances.Length; i++) {
            _movementDistances[i] = 999;
        }

        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) {
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) {
                if (_schematic[i, j] != 'O' && _schematic[i, j] != '#' && _schematic[i, j] != 'W') { //if the current location has a piece (or a piece ontop of a hole)
                    for (int k = 0; k < _startPostitions.Length; k++) { //for all indexes in the startposition array
                        if (_startPostitions[k] == new Vector2(999, 999)) { //if the index isn't used yet
                            _startPostitions[k] = new Vector2(i, j); //if the current index of startPositions doesnt have a value, give it the current piece coordinates
                            _startPositionChar[k] = _schematic[i, j]; //save the character at the position
                            break; //break
                        }
                    }

                    int _numberOfSpaces = 0; //the number of spaces in between the current piece and the closest immovable object (in the direction to be traveled)
                    int _searchIndex = 1; //this int is used to search x spaces away from the current location (increments per iteration)
                    char _currentChar = ' '; //the value of the current index that is being searched

                    //while a border hasn't been hit
                    while (_currentChar != '#' && _currentChar != 'W') { //while a wall hasn't been hit
                        _currentChar = _schematic[i + (_verticalDisplacement * _searchIndex), j + (_horizontalDisplacement * _searchIndex)]; //traverse the schematic in given direction
                        if (_currentChar == 'O') //if the current char is a hole or a space
                            _numberOfSpaces++; //increment the number of spaces needed to be moved
                        _searchIndex++;
                    }
                    for (int k = 0; k < _movementDistances.Length; k++) {
                        if (_movementDistances[k] == 999) {
                            _movementDistances[k] = _numberOfSpaces;
                            break; //break
                        }
                    }
                }
            }
        }

        bool _isThisMovementDirectionValid = false; //set the movementdirection to invalid if none of the pieces are able to move in that direction

        for (int i = 0; i < _movementDistances.Length; i++) {
            if (_movementDistances[i] > 0) { //if atleast 1 piece can move in this direction
                _isThisMovementDirectionValid = true; //set the direction as movable
                break; //break (to prevent unnecessary checks)
            }
        }

        if (!_isThisMovementDirectionValid) return false; //if the movement direction isn't valid, don't bother proceeding

        else { //if the movement direction is valid, move the piece(s) in the given direction and return true
            //for all pieces on the schematic
            for (int i = 0; i < _startPostitions.Length; i++) {
                //if the target location is a piece, set it to an empty space
                if (_schematic[(int)_startPostitions[i].x, (int)_startPostitions[i].y] != 'W'
                    && _schematic[(int)_startPostitions[i].x, (int)_startPostitions[i].y] != '#'
                    && _schematic[(int)_startPostitions[i].x, (int)_startPostitions[i].y] != 'O')
                    _schematic[(int)_startPostitions[i].x, (int)_startPostitions[i].y] = 'O'; //if a piece is the current index 
            }

            //here's when the movement of pieces actually takes place
            for (int i = 0; i < _startPostitions.Length; i++) { //for all pieces initially found
                if (_movementDistances[i] > 0) { //if the piece is able to move
                    if (_schematic[(int)_startPostitions[i].x + (_movementDistances[i] * _verticalDisplacement), (int)_startPostitions[i].y + (_movementDistances[i] * _horizontalDisplacement)] == 'O')  //if the object in the target coordinates is an empty space
                        _schematic[(int)_startPostitions[i].x + (_movementDistances[i] * _verticalDisplacement), (int)_startPostitions[i].y + (_movementDistances[i] * _horizontalDisplacement)] = _startPositionChar[i]; //fill it with a piece
                }
                else if (_movementDistances[i] == 0) { //if the piece couldn't move
                    _schematic[(int)_startPostitions[i].x, (int)_startPostitions[i].y] = _startPositionChar[i]; //place the char back in its place
                }
            }
            return true;  //return true
        }
    }

    //this method takes a schematic and ensures the level is completable
    public void ValidateBoardSchematic(char[,] _schematic, Vector2[] _holeLocations) {
        Vector2[] _startPositions = new Vector2[HowManyPiecesAreInSchematic(_schematic)]; //this array keeps track of the starting positions to revert back to at the end
        char[] _startCharacters = new char[HowManyPiecesAreInSchematic(_schematic)]; //this array keeps track of the characters that were in each startpostion index
        Vector2[,] _visitedLocations = new Vector2[HowManyPiecesAreInSchematic(_schematic), HowManyPiecesAreInSchematic(_schematic)]; //this array keeps track of the special locations' positions while the level is being validated to make sure the special piece can be in any hole while the other pieces are in holes


        //iterate through the visitedLocations array, making each index a new vector2 of 999
        for (int i = 0; i < _visitedLocations.GetLength(0); i++) { //for the rows in the array
            for (int j = 0; j < _visitedLocations.GetLength(1); j++) //for the columns in the array
                _visitedLocations[i, j] = new Vector2(999, 999);
        }

        int _indexCounter = 0; //to count indexes in the array
        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) { //for all columns in the schematic
                if (_schematic[i, j] != 'W' && _schematic[i, j] != 'O' && _schematic[i, j] != '#') { //if the current index is a piece
                    _startPositions[_indexCounter] = new Vector2(i, j); //save the current position to the vector2 array
                    _startCharacters[_indexCounter] = _schematic[i, j]; //save the character to the array
                    _indexCounter++; //increment
                }
            }
        }

        int _randomSelection; //the int that decides which direction the board is slid in
        char _slideDirection = ' '; //the direction of the slide
        int _shuffleAmount = 1000; //the amount of moves allocated to complete one solution 
        int _validations = 0; //the amount of times all pieces can be proven to end in any hole (out of 1000)
        int _specialValidations = 0; //the amount of times a piece ends in a new hole (out of piece# ^ 2)
        int _solvableValidations = 0; //the amount of times the board has been proven solvable (out of 50)

        for (int i = 0; i < 1000; i++) { //for the amount of times the board is to be proven solvable
            for (int j = 0; j < _shuffleAmount; j++) { //for the amount of moves allowed per proof
                _randomSelection = UnityEngine.Random.Range(0, 4); //pick a random number
                switch (_randomSelection) { //switch statement for direction
                    case 0: _slideDirection = 'u'; break; //moving up
                    case 1: _slideDirection = 'd'; break; //moving down
                    case 2: _slideDirection = 'l'; break; //moving left
                    case 3: _slideDirection = 'r'; break; //moving right
                }
                if (Slide(_schematic, _slideDirection)) { //if a valid direction is given, go on
                    if (AreAllSchematicPiecesInAnyHole(_schematic, _holeLocations)) { //if the board is proven solvable
                        _specialValidations += NumberOfPiecesInNewLocation(_schematic, _visitedLocations);
                        _validations++; //increment the validations 
                        if (AreAllSchematicPiecesInCorrectHole(_schematic, _holeLocations)) _solvableValidations++; //increment  
                        j = _shuffleAmount; //move to last iteration of current proof so next proof can start
                    }
                }
            }
            if (i == 0 && _validations == 0) break; //if the first iteration is unable to be valideated, save time by moving on an scrapping the schematic early
        }

        //Debug.Log("VALIDATIONS: " + _validations);
        //Debug.Log("SPECIAL VALIDATIONS: " + _specialValidations);
        //Debug.Log("SOLVABLE VALIDATIONS: " + _solvableValidations);
        //iterate through the entire schematic, removing all the pieces so they can be placed in their correct locations 
        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) { //for all columns in the schematic
                if (_schematic[i, j] != 'W' && _schematic[i, j] != 'O' && _schematic[i, j] != '#') _schematic[i, j] = 'O'; //if the current location contains a piece switch it to an O
            }
        }

        for (int i = 0; i < _startPositions.Length; i++) { //for all the original start positions 
            _schematic[(int)_startPositions[i].x, (int)_startPositions[i].y] = _startCharacters[i]; //set the character at the coordinates back to its original value 
        }

        //if all the checks were passed (1000 regular validations, #ofpieces ^ 2 special validations, (10% of regular validations) solvable validations)
        if (_validations > 900 && _specialValidations == Mathf.Pow(HowManyPiecesAreInSchematic(_schematic), 2) && _solvableValidations >= (_validations * 0.1f)) { //if the schematic is valid

            //delete any walls that are alone (no other walls surrounding it)
            for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows on the board
                for (int j = 1; j < _schematic.GetLength(1) - 1; j++) { //for all columns on thhe board
                    if (_schematic[i, j] == 'O') { //if the current item is an open area
                        //if there's no open area surrounding the current one, replace it with a wall because it's not an accessible area
                        if (((_schematic[i - 1, j] == 'W') || (_schematic[i - 1, j] == '#')) &&
                            ((_schematic[i + 1, j] == 'W') || (_schematic[i + 1, j] == '#')) &&
                            ((_schematic[i, j - 1] == 'W') || (_schematic[i, j - 1] == '#')) &&
                            ((_schematic[i, j + 1] == 'W') || (_schematic[i, j + 1] == '#')))
                            _schematic[i, j] = 'W';
                    }
                }
            }

            AddSchematicToQueue(_schematic, _holeLocations); //add the schematic that needs to be built to the queue
            //if (_schematicsQueue.Count < 10) { //if there is less than 10 schematics in queue
            //    GenerateSchematicWithSameProperties(_schematic); //generate one
            //}
        }
        else { //if the schematic could not be verified, generate another
            GenerateSchematicWithSameProperties(_schematic); //generate another one with the same properties
        }
    }

    //generate another schematic with the same properties
    private void GenerateSchematicWithSameProperties(char[,] _schematic) {
        //board size
        string _schematicSize; //the size of the shematic
        switch (_schematic.GetLength(0)) { //switch for board size
            case 17: _schematicSize = "tiny"; break; //for tiny board
            case 11: _schematicSize = "small"; break; //for small board
            case 7: _schematicSize = "medium"; break; //for medium board
            case 5: _schematicSize = "large"; break; //for large board
            default: _schematicSize = "medium"; break; //default case (to prevent error)
        }

        //piece count
        int _pieceCount = 0; //to count indexes in the array
        for (int i = 1; i < _schematic.GetLength(0); i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0); j++) { //for all columns in the schematic
                if (_schematic[i, j] != 'W' && _schematic[i, j] != '#' && _schematic[i, j] != 'O') { //if the current index is a pice or a piece on a hole
                    _pieceCount++; //increment the piece count
                }
            }
        }

        //wall count 
        int _wallCount = 0; //the amount of walls in the schematic
        for (int i = 1; i < _schematic.GetLength(0); i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0); j++) { //for all columns in the schematic
                if (_schematic[i, j] == 'W') _wallCount++; //if the current index is a wall, increment
            }
        }

        //generate the schematic
        GenerateLevelSchematic(new Schematic(_schematicSize, _pieceCount, _wallCount));
    }
    #endregion

    #region clearing board

    //this method clears all gameobjects on the board
    public void ClearBoard() {
        ClearHoles(); //destroy all holes in the scene
        ClearPieces(); //destroy all pieces in the scence and remove all pointers to them in array
        ClearWalls(); //destroy all walls in the scene
        _areAllPiecesInCorrectHole = false; //reset to false 
    }

    //this method destroys all walls on the board
    void ClearWalls() {
        GameObject[] _allWalls = GameObject.FindGameObjectsWithTag("wall"); //find all walls and save them to an array
        for (int i = 0; i < _allWalls.Length; i++) { //for each wall in the all walls array destroy current gameobject
            DestroyImmediate(_allWalls[i]); //destroy the current wall
        }
    }

    //this method destroys all holes on the board
    void ClearHoles() {
        GameObject[] _allHoles = GameObject.FindGameObjectsWithTag("hole"); //find all holes and save them to an array
        for (int i = 0; i < _allHoles.Length; i++) { //for each wall in the all walls array destroy current gameobject
            DestroyImmediate(_allHoles[i]); //destroy the current hole
        }
    }

    //this method deletes all pieces and clears all the indicies in the _pieces array
    void ClearPieces() {
        for (int i = 0; i < _pieces.Length; i++) { //for the length of the pieces array
            _pieces[i] = null; //set the current index to null
        }

        GameObject[] _allPieces = GameObject.FindGameObjectsWithTag("piece"); //find all pieces and save them to an array
        for (int i = 0; i < _allPieces.Length; i++) { //for each piece in the piece array
            DestroyImmediate(_allPieces[i]); //destroy the piece
        }
    }

    #endregion

    #region player movement

    //this method takes the user input and converts it into values that can be read by the move method
    private void RegisterTouch(Vector2 startTouch, Vector2 endTouch) {
        float _xDisplacement, _yDisplacement; //initialize the variables of displacement
        _xDisplacement = endTouch.x - startTouch.x; //save the x displacement
        _yDisplacement = endTouch.y - startTouch.y; //save the y displacement

        //if the horizontal swipe OR the vertical swipe surpassed the threshold of movement activation
        if (Mathf.Abs(_xDisplacement) > _minimumSwipeDistance || Mathf.Abs(_yDisplacement) > _minimumSwipeDistance) {

            HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact); //play light impact vibration

            //if the x distance traveled is sufficient enough for a swipe registry (and greater than the y distance traveled)
            if (Mathf.Abs(_xDisplacement) > Mathf.Abs(_yDisplacement)) {
                if (endTouch.x < startTouch.x) //if the touch ends to the left of the start point
                    Move('l'); //move left
                else Move('r'); //move right
            }

            //if the y distance traveled is sufficient enough for a swipe registry (and greater than the x distance traveled)
            else if (Mathf.Abs(_yDisplacement) > Mathf.Abs(_xDisplacement)) {
                if (endTouch.y < startTouch.y) //if the touch ends below the start point
                    Move('d'); //move down
                else Move('u'); //move up
            }
        }
    }

    //this method moves all pieces on the board in the given direction
    private void Move(char _direction) { //this method is called when a swipe is successfully registered, this is the method that makes the puzzle move
        if (_direction == '?') { //for when a random direction is passed
            int _decision = UnityEngine.Random.Range(1, 5); //set the decision to a random number (1-4) inclusive
            switch (_decision) { //switch (for picking a proper char)
                case 1: //up
                    _direction = 'u'; break;
                case 2: //down
                    _direction = 'd'; break;
                case 3: //left
                    _direction = 'l'; break;
                case 4: //right
                    _direction = 'r'; break;
            }
        }

        for (int i = 0; i < HowManyPiecesAreInArray(); i++) { //for all the pieces in the puzzle
            _pieces[i].GetComponent<PuzzlePieceScript>().CanThisPieceBeMoved(_direction); //check if the piece can be moved in the given direction, if it can, move it
        }
    }

    #endregion

    #region board check

    //this method checks if there are any puzzle pieces moving
    private bool AreAnyPiecesMoving() {
        if (HowManyPiecesAreInArray() >= 1) { //if there's atleast 1 piece in the array and the setup is finished
            for (int i = 0; i < HowManyPiecesAreInArray(); i++) { //for all pieces in the puzzle
                if (_pieces[i].GetComponent<PuzzlePieceScript>().IsPieceMoving()) { //if any piece is moving
                    return true; //return true if found piece that's moving
                }
            }
            return false; //if the code has made it here, there is no piece moving so return false
        }
        else return false; //if there is no piece in array and / or the setup hasn't finished
    }

    //this method returns the amount of pieces in the _pieces array
    private int HowManyPiecesAreInArray() {
        int _pieceCount = 0; //initilize the piece counter
        for (int i = 0; i < _pieces.Length; i++) { //for all pieces in the _pieces array
            if (_pieces[i] == null) break; //if found an empty index, break
            _pieceCount++; //increment the piece count
        }
        return _pieceCount; //return the number of pieces
    }

    //this method returns an array containing ONLY pieces (no empty indecies)
    GameObject[] CurrentPieceArray() {
        GameObject[] _currentPieceArray = new GameObject[HowManyPiecesAreInArray()]; //new gameobject array of size HowManyPiecesAreInArray()
        for (int i = 0; i < HowManyPiecesAreInArray(); i++) { //for all pieces in the piece array
            _currentPieceArray[i] = _pieces[i]; //save the piece from the master array to the piece array
        }
        return _currentPieceArray; //return the current piece array
    }

    //this method checks if the level is completed
    private void CheckLevelCompletion() {
        //if all pieces are in the correct hole and no piece is moving
        if (AreAllPiecesInCorrectHole() && !AreAnyPiecesMoving() && _currentGameState == GameState.Ingame) {
            LevelComplete(); //call the level complete method
        }
    }

    //this method updates the level and the corresponding text
    private void UpdateLevel() {
        PlayerPrefs.SetInt("solves", PlayerPrefs.GetInt("level", 1)); //set solves int
        PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 1) + 1); //increment the level
        GlobalGameHandlerScript.Instance.Level = PlayerPrefs.GetInt("level", 1);
        _levelText.text = "LEVEL: " + PlayerPrefs.GetInt("level").ToString(); //set the level text
        //GameServices.Instance.SubmitScore(PlayerPrefs.GetInt("solves"), LeaderboardNames.Solves); //submit score to leaderboards
    }

    //return counter that corresponds with current level
    private Schematic GetNextLevelType() {
        //very hard
        if (_counter.VeryHardCounter == 0) {
            _difficultyText.text = "DIFFICULTY: VERY HARD";
            return _veryHardLevel;
        }
        //very easy
        else if (_counter.VeryEasyCounter == 0) {
            _difficultyText.text = "DIFFICULTY: VERY EASY";
            return _veryEasyLevel;
        }
        //hard
        else if (_counter.HardCounter == 0) {
            _difficultyText.text = "DIFFICULTY: HARD";
            return _hardLevel;
        }
        //easy
        else if (_counter.EasyCounter == 0) {
            _difficultyText.text = "DIFFICULTY: EASY";
            return _easyLevel;
        }
        //normal 
        else {
            _difficultyText.text = "DIFFICULTY: NORMAL";
            return _normalLevel;
        }
    }

    private bool AreAllPiecesInCorrectHole() {
        var _allPieces = GameObject.FindGameObjectsWithTag("piece");
        if (_allPieces.Length > 0) { //if atleast 1 piece exists 
            foreach (GameObject _piece in CurrentPieceArray()) { //for all pieces currently in the scene
                if (!_piece.GetComponent<PuzzlePieceScript>()._isPieceInCorrectHole) return false; //if a single piece isn't in it's correct hole, return false
            }
            return true; //if all checks have been passed without false return, return true
        }
        else return false; //if no pieces exist
    }
    #endregion

    #region schematic functions
    //this method adds the given piece to the _pieces array
    void AddPieceToArray(GameObject _p) {
        for (int i = 0; i < _pieces.Length; i++) { //for the length of the pieces array
            if (_pieces[i] == null) { //if the current index is null
                _pieces[i] = _p; //set the current index to the parameter
                break; //break to prevent piece taking more than 1 slot in array
            }
        }
    }

    //this method removes the schematic from the queue
    private void RemoveSchematicFromQueue() {
        _schematicsQueue.RemoveAt(0);
        _solutionKey.RemoveAt(0);
    }

    //this method returns the next schematic in _schematicQueue then removes it
    private char[,] NextSchematic() {
        return _schematicsQueue[0]; //return the schematic at index i = 0 in the 3d schematic queue array
    }

    //this method returns the next solution for the next schematic in queue then removes it
    private Vector2[] NextSolutionKey() { //return the next solution key
        return _solutionKey[0];
    }

    //this method returns the number of pieces that are in a new location 
    private int NumberOfPiecesInNewLocation(char[,] _schematic, Vector2[,] _visitedLocations) {
        int _numberOfPiecesInNewLocation = 0; //set the int to 0
        int _asciiValue = 97; //the ascii value of 'a'
        int _numberOfPiecesInSchematic = HowManyPiecesAreInSchematic(_schematic); //save this number because each piece has to appear in [HowManyPiecesAreInSchematic] different holes

        for (int i = 0; i < _schematic.GetLength(0); i++) { //for all rows in the array
            for (int j = 0; j < _schematic.GetLength(1); j++) { //for all columns in the array
                if (_schematic[i, j] != 'W' && _schematic[i, j] != '#' && _schematic[i, j] != 'O') { //if the current location contains a piece
                    var _charToAscii = (int)_schematic[i, j]; //cast the 

                    Vector2[] _tempArray = new Vector2[_numberOfPiecesInSchematic];

                    //create a duplicate array of the row to check if the location is already in that row
                    for (int k = 0; k < _numberOfPiecesInSchematic; k++) {
                        _tempArray[k] = _visitedLocations[_charToAscii - _asciiValue, k];
                    }

                    //for the amount of locations to be checked for this piece
                    for (int k = 0; k < _numberOfPiecesInSchematic; k++) {
                        //if the current vector 2 in the array is not the default value, nor Vector2.zero, nor any other vector2 that already exists in the array
                        if ((int)_visitedLocations[_charToAscii - _asciiValue, k].x == 999) {
                            //if the current location isn't already in the array
                            if (!_tempArray.Contains(new Vector2(i, j))) {
                                _visitedLocations[_charToAscii - _asciiValue, k] = new Vector2(i, j); //save the current location of the piece to the array to prevent marking the same location as a new location twice
                                _numberOfPiecesInNewLocation++; //increment for this specific piece
                                break; //break out of this loop
                            }
                        }
                    }
                }
            }
        }
        return _numberOfPiecesInNewLocation; //return the number of pieces in a new location
    }

    private bool AreAllSchematicPiecesInAnyHole(char[,] _schematic, Vector2[] _holeLocations) {
        int _holeValidations = 0; //the amount of hole validations (must be equal to holelocations for this method to return true)

        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows in the schematic
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) { //for all columns in the schematic

                //if the current location is a piece
                if (_schematic[i, j] == 'a' || _schematic[i, j] == 'b' || _schematic[i, j] == 'c' || _schematic[i, j] == 'd' || _schematic[i, j] == 'e' || _schematic[i, j] == 'f' || _schematic[i, j] == 'g' || _schematic[i, j] == 'h' || _schematic[i, j] == 'i') {

                    if (_holeLocations.Contains(new Vector2(i, j))) { //if there is a piece on the current hole
                        _holeValidations++; //increment the hole validations
                    }
                    else { //if there is no hole under the current piece
                        return false; //return false
                    }
                }
            }
        }

        if (_holeValidations == _holeLocations.Length) return true; //if the amount of holes under pieces are the same as the amount of pieces in array return true
        else {
            Debug.Log("IT IS VERY VERY VERY VERY ILLEGAL TO BE SEEING THIS MESSAGE");
            return false; //otherwise, return false
        }
    }

    //this method returns the amount of pieces from a given schematic
    private int HowManyPiecesAreInSchematic(char[,] _schematic) {
        int _numberOfPieces = 0; //this is the number that will be returned at the end of the method
        for (int i = 1; i < _schematic.GetLength(0) - 1; i++) { //for all rows on the board
            for (int j = 1; j < _schematic.GetLength(0) - 1; j++) { //for all columns on the board
                if (_schematic[i, j] != 'O' && _schematic[i, j] != '#' && _schematic[i, j] != 'W') _numberOfPieces++; //if a piece is found at the current array, increment the int
            }
        }
        return _numberOfPieces; //return the number of pieces counted
    }

    //this method returns true if all pieces are in the correct hole
    private bool AreAllSchematicPiecesInCorrectHole(char[,] _schematic, Vector2[] _holeLocations) {
        int _validations = 0;
        for (int i = 0; i < _holeLocations.Length; i++) { //for all the hole locations
            if (_schematic[(int)_holeLocations[i].x, (int)_holeLocations[i].y] == (char)(97 + i)) { //if the location matches up with the piece
                _validations++; //increment validations
            }
            else return false; //if it doesnt match up, return false;
        }
        if (_validations == _holeLocations.Length) return true; //if the validations match up with the hole locations, return true
        else return false; //otherwise, return false
    }

    //this method adds the given schematic to the queue
    private void AddSchematicToQueue(char[,] _schematic, Vector2[] _holeLocations) {
        _schematicsQueue.Add(_schematic); //add the schematic to the queue
        _solutionKey.Add(_holeLocations); //add the hole locations to the solution key list
    }
    #endregion

    //this function is called when the game ends
    private void LevelComplete() {
        //LionAnalytics.LevelComplete(GlobalGameHandlerScript.Instance.Level, GlobalGameHandlerScript.Instance.LevelAttempt); //lion analytics for level complete
        GlobalGameHandlerScript.Instance.ResetLevelAttempt(); //restart the level attempt counter
        _levelText.text = "";
        _difficultyText.text = ""; 
        _endGameCanvas.enabled = true; //enable the endgame canvas
        _inGameCanvas.enabled = false; //disable the ingame canvas
        _settingsCanvas.enabled = false; //disable the settings canvas
        _ps.Play();
        //ClearBoard(); //clear the board
        //LoadSeed(GlobalGameHandlerScript.Instance.Level); //load a new seed
        //Debug.Log("SEED: " + GlobalGameHandlerScript.Instance.Level);
        //GenerateLevelSchematic(GetNextLevelType()); //generate the schematic based on the level\
        SoundManager.PlaySound("win"); //play the win sound
        _currentGameState = GameState.LevelComplete; //set the gamestate to level complete
        UpdateLevel(); //update the level
        _counter.DecrementAllCounters(); //decrement the level counters
    }

    //this function is called when the settings button is pressed
    public void SettingsButtonPressed() {
        PlayButtonSound(); //play the button sound
        _currentGameState = GameState.Paused; //set current gamestate to ingame
        _settingsCanvas.enabled = true; //enable the settings canvas
        _inGameCanvas.enabled = false; //disable the ingame canvas
    }

    //this function is called when the back button (from settings) is pressed
    public void BackButtonPressed() {
        PlayButtonSound(); //play the button sound
        _currentGameState = GameState.Ingame; //set current gamestate to ingame
        _settingsCanvas.enabled = false; //set the settings canvas to true
        _inGameCanvas.enabled = true; //set the ingame canvas to true
    }

    public void MenuButtonPressed() {
        PlayButtonSound(); //play the button sound
        SceneManager.LoadScene(sceneName: "mainMenu");
    }

    //when the continue button is pressed
    public void OnContinueButtonPressed() {
        PlayButtonSound(); //play the button sound
        SceneManager.LoadScene(sceneName: "mainMenu"); //go back to the main menu
    }

    public void OnRestartButtonPressed() {
        //LionAnalytics.LevelRestart(GlobalGameHandlerScript.Instance.Level, GlobalGameHandlerScript.Instance.LevelAttempt);
        PlayButtonSound(); //play the button sound
        _generatingGameBoardCover.SetActive(true);
        SceneManager.LoadScene(sceneName: "ingame");
    }

    //when the next button on the level complete canvas is pressed
    public void OnNextButtonPressed() {
        PlayButtonSound();
        SceneManager.LoadScene("ingame");
    }

    //this method plays the button sound
    void PlayButtonSound() {
        SoundManager.PlaySound("button");
    }
}
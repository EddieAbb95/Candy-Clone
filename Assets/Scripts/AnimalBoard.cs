using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Board Controller for the tile game. May be slightly rigged or confusing. I built this over a two week
/// period then stopped touching it due to being stuck on a glitch. It works good now!
/// </summary>
[RequireComponent(typeof(Camera))]
public class AnimalBoard : MonoBehaviour {
    public const int Width = 5;
    public const int Height = 7;
    private const int spawnYPos = 9;
    private const int MinMatch = 4;

    [SerializeField]
    private GameObject[] animalPrefabs;
    [SerializeField]
    private AnimalSpawner[] animalSpawners;

    private AnimalTile[,] animals;
    private Queue<IntVector2> positionsToCheck;
    private Transform animalStorage;
    private PlayerScore playerScore;
    private bool[] columnNeedsUpdate;
    private bool isBoardReady;
    private bool IsReady { get; set; }

    // Use this for initialization
    void Start () {
        positionsToCheck = new Queue<IntVector2>();
        animalStorage = GameObject.FindGameObjectWithTag("AnimalContainer").transform;
        playerScore = GetComponent<PlayerScore>();
        animals = new AnimalTile[Width, Height];
        columnNeedsUpdate = new bool[Width];
        IsReady = false;

        //Fancily populate board.
        StartCoroutine(PopulateBoard());
	}
	
    void Update() {
        //Don't allow for match checks until game has started.
        if (!IsReady && IsBoardFull()) {
            IsReady = true;

            //Speed things up for re-populating board.
            AnimalTile.lerpSpeed = 0.117f;
        }

        //Squeeze column back together if any need it
        for (int x = 0; x < Width; x++) {
            if (columnNeedsUpdate[x]) {
                UpdateColumn(x);
                columnNeedsUpdate[x] = false;
            }
        }

        //Don't allow multiple matches to occur at once. Pull dat HelloPet trick
        if (IsReady && positionsToCheck.Count > 0)
            CheckForMatches(positionsToCheck.Dequeue());
    }

    /// <summary>
    /// Add a position to the list that needs to be checked for matches.
    /// </summary>
    /// <param name="pos"></param>
    public void AddPositionToCheck(IntVector2 pos) {
        positionsToCheck.Enqueue(pos);
    }

    /// <summary>
    /// Fill the board with random animal tiles.
    /// </summary>
    IEnumerator PopulateBoard() {
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                DropAnimalIntoTable(x, y);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Checks to see if every animal has finally reached its target.
    /// </summary>
    /// <returns></returns>
    private bool IsBoardFull() {
        for(int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                AnimalTile currAnimal = GetAnimal(new IntVector2(x, y));

                if (AnimalTile.IsNullOrMoving(currAnimal))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Call this to tell the board to check for floaters and squeeze
    /// column together accordingly.
    /// </summary>
    /// <param name="x"></param>
    public void FlagColumnForUpdate(int x) {
        if (x >= 0 && x < Width)
            columnNeedsUpdate[x] = true;
    }

    /// <summary>
    /// Squeeze the column back together.
    /// </summary>
    private int UpdateColumn(int x) {
        int emptyY = -1;

        //Start at bottom of column and squeeze it down.
        for(int y = 0; y < Height; y++) {
            IntVector2 currPos = new IntVector2(x, y);
            AnimalTile currAnimal = GetAnimal(currPos);

            //if empty tile, store y coord if none yet.
            if(AnimalTile.IsNullOrMoving(currAnimal)) {
                emptyY = (emptyY == -1) ? y : emptyY;
            }
            //Tile was not empty. If we need to fill a lower one do so.
            else {
                //We have an empty spot to fill.
                if(emptyY != -1) {
                    //Move current animal to last empty slot found.
                    IntVector2 newPos = new IntVector2(x, emptyY);
                    MoveAnimal(currAnimal, newPos);
                    KillAnimalAt(currPos);
                    //Debug.Log("Moving " + currAnimal + " from: " + currPos + " to " + newPos);

                    //If we jumped more than 1 empty slot, set it to current y.
                    int yDelta = emptyY - y;
                    emptyY = (yDelta > 1) ? y : emptyY + 1;
                }
            }
        }

        for (int y = 0; y < Height; y++) {
            IntVector2 currPos = new IntVector2(x, y);
            if (GetAnimal(currPos) == null)
                animalSpawners[x].AddAnimalToSpawnQueue(currPos);
        }

        return emptyY;
    }

    /// <summary>
    /// Set up the lerp of the new animal into the board. If
    /// you want to set the animals coord to a fixed y append and extra
    /// int to the end of the parameters. If not let the game figure it out.
    /// </summary>
    private void DropAnimalIntoTable(int x, int y) {
        IntVector2 endPos = new IntVector2(x, y);
        animalSpawners[x].AddAnimalToSpawnQueue(endPos);
    }

    /// <summary>
    /// Store the inputted animal in the array at the specific
    /// location. Position is checked to ensure it's valid. This
    /// is called from AnimalTile.cs when the lerp position
    /// reaches it's destination.
    /// </summary>
    public bool SetAnimalInBoard(AnimalTile animal, IntVector2 pos) {
        if (IsValidIndex(pos)) {
            animals[pos.x, pos.y] = animal;
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Remove the reference at the inputted position.
    /// </summary>
    /// <param name="pos"></param>
    public void KillAnimalAt(IntVector2 pos) {
        if (IsValidIndex(pos))
            animals[pos.x, pos.y] = null;
    }

    /// <summary>
    /// Checks in every direction around the centerPos for atleast
    /// 4 animal tiles of the same type in a row. Animal rows that are found
    /// are marked matched at the tile level.
    /// </summary>
    /// <param name="centerPos"></param>
    public void CheckForMatches(IntVector2 centerPos) {
        if (!IsValidIndex(centerPos))
            return;
        AnimalTile centerAnimal = GetAnimal(centerPos);

        if (AnimalTile.IsNullOrMoving(centerAnimal)) {
            return;
        }

        //Match count for each direction. 
        int[] matchCounts = { 1, 1, 1, 1, 0, 0, 0, 0 };

        //Count matches found extending out from (x,y) in each direction.
        foreach (SwipeDirection direction in Enum.GetValues(typeof(SwipeDirection))) {
            IntVector2 currPos = GetNextNeighbor(centerPos, direction);
            AnimalTile currAnimal = GetAnimal(currPos);

            //While neighbor in direction is the same as center. Add to count and move to next
            while (!AnimalTile.IsNullOrMoving(currAnimal) && AnimalTile.IsSameType(centerAnimal, currAnimal)) {
                matchCounts[(int)direction]++;

                currPos = GetNextNeighbor(currPos, direction);
                currAnimal = GetAnimal(currPos);
            }
        }

        //Only iterate: North, NorthEast, East, SouthEast. The rest can be considered part of these.
        for (int i = 0; i < 4; i++) {
            IntVector2 currStartPos = new IntVector2(centerPos.x, centerPos.y);
            SwipeDirection currDirection = (SwipeDirection)i;
            int currLength = 0;

            //Center is not start of match
             SwipeDirection invertedDirection = InputController.InvertDirection(currDirection);

            //Find correct start, and how long it really is.
            currStartPos = GetNextNeighbor(currStartPos, invertedDirection, matchCounts[i + 4]);
            currLength += matchCounts[i + 4] + matchCounts[i];

            //If the row is 4 or longer and has an unmatched animal in it. Remove it.
            if (currLength >= MinMatch && DoesRowHaveNonMatched(currStartPos, currLength, currDirection)) {
                IsReady = false;
                //Set them marked in the board. And give player points.
                MarkRowMatched(currStartPos, currLength, currDirection);
                playerScore.AddPoints(currStartPos, currLength, currDirection);
            }
        }
    }

    /// <summary>
    /// Sets all the animals in the row as matched.
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="length"></param>
    /// <param name="rowDirection"></param>
    private void MarkRowMatched(IntVector2 startPos, int length, SwipeDirection rowDirection) {
        IntVector2 currPos = new IntVector2(startPos.x, startPos.y);

        //Iterate through the row setting each animal to matched.
        for (int i = 0; i < length; i++) {
            AnimalTile toMark = GetAnimal(currPos);
            
            if(!AnimalTile.IsNullOrMoving(toMark))
                toMark.State = AnimalState.Matched;

            currPos = GetNextNeighbor(currPos, rowDirection);
        }
    }

    /// <summary>
    /// Checks if any of the animal tiles in the row
    /// are not matched. If so, match them.
    /// </summary>
    private bool DoesRowHaveNonMatched(IntVector2 startPos, int length, SwipeDirection rowDirection) {
        IntVector2 currPos = new IntVector2(startPos.x, startPos.y);
        bool unMatchedFound = false;
        AnimalTile currAnimal;

        for (int i = 0; i < length; i++) {
            currAnimal = GetAnimal(currPos);

            if (!AnimalTile.IsNullOrMoving(currAnimal) && currAnimal.State != AnimalState.Matched)
                unMatchedFound = true;
            currPos = GetNextNeighbor(currPos, rowDirection);
        }

        return unMatchedFound;
    }

    /// <summary>
    /// Switch the animals at the two specific locations. Their
    /// references in the array are set to null until
    /// they both finish lerping to them.
    /// </summary>
    public void SwitchAnimals(IntVector2 posA, IntVector2 posB) {
        AnimalTile animalA = GetAnimal(posA);
        AnimalTile animalB = GetAnimal(posB);

        if (!AnimalTile.IsNullOrMoving(animalA) && !AnimalTile.IsNullOrMoving(animalB)) {
            MoveAnimal(animalA, posB);
            MoveAnimal(animalB, posA);
        }
    }

    /// <summary>
    /// Safe way to retrieve an animal tile from the board. This
    /// will prevent any invalid index errors.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AnimalTile GetAnimal(IntVector2 index) {
        if (IsValidIndex(index)) {
            AnimalTile animal = animals[index.x, index.y];

        if (animal != null)
            return animals[index.x, index.y];
        }
        return null;
    }

    /// <summary>
    /// Move the animal and remove it's reference. That way
    /// during lerping it wont have an incorrect reference.
    /// </summary>
    private void MoveAnimal(IntVector2 animalPos, IntVector2 targetPos) {
        AnimalTile animalToMove = GetAnimal(animalPos);

        if (AnimalTile.IsNullOrMoving(animalToMove))
            return;

        animalToMove.LerpToPosition(targetPos);
        animals[animalPos.x, animalPos.y] = null;
        SetAnimalInBoard(animalToMove, targetPos);
    }

    /// <summary>
    /// Move the animal and remove it's reference. That way
    /// during lerping it wont have an incorrect reference.
    /// </summary>
    private void MoveAnimal(AnimalTile animal, IntVector2 targetPos) {
        if (AnimalTile.IsNullOrMoving(animal))
            return;

        animal.LerpToPosition(targetPos);
        SetAnimalInBoard(animal, targetPos);
    }

    /// <summary>
    /// Check if the index is actually in the board.
    /// </summary>
    private bool IsValidIndex(IntVector2 index) {
        if (index.x >= 0 && index.x < Width && index.y >= 0 && index.y < Height)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Returns the index of the neighbor at the direction
    /// away from the pos.
    /// </summary>
    private static IntVector2 GetNextNeighbor(IntVector2 pos, SwipeDirection direction, int distance = 1) {
        IntVector2 nextPos = new IntVector2(pos.x, pos.y);

        //Figure out x movement.
        switch (direction) {
            case (SwipeDirection.NorthEast):
            case (SwipeDirection.East):
            case (SwipeDirection.SouthEast):
                nextPos.x = nextPos.x + distance;
                break;
            case (SwipeDirection.NorthWest):
            case (SwipeDirection.West):
            case (SwipeDirection.SouthWest):
                nextPos.x = nextPos.x - distance;
                break;
        }
        //Figure out y movement.
        switch (direction) {
            case (SwipeDirection.North):
            case (SwipeDirection.NorthEast):
            case (SwipeDirection.NorthWest):
                nextPos.y = nextPos.y + distance;
                break;
            case (SwipeDirection.South):
            case (SwipeDirection.SouthEast):
            case (SwipeDirection.SouthWest):
                nextPos.y = nextPos.y - distance;
                break;
        }
        return nextPos;
    }

    ///// <summary>
    ///// Returns a random Animal tile.
    ///// </summary>
    //public AnimalTile GenerateRandomTile() {
    //    int type = UnityEngine.Random.Range(0, 5);
    //    GameObject newTile = Instantiate(animalPrefabs[type], Vector3.zero, Quaternion.identity, animalStorage);
    //    AnimalTile newAnimalTile = newTile.GetComponent<AnimalTile>();
    //    newAnimalTile.animalBoard = this;

    //    return newAnimalTile;
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimalType { Parrot, Giraffe, Snake, Hippo, Panda }
public enum AnimalState { Moving, Idle, Matched, }

/// <summary>
/// Animal Tile.
/// 
/// Script to represent a tile on the board. It handles all
/// lerping of movement, what type it is, and what state the tile
/// is currently in.
/// </summary>
public class AnimalTile : MonoBehaviour {
    public AnimalBoard animalBoard { get; set; }

    //State control
    [SerializeField]
    private AnimalType type;
    private AnimalState state;

    //Lerp control
    public static float lerpSpeed = 0.15f;
    private Vector3 startPos;
    private Vector3 endPos;
    private float distance;
    private float lerpStartTime;

    //Properties.
    public AnimalType Type { get { return type; } }
    public AnimalState State { get { return state; } set { state = value; } }

    void Update() {
        //If the tile is matched. EXPLODE IT!
        if(state == AnimalState.Matched) {
            IntVector2 thisPos = new IntVector2(transform.position.x, transform.position.y);
            animalBoard.KillAnimalAt(thisPos);
            animalBoard.FlagColumnForUpdate(thisPos.x);

            Destroy(gameObject);
        }
    }

    //www.blueraja.com/blog/404/how-to-use-unity-3ds-linear-interpolation-vector3-lerp-correctly
    //We do the actual interpolation in FixedUpdate(), since we're dealing with a rigidbody
    void FixedUpdate() {
        if (state == AnimalState.Moving) {
            //We want percentage = 0.0 when Time.time = _timeStartedLerping
            //and percentage = 1.0 when Time.time = _timeStartedLerping + timeTakenDuringLerp
            //In other words, we want to know what percentage of "timeTakenDuringLerp" the value
            //"Time.time - _timeStartedLerping" is.

            //Determine how much time should be taken.
            float lerpTime = lerpSpeed;
            lerpTime *= distance < 0.01f ? 1 : distance;

            float timeSinceStarted = Time.time - lerpStartTime;
            float percentageComplete = timeSinceStarted / lerpTime;

            //Perform the actual lerping.  Notice that the first two parameters will always be the same
            //throughout a single lerp-processs (ie. they won't change until we hit the space-bar again
            //to start another lerp)
            transform.position = Vector3.Lerp(startPos, endPos, percentageComplete);

            //When we've completed the lerp, we set _isLerping to false
            if (percentageComplete >= 1.0f) {
                state = AnimalState.Idle;

                //Check if any matches were made when target is hit.
                //if (animalBoard.IsReady) {
                    IntVector2 pos = new IntVector2((int)endPos.x, (int)endPos.y);
                    animalBoard.AddPositionToCheck(pos);
                //}
            }
        }
    }

    /// <summary>
    /// Set the target position of the animal tile and begin lerping towards it.
    /// Speed is set via isFalling. For dropping into the board set isFalling to true.
    /// </summary>
    public void LerpToPosition(IntVector2 targetPos) {
        IntVector2 beginPos = new IntVector2(transform.position.x, transform.position.y);
        lerpStartTime = Time.time;
        distance = IntVector2.Distance(beginPos, targetPos);

        //Don't lerp for no reason.
        if (distance < 0.0001f) {
            Debug.Log("Saved a lerp");
            return;
        }

        //Store initial position, and store the end position.
        startPos = new Vector3(beginPos.x, beginPos.y, transform.position.z);
        endPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = startPos;

        //Set the Animal tile state. This determines speed of lerp.
        state = AnimalState.Moving;
    }

    /// <summary>
    /// Set the target position of the animal tile and begin lerping towards it.
    /// Speed is set via isFalling. For dropping into the board set isFalling to true.
    /// </summary>
    public void LerpToPosition(IntVector2 beginPos, IntVector2 targetPos) {
        lerpStartTime = Time.time;
        distance = IntVector2.Distance(beginPos, targetPos);

        //Don't lerp for no reason.
        if (distance < 0.0001f) {
            Debug.Log("Saved a lerp");
            return;
        }

        //Store initial position, and store the end position.
        startPos = new Vector3(beginPos.x, beginPos.y, transform.position.z);
        endPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        transform.position = startPos;

        //Set the Animal tile state. This determines speed of lerp.
        state = AnimalState.Moving;
    }

    /// <summary>
    /// Gives the name of the object in string form.
    /// </summary>
    public override string ToString() {
        return Type.ToString() + " " + State.ToString() + " at: " + transform.position;
    }

    /// <summary>
    /// Checks if the animalTile is moving or null.
    /// </summary>
    public static bool IsNullOrMoving(AnimalTile animal) {
        if (animal == null || animal.state == AnimalState.Moving)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if two animals share the same type.
    /// </summary>
    /// <returns></returns>
    public static bool IsSameType(AnimalTile a, AnimalTile b) {
        if (a == null || b == null)
            return false;

        if (a.Type == b.Type)
            return true;
        else
            return false;
    }
}

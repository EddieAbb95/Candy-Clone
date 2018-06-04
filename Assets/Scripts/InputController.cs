using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwipeDirection { North = 0, NorthEast = 1, East = 2, SouthEast = 3, South = 4, SouthWest = 5, West = 6, NorthWest = 7}
public class InputController : MonoBehaviour {
    private Camera gameCamera;
    private AnimalBoard animalBoard;

    //Mouse control
    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private AnimalTile animalHit;

    private bool isClicking;
    // Use this for initialization
    void Start () {
        gameCamera = GetComponent<Camera>();
        animalBoard = GetComponent<AnimalBoard>();

        isClicking = false;
    }

    // Update is called once per frame
    void Update() {
        //If mouse was clicked.
        if (Input.GetMouseButtonDown(0)) {
            isClicking = true;
            startMousePos = new Vector2(gameCamera.ScreenToWorldPoint(Input.mousePosition).x, gameCamera.ScreenToWorldPoint(Input.mousePosition).y);

            //Shoot a ray out from cursor.
            RaycastHit2D hit = Physics2D.Raycast(startMousePos, new Vector2(), 0f);

            //If not animal was hit. End the click so nothing funky happens.
            if (hit.collider != null)
                animalHit = hit.collider.GetComponent<AnimalTile>();
            else
                isClicking = false;
        }

        //Mouse was released.
        if (Input.GetMouseButtonUp(0)) {
            endMousePos = new Vector2(gameCamera.ScreenToWorldPoint(Input.mousePosition).x, gameCamera.ScreenToWorldPoint(Input.mousePosition).y);

            //User is finishing up a click.
            if (isClicking) {
                //Prevent double click from doing anything
                float distance = Vector2.Distance(startMousePos, endMousePos);
                if (distance < 0.1f) {
                    isClicking = false;
                    return;
                }

                //Get direction from clicks, and figure out where it started.
                float swipeAngle = AngleBetweenVector2(animalHit.transform.position, endMousePos);
                SwipeDirection swipeDirection = GetSwipeDirection(swipeAngle);

                IntVector2 startIndex = new IntVector2(animalHit.transform.position.x, animalHit.transform.position.y);

                //Verify it was valid. Get end swipe position and switch the two.
                if (IsValidMove(startIndex, swipeDirection)) {
                    IntVector2 endIndex = GetNeighborPos(startIndex, swipeDirection);
                    animalBoard.SwitchAnimals(startIndex, endIndex);
                }
            }
            isClicking = false;
        }
    }

    /// <summary>
    /// Get the angle between two vector2s.
    /// </summary>
    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2) {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;

        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    /// <summary>
    /// Returns the direction of the mosue swipe in terms of cardinal directions.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private SwipeDirection GetSwipeDirection(float angle) {
        //North
        if (angle > 67.5f && angle <= 135.0f)
            return SwipeDirection.North;
        //NorthEast
        if (angle > 22.5f && angle <= 67.5f)
            return SwipeDirection.NorthEast;
        //East
        if (angle > -22.5f && angle <= 22.5f)
            return SwipeDirection.East;
        //SouthEast
        if (angle > -67.5f && angle <= -22.5f)
            return SwipeDirection.SouthEast;
        //South
        if (angle > -112.5f && angle <= -67.5f)
            return SwipeDirection.South;
        //SouthWest
        if (angle > -157.5f && angle <= -112.5f)
            return SwipeDirection.SouthWest;
        //West
        if ((angle > 157.5f && angle <= 180.0f) || (angle >= -180 && angle <= -157.5f))
            return SwipeDirection.West;
        //NorthWest
        if (angle > 112.5f && angle <= 157.5f)
            return SwipeDirection.NorthWest;

        Debug.Log("Angle error hit: " + angle);
        return SwipeDirection.North;
    }

    /// <summary>
    /// Returns the neighbor position at the direction away
    /// from the start pos.
    /// </summary>
    /// <returns></returns>
    private IntVector2 GetNeighborPos(IntVector2 startPos, SwipeDirection direction) {
        IntVector2 neighborPos = new IntVector2(startPos.x, startPos.y);

        //Figure out y movement.
        switch (direction) {
            case (SwipeDirection.North):
            case (SwipeDirection.NorthEast):
            case (SwipeDirection.NorthWest):
                neighborPos.y += 1;
                break;
            case (SwipeDirection.South):
            case (SwipeDirection.SouthEast):
            case (SwipeDirection.SouthWest):
                neighborPos.y -= 1;
                break;
        }

        //Figure out x movement.
        switch (direction) {
            case (SwipeDirection.NorthEast):
            case (SwipeDirection.East):
            case (SwipeDirection.SouthEast):
                neighborPos.x += 1;
                break;
            case (SwipeDirection.NorthWest):
            case (SwipeDirection.West):
            case (SwipeDirection.SouthWest):
                neighborPos.x -= 1;
                break;
        }

        return neighborPos;
    }

    /// <summary>
    /// Was it a legal swipe.
    /// </summary>
    /// <returns></returns>
    private bool IsValidMove(IntVector2 pos, SwipeDirection direction) {
        if (pos.x == AnimalBoard.Width - 1 &&
            (direction == SwipeDirection.NorthEast ||
            direction == SwipeDirection.East ||
            direction == SwipeDirection.SouthEast))
            return false;

        if (pos.x == 0 &&
            (direction == SwipeDirection.NorthWest ||
            direction == SwipeDirection.West ||
            direction == SwipeDirection.SouthWest))
            return false;

        if (pos.y == AnimalBoard.Height - 1 &&
            (direction == SwipeDirection.North ||
            direction == SwipeDirection.NorthEast ||
            direction == SwipeDirection.NorthWest))
            return false;

        if (pos.y == 0 &&
            (direction == SwipeDirection.South ||
            direction == SwipeDirection.SouthEast ||
            direction == SwipeDirection.SouthWest))
            return false;

        return true;
    }

    /// <summary>
    /// Returns the exact oppposite swipe direction.
    /// </summary>
    public static SwipeDirection InvertDirection(SwipeDirection regular) {
        SwipeDirection inverted = (int)regular < 4 ? (regular + 4) : (regular - 4);
        return inverted;
    }

}

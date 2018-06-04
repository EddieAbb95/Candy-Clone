using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour {
    [SerializeField]
    private GameObject[] animalPrefabs;
    private Transform animalStorage;
    private AnimalBoard animalBoard;

    private Queue<AnimalBluePrint> animalsToSpawn;
    private IntVector2 targetPos;
    private float spawnTimer;
    private float spawnWaitTime;

    void Awake () {
        animalsToSpawn = new Queue<AnimalBluePrint>();
        targetPos = new IntVector2(transform.position.x, 0.0f);
        spawnTimer = 0.0f;
        spawnWaitTime = 0.15f;
    }

	// Use this for initialization
	void Start () {
        animalStorage = GameObject.FindGameObjectWithTag("AnimalContainer").transform;
        animalBoard = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AnimalBoard>();
	}
	
	// Update is called once per frame
	void Update () {
        spawnTimer += Time.deltaTime;

        if(spawnTimer >= spawnWaitTime && animalsToSpawn.Count > 0) {
            spawnTimer = 0.0f;
            SpawnAnimal();
        }
	}

    /// <summary>
    /// Call this to add an animal to the wait count.
    /// </summary>
    public void AddAnimalToSpawnQueue(IntVector2 targetPos) {
        AnimalBluePrint newAnimal = new AnimalBluePrint(targetPos);
        animalsToSpawn.Enqueue(newAnimal);
    }

    

    /// <summary>
    /// Spawns a new gameobject and stores its reference in the board.
    /// </summary>
    private void SpawnAnimal() {
        AnimalBluePrint bluePrint = animalsToSpawn.Dequeue();
        GameObject newTile = Instantiate(animalPrefabs[(int)bluePrint.Type], this.transform.position, Quaternion.identity, animalStorage);
        AnimalTile newAnimalTile = newTile.GetComponent<AnimalTile>();

        //Start lerping towards it's target, and store it's reference in the board.
        newAnimalTile.animalBoard = animalBoard;
        newAnimalTile.LerpToPosition(bluePrint.TargetPos);
        animalBoard.SetAnimalInBoard(newAnimalTile, bluePrint.TargetPos);
    }

}

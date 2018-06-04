using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    //Singleton instance
    static SoundManager instance;

    public static SoundManager GetInstance() {
        if(instance == null) {
            GameObject soundManager = new GameObject("SoundManager");
            instance = soundManager.AddComponent<SoundManager>();
            
        }

        return instance;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    World world;
    Text text;
    float frameRate = 0;
    float timer = 0;
	void Start () {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
	}
	
	void Update () {
        string debugText = "VR / Space / Minecraft";
        debugText += "\n";
        debugText += frameRate + " : FPS";
        debugText += "\n";
        debugText += "XYZ : " + Mathf.FloorToInt(world.Player.transform.position.x) + " / "+ Mathf.FloorToInt(world.Player.transform.position.y) + " / " + Mathf.FloorToInt(world.Player.transform.position.z);
        debugText += "\n";

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
	}
}

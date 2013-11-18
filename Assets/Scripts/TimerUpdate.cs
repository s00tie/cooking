﻿using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
/*The bar goes right to left, if you want it in the other direction, just change curTime to 0 and increase the time while adjusting  */
public class TimerUpdate : MonoBehaviour {
public int maxTime = 100;
public float curTime = 100;
public float timeBarLength;
float GUIWidth; 
public Texture2D background;  
public Texture2D foreground;
public Texture2D lion;
Rect pivot;
public float pivotTime = 0f;
Rect backgroundRect;
Rect timeRec;
public float step = 0f;
public float offsetY = 0f;
private GameState gameState = null;
private MovementScripts movement = null;
    public TimerUpdate()
    {
      
    }
    void Start () 
    {
		GUIWidth = maxTime;
		gameState = (GameState)GameObject.FindObjectOfType(typeof(GameState));
		movement = (MovementScripts)GameObject.FindObjectOfType(typeof(MovementScripts));
    }


    public enum ResponseType
    {
        perfect,
        missed
    }
 
    void Update () 
    {
        AddjustCurrentTime(step);        //This can allow you to have differernt time steps for the change. 
        if(Input.GetKeyDown(KeyCode.F))
            Debug.Log(Check().ToString());
    }

    private ResponseType Check()
    {
        if (pivot.Contains(new Vector2(timeRec.x + timeRec.width, pivot.y + pivot.height / 2)))      //If the current time is in perfect time range
        {
            return ResponseType.perfect;
        }
        else
            return ResponseType.missed;
    }
    
    void OnGUI()
    {
        timeRec = new Rect(Screen.width - GUIWidth, 10 + offsetY, timeBarLength, 20);
        pivot = new Rect(Screen.width - GUIWidth + pivotTime, 10 + offsetY, 5, 20);
        backgroundRect = new Rect(Screen.width - GUIWidth, 10 + offsetY, GUIWidth, 20);
        GUI.DrawTexture(backgroundRect, background);
        GUI.DrawTexture(timeRec, foreground, ScaleMode.StretchToFill, false);
        GUI.DrawTexture(pivot, lion);
    }

    public void AddjustCurrentTime(float adj)
    {
	if (movement.IsRelativistic) {
		adj *= 1 - (float)(gameState.PlayerVelocity / gameState.totalC);
		adj /= 5.0f;
		Debug.Log(adj);
	}

        curTime += adj;
        if (curTime < 0)
		{
			curTime = 0;
			
		}
        if (curTime > maxTime)
            curTime = maxTime;
        if (maxTime < 1)
            maxTime = 1;
        timeBarLength = GUIWidth * (curTime / (float)maxTime); 
    }
 
}

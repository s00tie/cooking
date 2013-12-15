﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MainGameEventScheduler : MonoBehaviour {
    public static task currentTask = 0;
    public static List<Vector3> playerPositions = new List<Vector3>();
    public GameObject torchPrefab;
    public static MainGameEventScheduler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindGameObjectWithTag("Globals").GetComponent<MainGameEventScheduler>();
            }
            return instance;
        }
    }
    public static MainGameEventScheduler instance = null;
    
    // Use this for initialization
	void Start () {
		ToastScript.isActive = true;
        currentTask = task.toaster;
	}
	
	// Update is called once per frame
	void Update () {
		//EggScript.isActive = true;
	}
	
	public void FailedObjective()
	{

        GameObject torch = (GameObject)Instantiate(torchPrefab, GameObject.Find("TorchPlaceHolder").transform.position, Quaternion.identity);
        torch.transform.parent = GameObject.FindGameObjectWithTag("Playermesh").transform;
		LoadAgain();
		GameObject.FindGameObjectWithTag("Playermesh").transform.position = playerPositions[playerPositions.Count - 1];
	}
	
    public static void LoadAgain()
    {
		GUIManager.Instance.RemoveAllObjectives();
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		player.GetComponent<OverheatMeter>().reset();
		player.GetComponent<MovementScripts>().ToggleSpecialRelativity(true, false);
		
		switchTask(true);
    }
    public static void switchTask(bool resetFromFailure = false)
    {
        if ((int)currentTask < (int)task.none - 1)
        {
			if (!resetFromFailure) {
           		currentTask++;
			}
        }
        switch ((int)currentTask)
        {
            case 0: DisableAllEventScripts();
                ToastScript.isActive = true;
				ToastScript.Instance.StartTask();
				GUIManager.message = "Heat stove and grab egg";
				return;
                break;
            case 1: DisableAllEventScripts();
				EggScript.isActive = true;
				EggScript.Instance.StartTask();
				return;
                break;
            case 2: DisableAllEventScripts();
                OmeletteScript oScript = GameObject.Find("Objectives").GetComponent<OmeletteScript>();
                oScript.Initialize();
                OmeletteScript.isActive = true;
				return;
                break;
        }
    }

    private static void DisableAllEventScripts()
    {
        ToastScript.isActive = false;
		EggScript.isActive = false;
		OmeletteScript.isActive = false;
    }
}


public enum task
{
    toaster = 0, 
    eggs = 1,
    omlette = 2,
    //Quiche = 3,
    none = 3
}

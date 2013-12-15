using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class ToastScript : MonoBehaviour {
	public bool breadAcquired = false;
	private List<GameObject> breads = new List<GameObject>();
	private TimerUpdate toastTimer = null;
	TimerManager timerManager = null;
	GameObject player = null;
	GameObject toaster = null;
	public static List<GameObject> destroyObjects = new List<GameObject>();
	public static bool isActive = false;
	public AudioSource success = null;
	public AudioSource failure = null;
	public Objective grabToast = new Objective("grab toast", "Grab 100% Toast (F)");
	public Objective toastToast = new Objective("toast toast", "Toast the Toast in the Toastatron (F)");
	public Objective finishToast = new Objective("finish toast", "Save the Toast!!! Patience (F)");
	
	// Use this for initialization
	void Start () {
		
		timerManager = GameObject.FindGameObjectWithTag("Globals").GetComponent<TimerManager>();
		player = GameObject.FindGameObjectWithTag("Player");
		toaster = GameObject.FindGameObjectWithTag("Toaster");
		//toastTimer = GameObject.Find("Toaster").GetComponent<TimerUpdate>();
		//toastTimer.AddTimee(this);
	}
	
	// Update is called once per frame
    void Update()
    {
		if (toastTimer == null) {
			toastTimer = timerManager.FindTimer("Toaster");
			toastTimer.AddTimee(this);
		}
		
        if (isActive)
        {
            breads = GameObject.FindGameObjectsWithTag("Bread").ToList();
            foreach (var bread in breads)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    if (Vector3.Distance(bread.transform.position, player.transform.position) < 5)
                    {
						bread.SetActive(false);
						destroyObjects.Add(bread);
                        break;
                    }
                }
            }
			if (breads.Count < 1) {
				breadAcquired = true;
				GUIManager.Instance.RemoveObjective(grabToast.name);
			}

            

            if (toastTimer.IsActive)
            {
                if (Vector3.Distance(toaster.transform.position, player.transform.position) < 5)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (toastTimer.AttemptSuccess(null, null, success, failure)) {
                            MainGameEventScheduler.switchTask();
								foreach(var obj in destroyObjects)
									Destroy(obj);
								destroyObjects.Clear();
						}
                    }
                }
                return;
            }
            else
                if (Vector3.Distance(toaster.transform.position, player.transform.position) < 5)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (breads.Count == 0)
                        {
                            if (breadAcquired)
                            {
								toastTimer.StartTimer();
                                GUIManager.message = "Shut the toaster before the bread burns. Press 'f' to shut the toster. Make sure you shut it at the right time, else the bread wont toast well";
                                return;
                            }
                        }
                        else
                        {
                            GUIManager.message = "Grab all breads, before you turn the toaster ON";
                        }
                    }
                }
                else
                {
                    //GUIManager.message = "Find breads and toast them";
                }
        }

    }
	
	public void TimerUpdate(TimerStep step) {
		if (step.name.Equals("Toaster")) {
			
		}
	}
	
	public void ControlTimerEnd(string timerName) {
		if (timerName.Equals("Toaster")) {
			
		}
	}

	public static void LoadFromDestroy()
	{
		foreach(var obj in destroyObjects)
		{
			obj.SetActive(true);
			obj.renderer.enabled = true;
		}
		destroyObjects.Clear();
	}
}

﻿using UnityEngine;
using System.Collections;

public class Door1 : MonoBehaviour {
	
	public float movespeed = 0.5f;

	// Use this for initialization
	void Start () {

	
	
	}
	
	// Update is called once per frame
	void Update () {
		Decontamination decontaminationScript = GameObject.Find("Decontamination Trigger").GetComponent<Decontamination>();
		KitchenDoor kitchenScript = GameObject.Find("Kitchen Trigger").GetComponent<KitchenDoor>();
		if(decontaminationScript.opened){
			//gameObject.audio.Play();
			GUIManager.Instance.RemoveObjective(decontaminationScript.pleaseWait.name);
			StartCoroutine(doormove());
		}
	
	
	}
	
	
	IEnumerator doormove (){
	
		transform.Translate(Vector3.right * movespeed * Time.deltaTime);
		yield return new WaitForSeconds(50f);
		transform.Translate(Vector3.zero * movespeed * Time.deltaTime);
	}
	
}
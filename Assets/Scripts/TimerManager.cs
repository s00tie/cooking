using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimerManager : MonoBehaviour
{
	List<TimerUpdate> timers = new List<TimerUpdate>();
	
	void Start() {
		AddTimer(gameObject.GetComponent<TimerUpdate>());
	}
	
	public void AddTimer(TimerUpdate timer) {
		timers.Add(timer);
	}
	
	public void RemoveTimer(TimerUpdate timer) {
		timers.Remove(timer);
	}
	
	public void RemoveTimer(string timerName) {
		RemoveTimer(FindTimer("timeName"));
	}
	
	public TimerUpdate FindTimer(string name) {
		TimerUpdate foundTimer = null;
		foreach(TimerUpdate timer in timers) {
			if (timer.name.Equals(name)) {
				foundTimer = timer;	
			}
		}
		return foundTimer;
	}
}

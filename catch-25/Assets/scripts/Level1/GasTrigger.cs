using UnityEngine;
using System.Collections;
using System;


public class GasTrigger : MonoBehaviour
{
	public int secs2Wait = 3;
	GameObject GasCombo;
	public int[] totalSec = new int[3];
	public static event Action<bool> OnDeath = (bool newClone) => {};
	void OnTriggerEnter2D(Collider2D col) 
	{
		if (col.gameObject.CompareTag("Player") )
		{
			InvokeRepeating("GasInvoke",1,1);
		}
		if (col.gameObject.CompareTag ("DeadCat"))
						GasCombo = GameObject.FindWithTag ("Gas");  
		Destroy (GasCombo.gameObject);
	}
	void GasInvoke()
	{
		if (totalSec [0] < secs2Wait)
			totalSec [0]++;
		else
			OnDeath(true);
		AudioManager.Instance.PlayAudio (Utility.SOUND_SPIKE_IMPALE, 1.0f, 1.0f);
		CancelInvoke ("GasInvoke");
		
		
	}

}


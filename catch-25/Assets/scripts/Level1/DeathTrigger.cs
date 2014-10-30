using UnityEngine;
using System.Collections;
using System;


public class DeathTrigger : MonoBehaviour {
	public int secs2Wait = 3;
	public int[] totalSec = new int[3];
	GameObject GasCombo;

	public static event Action<bool> OnDeath = (bool newClone) => {};
    private Action<Collision2D> OnCollision = (Collision2D) => { };
	
	[SerializeField] private EnvironmentItem environment;
	void GasInvoke()
	{
				if (totalSec [0] < secs2Wait)
						totalSec [0]++;
				else
			OnDeath(true);
		AudioManager.Instance.PlayAudio (Utility.SOUND_SPIKE_IMPALE, 1.0f, 1.0f);
						CancelInvoke ("GasInvoke");
						

		}

	void Start()
	{
		switch(environment)
		{
			case EnvironmentItem.FallingRock:
                OnCollision = FallingRockCollision;
			    break;
            case EnvironmentItem.Spikes:
                OnCollision = SpikeCollision;
                break;
            case EnvironmentItem.DeathZone:
                OnCollision = DeathZoneCollision;
                break;
			case EnvironmentItem.Gas:
				OnCollision = GasCollision;
				break;
			case EnvironmentItem.SkullDoor:
				OnCollision = SkullCollision;
				break;
		}
	}
	void OnTriggerEnter2D(Collider2D col) 
	{
				if (col.gameObject.CompareTag ("Player")) {
						InvokeRepeating ("GasInvoke", 1, 1);
				}
				
		}
	void OnCollisionEnter2D(Collision2D col)
	{
        OnCollision(col);
	}
	void GasCollision(Collision2D col)
	{
		if (col.gameObject.CompareTag ("Player")) {
			InvokeRepeating("GasInvoke",1,1);
			//OnDeath (true);
			//AudioManager.Instance.PlayAudio (Utility.SOUND_SQUISHED, 1.0f, 1.0f);
				}
	}
	void SkullCollision(Collision2D col)
	{
		if (col.gameObject.CompareTag ("Player")) {


			OnDeath (true);
			AudioManager.Instance.PlayAudio (Utility.SOUND_SQUISHED, 1.0f, 1.0f);
		}
	}
    void FallingRockCollision(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && rigidbody2D.isKinematic == false)
        {
            OnDeath(true);
            AudioManager.Instance.PlayAudio(Utility.SOUND_SQUISHED, 1.0f, 1.0f);
        }
        if (col.gameObject.CompareTag("DeadCat"))
        {
            col.gameObject.collider2D.enabled = false;         
        }
    }
    void SpikeCollision(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
		

          OnDeath(true);
           AudioManager.Instance.PlayAudio(Utility.SOUND_SPIKE_IMPALE,1.0f,1.0f);
        }

    }
    void DeathZoneCollision(Collision2D col) 
    {
        if (col.gameObject.CompareTag("Player")) 
        {
			OnDeath(false);
			AudioManager.Instance.PlayAudio(Utility.SOUND_SQUISHED,1.0f,1.0f);
        }
    }
    void OnDestroy() 
    {
        OnDeath = null;
    }
}

public enum EnvironmentItem
{
	FallingRock, Spikes, DeathZone, Gas, SkullDoor
}



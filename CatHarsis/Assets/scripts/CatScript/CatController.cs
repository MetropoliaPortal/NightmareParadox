﻿using UnityEngine;
using System.Collections;

/// <summary>
/// CatController is attached to the cat. It is the higher layer of the cat object and the only component to communicate 
/// with the other cat component and the component outside the cat object.
/// It is of type StatefulMonobehaviour and controls the states of the cat. 
/// It also communicates witht he GameHandler to update the game state, mainly if the cat is dead. 
/// </summary>
public class CatController : StatefulMonobehaviour 
{
	[SerializeField] private CatMove catMoveRef;
	[SerializeField] private CatHealth catHealth;
    [SerializeField] private CatInventory catInventory;
	[SerializeField] private GameObject catPrefab;
    [SerializeField] private Transform jellyPosition;
    [SerializeField]
    private GameHandler gameHandler;
    [SerializeField]
    private float resetTimer = 1.0f;
     

    private SpriteRenderer spriteRenderer;
    private int coins;
    float timer = 1f;

	CatDeath catDeath = CatDeath.None;
	void Awake () 
	{
        gameHandler = FindObjectOfType<GameHandler>();

        if (gameHandler != null)
        {
            gameHandler.OnChangeState += OnGameHandlerChangeState;
        }
        DeathTrigger.OnDeath += ResetOnDeath;

        catInventory.OnAddSoul += CheckCoinForExtraLife;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        InitializeStateMachine(true);
        AddStateWithTransitions(Utility.STATE_STARTING, new string[]{Utility.STATE_PLAYING});
        AddStateWithTransitions(Utility.STATE_PLAYING, new string[] { Utility.STATE_PAUSE, Utility.STATE_RESET,Utility.STATE_POISONED, Utility.STATE_DEAD });
        AddStateWithTransitions(Utility.STATE_RESET, new string[]{Utility.STATE_PLAYING, Utility.STATE_PAUSE});
        AddStateWithTransitions(Utility.STATE_DEAD, new string[] { Utility.STATE_STARTING});
        AddStateWithTransitions(Utility.STATE_PAUSE, new string[]{Utility.STATE_PLAYING, Utility.STATE_RESET});
        AddStateWithTransitions(Utility.STATE_POISONED, new string[] { Utility.STATE_RESET});
        RequestState(Utility.STATE_STARTING);   
	}
    
    void Update()
    {
        StateUpdate();
        if (CurrentState == Utility.STATE_STARTING)
        {
            return;
        }
    }

    public void PoisonCat()
    {
        if (CurrentState == Utility.STATE_POISONED) 
        {
            return;
        }
        RequestState(Utility.STATE_POISONED);
    }

    protected virtual void EnterPlaying(string oldState)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(Utility.SOUND_RESPAWN, 1.0f, 1.0f);
        }
        catMoveRef.enabled = true;
        spriteRenderer.color = Color.white;
        SuscribeControl();
    }
    // Exiting playing we unsuscribe the control so that Pause or death disable controls

    protected virtual void ExitPlaying(string nextState)
    {
        if (nextState != Utility.STATE_POISONED)
        {
            UnsuscribeControl();
        }
    }
    // Entering the reset state, collider is disabled to avoid multiple collision
    // timer is set and control are unregistered
    protected virtual void EnterReset(string oldState) 
    {
        collider2D.enabled = false;
        rigidbody2D.isKinematic = true;
        catMoveRef.enabled = false;
        timer = resetTimer;
        UnsuscribeControl();
    }

    protected virtual void UpdateReset() 
    {
        // Decrease timer until 0
        timer -= Time.deltaTime;
        if (timer <= 0.0f) 
        {
            RequestState(Utility.STATE_PLAYING);
        }
    }
    protected virtual void ExitReset(string nextState)
    {
        // if we require a dead corpse
        if (catDeath != CatDeath.Fall)
        {
            GameObject obj = (GameObject)Instantiate(catPrefab, transform.position, Quaternion.identity);
			obj.GetComponent<DeadCatController>().InitDeadCat(catDeath, transform.localScale.x);
            
        }
        // Place on the jelly
        Vector3 position = jellyPosition.position;
        position.y -= 0.5f;
        transform.position = position;
        // Reset the CatMove component
        catMoveRef.ResetToPlay();
        // Set the collider and rigidbody
        collider2D.enabled = true;
        rigidbody2D.isKinematic = false;
    }
    protected virtual void EnterPoisoned(string oldState)
    {
        timer = 3f;
    }
    protected virtual void UpdatePoisoned()
    {
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, new Color(0.1f,0.6f,0.1f,1.0f), Time.deltaTime);
        timer -= Time.deltaTime;
        if (timer <= 0.0f)
        {
            ResetOnDeath(CatDeath.Gas);
        }
    }
    private void OnGameHandlerChangeState(string newState)
    {
        if (newState == Utility.GAME_STATE_PLAYING)
        {
            RequestState(Utility.STATE_PLAYING);
        }
        else if (newState == Utility.GAME_STATE_PAUSE || newState == Utility.GAME_STATE_GAMEWON)
        {
            RequestState(Utility.STATE_PAUSE);
        }
        else if (newState == Utility.GAME_STATE_GAMELOST)
        {
            RequestState(Utility.STATE_DEAD);
        }
    }

    private void SuscribeControl() 
    {
        InputController.Instance.OnMovement += catMoveRef.Move;
        InputController.Instance.OnJump += catMoveRef.Jump;
    }
    private void UnsuscribeControl() 
    {
#if UNITY_EDITOR
        InputController.Instance.OnMovement -= catMoveRef.Move;
        InputController.Instance.OnJump -= catMoveRef.Jump;
#endif
    }

    private void CheckCoinForExtraLife(int coin) 
    {
        coins += coin;
        if (coins > 10)
        {
            coins -= 10;
            catHealth.IncreaseHealth();
        }
    }

    private void ResetOnDeath(CatDeath catDeath)
    {
        catMoveRef.ResetOnDeath();
        int life = catHealth.DecreaseHealth();
		this.catDeath = catDeath;
        if (life >= 0)
        {
            RequestState(Utility.STATE_RESET);
        }
        else
        {
            gameHandler.RequestStateHandler(Utility.GAME_STATE_GAMELOST);
        }
    } 
}

using UnityEngine;
using System.Collections;

public class CatController : StatefulMonobehaviour 
{
	[SerializeField] private CatMove catMoveRef;
	[SerializeField] private CatHealth catHealth;
    [SerializeField] private CatInventory catInventory;
	[SerializeField] private GameObject catPrefab;
    [SerializeField] private Transform jellyPosition;

    private InputManager inputManager;

	void Awake () 
	{
        inputManager = FindObjectOfType<InputManager>();
        FindObjectOfType<GameHandler>().OnChangeState += OnGameHandlerChangeState;
        DeathTrigger.OnDeath += DeadCatClone;
		GasTrigger.OnDeath += DeadCatClone;
        catInventory.OnAddCoin += CheckCoinForExtraLife;

        InitializeStatefulness(true);
        AddStateWithTransitions(Utility.STATE_STARTING, new string[]{Utility.STATE_PLAYING});
        AddStateWithTransitions(Utility.STATE_PLAYING, new string[] { Utility.STATE_PAUSE, Utility.STATE_RESET, Utility.STATE_DEAD });
        AddStateWithTransitions(Utility.STATE_RESET, new string[]{Utility.STATE_PLAYING, Utility.STATE_PAUSE});
        AddStateWithTransitions(Utility.STATE_DEAD, new string[] { Utility.STATE_STARTING});
        AddStateWithTransitions(Utility.STATE_PAUSE, new string[]{Utility.STATE_PLAYING, Utility.STATE_RESET});      
        RequestState(Utility.STATE_STARTING);
        
	}
    protected virtual void Update() 
    {
        StateUpdate();
        if (CurrentStateName == Utility.STATE_STARTING)
        { 
            return; 
        }
    }
    protected virtual void EnterStatePlaying(string oldState)
    {
        AudioManager.Instance.PlayAudio(Utility.SOUND_RESPAWN,1.0f,1.0f);
        SuscribeControl();
    }
    protected virtual void ExitStatePlaying(string oldState)
    {
        UnsuscribeControl();
    }
    protected virtual void EnterStateReset(string oldState) 
    {
        catHealth.DecreaseHealth();
        collider2D.enabled = false;
        rigidbody2D.isKinematic = true;
        resetTimer = 1.5f;
    }

    float resetTimer = 1.5f;
    bool clone = false;
    protected virtual void UpdateReset() 
    {
        resetTimer -= Time.deltaTime;
        if (resetTimer > 0.0f) 
        {
            return;
        }
        if (clone)
        {
            Instantiate(catPrefab, transform.position, Quaternion.identity);
        }
        Vector3 position = jellyPosition.position;
        position.y -= 0.5f;
        transform.position = position;
        catMoveRef.ResetToPlay();
        collider2D.enabled = true;
        rigidbody2D.isKinematic = false;
        RequestState(Utility.STATE_PLAYING);
    }
	private void DeadCatClone(bool newClone)
	{
        this.clone = newClone;
        RequestState(Utility.STATE_RESET);
	}
    private void OnGameHandlerChangeState(string newState)
    {
        if (newState == Utility.GAME_STATE_PLAYING)
        {
            RequestState(Utility.STATE_PLAYING);
        }
        else if (newState == Utility.GAME_STATE_PAUSE)
        {
            RequestState(Utility.STATE_PAUSE);
        }
    }

    private void SuscribeControl() 
    {
        inputManager.OnMovementCall += catMoveRef.Move;
        inputManager.OnJumpCall += catMoveRef.Jump;
    }
    private void UnsuscribeControl() 
    {
        inputManager.OnMovementCall -= catMoveRef.Move;
        inputManager.OnJumpCall -= catMoveRef.Jump;
    }

    private int coins;
    private void CheckCoinForExtraLife(int coin) 
    {
        coins += coin;
        if (coins > 10)
        {
            coins -= 10;
            catHealth.IncreaseHealth();
        }
    }
    void OnDestroy() 
    {
        inputManager.OnMovementCall -= catMoveRef.Move;
        inputManager.OnJumpCall -= catMoveRef.Jump;
    }   
}

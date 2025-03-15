using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }
    public string UserDisplayName;

    private GameObject localPlayerObject;
    float positionUpdateTimer = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    private async void Start()
    {
        await InitializeAsync();
        BindSessionEvents(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        positionUpdateTimer -= Time.deltaTime;
        if (positionUpdateTimer < 0) {
            positionUpdateTimer = 0.5f;
            UpdatePlayerPosition();
        }
    }

    async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log("Initializing");
        await VivoxService.Instance.InitializeAsync();
    }

    public async void LoginToVivoxAsync()
    {
        LoginOptions options = new LoginOptions();
        options.DisplayName = UserDisplayName;
        options.EnableTTS = true;
        await VivoxService.Instance.LoginAsync(options);
    }

    public async void JoinEchoChannelAsync()
    {
        string channelToJoin = "TestLobby";
        await VivoxService.Instance.JoinEchoChannelAsync(channelToJoin, ChatCapability.TextAndAudio);
    }

    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = "TestLobby";
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }

    public async void JoinPositionalChannelAsync() {
        string channelToJoin = "GlobalProximityChat";

        Channel3DProperties properties = new Channel3DProperties(
            audibleDistance: 20,  
            conversationalDistance: 2,   
            audioFadeIntensityByDistanceaudio: 1.0f,
            audioFadeModel: Unity.Services.Vivox.AudioFadeModel.ExponentialByDistance

        );

        await VivoxService.Instance.JoinPositionalChannelAsync(channelToJoin, ChatCapability.TextAndAudio, properties);
    }

    public async void LeavePositionalChannelAsync() {
        string channelToLeave = "GlobalProximityChat";
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }

    private void BindSessionEvents(bool doBind)
    {
        if(doBind)
        {
            VivoxService.Instance.ParticipantAddedToChannel += onParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel += onParticipantRemovedFromChannel;
        }
        else
        {
            VivoxService.Instance.ParticipantAddedToChannel -= onParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel -= onParticipantRemovedFromChannel;
        }
    }

    private void onParticipantAddedToChannel(VivoxParticipant participant)
    {
        Debug.Log($"Participant {participant.DisplayName} added to channel");
        participant.ParticipantSpeechDetected += onParticipantSpeechDetected;
    }

    private void onParticipantRemovedFromChannel(VivoxParticipant participant)
    {
        Debug.Log($"Participant {participant.DisplayName} removed from channel");
        participant.ParticipantSpeechDetected -= onParticipantSpeechDetected;
    }

    private void onParticipantSpeechDetected() {
        Debug.Log($"Someone is speaking!");
    }

    /// <summary>
    /// Usage: VivoxManager.Instance.UpdatePlayerPosition(transform.position);
    /// Used to update player position in Vivox positional audio channels
    /// </summary>
    /// <param name="playerPosition"></param>
    public async void UpdatePlayerPosition()
    {
        if (localPlayerObject != null) VivoxService.Instance.Set3DPosition(localPlayerObject, "GlobalProximityChat");
    }

}

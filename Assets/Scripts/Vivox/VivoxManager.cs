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

    async void InitializeAsync()
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
            maxDistance: 20.0f,   // Maximum distance before voice fades out
            minDistance: 2.0f,    // Minimum distance for full volume
            rollOff: 1.0f         // Roll-off factor (higher = sharper drop-off)
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
    }

    private void onParticipantRemovedFromChannel(VivoxParticipant participant)
    {
        Debug.Log($"Participant {participant.DisplayName} removed from channel");
    }

    public async void UpdatePlayerPosition(Vector3 playerPosition)
    {
        if (VivoxService.Instance.ActiveChannels.TryGetValue("GlobalProximityChat", out var session))
        {
            await session.SetChannelSessionPositionAsync(playerPosition);
        }
    }

}

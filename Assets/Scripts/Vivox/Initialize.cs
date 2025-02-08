using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class Initialize : MonoBehaviour
{
    public string UserDisplayName;
    // Start is called before the first frame update
    async Task Start()
    {
        InitializeAsync();
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
        string channelToJoin = "Lobby";
        await VivoxService.Instance.JoinEchoChannelAsync(channelToJoin, ChatCapability.TextAndAudio);
    }

    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = "Lobby";
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
}

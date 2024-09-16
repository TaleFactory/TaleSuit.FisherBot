using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoenixWrapped;
using TaleKit.Game;
using TaleKit.Game.Entities;
using TaleKit.Network.Packet;
using TaleKit.Network.Packet.Animation;
using TaleKit.Phoenix;

namespace TaleSuit.FisherBot.Context;

public partial class MainWindowContext : ObservableObject
{
    public ObservableCollection<string> Characters { get; } = [];

    [ObservableProperty]
    private string? character;

    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SelectCharacterCommand { get; }
    public IRelayCommand StartStopCommand { get; }

    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private bool running;

    [ObservableProperty]
    private bool stopping;

    [ObservableProperty]
    private int fishCount;
    
    [ObservableProperty]
    private int legendaryFishCount;
    
    public MainWindowContext()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        SelectCharacterCommand = new AsyncRelayCommand(OnSelectCharacter);
        StartStopCommand = new RelayCommand(OnStartStop);
    }

    private void OnStartStop()
    {
        if (Session == null)
        {
            return;
        }

        if (Running)
        {
            Session.PacketReceived -= OnPacketReceived;
        }
        else
        {
            Session.PacketReceived += OnPacketReceived;
        }
        
        var fishingSkill = Session.Character.Skills.FirstOrDefault(x => x.CastId == 10 && !x.IsOnCooldown) ?? 
                           Session.Character.Skills.FirstOrDefault(x => x.CastId == 1);

        if (fishingSkill is not null)
        {
            Session.Character.AttackSelf(fishingSkill);
        }
        
        Running = !Running;
    }

    private void OnPacketReceived(IPacket packet)
    {
        if (Session == null)
        {
            return;
        }
        
        switch (packet)
        {
            case Guri guri:
                if (guri is { Id: 6, EntityType: EntityType.Player } && guri.EntityId == Session.Character.Id && guri.AnimationId is 30 or 31)
                {
                    Thread.Sleep(Random.Shared.Next(5, 10) * 100);
                    
                    var skill = Session.Character.Skills.FirstOrDefault(x => x.CastId == 2);
                    if (skill is not null)
                    {
                        Session.Character.AttackSelf(skill); 
                        
                        FishCount = guri.AnimationId == 30 
                            ? FishCount + 1 
                            : FishCount;
                        
                        LegendaryFishCount = guri.AnimationId == 31 
                            ? LegendaryFishCount + 1 
                            : LegendaryFishCount;
                    }
                }
                
                if (guri is { Id: 6, EntityType: EntityType.Player } && guri.EntityId == Session.Character.Id && guri.AnimationId is 0)
                {
                    Thread.Sleep(Random.Shared.Next(5, 10) * 100);
                    
                    var skills = Session.Character.Skills.Where(x => x.CastId is 9 or 8 or 3).ToArray();
                    foreach (var skill in skills)
                    {
                        if (skill.IsOnCooldown)
                        {
                            continue;
                        }
                        
                        Session.Character.AttackSelf(skill);
                        Thread.Sleep(Random.Shared.Next(20, 25) * 100);
                    }
                    
                    var fishingSkill = Session.Character.Skills.FirstOrDefault(x => x.CastId == 10 && !x.IsOnCooldown) ?? 
                                       Session.Character.Skills.FirstOrDefault(x => x.CastId == 1);

                    if (fishingSkill is not null)
                    {
                        Session.Character.AttackSelf(fishingSkill);
                    }
                }
                break;
        }
    }

    private Task OnSelectCharacter()
    {
        return Task.Run(() =>
        {
            if (Character == null)
            {
                return;
            }

            Session = PhoenixFactory.CreateSession(Character);
        });
    }
    
    private void OnLoaded()
    {
        Characters.Clear();
        
        var windows = PhoenixClientFactory.GetWindows();
        foreach (var window in windows)
        {
            Characters.Add(window.Character);
        }
    }
}
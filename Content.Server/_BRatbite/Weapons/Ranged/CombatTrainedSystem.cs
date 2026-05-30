using Content.Shared.Popups;
using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._BRatbite.Weapons.Ranged;
using Content.Shared.NukeOps;

namespace Content.Server._BRatbite.Weapons.Ranged;

public sealed partial class CombatTrainedSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CombatUntrainedComponent, GetRecoilModifiersEvent>(OnRecoilModifiersEvent);
        SubscribeLocalEvent<CombatUntrainedComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<CombatTrainedComponent, ComponentStartup>(OnInitializeComp);
        SubscribeLocalEvent<CombatTrainedComponent, ComponentShutdown>(OnShutdownComp);
        SubscribeLocalEvent<NukeOperativeComponent, ComponentStartup>(OnInitializeNukeOps);
    }

    private void OnInitializeNukeOps(Entity<NukeOperativeComponent> ent, ref ComponentStartup args)
    {
        // Add it like this to nukeops because there are a bunch of nuclear operative prototypes
        // And I don't want to add them manually
        _entityManager.RemoveComponent<CombatUntrainedComponent>(ent.Owner);
        AddComp<CombatTrainedComponent>(ent.Owner);
    }

    private void OnInitializeComp(Entity<CombatTrainedComponent> ent, ref ComponentStartup args)
    {
        _entityManager.RemoveComponent<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnShutdownComp(Entity<CombatTrainedComponent> ent, ref ComponentShutdown args)
    {
        AddComp<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnRecoilModifiersEvent(Entity<CombatUntrainedComponent> ent, ref GetRecoilModifiersEvent args)
    {
        args.Modifier = (args.Modifier * ent.Comp.RecoilDebuff) + ent.Comp.FlatRecoilDebuff;
    }

    private void OnShotAttempted(Entity<CombatUntrainedComponent> ent, ref ShotAttemptedEvent args)
    {
        if (_random.Prob(ent.Comp.DropGunChance) && TryComp<HandsComponent>(ent.Owner, out var handComp))
        {
            if (_handsSystem.TryDrop(new(ent.Owner, handComp)))
            {
                _popupSystem.PopupEntity(Loc.GetString("gun-fail-drop-shoot"), ent.Owner, ent.Owner);
                args.Cancel();
                return;
            }

        }
        if (_random.Prob(ent.Comp.MissShotChance))
        {
            _popupSystem.PopupEntity(Loc.GetString("gun-fail-shoot"), ent.Owner, ent.Owner);
            args.Cancel();
            return;
        }
    }
}

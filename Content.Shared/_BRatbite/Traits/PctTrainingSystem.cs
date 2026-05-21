// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Traits;

public sealed class PctTrainingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PctTrainingComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<PctTrainingComponent, GetUserMeleeDamageEvent>(OnGetUserMeleeDamage);
        SubscribeLocalEvent<PctTrainingComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<PctTrainingComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnAttackAttempt(Entity<PctTrainingComponent> ent, ref AttackAttemptEvent args)
    {
        if (IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (ent.Comp.BlockedUntil <= _timing.CurTime)
            return;

        args.Cancel();
    }

    private void OnGetUserMeleeDamage(Entity<PctTrainingComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner)
            return;

        var bonus = new DamageSpecifier();
        bonus.DamageDict.Add("Blunt", FixedPoint2.New(ent.Comp.BluntBonus));
        args.Damage += bonus;
    }

    private void OnGetMeleeAttackRate(Entity<PctTrainingComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner || ent.Comp.Combo <= 0)
            return;

        args.Rate += ent.Comp.Combo * ent.Comp.ComboAttackRateBonus;
    }

    private void OnMeleeHit(Entity<PctTrainingComponent> ent, ref MeleeHitEvent args)
    {
        if (IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner || !args.IsHit)
            return;

        if (IsCleanMobHit(args))
        {
            ent.Comp.Combo = Math.Min(ent.Comp.MaxCombo, ent.Comp.Combo + 1);
            Dirty(ent);
            return;
        }

        ent.Comp.Combo = 0;
        ent.Comp.BlockedUntil = _timing.CurTime + ent.Comp.FumbleCooldown;
        if (TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon) && weapon.NextAttack < ent.Comp.BlockedUntil)
        {
            weapon.NextAttack = ent.Comp.BlockedUntil;
            DirtyField(args.Weapon, weapon, nameof(MeleeWeaponComponent.NextAttack));
        }

        _alerts.ShowAlert(ent.Owner,
            ent.Comp.FumbleAlert,
            cooldown: (_timing.CurTime, ent.Comp.BlockedUntil),
            autoRemove: true);
        Dirty(ent);
    }

    private void ClearPctState(Entity<PctTrainingComponent> ent)
    {
        if (ent.Comp.Combo == 0 && ent.Comp.BlockedUntil <= _timing.CurTime)
            return;

        ent.Comp.Combo = 0;
        ent.Comp.BlockedUntil = TimeSpan.Zero;
        _alerts.ClearAlert(ent.Owner, ent.Comp.FumbleAlert);

        if (TryComp<MeleeWeaponComponent>(ent.Owner, out var weapon) && weapon.NextAttack > _timing.CurTime)
        {
            weapon.NextAttack = _timing.CurTime;
            DirtyField(ent.Owner, weapon, nameof(MeleeWeaponComponent.NextAttack));
        }

        Dirty(ent);
    }

    private bool IsCleanMobHit(MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return false;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hit))
                return false;
        }

        return true;
    }

    public bool IsUnarmedCombatSkillBlocked(EntityUid user)
    {
        if (HasComp<MartialArtsKnowledgeComponent>(user) ||
            HasComponentByName(user, "KravMaga"))
            return true;

        return _inventory.TryGetSlotEntity(user, "gloves", out var gloves) &&
               HasComp<UnarmedCombatSkillBlockerComponent>(gloves);
    }

    private bool HasComponentByName(EntityUid user, string componentName)
    {
        return EntityManager.ComponentFactory.TryGetRegistration(componentName, out var registration) &&
               EntityManager.HasComponent(user, registration.Type);
    }
}

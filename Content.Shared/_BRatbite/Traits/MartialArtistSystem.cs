// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee;

namespace Content.Shared._BRatbite.Traits;

public sealed class MartialArtistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MartialArtistComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MartialArtistComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<MartialArtistComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<MeleeWeaponComponent>(ent.Owner, out var melee))
            return;

        ent.Comp.OriginalAngle = melee.Angle;
        melee.Angle = ent.Comp.WideAttackAngle;
        Dirty(ent.Owner, melee);
    }

    private void OnShutdown(Entity<MartialArtistComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.OriginalAngle is not { } angle ||
            !TryComp<MeleeWeaponComponent>(ent.Owner, out var melee))
            return;

        melee.Angle = angle;
        Dirty(ent.Owner, melee);
    }
}

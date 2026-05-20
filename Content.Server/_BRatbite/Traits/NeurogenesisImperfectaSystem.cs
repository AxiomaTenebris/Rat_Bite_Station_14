// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Shared._BRatbite.Traits;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Server._BRatbite.Traits;

public sealed class NeurogenesisImperfectaSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeurogenesisImperfectaComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BrainComponent, OrganComponentsModifyEvent>(OnBrainOrganModify);
        SubscribeLocalEvent<NeurogenesisImperfectaBrainComponent, ItemSlotInsertAttemptEvent>(OnBrainInsertAttempt);
    }

    private void OnStartup(Entity<NeurogenesisImperfectaComponent> ent, ref ComponentStartup args)
    {
        if (!_body.TryGetBodyOrganEntityComps<BrainComponent>(ent.Owner, out var brains))
            return;

        foreach (var brain in brains)
        {
            EnsureComp<NeurogenesisImperfectaBrainComponent>(brain.Owner);
        }
    }

    private void OnBrainOrganModify(Entity<BrainComponent> ent, ref OrganComponentsModifyEvent args)
    {
        if (!HasComp<NeurogenesisImperfectaBrainComponent>(ent))
            return;

        if (args.Add && !HasComp<NeurogenesisImperfectaComponent>(args.Body))
            EnsureComp<DebrainedComponent>(args.Body);
    }

    private void OnBrainInsertAttempt(Entity<NeurogenesisImperfectaBrainComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (HasComp<MMIComponent>(args.SlotEntity) || HasComp<BorgChassisComponent>(args.SlotEntity))
            args.Cancelled = true;
    }
}

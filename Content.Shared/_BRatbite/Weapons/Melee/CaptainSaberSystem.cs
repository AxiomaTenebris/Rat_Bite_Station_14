using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._BRatbite.Weapons.Melee;


public sealed class CaptainSaberSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CaptainSaberWeaknessComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(Entity<CaptainSaberWeaknessComponent> ent, ref DamageModifyEvent args)
    {
        var origin = args.Origin;
        if (origin != null && TryComp<HandsComponent>(origin, out var handsComponent))
        {
            if (_handsSystem.TryGetActiveItem(new(origin.Value, handsComponent), out var item) && HasComp<CaptainSaberComponent>(item))
            {
                args.Damage *= ent.Comp.DamageMultiplier;
            }
        }
    }
}


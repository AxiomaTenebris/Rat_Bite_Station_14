using System.Numerics;
using Content.Client.Pointing.Components;
using Content.Shared._BRatbite.CCVar;
using Content.Shared.Pointing;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Client.Pointing;

public sealed partial class PointingSystem : SharedPointingSystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public void SubscribeCVars()
    {
        _cfg.OnValueChanged(RatbiteCVars.PointerScale, value => ScaleAllPointers(value));
    }

    private void ScaleAllPointers(float scale)
    {
        var enumerator = EntityQueryEnumerator<PointingArrowComponent, SpriteComponent>();
        while (enumerator.MoveNext(out var uid, out var _, out var sprite))
        {
            _sprite.SetScale((uid, sprite), Vector2.One * scale);
        }
    }

    private void ScalePointer(Entity<PointingArrowComponent, SpriteComponent> ent)
    {
        _sprite.SetScale((ent, ent.Comp2), Vector2.One * _cfg.GetCVar(RatbiteCVars.PointerScale));
    }
}

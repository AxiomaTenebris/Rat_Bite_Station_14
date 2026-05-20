// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class PctTrainingComponent : Component
{
    [DataField]
    public int BluntBonus = 5;

    [DataField]
    public float ComboAttackRateBonus = 0.25f;

    [DataField]
    public int MaxCombo = 3;

    [DataField]
    public TimeSpan FumbleCooldown = TimeSpan.FromSeconds(2);

    [DataField]
    public ProtoId<AlertPrototype> FumbleAlert = "PctFumble";

    [ViewVariables]
    public int Combo;

    [ViewVariables]
    public TimeSpan BlockedUntil;
}

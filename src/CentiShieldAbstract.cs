﻿using Fisobs;
using UnityEngine;

namespace CentiShields;

public sealed class CentiShieldAbstract : AbstractPhysicalObject
{
    public CentiShieldAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, CentiShieldFisob.Instance.Type, null, pos, ID)
    {
        scaleX = 1;
        scaleY = 1;
        saturation = 0.5f;
        hue = 1f;
    }

    public override void Realize()
    {
        realizedObject ??= new CentiShield(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
        base.Realize();
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;

    public override string ToString()
    {
        return this.SaveAsString($"{hue};{saturation};{scaleX};{scaleY}");
    }
}

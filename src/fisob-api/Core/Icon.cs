#nullable enable
using UnityEngine;

namespace CFisobs.Core
{
    // this would be an interface if I had any good name for it 🙃
    public abstract class Icon
    {
        public abstract string SpriteName(int data);
        public abstract Color SpriteColor(int data);
        public abstract int Data(AbstractPhysicalObject apo);
    }

    public sealed class NoIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo) => 0;
        public override Color SpriteColor(int data) => Ext.DefaultIconColor;
        public override string SpriteName(int data) => "Futile_White";
    }

    public sealed class SimpleIcon : Icon
    {
        private readonly string spriteName;
        private readonly Color spriteColor;

        public SimpleIcon(string spriteName, Color spriteColor)
        {
            this.spriteName = spriteName;
            this.spriteColor = spriteColor;
        }

        public override int Data(AbstractPhysicalObject apo) => 0;
        public override Color SpriteColor(int data) => spriteColor;
        public override string SpriteName(int data) => spriteName;
    }
}

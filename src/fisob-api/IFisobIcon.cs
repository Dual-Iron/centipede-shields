using UnityEngine;

namespace CFisobs
{
    public interface IFisobIcon
    {
        string SpriteName(int data);
        Color SpriteColor(int data);
        int Data(AbstractPhysicalObject apo);
    }

    public sealed class SimpleFisobIcon : IFisobIcon
    {
        public readonly string SpriteName;
        public readonly Color SpriteColor;

        public SimpleFisobIcon(string spriteName, Color spriteColor)
        {
            SpriteName = spriteName;
            SpriteColor = spriteColor;
        }

        int IFisobIcon.Data(AbstractPhysicalObject apo)
        {
            return 0;
        }

        Color IFisobIcon.SpriteColor(int data)
        {
            return SpriteColor;
        }

        string IFisobIcon.SpriteName(int data)
        {
            return SpriteName;
        }
    }
}

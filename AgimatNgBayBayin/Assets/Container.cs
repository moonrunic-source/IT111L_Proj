using System;
using AgimatNgBayBayin.Configurations;

namespace AgimatNgBayBayin.Assets
{
    public class Container : BaseEntity
    {
        public float containerSpeed { get; set; }

        protected Container(float x, float y, float speed)
        {
            X = x;
            Y = y;
            containerSpeed = speed;
            Width = Sprites.ContainerSpriteSize;
            Height = Sprites.ContainerSpriteSize;
        }
        public override void Update(float time)
        {
            X -= containerSpeed * time;

            if (X + Width < 0)
            {
                isAlive = false;
            }
        }

        public override void DrawPicture(Graphics g)
        {
        }

    }
}

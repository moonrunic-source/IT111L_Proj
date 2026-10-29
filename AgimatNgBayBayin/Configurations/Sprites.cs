using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgimatNgBayBayin.Configurations
{
    public class Sprites
    {
        /* <summary>
           Fonts stuff
           <\summary> */

        public readonly Color BaybayinFontColor = Color.FromArgb(128, 74, 0);

        /*<summary>
         Gets the path for the assets sprite. 
         </summary> */
        public const string PlayerSpritePath = "BirdPlayer.png";
        public const string BayBayinContainerSpritePath = "BayBayinContainer.png";
        public const string AgimatSpritePath = "Agimat.png";

        /*<summary>
         Gets the width and height for the assets sprite.
         </summary> */
        public const int PlayerSpriteWidth = 70;
        public const int PlayerSpriteHeight = 70;
        public const int ContainerSpriteSize = 70;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgimatNgBayBayin.Configurations
{
    public class RunningGame
    {
        public const int WindowWidth = 800;
        public const int WindowHeight = 600;
        public const int GameSpeed = 60;
        public const int PlayAreaTop = 40;
        public const int HudTop = 470;
        /*<summary>
         Gets the maximum number of lives a player can have.
         </summary> */
        public const int initialLives = 3;

        public const int MaxLives = 5;
        /*<summary>
         * Gets the player values when running
         </summary>*/
        public const int PlayerInitialXPosition = 0;
        public const float PlayerSpeed = 360f;
        /*<summary>
         Gets the points awarded for each correct letter.
         </summary> */
        public const int InitialScore = 0;
        public const int PointsPerCorrectLetter = 10;
        public const int PointsPerCompletedWord = 50;
        public const int PenaltyWrongLetter = 25;
        public const int BonusAgimatAtFullLives = 15;
        public const bool ScoreCannotGoBelowZero = true;

    }
}

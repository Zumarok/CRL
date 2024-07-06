using UnityEngine;

namespace Crux.CRL.DataSystem
{
    public static class Constants
    {
        public const float WINDOW_FADE_TIME = 0.25f;
        public const float FADE_TO_BLACK_TIME = 1f;
        public const int MAX_BUFFS = 8;
        public const int MAX_DEBUFFS = 8;
        public const int MAX_NUM_CARDS = 4;
        public const int MAX_NUM_ENEMIES = 10;
        public const int MAX_ENEMY_HEALTH = 9999;
        public const int CARD_HAND_SIZE = 4;
        public const float FLOOR_BONUS_MULT = 0.02f;
        public const int MAX_FLOOR = 300;
        public const int MIN_NUM_WAVES = 2;


        public const string KEYWORD_TEXT_COLOR = "#57E9FF";
        public const string VARIABLE_MOD_TEXT_COLOR = "#A2EAA0";
        public const float MAX_ASPECT_RATIO = 1.777778f;

        public const float MIN_SEC_BETWEEN_EVENT_CHAT = 4f;
        public const float EVENT_CHAT_CHANCE = 0.5f;
        public const float IDLE_CHAT_CHANCE = 0.00f;


        /// <summary>
        /// Used to determine experience needed per level
        /// </summary>
        public const int EXP_CALC_BASE_VALUE = 250;

        /// <summary>
        /// Used to determine the curve (and value) of experience needed per level
        /// </summary>
        public const double EXP_CALC_EXPONENT = 0.3d;

        public const string SAVE_FILE_NAME = "save";
        public const float DEATH_EXP_PENALTY = 0.1f;

        // Combat Log Colors
        public const string CLC_DEFAULT = "#d0d5d5";
        public const string CLC_PLAYER = "#09c7d3";
        public const string CLC_ENEMY = "#c55e03";
        public const string CLC_DAMAGE = "#da0909";
        public const string CLC_HEAL = "#14ea09";
        public const string CLC_BUFF = "#0b9261";
        public const string CLC_DEBUFF = "#5c0ac2";
        public const string CLC_COUNT = "#dce673";
        public const string CLC_ABSORB = "#0a68a3";
        public const string CLC_SKILL = "#e5f388";

    }
}


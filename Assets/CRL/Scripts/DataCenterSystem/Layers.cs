
namespace Crux.CRL.DataSystem
{
    public static class Layers
    {
        //unity builtin
        public const int Default = 0;

        public const int TransparentFx = 1;

        public const int IgnoreRaycast = 2;

        public const int Water = 4;

        public const int UI = 5;

        public const int PostProcessing = 8;

        // custom
        public const int Card = 9;

        public const int Enemy = 10;


        // layer masks
        public const int EnemyMask = (1 << Enemy);


    }
}

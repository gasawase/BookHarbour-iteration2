using static Assets.Scripts.Models.Enums;

namespace Assets.Scripts.Helpers
{
    public static class HelperFunctions
    {
        public static eOrientation NextOrientation(eOrientation currentOrientation)
        {
            switch (currentOrientation)
            {
                case eOrientation.Upright:
                    return eOrientation.LyingFlat;
                case eOrientation.LyingFlat:
                    return eOrientation.Upright;
                default:
                    return eOrientation.Upright;
            }
        }
    }
}



using Xamarin.Forms;

namespace RoadBookXF
{
    static class RBGlobals
    {
        // usually between 100 to 900 ms, windows uses 500
        public static double RBG_MaxMillisecondsForDoubleClick { get; } = 500;

        // if no mouse moveend in this timer it is drag ....
        public static double RBG_MillisecondsBeforeDragging { get; } = 300;

        // TONS OF LINES DELETED !!!!!!


        // bullshit stuff for debug
        public static Label RBG_DebugLabel;

    }
}

#region #Disclaimer

// Author: Adalberto L. Simeone (Taranto, Italy)
// E-Mail: avengerdragon@gmail.com
// Website: http://www.avengersutd.com/blog
//
// This source code is Intellectual property of the Author
// and is released under the Creative Commons Attribution
// NonCommercial License, available at:
// http://creativecommons.org/licenses/by-nc/3.0/
// You can alter and use this source code as you wish,
// provided that you do not use the results in commercial
// projects, without the express and written consent of
// the Author.

#endregion #Disclaimer

#region Using directives

using System;

#endregion Using directives

namespace Nikse.SubtitleEdit.Logic.ColorChooser
{
    public class ColorChangedEventArgs : EventArgs
    {
        public ColorChangedEventArgs(ColorHandler.Argb argb, ColorHandler.Hsv hsv)
        {
            ARGB = argb;
            HSV = hsv;
        }

        public ColorHandler.Argb ARGB { get; private set; }

        public ColorHandler.Hsv HSV { get; private set; }
    }
}

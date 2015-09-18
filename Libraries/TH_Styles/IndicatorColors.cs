using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace TH_Styles
{
    public class IndicatorColors
    {

        public static List<Color> GetIndicatorColors(int numberOfIndicators)
        {
            List<Color> Result = new List<Color>();

            Color[] colors = GetColors();

            int[] colorArray = GetIndicatorColorSequence(numberOfIndicators);
            for (int x = 0; x <= numberOfIndicators - 1; x++)
                {
                    Result.Add(colors[colorArray[x]]);
                }

            return Result;
        }

                

        static Color[] GetColors()
        {
            return new Color[]
                {                                             
                Colors.Green,                                                 //0

                (Color)ColorConverter.ConvertFromString("#40FF00"),         //1
                (Color)ColorConverter.ConvertFromString("#7FFF00"),         //2
                (Color)ColorConverter.ConvertFromString("#BFFF00"),         //3

                Colors.Yellow,                                              //4

                (Color)ColorConverter.ConvertFromString("#FFBF00"),         //5
                (Color)ColorConverter.ConvertFromString("#FF7F00"),         //6
                (Color)ColorConverter.ConvertFromString("#FF4000"),         //7

                Colors.Red                                                //8                                             
                };
        }

        static int[] GetIndicatorColorSequence(int IndicatorCount)
        {

            int[] Result = null;

            switch (IndicatorCount)
            {
                case 1: Result = new int[] { 8 }; break;
                case 2: Result = new int[] { 0, 8 }; break;
                case 3: Result = new int[] { 0, 4, 8 }; break;
                case 4: Result = new int[] { 0, 2, 4, 8 }; break;
                case 5: Result = new int[] { 0, 2, 4, 6, 8 }; break;
                case 6: Result = new int[] { 0, 1, 2, 4, 6, 8 }; break;
                case 7: Result = new int[] { 0, 1, 2, 4, 6, 7, 8 }; break;
                case 8: Result = new int[] { 0, 1, 2, 3, 4, 5, 6, 8 }; break;
                case 9: Result = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }; break;
            }

            return Result;

        }

    }
}

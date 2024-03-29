using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

public class JaroWinklerDistance
{
    /* The Winkler modification will not be applied unless the 
    * percent match was at or above the mWeightThreshold percent 
    * without the modification. 
    * Winkler's paper used a default value of 0.7
    */
    private const double WeightThreshold = 0.7;

    /* Size of the prefix to be concidered by the Winkler modification. 
        * Winkler's paper used a default value of 4
        */
    private const int NumChars = 4;

    /// <summary>
    /// Returns the Jaro-Winkler distance between the specified  
    /// strings. The distance is symmetric and will fall in the 
    /// range 0 (perfect match) to 1 (no match). 
    /// </summary>
    /// <param name="aString1">First String</param>
    /// <param name="aString2">Second String</param>
    /// <param name="comparer">Comparer used to determine character equality.</param>
    /// <returns></returns>
    public double Distance(string aString1, string aString2, IEqualityComparer<char> comparer = null)
    {
        return 1.0 - Proximity(aString1, aString2, comparer);
    }

    /// <summary>
    /// Returns the Jaro-Winkler distance between the specified  
    /// strings. The distance is symmetric and will fall in the 
    /// range 0 (no match) to 1 (perfect match). 
    /// </summary>
    /// <param name="aString1">First String</param>
    /// <param name="aString2">Second String</param>
    /// <param name="comparer">Comparer used to determine character equality.</param>
    /// <returns></returns>
    public double Proximity(string aString1, string aString2, IEqualityComparer<char> comparer = null)
    {
        comparer = comparer ?? EqualityComparer<char>.Default;

        var lLen1 = aString1.Length;
        var lLen2 = aString2.Length;
        if (lLen1 == 0)
            return lLen2 == 0 ? 1.0 : 0.0;

        var lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);

        var lMatched1 = new bool[lLen1];
        var lMatched2 = new bool[lLen2];

        var lNumCommon = 0;
        for (var i = 0; i < lLen1; ++i)
        {
            var lStart = Math.Max(0, i - lSearchRange);
            var lEnd = Math.Min(i + lSearchRange + 1, lLen2);
            for (var j = lStart; j < lEnd; ++j)
            {
                if (lMatched2[j]) continue;
                if (!comparer.Equals(aString1[i], aString2[j]))
                    continue;
                lMatched1[i] = true;
                lMatched2[j] = true;
                ++lNumCommon;
                break;
            }
        }

        if (lNumCommon == 0) return 0.0;

        var lNumHalfTransposed = 0;
        var k = 0;
        for (var i = 0; i < lLen1; ++i)
        {
            if (!lMatched1[i]) continue;
            while (!lMatched2[k]) ++k;
            if (!comparer.Equals(aString1[i], aString2[k]))
                ++lNumHalfTransposed;
            ++k;
        }
        // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
        var lNumTransposed = lNumHalfTransposed / 2;

        // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
        double lNumCommonD = lNumCommon;
        var lWeight = (lNumCommonD / lLen1
                            + lNumCommonD / lLen2
                            + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

        if (lWeight <= WeightThreshold) return lWeight;
        var lMax = Math.Min(NumChars, Math.Min(aString1.Length, aString2.Length));
        var lPos = 0;
        while (lPos < lMax && comparer.Equals(aString1[lPos], aString2[lPos]))
            ++lPos;
        if (lPos == 0) return lWeight;
        return lWeight + 0.1 * lPos * (1.0 - lWeight);

    }

    public string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
        else
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("iso-8859-8").GetBytes(text);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
    
    public static void Main(String[] args){
        var jw = new JaroWinklerDistance();
        
        // Obtém o texto informado depois de converter para maíusculo e remover os acentos
        var texto1 = jw.RemoveDiacritics(args[0].ToUpper());
        var texto2 = jw.RemoveDiacritics(args[1].ToUpper());


        // Calcula a distância de Jaro-Winkler
        double retorno = jw.Proximity(texto1, texto2);

        Console.WriteLine("Proximidade: " + retorno);
    }

    
}

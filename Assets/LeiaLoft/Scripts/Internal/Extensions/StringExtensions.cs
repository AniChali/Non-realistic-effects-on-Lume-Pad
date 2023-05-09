using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

namespace LeiaLoft
{
	public static class StringExtensions
	{
		/// <summary>
        /// Parses Leia-formatted columns and rows from a string. Especially useful for getting rows and cols from a filename.
        /// </summary>
        /// <param name="filename">A string with a filename of style [filename]_[cols]x[rows].[fmt] where [cols] and [rows] are digits</param>
        /// <returns>True if parse was successful and cols and rows variables are populated</returns>
		public static bool TryParseColsRowsFromFilename(this string filename, out int cols, out int rows)
        {
            cols = 0;
            rows = 0;

            try
            {
                // parse right to left in order to find last instance of 
                Regex regColsRows = new Regex("(?<colGroup>\\d+)x(?<rowGroup>\\d+)", RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
                Match parseResult = regColsRows.Match(filename);

                if (parseResult.Success)
                {
                    // if fail on parsing digit as int, fail out
                    if (!int.TryParse(parseResult.Groups["colGroup"].Value, out cols)) { return false; }
                    if (!int.TryParse(parseResult.Groups["rowGroup"].Value, out rows)) { return false; }

                    // if we succeeded in parsing both digits, $cols and $rows hold valid data
                    return true;
                }
                else
                {
                    LogUtil.Log(LogLevel.Trace, "Failed to parse filename {0} as string with [cols]x[rows].[fmt]", filename);
                    // no crash, but also failed to parse
                    return false;
                }
            }
            catch (System.Exception e)
            {
                LogUtil.Log(LogLevel.Error, "While trying to parse {0} got error {1}", filename, e.ToString());
                return false;
            }
        }
	}
}

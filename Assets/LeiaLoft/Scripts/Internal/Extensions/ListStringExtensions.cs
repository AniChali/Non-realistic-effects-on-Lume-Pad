/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace LeiaLoft
{
    public static class ListStringExtensions
    {
        /// <summary>
        /// Returns index of substring, allows to ignore case
        /// </summary>
        public static int IndexOf(this List<string> list, string item, bool ignoreCase)
        {
            if (item == null)
            {
                return -1;
            }

            if (ignoreCase)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    string listItem = list[i];

                    if (item.Equals(listItem, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }

            return list.IndexOf(item);
        }

        /// <summary>
        /// Turns strings with length less than 4 into upper-cased strings.
        /// Makes first character uppercase in other strings.
        /// basic => Basic
        /// vpo => VPO
        /// xxXxx => XxXxx
        /// xxxx => Xxxx
        /// xxx => XXX
        /// xx => XX
        /// x => X
        /// </summary>
        /// <param name="orig">Original.</param>
        public static List<string> Beautify(this List<string> orig)
        {
            var list = new List<string>(orig);

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Length <= 3)
                {
                    list[i] = list[i].ToUpper(CultureInfo.InvariantCulture);
                }
                else
                {
                    list[i] = list[i][0].ToString().ToUpper(CultureInfo.InvariantCulture) + list[i].Substring(1);
                }
            }

            return list;
        }
    }

}
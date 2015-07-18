/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
using ASC.Common.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json.Linq;
using log4net;
using LumenWorks.Framework.IO.Csv;
using ASC.Web.CRM.Core.Enums;

namespace LumenWorks.Framework.IO.Csv
{
    public static class CsvReaderExtension
    {
        public static String[] GetCurrentRowFields(this CsvReader csvReader, bool htmlEncodeColumn)
        {
            var fieldCount = csvReader.FieldCount;
            var result = new String[fieldCount];

            for (int index = 0; index < fieldCount; index++)
            {
                if (htmlEncodeColumn)
                    result[index] = csvReader[index].HtmlEncode().ReplaceSingleQuote();
                else
                    result[index] = csvReader[index];
            }

            return result;
        }
    }
}
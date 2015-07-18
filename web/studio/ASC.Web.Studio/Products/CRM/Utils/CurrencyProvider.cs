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


#region Import

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Xml;
using ASC.CRM.Core;
using ASC.Web.CRM.Resources;
using log4net;
using System.Security.Principal;
using System.Xml.Linq;

#endregion

namespace ASC.Web.CRM.Classes
{

    public static class CurrencyProvider
    {

        #region Members

        private static readonly ILog _log = LogManager.GetLogger(typeof(CurrencyProvider));
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<String, CurrencyInfo> _currencies;
        private static Dictionary<String, Decimal> _exchangeRates;
        private static DateTime _publisherDate;
        private const String _formatDate = "yyyy-MM-ddTHH:mm:ss.fffffffK";

        #endregion

        #region Constructor

        static CurrencyProvider()
        {
            var currencies = Global.DaoFactory.GetCurrencyInfoDao().GetAll();

            if (currencies == null || currencies.Count == 0)
            {
                currencies = new List<CurrencyInfo>
                    {
                        new CurrencyInfo("Currency_UnitedStatesDollar", "USD", "$", "US", true, true)
                    };
            }

            _currencies = currencies.ToDictionary(c => c.Abbreviation);
        }

        #endregion

        #region Property

        public static DateTime GetPublisherDate
        {
            get {
                TryToReadPublisherDate(GetExchangesTempPath());
                return _publisherDate; }
        }

        #endregion

        #region Public Methods

        public static CurrencyInfo Get(string currencyAbbreviation)
        {
            if (!_currencies.ContainsKey(currencyAbbreviation))
                return null;

            return _currencies[currencyAbbreviation];
        }

        public static List<CurrencyInfo> GetAll()
        {
            return _currencies.Values.OrderBy(v => v.Abbreviation).ToList();
        }

        public static List<CurrencyInfo> GetBasic()
        {
            return _currencies.Values.Where(c => c.IsBasic).OrderBy(v => v.Abbreviation).ToList();
        }

        public static List<CurrencyInfo> GetOther()
        {
            return _currencies.Values.Where(c => !c.IsBasic).OrderBy(v => v.Abbreviation).ToList();
        }

        public static Dictionary<CurrencyInfo, Decimal> MoneyConvert(CurrencyInfo baseCurrency)
        {
            if (baseCurrency == null) throw new ArgumentNullException("baseCurrency");
            if (!_currencies.ContainsKey(baseCurrency.Abbreviation)) throw new ArgumentOutOfRangeException("baseCurrency", "Not found.");

            var result = new Dictionary<CurrencyInfo, Decimal>();
            var rates = GetExchangeRates();
            foreach (var ci in GetAll())
            {

                if (baseCurrency.Title == ci.Title)
                {
                    result.Add(ci, 1);

                    continue;
                }

                var key = String.Format("{1}/{0}", baseCurrency.Abbreviation, ci.Abbreviation);

                if (!rates.ContainsKey(key))
                    continue;

                result.Add(ci, rates[key]);
            }
            return result;
        }


        public static bool IsConvertable(String abbreviation)
        {
            var findedItem = _currencies.Keys.ToList().Find(item => String.Compare(abbreviation, item) == 0);

            if (findedItem == null)
                throw new ArgumentException(abbreviation);

            return _currencies[findedItem].IsConvertable;
        }

        public static Decimal MoneyConvert(decimal amount, string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.Compare(from, to, true) == 0) return amount;

            var rates = GetExchangeRates();

            if (from.Contains('-')) from = new RegionInfo(from).ISOCurrencySymbol;
            if (to.Contains('-')) to = new RegionInfo(to).ISOCurrencySymbol;
            var key = string.Format("{0}/{1}", to, from);

            return Math.Round(rates[key] * amount, 4, MidpointRounding.AwayFromZero);
        }

        public static Decimal MoneyConvertToDefaultCurrency(decimal amount, string from)
        {
            return MoneyConvert(amount, from, Global.TenantSettings.DefaultCurrency.Abbreviation);
        }

        #endregion

        #region Private Methods

        private static bool ObsoleteData()
        {
            return _exchangeRates == null || (DateTime.UtcNow.Date.Subtract(_publisherDate.Date).Days > 0);
        }

        private static string GetExchangesTempPath() {
            return Path.Combine(Path.GetTempPath(), Path.Combine("onlyoffice", "exchanges"));
        }

        private static Dictionary<String, Decimal> GetExchangeRates()
        {
            if (ObsoleteData())
            {
                lock (_syncRoot)
                {
                    if (ObsoleteData())
                    {
                        try
                        {
                            _exchangeRates = new Dictionary<string, decimal>();

                            var tmppath = GetExchangesTempPath();

                            TryToReadPublisherDate(tmppath);

                            var regex = new Regex("= (?<Currency>([\\s\\.\\d]*))");
                            var updateEnable = WebConfigurationManager.AppSettings["crm.update.currency.info.enable"] != "false";
                            foreach (var ci in _currencies.Values.Where(c => c.IsConvertable))
                            {
                                var filepath = Path.Combine(tmppath, ci.Abbreviation + ".xml");

                                if (updateEnable && 0 < (DateTime.UtcNow.Date - _publisherDate.Date).TotalDays || !File.Exists(filepath))
                                {
                                    DownloadRSS(ci.Abbreviation, filepath);
                                }

                                if (!File.Exists(filepath))
                                {
                                    continue;
                                }

                                var currencyXml = XDocument.Load(filepath);

                                var channel = currencyXml.Root.Element("channel");
                                if (channel == null) continue;

                                var lastBuildDate = channel.Element("lastBuildDate").Value.Trim();
                                var items = channel.Elements("item").ToList();

                                _publisherDate = new RSSDateTimeParser().Parse(lastBuildDate);
                                foreach (var item in items) {
                                    var currency = regex.Match(item.Element("description").Value.Trim()).Groups["Currency"].Value.Trim();
                                    _exchangeRates.Add(item.Element("title").Value.Trim(), Convert.ToDecimal(currency, CultureInfo.InvariantCulture.NumberFormat));
                                }
                            }

                            WritePublisherDate(tmppath);

                        }
                        catch (Exception error)
                        {
                            LogManager.GetLogger("ASC.CRM").Error(error);
                            _publisherDate = DateTime.UtcNow;
                        }
                    }
                }
            }

            return _exchangeRates;
        }


        private static void TryToReadPublisherDate(string tmppath)
        {
            if (_publisherDate == default(DateTime))
            {
                try
                {
                    var timefile = Path.Combine(tmppath, "last.time");
                    if (File.Exists(timefile))
                    {
                        var dateFromFile = File.ReadAllText(timefile);
                        _publisherDate = DateTime.ParseExact(dateFromFile, _formatDate, null);
                    }
                }
                catch (Exception err)
                {
                    LogManager.GetLogger("ASC.CRM").Error(err);
                }
            }
        }

        private static void WritePublisherDate(string tmppath)
        {
            try
            {
                var timefile = Path.Combine(tmppath, "last.time");
                File.WriteAllText(timefile, _publisherDate.ToString(_formatDate));
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.CRM").Error(err);
            }
        }

        private static void DownloadRSS(string currency, string filepath)
        {

            try
            {
                var dir = Path.GetDirectoryName(filepath);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var destinationURI = new Uri(String.Format("http://themoneyconverter.com/rss-feed/{0}/rss.xml", currency));

                var request = (HttpWebRequest)WebRequest.Create(destinationURI);
                request.Method = "GET";
                request.AllowAutoRedirect = true;
                request.MaximumAutomaticRedirections = 2;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; rv:8.0) Gecko/20100101 Firefox/8.0";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = new StreamReader(response.GetResponseStream()))
                {
                    var data = responseStream.ReadToEnd();

                    File.WriteAllText(filepath, data);

                }
            }
            catch (Exception error)
            {
                _log.Error(error);
            }
        }

        #endregion

        internal class RSSDateTimeParser
        {
            private static string[] _dateFormats = new[] {
                    @"ddd, dd MMM yyyy hh':'mm':'ss tt 'GMT'",
                    @"ddd, dd MMM yyyy HH':'mm':'ss 'GMT'",
                };

            private static DateTimeStyles _styles = DateTimeStyles.AllowLeadingWhite
                | DateTimeStyles.AllowInnerWhite
                | DateTimeStyles.AllowTrailingWhite
                | DateTimeStyles.AllowWhiteSpaces;

            public DateTime Parse(string dateTime)
            {
                // Attempt default DateTime parse
                DateTime dt;
                if (!DateTime.TryParse(dateTime, out dt))
                {
                    // Parse using custom formats
                    if (!DateTime.TryParseExact(dateTime, _dateFormats,
                        CultureInfo.InvariantCulture, _styles, out dt))
                    {
                        // Throw exception if custom formats can't parse the string.
                        throw new FormatException(CRMErrorsResource.DateTimeFormatInvalid);
                    }
                }

                return dt.ToUniversalTime();
            }
        }

    }
}

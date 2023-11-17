using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace myRaiHelper
{
    public class HtmlTrim
    {
        public static string Trim(string htmlString)
        {
            htmlString = TrimTag(htmlString);
            htmlString = TrimBR(htmlString);
            htmlString = NormalizeFont(htmlString);
            return htmlString;
        }
        public static string TrimTag(string htmlString)
        {
            foreach (string tag in "p,div,span".Split(','))
            {
                Regex pRegex = new Regex("<" + tag + ".*?>(.*?)</" + tag + ">", RegexOptions.IgnoreCase);
                MatchCollection M = pRegex.Matches(htmlString);
                if (M != null)
                {
                    if (M.Count > 0)
                    {
                        var first = M[0];
                        String Content = first.Groups[1].Value;
                        if (Content != null)
                            Content = Content = Content.ToLower().Replace("&nbsp;", "").Replace("<br>","").Replace("<br/>", "").Replace("<br />", "").Trim();

                        if (String.IsNullOrWhiteSpace(Content))
                            htmlString = htmlString.Replace(first.Groups[0].Value, "".PadLeft(first.Groups[0].Length, ' '));
                    }
                    if (M.Count > 1)
                    {
                        var last = M[M.Count - 1];
                        String Content = last.Groups[1].Value;
                        if (Content != null)
                            Content = Content = Content.ToLower().Replace("&nbsp;", "").Replace("<br>", "").Replace("<br/>", "").Replace("<br />", "").Trim();

                        if (String.IsNullOrWhiteSpace(Content))
                            htmlString = htmlString.Replace(last.Groups[0].Value, "".PadLeft(last.Groups[0].Length, ' '));
                    }
                }
            }

            return htmlString;
        }
        public static string TrimBR(string htmlString)
        {
            string[] parti = htmlString.Split(new string[] { "<br>", "<br />", "<br/>" }, StringSplitOptions.None);
            List<string> L = new List<string>();

            foreach (string p in parti)
            {
                string a = Regex.Replace(p, "<.*?>|&nbsp;", "").Trim();
                if (String.IsNullOrWhiteSpace(a))
                    L.Add(a);
                else
                    L.Add(p);
            }
            L = L.SkipWhile(x => String.IsNullOrWhiteSpace(x)).ToList();
            L.Reverse();
            L = L.SkipWhile(x => String.IsNullOrWhiteSpace(x)).ToList();
            L.Reverse();

            return String.Join("<br>", L.ToArray());
        }

        public static string NormalizeFont(string htmlString)
        {
            return Regex.Replace(htmlString, "class=\".*?\"|line-height.*?pt|line-height.*?px|line-height.*?;|face=\".*?\"", "",RegexOptions.IgnoreCase |RegexOptions.Singleline);
        }
    }
}
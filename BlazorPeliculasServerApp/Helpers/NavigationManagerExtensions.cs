using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorPeliculasServerApp.Helpers
{
    public static class NavigationManagerExtensions
    {
        public static Dictionary<string, string> GetQueryString(this NavigationManager navigationManager, string url)
        {
            if(string.IsNullOrWhiteSpace(url) || !url.Contains("?") || url.Substring(url.Length - 1) == "?")
            {
                return null;
            }

            var queryString = url.Split(new string[] { "?" }, StringSplitOptions.None)[1];
            Dictionary<string, string> queryStringDict = queryString.Split('&')
                                                                    .ToDictionary(c => c.Split('=')[0],
                                                                                  c => Uri.UnescapeDataString(c.Split('=')[1]));

            return queryStringDict;


        }
    }
}

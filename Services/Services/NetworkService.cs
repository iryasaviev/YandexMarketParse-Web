using Services.Services;
using System;
using System.IO;
using System.Net;

namespace YMParseWeb.Services
{
    public class NetworkService
    {
        public static Uri _currentProxyAddress = null;

        /// <summary>
        /// Запрашивает страницу и возвращает её.
        /// </summary>
        public string LoadPage(string url, bool error)
        {
            string result = "";

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            WebRequest request = WebRequest.Create(url);
            request.Timeout = 2000;

            if (error)
            {
                Uri newProxy = GetProxyString();
                request.Proxy = new WebProxy
                {
                    Address = newProxy
                };

                new ProxyService().PutProxyToEndOfFile(newProxy.OriginalString);
            }

            try
            {
                WebResponse response = request.GetResponse();

                Stream receiveStream = response.GetResponseStream();
                if (receiveStream != null)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        result = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                response.Close();

                return result;
            }
            catch
            {
                LoadPage(url, true);
                return result;
            }
        }

        /// <summary>
        /// Возвращает прокси.
        /// </summary>
        public WebProxy GetProxy()
        {
            //https://docs.microsoft.com/ru-ru/dotnet/api/system.net.webrequest.proxy?view=netframework-4.8
            //https://htmlweb.ru/analiz/proxy_list.php

            string[] proxies = new ProxyService().GetProxiesListFromFile();

            WebProxy proxy = new WebProxy
            {
                Address = _currentProxyAddress
            };

            string address = "";
            for (int i = 1; proxies.Length >= i; i++)
            {
                if (proxy.Address != null)
                {
                    string pAddress = proxy.Address.OriginalString.Substring(7);
                    if (pAddress == proxies[i])
                    {
                        if (i + 1 != proxies.Length)
                        {
                            if (proxies[i + 1] != null)
                            {
                                address = proxies[i + 1];
                                break;
                            }
                        }
                        else
                        {
                            i = 1;
                            address = proxies[i + 1];
                        }
                    }
                    else
                    {
                        if ((i + 1) == proxies.Length)
                        {
                            address = proxies[proxies.Length - (proxies.Length - 1)];
                            break;
                        }
                    }
                }
                else
                {
                    address = proxies[1];
                    break;
                }
            }

            proxy.Address = new Uri("http://" + address);
            proxy.BypassProxyOnLocal = true;
            _currentProxyAddress = proxy.Address;

            return proxy;
        }

        public Uri GetProxyString()
        {
            string[] proxies = new ProxyService().GetProxiesListFromFile();

            string address = "";
            for (int i = 1; proxies.Length >= i; i++)
            {
                if (_currentProxyAddress != null)
                {
                    if (_currentProxyAddress.OriginalString.Substring(7) == proxies[i])
                    {
                        if (i + 1 != proxies.Length)
                        {
                            if (proxies[i + 1] != null)
                            {
                                address = proxies[i + 1];
                                break;
                            }
                        }
                        else
                        {
                            i = 1;
                            address = proxies[i + 1];
                            break;
                        }
                    }
                    else
                    {
                        if ((i + 1) == proxies.Length)
                        {
                            address = proxies[proxies.Length - (proxies.Length - 1)];
                            break;
                        }
                    }
                }
                else
                {
                    address = proxies[1];
                    break;
                }
            }

            _currentProxyAddress = new Uri("http://" + address);

            return new Uri("http://" + address);
        }
    }
}
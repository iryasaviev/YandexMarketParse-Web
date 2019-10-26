using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using YMParseWeb.Models;

namespace Services.Services
{
    public class ProxyService
    {
        /// <summary>
        /// Проверяет нужно ли обновить файл со списком прокси.
        /// Возвращает true если нужно сменить прокси, false если не нужно.
        /// </summary>
        public bool CheckList()
        {
            string path = GetPathToFile();

            if (new FileInfo(path).Exists)
            {
                using (StreamReader stream = new StreamReader(path))
                {
                    string line = stream.ReadLine();
                    if (line != null)
                    {
                        if (DateTime.Parse(line).ToShortDateString() == DateTime.Now.ToShortDateString())
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Возвращает список прокси из файла.
        /// </summary>
        public string[] GetProxiesListFromFile(string path = null)
        {
            if (path == null)
                path = GetPathToFile();

            string[] data = new string[1];
            if (new FileInfo(path).Exists)
            {
                using (StreamReader stream = new StreamReader(path))
                {
                    for (int i = 0; data.Length >= i; i++)
                    {
                        string line = stream.ReadLine();
                        if (line != null)
                        {
                            Array.Resize(ref data, i + 1);
                            data[i] = line;
                        }
                    }
                    stream.Close();
                }
            }

            return data;
        }

        /// <summary>
        /// Обновляет список прокси в файле.
        /// </summary>
        public void UpdateProxiesInFile(string[] data)
        {
            string path = GetPathToFile();

            // Считывает первую строку (дату)
            string dateFromFile = "";
            if (new FileInfo(path).Exists)
            {
                using (StreamReader stream = new StreamReader(path))
                {
                    dateFromFile = stream.ReadLine();
                    stream.Close();
                }
            }

            // Перезаписывает прокси
            using (StreamWriter stream = new StreamWriter(path, false))
            {
                if (dateFromFile != "" && dateFromFile != null)
                    stream.WriteLine(dateFromFile);

                for (int i = 0; data.Length > i; i++)
                {
                    if (data[i] != null)
                        stream.WriteLine(data[i]);
                }
                stream.Close();
            }
        }

        /// <summary>
        /// Устанавливает в файле новый список прокси.
        /// </summary>
        public void SetProxiesToFile(string[] data = null, bool tosetDate = true)
        {
            if (data == null)
                data = GetProxiesFromApi();

            string path = GetPathToFile();

            if (!new FileInfo(path).Exists)
                using (File.Create(path)) { }

            using (StreamWriter stream = new StreamWriter(path, false))
            {
                if (tosetDate)
                    stream.WriteLine(DateTime.Now);

                for (int i = 0; data.Length > i; i++)
                {
                    if (data[i] != null)
                        stream.WriteLine(data[i]);
                }
                stream.Close();
            }
        }

        /// <summary>
        /// Возвращает массив прокси полученный по API сервиса.
        /// </summary>
        public string[] GetProxiesFromApi()
        {
            // https://htmlweb.ru/analiz/proxy_list.php

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://htmlweb.ru/json/proxy/get?country=RU&perpage=380&short");
            request.Proxy = null;
            WebResponse response = (HttpWebResponse)request.GetResponse();

            ProxyModel proxy = new ProxyModel();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string responseBody = reader.ReadToEnd();
                    if (responseBody != null && responseBody != "")
                    {
                        Dictionary<string, string> items = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
                        string[] proxies = new string[items.Values.Count];

                        int i = 0;
                        foreach (string value in items.Values)
                        {
                            if (value.Length > 2)
                            {
                                proxies[i] = value;
                            }
                            i++;
                        }

                        proxy.Proxies = proxies;
                        proxy.AddedDate = DateTime.Now;
                    }
                    response.Close();
                }
            }

            return proxy.Proxies;
        }

        /// <summary>
        /// Возвращает путь до файла proxies.txt.
        /// </summary>
        public string GetPathToFile()
        {
            string[] pathItems = AppDomain.CurrentDomain.BaseDirectory.Split(@"\".ToCharArray());
            string path = "";
            foreach (string item in pathItems)
            {
                if (item.ToLower() == "bin" || item.ToLower() == "debug")
                    break;

                if (path == "")
                {
                    path = item + @"\";
                }
                else
                {
                    path = Path.Combine(path, item);
                }
            }
            return Path.Combine(path, "proxies.txt");
        }

        /// <summary>
        /// Перемешивает список прокси в файле.
        /// </summary>
        public void MixProxiesInFile()
        {
            string[] proxies = GetProxiesListFromFile();

            int l = proxies.Length - 1;
            for (int i = l; i >= 1; i--)
            {
                int j = new Random().Next(i + 1);
                string temp = proxies[j];
                if (j != 0 && i != 0)
                {
                    proxies[j] = proxies[i];
                    proxies[i] = temp;
                }
            }

            string[] result = new string[l];
            for (int i = 0; proxies.Length > i; i++)
            {
                if (i + 1 != proxies.Length)
                {
                    result[i] = proxies[i + 1];
                }
            }

            UpdateProxiesInFile(result);
        }

        /// <summary>
        /// Ставит текущий прокси на последнее место.
        /// </summary>
        public void PutProxyToEndOfFile(string currentProxy)
        {
            string[] proxies = GetProxiesListFromFile();

            for (int a = 1; proxies.Length > a; a++)
            {
                if (currentProxy.Substring(7) == proxies[a])
                {
                    for (int b = a; proxies.Length > b; b++)
                    {
                        if (b + 1 == proxies.Length)
                        {
                            proxies[b] = currentProxy.Substring(7);
                        }
                        else
                        {
                            proxies[b] = proxies[b + 1];
                        }
                    }
                    break;
                }
            }

            string[] result = new string[proxies.Length - 1];
            for (int i = 0; proxies.Length > i; i++)
            {
                if (i + 1 != proxies.Length)
                {
                    result[i] = proxies[i + 1];
                }
            }

            UpdateProxiesInFile(result);
        }
    }
}
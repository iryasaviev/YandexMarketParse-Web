using HtmlAgilityPack;
using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;

namespace YMParseWeb.Services
{
    public class ParseService
    {
        /// <summary>
        /// Метод для парсинга страниц.
        /// </summary>
        public List<ProductModel> ParseOneQuery(string url)
        {
            RequestModel rModel = SetRequest(url);
            url = SetRequestUrl(rModel);

            List<ProductModel> items = new List<ProductModel>();

            // Перебор по страницам
            bool error = false;
            int pageNum = 0;
            while (true)
            {error:
                string pageContent = new NetworkService().LoadPage(url, error);

                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(pageContent);

                // Перебор по товарам
                HtmlNodeCollection cards = document.DocumentNode.SelectNodes("//div[@data-id]");
                if (cards != null)
                {
                    foreach (HtmlNode card in cards)
                    {
                        //descriptions = card.SelectNodes("//div[contains(@class, 'n-snippet-card2__content')]")
                        HtmlNodeCollection titles = card.SelectNodes("//div[contains(@class, 'n-snippet-card2__title')]"),
                            prices = card.SelectNodes("//div[contains(@class, 'n-snippet-card2__main-price-wrapper')]");
                        for (int i = 0; cards.Count > i; i++)
                        {
                            string price = "";
                            if (prices[i].InnerText != "")
                            {
                                price = prices[i].InnerText.Replace(" ", "");
                                price = price.Substring(0, price.Length - 2);
                            }

                            string link = titles[i].FirstChild.GetAttributeValue("href", "").Replace("&amp;", "&");
                            if (link.Contains("market-click2"))
                            {
                                link = "http:" + link;
                                link.Replace(@"\", "/");
                            }
                            else
                            {
                                link = "https://market.yandex.ru" + link;
                            }

                            items.Add(new ProductModel
                            {
                                //Description = descriptions[i].InnerText,
                                Name = titles[i].InnerText,
                                Link = link,
                                Price = price
                            });

                            error = false;

                            // Если указано количество позиций, то количество страниц не учитываются
                            if (rModel.PositionsCount > 0)
                            {
                                if (i >= (rModel.PositionsCount - 1))
                                {
                                    return items;
                                }
                            }
                        }
                        break;
                    }
                }
                else
                {
                    error = true;
                    goto error;
                }

                // Изменить URL для следующего запроса
                pageNum++;

                if (rModel.PageCount == 0 || rModel.PageCount == pageNum)
                    return items;

                url = UpdatePageNumInRequestUrl(url, pageNum + 1);
            }
        }

        /// <summary>
        /// Парсит несколько запросов.
        /// </summary>
        public List<QueriesModel> ParseFewQueries(string queryString)
        {
            List<QueriesModel> result = new List<QueriesModel>();

            string[] queries = queryString.Split(new string[] { "and" }, StringSplitOptions.None);
            for (int i = 0; queries.Length > i; i++)
            {
                List<ProductModel> products = ParseOneQuery(queries[i]);
                QueriesModel model = new QueriesModel
                {
                    Products = products,
                    MinPrice = GetMinPrice(products),
                    AveragePrice = GetAveragePrice(products)
                };

                result.Add(model);
            }

            return result;
        }

        /// <summary>
        /// Возвращает минимальную цену из списка товаров.
        /// </summary>
        public double GetMinPrice(List<ProductModel> pModel)
        {
            double result = 0;

            for (int i = 0; pModel.Count > i; i++)
            {
                try
                {
                    double price = Convert.ToDouble(pModel[i].Price);

                    if (result == 0)
                    {
                        result = price;
                    }
                    else
                    {
                        if (result > price)
                            result = price;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// Возвращает среднее арифмитическое цены из списка товаров.
        /// </summary>
        public double GetAveragePrice(List<ProductModel> pModel)
        {
            double sum = 0;

            int i;
            for (i = 0; pModel.Count > i; i++)
            {
                try
                {
                    sum += Convert.ToDouble(pModel[i].Price);
                }
                catch
                {
                    continue;
                }
            }

            if (sum != 0)
                return sum / i;

            return sum;
        }

        /// <summary>
        /// Возвращает класс RequestModel с заполненными свойствами.
        /// </summary>
        public RequestModel SetRequest(string request)
        {
            RequestModel model = new RequestModel();

            string[] items = request.ToLower().Split(new string[] { "&&" }, StringSplitOptions.None);
            for (int i = 0; items.Length > i; i++)
            {
                string[] options = items[i].Split('=');
                switch (options[0])
                {
                    case "text":
                        model.Text = options[1];
                        break;

                    case "partnumber":
                        model.PartNumber = options[1];
                        break;

                    case "city":
                        model.City = options[1];
                        break;

                    case "pagecount":
                        model.PageCount = Convert.ToInt32(options[1]);
                        break;

                    case "positionscount":
                        model.PositionsCount = Convert.ToInt32(options[1]);
                        break;
                }
            }

            return model;
        }

        /// <summary>
        /// Формирует URL строку запроса.
        /// </summary>
        public string SetRequestUrl(RequestModel model)
        {
            string text = "";

            if (model.Text != null)
            {
                text = model.Text.Replace(" ", "%20");

                if (model.PartNumber != null)
                {
                    text += "%20" + model.PartNumber;
                }

                if (model.City != null)
                {
                    text += "%20" + model.City.Replace(" ", "%20");
                }
            }

            return "https://market.yandex.ru/search?&text=" + text;
        }

        /// <summary>
        /// Обновляет в URL строке параметр pageCounter.
        /// </summary>
        public string UpdatePageNumInRequestUrl(string url, int pageNum)
        {
            if (url.IndexOf("page") == -1)
            {
                url = url + "&page=" + pageNum;
            }
            else
            {
                string[] urlParams = url.Split('&');

                foreach (string param in urlParams)
                {
                    if (param.IndexOf("page") != -1)
                    {
                        url = url.Replace(param, "page=" + pageNum.ToString());
                        break;
                    }
                }
            }
            return url;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Services.Models;
using Services.Services;
using System.Collections.Generic;
using YMParseWeb.Services;

namespace YMParseWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParseController : ControllerBase
    {
        // api/parse/search?text=ssd&&pagecount=1
        // api/parse/search?text=ssd120&&partnumber=16532&&city=kazan&&pagecount=5&&positionscount=20
        // api/parse/search?text=Коммутатор X440-G2-24t-10GE4&&partnumber=16532&&positionscount=20andtext=Кабель питания Pwr Cord 10A CEE 77 C13&&partnumber=10033&&positionscount=20andtext=DIN-рейка (25 см) оцинкованная&&partnumber=YDN10-0025&&positionscount=20
        [HttpGet("{searchQuery}")]
        public JsonResult Get(string searchQuery)
        {
            string reqString = HttpContext.Request.QueryString.Value;

            if (reqString != "" || reqString != null)
            {
                // Проверка списка прокси
                ProxyService proxyService = new ProxyService();
                if (proxyService.CheckList())
                {
                    proxyService.SetProxiesToFile();
                }

                // Парсинг
                List<QueriesModel> items = new ParseService().ParseFewQueries(reqString.Remove(0, 1));
                return new JsonResult(items);
            }

            return new JsonResult("Error! Искомый запрос не может быть равен null.");
        }
    }
}
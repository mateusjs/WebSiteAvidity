using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MarvelTestCore.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;

namespace MarvelTestCore.Controllers
{
    public class HomeController : Controller
    {


        public dynamic Index(int request)
        {


            using (var client = new HttpClient())
            {
                IndexModel model = new IndexModel();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string ts = DateTime.Now.Ticks.ToString();
                string publicKey = "e03c763129548d360f04c21e913dee0b";
                string hash = GerarHash(ts, publicKey,
                   "321571b0e166adfd22001ea67bc60162af30fffd");

                //if (request == 1)
                //{
                Personagem personagem;
                List<Personagem> personagens = new List<Personagem>();

                HttpResponseMessage response = client.GetAsync(
                "http://gateway.marvel.com/v1/public/" +
                $"comics/43501/characters?ts={ts}&apikey={publicKey}&hash={hash}&").Result;

                response.EnsureSuccessStatusCode();
                string conteudo =
                    response.Content.ReadAsStringAsync().Result;

                dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                foreach (var item in resultado.data.results)
                {
                    personagem = new Personagem();
                    personagem.Nome = item.name;
                    personagem.Descricao = item.description != "" ? item.description : "This char don't have a description";
                    personagem.UrlImagem = item.thumbnail.path + "." +
                        item.thumbnail.extension;
                    personagem.UrlWiki = item.urls[1].url;
                    personagens.Add(personagem);
                }
                //}
                //else
                //{
                Comic comic = new Comic();

                HttpResponseMessage comicResponse = client.GetAsync(
                "http://gateway.marvel.com/v1/public/" +
                $"comics/43501?ts={ts}&apikey={publicKey}&hash={hash}&").Result;

                comicResponse.EnsureSuccessStatusCode();
                string comicConteudo =
                    comicResponse.Content.ReadAsStringAsync().Result;

                dynamic comicResultado = JsonConvert.DeserializeObject(comicConteudo);

                comic.Titulo = comicResultado.data.results[0].title;
                comic.Descricao = comicResultado.data.results[0].description;
                comic.UrlImagem = comicResultado.data.results[0].images[0].path + "." + comicResultado.data.results[0].images[0].extension;
                comic.CopyRights = comicResultado.copyright;
                comic.Attribution = comicResultado.attributionText;

                model.personagems = personagens;
                model.comic = comic;

                return View(model);

                //}
            }

        }

        private string GerarHash(
            string ts, string publicKey, string privateKey)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash)
                .ToLower().Replace("-", String.Empty);
        }



        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

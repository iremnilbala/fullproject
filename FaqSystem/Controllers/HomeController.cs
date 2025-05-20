using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FaqSystem.Models;
using Newtonsoft.Json;

namespace FaqSystem.Controllers
{
    public class HomeController : Controller
    {
        Context _context = new Context();
        public ActionResult Index()
        {
            var faqs = _context.Faqs.Where(x => x.isDeleted == false).ToList();

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> GetAnswer(string question)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(question))
                {
                    return Json(new { success = false, message = "Soru boş olamaz." }, JsonRequestBehavior.AllowGet);
                }

                string normalizedQuestion = question.Trim().ToLower().TrimEnd('?');


                var faqs = _context.Faqs.Where(x => !x.isDeleted).ToList();
                var faq = faqs.FirstOrDefault(x =>
                    x.Question?.Trim().ToLower().TrimEnd('?') == normalizedQuestion);


                if (faq != null)
                {
                    return Json(new { success = true, answer = faq.Answer }, JsonRequestBehavior.AllowGet);
                }

                var client = new HttpClient();
                var requestData = new { question = question };
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://127.0.0.1:5000/get-answer", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic nlpResult = JsonConvert.DeserializeObject(responseString);
                string answer = nlpResult?.answer;

                if (!string.IsNullOrEmpty(answer))
                {
                    return Json(new { success = true, answer = answer }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "NLP boş cevap döndürdü." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // En kritik satır bu!
                return Json(new { success = false, message = "Sunucu hatası: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}


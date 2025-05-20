using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FaqSystem.Models;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using PagedList;

namespace FaqSystem.Controllers
{
    //[Authorize]
    public class AdminPanelController : Controller
    {

        Models.Context _context = new Models.Context();
        public UserManager<User> UserManager { get; private set; }

        public AdminPanelController() : this(new UserManager<User>(new UserStore<User>(new Models.Context())))
        {

        }

        public AdminPanelController(UserManager<User> userManager)
        {
            UserManager = userManager;
        }

        // GET: AdminPanel
        [Authorize]
        public ActionResult Index()
        {

            // Session'dan kullanıcı bilgilerini al
            var user = Session["User"] as User;

            // Eğer kullanıcı varsa, ViewModel oluşturup gönder
            if (user != null)
            {
                var userModel = new User
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    Surname = user.Surname,
                    PhoneNumber = user.PhoneNumber,
                    isAdmin = user.isAdmin // ✅ admin bilgisi eklendi
                };


                return View(userModel);
            }

            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }


        [HttpPost]
        public ActionResult Login(User user, string returnUrl)
        {
            var result = _context.Users.FirstOrDefault(x => user.UserName == x.UserName && user.Password == x.Password);

            if (result != null)
            {
                // Giriş başarılı
                FormsAuthentication.SetAuthCookie(result.UserName, false);

                // 🔁 Güncel kullanıcıyı tekrar veritabanından çek (garanti olur)
                var updatedUser = _context.Users.FirstOrDefault(u => u.UserName == result.UserName);
                Session["User"] = updatedUser;

                return RedirectToAction("Index", "AdminPanel");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }



        //private IAuthenticationManager AuthenticationManager => HttpContext.GetOwin().Authentication;
        //public async Task SignInAsync(User user, bool isPersistent)
        //{
        //    AuthenticationManager.SignOut();
        //}

        [HttpGet]
        public ActionResult Register()
        {

            return View();
        }


        [HttpPost]
        public ActionResult Register(User user)
        {
            string name = user.Name?.Trim().ToLower();
            string surname = user.Surname?.Trim().ToLower();

            var isAuthorized = _context.AuthorizedUsers.Any(x =>
                x.Name.Trim().ToLower() == name &&
                x.Surname.Trim().ToLower() == surname);

            if (!isAuthorized)
            {
                ModelState.AddModelError("", "Only authorized persons can sign up.");
                return View(user); // ❗ Yönlendirme yok → aynı View tekrar yükleniyor
            }

            user.isAdmin = false;
            user.Email = user.UserName;

            var result = UserManager.Create(user, user.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(user);
        }


        [Authorize]
        public ActionResult Faqs(string Search, int? page)
        {
            var faqs = _context.Faqs
                .Where(x => !x.isDeleted &&
                            (string.IsNullOrEmpty(Search) ||
                             x.Question.Contains(Search) ||
                             x.Answer.Contains(Search)))
                .OrderByDescending(x => x.Id); // İsteğe göre sıralama

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            var user = Session["User"] as User;
            if (user != null)
            {
                ViewBag.UserId = user.Id;
            }

            ViewBag.Search = Search;
            return View(faqs.ToPagedList(pageNumber, pageSize));
        }


        [Authorize]
        public ActionResult AddNewFaq()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddNewFaq(Faq faq)
        {
            if (ModelState.IsValid)
            {
                var user = Session["User"] as User;
                faq.CreateDate = DateTime.Now;
                faq.isDeleted = false;
                faq.UserId = user.Id;

                _context.Faqs.Add(faq);
                _context.SaveChanges();
            }

            return RedirectToAction("Faqs", "AdminPanel");
        }

        [Authorize]
        public ActionResult EditFaq(int id)
        {
            if (id <= 0) // id geçerli bir değer değilse, yönlendirme yap.
            {
                return RedirectToAction("Faqs", "AdminPanel");
            }

            var faq = _context.Faqs.FirstOrDefault(x => x.Id == id);

            if (faq == null) // FAQ bulunamazsa yönlendir.
            {
                return RedirectToAction("Faqs", "AdminPanel");
            }

            return View(faq);
        }

        [HttpPost]
        public ActionResult EditFaq(Faq faq)
        {
            if (ModelState.IsValid)
            {
                var faqDb = _context.Faqs.FirstOrDefault(x => x.Id == faq.Id);

                if (faqDb != null)
                {
                    // Değişiklikleri uygulayın
                    faqDb.Answer = faq.Answer;
                    faqDb.Question = faq.Question;

                    // Değişiklikleri veritabanına kaydedin
                    _context.SaveChanges();
                }
            }

            // Başarılı işlem sonrası kullanıcıyı 'Faqs' sayfasına yönlendirin
            return RedirectToAction("Faqs", "AdminPanel");
        }

        [Authorize]
        public ActionResult RemoveFaq(int id)
        {
            if (id <= 0) // id geçerli bir değer değilse, yönlendirme yap.
            {
                return RedirectToAction("Faqs", "AdminPanel");
            }

            var faq = _context.Faqs.FirstOrDefault(x => x.Id == id);

            if (faq == null) // FAQ bulunamazsa yönlendir.
            {
                return RedirectToAction("Faqs", "AdminPanel");
            }
            else
            {
                faq.isDeleted = true;

                _context.SaveChanges();
            }

            return RedirectToAction("Faqs", "AdminPanel");
        }

        public ActionResult SignOut()
        {
            // Kullanıcıyı oturumdan çıkar
            FormsAuthentication.SignOut();

            // Session'ı temizle
            Session["User"] = null;

            // Kullanıcıyı giriş sayfasına veya anasayfaya yönlendir
            return RedirectToAction("Index", "Home");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using CNW_N8_MVC.Models;
using System.Dynamic;
using Newtonsoft.Json;


namespace CNW_N8_MVC.Controllers
{
    public class HomestayController : Controller
    {
        Model1 context = new Model1();
        static ServiceReference1.WebService1SoapClient client = new ServiceReference1.WebService1SoapClient();
        // GET: Homestay
        IPagedList<homestay> model;
        int quantity;
        private void setUsername()
        {
            if (Session["Login"] == null)
            {
                ViewData["username"] = null;
            }
            else
            {
                ViewData["username"] = UserController.userName;
            }
        }
        public ActionResult Index()
        {
            setUsername();
            return View();
        }

        public ActionResult List(int page)
        {
            dynamic model = new ExpandoObject();
            setUsername();
            var optionList = context.locations.Where(o => o.id != null).ToList();
            ViewData["optionList"] = optionList;

            model.homestays = JsonConvert.DeserializeObject<List<homestay>>(client.FEGetListHomestays(page));
            //var model = context.homestays.OrderByDescending(h => h.id).ToPagedList(page ?? 1, 9);
            ViewData["count"] = model.Count.ToString();

            var minPrice = context.homestays.Min(h => h.sell_price);
            var maxPrice = context.homestays.Max(h => h.sell_price);
            ViewData["minPrice"] = minPrice.ToString();
            ViewData["maxPrice"] = maxPrice.ToString();

            return View(model);
        }



        [HttpGet]
        public ActionResult SearchEngine(int page)
        {
            dynamic model1 = new ExpandoObject();
            setUsername();
            var priceRange = Request["priceRange"];
            var locationSelect = Request["locationSelect"];
            var txtSearch = Request["txtSearch"];

            var optionList = context.locations.Where(o => o.id != null).ToList();
            ViewData["optionList"] = optionList;
            if (priceRange == "Dưới 300.000VNĐ")
            {
                //model = context.homestays.OrderByDescending(h => h.id).Where(h => (h.location.location_name == locationSelect && (h.sell_price < 300000) && h.homestay_name.Contains(txtSearch))).ToPagedList(page ?? 1, 9);
                model1 = JsonConvert.DeserializeObject<List<homestay>>(client.FEFindHomestayByPriceLow(page, txtSearch, locationSelect));
            }
            if (priceRange == "300.000- 500.000VNĐ")
            {
                //model = context.homestays.OrderByDescending(h => h.id).Where(h => (h.location.location_name == locationSelect && (h.sell_price >= 300000 && h.sell_price < 500000) && h.homestay_name.Contains(txtSearch))).ToPagedList(page ?? 1, 9);
                model1 = JsonConvert.DeserializeObject<List<homestay>>(client.FEFindHomestayByPriceMedium(page, txtSearch, locationSelect));
            }
            if (priceRange == "500.000- 1.000.000VNĐ")
            {
                //model = context.homestays.OrderByDescending(h => h.id).Where(h => (h.location.location_name == locationSelect && (h.sell_price >= 500000 && h.sell_price < 1000000) && h.homestay_name.Contains(txtSearch))).ToPagedList(page ?? 1, 9);
                model1 = JsonConvert.DeserializeObject<List<homestay>>(client.FEFindHomestayByPriceHigh(page, txtSearch, locationSelect));
            }

            var minPrice = model.Min(h => h.sell_price);
            var maxPrice = model.Max(h => h.sell_price);
            ViewData["minPrice"] = minPrice.ToString();
            ViewData["maxPrice"] = maxPrice.ToString();
            ViewData["count"] = model.Count.ToString();
            ViewData["priceRange"] = priceRange.ToString();
            ViewData["locationSelect"] = locationSelect.ToString();
            ViewData["txtSearch"] = txtSearch.ToString();


            return View("List", model);
        }
        public ActionResult Detail(int? id)
        {
            if (id.HasValue)
            {
                setUsername();
                dynamic model = new ExpandoObject();
                //model.homestays = context.homestays.Where(x => x.id != 0).ToList();
                model.homestays = JsonConvert.DeserializeObject<List<homestay>>(client.FEGetDetailHomestay(id.Value));
                //model.homestay = context.homestays.Where(x => x.id == id).FirstOrDefault();
                model.homestays = JsonConvert.DeserializeObject<List<homestay>>(client.FEGetDetailHomestay0());

                ViewData["id"] = id.ToString();
                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        [HttpGet]
        public ActionResult AddItemHomestay(int id)
        {
            //  int quantity;
            DateTime checkIn = Convert.ToDateTime(Request["check_in"]);
            DateTime checkOut = Convert.ToDateTime(Request["check_out"]);
            if (checkOut <= checkIn)
            {
                //Ngày đi lớn hơn ngày về//
                return RedirectToAction("Detail", "Homestay", new { id = id });
            }
            else
            {
                TimeSpan t = checkOut - checkIn;
                quantity = (int)t.TotalDays;
            }

            var homestay = context.homestays.Find(id);
            var cart = (Cart)Session["CartSession"];
            if (cart != null)
            {
                cart.AddItemHomestay(homestay, checkIn.ToShortDateString(), checkOut.ToShortDateString(), quantity);
            }
            else
            {
                cart = new Cart();
                cart.AddItemHomestay(homestay, checkIn.ToShortDateString(), checkOut.ToShortDateString(), quantity);
                Session["CartSession"] = cart;
            }

            return RedirectToAction("Booking", "User");

        }

        public int checkDate(string checkin, string checkout)
        {
            if (checkin == "" || checkout == "")
            {
                return -1;
            }
            else
            {
                DateTime checkIn = Convert.ToDateTime(checkin);
                DateTime checkOut = Convert.ToDateTime(checkout);

                if (checkOut <= checkIn)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }

            }

        }
    }
}
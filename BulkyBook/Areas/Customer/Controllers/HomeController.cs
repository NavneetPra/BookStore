﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BulkyBook.Models.ViewModels;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http; 

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == claim.Value)
                    .ToList()
                    .Count();

                HttpContext.Session.SetInt32(SD.ssShopingCart, count);
            }

            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
            ShoppingCart cartObj = new ShoppingCart()
            {
                Product = productFromDb,
                ProductId = productFromDb.Id
            };
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;
                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId,
                    includeProperties: "Product"
                    );
                if(cartFromDb == null)
                {
                    _unitOfWork.ShoppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfWork.Save();

                var count = _unitOfWork.ShoppingCart
                    .GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                    .ToList()
                    .Count();

                HttpContext.Session.SetInt32(SD.ssShopingCart, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productFromDb = _unitOfWork.Product.
                        GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,CoverType");
                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }

        public IActionResult Help()
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

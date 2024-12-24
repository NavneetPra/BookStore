using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class BookSearchController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public SearchVM SearchVM { get; set; }

        public BookSearchController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            SearchVM = new SearchVM()
            {
                SearchString = "",
                SearchResults = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType")
            };
            return View(SearchVM);
        }

        [HttpPost]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            if (String.IsNullOrEmpty(SearchVM.SearchString))
            {
                SearchVM = new SearchVM()
                {
                    SearchString = "",
                    SearchResults = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType")
                };
                return RedirectToAction(nameof(Index), SearchVM);
            }
            SearchVM.SearchString = SearchVM.SearchString.ToLower();
            var searchVM = new SearchVM()
            {
                SearchString = SearchVM.SearchString
            };
            return RedirectToAction(nameof(Search), searchVM);
        }

        public IActionResult Search(SearchVM searchVM)
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            searchVM.SearchResults = productList.Where(p => p.Title.ToLower().Contains(searchVM.SearchString)).ToList();
            SearchVM = new SearchVM()
            {
                SearchResults = searchVM.SearchResults,
                SearchString = searchVM.SearchString
            };
            return View(SearchVM);
        }
    }
}

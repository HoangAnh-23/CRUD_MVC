using CRUD_MVC.Models;
using CRUD_MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace CRUD_MVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;

        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();
            return View("Index", products);
        }
        public IActionResult Create()
        {
            return PartialView("_CreateProductForm", new ProductDto());
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            // Kiểm tra lỗi khi không chọn ảnh
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            // Kiểm tra nếu có lỗi validation
            if (!ModelState.IsValid)
            {
                // Trả lại partial view với model và lỗi
                return PartialView("_CreateProductForm", productDto);
            }

            // Tạo tên file mới
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            // Đường dẫn đầy đủ của tệp ảnh
            string imageFullPath = Path.Combine(environment.WebRootPath, "Product", newFileName);

            // Lưu file vào đường dẫn đã chỉ định
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // Lưu thông tin sản phẩm vào cơ sở dữ liệu
            var product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now,
            };

            context.Products.Add(product);
            context.SaveChanges();

            // Sau khi lưu thành công, chuyển hướng về trang Index
            return RedirectToAction("Index", "Products");
        }


        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // Tạo productDto từ product
            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

            return View(productDto);
        }
        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("MM/dd/yyyy");

                return View(productDto);
            }

            // update the image file if we have a new image file

            string newFileName = product.ImageFileName;

            if (productDto.ImageFile != null)

            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmss fF");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);
                string imageFullPath = environment.WebRootPath + "/Product/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
                // delete the old image
                string oldImageFullPath = environment.WebRootPath + "/Product/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            // update product in the database 

            {
                product.Name = productDto.Name;
                product.Brand = productDto.Brand;
                product.Category = productDto.Category;
                product.Price = productDto.Price;
                product.Description = productDto.Description;
                product.ImageFileName = newFileName;
            }


            context.SaveChanges();
            return RedirectToAction("Index", "Products");

        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            // Xóa hình ảnh nếu có
            string imageFullPath = environment.WebRootPath + "/Product/" + product.ImageFileName;
            if (System.IO.File.Exists(imageFullPath))
            {
                System.IO.File.Delete(imageFullPath);
            }

            // Xóa sản phẩm khỏi cơ sở dữ liệu
            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class AdminTests
    {
        [TestMethod]
        public void Index_Contains_All_Products() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1" },
                new Product {ProductID=2, Name="P2" },
                new Product {ProductID=3, Name="P3" },
            });

            //Организация - создание контроллера
            AdminController target = new AdminController(mock.Object);

            //Действие
            Product[] result = ((IEnumerable<Product>) target.Index().ViewData.Model).ToArray();

            //Утверждение
            Assert.AreEqual(result.Length, 3);
            Assert.AreEqual("P1", result[0].Name);
            Assert.AreEqual("P2", result[1].Name);
            Assert.AreEqual("P3", result[2].Name);
        }

        [TestMethod]
        public void Can_Edit_Product() {

            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product { ProductID=1, Name="P1" },
                new Product { ProductID=2, Name="P2" },
                new Product { ProductID=3, Name="P3" }
            });

            //Организация - создание контроллера
            AdminController target = new AdminController(mock.Object);

            //Действие
            Product p1 = target.Edit(1).ViewData.Model as Product;
            Product p2 = target.Edit(2).ViewData.Model as Product;
            Product p3 = target.Edit(3).ViewData.Model as Product;

            Assert.AreEqual(1, p1.ProductID);
            Assert.AreEqual(2, p2.ProductID);
            Assert.AreEqual(3, p3.ProductID);
        }

        [TestMethod]
        public void Cannot_Nonexistent_Product() {

            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();

            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1" },
                new Product {ProductID=2, Name="P2" },
                new Product {ProductID=3, Name="P3" }
            });

            //Организация - создание контроллера 
            AdminController target = new AdminController(mock.Object);

            //Действие
            Product result = (Product)target.Edit(4).ViewData.Model as Product;

            //Утверждение
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Can_Save_Valid_Changes() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            //Организация - создание контроллера
            AdminController target = new AdminController(mock.Object);
            //Организация - создание товара
            Product product = new Product { Name = "Test" };

            //Действие - попытка сохранения товара
            ActionResult result = target.Edit(product);

            //Утверждение - проверка того, что к хранилищу проводится обращение
            mock.Verify(m => m.SaveProduct(product));
            //Утверждение - проверка типа результата метода
            Assert.IsNotInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Cannot_Save_Invalid_Changes() {
            //Организация - создание имитрированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            //Организация - создание контроллера
            AdminController target = new AdminController(mock.Object);
            //Организация - создание товара
            Product product = new Product { Name = "Test" };
            //Организация - добавление ошибки в состояние модели
            target.ModelState.AddModelError("error", "error");
            
            //Действие - попытка сохранения товара
            ActionResult result = target.Edit(product);
            
            //Утвержденеи - проверка того, что обращение к хранилищу не производится
            mock.Verify(m => m.SaveProduct(It.IsAny<Product>()), Times.Never());
            //Утверждение - проверка типа результата метода
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void Can_Delete_Valid_Products() {
            //Организация - создание объекта Product
            Product prod = new Product { ProductID = 2, Name = "Test" };

            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1" },
                prod,
                new Product {ProductID=3, Name="P3" },
            });

            //Организация - создание контроллера
            AdminController target = new AdminController(mock.Object);

            //действие - удаление товара
            target.Delete(prod.ProductID);

            //Утверждение - проверка того, что метод удаления в хранилище вызывается 
            //для корректного объекта Product
            mock.Verify(m => m.DeleteProduct(prod.ProductID));
        }
    }
}

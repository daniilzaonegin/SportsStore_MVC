using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.HtmlHelpers;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate() {
            Mock<IProductRepository> mock = GetMockRepository();

            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Действие
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            //Утверждение
            Product[] prodArray = result.Products.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual(prodArray[0].Name, "P4");
            Assert.AreEqual(prodArray[1].Name, "P5");
        }

        private static Mock<IProductRepository> GetMockRepository() {
            //Организация
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
            new Product {ProductID=1, Name="P1"},
            new Product {ProductID=2, Name="P2"},
            new Product {ProductID=3, Name="P3"},
            new Product {ProductID=4, Name="P4"},
            new Product {ProductID=5, Name="P5"}
            });
            return mock;
        }

        [TestMethod]
        public void Can_Generate_Page_Links() {
            //Организация - определение вспомогательного метода HTML;
            //это необходимо для применения расширяющего метода
            HtmlHelper myHelper = null;
            //Организация - создание данных PagingInfo
            PagingInfo pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };
            //Организация - настройка делегата с помощью лямбда-выражения
            Func<int, string> pageUrlDelegate = i => "Page" + i;
            //Действие
            MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);
            //Утверждение
            Assert.AreEqual(@"<a class=""btn btn-default"" href=""Page1"">1</a>"
                + @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>"
                + @"<a class=""btn btn-default"" href=""Page3"">3</a>", result.ToString());
        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model() {
            //Организация
            Mock<IProductRepository> mock = GetMockRepository();

            //Организация
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Действие
            ProductsListViewModel result = (ProductsListViewModel)controller.List(null, 2).Model;

            //Утверждение
            PagingInfo pageInfo = result.PagingInfo;
            Assert.AreEqual(pageInfo.CurrentPage, 2);
            Assert.AreEqual(pageInfo.ItemsPerPage, 3);
            Assert.AreEqual(pageInfo.TotalItems, 5);
            Assert.AreEqual(pageInfo.TotalPages, 2);
        }

        [TestMethod]
        public void Can_Filter_Products() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1", Category="Cat1" },
                new Product {ProductID=2, Name="P2", Category="Cat2" },
                new Product {ProductID=3, Name="P3", Category="Cat1" },
                new Product {ProductID=4, Name="P4", Category="Cat2" },
                new Product {ProductID=5, Name="P5", Category="Cat3" }
            });

            //Организация - создание контроллера и установка размера страницы
            //равным трем элементам
            ProductController controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            //Действие
            Product[] result = ((ProductsListViewModel)controller.List("Cat2", 1).Model).Products.ToArray();

            //Утверждение
            Assert.AreEqual(result.Length, 2);
            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "Cat2");
            Assert.IsTrue(result[1].Name == "P4" && result[1].Category == "Cat2");
        }

        [TestMethod]
        public void Can_Create_Categories() {
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1", Category="Apples" },
                new Product {ProductID=2, Name="P2", Category="Apples" },
                new Product {ProductID=3, Name="P3", Category="Plumps" },
                new Product {ProductID=4, Name="P4", Category="Oranges"}
            });

            //Организация - создание контроллера
            NavController target = new NavController(mock.Object);

            //Действие - получение набора категорий
            string[] results = ((IEnumerable<string>)target.Menu().Model).ToArray();

            //Утверждение
            Assert.AreEqual(results.Length, 3);
            Assert.AreEqual(results[0], "Apples");
            Assert.AreEqual(results[1], "Oranges");
            Assert.AreEqual(results[2], "Plumps");
        }

        [TestMethod]
        public void Indicates_Selected_Category() {

            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1", Category="Apples" },
                new Product {ProductID=4, Name="P2", Category="Oranges" }
            });

            //Организация - создание контроллера
            NavController target = new NavController(mock.Object);

            //Организация = определение выбранной категории
            string categoryToSelect = "Apples";

            //Действие
            string result = target.Menu(categoryToSelect).ViewBag.SelectedCategory;

            //Утверждение
            Assert.AreEqual(categoryToSelect, result);
        }

        [TestMethod]
        public void Generate_Category_Specific_Product_Count() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name= "P1", Category="Cat1" },
                new Product {ProductID = 2, Name= "P2", Category="Cat2" },
                new Product {ProductID = 3, Name= "P3", Category="Cat1" },
                new Product {ProductID = 4, Name= "P4", Category="Cat2" },
                new Product {ProductID = 5, Name= "P5", Category="Cat3" }
                });

            //Организация - создание котроллера и установка размера страницы
            //равным трем элементам
            ProductController target = new ProductController(mock.Object);
            target.PageSize = 3;

            //Действие - тестирование счетчиков товаров для различных категорий
            int res1 = ((ProductsListViewModel)target.List("Cat1").Model).PagingInfo.TotalItems;
            int res2 = ((ProductsListViewModel)target.List("Cat2").Model).PagingInfo.TotalItems;
            int res3 = ((ProductsListViewModel)target.List("Cat3").Model).PagingInfo.TotalItems;
            int resAll = ((ProductsListViewModel)target.List(null).Model).PagingInfo.TotalItems;

            //Утверждение
            Assert.AreEqual(res1, 2);
            Assert.AreEqual(res2, 2);
            Assert.AreEqual(res3, 1);
            Assert.AreEqual(resAll, 5);
        }

    }
}

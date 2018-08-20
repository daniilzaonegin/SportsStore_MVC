using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportsStore.Domain.Entities;
using System.Linq;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.WebUI.Controllers;
using System.Web.Mvc;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests
{

    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void Can_Add_To_Cart() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1", Category="Apples" }
            }.AsQueryable());
            
            //Организация - создание эксемпляра Cart
            Cart cart = new Cart();

            //Организация - создание контроллера
            CartController target = new CartController(mock.Object, null);

            //Действие - добавление товара в корзину
            target.AddToCart(cart, 1, null);

            //Утверждение
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1); 
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen() {
            //Организация - создание имитированного хранилища
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
            {
                new Product {ProductID=1, Name="P1", Category="Apples" },
            }.AsQueryable());

            //Организация - создание экземпляра Cart
            Cart cart = new Cart();

            //Организация - создание контроллера
            CartController target = new CartController(mock.Object,null);

            //Действие - добавление товара в корзину
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

            //Утверждение
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents() {
            //Организация - создание экземпляра Cart
            Cart cart = new Cart();

            //Организация - создание контроллера
            CartController target = new CartController(null,null);

            //Действие - вызов метода действия Index()
            CardIndexViewModel result = (CardIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            //Утверждение
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart() {
            //Организация - создание имитированного обработчика заказов
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Организация - создание пустой корзины
            Cart cart = new Cart();

            //Организация - создание деталей о доставке
            ShippingDetails shippingDetails = new ShippingDetails();

            //Организация - создание экземпляра контроллера
            CartController target = new CartController(null, mock.Object);

            //Действие
            ViewResult result = target.Checkout(cart, shippingDetails);

            //Утверждение - проверка, что метод вернул стандартное представление
            Assert.AreEqual("", result.ViewName);

            //Утверждение - проверка, что представлению передана неверная модель 
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails() {
            //Организация - создание имитированного обработчика заказов
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Организация - создание корзины с элементом
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            //Организация - создание экземпляра контроллера
            CartController target = new CartController(null, mock.Object);

            //Организация - добавление ошибки в модель
            target.ModelState.AddModelError("error", "error");

            //Действие - попытка перехода к оплате
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Утверждение - проверка, что заказ не передается обработчику
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never);

            //Утверждение - провкрка, что метод возвращает стандартное представление
            Assert.AreEqual("", result.ViewName);

            //Утверждение - проверка, что представлению передается недопустимая модель
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);             
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order() {
            //Организация - создание имитированного обработчика заказов
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            //Организация - создание корзины с элементом
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            //Организация - создание экземпляра контроллера
            CartController target = new CartController(null, mock.Object);

            //Действие - попытка перехода к оплате
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            //Утверждение - проверка, что заказ передан обработчику
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once);

            //Утверждение - проверка, что метод возвращает представление Completed
            Assert.AreEqual("Completed", result.ViewName);

            //Утверждение - проверка, что представлению передается допустимая модель
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Add_New_Lines() {
            //Организация - создание нескольких тестовых товаров
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            //Организация - создание новой корзины
            Cart target = new Cart();

            //Действие
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            CartLine[] results = target.Lines.ToArray();

            //Утверждение
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Product, p1);
            Assert.AreEqual(results[1].Product, p2);
        }
        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines() {
            //Организация - создание нескольких тестовых товаров
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };

            //Организация - создание новой корзины
            Cart target = new Cart();

            //Действие
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);
            CartLine[] results = target.Lines.OrderBy(c => c.Product.ProductID).ToArray();

            //Утверждение
            Assert.AreEqual(results.Length, 2);
            Assert.AreEqual(results[0].Quantity, 11);
            Assert.AreEqual(results[1].Quantity, 1);
        }
        [TestMethod]
        public void Can_Remove_Line() {
            //Организация - создание нескольких тестовых товаров
            Product p1 = new Product { ProductID = 1, Name = "P1" };
            Product p2 = new Product { ProductID = 2, Name = "P2" };
            Product p3 = new Product { ProductID = 3, Name = "P3" };
            //Организация - создание новой корзины
            Cart target = new Cart();
            //Организация - добавление некоторых товаров в корзину
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);
            //Действие
            target.RemoveLine(p2);
            //Утверждение
            Assert.AreEqual(target.Lines.Where(c => c.Product == p2).Count(), 0);
            Assert.AreEqual(target.Lines.Count(), 2);
        }
        [TestMethod]
        public void Calculate_Cart_Total() {
            //Организация -создание нескольких тестовых товаров
            Product p1 = new Product { ProductID = 1, Name = "P1", Price=100M };
            Product p2 = new Product { ProductID = 2, Name = "P2" , Price=50M };
            //Организация - создание тестовой корзины
            Cart target = new Cart();
            //Действие
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            decimal result = target.ComputeTotalValue();
            //Утверждение
            Assert.AreEqual(result, 450M);
        }
        [TestMethod]
        public void Can_Clear_Contents() {
            //Организация - создание нескольких тестовых товаров
            Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };
            //Организация - создание новой корзины
            Cart target = new Cart();
            //Организация - добавление нескольких элементов в корзину
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            //Действие - сброс корзины
            target.Clear();
            //Утверждение
            Assert.AreEqual(target.Lines.Count(), 0);
        }
    }
}

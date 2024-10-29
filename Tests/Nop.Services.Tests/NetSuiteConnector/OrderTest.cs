using Moq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using Nop.Services.Configuration;
using NUnit.Framework;
using System.Collections.Generic;

namespace Nop.Services.Tests.NetSuiteConnector
{
    public class OrderTest
    {
        //private OrderService _orderService;

        [SetUp]
        public new void SetUp()
        {
           // _orderService = new OrderService();
        }
        [Test]
        public void SendUpdateOrderNetsuite_WithValidData_ReturnsTrue()
        {
            //// Arrange
            //var order = new Order { Customer = new Customer { Parent = 123 }, CompanyId = 456 };
            //var orderNetsuiteUpdate = new TransactionDto { Orderid = "789" };
            //var settingServiceMock = new Mock<ISettingService>();
            //settingServiceMock.Setup(s => s.GetSetting("WebStoreCustomer.id")).Returns(new Setting { Value = "12345" });
            //settingServiceMock.Setup(s => s.GetSetting("WebStoreCustomer.PrimaryLocation")).Returns(new Setting { Value = "Location" });
            //settingServiceMock.Setup(s => s.GetSetting("WebStoreCustomer.customForm")).Returns(new Setting { Value = "678" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingPickupInStore.Nashville")).Returns(new Setting { Value = "Nashville" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingPickupInStore.Atlanta")).Returns(new Setting { Value = "Atlanta" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingPickupInStore.id")).Returns(new Setting { Value = "101112" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingUpsGroup.Name")).Returns(new Setting { Value = "UPS Ground" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingUpsGroup.id")).Returns(new Setting { Value = "131415" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAir.Name")).Returns(new Setting { Value = "UPS Next Day Air" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAir.id")).Returns(new Setting { Value = "161718" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAirEarlyAm.Name")).Returns(new Setting { Value = "UPS Next Day Air Early AM" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAirEarlyAm.id")).Returns(new Setting { Value = "192021" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAirSaver.Name")).Returns(new Setting { Value = "UPS Next Day Air Saver" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingNextDayAirSaver.id")).Returns(new Setting { Value = "222324" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingSecondDay.Name")).Returns(new Setting { Value = "UPS Second Day Air" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingSecondDay.id")).Returns(new Setting { Value = "252627" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingSecondDayAirAm.Name")).Returns(new Setting { Value = "UPS Second Day Air AM" });
            //settingServiceMock.Setup(s => s.GetSetting("ShippingSecondDayAirAm.id")).Returns(new Setting { Value = "282930" });
            //settingServiceMock.Setup(s => s.GetSetting("Shipping3DaySelect.Name")).Returns(new Setting { Value = "UPS 3 Day Select" });
            //settingServiceMock.Setup(s => s.GetSetting("Shipping3DaySelect.id")).Returns(new Setting { Value = "313233" });
            
            //var target = new MyClass(settingServiceMock.Object);

            //// Act
            //var result = target.SendUpdateOrderNetsuite(order, orderNetsuiteUpdate);

            //// Assert
            //Assert.IsTrue(result);
        }
        //[Test]
        //public void SendUpdateOrderNetsuite_Should_Return_True()
        //{
        //    // Arrange
        //    Order order = new Order()
        //    {
        //        Id = 1,
        //        CompanyId = 1,
        //        Customer = new Customer()
        //        {
        //            Parent = 0,
        //            Companies = new List<Company>()
        //    {
        //        new Company()
        //        {
        //            Id = 1,
        //            NetsuiteId = "CompanyNetsuiteId",
        //            PrimaryLocation = "CompanyPrimaryLocation"
        //        }
        //    }
        //        },
        //        ShippingAddress = new Address()
        //        {
        //            City = "Nashville"
        //        },
        //        BillingAddress = new Address()
        //        {
        //            City = "Nashville"
        //        },
        //        ShippingRateComputationMethodSystemName = "Shipping.UPS",
        //        ShippingMethod = "UPS Ground",
        //        OrderShippingExclTax = 50
        //    };

        //    TransactionDto orderNetsuiteUpdate = new TransactionDto()
        //    {
        //        Orderid = "1"
        //    };

        //    object[] parameters = new object[] { order, orderNetsuiteUpdate };
        //    bool expected = true;

        //    MyClass myClass = new MyClass();
        //    MethodInfo methodInfo = typeof(MyClass).GetMethod("SendUpdateOrderNetsuite", BindingFlags.NonPublic | BindingFlags.Instance);

        //    // Act
        //    bool result = (bool)methodInfo.Invoke(myClass, parameters);

        //    // Assert
        //    Assert.AreEqual(expected, result);
        //}

    }
}

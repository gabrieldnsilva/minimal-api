using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using minimal_api.Dominio.Entities;


namespace Tests.Domains.Entities
{
    [TestClass]
    public class AdministratorsTests
    {
        [TestMethod]
        public void TestGetSetProperties()
        {
        //Arrange
        var admin = new Administrators
        {
            //Act
            Id = 1,
            Email = "teste@teste",
            Senha = "teste",
            Perfil = "Administrator"
        };

        //Assert
        Assert.AreEqual(1, admin.Id);
            Assert.AreEqual("teste@teste", admin.Email);
            Assert.AreEqual("teste", admin.Senha);
            Assert.AreEqual("Administrator", admin.Perfil);

        }
    }
}
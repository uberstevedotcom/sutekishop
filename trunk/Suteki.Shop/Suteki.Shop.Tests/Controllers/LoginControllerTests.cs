﻿using System;
using NUnit.Framework;
using Suteki.Common.Repositories;
using Suteki.Common.TestHelpers;
using Suteki.Common.ViewData;
using Suteki.Shop.Controllers;
using Suteki.Shop.Services;
using Suteki.Shop.Tests.Repositories;
using Rhino.Mocks;

namespace Suteki.Shop.Tests.Controllers
{
    [TestFixture]
    public class LoginControllerTests
    {
        private IRepository<User> userRepository;
        private IUserService userService;

        private LoginController loginController;

    	private const string henry1password = "6C80B78681161C8349552872CFA0739CF823E87B";
    	private const string george1password = "DC25F9DC0DF2BE9E6A83E6F0B26F4B41F57ADF6D";
    	private const string sky1pasword = "980BC222DA7FDD0D37BE816D60084894124509A1";
    	private const string email = "Henry@suteki.co.uk";
    	private const string password = "henry1";

    	[SetUp]
        public void SetUp()
        {
            userRepository = MockRepositoryBuilder.CreateUserRepository();
            userService = MockRepository.GenerateStub<IUserService>();
            loginController = new LoginController(userRepository, userService);
        }

        [Test]
        public void Index_ShouldDisplayIndexView()
        {
            const string view = "Index";

            loginController.Index()
                .ReturnsViewResult()
                .ForView(view);
        }

        [Test]
        public void Authenticate_ShouldAuthenticateValidUser()
        {
        	userService.Expect(x => x.HashPassword(password)).Return(henry1password);
            
            loginController.Index(email, password, null)
                .ReturnsRedirectToRouteResult()
                .ToAction("Index")
                .ToController("Home");

            userService.AssertWasCalled(c => c.SetAuthenticationCookie(email));
        }
			
        [Test]
        public void Authenticate_ShouldNotAuthenticateInvalidUser()
        {
			const string password = "henry3";
        	// throw if SetAuthenticationToken is called
            userService.Expect(c => c.SetAuthenticationCookie(password))
                .Throw(new Exception("SetAuthenticationToken shouldn't be called"));

            loginController.Index(email, password, null)
                .ReturnsViewResult()
                .ForView("") //view should match action
                .WithModel<IErrorViewData>()
                .AssertAreEqual("Unknown email or password", vd => vd.ErrorMessage);
        }

        [Test]
        public void Logout_ShouldLogUserOut()
        {
            loginController.Logout()
                .ReturnsRedirectToRouteResult()
                .ToAction("Index")
                .ToController("Home");

            userService.AssertWasCalled(c => c.RemoveAuthenticationCookie());
        }

    	[Test]
    	public void Should_redirect_to_returnurl()
    	{
			userService.Expect(x => x.HashPassword(password)).Return(henry1password);

			loginController.Index(email, password, "/foo/bar")
				.ReturnsRedirect().Url.ShouldEqual("/foo/bar");

    	}
    }
}

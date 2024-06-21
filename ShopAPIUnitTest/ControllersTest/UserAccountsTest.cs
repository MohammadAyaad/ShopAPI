using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using ShopAPI.Controllers;
using ShopAPI.Data;
using ShopAPI.Model.Users;

namespace ShopAPIUnitTest.ControllersTest
{
    public class UserAccountsTest
    {
        private readonly UserAccountsController _controller;
        private readonly ShopDBContext _context;

        public UserAccountsTest()
        {
            _context = A.Fake<ShopDBContext>();
            _controller = new UserAccountsController(_context);
        }
        [Fact]
        public async Task CreateUserAccount_ReturnsCreatedStatus()
        {
            // Arrange
            UserAccountDTO _testUserAccountDTO = A.Fake<UserAccountDTO>();
            string password = GenerateRandomString(RandomNumberGenerator.GetInt32(1, 32));
            // Act
            var result = await _controller.CreateUserAccount(_testUserAccountDTO, password);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.IsType<UserAccountDTO>(createdAtActionResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsSuccessfulStatusWithToken()
        {
            // Arrange
            UserAccountDTO _testUserAccountDTO = A.Fake<UserAccountDTO>();
            string password = GenerateRandomString(RandomNumberGenerator.GetInt32(1, 32));
            await _controller.CreateUserAccount(_testUserAccountDTO, password);
            // Act
            var result = await _controller.Login($"Basic {_testUserAccountDTO.Email}:{password}");
            var actionResult = Assert.IsType<OkObjectResult>(result);

            // Assert
            Assert.True(result.Value.ContainsKey("token"));
            string token = result.Value["token"].ToString();
            Assert.IsType<OkResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_Returns_Correct_User_Data()
        {
            // Arrange
            UserAccountDTO _testUserAccountDTO = A.Fake<UserAccountDTO>();
            string password = GenerateRandomString(RandomNumberGenerator.GetInt32(1, 32));
            await _controller.CreateUserAccount(_testUserAccountDTO, password);
            var _result = await _controller.Login($"Basic {_testUserAccountDTO.Email}:{password}");
            var actionResult = Assert.IsType<OkObjectResult>(_result);
            Assert.True(_result.Value.ContainsKey("token"));
            string token = _result.Value["token"].ToString();
            // Act

            var result = await _controller.GetUserAccount(token);
            // Assert
            Assert.Equal(_testUserAccountDTO, result.Value);
        }

        [Fact]
        public async Task PutUser_Edits_The_User_Correctly()
        {
            UserAccountDTO _testUserAccountDTO = A.Fake<UserAccountDTO>();
            string password = GenerateRandomString(RandomNumberGenerator.GetInt32(1, 32));
            await _controller.CreateUserAccount(_testUserAccountDTO, password);
            var _result = await _controller.Login($"Basic {_testUserAccountDTO.Email}:{password}");
            var actionResult = Assert.IsType<OkObjectResult>(_result);
            Assert.True(_result.Value.ContainsKey("token"));
            string token = _result.Value["token"].ToString();
            UserAccountDTO newTestUserAccountDTO = A.Fake<UserAccountDTO>();
            var result0 = await _controller.PutUserAccount(token, newTestUserAccountDTO);

            Assert.IsType<NoContentResult>(result0.GetType());

            var result1 = await _controller.GetUserAccount(token);
            Assert.IsType<OkObjectResult>(result1.Result.GetType());
            Assert.Equal(newTestUserAccountDTO, result1.Value);
        }
        [Fact]
        public async Task DeleteUser_Clears_The_System()
        {
            UserAccountDTO _testUserAccountDTO = A.Fake<UserAccountDTO>();
            string password = GenerateRandomString(RandomNumberGenerator.GetInt32(1, 32));
            await _controller.CreateUserAccount(_testUserAccountDTO, password);
            var _result = await _controller.Login($"Basic {_testUserAccountDTO.Email}:{password}");
            var actionResult = Assert.IsType<OkObjectResult>(_result);
            Assert.True(_result.Value.ContainsKey("token"));
            string token = _result.Value["token"].ToString();
            Assert.IsType<OkResult>(_result.Result);

            var result = await _controller.DeleteUserAccount(token);
            Assert.IsType<NoContentResult>(result.GetType());

            var login_result = await _controller.Login($"Basic {_testUserAccountDTO.Email}:{password}");

            Assert.IsType<NotFoundResult>(login_result.Result.GetType());

            Assert.IsType<NotFoundResult>((await _controller.GetUserAccount(token)));
        }

        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(0, s.Length)]).ToArray());
        }
    }
}

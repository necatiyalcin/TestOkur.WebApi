﻿namespace TestOkur.WebApi.Integration.Tests.Sms
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.Threading.Tasks;
	using FluentAssertions;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using TestOkur.Common;
	using TestOkur.Contracts.Sms;
	using TestOkur.Data;
	using TestOkur.TestHelper.Extensions;
	using TestOkur.WebApi.Application.Sms;
	using TestOkur.WebApi.Application.Sms.Commands;
	using TestOkur.WebApi.Integration.Tests.Common;
	using TestOkur.WebApi.Integration.Tests.User;
	using Xunit;

	public class SendSmsTests : UserTest
	{
		private new const string ApiPath = "api/v1/sms";

		[Fact]
		public async Task When_UsersLimitNotEnoughToSendSms_InsufficientFundError_Should_BeReturned()
		{
			using (var testServer = await CreateWithUserAsync())
			{
				var client = testServer.CreateClient();
				var messages = new List<SmsMessageModel>();

				for (var i = 0; i < 100; i++)
				{
					messages.Add(new SmsMessageModel("5544567788", "TEST", "TEST"));
				}

				var command = new SendSmsCommand(Guid.NewGuid(), messages);
				var response = await client.PostAsync(ApiPath, command.ToJsonContent());
				await response.Should().BeBadRequestAsync(ErrorCodes.InsufficientFunds);
			}
		}

		[Fact]
		public async Task AdminShouldBeAbleToSendSms()
		{
			using (var testServer = await CreateWithUserAsync())
			{
				var command = new SendSmsAdminCommand(
					Guid.NewGuid(),
					"5544205163",
					"This is a test message");
				var response = await testServer.CreateClient().PostAsync(
					$"{ApiPath}/send-admin", command.ToJsonContent());
				response.EnsureSuccessStatusCode();
				var @events = Consumer.Instance.GetAll<ISendSmsRequestReceived>();
				var @event = @events.FirstOrDefault(e => e.UserId == default);
				@event.Should().NotBeNull();
				@event.SmsMessages.Should().HaveCount(1)
					.And
					.Contain(m => m.Receiver == command.Receiver)
					.And
					.Contain(m => m.Body == command.Body);
			}
		}

		[Fact]
		public async Task When_UserHasEnoughSmsLimit_SmsEventIsPushed()
		{
			using (var testServer = await CreateWithUserAsync())
			{
				//Add Some SMS Credits
				var dbContext = testServer.Host.Services.GetService(typeof(ApplicationDbContext))
					as ApplicationDbContext;
				var subjectId = (testServer.Host.Services
					.GetRequiredService(typeof(Claim)) as Claim).Value;

				var user = await dbContext.Users.FirstAsync(u => u.SubjectId == subjectId);
				user.AddSmsBalance(2);
				await dbContext.SaveChangesAsync();

				var messages = new List<SmsMessageModel>();

				for (var i = 0; i < 2; i++)
				{
					messages.Add(new SmsMessageModel("5544567788", "TEST", "TEST"));
				}

				var command = new SendSmsCommand(Guid.NewGuid(), messages);
				var response = await testServer.CreateClient().PostAsync(ApiPath, command.ToJsonContent());
				response.EnsureSuccessStatusCode();

				var @event = Consumer.Instance.GetFirst<ISendSmsRequestReceived>();
				@event.UserId.Should().NotBe(default);
				@event.SmsMessages.Should().HaveCount(2)
					.And
					.Contain(m => m.Receiver == "5544567788");
			}
		}
	}
}

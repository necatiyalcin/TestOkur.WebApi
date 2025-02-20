﻿namespace TestOkur.Report.Integration.Tests.Consumers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using FluentAssertions;
	using Microsoft.Extensions.Logging;
	using TestOkur.Optic.Form;
	using TestOkur.Report.Consumers;
	using TestOkur.Report.Repositories;
	using TestOkur.TestHelper;
	using Xunit;

	public class EvaluateExamConsumerShould : ConsumerTest
	{
		[Fact]
		public async Task ShouldEvaluateAndSaveResults()
		{
			var userId = RandomGen.Next(10000);
			var answerKeyForms = GenerateAnswerKeyOpticalForms(1).ToList();

			using (var testServer = Create(userId))
			{
				var client = testServer.CreateClient();
				var examId = await ExecuteExamCreatedConsumerAsync(testServer, answerKeyForms);
				var repository = testServer.Host.Services.GetService(typeof(IOpticalFormRepository));
				var logger = testServer.Host.Services.GetService(typeof(ILogger<EvaluateExamConsumer>));
				var forms = new List<StudentOpticalForm>
				{
					GenerateStudentForm(examId, userId),
					GenerateStudentForm(examId, userId),
				};
				var response = await client.PostAsync(ApiPath, forms.ToJsonContent());
				response.EnsureSuccessStatusCode();

				var consumer = new EvaluateExamConsumer(
					repository as IOpticalFormRepository,
					logger as ILogger<EvaluateExamConsumer>);
				await consumer.ConsumeAsync(examId);
				var studentOpticalForms = await GetListAsync<StudentOpticalForm>(client, examId);
				studentOpticalForms.Should().HaveCount(forms.Count);
				studentOpticalForms.Should().NotContain(s => !s.Orders.Any());
				studentOpticalForms.First().GeneralAttendanceCount.Should().Be(forms.Count);
				studentOpticalForms.First().CityAttendanceCount.Should().Be(forms.Count);
				studentOpticalForms.First().ClassroomAttendanceCount.Should().Be(forms.Count);
				studentOpticalForms.First().SchoolAttendanceCount.Should().Be(forms.Count);
			}
		}
	}
}

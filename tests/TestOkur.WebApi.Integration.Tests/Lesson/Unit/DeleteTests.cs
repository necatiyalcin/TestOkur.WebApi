﻿namespace TestOkur.WebApi.Integration.Tests.Lesson.Unit
{
	using System.Linq;
	using System.Threading.Tasks;
	using FluentAssertions;
	using Xunit;

	public class DeleteTests : UnitTest
	{
		[Fact]
		public async Task ShouldDeleteSubject()
		{
			using (var testServer = await CreateWithUserAsync())
			{
				var client = testServer.CreateClient();
				var command = await AddSubjectAsync(client);
				var units = await GetUnitListAsync(client);
				var id = units.First(u => u.Id == command.UnitId)
					.Subjects.First(s => s.Name == command.Name)
					.Id;
				await client.DeleteAsync($"{ApiPath}/{command.UnitId}/subjects/{id}");
				units = await GetUnitListAsync(client);
				units.First(u => u.Id == command.UnitId)
					.Subjects
					.Should()
					.NotContain(s => s.Id == id);
			}
		}

		[Fact]
		public async Task ShouldDelete()
		{
			using (var testServer = await CreateWithUserAsync())
			{
				var client = testServer.CreateClient();
				var command = await CreateUnitAsync(client);
				var id = (await GetUnitListAsync(client))
					.First(l => l.Name == command.Name).Id;
				await client.DeleteAsync($"{ApiPath}/{id}");
				(await GetUnitListAsync(client)).Should()
					.NotContain(l => l.Name == command.Name);
			}
		}
	}
}

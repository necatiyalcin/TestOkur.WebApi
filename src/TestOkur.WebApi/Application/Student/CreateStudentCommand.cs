﻿namespace TestOkur.WebApi.Application.Student
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using TestOkur.Infrastructure.Cqrs;
	using TestOkur.WebApi.Application.Contact;
	using Classroom = TestOkur.Domain.Model.ClassroomModel.Classroom;
	using Student = TestOkur.Domain.Model.StudentModel.Student;

	[DataContract]
	public class CreateStudentCommand : CommandBase, IClearCache
	{
		public CreateStudentCommand(
			Guid id,
			string firstName,
			string lastName,
			int studentNumber,
			int classroomId,
			string notes,
			IEnumerable<CreateContactCommand> contacts)
		 : base(id)
		{
			FirstName = firstName;
			LastName = lastName;
			StudentNumber = studentNumber;
			Notes = notes;
			ClassroomId = classroomId;
			Contacts = contacts;
		}

		public IEnumerable<string> CacheKeys => new[]
		{
			$"Students_{UserId}",
			$"Contacts_{UserId}",
		};

		[DataMember]
		public string FirstName { get; private set; }

		[DataMember]
		public string LastName { get; private set; }

		[DataMember]
		public int StudentNumber { get; private set; }

		[DataMember]
		public int ClassroomId { get; private set; }

		[DataMember]
		public string Notes { get; private set; }

		[DataMember]
		public IEnumerable<CreateContactCommand> Contacts { get; private set; }

		public Student ToDomainModel(Classroom classroom) => ToDomainModel(classroom, UserId);

		public Student ToDomainModel(Classroom classroom, int userId)
		{
			return new Student(
				FirstName,
				LastName,
				StudentNumber,
				classroom,
				Contacts?.Select(c => c.ToDomainModel()) ?? Enumerable.Empty<Domain.Model.StudentModel.Contact>(),
				Notes);
		}
	}
}

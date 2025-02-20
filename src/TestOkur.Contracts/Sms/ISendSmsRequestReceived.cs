﻿namespace TestOkur.Contracts.Sms
{
	using System.Collections.Generic;

	public interface ISendSmsRequestReceived : IIntegrationEvent
    {
        int UserId { get; }

        string UserSubjectId { get; }

        string UserEmail { get; }

        IEnumerable<ISmsMessage> SmsMessages { get; }
    }
}
